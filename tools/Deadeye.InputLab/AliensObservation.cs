using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml.Linq;
using Microsoft.Win32.SafeHandles;
using Reaper.Core;
using Reaper.Windows;

namespace Deadeye.InputLab;

// Read-only measurements for two exact known builds. Address facts and build
// identifiers: argonlefou/DemulShooter, DemulShooter/Games/Game_GvrAliens.cs.
// This observer never patches instructions or writes game memory.
internal sealed class AliensObservation : IDisposable
{
    public static string? LastBuildFingerprint {get;private set;}
    private readonly SafeProcessHandle handle;
    private readonly nint start;
    private AliensObservation(Process process){start=process.MainModule!.BaseAddress;handle=OpenProcess(0x1010,false,process.Id);}
    public static AliensObservation? Attach(GameSession session,GameEntry game)
    {
        if(!Path.GetFileNameWithoutExtension(game.SourcePath).Equals("AliensExtermination",StringComparison.OrdinalIgnoreCase))return null;
        string path=XDocument.Load(game.SourcePath).Root!.Element("GamePath")!.Value;
        string fingerprint=Convert.ToHexString(MD5.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
        LastBuildFingerprint=fingerprint;
        if(fingerprint is not ("30c8725c19d07fbebb68903f8f068052" or "9ed286aafe474a16c07f7d62b5c06ecb"))return null;
        foreach(int pid in session.LiveProcesses())
            try
            {
                using var process=Process.GetProcessById(pid);
                if(!process.ProcessName.Equals(Path.GetFileNameWithoutExtension(path),StringComparison.OrdinalIgnoreCase))continue;
                var observer=new AliensObservation(process);if(!observer.handle.IsInvalid)return observer;observer.Dispose();
            }
            catch(Exception e) when(e is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception) { }
        return null;
    }
    private byte[]? Read(int offset,int count)
    {
        var bytes=new byte[count];return ReadProcessMemory(handle,start+offset,bytes,(nuint)count,out nuint read)&&read==(nuint)count?bytes:null;
    }
    private ushort? U16(int offset)=>Read(offset,2) is {} bytes?BitConverter.ToUInt16(bytes):null;
    private byte? U8(int offset)=>Read(offset,1)?[0];
    private float? F32(int offset)
    {
        if(Read(offset,4) is not {} bytes)return null;
        float value=BitConverter.ToSingle(bytes);return float.IsFinite(value)?value:null;
    }
    public AliensSample Sample()=>new(U8(0x0047C960),
        U16(0x0556AEA8),U16(0x0556AEAC),U8(0x0556ACBC),U8(0x0556C844),F32(0x0556CB08),
        U16(0x0556B1C0),U16(0x0556B1C4),U8(0x0556AFD4),U8(0x0556CBD8),F32(0x0556CE9C));
    public void Dispose()=>handle.Dispose();
    [DllImport("kernel32.dll",SetLastError=true)]private static extern SafeProcessHandle OpenProcess(uint access,bool inherit,int process);
    [DllImport("kernel32.dll",SetLastError=true)]private static extern bool ReadProcessMemory(SafeProcessHandle process,nint address,[Out]byte[] data,nuint size,out nuint read);
}
internal sealed record AliensSample(byte? GameStatus,ushort? P1X,ushort? P1Y,byte? P1Buttons,byte? P1Status,float? P1Ammo,
    ushort? P2X,ushort? P2Y,byte? P2Buttons,byte? P2Status,float? P2Ammo);
