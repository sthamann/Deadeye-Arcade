using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Reaper.Core;

public record MameInputMetadata(string Rom, string MachineXml, string ConfigDirectory, long BinaryLength, long BinaryModified);
public static class MameControls
{
    public static int Players(MameInputMetadata metadata) => (int?)XElement.Parse(metadata.MachineXml).Element("input")?.Attribute("players")??0;
    public static string? Rom(GameEntry game) => game.Source=="mame" && game.Arguments.FirstOrDefault() is string rom && Regex.IsMatch(rom,"^[a-zA-Z0-9_]+$") ? rom : null;
    public static string MetadataPath(GameEntry game,string data) => Path.Combine(data,"input-metadata","mame",Regex.Replace(game.Id,"[^a-zA-Z0-9_-]","_")+".json");
    public static MameInputMetadata? Metadata(GameEntry game,string data)
    {
        string path=MetadataPath(game,data);if(!File.Exists(path)||!File.Exists(game.Executable))return null;
        try {
            var value=JsonSerializer.Deserialize<MameInputMetadata>(File.ReadAllText(path),JsonDefaults.Options);
            var binary=new FileInfo(game.Executable);
            return value is not null && value.Rom==Rom(game) && value.BinaryLength==binary.Length && value.BinaryModified==binary.LastWriteTimeUtc.Ticks ? value : null;
        } catch(JsonException) {return null;}
    }
    public static string[] RequiredPorts(MameInputMetadata metadata,int player)
    {
        var input=XElement.Parse(metadata.MachineXml).Element("input");
        if(input is null || player>Players(metadata))return [];
        // Alien3 replaces both generic Start bits with service/unused inputs.
        // Its original cabinet starts with the trigger after inserting credits.
        var result=new List<string>();
        if(metadata.Rom!="alien3")result.Add("START"+player);
        if(player<=((int?)input.Attribute("coins")??0))result.Add("COIN"+player);
        foreach(var control in input.Elements("control").Where(c=>((int?)c.Attribute("player")??1)==player))
        {
            int buttons=(int?)control.Attribute("buttons")??0;
            for(int n=1;n<=buttons;n++)result.Add($"P{player}_BUTTON{n}");
            string? axis=(string?)control.Attribute("type") switch {"lightgun"=>"LIGHTGUN","stick"=>"AD_STICK",_=>null};
            if(axis is not null){result.Add($"P{player}_{axis}_X");result.Add($"P{player}_{axis}_Y");}
        }
        return result.Distinct().ToArray();
    }
    public static Dictionary<string,string> Effective(GameEntry game,string data)
    {
        var values=new Dictionary<string,string>();
        string controller=Path.Combine(data,"controllers","reaper.cfg");
        var metadata=Metadata(game,data);
        IEnumerable<string> files=[controller];
        if(metadata is not null)files=files.Concat(new[]{Path.Combine(metadata.ConfigDirectory,"default.cfg"),Path.Combine(metadata.ConfigDirectory,metadata.Rom+".cfg")});
        foreach(string file in files.Where(File.Exists))
            foreach(var port in XDocument.Load(file).Descendants("port"))
                if((string?)port.Attribute("type") is string type && port.Elements("newseq").FirstOrDefault(s=>(string?)s.Attribute("type")=="standard") is {} seq)values[type]=seq.Value.Trim();
        return values;
    }
    public static int Configure(GameEntry game,string data,IEnumerable<GunBinding> bindings)
    {
        var guns=bindings.Where(b=>b.Player is 1 or 2).ToArray();
        string controller=Path.Combine(data,"controllers","reaper.cfg");Directory.CreateDirectory(Path.GetDirectoryName(controller)!);
        var controllerDocument=XDocument.Parse(LaunchRules.MameController(guns));
        if(Rom(game)=="alien3")
            foreach(var gun in guns)
            {
                XElement? Sequence(string type)=>controllerDocument.Descendants("port").SingleOrDefault(p=>(string?)p.Attribute("type")==type)?.Element("newseq");
                var trigger=Sequence($"P{gun.Player}_BUTTON1");var start=Sequence("START"+gun.Player);
                if(trigger is not null && start is not null)
                    trigger.Value=string.Join(" OR ",new[]{trigger.Value,start.Value}.SelectMany(v=>v.Split(" OR ")).Distinct());
            }
        string text=controllerDocument.ToString();if(!File.Exists(controller)||File.ReadAllText(controller)!=text)File.WriteAllText(controller,text);
        var metadata=Metadata(game,data);if(metadata is null)return 0;
        var required=guns.SelectMany(g=>RequiredPorts(metadata,g.Player)).ToHashSet();
        var expected=XDocument.Parse(text).Descendants("port").Where(p=>required.Contains((string?)p.Attribute("type")??""))
            .ToDictionary(p=>(string)p.Attribute("type")!,p=>p.Element("newseq")!.Value);
        int changes=0;
        foreach(string file in new[]{Path.Combine(metadata.ConfigDirectory,"default.cfg"),Path.Combine(metadata.ConfigDirectory,metadata.Rom+".cfg")}.Where(File.Exists))
        {
            var doc=XDocument.Load(file);bool changed=false;
            foreach(var port in doc.Descendants("port"))
            {
                string type=(string?)port.Attribute("type")??"";if(!expected.TryGetValue(type,out string? route))continue;
                var sequence=port.Elements("newseq").FirstOrDefault(s=>(string?)s.Attribute("type")=="standard");
                if(sequence?.Value==route)continue;
                if(sequence is null)port.Add(new XElement("newseq",new XAttribute("type","standard"),route));else sequence.Value=route;
                changed=true;changes++;
            }
            if(changed){if(!File.Exists(file+".before-deadeye-input"))File.Copy(file,file+".before-deadeye-input");doc.Save(file+".new");File.Move(file+".new",file,true);}
        }
        return changes;
    }
    public static GameControls? Read(GameEntry game,string data)
    {
        var metadata=Metadata(game,data);if(metadata is null)return null;
        var values=Effective(game,data);
        var rows=new List<ControlRow>();
        for(int player=1;player<=2;player++)foreach(string type in RequiredPorts(metadata,player))
        {
            string expression=values.GetValueOrDefault(type,"");
            bool aim=type.Contains("_LIGHTGUN_")||type.Contains("_AD_STICK_");
            string label=aim?"Light Gun "+type[^1]:type.StartsWith("START")?"Start":type.StartsWith("COIN")?I18n.T("Münze"):type.Replace($"P{player}_BUTTON",I18n.T("Taste "));
            // Verified against both cabinets' live trigger/bomb responses. Keep
            // unknown games generic rather than guessing their secondary action.
            if(metadata.Rom is "alien3" or "term2")
            {
                if(type==$"P{player}_BUTTON1")label=I18n.T(metadata.Rom=="alien3"?"Schießen / Start":"Schießen");
                if(type==$"P{player}_BUTTON2")label=I18n.T("Bombe");
            }
            rows.Add(new(player,label,string.IsNullOrWhiteSpace(expression)||expression=="NONE"?I18n.T("Nicht belegt"):expression,"MAME -listxml + effective cfg",expression,aim,type));
        }
        return new(rows.ToArray(),I18n.T("Benötigte Eingänge aus dem installierten MAME gelesen. Globale und individuelle Spielbelegungen sind berücksichtigt."));
    }
}
