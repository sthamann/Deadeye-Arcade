# Deadeye Input Lab

An optional Windows diagnostic tool for replaying lightgun input in a real game.
It creates two independent absolute HID mice with keyboard collections, reuses
the assigned guns' learned mappings and launches through Deadeye's existing
`GameSession`. It does not use `SendInput` as a substitute for two physical devices.

The TeknoParrot adapter runs direct absolute RawInput profiles without external
helpers. Experimental adapters cover Alien3 and Terminator 2 in MAME, and
Extraction, Overkill and both Resident Evil Chronicles families in Dolphin.
Dolphin uses the production DSU bridge with an isolated emulator user directory.
The MAME adapter keeps its controller profiles, cabinet configuration and NVRAM
in a separate directory. Primeval Hunt is excluded. HOTD EX requires both shared
loader libraries to be present before replay. Other launch paths remain
unsupported until their changed files have a reviewed restoration recipe.
`plan` can read the existing controls for any game.

## Build

From the repository root, run `./scripts/build-input-lab.ps1`. The script downloads
HIDMaestro 2.1.0 from its official release, checks the pinned archive SHA256 and
preserves its MIT license and third party notices. It builds a separate portable
tool. The normal frontend does not acquire a driver dependency.

## Use

Read the plan without installing anything:

```powershell
Deadeye.InputLab.exe plan inventory-35
```

Close the frontend and use an elevated console for a live test:

```powershell
Deadeye.InputLab.exe run inventory-35 --output C:\Lightgun\Tests\InputLab\Aliens
```

The tool refuses to install a missing driver during an ordinary run. The optional
`--install-driver` flag permits HIDMaestro's installation, which creates a local
code signing certificate in LocalMachine My, Root and TrustedPublisher and deploys
its UMDF driver. This is an explicit operator decision for the test machine.
It is not part of game setup or normal frontend startup.

Keep other applications closed during replay. The tool stops when the game loses
the foreground. Press F12 or Ctrl+C to abort. Administrative service, volume and
exit controls are excluded from gameplay replay.

## Evidence and restoration

Screenshots use DXGI Desktop Duplication on the game's monitor. Tests should run
in the local console session; unsupported GPU capture fails explicitly.

For launch checks of games without a replay adapter, capture the current monitor:

```powershell
Deadeye.InputLab.exe capture --output C:\Lightgun\Tests\Capture
```

This command creates no virtual devices, opens no game and changes no profiles.
It records the foreground process, physical output size and Remote Desktop state
alongside the GPU image. Pair it with the frontend's existing `--test-game`
command to inspect a running game and its overlay. These observations cover
presentation and launch behaviour; they do not verify gun input in that game.

The output directory contains the original profile, a restoration manifest, the
controls read from the launched profile, RawInput observations and screenshots.
The tool checks both ends and the middle of each aiming axis, and the actual
press and release packets for replayable game actions. Coin and Start run first
after a recognized menu or game scene appears. A renderer
that remains on a loading or unknown scene is reported as a failure. Neutral aim
reports are sent during boot, as they are by connected tracking guns.

Aliens Extermination and Jurassic Park have a reviewed 30-second mission vote.
The replay waits for that timer and uses each player's mapped trigger to dismiss
the briefing before its aiming grid and weapon sequence. The resulting scene is
captured for review. Actions have before, held and released screenshots. The
mission warmup is skipped by the explicit input-route-only probe.

Scarlet Dawn, Raccoon Rampage and Tomb Raider have additional selection warmups
based on observed menu flows. The tool records those steps and the resulting
scene for review. It also recognizes the observed OCR spellings of Raccoon's
start prompt and Tomb Raider's `Insert credits` prompt. Loading-screen rejection
still takes precedence over menu text. A completed warmup does not itself prove
that a level started or that both players joined.

Each player also receives a 121-point horizontal sweep. Reported delivery timing
is measured between virtual HID submission and this tool's RawInput receiver.
It does not measure camera tracking or rendered frame latency.

The original game profile is restored when the prepared file is still unchanged.
An intervening edit is retained and reported for review. The library is read only,
and its hash is checked again after the test. Virtual test devices are disposed.

**Input delivery and gameplay response are separate results.** Receiving a
RawInput packet proves the Windows device path. A screenshot records the scene,
but does not by itself prove that a shot, weapon switch or second player worked.
Those observations need a per-game visual check or a reviewed game-specific
measurement. The tool never marks an entire game or all four gun models verified
merely because its replay sequence finished.

