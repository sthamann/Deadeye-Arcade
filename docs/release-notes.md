Avoid accidental game menus during interrupted trigger fire.

- Honor real mouse button release packets in the global trigger fallback, including releases that occur between timer polls.
- Restore a minimized game renderer before returning focus from the game menu.
- Keep the existing ten-second continuous hold, Start + Coin emergency exit and F10 menu shortcut.
- Save a local per-game session report with observed runtime exit codes and whether the stop was requested.
- Distinguish an unexpected exit after gameplay started from a failure to start a game.
- Add an opt-in `--with-overlay` option to the existing native game test. It opens the real menu for fifteen seconds, resumes the renderer and records whether that sequence completed.

Gun assignments, calibration and game library entries are preserved. Startup and menu tests do not establish complete mission or physical two-player gameplay compatibility.
