# Verification scope

On the installed 0.3.78 build, targeted checks passed for Big Buck Hunter World, HOTD EX, Silent Hill Arcade, Air Twister and Alien3. The subsequent batch again exposed a separate Big Buck foreground failure: the responsive FREEGLUT renderer was behind a classic ConsoleWindowClass window belonging to BudgieLoader itself. The existing Windows Terminal preference repair does not address this same-process console case.

The new recovery checks exact window classes and matching nonzero process IDs, acts only inside the active TeknoParrot session with a visible renderer and no overlay or shutdown, and verifies the actual foreground before and after activation. It never closes or hides the console or treats focus failure as a pass.

The Input Lab MAME action filter now excludes only standalone Coin and Start labels. Its added regression cases retain Shoot / Start and Bomb while keeping the startup-only actions excluded from weapon replay. A combined action label must not silently remove a required fire test.

Windows release and installed-game results are recorded after execution. The ongoing full-library native run uses the installed 0.3.78 build and is not labelled as a 0.3.79 or physical gameplay acceptance.

The local core suite passed 844 meaningful checks. Both the Windows app cross-build and the optional Input Lab cross-build completed with zero warnings and zero errors. Windows release CI passed 879 meaningful core checks and all 36 Input Lab self-check cases, including the four added MAME action-selection cases. These checks verify the software paths; native recovery of the owned console and actual gameplay still require an installed-game test.
