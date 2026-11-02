using System.Runtime.InteropServices;
using System.Text;

namespace Deadeye.InputLab;

// Observe the desktop on capture failures. Never switch or unlock desktops.
internal static class InputDesktopDiagnostics
{
    [StructLayout(LayoutKind.Sequential)] private struct LastInput {public uint Size,Time;}
    [DllImport("user32.dll",SetLastError=true)] private static extern nint OpenInputDesktop(uint flags,bool inherit,uint access);
    [DllImport("user32.dll")] private static extern bool CloseDesktop(nint desktop);
    [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] private static extern bool GetUserObjectInformation(nint handle,int index,StringBuilder value,uint length,out uint required);
    [DllImport("user32.dll")] private static extern nint GetThreadDesktop(uint thread);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] private static extern bool GetLastInputInfo(ref LastInput info);
    internal sealed record DesktopState(string? inputDesktop,string? threadDesktop,int inputDesktopError,uint? idleMilliseconds);
    public static DesktopState Read()
    {
        string? Name(nint handle)
        {
            if(handle==0)return null;
            var value=new StringBuilder(256);
            return GetUserObjectInformation(handle,2,value,512,out _)?value.ToString():null;
        }
        nint input=OpenInputDesktop(0,false,1);int error=input==0?Marshal.GetLastWin32Error():0;
        try
        {
            var idle=new LastInput {Size=(uint)Marshal.SizeOf<LastInput>()};
            return new DesktopState(Name(input),Name(GetThreadDesktop(GetCurrentThreadId())),error,
                GetLastInputInfo(ref idle)?(uint?)(unchecked((uint)Environment.TickCount)-idle.Time):null);
        }
        finally {if(input!=0)CloseDesktop(input);}
    }
}
