using Reaper.Core;

internal static class ConsoleCompatibilityChecks
{
    internal static void Run(Action<bool,string> check)
    {
        var consoleGame=new GameEntry("console-fixture","Console fixture","Arcade",@"C:\TP\TeknoParrotUi.exe",[],@"C:\TP","teknoparrot","");
        var consoleNow=DateTimeOffset.UtcNow;
        var consoleEvidence=System.Text.Json.Nodes.JsonNode.Parse("""
        {"id":"console-fixture","launchPassed":false,"cancelled":false,"failure":{"category":"foreground-missing"},"rendererResponse":{"responding":true},"time":"2026-01-01T00:00:00Z","processDetails":{"foregroundProcess":{"processName":"WindowsTerminal","mainWindowTitle":"C:\\TP\\ElfLdr2\\BudgieLoader.exe"},"processes":[{"processName":"BudgieLoader","path":"C:\\TP\\ElfLdr2\\BudgieLoader.exe"}]}}
        """)!;
        consoleEvidence["time"]=consoleNow.AddMinutes(-1).ToString("O");
        string consoleJson=consoleEvidence.ToJsonString();
        check(Reaper.Windows.ConsoleCompatibility.Affected(consoleGame,consoleJson,consoleNow) is not null,"Console repair requires an observed owned BudgieLoader console stealing foreground focus");
        consoleEvidence["processDetails"]!["foregroundProcess"]!["mainWindowTitle"]=@"C:\Users\Example\maintenance.ps1";
        check(Reaper.Windows.ConsoleCompatibility.Affected(consoleGame,consoleEvidence.ToJsonString(),consoleNow) is null,"An unrelated maintenance terminal cannot trigger a system preference repair");
        consoleEvidence=System.Text.Json.Nodes.JsonNode.Parse(consoleJson)!;consoleEvidence["cancelled"]=true;
        check(Reaper.Windows.ConsoleCompatibility.Affected(consoleGame,consoleEvidence.ToJsonString(),consoleNow) is null,"A cancelled diagnostic cannot trigger console repair");
        consoleEvidence=System.Text.Json.Nodes.JsonNode.Parse(consoleJson)!;consoleEvidence["time"]=consoleNow.AddDays(-8).ToString("O");
        check(Reaper.Windows.ConsoleCompatibility.Affected(consoleGame,consoleEvidence.ToJsonString(),consoleNow) is null,"Stale console observations cannot change the current Windows preference");
        check(Reaper.Windows.ConsoleCompatibility.Affected(consoleGame with {Id="another-game"},consoleJson,consoleNow) is null && Reaper.Windows.ConsoleCompatibility.Affected(consoleGame with {Source="pc"},consoleJson,consoleNow) is null,"Console repair excludes other games and non-TeknoParrot launch paths");
        check(Reaper.Windows.ConsoleCompatibility.Affected(consoleGame,"{}",consoleNow) is null && Reaper.Windows.ConsoleCompatibility.Affected(consoleGame,"not JSON",consoleNow) is null,"Incomplete console diagnostics stay non-actionable");
        if(OperatingSystem.IsWindows())
        {
            string consoleRegistry=@"Software\DeadeyeArcade\Checks\"+Guid.NewGuid().ToString("N");
            string consoleData=Path.Combine(Path.GetTempPath(),"deadeye-console-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(consoleData);
            try
            {
                using var fixtureKey=Microsoft.Win32.Registry.CurrentUser.CreateSubKey(consoleRegistry,true);
                fixtureKey.SetValue("DelegationTerminal","original terminal",Microsoft.Win32.RegistryValueKind.String);
                File.WriteAllText(Path.Combine(consoleData,"game-test-console-fixture.json"),consoleJson);
                var repaired=Reaper.Windows.ConsoleCompatibility.Apply([consoleGame],consoleData,consoleRegistry);
                var consoleTarget=new Reaper.Windows.ConsoleCompatibility.SavedPreference(Reaper.Windows.ConsoleCompatibility.ConsoleHost,Reaper.Windows.ConsoleCompatibility.ConsoleHost);
                check(repaired.Single().Status=="repaired" && Reaper.Windows.ConsoleCompatibility.Read(fixtureKey)==consoleTarget,"Observed console failure repairs both preferences through the actual repair path in an isolated registry key");
                using var backup=System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(consoleData,"console-preference-before-deadeye.json")));
                var original=System.Text.Json.JsonSerializer.Deserialize<Reaper.Windows.ConsoleCompatibility.SavedPreference>(backup.RootElement.GetProperty("original"),JsonDefaults.Options)!;
                check(original.Console is null && original.Terminal=="original terminal","Console repair backup retains missing values and the exact original preference");
                check(Reaper.Windows.ConsoleCompatibility.Apply([consoleGame],consoleData,consoleRegistry).Single().Status=="ready","Console repair is idempotent when the verified preference is already active");
                Reaper.Windows.ConsoleCompatibility.Write(fixtureKey,original);
                check(!fixtureKey.GetValueNames().Contains("DelegationConsole") && Reaper.Windows.ConsoleCompatibility.Read(fixtureKey)==original,"Restoring the backup removes originally absent values and restores the terminal choice");
                check(Reaper.Windows.ConsoleCompatibility.Apply([consoleGame],consoleData,consoleRegistry).Single().Status=="needs-action" && Reaper.Windows.ConsoleCompatibility.Read(fixtureKey)==original,"Old game evidence cannot repeatedly override a subsequent user preference");
            }
            finally {Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(consoleRegistry,false);Directory.Delete(consoleData,true);}
        }
    }
}
