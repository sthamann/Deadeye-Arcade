using System.IO;
using System.Text.Json;
using HIDMaestro;
using Reaper.Core;
using Reaper.Windows;

namespace Deadeye.InputLab;

internal static partial class Program
{
    private static bool SupportsDolphinReplay(GameEntry game)
    {
        string id=DolphinSetup.IsDolphin(game)?DolphinSetup.DiscId(game)??"":"";
        return (id.StartsWith("RZJ")||id.StartsWith("RHO")||id.StartsWith("RBU")||id.StartsWith("SBD"))&&(game.Helpers??[]).Length==0
            &&!game.Arguments.Any(a=>a is "-u" or "--user"||a.StartsWith("--user="));
    }

    // Run the actual RawInput -> GameSession -> DSU -> Dolphin path. Keep all
    // emulator writes in a new user directory; never back up or copy the disc.
    private static async Task ReplayDolphin(GameEntry original,string data,List<GunBinding> players,
        HMController[] devices,string libraryFile,string libraryHash)
    {
        if(original.Arguments.Any(a=>a is "-u" or "--user"||a.StartsWith("--user=")))
            throw new NotSupportedException("Review the existing Dolphin user-directory override before replay.");
        string realUser=DolphinSetup.UserDirectory(original),isolated=Path.Combine(output,"dolphin-user");
        if(Path.GetFullPath(isolated).StartsWith(Path.TrimEndingDirectorySeparator(Path.GetFullPath(realUser))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
            throw new IOException("The replay output must be outside the original Dolphin user directory.");
        var originalFiles=new Dictionary<string,string>();
        foreach(string folder in new[]{"Config","GameSettings"})
        {
            string source=Path.Combine(realUser,folder);
            if(!Directory.Exists(source))continue;
            foreach(string path in Directory.EnumerateFiles(source,"*.ini",SearchOption.AllDirectories))
            {
                originalFiles[path]=Hash(path);
                string copy=Path.Combine(isolated,Path.GetRelativePath(realUser,path));
                Directory.CreateDirectory(Path.GetDirectoryName(copy)!);File.Copy(path,copy,false);
            }
        }
        Directory.CreateDirectory(isolated);
        string config=Path.Combine(isolated,"Config","Dolphin.ini");
        DolphinSetup.WriteMerged(config,"General",new Dictionary<string,string>{["NANDRootPath"]=Path.Combine(isolated,"Wii"),["WiiSDCardPath"]=Path.Combine(isolated,"Wii","sd.raw")});
        DolphinSetup.WriteMerged(config,"Core",new Dictionary<string,string>{["WiiSDCard"]="False",["WiiSDCardAllowWrites"]="False",["WiiSDCardEnableFolderSync"]="False",["MemcardAPath"]=Path.Combine(isolated,"GC","MemoryCardA.USA.raw"),["MemcardBPath"]=Path.Combine(isolated,"GC","MemoryCardB.USA.raw")});
        var game=original with {Arguments=original.Arguments.Concat(new[]{"--user",isolated}).ToArray()};
        File.WriteAllText(Path.Combine(output,"dolphin-isolation.json"),JsonSerializer.Serialize(new {
            originalUser=realUser,isolatedUser=isolated,originalFiles,discCopied=false,savesCopied=false
        },json));
        evidence.Add(new {stage="emulator-build",sha256=Hash(original.Executable),discId=DolphinSetup.DiscId(original),isolatedUser=true});
        var session=new GameSession();Task? running=null;
        using var neutralStop=CancellationTokenSource.CreateLinkedTokenSource(cancel.Token);
        var neutral=Task.Run(async()=>
        {
            try {while(!neutralStop.IsCancellationRequested){foreach(var device in devices)device.SubmitMouse(new HMMouseState{X=16384,Y=16384});await Task.Delay(16,neutralStop.Token);}}
            catch(OperationCanceledException) when(neutralStop.IsCancellationRequested) { }
        });
        raw.Packet+=session.Input;
        try
        {
            var ready=new TaskCompletionSource<nint>(TaskCreationOptions.RunContinuationsAsynchronously);
            session.GameWindowReady+=h=>ready.TrySetResult(h);
            running=session.Run(game,data,players,cancel.Token,fixesPrepared:true);
            var winner=await Task.WhenAny(ready.Task,running,Task.Delay(60000,cancel.Token));
            if(winner!=ready.Task){if(running.IsCompleted)await running;throw new IOException("No Dolphin renderer appeared within sixty seconds.");}
            var controls=OverlayControls.Read(game,data,players,raw.Devices);
            File.WriteAllText(Path.Combine(output,"launched-controls.json"),JsonSerializer.Serialize(controls,json));
            var plan=await ControlSimulation.Run(new[]{game},data,players,raw.Devices);
            var actions=plan.Games.Single().Actions;
            evidence.Add(new {stage="dolphin-action-plan",plan,note="Production DSU frame evaluation; live game response is recorded separately."});
            if(plan.Games.Single().Status!="simulation-passed")throw new IOException("Dolphin action plan has unresolved or colliding inputs; live replay refused.");
            await Delay(15000);RequireGame(session);neutralStop.Cancel();await neutral;
            await DolphinTitle(session,devices,players,actions,DolphinSetup.DiscId(original)??"");
            var bridge=JsonSerializer.SerializeToElement(session.InputDiagnostics(),json);
            evidence.Add(new {stage="dolphin-consumer",bridge,note="Dolphin subscriptions and production bridge frames sent to each controller slot."});
            if(bridge.ValueKind!=JsonValueKind.Object||!bridge.TryGetProperty("subscriptions",out var subscriptions)||!bridge.TryGetProperty("frames",out var frames)
                ||players.Any(p=>subscriptions[p.Player-1].GetInt64()==0||frames[p.Player-1].GetInt64()==0))
                throw new IOException("Dolphin did not subscribe to both configured player streams; game-input replay remains unverified.");
            evidence.Add(new {stage="dolphin-initial-scene",frame=Capture(session,"dolphin-initial-scene")});
            // A/B dismisses the known title prompt. No save data is copied into
            // this disposable user directory, so original progress is untouched.
            await DolphinAction(session,devices,players,actions.First(a=>a.Player==1&&a.Function=="Buttons/B"));
            await Delay(5000);
            evidence.Add(new {stage="dolphin-after-title",frame=Capture(session,"dolphin-after-title")});
            foreach(var player in players)
            {
                foreach(var point in new[]{(0,0),(32767,0),(32767,32767),(0,32767),(16384,16384)})
                {
                    RequireGame(session);Drain();devices[player.Player-1].SubmitMouse(new HMMouseState{X=point.Item1,Y=point.Item2});await Delay(300);
                    var got=Drain();bool received=got.Any(p=>p.DeviceId==player.MouseId&&p.Absolute&&Math.Abs(p.X-point.Item1*65535d/32767)<=64&&Math.Abs(p.Y-point.Item2*65535d/32767)<=64);
                    evidence.Add(new {stage="dolphin-aim",player.Player,x=point.Item1,y=point.Item2,received,frame=Capture(session,$"dolphin-p{player.Player}-aim-{point.Item1}-{point.Item2}"),gameResponse="requires-visual-review"});
                    if(!received)throw new IOException("Dolphin virtual aim packet was not received.");
                }
                await Sweep(session,devices[player.Player-1],player);
            }
            foreach(var action in actions)
            {
                await DolphinAction(session,devices,players,action);
                // Exercise Pause as a pair so later actions are not deliberately
                // left in the pause menu. Captures still require visual review.
                if(action.Function=="Buttons/+")await DolphinAction(session,devices,players,action);
            }
        }
        finally
        {
            neutralStop.Cancel();try{await neutral;}catch(OperationCanceledException) { }
            foreach(var device in devices)try{device.SubmitKeyboard(HMKeyModifiers.None,ReadOnlySpan<HMKey>.Empty);device.SubmitMouse(new HMMouseState{X=16384,Y=16384});}catch(Exception e){evidence.Add(new {stage="release-input",error=e.Message});}
            try {await session.End();}
            finally
            {
                if(running is not null)try{await running.WaitAsync(TimeSpan.FromSeconds(15));}catch(Exception e){evidence.Add(new {stage="session-end",error=e.Message});}
                raw.Packet-=session.Input;
                bool untouched=originalFiles.All(p=>File.Exists(p.Key)&&Hash(p.Key)==p.Value)
                    && new[]{"Config","GameSettings"}.Where(f=>Directory.Exists(Path.Combine(realUser,f))).SelectMany(f=>Directory.EnumerateFiles(Path.Combine(realUser,f),"*.ini",SearchOption.AllDirectories)).All(originalFiles.ContainsKey);
                bool libraryUnchanged=Hash(libraryFile)==libraryHash;
                evidence.Add(new {stage="restore",isolatedUser=true,originalConfigUnchanged=untouched,libraryUnchanged,session.CleanupIssues});
                if(!untouched||!libraryUnchanged)throw new IOException("Original Dolphin configuration or library changed during isolated replay; newer contents retained.");
            }
        }
    }

    private static async Task DolphinTitle(GameSession session,HMController[] devices,List<GunBinding> players,SimulatedAction[] actions,string discId)
    {
        var clock=System.Diagnostics.Stopwatch.StartNew();
        var accept=actions.First(a=>a.Player==1&&a.Function=="Buttons/A");
        while(clock.Elapsed<TimeSpan.FromSeconds(100))
        {
            RequireGame(session);string frame=Capture(session,"dolphin-boot");string text=await DolphinSceneText(Path.Combine(output,frame));
            bool title=DolphinReadyText(text);
            evidence.Add(new {stage="dolphin-boot",seconds=clock.Elapsed.TotalSeconds,text,title,frame});
            if(title)return;
            // Observed Wii strap/Zapper screens need A. These are boot prompts,
            // not gameplay actions. Stop rather than certifying an unknown scene.
            bool createSave=DolphinCreateSaveText(text,discId);
            bool newProfile=(discId.StartsWith("RBU")&&System.Text.RegularExpressions.Regex.IsMatch(text,@"\bLoad\b",System.Text.RegularExpressions.RegexOptions.IgnoreCase)&&System.Text.RegularExpressions.Regex.IsMatch(text,@"NEW\s*GA[MN]E",System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                ||(discId.StartsWith("SBD")&&text.Contains("PROFILE SELECTION",StringComparison.OrdinalIgnoreCase)&&text.Contains("NO DATA",StringComparison.OrdinalIgnoreCase));
            bool prompt=System.Text.RegularExpressions.Regex.IsMatch(text,@"ZAPPER|WRIST|STRAP|NUNCHUK|WII\s*REMOTE|HEALTH.*SAFETY|PRESS.*(?:A|BUTTON)",System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if(newProfile)
            {
                bool saveSlot=discId.StartsWith("SBD")&&System.Text.RegularExpressions.Regex.IsMatch(text,@"file\s*to\s*save",System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                int x=(int)(32767*(discId.StartsWith("SBD")?(saveSlot?.31:.21):.50)),y=(int)(32767*(discId.StartsWith("SBD")?.38:.21));
                var tokens=accept.Buttons.Select(id=>StartupControls.Hardware(players[0]).Single(h=>h.Id==id).Token!).Distinct().ToArray();
                devices[0].SubmitMouse(new HMMouseState{X=x,Y=y});await Delay(400);
                try{DolphinChord(devices[0],tokens,x,y);await Delay(150);}finally{DolphinChord(devices[0],[],x,y);}
                evidence.Add(new {stage="dolphin-new-test-profile",x,y,note="Select observed New Game target in isolated NAND only."});
            }
            else if(createSave)
            {
                // Observed Umbrella first-run prompt in the disposable NAND.
                // Its Yes target is left/below centre and needs pointed input;
                // pressing B repeatedly in the centre cannot select it.
                int x=(int)(32767*(discId.StartsWith("SBD")?.40:.28)),y=(int)(32767*(discId.StartsWith("SBD")?.60:.67));
                var tokens=accept.Buttons.Select(id=>StartupControls.Hardware(players[0]).Single(h=>h.Id==id).Token!).Distinct().ToArray();
                devices[0].SubmitMouse(new HMMouseState{X=x,Y=y});await Delay(400);
                try{DolphinChord(devices[0],tokens,x,y);await Delay(150);}finally{DolphinChord(devices[0],[],x,y);}
                evidence.Add(new {stage="dolphin-create-test-save",x,y,note="Pointed confirmation inside isolated NAND only; original saves are not copied or modified."});
            }
            else if(prompt)
            {
                var tokens=accept.Buttons.Select(id=>StartupControls.Hardware(players[0]).Single(h=>h.Id==id).Token!).Distinct().ToArray();
                try{DolphinChord(devices[0],tokens);await Delay(150);}finally{DolphinChord(devices[0],[]);}
            }
            else
            {
                var skip=actions.First(a=>a.Player==1&&a.Function=="Buttons/B");
                var tokens=skip.Buttons.Select(id=>StartupControls.Hardware(players[0]).Single(h=>h.Id==id).Token!).Distinct().ToArray();
                evidence.Add(new {stage="dolphin-boot-skip",button="B",note="Skip intro/navigation only; this is not a gameplay action verdict."});
                try{DolphinChord(devices[0],tokens);await Delay(150);}finally{DolphinChord(devices[0],[]);}
            }
            await Delay(5000);
        }
        throw new IOException("Dolphin did not reach the reviewed title prompt; input delivery is not gameplay verification.");
    }

    private static bool DolphinCreateSaveText(string text,string discId) => (discId.StartsWith("RBU")||discId.StartsWith("SBD"))
        && System.Text.RegularExpressions.Regex.IsMatch(text,@"Save\s+game\s+data\s+not\s+found.*Create\s+data",System.Text.RegularExpressions.RegexOptions.IgnoreCase|System.Text.RegularExpressions.RegexOptions.Singleline);

    private static bool DolphinReadyText(string text)
    {
        var options=System.Text.RegularExpressions.RegexOptions.IgnoreCase;
        bool Match(string pattern)=>System.Text.RegularExpressions.Regex.IsMatch(text,pattern,options);
        if(Match("LOADING|ZAPPER|WRIST|STRAP"))return false;
        string compact=System.Text.RegularExpressions.Regex.Replace(text.ToUpperInvariant(),"[^A-Z]","");
        return Match(@"PRESS.*(?:START|BEGIN)")
            ||compact.Contains("DEADSPACE")&&Match(@"Electronic.*Arts|Rights.*Reserved")
            ||compact.Contains("OVERKILL")&&Match(@"PRESS.*(?:A|BUTTON)")
            ||compact.Contains("RESIDENTEVIL")&&compact.Contains("CHRONICLES")&&Match(@"PRESS.*(?:A|BUTTON)|NEW.*GAME|LOAD.*GAME")
            ||Match(@"Story.*Mode")&&Match(@"Challenge|Options")
            ||Match(@"Single.*Player")&&Match(@"Cooperative|Options");
    }

    private static async Task<string> DolphinSceneText(string file)
    {
        string text=await SceneText(file);
        // Very large stylized title lettering can be ignored by Windows OCR.
        // Read the upper part at two smaller scales without storing game art.
        using var source=new System.Drawing.Bitmap(file);
        foreach(int width in new[]{480,320})
        {
            int cropHeight=(int)(source.Height*.65);
            using var small=new System.Drawing.Bitmap(width,Math.Max(1,cropHeight*width/source.Width));
            using(var graphics=System.Drawing.Graphics.FromImage(small))
                graphics.DrawImage(source,new System.Drawing.Rectangle(0,0,small.Width,small.Height),new System.Drawing.Rectangle(0,0,source.Width,cropHeight),System.Drawing.GraphicsUnit.Pixel);
            string temporary=Path.Combine(Path.GetTempPath(),"deadeye-ocr-"+Guid.NewGuid().ToString("N")+".png");
            try {small.Save(temporary,System.Drawing.Imaging.ImageFormat.Png);text+="\n"+await SceneText(temporary);}
            finally {File.Delete(temporary);}
        }
        return text;
    }

    private static async Task DolphinAction(GameSession session,HMController[] devices,List<GunBinding> players,SimulatedAction action,string scope="dolphin")
    {
        RequireGame(session);int p=action.Player;
        var hardware=StartupControls.Hardware(players[p-1]);
        var tokens=action.Buttons.Select(id=>hardware.Single(h=>h.Id==id).Token??throw new IOException("Missing physical token.")).Distinct().ToArray();
        if(tokens.Length==0)throw new IOException("No resolved physical input for "+scope+" action.");
        string before=Capture(session,$"{scope}-p{p}-{action.Function}-before");Drain();string? held=null;
        try {DolphinChord(devices[p-1],tokens);await Delay(requestedHoldMilliseconds??200);held=Capture(session,$"{scope}-p{p}-{action.Function}-held");}
        finally {DolphinChord(devices[p-1],[]);}
        await Delay(300);var received=Drain();
        bool Match(RawPacket r,GunBinding gun,string token,bool down)=>token.StartsWith("mouse:")
            ?r.DeviceId==gun.MouseId&&(r.Buttons&((down?1:2)<<(2*(int.Parse(token[6..])-1))))!=0
            :r.DeviceId==gun.KeyboardId&&r.Key==int.Parse(token[4..])&&r.Down==down;
        bool pressed=tokens.All(t=>received.Any(r=>Match(r,players[p-1],t,true)));
        bool released=tokens.All(t=>received.Any(r=>Match(r,players[p-1],t,false)));
        bool isolated=!tokens.Any(t=>received.Any(r=>Match(r,players[2-p],t,true)));
        evidence.Add(new {stage=scope+"-action",action.Player,action.Function,action.Expression,action.Buttons,tokens,rawPressed=pressed,rawReleased=released,isolated,before,heldFrame=held,after=Capture(session,$"{scope}-p{p}-{action.Function}-after"),gameResponse="requires-visual-review"});
        if(!pressed||!released||!isolated)throw new IOException(scope+" action has incomplete or cross-player RawInput events.");
    }

    private static void DolphinChord(HMController device,string[] tokens,int x=16384,int y=16384)
    {
        var keys=new List<HMKey>();HMMouseButtons mouse=0;
        foreach(string token in tokens)
        {
            int code=int.Parse(token[(token.IndexOf(':')+1)..]);
            if(token.StartsWith("mouse:")){if(code is <1 or >5)throw new NotSupportedException("Unsupported mouse button.");mouse|=(HMMouseButtons)(1<<(code-1));}
            else
            {
                int usage=code switch {>=65 and <=90=>code-65+4,>=49 and <=57=>code-49+0x1e,48=>0x27,13=>0x28,32=>0x2c,37=>0x50,38=>0x52,39=>0x4f,40=>0x51,_=>throw new NotSupportedException("Dolphin replay refuses an unreviewed key.")};
                keys.Add((HMKey)usage);
            }
        }
        device.SubmitMouse(new HMMouseState{X=x,Y=y,Buttons=mouse});
        device.SubmitKeyboard(HMKeyModifiers.None,keys.ToArray());
    }
}
