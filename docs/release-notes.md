Make game checks more useful for two-player lightgun setups.

- Show Shoot and Bomb for both players in Alien3: The Gun and Terminator 2 rather than generic button numbers. Keep the actual installed MAME bindings intact.
- Start the renderer startup budget after profile preparation and report preparation time separately. Slow first-run setup has its own bounded timeout and cleanup.
- Add experimental two-gun MAME replays for the reviewed Alien3 and Terminator 2 launch recipes. Use isolated configuration and NVRAM copies, preserve existing calibration and verify that original files and the library remain unchanged.
- Extend the optional Dolphin replay to the two Resident Evil Chronicles titles, with isolated first-run save prompts. Unknown game scenes still stop verification.
- Record input desktop details when native GPU capture fails. Keep input delivery, observed game effects and physical gun acceptance separate.

Input Lab is optional developer tooling. Its virtual HID driver is never installed by the ordinary frontend. A successful automated run does not certify a complete game or physical tracking.
