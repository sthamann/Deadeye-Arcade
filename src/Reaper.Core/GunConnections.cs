namespace Reaper.Core;

public static class GunConnections
{
    // RDP can hide a gun's RawInput mouse while its exact USB interface remains
    // present. That is enough to retain a device-name profile, not a mouse index.
    public static GunBinding[] ForConfiguration(IEnumerable<GunBinding> bindings,
        IReadOnlyList<InputDevice> inputs, IEnumerable<PhysicalGun> physical)
    {
        var present = physical.ToArray();
        // A USB reconnect may change the interface path while retaining the
        // physical container identity. Resolve it from this scan, preserving
        // the selected slot and learned controls rather than dropping that gun.
        var resolved = bindings.Select(b => {
            var matches = present.Where(g => b.PhysicalId is not null && g.Id == b.PhysicalId &&
                g.SystemId == b.SystemId && g.DriverHealthy && !string.IsNullOrWhiteSpace(g.MouseId) &&
                g.InputIds.Contains(g.MouseId, StringComparer.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1) return b;
            var gun = matches[0];
            return b with { MouseId = gun.MouseId!, KeyboardId = gun.KeyboardId, SerialPort = gun.Port };
        });
        return resolved.Where(b => b.Player is 1 or 2 && !string.IsNullOrWhiteSpace(b.MouseId) &&
            (inputs.Any(d => d.Kind == "mouse" && d.Id.Equals(b.MouseId, StringComparison.OrdinalIgnoreCase)) ||
             present.Any(g => g.DriverHealthy && g.SystemId == b.SystemId &&
                 string.Equals(g.MouseId, b.MouseId, StringComparison.OrdinalIgnoreCase) &&
                 g.InputIds.Contains(b.MouseId, StringComparer.OrdinalIgnoreCase)))).ToArray();
    }

    public static bool CanIndexMice(IEnumerable<GunBinding> bindings, IReadOnlyList<InputDevice> inputs) =>
        bindings.All(b => inputs.Any(d => d.Kind == "mouse" && d.Id.Equals(b.MouseId, StringComparison.OrdinalIgnoreCase)));
}
