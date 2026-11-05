# Verification scope for 0.3.75

Library maintenance and launch now share the same file preparation for supported
adapters and installed multiplayer helpers. Both paths retain assignments when
the selected P1 is absent. A keyboard-only launch does not erase gun profiles.
Mouse-index configuration still needs the local RawInput enumeration.

The core suite passed 818 checks locally. Added cases exercise the shared
RetroArch profile and returned launch arguments, GunCon/Zapper consumer inputs,
HOTD III keyboard modes and Start scan codes, fixed Vyper reload, independent
HOTD III profile routing, Vyper Move Cross, Wartran special actions for all four
gun families and exact service-menu exceptions. Metadata tests distinguish Wii
Overkill modes, alternating Crossbow Training and the assisting Wii Sin and
Punishment player. The Windows target builds successfully.

The HOTD III simulation replays profile expressions. It is not input injected
into the running game and does not prove hits or sustained play. Wii production
DSU frame checks and existing optional native Input Lab adapters retain their
separate verification scopes. No library entry is promoted to physical gameplay
verified by these tests.

The library test retains its existing per-game watchdog, owned-process cleanup,
restore logic and stop controls. Within the existing resume groups it now checks
preferred entries first, then priority and release year.

The large Claude review did not complete within its six-minute deadline. A
smaller follow-up review was attempted separately; no completed independent
review is claimed until its response is available.

GitHub Windows [run 42021810138](https://github.com/sthamann/Deadeye-Arcade/actions/runs/42021810138)
completed successfully: 843 core checks and 32 Input Lab checks (22 scene signal,
five MAME replay scope and five Dolphin save prompt cases). Installer and portable
packages are published in the corresponding release.

The follow-up Claude review also reached its 100-second deadline without a
completed response, despite an authenticated CLI. No Claude approval is claimed.
