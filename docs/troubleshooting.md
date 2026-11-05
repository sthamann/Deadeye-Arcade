# Troubleshooting

Find the symptom below, follow its steps and check the result before changing
another setting. The interface names here use English; German is available in
**Settings → Language**.

## Start here

1. Open **Settings → App updates → Check for updates** and install an available
   update after ending the current game.
2. Open **Settings → Automatic repairs → Check and repair library**. Expand
   **Results and next steps** to see what changed and what still needs attention.
3. For aiming or physical button problems, test directly at the connected screen.
   Remote Desktop is useful for setup, but it changes the display and input session.

Deadeye also checks the selected game before launch. A library repair leaves
healthy configurations unchanged and keeps backups for supported corrections.
[What automatic repair covers](automatic-repairs.md).

## Find your symptom

| Symptom | Go to |
| --- | --- |
| A gun is connected but buttons do nothing | [Connection and button input](#gun-connected-but-buttons-do-nothing) |
| Vyper buttons work but aiming does not | [Vyper tracking](#vyper-buttons-work-but-the-pointer-does-not) |
| The desktop pointer works but game aim is wrong | [Game aiming](#desktop-aim-works-but-game-aim-is-wrong) |
| Both guns control the same player | [Player assignment](#both-guns-control-the-same-player) |
| Coin, Start, reload or weapon change is missing | [Button mapping](#coin-start-reload-or-another-action-is-missing) |
| Testing a button triggers a menu or system action | [Button testing](#testing-a-button-triggers-another-action) |
| Start is disabled or the game does not launch | [Launch preparation](#start-is-disabled-or-the-game-does-not-launch) |
| A DLL or runtime is missing | [Dependencies](#a-dll-or-runtime-is-missing) |
| A trigger hold does not open the menu | [Leaving a game](#the-trigger-hold-does-not-open-the-menu) |
| The game is cropped, windowed or hidden by its launcher | [Presentation](#the-game-is-cropped-windowed-or-hidden-by-its-launcher) |
| A game closes or returns to the library | [Unexpected exits](#a-game-closes-or-returns-to-the-library) |
| A title shows JVS Error 11 or a vendor access error | [Arcade access and loaders](#jvs-error-11-or-a-vendor-access-error) |
| Input feels slow after Remote Desktop | [Local display and latency](#input-feels-slow-after-remote-desktop) |
| The app will not open, or the desktop becomes sluggish | [Startup and performance](#the-app-will-not-open-or-the-desktop-becomes-sluggish) |
| An update, installer or game is blocked | [Downloads and Windows messages](#an-update-installer-or-game-is-blocked) |
| Covers, videos or years are missing | [Library media and ordering](#covers-videos-or-years-are-missing) |

## Gun connected but buttons do nothing

USB detection, configured software, received button input and working aim are
separate checks. A wireless receiver can be present while its gun is asleep.

1. Open **My Guns**, wake the wireless gun if applicable and select the intended
   device for P1 or P2. Extra guns can **Sit out**.
2. Start the button test and press and release the trigger, Start, Coin/Select and
   reload. Check whether the corresponding controls highlight.
3. If a control sends an unexpected signal, use **Capture** for that physical
   control, then **Bind** to choose its action. End the test before playing.
4. After Windows unlock, resume or an RDP transition, allow detection to refresh.
   Deadeye renews its input registrations automatically. If input remains absent,
   close Deadeye and reopen it at the physical screen. Reconnect the affected USB
   device if it still sends no events.

For a wireless gun, check its receiver pairing, battery and manufacturer utility.
[Model setup and controls](lightgun-setup.md).

## Vyper buttons work but the pointer does not

Buttons prove that an input interface is working. The camera must also see the
correct IR arrangement.

1. In **My Guns**, use **Restore mouse pointer** for the affected Vyper.
2. Open Blamcon ARC and match Tracking to the actual emitters:
   **2 Up 2 Down** for four corners; **Crosshair** for emitters at the middle of
   the top, bottom, left and right edges.
3. Inspect ARC Live View from your playing position. All four emitters must fit
   in the camera view. If four appear only when standing much farther back, check
   lens coverage and the wide angle lens arrangement for your hardware.
4. Close Live View, calibrate the selected onboard profile and test the desktop
   pointer before testing a game.

If just one emitter is visible even when each is checked individually at short
range, investigate the emitters, their power and the camera view first. Changing
game button mappings will not restore missing camera points.
[Vyper setup](lightgun-setup.md#blamcon-vyper).

## Desktop aim works but game aim is wrong

When the desktop reaches every edge but a game reaches only a quarter of its
screen, investigate that game's coordinate conversion and input profile first.

1. End the game and confirm the correct physical guns are assigned in **My Guns**.
2. Run the library repair, then launch the entry through Deadeye so its supported
   profile corrections are applied.
3. For supported **Aliens: Extermination** and **Aliens Armageddon** profiles,
   Deadeye prepares the reviewed absolute RawInput display route. For **HOTD EX**,
   its fixed viewport requires a supported native display mode on a single local
   monitor. These rules apply to their matching profiles.
4. For Dolphin, check the actual game disc region and the resulting title
   profile. An unsupported disc ID keeps its existing settings. Check required
   actions such as Wii Remote and Nunchuk gestures as well as aim.

If the desktop pointer is already wrong, return to the gun's tracking and
manufacturer calibration instead. Record the display resolution and scaling when
reporting a game that still has a restricted aim area.
[Calibration and game profiles](compatibility.md).

## Both guns control the same player

1. In **My Guns**, use **Use for P1** and **Use for P2** on two different physical
   guns. Selecting the other player's gun swaps their slots.
2. Check the header: both active slots should show the intended models. A gun's
   hardware player number and its game slot are separate settings.
3. Launch through Deadeye to refresh the supported game profile. Check that both
   players have their own aim, Start/Coin and required action routes.
4. Test each player separately, then together, inside the game.

Windows has one shared system click pointer. Two guns moving that desktop
pointer does not establish that they share a player inside a game. Conversely,
two connected devices do not create local multiplayer in a game that lacks it.

PCSX2 ports sharing one pointer and shared keyboard codes in supported RetroArch
paths are reported for review. Matching multiplayer plugins are required for
games that use them; an unrelated patch is not a substitute.
[How gun profiles reach games](lightgun-setup.md#how-gun-profiles-reach-your-games).

## Coin, Start, reload or another action is missing

1. In **My Guns**, check the affected physical button in the live input view.
   **Capture** learns its signal; **Bind** assigns its function.
2. Run **Check and repair library**, then launch the game again. Read the startup
   controls reference or open the game menu to see the resulting game mappings.
3. Check additional actions required by that title, such as weapon selection,
   grenades, pedals or Wii gestures. Test them for each supported player.

A failed profile write or unavailable accuracy package remains visible in the
control audit and repair results. A later successful check clears that game's
error. An older readable mapping does not hide a failed update.

Some physical controls share one electrical input. The Vyper magazine and an
optional pedal can share a channel; the documented XGunner magazine release is
mechanical. These cannot be treated as independent buttons without distinct
received signals. [Physical button reference](HARDWARE-CONTROLS.md).

## Testing a button triggers another action

Use **Start button test** in **My Guns** to suppress that gun's normal frontend
navigation actions during the test. Other guns and **Close app · Windows** remain
available.

Shortcuts implemented in the gun's firmware or manufacturer software still apply.
Use short presses with a full release between controls. For a control that emits
several signals, use guided capture before changing its action assignment.
[Gun Studio walkthrough](gun-studio.md).

## Start is disabled or the game does not launch

Aim and button tests are optional. Completing them is not a requirement to start
a game in the current app.

1. Check whether a game session, installation or another launch is still active.
   End the owned game or let its cleanup finish before starting again.
2. Open **Game details** and inspect setup findings and required files. Run the
   library repair after correcting a moved or missing path.
3. If Deadeye reports a required runtime, install the matching package listed in
   Settings, then launch again. Complete any required restart first.
4. Close an already open TeknoParrot interface or configured helper before
   Deadeye starts its own session. Repeated clicks will not create another owned
   launch while one is reserved.

If Start is still disabled after a fresh app start, save diagnostics and include
the visible status. Deleting the library is not part of this recovery path.

## A DLL or runtime is missing

Open **Settings → Games and emulators: required runtimes**. Install the packages
listed as missing and finish any Windows consent, installer terms or requested
restart. Deadeye checks supported Microsoft packages for architecture and
signature.

Examples include Visual C++ runtime DLLs, legacy DirectX components and supported
.NET runtimes. A 64 bit Windows installation can still run a 32 bit game that
needs the corresponding x86 package. OpenAL and reviewed Hypseus support assets
have separate automatic repair rules.

An unrecognized DLL or a component loaded dynamically may need a title specific
check. Use the exact filename from the error. Obtain the matching runtime or
vendor component rather than a standalone DLL from an arbitrary download site.
[Dependency and repair coverage](automatic-repairs.md).

## The trigger hold does not open the menu

Use the assigned **Start + Coin/Select** buttons together for about **two seconds**
to end the game. **F12** is the keyboard alternative. Release both buttons before
holding the combination again; in the frontend, a fresh hold closes Deadeye.

The optional menu gesture needs a continuous trigger hold of at least ten seconds,
followed by release. Autofire or a game's input mode can interrupt that held
signal. **F10** opens the menu from a keyboard.

If the emergency combination does not register, first verify both physical
signals and their Start/Coin assignments in the button test. The game menu offers
Resume, Restart and End game; opening it does not pause gameplay.
[Menu and exit controls](in-game-overlay.md).

## The game is cropped, windowed or hidden by its launcher

End the session, run the repair and start the game through Deadeye again. The app
tracks the owned renderer and applies presentation rules for matching profiles.

| Title or renderer | Reviewed behavior |
| --- | --- |
| Silent Hill Arcade | Use the existing vendor windowed setting, then fill the monitor. Keep resolution and gun controls. Duplicate Windowed fields produce a finding for review. |
| Classic Windows HOTD III with the reviewed Arcade Mod build | Back up the mod configuration, disable its affected exclusive fullscreen setting and present the owned renderer across the monitor. |
| HOTD EX | Temporarily use its supported native 1360 × 768 at 60 Hz on one physical display, then restore the previous current mode on exit. RDP and multiple displays are excluded from automatic mode selection. |
| Supported Aliens profiles | Prepare the correct display route for the configured input mode. |

If the controls guide reappears over an already running game, or the launcher
stays in front, record which windows are visible and whether the issue followed
menu use. Include the exact game and emulator build. A windowed rule from another
game is not a general fullscreen fix.
[Presentation repairs](automatic-repairs.md) · [Native observations](native-game-checks.md).

## A game closes or returns to the library

Note the time, the last action and whether the game menu appeared first. A menu
opened while shooting is a different observation from a game process crashing.
If the menu is still open and you want to continue, choose **Resume**.

For a completed owned session, `last-session-<game-id>.json` records observed
runtime exit codes and whether a stop was requested. Keep this report and the
relevant part of `activity.log`. A short successful start check does not establish
that the game is stable through a whole level.
[Session and library checks](library-validation.md).

## JVS Error 11 or a vendor access error

**Chihiro HOTD III: JVS I/O board not connected.** This needs a compatible
Chihiro loader and its matching input/helper configuration. A responsive error
screen is not successful gameplay. Complete automatic gun setup for this case
remains unresolved in the documented checks; changing a display flag does not
provide the missing cabinet I/O integration.

**TeknoParrot serial expired, missing or access required.** Sign in and verify the
required access in TeknoParrot for that exact title. Deadeye detects a vendor
access requirement but cannot establish a valid subscription from a profile flag.
If the profile requires administrator rights, the elevated launch needs Windows
consent as well. [Known findings](automatic-repairs.md#problems-that-require-a-real-next-step).

## Input feels slow after Remote Desktop

Disconnect RDP and test directly at the physical screen. Check the local monitor's
actual resolution and refresh rate in Windows Advanced display settings. The
remote session's 30 or 32 Hz mode does not establish the TV's local refresh rate.

Deadeye records current and saved display modes separately and labels remote
sessions in Settings. `display-report.json` and `display-history.jsonl` help check
the transition back to the local screen. The app does not install a graphics
driver or select a guessed refresh mode from an RDP measurement.

If local play is still slow, record whether the desktop pointer, the firing range
and the game are all affected. For Dolphin, include the game disc ID and active
profile. [Display diagnostics](automatic-repairs.md#display-and-remote-desktop).

## The app will not open or the desktop becomes sluggish

1. Record the exact startup message. Check Task Manager for an already running
   Deadeye instance and for the process consuming CPU or memory.
2. If the frontend responds, end its game and use **Close app · Windows**, then
   open Deadeye once. Run the current installer if application files are missing.
3. Record whether the desktop remains slow or its icons keep flickering after
   Deadeye closes. Include the responsible process if Task Manager identifies it;
   do not infer a cause from flickering alone.
4. Keep `%LOCALAPPDATA%\ReaperArcade\activity.log` and any exact Windows message.
   This also helps investigate a reported server startup failure.

Portable users need the complete extracted package, including `web/`. The normal
installed app does not require a separate local web server. Setup keeps user data
outside the program directory. [Installation details](updates.md).

## An update, installer or game is blocked

For a download error, open **Settings → App updates** and read the reported
reason. Retry **Check for updates** when connectivity is restored, or obtain the
matching installer from [GitHub Releases](https://github.com/sthamann/deadeye-arcade/releases/latest).
Portable users should extract the complete archive into its own folder.

A checksum mismatch is a failed verification. Keep the existing installation and
retry the official release download. Deadeye does not install that failed file.

If Windows names SmartScreen, Smart App Control or another application control
policy, record its exact message and affected filename. The current application
installer is unsigned. A policy block is a separate issue from a missing runtime;
reinstalling DLLs does not resolve it. On a managed PC, involve the administrator.
[Update verification](updates.md) · [Failure reporting](library-validation.md).

## Covers, videos or years are missing

Open **Game details** and check the entry's media and information. Media files
must exist at their configured paths and remain accessible to the Windows account.
Select local media or use the supported cover download setup; games and original
artwork are not bundled with Deadeye.

The library places your saved preferred selection first, newest first within
that group. Remaining titles follow by release year. Favorites and platform
filters still restrict the visible selection. A prototype without a known release
year has no year label and follows dated titles within its group.

## Report a problem

In **Settings**, select **Save diagnostics**. Include:

- The app version, game title and emulator or game build.
- The selected gun models and which player is affected.
- Whether it happens at the physical screen, over RDP or in both places.
- The exact error, time and steps that reproduce it.
- Whether the desktop pointer and firing range work, and whether the other
  player works.

Local reports live under `%LOCALAPPDATA%\ReaperArcade`. Useful files include
`activity.log`, `game-input.log`, `fix-report.json`, `fixes-history.jsonl`,
`controls-audit.json`, `display-report.json` and `last-session-<game-id>.json`.
Some reports exist only after their corresponding check or game session ran.

Diagnostics can contain device IDs and local paths. Review them and remove private
details before attaching them to a [public issue](https://github.com/sthamann/deadeye-arcade/issues/new).
API keys are excluded from the app's diagnostic export.

[Back to the README](../README.md) · [Automatic repairs](automatic-repairs.md) ·
[Model setup](lightgun-setup.md) · [Library checks](library-validation.md)

## Vyper stops responding after an idle period

In **My Guns → Buttons & live input**, select the affected player and use
**Reconnect this Vyper**. Windows asks for administrator permission to restart
that specific present Blamcon USB device. Deadeye then re-registers its input
and refreshes detection. Calibration and saved bindings are retained. The app
never restarts a USB hub or resets every connected gun as part of this action.

Canceling the Windows prompt leaves the device unchanged. If Windows cannot
uniquely identify the device, check its cable and port. USB selective suspend
is only one possible cause; a disabled global power setting does not prove
that the firmware, hub or individual device is healthy. This recovery action
is not evidence that a particular idle failure has been reproduced or cured.

## Which games officially support two players?

Use **Local multiplayer** in the library and inspect the player mode below each
game. **Taking turns** differs from simultaneous play; **Online multiplayer**
is excluded from this local filter. Game details link to the manufacturer or
store used for the exact edition. A game's official player mode does not
certify two independent gun inputs on the current installation.

The initial catalog has 43 documented editions. Other entries display
**Player count unverified**. Imported free-text setup notes are retained in the
library but cannot override this source-backed metadata.
