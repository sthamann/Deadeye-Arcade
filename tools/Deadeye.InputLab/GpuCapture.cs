using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Deadeye.InputLab;

internal static class GpuCapture
{
    public static Bitmap Frame(nint monitor)
    {
        using var factory=DXGI.CreateDXGIFactory1<IDXGIFactory1>();
        for(uint a=0;factory.EnumAdapters1(a,out var candidate).Success;a++)
        using(candidate)
        {
            for(uint o=0;candidate.EnumOutputs(o,out var output).Success;o++)
            using(output)
            {
                var description=output.Description;
                if(!description.AttachedToDesktop||description.Monitor!=monitor)continue;
                using var output1=output.QueryInterface<IDXGIOutput1>();
                D3D11.D3D11CreateDevice(candidate,DriverType.Unknown,DeviceCreationFlags.BgraSupport,
                    new[]{FeatureLevel.Level_11_0,FeatureLevel.Level_10_0},out ID3D11Device? created).CheckError();
                using var device=created??throw new InvalidOperationException("GPU capture device could not be created.");
                using(var context=device.ImmediateContext)
                using(var duplication=output1.DuplicateOutput(device))
                {
                    duplication.AcquireNextFrame(2000,out _,out var resource).CheckError();
                    try
                    {
                        using(resource)
                        using(var source=resource.QueryInterface<ID3D11Texture2D>())
                        {
                            var desc=source.Description;
                            desc.Usage=ResourceUsage.Staging;desc.BindFlags=BindFlags.None;
                            desc.CPUAccessFlags=CpuAccessFlags.Read;desc.MiscFlags=ResourceOptionFlags.None;
                            using var staging=device.CreateTexture2D(desc);
                            context.CopyResource(staging,source);
                            var pixels=context.Map(staging,0,MapMode.Read,Vortice.Direct3D11.MapFlags.None);
                            try
                            {
                                var bitmap=new Bitmap((int)desc.Width,(int)desc.Height,PixelFormat.Format32bppRgb);
                                var target=bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),ImageLockMode.WriteOnly,bitmap.PixelFormat);
                                try
                                {
                                    var line=new byte[bitmap.Width*4];
                                    for(int y=0;y<bitmap.Height;y++)
                                    {
                                        Marshal.Copy(pixels.DataPointer+y*(int)pixels.RowPitch,line,0,line.Length);
                                        Marshal.Copy(line,0,target.Scan0+y*target.Stride,line.Length);
                                    }
                                }
                                finally{bitmap.UnlockBits(target);}
                                return bitmap;
                            }
                            finally{context.Unmap(staging,0);}
                        }
                    }
                    finally{duplication.ReleaseFrame();}
                }
            }
        }
        throw new InvalidOperationException("No matching active GPU output. Run the test in the local console session.");
    }
}
