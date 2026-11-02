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
            &&document.Descendants("FieldInformation").Any(f=>f.Element("CategoryName")?.Value=="General"&&f.Element("FieldName")?.Value=="Windowed"&&f.Element("FieldValue") is not null);
    }
    public static bool Configure(GameEntry game,int width,int height)
    {
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
        if(Path.GetFileNameWithoutExtension(game.SourcePath)=="SilentHill")
        {
            // Native A/B checks: exclusive fullscreen ignores Start and loses
            // its window after the menu; windowed rendering survives Resume.
            // Keep the game's existing resolution and gun controls intact.
            Set("Windowed","1");
        }
        else
        {
            // The reviewed Aliens windowed route mixes WPF DIPs with pixels at
            // increased scaling. Its fullscreen absolute route avoids truncation.
            bool absoluteRawInput=Value("Input API")=="RawInput" && Value("Use Relative Input")!="1";
            Set("Windowed",absoluteRawInput?"0":"1");Set("CustomResolution","1");
            Set("ResolutionWidth",width.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Set("ResolutionHeight",height.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        if(!changed) return false;
        string backup=game.SourcePath+".before-deadeye-display-fix";
        if(!File.Exists(backup)) File.Copy(game.SourcePath,backup);
        string temporary=game.SourcePath+".reaper-new";document.Save(temporary);File.Move(temporary,game.SourcePath,true);
        return true;
    }
}