For a temporary comparison of the windowed Aliens input route, use
`--windowed-probe`. This is restricted to the Aliens Extermination profile and is
recorded as a diagnostic override. Its profile change is restored by the same
hash-checked cleanup. Ordinary tests use the application's normal presentation.

The reviewed Aliens emulator build also supports `--input-route-probe`. This
explicitly skips scene readiness and measures the emulator's read-only JVS input
exchange while replaying input. The report labels the scene unverified. It checks
that each player's axis span reaches the profile's limits and that each action
sets its expected emulator button bit and clears it on release. A mismatch fails
the run. An unknown emulator build is refused. This establishes input routing
through the emulator, including the scaled-window regression, without treating a
loading screen as working gameplay.

## Capture and replay limits

The GPU capture backend requests BGRA8 output, skips pointer-only updates and
rejects protected or unexpected surfaces. This avoids misleading black captures
on HDR/fullscreen output. Capture metadata records the process, output dimensions
and whether the session is remote.

The reviewed arcade recipes insert credits before Start. The observed Tomb
Raider cabinet needs four coin pulses per player; it still requires a verified
transition into gameplay. Warmup completion and a received Start key cannot
establish that transition. Generic replay keeps `gameplayVerified` false.

[Additional native game observations and unresolved issues](../../docs/native-game-checks.md)

## Experimental isolated Dolphin replay

`run` has an experimental adapter for the Dead Space: Extraction, House of the Dead:
Overkill, Resident Evil: The Umbrella Chronicles and The Darkside Chronicles
disc families when their accuracy profile and two gun assignments are
available. It copies configuration INIs into a new `dolphin-user` directory beside
the report, redirects NAND and memory-card paths there and disables SD-card
writes. It does not copy the disc or existing saves. Original configuration files
and the game library are checked again after the run.

The pilot feeds the virtual devices through the actual RawInput receiver and
`GameSession` into Dolphin's production DSU bridge. It records subscriptions and
frames sent to each player slot, resolves required actions into physical button
chords, and checks their press/release and player isolation. Boot observations
handle the reviewed Wii notices and wait for a recognized title prompt before
the aim grid and action replay. Missing subscriptions or an unknown boot scene
fail explicitly. The Chronicles pilot confirms only the observed Create Data
prompts and selects a new profile inside the isolated NAND. It never operates
on existing player saves.

The original Extraction and Overkill transport pilot received both controller
subscriptions and delivered independent inputs. Its scene-gated runs stopped on
unrecognized boot/title images. Automatic gameplay entry requires a reviewed
scene in each title; the adapter must not be presented as a completed gameplay
test. `read-scene IMAGE` reads OCR from an
existing image without creating devices or launching a game.

These checks establish input transport and retain images for review. A title
screen or matching DSU frame does not establish an in-level hit, reload, weapon
selection or successful two-player gameplay. Tracking cameras and recoil remain
outside this test.

## Experimental isolated MAME replay

The MAME pilot accepts the `alien3` and `term2` ROM identifiers, without helpers
or custom launch scripts. It reads the installed emulator's effective output
directories, copies the selected cabinet configuration and NVRAM, and redirects
configuration, NVRAM, save states, screenshots, recordings, disk differences and
debugger comments into the report's `mame-user` directory. Existing calibration
values are retained in the copies. ROMs are not copied and autosave is disabled.

The normal production launch code builds the controller mapping for the two
virtual devices. The pilot resolves each required action from that effective
mapping, sends Coin and Start first, then replays the aiming grid, sweeps and
buttons. Captures and RawInput events are separate evidence. Original
configuration, NVRAM and library hashes are checked after cleanup. Linked paths,
ambiguous output directories and configuration copies larger than 32 MB are
refused. A completed replay still needs visual review of movement, shots and
secondary actions in the game.

For a deliberate long-press comparison, use `--hold-ms 1000` with `run`. The
accepted range is 50–2000 milliseconds. The requested duration is recorded in the
report; it does not change the physical guns' firmware or normal game controls.

The reviewed mission timers also cover Terminator Salvation and Walking Dead.
Wait for the post-timer scene before interpreting the later weapon actions.

Armageddon's reviewed replay aims at Story Mode before its warmup and weapon
actions. Captures must show the subsequent mission; selecting a menu tile is
not a gameplay result. Capture retries after access denial are limited to three
seconds on the existing Default desktop, without switching desktops or changing
security settings. A persistent refusal remains a failed capture.
