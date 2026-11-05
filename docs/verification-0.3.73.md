# Input preparation corrections

The ordinary frontend launch filtered saved player bindings through its cached
discovery list before calling the shared session. The separate game test passed
all saved bindings to fresh discovery. That discrepancy could remove a selected
player before the common setup code had a chance to see the current USB device.

The frontend now passes the selected slots directly. Shared discovery resolves
changed interface paths through the same physical container identity, retaining
the player's controls and slot. An unavailable selected P1 stops preparation
before its game assignments are cleared. Per game input logs record the selected
players and any exclusions by fresh discovery.

The regression for a connected gun with a changed USB interface failed before
the correction and passed afterwards. The local suite passed 793 meaningful core
checks. Sixteen combinations exercise all four model profiles in both player
slots using Walking Dead's Start, trigger, reload and aim action identifiers.
These tests write real profile files and read back each player's device and
reachable action routes. Learned inputs in a test fixture are not proof of a
specific physical device's firmware settings.

The installed HOTD 1 ArcadePlugin route now prepares `InputType=1`, preserves the
other options and records its backup. Its control legend reads the helper's
actual device assignments, fixed P1/P2 Start keys and mouse inputs. Vyper magazine
reload uses the same temporary right mouse channel as other fixed mouse plugins.
The original menu profile is restored after the game. This recipe applies only
to the first remake with its existing ArcadePlugin and `hotdra` helper route.

Supported profiles are read back before the game process starts. A known missing
required action stops launch with its player and action name. Incomplete or
unknown action catalogs remain marked for review. Optional aim and button tests
are not prerequisites for play.

The earlier installed package was inspected on the Windows machine. Its saved
Walking Dead profile had empty P1 entries; HOTD 2 assigned only P2; the first
remake's helper had P1 set to disconnected. Fresh separate-session preparation
restored both selected Vyper assignments. Those observations establish the
configuration defect and the different launch paths, not successful shots.

The subsequent bounded native observations did not complete gameplay acceptance.
The first remake reached its Freeplay start prompt. Walking Dead timed out and
HOTD 2 reported an unresponsive renderer; the capture process also opened a
foreground console, so these runs cannot establish a game-specific root cause.
They provide no new aiming, hit or two-player acceptance. A new package still
requires an installed native test and a physical gameplay check.
