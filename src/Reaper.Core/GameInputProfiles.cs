namespace Reaper.Core;

// File preparation shared by library repair and the actual launch. No serial
// changes, feedback output, controller bridge or game process starts here.
public static class GameInputProfiles
{
    public static GameEntry Configure(GameEntry game, string data, GunBinding[] players,
        IReadOnlyList<InputDevice> devices, IReadOnlyList<string> mouseOrder,
        IReadOnlyDictionary<string,string>? deviceNames = null)
    {
        if (players.Length == 0) return game; // A keyboard-only launch must not erase gun profiles.
        if (game.Source == "mame") MameControls.Configure(game, data, players);
        if (game.Source == "teknoparrot") TeknoGunSetup.Configure(game, players);
        bool indexed = GunConnections.CanIndexMice(players, devices);
        if (indexed) game = RetroArchSetup.Configure(game, data, players, mouseOrder);
        game = FlycastSetup.ResolveContent(game, validateArchive: true);
        game = Rpcs3Setup.Configure(game, players);
        game = TimeCrisis5Controls.Configure(game, players);
        if (indexed)
        {
            SupermodelSetup.Configure(game, players, devices);
            if (FlycastSetup.IsFlycast(game))
                FlycastSetup.Configure(game, players, devices, deviceNames ?? devices.ToDictionary(d=>d.Id,d=>d.Name,StringComparer.OrdinalIgnoreCase));
        }
        MultiplayerSetup.Configure(game, players);
        Hod3Controls.Configure(game, players);
        return game;
    }
}
