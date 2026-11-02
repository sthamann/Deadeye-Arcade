# Native game checks

These observations were made on a physical Windows 11 console on 1–2 November
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
| The House of the Dead 4 Special | Automatic setup with the matching installed ElfLdr2 metadata and complete `elf`/`fs`/`drv` data passed bounded native launch and cleanup. A virtual Start left the ride-reset prompt and reached the opening cinematic. | The earlier partial-data replay did not leave the cabinet prompt. Fullscreen/menu presentation, in-level weapon effects and independent P2 gameplay remain unverified. |
| The House of the Dead EX | Complete sibling data fixes the flattened-copy hang. The published native-display repair passed visible menu, full resumed image, renderer response and cleanup. Its input replay delivered both players' mapped actions and reached Chapter 1. | Independent P2 gameplay, all action effects and physical tracking remain unverified. The staged shared libraries already matched their source; they were not replaced. |
| The House of the Dead, Model 2 | Native launch, rendered output, menu/resume and cleanup passed. | Complete button coverage and independent P2 actions have not been measured. |
| The House of the Dead, Saturn | Native launch, menu/resume and cleanup passed. Follow-up removed false warnings about unused A/B/C and D-pad inputs; the reviewed core consumes Trigger, Reload and Start only. | Core/content overrides and actual gun actions remain unverified. Missing required inputs and shared Start keys are still detected. |
| The House of the Dead 2, Naomi | Native launch, menu/resume and cleanup passed. | Actual gun actions and independent P2 gameplay. |
| The House of the Dead 2, Dreamcast | Native launch, menu/resume and cleanup passed. | Actual gun actions and independent P2 gameplay. |
| The House of the Dead 2, classic Windows | Initial check exited with code -1. Two later native launches passed renderer response, menu/resume and cleanup, with rendered game scenes. | Earlier failure is intermittent and its cause is unproven. These passes occurred without a configuration repair. Independent P2 support and complete gun controls remain open. |
| The House of the Dead III, classic Windows with Arcade Mod | Controlled comparison identified the exclusive fullscreen flag: `1` froze; changing only that flag to `0` passed. The automatic repair then passed a 60-second native check with the existing helper, borderless presentation, menu/resume and cleanup. GPU capture shows rendered Start/Freeplay output filling the monitor without window chrome. | Actual independent P2 shots, reload and path selection remain unverified. The installed executable fingerprint scopes the repair; no game binary was replaced. |
| The House of the Dead III, Chihiro | The process/window, menu/resume and cleanup checks pass as an ordinary user, but visual review reveals Error 11: JVS I/O board not connected. | A side-by-side native comparison using the matching Chihiro-DS loader reached normal intro/credits output without Error 11. It remains windowed and has no confirmed gun/helper mapping, so the original library entry was restored. No in-level acceptance. |
| The House of the Dead: Remake | Native launch, rendered menu, menu/resume and cleanup passed. Existing multiplayer input mode was confirmed. | Actual independent P2 actions and sustained play remain unverified. The two installed version-specific plugin DLLs deliberately self-select; their presence alone is not a conflict. |
| The Walking Dead Arcade | Both players joined an active level. Separate reticles and ammunition counters were visible; each player's shot reduced its own ammunition from 8 to 7. Both aim sweeps and packet isolation passed. | Hits, complete special-action coverage, sustained play and physical tracking remain open. Some attract-screen captures show overlapping image fragments; the in-level replay frames are clear. An initial cold launch was black; later launches rendered with and without the menu, so an overlay cause was not established. |
| Terminator Salvation | Follow-up replay used the mapped Reload to skip the cinematic, reached an active two-player mission and passed independent aim/button packet checks. Each player shot from 60 to 57 rounds without reducing the other player's ammunition, then reloaded from 57 to 60. | Grenade effects, confirmed hits, sustained play and physical tracking remain unverified. |
| Terminator 2: Judgment Day, MAME | Native launch, menu/resume and cleanup passed. Two virtual players entered gameplay; shots were visible and each mapped Bomb changed its own counter from 25 to 24. | Confirmed hits, complete aim accuracy, sustained play and physical tracking. |

