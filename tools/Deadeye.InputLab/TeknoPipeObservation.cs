using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Xml.Linq;
using Reaper.Core;

namespace Deadeye.InputLab;

// Read-only observation of the reviewed emulator's JVS input exchange.
// A pipe sample proves emulator input routing, not the game's use of that input.
internal sealed class TeknoPipeObservation : IDisposable
{
    private readonly MemoryMappedFile map;
    private readonly MemoryMappedViewAccessor view;
    public int MinX{get;} public int MaxX{get;} public int MinY{get;} public int MaxY{get;}
    private TeknoPipeObservation(GameEntry game)
    {
        map=MemoryMappedFile.OpenExisting("TeknoParrot_JvsState",MemoryMappedFileRights.Read);
        view=map.CreateViewAccessor(0,64,MemoryMappedFileAccess.Read);
        var root=XDocument.Load(game.SourcePath).Root!;
        MinX=int.Parse(root.Element("xAxisMin")!.Value);MaxX=int.Parse(root.Element("xAxisMax")!.Value);
        MinY=int.Parse(root.Element("yAxisMin")!.Value);MaxY=int.Parse(root.Element("yAxisMax")!.Value);
    }
    public static TeknoPipeObservation? Attach(GameEntry game)
    {
        if(!Path.GetFileNameWithoutExtension(game.SourcePath).Equals("AliensExtermination",StringComparison.OrdinalIgnoreCase))return null;
        if(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(game.Executable)))!="17ABD4D24415997F905148E063BAC0856505F55DBF8085D55BB12933D93644C2")return null;
        try{return new(game);}catch(FileNotFoundException){return null;}
    }
    public PipeSample Sample()=>new(view.ReadUInt32(8),view.ReadByte(12),view.ReadByte(16),view.ReadByte(20),view.ReadByte(24));
    public static uint? Mask(string? source)=>source switch {
        "Coin1"=>1024,"Coin2"=>2048,"P1Button1"=>512,"P1Button2"=>256,"P1Button3"=>16,"P1Button4"=>32,
        "P2Button1"=>16384,"P2Button2"=>8192,"P2Button3"=>4,"P2Button4"=>8,_=>null};
    public void Dispose(){view.Dispose();map.Dispose();}
}
internal sealed record PipeSample(uint Buttons,byte P1X,byte P1Y,byte P2X,byte P2Y);
