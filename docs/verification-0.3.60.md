# Aliens absolute aiming correction

The installed TeknoParrot input implementation was inspected for the reported
upper-left-quarter aiming limitation. Its windowed absolute RawInput route uses
WPF's primary-screen dimensions in scaled desktop units, then divides by the
renderer client dimensions in pixels. With 200% Windows scaling and a native
3840 × 2160 renderer, this can halve both axes. Its fullscreen absolute route
normalizes the original HID coordinates directly.

Deadeye previously forced the windowed route for both supported Aliens profiles.
It now selects fullscreen input when RawInput is absolute, retains the monitor's
native game resolution and applies the existing renderer presentation. Relative
input and other input APIs keep the windowed route. No vendor executable or gun
calibration is modified.

The new absolute-input regression failed against the previous implementation.
After the change, all 721 core checks passed on macOS, including relative-input
preservation and the existing idempotent profile-backup checks.

A Windows candidate was tested directly on the connected display with two Vyper
assignments. Aliens: Extermination produced a renderer after 20 seconds and
completed 60 seconds of observed renderer time. The real game menu opened and
resumed; the renderer returned to the foreground. Exit left no owned processes
and no cleanup issues. The game library was unchanged. This establishes launch,
menu/resume and cleanup behavior; physical targeting and complete two-player
missions still require play at the display.
