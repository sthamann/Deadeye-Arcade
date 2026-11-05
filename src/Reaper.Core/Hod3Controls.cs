using System.Text.RegularExpressions;
namespace Reaper.Core;

public static class Hod3Controls
{
    public static bool Supports(GameEntry game) =>
        Path.GetFileName(game.Executable).Equals("Hotd3Arcade_Launcher.exe",StringComparison.OrdinalIgnoreCase)
        && File.Exists(Hod3Presentation.ConfigPath(game))
        && (game.Helpers??[]).Any(h=>MultiplayerSetup.IsDemulShooter(h.Executable)
            && h.Arguments.Contains("-target=windows") && h.Arguments.Contains("-rom=hod3pc"));

    public static void Configure(GameEntry game,IEnumerable<GunBinding> bindings)
    {
        if(!Supports(game))return;
        string file=Hod3Presentation.ConfigPath(game),old=File.ReadAllText(file),text=old;
        void Set(string key,int value)
        {
            string pattern=@"(?m)^"+Regex.Escape(key)+@":[^\r\n]*";
            text=Regex.IsMatch(text,pattern)?Regex.Replace(text,pattern,key+":"+value):text.TrimEnd()+"\n"+key+":"+value+"\n";
        }
        // The mod's source Configurator reads decimal DirectInput scan codes.
        // DemulShooter requires keyboard mode for both players, even for guns.
        Set("P1Controls",0);Set("P2Controls",0);
        foreach(var gun in bindings.Where(b=>b.Player is 1 or 2))
        {
            string? token=StartupControls.ActionToken(gun,"start");
            if(token?.StartsWith("key:")==true && int.TryParse(token[4..],out int key))
            {
                int scan=Rpcs3Setup.EncodeKey(key)>>16;
                if(scan is >0 and <128)Set("Keyboard"+gun.Player+"pStart",scan);
            }
        }
        if(text==old)return;
        if(!File.Exists(file+".before-deadeye-controls"))File.Copy(file,file+".before-deadeye-controls");
        File.WriteAllText(file+".new",text);File.Move(file+".new",file,true);
    }
    public static GameControls Read(GameEntry game,IEnumerable<GunBinding> bindings)
    {
        string text=File.ReadAllText(Hod3Presentation.ConfigPath(game));
        string? Value(string name)=>Regex.Match(text,@"(?m)^"+Regex.Escape(name)+@":([^\r\n]*)").Groups[1].Value.Trim();
        var helper=(game.Helpers??[]).Single(h=>MultiplayerSetup.IsDemulShooter(h.Executable) && h.Arguments.Contains("-rom=hod3pc"));
        string config=Path.Combine(helper.WorkingDirectory,"config.ini");
        string[] lines=File.Exists(config)?File.ReadAllLines(config):[];
        var rows=new List<ControlRow>();
        foreach(var gun in bindings.Where(b=>b.Player is 1 or 2))
        {
            bool assigned=Value("P"+gun.Player+"Controls")=="0" && lines.Any(l=>Regex.IsMatch(l,@"^\s*P"+gun.Player+@"Mode\s*=\s*RAWINPUT\s*$"))
                && lines.Any(l=>l.Split('=',2) is {Length:2} p && p[0].Trim()=="P"+gun.Player+"DeviceName" && p[1].Trim().Equals(gun.MouseId,StringComparison.OrdinalIgnoreCase));
            void Add(string function,string? token,bool global=false)
            {
                bool reachable=assigned && token is not null && (!token.StartsWith("key:") || !string.IsNullOrWhiteSpace(gun.KeyboardId));
                rows.Add(new(gun.Player,function,reachable?OverlayControls.InputName(token!):I18n.T("Nicht belegt"),
                    "HOTD III ArcadeMod + DemulShooter"+(global?" · global keyboard":""),reachable?$"P{gun.Player}|{token}":null));
            }
            Add(I18n.T("Abzug"),"mouse:1");Add(I18n.T("Nachladen"),"mouse:2");Add(I18n.T("Wegeauswahl"),"mouse:3");
            string? start=StartupControls.Hardware(gun).Select(h=>h.Token).OfType<string>().FirstOrDefault(t=>t.StartsWith("key:")
                && int.TryParse(Value("Keyboard"+gun.Player+"pStart"),out int scan) && (Rpcs3Setup.EncodeKey(int.Parse(t[4..]))>>16)==scan);
            Add("Start",start,true);
            if(gun.Player==1 && Value("Freeplay")!="1")Add(I18n.T("Münze"),"key:53",true);
        }
        return new(rows.ToArray(),I18n.T("Installierter HOTD-III-ArcadeMod: Tastaturmodus, Start-Tastencodes und DemulShooter-Gerätezuordnung ausgelesen. Zielen und Treffer müssen im Spiel geprüft werden."));
    }
}
