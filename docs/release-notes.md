Add an optional live input diagnostic tool alongside the Windows frontend.

- Deadeye Input Lab creates two independent absolute HID test guns with keyboard collections and reuses the assigned guns' learned mappings through the normal game launch path.
- Check aiming endpoints, continuous sweeps, button press and release events and player isolation, with game screenshots and separate input delivery results.
- Direct TeknoParrot profiles without external helpers are the first supported adapter. The reviewed Aliens emulator build also has a read-only input exchange probe for axis limits and action bits.
- Scene readiness, emulator input routing and actual gameplay remain separate results. Unknown scenes and unsupported emulator builds are reported explicitly.
- Preserve the original profile, check restoration by hash and keep the library read only. Test devices are removed when the tool exits.
- Exclude Aliens Extermination's cabinet Service Select route from gameplay requirements while retaining real gameplay Select actions.

Input Lab is a separate portable download. Its test driver and local signing certificate require explicit installation; Deadeye Setup and ordinary game startup do not install them. Physical tracking, calibration, recoil and game response still need their own verification.
