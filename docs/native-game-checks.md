# Native game checks

These observations were made on a physical Windows 11 console on 1 November
2026. Remote Desktop was disconnected while games ran. They are bounded checks,
not complete playthroughs or a certification of every physical gun model.

## Recent and well rated titles

The selection combines newer releases with established games that have positive
player feedback. [Blue Estate](https://store.steampowered.com/app/305380/),
[Mad Bullets](https://store.steampowered.com/app/452860/) and
[Rez Infinite](https://store.steampowered.com/app/636450/) had Very Positive Steam
ratings when selected. A high rating does not establish lightgun compatibility.

| Game | Observed result | Remaining verification |
| --- | --- | --- |
| Raccoon Rampage | Two virtual players joined an active level. Separate scores, shots, hits and additional action effects were visible. Both absolute aim sweeps and press/release delivery passed. | Physical tracking, all special actions and sustained play. |
| The House of the Dead: Scarlet Dawn | Two virtual players joined an active level. Shooting, critical hits and changing weapon icons were observed. | Complete weapon/action coverage and sustained play. A later replay reached a Continue screen, so packet delivery alone remains insufficient. |
| Tomb Raider Arcade | Native renderer starts; separate aim/button packets and cabinet credits are received. | Start still fails to enter gameplay in the tested replay. Not accepted as gameplay working. |
| Railbreak | Native start, rendered menu, menu/resume focus and owned process cleanup passed. | Actual shooting, reload and independent two-gun gameplay. |
| Dead Containment | Installed demo starts, renders its menu, resumes after the Deadeye menu and closes cleanly. | Full game and actual gun actions. Online co-op is not local two-gun verification. |
| The House of the Dead 2: Remake | Native start, rendered menu, menu/resume focus and cleanup passed. | Actual gun actions and independent P2 gameplay. |
| Blue Estate | Native start, rendered menu, menu/resume focus and cleanup passed. | Actual gun actions and fullscreen behavior with a matching multiplayer patch. The staged multiplayer files were not active, so two-player support was not accepted. |
| Mad Bullets | Native start with Steam running, rendered menu, menu/resume focus and cleanup passed. | Shooting and reload in gameplay. |
| DEAD RELAY: Relay Mart | Native start, rendered menu, menu/resume focus and cleanup passed. | Actual gun actions and sustained play. |
| Blood Storm: Alien Purge | Native start, foreground/menu-resume and cleanup passed. | Actual gun actions and a fresh visual review using the corrected capture backend. |
| Rez Infinite | Rejected: white startup window that does not respond to Windows messages. | Startup hang remains unresolved. |

## Dead Space, House of the Dead, Walking Dead and Terminator

A second group exercised sixteen additional installed launch paths on the native
console. Checks ran for a bounded 50–120 seconds after startup, with further
virtual-input observations where an adapter was available.

| Game or edition | Observed result | Remaining verification or failure |
| --- | --- | --- |
| Dead Space: Extraction, Dolphin | Native launch, rendered title, menu/resume and cleanup passed. Two controller subscriptions and independent production DSU streams were observed in an isolated configuration. All 24 required action routes resolved. | Initial input replay occurred during boot. The stricter automatic title recognizer then refused the scene. In-level shooting, secondary actions and two-player gameplay remain unverified. |
| The House of the Dead: Overkill, Dolphin | Native launch, menu/resume and cleanup passed. Two DSU consumers and all 16 required action routes were observed. | The scene-gated replay stopped during boot/movie recognition. In-level actions remain unverified. |
| The House of the Dead 4 | Native launch and menu/resume passed. The production setup repaired an existing relative-input setting to absolute aim. | The later virtual replay stopped on an out-of-output capture/client transition and unexpected session end. No full input/gameplay pass is claimed. |
| The House of the Dead 4 Special | Renderer starts and receives virtual input packets. | Still displays the cabinet ride-reset prompt after both ordinary Start presses and a one-second hold. Session-end anomalies were retained. Not accepted as working gameplay. |
| The House of the Dead EX | Rejected as renderer-unresponsive, with and without the Deadeye menu. | Startup remains unresolved. The staged shared loader library matched the available source file, ruling out a simple missing/different-copy explanation. |
| The House of the Dead, Model 2 | Native launch, rendered output, menu/resume and cleanup passed. | Complete button coverage and independent P2 actions have not been measured. |
| The House of the Dead, Saturn | Native launch, menu/resume and cleanup passed. | Complete game-specific gun controls and physical play remain unverified. |
| The House of the Dead 2, Naomi | Native launch, menu/resume and cleanup passed. | Actual gun actions and independent P2 gameplay. |
| The House of the Dead 2, Dreamcast | Native launch, menu/resume and cleanup passed. | Actual gun actions and independent P2 gameplay. |
| The House of the Dead 2, classic Windows | Rejected: process exits with code -1 before a renderer appears. | Startup remains unresolved. DDrawCompat was already present and initialized; a missing wrapper is not the established cause. |
| The House of the Dead III, classic Windows with Arcade Mod | Rejected as renderer-unresponsive, also without the Deadeye menu. | Installed executable digest is absent from the mod's documented supported list. This is a compatibility lead, not a proven cause. Existing keyboard control mode is correct and was preserved. |
| The House of the Dead III, Chihiro | Passed native launch, renderer response, menu/resume and cleanup when started as an ordinary user. | The first elevated test hit the emulator's administrator warning. No in-level gun-action certificate. |
| The House of the Dead: Remake | Native launch, rendered menu, menu/resume and cleanup passed. Existing multiplayer input mode was confirmed. | Actual independent P2 actions and sustained play remain unverified. The two installed version-specific plugin DLLs deliberately self-select; their presence alone is not a conflict. |
| The Walking Dead Arcade | Both players joined an active level. Separate reticles and ammunition counters were visible; each player's shot reduced its own ammunition from 8 to 7. Both aim sweeps and packet isolation passed. | Hits, complete special-action coverage, sustained play and physical tracking remain open. An initial cold launch was black; later launches rendered with and without the menu, so an overlay cause was not established. |
| Terminator Salvation | Native launch/menu-resume passed. Virtual P1/P2 input, full aim sweeps and button press/release isolation passed. The replay advanced through mission selection into the cinematic and mission briefing. | In-level shots, grenade and magazine/pump reload effects were not established by the captured scenes. |
| Terminator 2: Judgment Day, MAME | Native launch, menu/resume and cleanup passed. | Actual game-specific action effects, independent players and physical tracking. |

The automatic TeknoParrot setup now detects stale `Use Relative Input` settings
and turns them off for assigned RS3 Reaper, Sinden, XGunner and Blamcon profiles
that provide that vendor field. This correction ran through the real House of
the Dead 4 launch path. It does not invent settings in profiles that lack the
field or alter the relative setting without an assigned supported gun.

The Dolphin input lab uses a separate emulator user directory, copies only
configuration INIs, redirects write paths and never copies original saves or
discs. Original configuration hashes and the library were unchanged after the
replays. Its title recognizer is experimental and rejects scenes it cannot
identify rather than marking boot input as working gameplay.

Vendor guidance checked during diagnosis: [Dolphin accuracy profiles](https://github.com/ProfgLX/Dolphin-Lightguns-Accuracy-Inis),
[DemulShooter Windows game setup](https://github.com/argonlefou/DemulShooter/wiki/Windows-games),
[HOTD III Arcade Mod](https://github.com/argonlefou/Hotd3pc_ArcadeMod) and
[HOTD Remake Arcade Plugin](https://github.com/argonlefou/HotdRemake_ArcadePlugin).
No replacement commercial game executable was downloaded or installed during
these checks.

## What the checks establish

A native launch check exercises the real session owner, foreground handling,
optional menu/resume and process cleanup. It now also probes whether the renderer
responds to Windows messages. A visible window alone cannot pass a frozen renderer.

Input Lab adds independent virtual absolute HID guns and keyboard collections.
Its report separates packet delivery, scene observations and gameplay verification.
A successful sweep means the input path received the expected coordinates; only
reviewed game images or a supported game-state probe establish the game's response.
The report deliberately retains `gameplayVerified: false` for generic replay.

## Diagnostic corrections from these checks

- Request BGRA8 images from desktop duplication and skip pointer-only frames.
  Some HDR/fullscreen games returned half-float or empty first frames; treating
  those as BGRA8 produced misleading black screenshots.
- Recognize observed arcade-font variations of Start and Insert Credits without
  accepting a loading screen as ready.
- Send cabinet credits before Start, including four coin pulses per player for
  the observed Tomb Raider cabinet. This corrected the test recipe; it did not
  establish successful gameplay entry.
- Allow the reviewed Raccoon Rampage, Scarlet Dawn and Tomb Raider menu sequences
  time to complete. Save the post-menu scene for review.
- Treat a nonresponsive renderer as a failed launch check and retain the native
  response/error fields in the report.

A stuck Windows GameInput service held foreground focus after a virtual-device
sequence. Restarting that service restored the two affected PC launch checks.
This was a test-machine recovery, not a persistent service or security change.
The ordinary frontend does not install virtual devices or restart that service.

Each replay restored its original profile and preserved the game library. The
final native console check also checks removal of the temporary virtual devices,
return to the frontend and the physical display mode.

A trial to force a composited borderless Blue Estate window was rejected because
the engine repeatedly restored its window frame. It is not part of the shipped
change; existing display behavior and installed multiplayer patches are preserved.

## Published package verification

The [0.3.63 release](https://github.com/sthamann/deadeye-arcade/releases/tag/v0.3.63)
passed [747 Windows core checks and eight scene signal cases](https://github.com/sthamann/deadeye-arcade/actions/runs/41319015483).
The installer and optional Input Lab archive were checked against their published
SHA-256 digests before installation.

On the physical Windows console, the installed package then passed a Railbreak
launch, menu/resume, renderer-response and cleanup check. Rez Infinite remained
frozen and was correctly rejected as `renderer-unresponsive` with
`rendererResponse.responding: false`, even without opening the overlay.

The library hash remained unchanged, no virtual test device collections remained,
and the frontend returned to foreground. Both current and saved display modes
were 3840 × 2160 at 60 Hz. These package checks add no physical-gun gameplay claim.
