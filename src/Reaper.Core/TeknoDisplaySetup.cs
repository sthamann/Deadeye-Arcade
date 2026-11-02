using System.Xml.Linq;
namespace Reaper.Core;

public static class TeknoDisplaySetup
{
    public static bool Supports(GameEntry game)
    {
        if(game.Source!="teknoparrot")return false;
        string id=Path.GetFileNameWithoutExtension(game.SourcePath);
        if(id is "AliensExtermination" or "AliensArmageddon")return true;
        if(id!="SilentHill"||!File.Exists(game.SourcePath))return false;
        var document=XDocument.Load(game.SourcePath);
        return document.Root?.Element("EmulationProfile")?.Value=="SilentHill"
            &&document.Descendants("FieldInformation").Count(f=>f.Element("CategoryName")?.Value=="General"&&f.Element("FieldName")?.Value=="Windowed"&&f.Element("FieldValue") is not null)==1;
    }
    // This repair depends only on the reviewed profile, not the current RDP/TV mode.
    public static bool ConfigureSilentHill(GameEntry game)
    {
        if(game.Source!="teknoparrot" || Path.GetFileNameWithoutExtension(game.SourcePath)!="SilentHill" || !File.Exists(game.SourcePath))return false;
        var document=XDocument.Load(game.SourcePath);
        if(document.Root?.Element("EmulationProfile")?.Value!="SilentHill")return false;
        var fields=document.Descendants("FieldInformation").Where(f=>f.Element("CategoryName")?.Value=="General"&&f.Element("FieldName")?.Value=="Windowed").ToArray();
        if(fields.Length>1)throw new InvalidDataException(I18n.T("Das Silent-Hill-Profil enthält mehrere Windowed-Felder. Die Anzeigeeinstellung muss geprüft werden; das Profil bleibt erhalten."));
        var value=fields.SingleOrDefault()?.Element("FieldValue");
        if(value is null || value.Value=="1")return false;
        value.Value="1";
        Save(game,document);
        return true;
    }
    public static bool Configure(GameEntry game,int width,int height)
    {
        if(game.Source=="teknoparrot"&&Path.GetFileNameWithoutExtension(game.SourcePath)=="SilentHill")return ConfigureSilentHill(game);
        if(!Supports(game) || width<640 || height<480) return false;
        var document=XDocument.Load(game.SourcePath); bool changed=false;
        void Set(string name,string value)
        {
            // Use existing vendor fields only, never invent unsupported renderer options.
            var field=document.Descendants("FieldInformation").SingleOrDefault(f=>f.Element("CategoryName")?.Value=="General" && f.Element("FieldName")?.Value==name)?.Element("FieldValue");
            if(field is null || field.Value==value) return;
            field.Value=value; changed=true;
        }
        string? Value(string name)=>document.Descendants("FieldInformation")
            .FirstOrDefault(f=>f.Element("CategoryName")?.Value=="General" && f.Element("FieldName")?.Value==name)?.Element("FieldValue")?.Value;
        // The reviewed Aliens windowed route mixes WPF DIPs with pixels at
        // increased scaling. Its fullscreen absolute route avoids truncation.
        bool absoluteRawInput=Value("Input API")=="RawInput" && Value("Use Relative Input")!="1";
        Set("Windowed",absoluteRawInput?"0":"1");Set("CustomResolution","1");
        Set("ResolutionWidth",width.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Set("ResolutionHeight",height.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if(!changed) return false;
        Save(game,document);
        return true;
    }
    private static void Save(GameEntry game,XDocument document)
    {
        string backup=game.SourcePath+".before-deadeye-display-fix";
        if(!File.Exists(backup)) File.Copy(game.SourcePath,backup);
        string temporary=game.SourcePath+".reaper-new";
        try {document.Save(temporary);File.Move(temporary,game.SourcePath,true);}
        finally {if(File.Exists(temporary))File.Delete(temporary);}
    }
}
