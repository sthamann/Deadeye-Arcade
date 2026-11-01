using System.Xml.Linq;

namespace Reaper.Core;

public static class LindberghPresentation
{
    // EX keeps a fixed 1360 x 768 viewport even when its window is enlarged.
    // Other Lindbergh games have not been verified through this display route.
    public const int Width = 1360, Height = 768, Hz = 60;
    public static bool Supports(GameEntry game)
    {
        if(game.Source!="teknoparrot" || !File.Exists(game.SourcePath))return false;
        var root=XDocument.Load(game.SourcePath).Root;
        return root?.Element("EmulatorType")?.Value=="ElfLdr2"
            && root.Element("EmulationProfile")?.Value=="Hotd4ex"
            && root.Element("ExecutableName")?.Value.Equals("hodexRI.elf",StringComparison.OrdinalIgnoreCase)==true
            && root.Element("ConfigValues")?.Elements("FieldInformation").Any(f=>
                f.Element("CategoryName")?.Value=="General" && f.Element("FieldName")?.Value=="DisplayMode")==true;
    }
    public static bool Configure(GameEntry game)
    {
        if(!Supports(game))return false;
        var document=XDocument.Load(game.SourcePath);
        var value=document.Descendants("FieldInformation").Single(f=>
            f.Element("CategoryName")?.Value=="General" && f.Element("FieldName")?.Value=="DisplayMode").Element("FieldValue");
        if(value is null || value.Value=="Windowed")return false;
        value.Value="Windowed";
        string backup=game.SourcePath+".before-deadeye-native-display";
        if(!File.Exists(backup))File.Copy(game.SourcePath,backup);
        string temporary=game.SourcePath+".deadeye-new";
        try{document.Save(temporary);File.Move(temporary,game.SourcePath,true);}
        finally{if(File.Exists(temporary))File.Delete(temporary);}
        return true;
    }
}
