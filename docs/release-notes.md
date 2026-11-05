Keep arcade games in the foreground and recognize a specific Windows console compatibility failure.

- Diagnose a recent native TeknoParrot failure where Windows Terminal is hosting the owned BudgieLoader console while a responsive game renderer loses focus. Back up the current user's console preference, select the Microsoft documented Windows Console Host choice for new consoles, and read back both values. Preserve a later user change. This affects new consoles for that Windows user and requires a fresh game test.
- Ignore invisible GameInput service windows when identifying blocking dialogs. Real visible Windows prompts still stop unattended tests.
- Recover from a delayed, owned TeknoParrot Game Running status window after the renderer is available. Restrict recovery to that exact non-modal launcher status rather than other applications or error dialogs.
- Add sourced player metadata for Lost Land Adventure and the four-player Raccoon Rampage profile. Deadeye currently prepares P1 and P2; the original cabinet's four-player specification does not enable four assignments in the app.

Native launch checks, readable profile routing and physical multiplayer gameplay remain separate verification levels.
