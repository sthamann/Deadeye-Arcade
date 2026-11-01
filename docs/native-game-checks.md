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
