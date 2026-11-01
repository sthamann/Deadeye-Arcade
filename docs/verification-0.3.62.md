# In-level replay verification

This update extends [the live input route checks](verification-0.3.61.md).
The same optional driver, device isolation and hash-checked profile restoration
apply. The normal frontend does not install the test driver.

## Scene progression

Native GPU captures showed both Aliens Extermination and Jurassic Park reaching
their mission selection with both players joined. Aliens completed its ten
unique gameplay input routes through the reviewed emulator input exchange.
Jurassic Park completed its seven replayable gameplay routes at the Windows
RawInput receiver. These observations did not certify every weapon: some actions
occurred while a mission vote or briefing was still visible.

The replay now allows the reviewed 30-second mission vote to finish, then sends
each player's mapped trigger to dismiss the briefing. It captures the following
scene and the before, held and released states of every action. That timing
recipe is limited to these two profiles. An elapsed timer is not treated as a
certificate of gameplay response.

Reports include the tool version and binary hash and the emulator binary hash.
Unsupported or unknown observations remain explicit. `gameplayVerified` remains
false in the automatic report; per-action game response needs visual review or
a matching read-only game-state observer. Physical tracking and recoil remain
separate tests.

## Native in-level observations

The extended replay reached an active level in both titles with both players
joined. Aliens captures showed player-specific shots and flamethrower effects;
its ten unique gameplay routes also had matching emulator input bits and release
states. Jurassic Park captures showed shooting scores and grenade effects for
both players; all seven unique replayable gameplay routes had matching Windows
press/release events. Administrative test-menu navigation was excluded.

These are short input checks in a running level, not complete play-throughs.
Special actions in Aliens retain their emulator-route evidence where a single
image does not independently establish the whole effect. Physical camera
tracking, calibration and recoil are outside these virtual-device observations.

## Published package check

The GitHub 0.3.62 installer completed with exit code zero and left the library
hash unchanged. The published Input Lab archive passed its five scene-signal
checks and ran in the native Windows console using the already installed driver,
without the installation flag.

The published tool identified itself as
`0.3.62+bf84af0d50de28338974c3167e7077c5263cb1ee`. Its tested assembly SHA256 was
`998cd8b5d2986d6d8d315d12e01d3e488c5b77b9ee8e9158242056e48502d07c`.
Both emulator aiming spans and all ten Aliens action bits passed again. The
original profile was restored, the library was unchanged, and a fresh Windows
RawInput enumeration found zero remaining test-device collections.

After handoff the display was 3840 × 2160 at 60 Hz, with Remote Desktop inactive
and the installed frontend in the foreground. The Windows release workflow
passed 744 core checks, both package builds, the Input Lab startup check and its
five scene-signal cases.
