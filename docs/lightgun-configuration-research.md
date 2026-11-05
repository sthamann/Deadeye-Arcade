# Existing automatic lightgun configuration tools

Reviewed 5 November 2026. These tools solve parts of the setup workflow; their
documented support does not establish that every game and gun combination works.

| Project | Useful capability | Consequence for Deadeye |
| --- | --- | --- |
| [RetroBat](https://github.com/RetroBat-Official/emulatorlauncher/blob/master/emulatorLauncher/Generators/TeknoParrot.Guns.cs) | Discovers RawInput guns, resolves keyboards, applies game-specific control maps and selects a DemulShooter route for supported titles. | Retain independent per-player mouse and keyboard identities and per-game actions. Do not copy one game's entire controls to unrelated titles. The upstream generator explicitly resets the default keyboard for each button. |
| [Integrum Retro Lightgun Assistant](https://github.com/IntegrumRetro/Integrum-Retro-Lightgun-Assistant) | Templates and scripts for MAME, TeknoParrot and DemulShooter setup. | Useful reference workflow. It is not a verified replacement for this installation's profiles. GPL code has not been imported into Deadeye's MIT source. |
| [TeknoParrot Manager](https://github.com/Jumpstile/teknoparrot-manager) | Distributes configured reference controls within compatible game families and reports residual controls needing attention. | A reference mapping still needs the correct game-family match and special-action review. Existing profiles must be backed up and read back after preparation. |

## House of the Dead Remake

The [ArcadePlugin author](https://github.com/argonlefou/HotdRemake_ArcadePlugin)
documents two different input paths: SINGLE PLAYER uses one mouse; MULTIPLAYER
requires DemulShooter. The supported game builds documented upstream are the
April 2022 release and July 2022 v1.1.3. P1 Start is key 1, P2 Start key 2, and
coins key 5. Deadeye prepares the installed multiplayer route and audits these
actions before launch. An installed DLL alone does not verify build compatibility.

## House of the Dead 2 Remake

The [absolute mouse fix](https://github.com/Disc0-LightgunMad/HOTD2-Remake-Lightgun-Fix)
replaces relative gameplay aiming with absolute viewport coordinates. Its README
reports Sinden testing and resolution support; it does not establish support for
two independent RawInput guns. It must not be layered blindly over an existing
MultiLightgunPlugin, because both can change the same aim path. Deadeye retains
the installed multiplayer plugin workflow pending direct compatibility testing.

## Vyper USB idle recovery

[Blamcon troubleshooting](https://blamcon.com/get-started-with-blamcon/troubleshooting/)
and the [ARC guide](https://blamcon.com/get-started-with-blamcon/blamcon-arc-gui/)
cover input mode, tracking and USB power/connection issues. The published
[serial protocol](https://blamcon.com/get-started-with-blamcon/serial-commands/)
does not document a software reboot command. Deadeye therefore uses Windows'
device restart for a freshly resolved, selected Vyper composite USB node. This
is a user-invoked recovery; the app does not infer a hung gun merely from inactivity.

## Profile preparation and additional game requirements

Library repair and launch now use the same file preparation. Device-index adapters
still require a local Windows session with actual RawInput enumeration. A repair
under RDP cannot manufacture that order.

The [DemulShooter author](https://github.com/argonlefou/DemulShooter/wiki/Raw-Thrill)
documents that Jurassic Park, Aliens Armageddon, Terminator Salvation and Walking
Dead use TeknoParrot for gun input. DemulShooter serves outputs for these routes;
adding it cannot repair an absent Start assignment. The same distinction applies
to [Ghostbusters](https://github.com/argonlefou/DemulShooter/wiki/Arcade-PC-%7C-Ghostbusters).
Ghostbusters' service setting for the ball mechanism affects custom recoil output.
It does not establish tracking or button compatibility.

[HOTD III](https://github.com/argonlefou/DemulShooter/wiki/Windows-games#the-house-of-the-dead-iii)
requires keyboard mode for both players with DemulShooter. The installed
[ArcadeMod](https://github.com/argonlefou/Hotd3pc_ArcadeMod) stores decimal scan
codes in `Hod3Arcade.ini`. Deadeye prepares those modes and Start keys, reads back
the helper assignments and routes Vyper magazine pull to right mouse for this
fixed-input consumer. Original settings are backed up.

The [Dead Containment developer](https://store.steampowered.com/app/1302230/)
documents native lightgun support and online co-op. This does not establish two
independent local guns on one PC. Deadeye does not label online co-op as local.

The RetroArch action lists are based on the actual core consumers:
[Beetle PSX input](https://github.com/libretro/beetle-psx-libretro/blob/master/input.c),
[FCEUmm](https://docs.libretro.com/library/fceumm/),
[Genesis Plus GX](https://github.com/libretro/Genesis-Plus-GX/blob/master/libretro/libretro.c)
and the previously reviewed Saturn consumer. GunCon uses trigger, reload, A and B;
Zapper trigger belongs to console port two. Unused profile fields do not prove
missing game actions. Conversely, these core contracts do not verify every ROM's
controls, remaps or pointer range in the installed emulator build.
