# Verification scope

The preceding installed 0.3.76 build produced a responsive Big Buck Hunter World renderer while Windows Terminal hosted its owned BudgieLoader console in the foreground. Selecting Windows Console Host through the Windows Terminal interface was followed by two consecutive passing launch/focus/cleanup tests and a third passing check at the start of the resumed library batch. These checks did not inject gameplay inputs or certify physical guns.

The matching registry values were read back on Windows and cross-checked against Microsoft's published default terminal documentation. New automated checks exercise diagnosis and, on Windows, the real repair/backup/restore path under an isolated temporary HKCU Software key. They never modify the CI user's real default terminal preference.

The new delayed launcher recovery has native fixtures for owned visible status, wrong owner and non-status/error windows. Its actual game confirmation and the installed new-version checks are recorded after execution.

The full native library batch continues on the installed 0.3.76 build. Results from that version are not represented as verification of 0.3.77.

The resumed native batch stopped after Air Twister because the cleanup dialog detector reported an untitled GameInputServiceWindow. A direct Windows probe found the service window hidden (IsWindowVisible=false, 136 x 39). Blocking-dialog detection now requires a visible, non-minimized surface before classifying the process/window class. Windows fixtures retain visible errors while rejecting an invisible service-shaped window. No GameInput service or security setting was disabled.

The current local core suite passed 836 meaningful checks. The Windows cross-build completed with zero warnings and zero errors. Windows-only registry and native window fixtures remain pending the release CI run.

Windows release CI [run 42037724180](https://github.com/sthamann/Deadeye-Arcade/actions/runs/42037724180) passed 871 core checks and all 32 Input Lab checks (22 scene, five MAME scope, five Dolphin save-prompt cases). The published installer returned exit code zero on the test PC and matched commit 9f689481bafc575fd9c5ea51bbdce1f6152279c4. The library hash was unchanged.

Installed-version follow-up checks passed launch, stable foreground and cleanup for Big Buck Hunter World and Air Twister. The subsequent full native library run remains in progress; this is not a full library or physical gameplay acceptance.
