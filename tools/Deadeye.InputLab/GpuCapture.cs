using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
                using var output5=output.QueryInterface<IDXGIOutput5>();
                D3D11.D3D11CreateDevice(candidate,DriverType.Unknown,DeviceCreationFlags.BgraSupport,
                    new[]{FeatureLevel.Level_11_0,FeatureLevel.Level_10_0},out ID3D11Device? created).CheckError();
                using var device=created??throw new InvalidOperationException("GPU capture device could not be created.");
                using(var context=device.ImmediateContext)
                // Explicitly request the byte format used by the bitmap. HDR
                // outputs can otherwise supply half-float pixels, not BGRA8.
                using(var duplication=output5.DuplicateOutput1(device,new[]{Format.B8G8R8A8_UNorm}))
                {
                  for(int attempt=0;attempt<4;attempt++)
                  {
                    duplication.AcquireNextFrame(2000,out var frameInfo,out var resource).CheckError();
                    try
                    {
                        using(resource)
                        {
                            using var source=resource.QueryInterface<ID3D11Texture2D>();
                            var desc=source.Description;
                            Console.WriteLine($"DXGI frame: present={frameInfo.LastPresentTime}; accumulated={frameInfo.AccumulatedFrames}; protected={frameInfo.ProtectedContentMaskedOut}; format={desc.Format}; size={desc.Width}x{desc.Height}");
                            if(frameInfo.ProtectedContentMaskedOut)throw new IOException("Windows masked protected image content; capture is unavailable.");
                            if(frameInfo.LastPresentTime==0)continue; // Pointer-only update, not a new desktop image.
                            if(desc.Format!=Format.B8G8R8A8_UNorm)throw new IOException("DXGI returned a pixel format outside the requested capture contract.");
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
                  throw new IOException("DXGI supplied no updated desktop image after four frames.");
                }
            }
        }
        throw new InvalidOperationException("No matching active GPU output. Run the test in the local console session.");
    }
}
