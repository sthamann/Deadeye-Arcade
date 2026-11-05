using System.Text.RegularExpressions;

namespace Reaper.Core;

public static class VyperUsbRecovery
{
    // Only Blamcon player 1–4 composite nodes. Excludes hubs, MI interfaces and wildcards.
    public static bool IsDevice(string instance) => Regex.IsMatch(instance,
        @"\AUSB\\VID_3673&PID_010[0-3]\\[A-Z0-9_-]+\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
}
