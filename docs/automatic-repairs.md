# Automatic repairs

For a specific symptom, start with [Troubleshooting](troubleshooting.md).

Deadeye carries reviewed setup knowledge with the application. It checks the
selected game before dependency validation and launch. A full library check runs
after import and when **Check and repair library** is selected. Startup loads the
last saved results so a large collection cannot delay access to the frontend.
Checks run in the background so the native exit button remains responsive.

Open **Settings → Automatic repairs** to see detected problems, repairs from the
current check, recent profile changes and the catalog of known pitfalls. Both
English and German are supported. A second check leaves healthy files unchanged.

## What happens automatically

| Problem | Detection and response |
| --- | --- |
| Windows Terminal intercepts a game loader console | Requires a recent failed native focus test with a responsive renderer and a Terminal title matching the owned BudgieLoader executable inside that emulator directory. Backs up both per-user console delegation values and selects the Microsoft documented Windows Console Host preference for new consoles. A later user change is preserved. |
| Moved TeknoParrot game or second launcher | Repairs only an unambiguous existing library file; keeps an original XML backup. |
| Broken portable emulator storage/search paths | Adds the explicit Model 2 ROM folder, repairs inaccessible PCSX2 memory-card storage, and refreshes matching TeknoParrot vendor metadata. Existing accessible paths remain. |
| Missing OpenAL | Reads actual PE imports, checks x86/x64 compatibility, downloads pinned OpenAL Soft 1.25.2 from its official source, restores only an absent DLL and preserves COPYING. Ambiguous mixed architectures or incompatible existing DLLs require review. |
| Missing Hypseus support assets | Requires the exact Hypseus Singe 2.11.1 executable hash. Restores only absent reviewed images and `grumble.wav`; preserves LICENSE and existing assets. A healthy install needs no download. |
| P1/P2 input assignments | Uses the connected gun identities for supported TeknoParrot, DemulShooter, MAME, RetroArch, Flycast, Supermodel, RPCS3 and Dolphin adapters. Physical USB identities survive RDP; index-based adapters require actual visible RawInput mice. |
| Library repair differs from launch | Both call the same file preparation for managed adapters and installed multiplayer plugins. Repairs include helper configuration in the change history. Mouse-index profiles are retained when RDP hides the real enumeration and refreshed on a local launch. |
| After Dark steering and seat stop | Matches exact vendor action and routing identifiers. Prepares P1 Left/Right for steering and Up for the seat-stop input, keeping separate gun triggers, credits and Start. Missing captured directions remain a setup error. |
| Music GunGun 2 selection | Prepares Decision on P1 Start and shared left/right selection on P1 directions using exact vendor inputs. Unreviewed shared Select/Enter actions remain visible for review. |
| Alien3 trigger-to-start cabinet | The exact alien3 MAME recipe accepts both the selected player’s Shoot and Start actions on its real trigger input. Reads back the combined route, omits unused generic Start ports and keeps independent coin, bomb and aim inputs. |
| An arcade loader’s own classic console takes focus | During an active TeknoParrot session, return from a ConsoleWindowClass surface to a visible FREEGLUT renderer only when both belong to the same nonzero process ID. Preserve the console for diagnosis, unrelated terminals, modal dialogs and the game menu. Count only verified activation. |
| Transformers: Shadows Rising stays behind its launcher | Tracks the owned `Sega/Transformers2/Transformers2.exe` renderer instead of `Sega/shell/Shell.exe`. Window readiness, startup guide, menu return and natural exit use the actual game. The exact vendor profile and directory layout are required. |
| Hidden GameInput focus theft | When no game menu is open, restore the active renderer only from an invisible, untitled GameInputServiceWindow owned by the exact Windows service process name. Visible apps and prompts retain focus. Activation must be verified; the native test is not allowed to label an unfocused game as passed. |
| HOTD III ArcadeMod controls | With the installed matching launcher and DemulShooter route, selects keyboard mode for both players and writes their actual Start scan codes. Reads back trigger, reload, path selection and Start. Vyper magazine pull temporarily emits the helper's expected right mouse button. |
| Vyper Move Cross missing | RPCS3 uses D-pad Down when the Vyper selector has no click assignment. Trigger, A, B, magazine, Start and Select keep separate routes. |
| Unused RetroArch button errors | Reads the core's own GunCon, Zapper, Saturn or Genesis input contract. Does not require GunCon C/Start or invent a second Zapper. Content overrides remain unverified. |
| Supported Dolphin accuracy profiles | Matches the disc region to the reviewed accuracy package, preserves learned aiming offsets and prepares independent RS3 DSU channels. Extraction uses Hybrid Ubershaders. |
| Gun reconnects during frontend use | Resolves selected players from fresh USB discovery at launch. A changed interface path is recovered through its physical container identity, preserving the slot and learned controls. A stale frontend cache cannot remove P1 before setup. An unavailable selected P1 is reported without clearing the game's gun assignments. |
| Required game controls | After preparation, reads back supported game profiles before starting the game process. Missing required actions or unreachable buttons produce a specific setup error. Aim and button tests remain optional. A readable mapping is separate from a confirmed gameplay response. |
| Installed multiplayer plugins | Prepares the installed HOTD 1 ArcadePlugin's multiplayer input mode when its `hotdra` helper route is present. Reads its fixed Start keys and actual helper assignments for the controls legend. Configures recognized HOTD 2: Remake and installed Blue Estate patch inputs according to their actual device format. Unknown plugin versions retain their own workflow. |
| Supported game presentation | Uses native resolution and the normalized fullscreen route for absolute RawInput guns in supported Aliens profiles, avoiding a scaled-desktop conversion that can confine aim to a quarter of the screen. Relative input keeps its windowed route. Model 2 presentation is also prepared; changed settings are recorded. |
| Silent Hill Start input and menu resume | The matching SilentHill vendor profile uses the reviewed windowed route. Library repair and launch share this correction, retain resolution and gun bindings, and keep the original backup. Duplicate vendor fields require review. |
| Old HOTD 4 Special loader | Uses the matching installed ElfLdr2 vendor template for this exact game. Retains user bindings, game path and existing settings; adds only missing defaults. Keeps the original profile backup. |
| Incomplete Lindbergh ASCII copies | Keeps sibling `fs` and `drv` data beside `elf/*.elf` when preparing Unicode game paths. Revisits earlier prepared paths, preserves changed settings and game-created saves during the upgrade, and retains original game files. |
| Existing Lindbergh shared libraries | For the specific supported ElfLdr2 profiles, copies only absent, valid ELF libraries from the installed game into the loader search folder. |
| Duplicate launches / difficult exit | Launch reservation and process ownership are built into every session. Start + Coin is the independent emergency gesture; a trigger hold is optional because autofire can interrupt it. |

