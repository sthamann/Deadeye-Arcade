Preserve complete Lindbergh game data when preparing paths without special characters.

- Keep the relative `elf`, `fs` and `drv` directory layout instead of copying only the ELF executable folder.
- Upgrade earlier flattened copies while retaining modified player settings and game-created saves, including extensionless save files. Keep original game files and use their current executable data.
- Repeated launches preserve newer progress and continue to propagate source updates.
- Refresh old HOTD 4 Special loader metadata from the matching installed ElfLdr2 template, retaining bindings and user settings. Missing replacement templates or a different game executable leave the existing loader unchanged.
- Six new core regressions cover sibling assets, upgrading an existing prepared path and keeping newer progress across repeated preparation. Native observations and remaining game-specific failures are tracked separately in the game-check guide.
