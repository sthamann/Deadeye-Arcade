using System.IO;
using System.Text.Json;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Reaper.Core;

namespace Reaper.Windows;

// Windows' default terminal is a per-user OS preference, not a Terminal settings.json field.
// https://learn.microsoft.com/en-us/windows/terminal/group-policy#default-terminal-application
internal static class ConsoleCompatibility
{
    internal const string ConsoleHost = "{B23D10C0-E52E-411E-9D5B-C09FDF709C7D}";
    internal record SavedPreference(string? Console, string? Terminal);

    // Require a recent failed native observation of this game's owned BudgieLoader
    // console. Merely having a maintenance terminal open is not evidence.
    internal static DateTimeOffset? Affected(GameEntry game, string json, DateTimeOffset now)
    {
        if (game.Source != "teknoparrot") return null;
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.GetProperty("id").GetString() != game.Id || root.GetProperty("launchPassed").GetBoolean()
                || root.GetProperty("cancelled").GetBoolean()
                || root.GetProperty("failure").GetProperty("category").GetString() != "foreground-missing"
                || !root.GetProperty("rendererResponse").GetProperty("responding").GetBoolean()) return null;
            var time = root.GetProperty("time").GetDateTimeOffset();
            if (time > now || now - time > TimeSpan.FromDays(7)) return null;
            var details = root.GetProperty("processDetails");
            var foreground = details.GetProperty("foregroundProcess");
            if (!string.Equals(foreground.GetProperty("processName").GetString(), "WindowsTerminal", StringComparison.OrdinalIgnoreCase)) return null;
            string title = Normalize(foreground.GetProperty("mainWindowTitle").GetString());
            string folder = Normalize(game.WorkingDirectory).TrimEnd('/') + "/";
            if (folder.Length < 4 || !title.StartsWith(folder, StringComparison.OrdinalIgnoreCase)) return null;
            return details.GetProperty("processes").EnumerateArray().Any(p =>
                string.Equals(p.GetProperty("processName").GetString(), "BudgieLoader", StringComparison.OrdinalIgnoreCase)
                && Normalize(p.GetProperty("path").GetString()).Equals(title, StringComparison.OrdinalIgnoreCase)) ? time : null;
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or KeyNotFoundException or FormatException) { return null; }
    }

    private static string Normalize(string? path) => (path ?? "").Replace('\\', '/');

    [SupportedOSPlatform("windows")]
    internal static SavedPreference Read(RegistryKey key)
    {
        foreach (string name in new[] { "DelegationConsole", "DelegationTerminal" })
            if (key.GetValueNames().Contains(name, StringComparer.OrdinalIgnoreCase) && key.GetValueKind(name) != RegistryValueKind.String)
                throw new IOException("The console preference has an unsupported registry value type.");
        return new(key.GetValue("DelegationConsole") as string, key.GetValue("DelegationTerminal") as string);
    }

    [SupportedOSPlatform("windows")]
    internal static void Write(RegistryKey key, SavedPreference preference)
    {
        Set("DelegationConsole", preference.Console); Set("DelegationTerminal", preference.Terminal); key.Flush();
        void Set(string name, string? value)
        { if (value is null) key.DeleteValue(name, false); else key.SetValue(name, value, RegistryValueKind.String); }
    }

    [SupportedOSPlatform("windows")]
    internal static FixFinding[] Apply(IEnumerable<GameEntry> games, string data, string registryPath = @"Console\%%Startup")
    {
        List<FixFinding> findings = [];
        foreach (var game in games.Where(g => g.Source == "teknoparrot"))
        {
            string report = Path.Combine(data, "game-test-" + game.Id + ".json");
            if (!File.Exists(report)) continue;
            try
            {
                if (Affected(game, File.ReadAllText(report), DateTimeOffset.UtcNow) is not { } observed) continue;
                using var key = Registry.CurrentUser.CreateSubKey(registryPath, true);
                var original = Read(key);
                if (original.Console == ConsoleHost && original.Terminal == ConsoleHost)
                { Add("ready", I18n.T("Windows Console Host ist bereits ausgewählt. Wiederhole den Spieltest, um den Fensterfokus zu prüfen."), []); continue; }
                string backup = Path.Combine(data, "console-preference-before-deadeye.json");
                // An old failure must never undo a later deliberate user preference.
                if (File.Exists(backup))
                { Add("needs-action", I18n.T("Die Konsoleneinstellung wurde nach einer früheren Reparatur geändert. Deine Auswahl bleibt erhalten. Wähle Windows Console Host in den Terminaleinstellungen, um erneut zu testen."), [backup]); continue; }
                Directory.CreateDirectory(data);
                using (var saved = new FileStream(backup, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    JsonSerializer.Serialize(saved, new { observed, gameId = game.Id, original }, JsonDefaults.Options);
                try
                {
                    var target = new SavedPreference(ConsoleHost, ConsoleHost);
                    Write(key, target);
                    if (Read(key) != target) throw new IOException("The console preference could not be verified.");
                }
                catch { Write(key, original); throw; }
                Add("repaired", I18n.T("Die Startkonsole des Spiels hat in Windows Terminal den Fensterfokus übernommen. Für neue Konsolen dieses Windows Benutzers wurde Windows Console Host ausgewählt und die bisherige Einstellung gesichert. Wiederhole den Spieltest."), [backup]);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or System.Security.SecurityException or JsonException)
            { Add("needs-action", I18n.T("Die Reparatur der Konsolenkompatibilität konnte nicht abgeschlossen werden: ") + e.Message, []); }
            void Add(string status, string message, string[] files) => findings.Add(new(game.Id, game.Title, "windows-console-focus", status, message, files));
        }
        return findings.ToArray();
    }
}
