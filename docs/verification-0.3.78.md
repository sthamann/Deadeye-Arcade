# Verification scope

The 0.3.77 series produced responsive Big Buck Hunter World, HOTD EX and Silent Hill Arcade windows while the foreground process was GameInputSvc with an empty title. The service window was independently observed as invisible. Filtering invisible windows from modal-dialog detection fixed Air Twister's cleanup failure, but did not restore focus during active gameplay. This release adds that separately scoped runtime recovery.

Recovery requires the exact hidden service class and process, an untitled foreground surface, a visible active game renderer and no overlay or shutdown. It counts successful activation only after checking the real foreground window. Native checks still require actual game focus and at least two stable seconds. The Windows service remains enabled.

The Alien3 change follows the Sega cabinet manual and MAME's System 32 input definition: credits then trigger to start, with the generic Start bits replaced by service/unused inputs. Regression checks exercise actual generated controller routes for both physical players, repeat preparation and read back both language legends.

Installed game follow-ups and release CI results are recorded after execution. This is not a physical aiming, recoil or whole-library gameplay acceptance.

The current local core suite passed 841 meaningful checks. The Windows cross-build completed with zero warnings and zero errors. Native installed-version follow-ups remain required.