## Problems that require a real next step

The app identifies missing required game files, TeknoParrot vendor-access and
administrator requirements, and PCSX2 Guncon2 ports sharing the same pointer.
It keeps these visible instead of inventing a working two-player configuration.
Microsoft runtime inspection remains integrated; available installers come from
Microsoft and are checked for architecture and signature. Windows consent,
installer license prompts, vendor login and physical calibration can still require
user interaction.

Input preparation failures remain visible in the saved control audit and the
repair findings. An existing readable profile cannot hide a failed update or
unavailable accuracy package. A successful later check clears that game's error
and retains the other library audits. A selected-game check reports only that
game's findings, even when the saved audit also contains other titles.

Cancelling a launch propagates to queued repair checks, official repair-package
and Dolphin accuracy downloads, and MAME metadata queries. Temporary repair
downloads are removed, and a cancelled operation is not labelled as a completed
repair or a new repair failure. MAME query cancellation stops its query process.
Ordinary file operations and gun settings restoration finish through the existing
cleanup path.

Deadeye does not replace game binaries, bypass licenses, restore quarantined
executables or disable endpoint protection. A profile correction establishes a
configuration change. A real hit, successful campaign and independent P2 controls
still need a physical gameplay test.

## Display and Remote Desktop

The frontend and desktop-gun companion record current and saved Windows display
modes and distinguish a remote session from a local session. The companion keeps
observing transitions while the frontend is closed, when desktop crosshairs are
enabled. It writes only when the display state changes. The Settings view labels
RDP modes explicitly; they do not establish the physical TV's refresh rate.

`display-report.json` and `display-history.jsonl` support checking the actual local
mode after an RDP disconnect. The app does not force a guessed refresh mode or
install a graphics driver based on an RDP-only measurement.

## Structured knowledge and local records

The shipped [catalog](../src/Reaper.Core/KnownFixes.json) contains each rule's ID,
name, scope, source, automation mode and verification boundary. Implementations
are reviewed application code; the catalog cannot execute scripts or arbitrary
commands. New reviewed rules ship with normal app releases and self-updates.
Official support archives are cached locally and must match their pinned hash
and size before use. Downloads have bounded time and size limits.

