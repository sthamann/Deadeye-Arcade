using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Reaper.Core;

public static class Hod3Presentation
{
    public const string AffectedExecutableMd5="20a0af3884208f2c55391810593eb1b7";
    public static string ConfigPath(GameEntry game)=>Path.Combine(game.WorkingDirectory,"Hod3Arcade.ini");
    public static string WindowedConfig(string text,string md5)=>md5.Equals(AffectedExecutableMd5,StringComparison.OrdinalIgnoreCase)
        ? Regex.Replace(text,@"(?m)^Fullscreen:1(?=\r?$)","Fullscreen:0") : text;
    public static bool Supports(GameEntry game)
    {
        if(!Path.GetFileName(game.Executable).Equals("Hotd3Arcade_Launcher.exe",StringComparison.OrdinalIgnoreCase)
            ||!File.Exists(ConfigPath(game)))return false;
        string text=File.ReadAllText(ConfigPath(game));
        if(!Regex.IsMatch(text,@"(?m)^Fullscreen:[01]\r?$"))return false;
        var target=Regex.Match(text,"(?m)^FilePath:\"([^\"\\r\\n]+)\"\\r?$");
        if(!target.Success || !File.Exists(target.Groups[1].Value))return false;
        using var stream=File.OpenRead(target.Groups[1].Value);
        return Convert.ToHexString(MD5.HashData(stream)).Equals(AffectedExecutableMd5,StringComparison.OrdinalIgnoreCase);
    }
    public static bool Configure(GameEntry game)
    {
        if(!Supports(game))return false;
        string path=ConfigPath(game),text=File.ReadAllText(path),prepared=WindowedConfig(text,AffectedExecutableMd5);
        if(prepared==text)return false;
        string backup=path+".before-deadeye-presentation";
        if(!File.Exists(backup))File.Copy(path,backup);
        File.WriteAllText(path+".deadeye-new",prepared);
        File.Move(path+".deadeye-new",path,true);
        return true;
    }
}
