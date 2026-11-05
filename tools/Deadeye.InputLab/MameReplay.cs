using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using HIDMaestro;
using Reaper.Core;
using Reaper.Windows;

namespace Deadeye.InputLab;

internal static partial class Program
{
    // Start with the two reviewed launch recipes. Scripts, helpers and custom
    // output arguments require their own isolation review before replay.
    private static bool SupportsMameReplay(GameEntry game) => MameControls.Rom(game) is "alien3" or "term2"
        && (game.Helpers??[]).Length==0
        && (game.Arguments.Length==1 || game.Arguments.Length==3 && game.Arguments[1]=="-rompath");

    private static async Task ReplayMame(GameEntry original,string data,List<GunBinding> players,
        HMController[] devices,string libraryFile,string libraryHash)
    {
        var info=new ProcessStartInfo(original.Executable) {WorkingDirectory=original.WorkingDirectory,
            UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        foreach(string arg in original.Arguments.Append("-showconfig"))info.ArgumentList.Add(arg);
        using var query=Process.Start(info)??throw new IOException("MAME configuration query did not start.");
        using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var stdout=query.StandardOutput.ReadToEndAsync(timeout.Token);var stderr=query.StandardError.ReadToEndAsync(timeout.Token);
        try {await query.WaitForExitAsync(timeout.Token);}
        catch(OperationCanceledException) {query.Kill(true);await query.WaitForExitAsync();throw new IOException("MAME configuration query timed out.");}
        if(query.ExitCode!=0)throw new IOException("MAME configuration query: "+await stderr);
        string config=await stdout;
        string DirectoryOption(string name)
        {
            var match=Regex.Match(config,@"(?m)^"+Regex.Escape(name)+@"\s+(.+?)\s*$");
            string value=Environment.ExpandEnvironmentVariables(match.Groups[1].Value.Trim().Trim('"'));
            if(!match.Success||string.IsNullOrWhiteSpace(value)||value.Contains(';'))
                throw new IOException("Ambiguous MAME output directory: "+name);
            return Path.GetFullPath(value,original.WorkingDirectory);
        }
        string cfg=DirectoryOption("cfg_directory"),nvram=DirectoryOption("nvram_directory");
        string rom=MameControls.Rom(original)!,isolated=Path.Combine(output,"mame-user"),isolatedData=Path.Combine(isolated,"deadeye");
        RefuseLinkedPath(Path.GetFullPath(isolated));
        foreach(string source in new[]{cfg,nvram})
            if(Within(isolated,source))throw new IOException("Replay output must be outside the original MAME configuration and NVRAM directories.");
        if(Directory.Exists(isolated))throw new IOException("MAME replay requires a new output directory.");
        var before=new Dictionary<string,string?>(StringComparer.OrdinalIgnoreCase);
        void Remember(string path)=>before.TryAdd(path,File.Exists(path)?Hash(path):null);
        long copied=0;
        void Copy(string source,string target)
        {
            Remember(source);if(!File.Exists(source))return;
            RefuseLinkedPath(source);
            copied+=new FileInfo(source).Length;
            if(copied>32*1024*1024)throw new IOException("MAME calibration/configuration exceeds the replay copy limit.");
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);File.Copy(source,target,false);
        }
        foreach(string name in new[]{"default.cfg",rom+".cfg"})Copy(Path.Combine(cfg,name),Path.Combine(isolated,"cfg",name));
        string savedNvram=Path.Combine(nvram,rom);
        string[] NvramFiles()=>Directory.Exists(savedNvram)?Directory.EnumerateFiles(savedNvram,"*",new EnumerationOptions {RecurseSubdirectories=true,AttributesToSkip=FileAttributes.ReparsePoint}).Order(StringComparer.OrdinalIgnoreCase).ToArray():[];
        var nvramFiles=NvramFiles();
        void CopyNvram(string folder)
        {
            if((File.GetAttributes(folder)&FileAttributes.ReparsePoint)!=0)throw new IOException("Replay refuses linked NVRAM directories.");
            foreach(string file in Directory.EnumerateFiles(folder))Copy(file,Path.Combine(isolated,"nvram",rom,Path.GetRelativePath(savedNvram,file)));
            foreach(string sub in Directory.EnumerateDirectories(folder))CopyNvram(sub);
        }
        if(Directory.Exists(savedNvram))CopyNvram(savedNvram);
        Copy(Path.Combine(nvram,rom+".nv"),Path.Combine(isolated,"nvram",rom+".nv"));
        Remember(Path.Combine(original.WorkingDirectory,"mame.ini"));Remember(Path.Combine(original.WorkingDirectory,"ui.ini"));
        Remember(Path.Combine(data,"controllers","reaper.cfg"));Remember(MameControls.MetadataPath(original,data));
        string[] options=["cfg_directory","nvram_directory","state_directory","snapshot_directory","diff_directory","input_directory","comment_directory"];
        var args=original.Arguments.ToList();
        foreach(string option in options)
        {
            string target=Path.Combine(isolated,option=="cfg_directory"?"cfg":option=="nvram_directory"?"nvram":option);
            Directory.CreateDirectory(target);args.Add("-"+option);args.Add(target);
        }
        // Avoid restoring or overwriting a user's save state during a gun test.
        args.Add("-noautosave");
        var game=original with {Arguments=args.ToArray()};
        File.WriteAllText(Path.Combine(output,"mame-isolation.json"),JsonSerializer.Serialize(new {
            originalConfig=cfg,originalNvram=nvram,isolatedUser=isolated,originalFiles=before,
            copiedBytes=copied,romCopied=false,calibrationCopied=true,arguments=game.Arguments
        },json));
        var session=new GameSession();Task? running=null;
        raw.Packet+=session.Input;
        try
        {
            var ready=new TaskCompletionSource<nint>(TaskCreationOptions.RunContinuationsAsynchronously);
            session.GameWindowReady+=h=>ready.TrySetResult(h);
            running=session.Run(game,isolatedData,players,cancel.Token,fixesPrepared:true);
            var winner=await Task.WhenAny(ready.Task,running,Task.Delay(60000,cancel.Token));
            if(winner!=ready.Task){if(running.IsCompleted)await running;throw new IOException("No MAME renderer appeared within sixty seconds.");}
            var controls=OverlayControls.Read(game,isolatedData,players,raw.Devices);
            File.WriteAllText(Path.Combine(output,"launched-controls.json"),JsonSerializer.Serialize(controls,json));
            var plan=await ControlSimulation.Run(new[]{game},isolatedData,players,raw.Devices);
            var actions=plan.Games.Single().Actions;
            evidence.Add(new {stage="mame-action-plan",plan,emulatorHash=Hash(game.Executable)});
            if(plan.Games.Single().Status!="profile-routed"||actions.Length==0||actions.Any(a=>a.Status!="profile-routed"||!a.Isolated||a.Buttons.Length==0))
                throw new IOException("MAME action plan has unresolved or colliding inputs; live replay refused.");
            await Delay(12000);RequireGame(session);
            evidence.Add(new {stage="mame-initial-scene",frame=Capture(session,"mame-initial-scene")});
            foreach(var action in actions.Where(a=>Regex.IsMatch(a.Function,"coin",RegexOptions.IgnoreCase)))
                for(int n=0;n<2;n++)await DolphinAction(session,devices,players,action,"mame");
            foreach(var action in actions.Where(a=>a.Function=="Start"))await DolphinAction(session,devices,players,action,"mame");
            // Alien3's start prompt also accepts the mapped trigger.
            foreach(var action in actions.Where(a=>a.Buttons.Contains("trigger")))await DolphinAction(session,devices,players,action,"mame");
            // Alien3's observed stage intro outlasts the generic warmup. Its
            // reticles are absent during that intro, so wait before the grid.
            await Delay(rom=="alien3"?25000:8000);
            evidence.Add(new {stage="mame-after-start",frame=Capture(session,"mame-after-start"),gameResponse="requires-visual-review"});
            foreach(var player in players)
            {
                foreach(var point in new[]{(0,0),(32767,0),(32767,32767),(0,32767),(16384,16384)})
                {
                    RequireGame(session);Drain();devices[player.Player-1].SubmitMouse(new HMMouseState{X=point.Item1,Y=point.Item2});await Delay(300);
                    var received=Drain();bool delivered=received.Any(p=>p.DeviceId==player.MouseId&&p.Absolute&&Math.Abs(p.X-point.Item1*65535d/32767)<=64&&Math.Abs(p.Y-point.Item2*65535d/32767)<=64);
                    evidence.Add(new {stage="mame-aim",player.Player,x=point.Item1,y=point.Item2,delivered,frame=Capture(session,$"mame-p{player.Player}-aim-{point.Item1}-{point.Item2}"),gameResponse="requires-visual-review"});
                    if(!delivered)throw new IOException("MAME virtual aim packet was not received.");
                }
                await Sweep(session,devices[player.Player-1],player);
            }
            // A combined Shoot / Start label is still a gameplay fire action.
            // Exclude only standalone credit/start rows, not substrings.
            foreach(var action in actions.Where(a=>MameGameplayReplay(a.Function)))await DolphinAction(session,devices,players,action,"mame");
        }
        finally
        {
            foreach(var device in devices)try{device.SubmitKeyboard(HMKeyModifiers.None,ReadOnlySpan<HMKey>.Empty);device.SubmitMouse(new HMMouseState{X=16384,Y=16384});}catch(Exception e){evidence.Add(new {stage="release-input",error=e.Message});}
            try {await session.End();}
            finally
            {
                if(running is not null)try{await running.WaitAsync(TimeSpan.FromSeconds(15));}catch(Exception e){evidence.Add(new {stage="session-end",error=e.Message});}
                raw.Packet-=session.Input;
                bool untouched=before.All(p=>p.Value is null?!File.Exists(p.Key):File.Exists(p.Key)&&Hash(p.Key)==p.Value)
                    &&nvramFiles.SequenceEqual(NvramFiles(),StringComparer.OrdinalIgnoreCase);
                bool libraryUnchanged=Hash(libraryFile)==libraryHash;
                evidence.Add(new {stage="restore",isolatedUser=true,originalConfigUnchanged=untouched,libraryUnchanged,session.CleanupIssues});
                if(!untouched||!libraryUnchanged)throw new IOException("Original MAME configuration or library changed during isolated replay; newer contents retained.");
            }
        }
    }
    private static bool MameGameplayReplay(string function)=>!Regex.IsMatch(function,@"^(?:coin|start)$",RegexOptions.IgnoreCase);

    private static bool Within(string path,string directory) => Path.GetFullPath(path).Equals(Path.GetFullPath(directory),StringComparison.OrdinalIgnoreCase)
        ||Path.GetFullPath(path).StartsWith(Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase);

    private static void RefuseLinkedPath(string file)
    {
        for(string? path=Path.GetFullPath(file);path is not null;path=Path.GetDirectoryName(path))
            if((File.Exists(path)||Directory.Exists(path))&&(File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0)
                throw new IOException("Replay refuses linked configuration paths.");
    }
}
