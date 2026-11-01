using System.Runtime.InteropServices;
using System.Text.Json;
using Reaper.Core;

namespace Reaper.Windows;

// Temporary console-session mode only. Never changes the saved Windows mode.
internal sealed class NativeDisplayScope : IDisposable
{
    [DllImport("user32.dll",EntryPoint="EnumDisplaySettingsW",CharSet=CharSet.Unicode)]
    private static extern bool Enum(string? device,int mode,nint data);
    [DllImport("user32.dll",EntryPoint="ChangeDisplaySettingsExW",CharSet=CharSet.Unicode)]
    private static extern int Change(string? device,nint mode,nint window,uint flags,nint parameters);
    private readonly string device,data;
    private nint original;
    private NativeDisplayScope(string device,string data,nint original)
    {this.device=device;this.data=data;this.original=original;}
    private static nint Buffer()
    {
        nint buffer=Marshal.AllocHGlobal(220);
        for(int i=0;i<220;i++)Marshal.WriteByte(buffer,i,0);
        Marshal.WriteInt16(buffer,68,220);
        return buffer;
    }
    public static NativeDisplayScope? Begin(GameEntry game,string data)
    {
        if(!LindberghPresentation.Supports(game))return null;
        var snapshot=DisplayDiagnostics.Capture(data);
        if(snapshot.Remote || snapshot.Outputs.Length!=1 || !snapshot.Outputs[0].Primary)return null;
        string device=snapshot.Outputs[0].Device;
        nint original=Buffer(),candidate=Buffer();
        try
        {
            if(!Enum(device,-1,original))return null;
            for(int i=0;i<1000;i++)
            {
                if(!Enum(device,i,candidate))break;
                if(Marshal.ReadInt32(candidate,172)!=LindberghPresentation.Width
                    || Marshal.ReadInt32(candidate,176)!=LindberghPresentation.Height
                    || Marshal.ReadInt32(candidate,184)!=LindberghPresentation.Hz
                    || Marshal.ReadInt32(candidate,168)!=Marshal.ReadInt32(original,168)
                    || (Marshal.ReadInt32(candidate,180)&2)!=0)continue;
                if(Change(device,candidate,0,2,0)!=0)continue; // CDS_TEST
                if(Change(device,candidate,0,4,0)!=0)return null; // CDS_FULLSCREEN, no registry update
                var scope=new NativeDisplayScope(device,data,original);original=0;
                try
                {
                    LindberghPresentation.Configure(game);
                    scope.Record("selected",0);
                    return scope;
                }
                catch{scope.Dispose();throw;}
            }
            return null;
        }
        finally{Marshal.FreeHGlobal(candidate);if(original!=0)Marshal.FreeHGlobal(original);}
    }
    public void Dispose()
    {
        if(original==0)return;
        try
        {
            // A new RDP connection owns its own display. Do not apply the
            // physical console mode to that surface; saved settings are unchanged.
            if(DisplayDiagnostics.Capture(data).Remote){Record("console-restore-deferred",0);return;}
            int result=Change(device,original,0,4,0);
            if(result!=0)result=Change(device,0,0,0,0);
            Record(result==0?"restored":"restore-failed",result);
            if(result!=0)throw new IOException(I18n.F($"Windows konnte den vorherigen Bildschirmmodus nicht wiederherstellen (Code {result})."));
            DisplayDiagnostics.Capture(data);
        }
        finally{Marshal.FreeHGlobal(original);original=0;}
    }
    private void Record(string stage,int result)
    {
        try
        {
            Directory.CreateDirectory(data);
            File.AppendAllText(Path.Combine(data,"native-display-history.jsonl"),JsonSerializer.Serialize(new{
                time=DateTimeOffset.Now,rule="hotdex-native-display",stage,result,device,
                width=LindberghPresentation.Width,height=LindberghPresentation.Height,hz=LindberghPresentation.Hz
            })+Environment.NewLine);
        }
        catch(Exception error) when(error is IOException or UnauthorizedAccessException){ }
    }
}
