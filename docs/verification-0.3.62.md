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
