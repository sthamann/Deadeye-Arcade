using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.Win32.SafeHandles;
using HIDMaestro;
using Reaper.Core;
using Reaper.Windows;

namespace Deadeye.InputLab;

// An opt-in diagnostic consumer of the real launch path. It never saves LibraryState.
internal static partial class Program
{
    private static int? requestedHoldMilliseconds;
    private static readonly ConcurrentQueue<RawPacket> packets = new();
    private static readonly ConcurrentQueue<(RawPacket Packet,long Time)> timed = new();
    private static readonly List<object> evidence = [];
    private static readonly CancellationTokenSource cancel = new();
    private static readonly JsonSerializerOptions json = new(JsonDefaults.Options);
    private static RawInput raw = null!;
    private static Dispatcher receiver = null!;
    private static string output = "";
    private static int exit = 1;
    private static AliensObservation? gameObservation;
    private static TeknoPipeObservation? pipe;

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if(args is ["read-scene",var imagePath])
            {
                Console.WriteLine(Task.Run(()=>DolphinSceneText(Path.GetFullPath(imagePath))).GetAwaiter().GetResult());return 0;
            }
            if(args.Length>0 && args[0]=="capture")
            {
                // Read-only capture also works for games without a replay adapter.
                // It creates no virtual devices and does not load or change a profile.
                string destination=Option(args,"--output")??throw new ArgumentException("capture requires --output DIR");
                Directory.CreateDirectory(destination);
                nint foreground=GetForegroundWindow();
                GetWindowThreadProcessId(foreground,out uint pid);
                using var frame=GpuCapture.Frame(MonitorFromWindow(foreground,2));
                using var small=new Bitmap(1600,Math.Max(1,(int)(frame.Height*1600d/frame.Width)));
                using(var graphics=Graphics.FromImage(small))graphics.DrawImage(frame,0,0,small.Width,small.Height);
                string name=DateTime.UtcNow.ToString("HHmmssfff");
                small.Save(Path.Combine(destination,name+".jpg"),ImageFormat.Jpeg);
                File.WriteAllText(Path.Combine(destination,name+".json"),JsonSerializer.Serialize(new {
                    time=DateTimeOffset.Now,processId=pid,process=Process.GetProcessById((int)pid).ProcessName,
                    width=frame.Width,height=frame.Height,remote=GetSystemMetrics(0x1000)!=0,
                    captureBackend="dxgi-desktop-duplication",note="Observation only; no input or gameplay verification."
                },json));
                return 0;
            }
            if(args is ["--self-check"])
            {
                if(!SceneSignals("FREEPLRY").Ready||!SceneSignals("PRESS STRRT TO BEGIN").Ready
                    ||!SceneSignals("Dress to start").Ready||!SceneSignals("Dress to stort").Ready
                    ||!SceneSignals("INSERT CREDITS").Ready
                    ||SceneSignals("").Ready||SceneSignals("Loading audio systems 30% FREEPLAY").Ready
                    ||SceneSignals("NO GVRIO CARD FOUND").Ready
                    ||!DolphinReadyText("DEAD SPACE EXTRACTION 2009 Electronic Arts Inc. All Rights Reserved.")
                    ||!DolphinReadyText("HOUSE OF THE DEAD OVERKILL Press A Button")
                    ||!DolphinReadyText("Story Mode Challenge Mode Options")
                    ||DolphinReadyText("Wii Zapper Use Press A")||DolphinReadyText("Loading Press A or B to Start")
                    ||DolphinReadyText("OVERKILL The capability to obliterate a target with more weapons than required")
                    ||!DolphinReadyText("RESIDENT EVIL THE UMBRELLA CHRONICLES Press any button")
                    ||!DolphinReadyText("RESIDENT EVIL THE DARKSIDE CHRONICLES New Game Load Game")
                    ||DolphinReadyText("RESIDENT EVIL CHRONICLES CAPCOM All Rights Reserved")
                    ||DolphinReadyText("Loading Resident Evil Chronicles New Game"))throw new IOException("Scene signal regression failed.");
                var mame = new GameEntry("fixture","Alien3","MAME","mame.exe",["alien3","-rompath","roms"],".","mame","alien3.zip");
                if(!SupportsMameReplay(mame)||SupportsMameReplay(mame with {Arguments=["alien3","-autoboot_script","unknown.lua"]})
                    ||SupportsMameReplay(mame with {Arguments=["unreviewed"]})
                    ||!Within(Path.Combine("sample","cfg","child"),Path.Combine("sample","cfg"))
                    ||Within(Path.Combine("sample","cfg-other"),Path.Combine("sample","cfg")))throw new IOException("MAME replay scope regression failed.");
                if(!DolphinCreateSaveText("Save game data not found. Create data?","RBUE08")
                    ||DolphinCreateSaveText("Delete existing save data?","RBUE08")
                    ||!DolphinCreateSaveText("Save game data not found. Create data?","SBDE08")
                    ||DolphinCreateSaveText("Save game data not found. Create data?","RZJE69")
                    ||DolphinReadyText("Save game data not found. Create data?"))throw new IOException("Dolphin first-run prompt scope regression failed.");
                if(SceneSignals("KONAMI AMUSEMENT FREE PLAY",requireStart:true).Ready
                    ||SceneSignals("Please wait WARNING FREE PLAY",requireStart:true).Ready
                    ||SceneSignals("NOW LOADING Press the START button FREE PLAY",requireStart:true).Ready
                    ||!SceneSignals("Press the START button FREE PLAY",requireStart:true).Ready)
                    throw new IOException("Silent Hill boot scene checks failed.");
                Console.WriteLine("Scene signal checks passed (22 cases); MAME replay scope checks passed (five cases); Dolphin save prompt checks passed (five cases).");return 0;
            }
            if(args.Length < 2 || args[0] is not ("plan" or "run"))
                throw new ArgumentException("Usage: Deadeye.InputLab plan|run GAME_ID [--data DIR] [--output DIR] [--install-driver] [--input-route-probe] [--windowed-probe] [--hold-ms 50..2000]; capture --output DIR; read-scene IMAGE");
            if(Option(args,"--hold-ms") is {} hold)
            {
                if(!int.TryParse(hold,out int milliseconds)||milliseconds is <50 or >2000)
                    throw new ArgumentException("--hold-ms must be between 50 and 2000 milliseconds.");
                requestedHoldMilliseconds=milliseconds;
            }
            string data = Option(args,"--data") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"ReaperArcade");
            output = Path.GetFullPath(Option(args,"--output") ?? Path.Combine(data,"input-lab",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")));
            var library = new LibraryStore(data).Load();
            var game = library.Games.Single(g=>g.Id==args[1]);
            if(args[0]=="plan")
            {
                Console.WriteLine(JsonSerializer.Serialize(new { game.Id,game.Title,
                    backend=game.Source=="teknoparrot"?"virtual-absolute-hid":SupportsDolphinReplay(game)?"isolated-dolphin-dsu":SupportsMameReplay(game)?"isolated-mame-rawinput":"not-yet-supported",
                    controls=OverlayControls.Read(game,data,library.Bindings),
                    note="A plan reads configuration only. It does not install a driver or verify gameplay." },json));
                return 0;
            }
            bool dolphin=DolphinSetup.IsDolphin(game);
            if(dolphin)
            {
                if(!SupportsDolphinReplay(game))
                    throw new NotSupportedException("The isolated Dolphin replay covers Extraction, Overkill and the Resident Evil Chronicles without external helpers.");
                if(args.Contains("--input-route-probe")||args.Contains("--windowed-probe"))
                    throw new NotSupportedException("TeknoParrot comparison flags do not apply to Dolphin.");
            }
            else if(SupportsMameReplay(game))
            {
                if(args.Contains("--input-route-probe")||args.Contains("--windowed-probe"))
                    throw new NotSupportedException("TeknoParrot comparison flags do not apply to MAME.");
            }
            else
            {
                if(game.Source!="teknoparrot" || (game.Helpers??[]).Length!=0)
                    throw new NotSupportedException("This pilot runs direct TeknoParrot profiles or isolated Extraction/Overkill Dolphin profiles only.");
                var selectedProfile=XDocument.Load(game.SourcePath);
                string? Setting(string name)=>selectedProfile.Descendants("FieldInformation").FirstOrDefault(f=>f.Element("CategoryName")?.Value=="General"&&f.Element("FieldName")?.Value==name)?.Element("FieldValue")?.Value;
                if(Setting("Input API")!="RawInput"||Setting("Use Relative Input")=="1")
                    throw new NotSupportedException("This adapter requires an absolute RawInput profile. Other input APIs need their own replay adapter.");
                string? profileId=selectedProfile.Root?.Element("EmulationProfile")?.Value;
                bool existingExLibraries=profileId=="Hotd4ex" && new[]{"librnalindbergh_jr.so","libcri_soundoutput_lindbergh_jr.so"}
                    .All(name=>File.Exists(Path.Combine(Path.GetDirectoryName(game.Executable)!,"ElfLdr2","libs",name)));
                if(profileId=="PrimevalHunt" || profileId=="Hotd4ex"&&!existingExLibraries)
                    throw new NotSupportedException("This profile can prepare shared loader libraries and requires an extended restoration recipe before replay.");
            }
            if(library.Bindings.Count(b=>b.Player is 1 or 2)!=2)
                throw new InvalidOperationException("Assign two real gun profiles first; the lab reuses their actual learned buttons without changing the library.");
            using var mutex = new Mutex(true,"Local\\ReaperArcade-v1",out bool first);
            if(!first) throw new InvalidOperationException("Close Deadeye Arcade before running the input lab.");
            Directory.CreateDirectory(output);
            var dispatcher=Dispatcher.CurrentDispatcher;receiver=dispatcher;
            using var window=new HwndSource(new HwndSourceParameters("Deadeye Input Lab receiver") { ParentWindow=(nint)(-3),Width=1,Height=1 });
            using var input=new RawInput();raw=input;
            window.AddHook((nint h,int m,nint w,nint l,ref bool handled)=> {raw.Message(m,l);return 0;});
            raw.Register(window.Handle);
            raw.Packet+=p=>
            {
                packets.Enqueue(p);timed.Enqueue((p,Stopwatch.GetTimestamp()));
                while(packets.Count>4096)packets.TryDequeue(out _);
                while(timed.Count>4096)timed.TryDequeue(out _);
            };
            Console.CancelKeyPress+=(_,e)=>{e.Cancel=true;cancel.Cancel();};
            _=Task.Run(async()=>
            {
                try { await Run(game,data,library.Bindings,args.Contains("--install-driver"),args.Contains("--windowed-probe"),args.Contains("--input-route-probe"));exit=0; }
                catch(Exception e) { evidence.Add(new {stage="error",error=e.ToString()});Console.Error.WriteLine(e.Message); }
                finally {File.WriteAllText(Path.Combine(output,"report.json"),JsonSerializer.Serialize(new {
                    game.Id,game.Title,time=DateTimeOffset.Now,exit,evidence,gameplayVerified=false,requestedHoldMilliseconds,
                    toolVersion=typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,toolHash=Hash(typeof(Program).Assembly.Location),
                    captureBackend="dxgi-desktop-duplication",verificationLevel=args.Contains("--input-route-probe")?"emulator-input-route":"raw-input-and-scene-captures",
                    verdict="Input delivery and scene captures are recorded separately. Screenshots require game-specific review; this report is not an all-buttons gameplay certificate."
                },json));dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);}
            });
            Dispatcher.Run();return exit;
        }
        catch(Exception e) {Console.Error.WriteLine(e.Message);return 2;}
    }

    private static string? Option(string[] a,string key) {int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:null;}
    private static string Hash(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static async Task Delay(int ms)
    {
        for(int i=0;i<ms;i+=25) {cancel.Token.ThrowIfCancellationRequested();if((GetAsyncKeyState(0x7b)&0x8000)!=0)throw new OperationCanceledException("F12 stopped the input lab.");await Task.Delay(Math.Min(25,ms-i),cancel.Token);}
    }

    private static async Task Run(GameEntry game,string data,List<GunBinding> saved,bool allowInstall,bool windowedProbe,bool routeProbe)
    {
        if(!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            throw new InvalidOperationException("Virtual device creation requires an elevated console. Plan mode needs no elevation.");
        if(Hash(typeof(HMContext).Assembly.Location)!="F13E32591404DB0F418AF9265B8E240F71257297CFAE5A41DCD696FD87E9B4D5")
            throw new IOException("The HIDMaestro assembly differs from the reviewed 2.1.0 payload.");
        // The SDK can otherwise deploy a newer payload from CreateController.
        // Refuse that implicit deployment when installation was not requested.
        var fastPath=typeof(HMContext).Assembly.GetType("HIDMaestro.Internal.DriverBuilder")?
            .GetMethod("WillTakeFastPath",BindingFlags.Static|BindingFlags.NonPublic);
        if(!allowInstall&&fastPath?.Invoke(null,null) is not true)
            throw new InvalidOperationException("The reviewed driver payload is absent or needs deployment. Obtain explicit approval before using --install-driver.");
        // HMContext may stage its payload. Driver deployment, signing and certificate
        // trust require the explicit installation flag; ordinary runs refuse it.
        using var context=new HMContext();
        Console.WriteLine("Checking the reviewed virtual HID driver");
        if(!context.IsDriverInstalled)
        {
            if(!allowInstall)throw new InvalidOperationException("HIDMaestro is not installed. Its installation adds a local signing certificate to LocalMachine My, Root and TrustedPublisher. Obtain explicit approval before using --install-driver.");
            context.InstallDriver();
        }
        using var p1=context.CreateController(Profile(1),"deadeye-input-lab-p1");
        using var p2=context.CreateController(Profile(2),"deadeye-input-lab-p2");
        var controllers=new[]{p1,p2};
        evidence.Add(new {stage="driver",context.IsDriverInstalled,remote=GetSystemMetrics(0x1000)!=0,desktop=InputDesktopDiagnostics.Read()});
        var players=new List<GunBinding>();
        var until=Stopwatch.StartNew();
        while(players.Count<2 && until.Elapsed<TimeSpan.FromSeconds(15))
        {
            receiver.Invoke(raw.Refresh);players.Clear();
            var enumerated=raw.Devices.ToArray();
            for(int p=1;p<=2;p++)
            {
                string marker=$"VID_F0DE&PID_A00{p}";
                bool Matches(InputDevice d)=>d.Id.Contains(marker,StringComparison.OrdinalIgnoreCase)||HasTestIdentity(d.Id,p);
                var mouse=enumerated.SingleOrDefault(d=>d.Kind=="mouse"&&Matches(d));
                var keyboard=enumerated.SingleOrDefault(d=>d.Kind=="keyboard"&&Matches(d));
                if(mouse is null||keyboard is null)continue;
                players.Add(saved.Single(b=>b.Player==p) with {MouseId=mouse.Id,KeyboardId=keyboard.Id,SerialPort=null,PhysicalId=null,HardwarePlayer=p});
            }
            if(players.Count<2)await Delay(100);
        }
        if(players.Count!=2)
        {
            evidence.Add(new {stage="enumeration-failed",devices=raw.Devices.Select(d=>new {d.Id,d.Name,d.Kind,p1=HasTestIdentity(d.Id,1),p2=HasTestIdentity(d.Id,2)})});
            throw new IOException("Two distinct virtual mouse and keyboard pairs did not enumerate. No game profile was modified.");
        }
        evidence.Add(new {stage="devices",players=players.Select(b=>new {b.Player,b.MouseId,b.KeyboardId,b.SystemId})});
        Console.WriteLine("Two independent mouse and keyboard pairs are ready");
        cancel.CancelAfter(TimeSpan.FromMinutes(5));
        string libraryFile=Path.Combine(data,"library.json"),libraryHash=Hash(libraryFile);
        if(DolphinSetup.IsDolphin(game))
        {await ReplayDolphin(game,data,players,controllers,libraryFile,libraryHash);return;}
        if(SupportsMameReplay(game))
        {await ReplayMame(game,data,players,controllers,libraryFile,libraryHash);return;}
        string backup=Path.Combine(output,"profile-before.xml");File.Copy(game.SourcePath,backup,false);
        string? preparedHash=null;
        File.WriteAllText(Path.Combine(output,"restore.json"),JsonSerializer.Serialize(new {path=game.SourcePath,backup,beforeHash=Hash(backup),preparedHash},json));
        evidence.Add(new {stage="emulator-build",sha256=Hash(game.Executable)});
        var session=new GameSession();Task? running=null;
        using var neutralStop=CancellationTokenSource.CreateLinkedTokenSource(cancel.Token);
        // Real tracking guns already emit positions while the game boots. Keep
        // the virtual devices alive too, without any held buttons or keys.
        var neutral=Task.Run(async()=>
        {
            try
            {
                while(!neutralStop.IsCancellationRequested)
                {
                    foreach(var c in controllers)c.SubmitMouse(new HMMouseState{X=16384,Y=16384});
                    await Task.Delay(16,neutralStop.Token);
                }
            }
            catch(OperationCanceledException) when(neutralStop.IsCancellationRequested) { }
        });
        try
        {
            var ready=new TaskCompletionSource<nint>(TaskCreationOptions.RunContinuationsAsynchronously);
            session.GameWindowReady+=h=>ready.TrySetResult(h);
            session.ProfilePrepared+=()=>
            {
                if(windowedProbe)
                {
                    if(Path.GetFileNameWithoutExtension(game.SourcePath) is not ("AliensExtermination" or "SilentHill"))
                        throw new NotSupportedException("The windowed comparison is restricted to Aliens Extermination and Silent Hill.");
                    var profile=XDocument.Load(game.SourcePath);
                    var value=profile.Descendants("FieldInformation").First(f=>f.Element("CategoryName")?.Value=="General"&&f.Element("FieldName")?.Value=="Windowed").Element("FieldValue")!;
                    value.Value="1";
                    if(Path.GetFileNameWithoutExtension(game.SourcePath)=="SilentHill")
                    {
                        var cursor=profile.Descendants("FieldInformation").FirstOrDefault(f=>f.Element("FieldName")?.Value=="HideCursor")?.Element("FieldValue");
                        if(cursor is not null)cursor.Value="0";
                    }
                    profile.Save(game.SourcePath);
                    evidence.Add(new {stage="diagnostic-override",field="General/Windowed",value="1",note="Temporary comparison; restored after the session."});
                }
                preparedHash=Hash(game.SourcePath);
                File.WriteAllText(Path.Combine(output,"restore.json"),JsonSerializer.Serialize(new {path=game.SourcePath,backup,beforeHash=Hash(backup),preparedHash},json));
            };
            running=session.Run(game,data,players,cancel.Token,fixesPrepared:true);
            var winner=await Task.WhenAny(ready.Task,running,Task.Delay(60000,cancel.Token));
            if(winner!=ready.Task) {if(running.IsCompleted)await running;throw new IOException("No usable renderer appeared within sixty seconds.");}
            var controls=OverlayControls.Read(game,data,players,raw.Devices);
            gameObservation=AliensObservation.Attach(session,game);pipe=TeknoPipeObservation.Attach(game);
            evidence.Add(new {stage="emulator-input-observer",available=pipe is not null,routeProbe,note="Read-only emulator input exchange; not a gameplay measurement."});
            if(routeProbe&&pipe is null)throw new NotSupportedException("Input route probe requires the exact reviewed Aliens emulator build.");
            evidence.Add(new {stage="game-observer",available=gameObservation is not null,build=AliensObservation.LastBuildFingerprint,note="Read-only input, player and ammo values for exact known Aliens builds; other builds remain unverified."});
            File.WriteAllText(Path.Combine(output,"launched-controls.json"),JsonSerializer.Serialize(controls,json));
            var actions=controls.Rows.Where(r=>r.Player is 1 or 2&&!r.IsAim&&!Administrative(r)).ToArray();
            evidence.Add(new {stage="control-audit",audit=GameControlAudit.Read(game,data,players),actions=actions.Select(r=>new {r.Player,r.Function,r.Expression})});
            if(!routeProbe)await WaitForPlayableScene(session,game);
            else
            {
                await Delay(2000);RequireGame(session);
                evidence.Add(new {stage="scene-not-verified",note="Explicit input-route-only test. Gameplay remains unverified."});
            }
            neutralStop.Cancel();await neutral;
            // Credits and Start first. They are read from the launched profile.
            foreach(var row in actions.Where(r=>Regex.IsMatch(r.Function,"coin|credits?|start",RegexOptions.IgnoreCase))
                .OrderBy(r=>Regex.IsMatch(r.Function,"start",RegexOptions.IgnoreCase)?1:0))
            {
                // The observed Tomb Raider cabinet needs four coin pulses per
                // player. One pulse only reached 1/4 credit and could not start.
                int presses=Path.GetFileNameWithoutExtension(game.SourcePath)=="TombRaider"
                    &&Regex.IsMatch(row.Function,"coin",RegexOptions.IgnoreCase)?4:1;
                for(int i=0;i<presses;i++)await Action(session,controllers,players,row);
            }
            await Delay(5000);
            if(!routeProbe&&Path.GetFileNameWithoutExtension(game.SourcePath)=="SilentHill")
            {
                evidence.Add(new {stage="silent-hill-after-start",frame=Capture(session,"silent-hill-after-start")});
                var mission=Stopwatch.StartNew();bool hud=false;
                while(mission.Elapsed<TimeSpan.FromSeconds(120))
                {
                    string frame=Capture(session,"silent-hill-mission-wait");string text=await SceneText(Path.Combine(output,frame));
                    hud=Regex.IsMatch(text,"ERIC|TINA",RegexOptions.IgnoreCase)
                        &&!Regex.IsMatch(text,@"PLEASE\s*WAIT|HOLD.*FIRE|PRESS.*START",RegexOptions.IgnoreCase);
                    evidence.Add(new {stage="silent-hill-mission-wait",text,frame,hud});
                    if(hud)break;
                    await Delay(3000);
                }
                if(!hud)throw new IOException("Silent Hill did not show a reviewed player HUD; weapon replay refused.");
                foreach(var row in actions.Where(r=>Regex.IsMatch(r.Function,"start",RegexOptions.IgnoreCase)))await Action(session,controllers,players,row);
                await Delay(2000);
                evidence.Add(new {stage="silent-hill-before-aim",frame=Capture(session,"silent-hill-before-aim"),gameResponse="requires-visual-review"});
            }
            if(!routeProbe&&Path.GetFileNameWithoutExtension(game.SourcePath)=="AliensArmageddon")
            {
                string menu=Capture(session,"armageddon-mode-menu");string text=await SceneText(Path.Combine(output,menu));
                evidence.Add(new {stage="armageddon-mode-menu",frame=menu,text});
                if(!Regex.IsMatch(text,@"CHOOSE.*GAME.*MODE",RegexOptions.IgnoreCase)||!Regex.IsMatch(text,@"CHAPTER\s*MODE",RegexOptions.IgnoreCase)||!Regex.IsMatch(text,@"FROM\s*THE\s*BEGINNING",RegexOptions.IgnoreCase))
                    throw new IOException("Armageddon did not reach the reviewed Story/Chapter choice; weapon replay refused.");
                await Action(session,controllers,players,actions.First(r=>r.SourceKey=="P1Button1"),(int)(32767*.28),(int)(32767*.50));
                await Delay(5000);
                evidence.Add(new {stage="armageddon-after-story",frame=Capture(session,"armageddon-after-story")});
                await Delay(30000);
                foreach(var row in actions.Where(r=>r.SourceKey is "P1Button1" or "P2Button1"))await Action(session,controllers,players,row);
                await Delay(3000);
                evidence.Add(new {stage="armageddon-after-intro",frame=Capture(session,"armageddon-after-intro"),gameResponse="requires-visual-review"});
            }
            if(!routeProbe&&XDocument.Load(game.SourcePath).Root?.Element("EmulationProfile")?.Value=="Hotd4sp")
            {
                // The first Start resets the cabinet ride and begins a fresh
                // boot/attract sequence. It does not start a playable mission.
                evidence.Add(new {stage="hotd-special-boot",seconds=45,note="Wait for the observed post-reset boot sequence, then send the mapped cabinet Start again. Gameplay still requires frame review."});
                await Delay(45000);RequireGame(session);
                await Action(session,controllers,players,actions.First(r=>r.SourceKey=="P1ButtonStart"));
                await Delay(10000);RequireGame(session);
                evidence.Add(new {stage="hotd-special-after-start",frame=Capture(session,"hotd-special-after-start"),note="Review the resulting scene before accepting weapon actions."});
            }
            if(!routeProbe&&Path.GetFileNameWithoutExtension(game.SourcePath) is "HOTDSD" or "RaccoonRampage" or "TombRaider")
            {
                string profile=Path.GetFileNameWithoutExtension(game.SourcePath);
                bool scarlet=profile=="HOTDSD";
                int seconds=scarlet?65:profile=="RaccoonRampage"?45:55;
                evidence.Add(new {stage="menu-warmup",profile,seconds,note="Observed start/selection flow; subsequent frames still require visual review."});
                if(scarlet)
                {
                    // The opening animation can still consume an early Start.
                    foreach(var row in actions.Where(r=>Regex.IsMatch(r.Function,"start",RegexOptions.IgnoreCase)))
                    {await Action(session,controllers,players,row);await Delay(1500);}
                    await Delay(3000);
                }
                foreach(var row in actions.Where(r=>Regex.IsMatch(r.Function,"trigger",RegexOptions.IgnoreCase)&&!Regex.IsMatch(r.Function,"sub",RegexOptions.IgnoreCase)))
                    await Action(session,controllers,players,row);
                if(scarlet)
                {
                    await Delay(3000);
                    foreach(var row in actions.Where(r=>Regex.IsMatch(r.Function,"gun trigger",RegexOptions.IgnoreCase)))
                        await Action(session,controllers,players,row);
                }
                await Delay(seconds*1000);RequireGame(session);
                if(scarlet)
                {
                    // Dismiss the opening shooting prompt before measuring the
                    // later per-player actions in the active level.
                    foreach(var row in actions.Where(r=>Regex.IsMatch(r.Function,"gun trigger",RegexOptions.IgnoreCase)))
                        await Action(session,controllers,players,row);
                    await Delay(3000);
                }
                evidence.Add(new {stage="after-menu-warmup",frame=Capture(session,"after-menu-warmup"),note="Scene requires visual review."});
            }
            if(!routeProbe&&Path.GetFileNameWithoutExtension(game.SourcePath) is "AliensExtermination" or "JurassicPark" or "Terminator" or "WalkingDead")
            {
                // These reviewed titles have a 30-second mission vote. Let it
                // finish before treating weapon actions as in-level observations.
                evidence.Add(new {stage="mission-warmup",seconds=35,note="Allow the reviewed mission-selection timer to finish, then dismiss the briefing with each player's mapped trigger."});
                await Delay(35000);RequireGame(session);
                foreach(var row in actions.Where(r=>Regex.IsMatch(r.Function,"trigger",RegexOptions.IgnoreCase)))
                    await Action(session,controllers,players,row);
                await Delay(3000);
                evidence.Add(new {stage="after-mission-warmup",frame=Capture(session,"after-mission-warmup"),note="Scene still requires visual review; elapsed time alone does not certify gameplay."});
                if(Path.GetFileNameWithoutExtension(game.SourcePath)=="Terminator")
                {
                    // The reviewed intro explicitly says HIT RELOAD TO SKIP.
                    // Its small dark caption is not reliably recognized by OCR.
                    evidence.Add(new {stage="terminator-intro-skip",note="Use the mapped P1 Reload for the reviewed cinematic; this is navigation, not weapon-effect verification."});
                    await Action(session,controllers,players,actions.First(r=>r.SourceKey=="P1Button3"));
                    await Delay(15000);RequireGame(session);
                    evidence.Add(new {stage="terminator-after-intro",frame=Capture(session,"terminator-after-intro"),note="Post-briefing scene still requires visual review."});
                }
            }
            for(int p=1;p<=2;p++)
            {
                var pipePoints=new List<PipeSample>();
                bool silent=Path.GetFileNameWithoutExtension(game.SourcePath)=="SilentHill";
                if(silent)foreach(var start in actions.Where(r=>r.Player==p&&Regex.IsMatch(r.Function,"start",RegexOptions.IgnoreCase)))await Action(session,controllers,players,start);
                var points=silent?new[]{(3277,3277),(29490,3277),(29490,29490),(3277,29490),(16384,16384)}
                    :new[]{(0,0),(16384,0),(32767,0),(32767,16384),(32767,32767),(16384,32767),(0,32767),(0,16384),(16384,16384)};
                foreach(var point in points)
                {
                    RequireGame(session);Drain();controllers[p-1].SubmitMouse(new HMMouseState {X=point.Item1,Y=point.Item2});await Delay(250);
                    var observed=Drain();int x=(int)Math.Round(point.Item1*65535d/32767),y=(int)Math.Round(point.Item2*65535d/32767);
                    bool received=observed.Any(r=>r.DeviceId==players[p-1].MouseId&&r.Absolute&&Math.Abs(r.X-x)<=128&&Math.Abs(r.Y-y)<=128);
                    var pipeSample=pipe?.Sample();if(pipeSample is not null)pipePoints.Add(pipeSample);
                    evidence.Add(new {stage="aim",emulatorInput=pipeSample,player=p,expectedX=x,expectedY=y,rawInputVerified=received,gameState=gameObservation?.Sample(),frame=Capture(session,$"p{p}-aim-{point.Item1}-{point.Item2}"),gameResponse="requires-visual-review"});
                    if(!received)throw new IOException($"P{p} absolute aim did not reach the RawInput consumer correctly.");
                    if(silent)await Action(session,controllers,players,actions.First(r=>r.Player==p&&r.SourceKey==$"P{p}Button1"),point.Item1,point.Item2);
                }
                if(pipe is not null)
                {
                    var xs=pipePoints.Select(s=>p==1?s.P1X:s.P2X).ToArray();var ys=pipePoints.Select(s=>p==1?s.P1Y:s.P2Y).ToArray();
                    bool span=xs.Min()<=pipe.MinX+1&&xs.Max()>=pipe.MaxX-1&&ys.Min()<=pipe.MinY+1&&ys.Max()>=pipe.MaxY-1;
                    evidence.Add(new {stage="emulator-aim-span",player=p,minX=xs.Min(),maxX=xs.Max(),minY=ys.Min(),maxY=ys.Max(),expected=new {pipe.MinX,pipe.MaxX,pipe.MinY,pipe.MaxY},verified=span});
                    if(!span)throw new IOException($"P{p} emulator aiming range is truncated.");
                }
                await Sweep(session,controllers[p-1],players[p-1]);
                if(silent)await Action(session,controllers,players,actions.First(r=>r.Player==p&&Regex.IsMatch(r.Function,"reload",RegexOptions.IgnoreCase)));
            }
            foreach(var row in actions.Where(r=>!Regex.IsMatch(r.Function,"coin|credits?|start",RegexOptions.IgnoreCase)))await Action(session,controllers,players,row);
        }
        finally
        {
            neutralStop.Cancel();
            try{await neutral;}catch(Exception e){evidence.Add(new {stage="neutral-stream-end",error=e.Message});}
            foreach(var c in controllers) {try{c.SubmitKeyboard(HMKeyModifiers.None,ReadOnlySpan<HMKey>.Empty);c.SubmitMouse(new HMMouseState{X=16384,Y=16384});}catch{}}
            try{await session.End();}catch(Exception e){evidence.Add(new {stage="session-stop",error=e.Message});}
            if(running is not null)try{await running.WaitAsync(TimeSpan.FromSeconds(15));}catch(Exception e){evidence.Add(new {stage="session-end",error=e.Message});}
            gameObservation?.Dispose();gameObservation=null;pipe?.Dispose();pipe=null;
            bool restoreSafe=preparedHash is not null&&File.Exists(game.SourcePath)&&Hash(game.SourcePath)==preparedHash;
            if(preparedHash is null&&File.Exists(game.SourcePath)&&Hash(game.SourcePath)==Hash(backup))restoreSafe=true;
            if(restoreSafe){string tmp=game.SourcePath+".input-lab-restore";File.Copy(backup,tmp,true);File.Move(tmp,game.SourcePath,true);}
            evidence.Add(new {stage="restore",restored=restoreSafe,profileUnchanged=Hash(game.SourcePath)==Hash(backup),libraryUnchanged=Hash(libraryFile)==libraryHash,session.CleanupIssues});
            if(Hash(game.SourcePath)!=Hash(backup))throw new IOException("Profile restoration needs review. The original and restore manifest are saved in the output directory.");
            if(Hash(libraryFile)!=libraryHash)throw new IOException("The library changed during the test; its newer contents were retained.");
        }
    }

    private static HMProfile Profile(int player)=>new HMProfileBuilder().Id($"deadeye-lab-p{player}").Name($"Deadeye Input Lab P{player}")
        .Vid(0xF0DE).Pid((ushort)(0xA000+player)).ProductString($"Deadeye Input Lab P{player}").ManufacturerString("Deadeye Arcade test device")
        .Type("mouse").Connection("USB").FromDescriptorBuilder(new HidDescriptorBuilder().Mouse(absolute:true,wheel:false,pan:false,reportId:1).Keyboard(reportId:2)).Build();
    private static bool HasTestIdentity(string path,int player)
    {
        using var device=CreateFile(path,0,3,0,3,0,0);if(device.IsInvalid)return false;
        var a=new HidAttributes{Size=Marshal.SizeOf<HidAttributes>()};
        return HidD_GetAttributes(device,ref a)&&a.Vendor==0xF0DE&&a.Product==0xA000+player;
    }
    private static bool Administrative(ControlRow row)=>Regex.IsMatch(row.SourceKey??"","^(Test|Service)",RegexOptions.IgnoreCase)||Regex.IsMatch(row.Function,@"volume|service|test\s*menu|test\s*mode|exit|quit",RegexOptions.IgnoreCase);
    private static async Task WaitForPlayableScene(GameSession session,GameEntry game)
    {
        var started=Stopwatch.StartNew();
        while(started.Elapsed<TimeSpan.FromSeconds(150))
        {
            // Some launchers replace their console/splash with the renderer.
            // Wait through that startup transition; replay still requires focus.
            if(!session.GameWindowAvailable||!session.HasGameForeground())
            {await Delay(500);continue;}
            string frame;
            try {frame=Capture(session,"startup-scene");}
            catch(IOException) when(!session.GameWindowAvailable||!session.HasGameForeground())
            {await Delay(500);continue;}
            catch(Exception error) when(error.HResult==unchecked((int)0x80070005)
                ||error is IOException&&error.Message=="DXGI supplied no updated desktop image after four frames.")
            {
                var desktop=InputDesktopDiagnostics.Read();
                if(desktop.inputDesktopError!=0||desktop.inputDesktop!="Default"||desktop.threadDesktop!="Default")throw;
                evidence.Add(new {stage="startup-capture-wait",seconds=started.Elapsed.TotalSeconds,desktop,error=error.Message});
                await Delay(1000);continue;
            }
            string text=await SceneText(Path.Combine(output,frame));
            var (ready,loading)=SceneSignals(text,gameObservation?.Sample().GameStatus,Path.GetFileNameWithoutExtension(game.SourcePath)=="SilentHill");
            evidence.Add(new {stage="scene-readiness",seconds=started.Elapsed.TotalSeconds,frame,text,loading,ready});
            Console.WriteLine($"Scene at {started.Elapsed.TotalSeconds:F0}s: "+text.Replace('\r',' ').Replace('\n',' '));
            if(ready)return;
            await Delay(5000);
        }
        throw new IOException("The renderer never reached a recognized game/menu scene. No button or aiming replay was accepted as gameplay verification.");
    }
    private static (bool Ready,bool Loading) SceneSignals(string text,byte? gameStatus=null,bool requireStart=false)
    {
        bool loading=Regex.IsMatch(text,"LOADING|NO.*GVR[I1O0]|NO.*ALIENS.*GUNS",RegexOptions.IgnoreCase);
        // The arcade font's A is read as R in actual Windows OCR captures.
        bool menu=Regex.IsMatch(text,"[PD]RESS.*ST[ARO4]RT|INSERT.*(?:COIN|CREDITS?)|FREE\\s*PL[AR4]Y|SELECT.*MISSION|MISSION.*SELECT|SELECT.*LEVEL",RegexOptions.IgnoreCase);
        bool start=Regex.IsMatch(text,"[PD]RESS.*ST[ARO4]RT",RegexOptions.IgnoreCase);
        return (!loading&&(requireStart?start:menu||gameStatus is 4 or 64),loading);
    }
    private static async Task<string> SceneText(string file)
    {
        var engine=global::Windows.Media.Ocr.OcrEngine.TryCreateFromUserProfileLanguages();
        if(engine is null)throw new IOException("No Windows OCR language is installed; automatic scene readiness cannot be established.");
        var image=await global::Windows.Storage.StorageFile.GetFileFromPathAsync(file);
        using var stream=await image.OpenAsync(global::Windows.Storage.FileAccessMode.Read);
        var decoder=await global::Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
        using var bitmap=await decoder.GetSoftwareBitmapAsync(global::Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,global::Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied);
        return (await engine.RecognizeAsync(bitmap)).Text;
    }
    private static void RequireGame(GameSession session)
    {
        cancel.Token.ThrowIfCancellationRequested();
        if(!session.Active||!session.GameWindowAvailable||!session.HasGameForeground())throw new IOException("Game lost foreground. Input replay stopped before sending another action.");
    }
    private static RawPacket[] Drain(){var list=new List<RawPacket>();while(packets.TryDequeue(out var p))list.Add(p);return list.ToArray();}
    private static async Task Action(GameSession session,HMController[] devices,List<GunBinding> players,ControlRow row,int x=16384,int y=16384)
    {
        RequireGame(session);var match=Regex.Match(row.Expression??"",@"^P([12])\|(mouse|key):(\d+)$");
        if(!match.Success){evidence.Add(new {stage="action",row.Player,row.Function,row.Expression,status="not-replayable"});return;}
        int p=int.Parse(match.Groups[1].Value),code=int.Parse(match.Groups[3].Value);bool mouse=match.Groups[2].Value=="mouse";
        if(!mouse&&code is 27 or 91 or 92 or 0x73 or 0x79 or 0x7b){evidence.Add(new{stage="action",row.Player,row.Function,status="reserved-system-or-exit-key"});return;}
        if(x!=16384||y!=16384){devices[p-1].SubmitMouse(new HMMouseState{X=x,Y=y});await Delay(300);}
        string before=Capture(session,$"p{p}-{row.SourceKey}-before");var gameBefore=gameObservation?.Sample();var pipeBefore=pipe?.Sample();AliensSample? gameHeld=null;PipeSample? pipeHeld=null;string? heldFrame=null;Drain();
        try{Send(devices[p-1],mouse,code,true,x,y);await Delay(requestedHoldMilliseconds??150);gameHeld=gameObservation?.Sample();pipeHeld=pipe?.Sample();heldFrame=Capture(session,$"p{p}-{row.SourceKey}-held");}
        finally{Send(devices[p-1],mouse,code,false,x,y);}
        await Delay(300);var received=Drain();string id=mouse?players[p-1].MouseId:players[p-1].KeyboardId!;
        bool down=received.Any(r=>r.DeviceId==id&&(mouse?(r.Buttons&(1<<((code-1)*2)))!=0:r.Key==code&&r.Down));
        bool up=received.Any(r=>r.DeviceId==id&&(mouse?(r.Buttons&(2<<((code-1)*2)))!=0:r.Key==code&&!r.Down));
        var other=players.Single(b=>b.Player!=p);string otherId=mouse?other.MouseId:other.KeyboardId!;
        bool isolated=!received.Any(r=>r.DeviceId==otherId&&(mouse?(r.Buttons&(3<<((code-1)*2)))!=0:r.Key==code));
        var pipeReleased=pipe?.Sample();uint? expectedMask=pipe is null?null:TeknoPipeObservation.Mask(row.SourceKey);
        bool? routed=pipe is not null&&expectedMask is {} mask?pipeBefore?.Buttons==0&&pipeHeld?.Buttons==mask&&pipeReleased?.Buttons==0:null;
        evidence.Add(new {stage="action",emulatorInputVerified=routed,pipeBefore,pipeHeld,pipeReleased,expectedMask,row.Player,row.Function,row.Expression,rawPressed=down,rawReleased=up,isolated,
            gameBefore,gameHeld,heldFrame,gameReleased=gameObservation?.Sample(),before,after=Capture(session,$"p{p}-{row.SourceKey}-after"),gameResponse="requires-visual-review"});
        if(routed==false)throw new IOException($"P{p} {row.Function}: emulator input state did not match its press and release.");
        if(!down||!up||!isolated)throw new IOException($"P{p} {row.Function}: incomplete or cross-player RawInput press/release pair.");
    }
    private static async Task Sweep(GameSession session,HMController device,GunBinding player)
    {
        RequireGame(session);Drain();timed.Clear();var sent=new List<(int X,long Time)>();
        for(int i=0;i<=120;i++)
        {
            RequireGame(session);int x=(int)Math.Round(i*32767d/120);
            sent.Add(((int)Math.Round(x*65535d/32767),Stopwatch.GetTimestamp()));
            device.SubmitMouse(new HMMouseState{X=x,Y=16384});await Delay(8);
        }
        await Delay(100);
        var received=timed.Where(r=>r.Packet.DeviceId==player.MouseId&&r.Packet.Absolute).ToArray();
        var latency=new List<double>();
        foreach(var s in sent)
        {
            var r=received.FirstOrDefault(r=>r.Time>=s.Time&&Math.Abs(r.Packet.X-s.X)<=64);
            if(r.Time!=0)latency.Add((r.Time-s.Time)*1000d/Stopwatch.Frequency);
        }
        var values=received.Select(r=>r.Packet.X).ToArray();
        bool monotonic=values.Zip(values.Skip(1)).All(pair=>pair.First<=pair.Second);
        latency.Sort();
        evidence.Add(new {stage="sweep",player=player.Player,submitted=sent.Count,received=received.Length,
            matched=latency.Count,monotonic,minX=values.DefaultIfEmpty(-1).Min(),maxX=values.DefaultIfEmpty(-1).Max(),
            p50DeliveryMilliseconds=latency.Count>0?(double?)latency[latency.Count/2]:null,
            p95DeliveryMilliseconds=latency.Count>0?(double?)latency[(int)((latency.Count-1)*.95)]:null,
            frame=Capture(session,$"p{player.Player}-sweep-end"),
            note="Latency is virtual HID submission to this RawInput receiver. Camera tracking and rendered frame latency are not measured."});
        if(!monotonic||latency.Count<sent.Count*.95)throw new IOException($"P{player.Player} sweep lost or reordered input reports.");
    }
    private static void Send(HMController device,bool mouse,int code,bool down,int x=16384,int y=16384)
    {
        if(mouse){if(code is <1 or >5)throw new ArgumentOutOfRangeException(nameof(code));device.SubmitMouse(new HMMouseState{X=x,Y=y,Buttons=down?(HMMouseButtons)(1<<(code-1)):0});return;}
        int usage=code switch {>=65 and <=90=>code-65+4,>=49 and <=57=>code-49+0x1e,48=>0x27,13=>0x28,27=>0x29,8=>0x2a,9=>0x2b,32=>0x2c,37=>0x50,38=>0x52,39=>0x4f,40=>0x51,>=112 and <=123=>code-112+0x3a,_=>throw new NotSupportedException($"Virtual key {code} needs a reviewed HID translation.")};
        device.SubmitKeyboard(HMKeyModifiers.None,down?new[]{(HMKey)usage}:ReadOnlySpan<HMKey>.Empty);
    }
    private static string Capture(GameSession session,string name)
    {
        RequireGame(session);if(!GetClientRect(session.GameWindow,out var rect))throw new IOException("Cannot inspect game client rectangle.");
        var origin=new PointNative();if(!ClientToScreen(session.GameWindow,ref origin)||rect.Right<1||rect.Bottom<1)throw new IOException("Invalid renderer dimensions.");
        nint monitor=MonitorFromWindow(session.GameWindow,2);var info=new MonitorInfo{Size=Marshal.SizeOf<MonitorInfo>()};
        if(!GetMonitorInfo(monitor,ref info))throw new IOException("Cannot inspect the game monitor.");
        using var full=CaptureOutput(monitor,name);
        var region=new Rectangle(origin.X-info.Monitor.Left,origin.Y-info.Monitor.Top,rect.Right,rect.Bottom);
        if(region.Left<0||region.Top<0||region.Right>full.Width||region.Bottom>full.Height)throw new IOException("The game client extends outside its captured GPU output.");
        int width=name=="startup-scene"?1600:960;
        using var small=new Bitmap(width,Math.Max(1,(int)(region.Height*width/(double)region.Width)));
        using(var g=Graphics.FromImage(small))g.DrawImage(full,new Rectangle(0,0,small.Width,small.Height),region,GraphicsUnit.Pixel);
        string file=$"{DateTime.UtcNow:HHmmssfff}-{Regex.Replace(name,@"[^\w-]","_")}.jpg";small.Save(Path.Combine(output,file),ImageFormat.Jpeg);return file;
    }
    private static Bitmap CaptureOutput(nint monitor,string name)
    {
        for(int attempt=0;;attempt++)
        {
            try {return GpuCapture.Frame(monitor);}
            catch(Exception error)
            {
                var desktop=InputDesktopDiagnostics.Read();
                // Microsoft's desktop-duplication sample treats access denial
                // as a possible display transition. Retry briefly on Default
                // only; never switch desktop or elevate capture privileges.
                bool retry=error.HResult==unchecked((int)0x80070005)&&attempt<3
                    &&desktop.inputDesktopError==0&&desktop.inputDesktop=="Default"&&desktop.threadDesktop=="Default";
                evidence.Add(new {stage=retry?"capture-retry":"capture-failure",name,attempt,desktop,error=error.Message});
                if(!retry)throw;
                cancel.Token.WaitHandle.WaitOne(1000);cancel.Token.ThrowIfCancellationRequested();
            }
        }
    }
    [StructLayout(LayoutKind.Sequential)]private struct MonitorInfo{public int Size;public Rect Monitor,Work;public uint Flags;}
    [DllImport("user32.dll")]private static extern nint MonitorFromWindow(nint window,uint flags);
    [DllImport("user32.dll")]private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")]private static extern uint GetWindowThreadProcessId(nint window,out uint process);
    [DllImport("user32.dll")]private static extern bool GetMonitorInfo(nint monitor,ref MonitorInfo info);
    [StructLayout(LayoutKind.Sequential)]private struct Rect{public int Left,Top,Right,Bottom;}
    [StructLayout(LayoutKind.Sequential)]private struct PointNative{public int X,Y;}
    [StructLayout(LayoutKind.Sequential)]private struct HidAttributes{public int Size;public ushort Vendor,Product,Version;}
    [DllImport("user32.dll")]private static extern bool GetClientRect(nint window,out Rect rect);
    [DllImport("user32.dll")]private static extern bool ClientToScreen(nint window,ref PointNative point);
    [DllImport("user32.dll")]private static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll")]private static extern int GetSystemMetrics(int index);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]private static extern SafeFileHandle CreateFile(string path,uint access,uint share,nint security,uint creation,uint flags,nint template);
    [DllImport("hid.dll")]private static extern bool HidD_GetAttributes(SafeFileHandle device,ref HidAttributes attributes);
}
