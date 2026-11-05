using System.Text.Json;
using System.Text.RegularExpressions;

namespace Reaper.Core;

public record PlayerSupport(string Mode, int? MaxPlayers, string[] Sources, string CheckedAt, string? Note = null)
{
    public bool LocalMultiplayer => Mode is "local-coop" or "local-versus" or "local-mixed" or "alternating" or "asymmetric";
}
public record PlayerRule(string[] Titles, string? Platform, string? SteamAppId, string? TeknoProfile, PlayerSupport Support);

// Publisher/store and manufacturer metadata describes the game edition, never
// whether its current emulator, patch or two physical guns have been verified.
public static class GamePlayers
{
    public static IReadOnlyList<PlayerRule> Catalog { get; } = Load();
    private static PlayerRule[] Load()
    {
        using var stream = typeof(GamePlayers).Assembly.GetManifestResourceStream("Reaper.Core.GamePlayers.json")!;
        return JsonSerializer.Deserialize<PlayerRule[]>(stream, JsonDefaults.Options)!;
    }
    private static string Name(string text) => Regex.Replace(text.ToUpperInvariant(), @"[^\p{L}\p{Nd}]", "");
    public static PlayerSupport? Read(GameEntry game)
    {
        if (game.Platform == "PC / Windows")
        {
            var apps = (game.MetadataSources ?? []).Select(s=>Regex.Match(s,@"^https://store\.steampowered\.com/app/(\d+)(?:/|$)",RegexOptions.IgnoreCase))
                .Where(m=>m.Success).Select(m=>m.Groups[1].Value).Distinct().ToArray();
            var byApp = Catalog.Where(r=>r.SteamAppId is not null && apps.Contains(r.SteamAppId)).ToArray();
            if (byApp.Length == 1) return byApp[0].Support;
            if (byApp.Length > 1) return null;
        }
        string profile = Path.GetFileNameWithoutExtension(game.SourcePath.Replace('\\','/'));
        var matches = Catalog.Where(r=>r.TeknoProfile is not null
            ? game.Source == "teknoparrot" && r.TeknoProfile.Equals(profile,StringComparison.OrdinalIgnoreCase)
            : r.Platform == game.Platform && r.Titles.Any(title=>Name(title)==Name(game.Title))).ToArray();
        return matches.Length == 1 ? matches[0].Support : null;
    }
    public static object[] Inventory(IEnumerable<GameEntry> games) => games.Select(g=>new {
        g.Id, g.Title, g.Platform, officialPlayers=Read(g), existingCatalogNote=g.Players,
        localTestStatus=g.Status
    }).Cast<object>().ToArray();
}
