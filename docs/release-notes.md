Restore the full aiming range in Aliens: Extermination on displays with Windows scaling.

- Automatically select TeknoParrot’s normalized fullscreen input route for absolute RawInput guns in Aliens: Extermination and Aliens: Armageddon.
- Keep the game at the monitor’s native resolution while avoiding the windowed input conversion that mixes scaled desktop units with physical window pixels.
- Preserve the existing windowed route for relative mouse input and other input APIs.
- Apply the correction when preparing a game, with a backup of the original profile. Gun assignments, button mappings, calibration and the library are preserved.

This fixes the launch configuration responsible for aiming being confined to the upper-left quarter at 200% Windows scaling. Physical aiming and complete gameplay remain separate from automated input and startup checks.