The automatic TeknoParrot setup now detects stale `Use Relative Input` settings
and turns them off for assigned RS3 Reaper, Sinden, XGunner and Blamcon profiles
that provide that vendor field. This correction ran through the real House of
the Dead 4 launch path. It does not invent settings in profiles that lack the
field or alter the relative setting without an assigned supported gun.

The Dolphin input lab uses a separate emulator user directory, copies only
configuration INIs, redirects write paths and never copies original saves or
discs. Original configuration hashes and the library were unchanged after the
replays. Its scene recognizer is experimental and rejects scenes it cannot
identify rather than marking boot input as working gameplay.

Vendor guidance checked during diagnosis: [Dolphin accuracy profiles](https://github.com/ProfgLX/Dolphin-Lightguns-Accuracy-Inis),
[DemulShooter Windows game setup](https://github.com/argonlefou/DemulShooter/wiki/Windows-games),
[HOTD III Arcade Mod](https://github.com/argonlefou/Hotd3pc_ArcadeMod) and
[HOTD Remake Arcade Plugin](https://github.com/argonlefou/HotdRemake_ArcadePlugin).
No replacement commercial game executable was downloaded or installed during
these checks.

## Additional native and two-player checks

A further selected group ran on 2 November. Both Resident Evil Chronicles titles,
Alien3: The Gun, Aliens: Extermination, Silent Hill Arcade, Terminator Salvation
and the installed Dead Containment demo passed bounded native window,
menu/resume and cleanup checks. Fourteen House of the Dead launch entries also
completed that technical check. Visual review remains essential: the Chihiro
entry above shows a JVS error despite its responsive window.

| Game | Additional observation | Still open |
| --- | --- | --- |
| Alien3: The Gun, MAME | Credits increased independently. Two players entered the mission, shots were visible and each Bomb reduced its own remaining count. The English in-game guide visibly names Shoot and Bomb for both selected guns. | Full aiming accuracy, hits, sustained play and physical tracking. |
| Aliens: Extermination | The native and virtual two-gun replays completed; reviewed frames show an active two-player level and shots. Profiles and the library were restored unchanged. | Complete weapon effects, hit confirmation, sustained play and physical tracking. |
| Aliens Armageddon | A later native candidate passed renderer, foreground, menu/resume and cleanup checks. A pointed Story Mode selection reached an active two-player mission. Both triggers produced visible shots; P2 also increased its kill counter. | Grenades, reload effects, sustained play and physical tracking. The earlier timeout is retained as an intermittent failure. |
| Resident Evil: The Umbrella Chronicles | Native menu/resume and cleanup pass. In isolated NAND, pointed A confirms the first-run save prompt and a new test profile reaches Scenario Selection. | The replay must enter an actual mission before weapon actions can be accepted. |
| Resident Evil: The Darkside Chronicles | Native menu/resume and cleanup pass. In isolated NAND, pointed A confirms the first-run save prompt, creates a test profile and reaches Difficulty Select. | Mission entry and actual independent player actions. |
| Silent Hill: The Arcade | Exclusive fullscreen passes a short launch check but ignores the observed virtual Start input and can lose the renderer after the menu. A controlled windowed comparison passes two minutes with menu/resume. In a separate mission, both players shoot and reload with visible ammunition changes. | Confirmed hits, full aiming accuracy, contextual Option L/R effects, sustained play and physical tracking. |

The Silent Hill repair changes only the matching vendor Windowed field and
uses the existing monitor presentation path. Its native comparison preserved
the original profile exactly. The virtual replay also restored its temporary
cursor setting and both original gun assignments. A trial to reveal a hidden
fullscreen window directly did not solve the failure and was rejected.

The MAME pilot reads the emulator's effective output directories, preserves
calibration in isolated configuration/NVRAM copies, and checks original hashes
after each replay. It does not alter original saves or the game library. Its
reviewed scope is Alien3 and Terminator 2, without custom helper scripts.

The launch test now measures profile preparation separately from renderer
startup. Preparation has a three-minute limit; the renderer budget begins at the
actual profile-prepared event. A slow preparation cannot consume the entire
renderer-start budget, and both phases remain bounded with ordinary cleanup.

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

The [0.3.68 release](https://github.com/sthamann/deadeye-arcade/releases/tag/v0.3.68)
passed [775 Windows core checks and fourteen scene signal cases](https://github.com/sthamann/deadeye-arcade/actions/runs/41342976356).
The installer and optional Input Lab archive were checked against their published
SHA-256 digests before installation. The installed version matched release commit
`e07753f1c8141784a564e5d307b582773718c839`.

The installed package was tested after restoring EX's previous display value
from its retained backup. The ordinary launch path selected the supported native
1360 x 768, 60 Hz mode and configured windowed rendering. Full-monitor captures
show the actual game menu and the resumed game filling the output. The test
exercised the menu's real Resume command. Native mode selection and restoration
both returned success. No external test script resized the game window or
changed its display mode during this package check.

A separate replay using the published Input Lab delivered nine aim positions
and a monotonic 121-packet sweep for each virtual player. Coin, Start, Trigger
and Pedal press/release delivery and player isolation passed for both players.
The active profile audit resolved these required actions. Start reached Chapter
1; later action frames still showed the introduction. This is input-delivery and
scene evidence, not a certificate of successful shots, pedal effects or complete
two-player gameplay.

Earlier controlled comparisons established why a simple window resize was
insufficient: this EX renderer retains a fixed viewport. A process-only DPI
comparison did not correct it, and a nonactivating menu trial remained invisible.
Neither rejected approach is shipped. HOTD 4 Special retains its original display
selection; the EX-specific native-display rule does not apply to it.

The earlier complete-data checks passed both Lindbergh launch paths with `elf`,
`fs` and `drv` intact. EX Start reached its first chapter, while Special left the
cabinet ride-reset prompt and reached gameplay advice. Those checks still do not
establish in-level weapon effects or independent two-player gameplay for Special.

After both published-package checks, the library hash was unchanged, no virtual
test device collections remained and the frontend returned to foreground. Both
current and saved physical desktop modes were 3840 x 2160 at 60 Hz. Physical gun
tracking, recoil and complete playthroughs remain separate verification tasks.

The [0.3.69 package](https://github.com/sthamann/deadeye-arcade/releases/tag/v0.3.69)
passed [779 Windows core checks and 28 Input Lab checks](https://github.com/sthamann/deadeye-arcade/actions/runs/41412490642).
Its installer checksum was checked before installation, and the installed
assembly reports `0.3.69+478d62f8d213b092278085a30a5e4078f61da91c`.

The [0.3.70 package](https://github.com/sthamann/deadeye-arcade/releases/tag/v0.3.70)
passed [782 Windows core checks and 32 Input Lab checks](https://github.com/sthamann/deadeye-arcade/actions/runs/41426337445).
The published installer and Input Lab archive passed checksum verification before
installation. The installed assembly reports
`0.3.70+8cff6f4924a7f5f039e49e3acc265d5e9890561b`, and the installed Input Lab
self-check passed all 32 cases.

Starting from Silent Hill's previous Windowed=0 setting, the installed app changed
that field to 1 through its normal preparation path. It retained the existing
cursor setting and both physical Vyper assignments. With Remote Desktop
disconnected, the native check observed the renderer for 120 seconds, opened and
resumed the game menu, and removed all owned game processes on exit. Reviewed
captures show the menu and the resumed attract sequence filling the display.
The separate virtual replay above provides the observed two-player ammunition
changes; this package check does not itself establish hits or aiming accuracy.

After this final check, the library hash was unchanged and the frontend window
was observed again. Current and saved physical display modes were both
3840 x 2160 at 60 Hz. These are bounded technical checks, not complete playthroughs
or physical gun certification.
