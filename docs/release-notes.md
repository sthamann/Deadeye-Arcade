Keep selected guns in the game when devices reconnect, and catch missing controls before launch.

- Use fresh USB discovery for ordinary frontend launches, matching the shared game test path. Recover changed mouse and keyboard interfaces through the physical gun identity while preserving player slots and learned controls.
- Report an unavailable selected P1 before clearing game assignments. Record each launch player and discovery exclusion in the input log.
- Read back supported profiles after preparation. Missing required Start, trigger or other action routes produce a specific setup error before the game process starts. Aim and button tests remain optional.
- Prepare the installed first House of the Dead Remake ArcadePlugin for its DemulShooter multiplayer route. Read actual helper assignments and fixed Start keys for the controls legend; use the temporary Vyper magazine reload route and preserve the original settings.
- Add profile regressions for all 16 P1/P2 combinations of RS3, Sinden, XGunner and Vyper using Walking Dead action identifiers, plus USB reconnect and ArcadePlugin mode tests.

The local core suite passed 793 checks. These are profile and software checks. Physical shots, tracking and complete two-player gameplay remain separate acceptance steps. See [verification notes](https://github.com/sthamann/Deadeye-Arcade/blob/v0.3.73/docs/verification-0.3.73.md).