Under `%LOCALAPPDATA%\ReaperArcade`:

- `fix-report.json`: latest combined repair and control findings shown in the app.
- `fixes-report.json`: latest library inspection.
- `fixes-<game-id>.json`: latest selected-game inspection.
- `fixes-history.jsonl`: actual repaired files and launch-time profile changes;
  profile changes include before/after SHA-256 hashes.
- `repair-packages/`: verified support archives.
- `display-report.json` / `display-history.jsonl`: measured session modes.

The last 50 recorded repair/configuration events appear in Settings. Diagnostic
export includes the current repair report and recent history. Reports are local;
no library, device identities or game paths are uploaded by repair checks.
With the frontend closed, the existing app also provides:

```powershell
ReaperArcade.exe --repair-library
ReaperArcade.exe --repair-game <game-id>
ReaperArcade.exe --inspect-display
```

## Verification

Core checks cover real path repair with backup, repeat-run idempotence, privilege
and vendor-access detection, shared versus independent PCSX2 pointers, package
hash/size validation, preservation of existing assets, rejection of absent archive
entries, actual changed-file hashes and ambiguous OpenAL architectures. Optional
checks exercise both exact official archives on fresh temporary installations.
The browser checks cover the native repair command, findings/history, both
languages and the distinction between remote and physical display modes.
Additional regression checks exercise Silent Hill through the normal library
repair, unchanged repeat runs, ambiguous fields, cancelled package preparation,
failed input preparation over an otherwise complete profile, and successful
recovery without losing unrelated audits.

## Beetle Saturn gun actions

For the reviewed Stunner/Virtua Gun ports, the legend and audit use the
[core's actual Trigger, Reload and Start inputs](https://docs.libretro.com/library/beetle_saturn/#lightgun).
Unused generic A/B/C or D-pad bindings no longer create false missing-action or
shared-key warnings. A missing Reload entry and a genuinely shared Start key
still require action. Core/content overrides and physical game response remain
separate checks.

## Classic HOTD III fullscreen hang

The `hod3-presentation` rule is limited to the reviewed installed game executable
and the Arcade Mod launch path. An isolated native comparison froze with the
mod's `Fullscreen:1`, then passed renderer response, menu/resume and cleanup
with only that value changed to `0`.

Deadeye retains `Hod3Arcade.ini.before-deadeye-presentation`, switches that known
build to windowed rendering and uses its existing borderless renderer handling
to fill the physical monitor. Unknown executable versions are preserved. This
repairs presentation; independent shots, reloads and other game actions still
need their own verification. No replacement game executable is installed.


## HOTD EX fixed viewport

The `hotdex-native-display` launch rule matches the `Hotd4ex` ElfLdr2 profile
and `hodexRI.elf` executable metadata. This renderer retains a fixed 1360 × 768
viewport when its window is enlarged. Its fullscreen route can also lose the
correct presentation when an activating game menu opens.

On one physical Windows display offering a progressive 1360 × 768, 60 Hz mode
with the current colour depth, Deadeye temporarily selects that mode, backs up
the profile, uses windowed rendering and fills the monitor through the existing
borderless presentation path. The original current mode is restored after the
owned game processes stop, including cancellation and launch failure. No saved
Windows display setting is changed. Mode selection and restoration results are
recorded in `native-display-history.jsonl`; profile changes also retain their
normal hashes in repair history.

Remote Desktop and multiple-display sessions are excluded from automatic mode
selection. A new Remote Desktop connection owns its own display; Deadeye leaves
that connection untouched. Other Lindbergh profiles, including HOTD 4 Special,
do not inherit this rule. Fullscreen/menu evidence does not certify independent
aiming, successful shots or physical recoil.

## Console host compatibility

Microsoft documents the paired console preference in its [Windows Terminal guide](https://learn.microsoft.com/en-us/windows/terminal/group-policy#default-terminal-application). The repair changes the default host for new consoles for the current Windows user, including consoles opened outside Deadeye. It requires the specific game failure described above. Merely opening Terminal does not trigger it.

The original values, including an originally absent value, are saved locally in `console-preference-before-deadeye.json`. You can also choose your preferred host in Windows Terminal → Settings → Startup → Default terminal application. Deadeye preserves a later change and reports that manual attention is needed instead of repeatedly replacing your preference. This repair needs a fresh game test; applying it does not certify gameplay.
