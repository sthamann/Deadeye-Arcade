Restore game input after hidden Windows service focus loss and make Alien3 Start work through the actual cabinet input.

- Recover the active game renderer from the exact invisible, untitled GameInput service window observed taking focus during several arcade launches. Preserve visible applications, real prompts and the in-game menu. Record successful recoveries and retain the native test’s actual focus requirement. No Windows service or security setting is disabled.
- Map the selected player’s Shoot and Start actions to Alien3’s actual trigger input. The original cabinet starts with the trigger after inserting credits; the MAME driver has no dedicated gameplay Start ports. Show Shoot / Start in the guide and keep coin, bomb and both aim axes independent.
- Add manufacturer manual sources for two-player Alien3, Terminator 2 and the Model 2 edition of Virtua Cop. Player metadata describes the original game edition, independently of emulator and physical input verification.

Native launch checks, readable profile routing and physical multiplayer gameplay remain separate verification levels.
