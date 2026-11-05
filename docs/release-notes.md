Find local multiplayer games and reconnect a stalled Vyper from Gun Studio.

- Display player modes below library entries and in game details: local co-op, local versus, taking turns, online and linked cabinets. Filter for local multiplayer and open manufacturer/store sources for the exact game edition.
- Include an initial catalog of 43 documented editions. Unknown entries are visibly unverified; imported notes cannot silently become official player metadata.
- Reconnect only the selected present Vyper USB device, then refresh RawInput registration and discovery. Windows requests administrator permission. Calibration and saved mappings are retained; hubs and wildcard resets are excluded.
- Export structured player metadata with `--export-player-support` for diagnostics.

Game player counts, generated control profiles and physical two-gun gameplay are separate verification steps. The Vyper idle failure still requires a reproduced long-idle test; a successful restart does not prove its root cause is fixed.
