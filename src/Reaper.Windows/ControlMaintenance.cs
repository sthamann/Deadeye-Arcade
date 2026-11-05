using Reaper.Core;
namespace Reaper.Windows;

internal static class ControlMaintenance
{
    public static async Task<ControlAuditReport> Run(IEnumerable<GameEntry> entries,string data,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var games=entries.ToArray();var library=new LibraryStore(data).Load();
        var failures=new Dictionary<string,string>();
        using var raw=new RawInput();raw.Refresh();
        var players=GunConnections.ForConfiguration(library.Bindings,raw.Devices,GunDiscovery.Scan(raw.Devices));
        if(library.Bindings.Any(b=>b.Player==1) && !players.Any(b=>b.Player==1))
            return GameControlAudit.Save(games,data,players,raw.Devices,merge:true,
                preparationErrors:games.ToDictionary(g=>g.Id,_=>"Assigned P1 gun is unavailable. Existing game profiles were retained."));
        string? pack=null;
        if(games.Any(DolphinSetup.IsDolphin) && players.Any(p=>p.Player==1 && DolphinSetup.Supports(p)))
            try {pack=await DolphinAccuracyPackage.Prepare(data,cancellationToken);}
            catch(Exception e) when(e is IOException or HttpRequestException or TaskCanceledException) {
                cancellationToken.ThrowIfCancellationRequested();
                foreach(var game in games.Where(DolphinSetup.IsDolphin))failures[game.Id]=e.Message;
                File.AppendAllText(Path.Combine(data,"game-input.log"),$"{DateTimeOffset.Now:o} Accuracy package unavailable: {e.Message}\n");
            }
        foreach(var game in games)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if(game.Source=="mame")await MameMetadata.Prepare(game,data,cancellationToken);
                string[] Files()=>KnownFixes.InputFiles(game,data).Concat(KnownFixes.MultiplayerFiles(game)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                var before=KnownFixes.Snapshot(Files());
                _=GameInputProfiles.Configure(game,data,players,raw.Devices,raw.MouseOrder,
                    raw.Devices.ToDictionary(d=>d.Id,d=>InputDeviceNames.Name(d.Id)??(d.Kind=="mouse"?"Mouse":"Keyboard"),StringComparer.OrdinalIgnoreCase));
                if(pack is not null && DolphinSetup.IsDolphin(game))
                {
                    // No controller is started during a library audit. The launch
                    // path replaces this port with the running bridge's real port.
                    DolphinSetup.Configure(game,pack,players,DolphinSetup.RequiresBridge(players)?26761:null);
                }
                KnownFixes.RecordChanges(game,data,"control-completeness",before,Files());
            }
            catch(Exception e) when(e is IOException or UnauthorizedAccessException or ArgumentException or System.Xml.XmlException) {
                cancellationToken.ThrowIfCancellationRequested();
                failures[game.Id]=e.Message;
                File.AppendAllText(Path.Combine(data,"game-input.log"),$"{DateTimeOffset.Now:o} Control repair for {game.Title}: {e.Message}\n");
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return GameControlAudit.Save(games,data,players,raw.Devices,merge:true,preparationErrors:failures);
    }
}
