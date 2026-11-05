Prepare supported game controls consistently during library repair and launch.

- Library repair and game launch share the same profile preparation for TeknoParrot, MAME, RetroArch, RPCS3, Supermodel, Flycast, Time Crisis 5 and installed multiplayer helpers. Device discovery is refreshed before configuration. A missing selected P1 leaves the existing assignments intact.
- Configure the installed HOTD III ArcadeMod for DemulShooter keyboard mode and actual Start scan codes. Read back trigger, reload, path selection and Start. Vyper magazine pull temporarily uses the fixed mouse reload input and restores the saved menu configuration afterward.
- Keep Vyper Cross reachable in RPCS3 when the recoil selector has no button assignment, using D-pad Down as a separate default.
- Audit the PS1 GunCon, NES Zapper, Saturn and Genesis inputs consumed by their specific cores. Unused buttons no longer produce invented mapping failures. NES menu controls stay with physical P1 even though the Zapper occupies console port two.
- Map Wartran Troopers Shoulder and Sense separately from Option for all supported gun families. Distinguish specific cabinet service-menu Enter controls from real game confirmation actions.
- Expand the sourced multiplayer catalog to 47 editions. Separate Overkill's two-player story from its four-player minigames, Link's alternating turns and Sin and Punishment's assisting second player.

Profile coverage and synthetic routing checks do not mark a game as gameplay verified. Unsupported title actions, content overrides and physical tracking remain explicit review items.
