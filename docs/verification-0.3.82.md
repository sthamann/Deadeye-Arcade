# Verification scope

A native two-minute Shadows Rising check exposed a wrong renderer identity. Deadeye tracked the visible Shell.exe GameShell window while Transformers2.exe, owned by the same session, was responsive and in the foreground. The TeknoParrot vendor profile specifies TransformersShadowsRising, a shell GamePath, no second GamePath, and the executable-name pair shell.exe;Transformers2.exe.

The session now resolves that exact shell layout to the sibling Transformers2/Transformers2.exe. The launch command remains the configured vendor route. PID, process start time and job ownership still control tracking and cleanup; a matching name outside the job cannot become the game. Target lifetime, renderer readiness and overlay return now share the actual runtime path. A missing runtime fails explicitly instead of accepting the bootstrap.

Core cases cover the actual sibling layout, unrelated profile, unsupported shell layout and Windows path separators. Local core and Windows build results, release CI, installed native observation and physical checks are recorded independently.

Local checks passed 862 meaningful core cases. Windows release CI passed 897 meaningful core checks and all 36 Input Lab cases. The published installer was checksum-verified and installed with exit code zero. The library hash remained unchanged.

The installed follow-up passed a 120-second Shadows Rising observation with the native in-game menu and resume enabled. Its tracked target was Transformers2.exe, renderer class TF2_Gun | Release, actual foreground process Transformers2; responsiveness, foreground, menu resume and cleanup passed. The library hash remained unchanged. This closes the observed bootstrap-window identity failure; independent gun gameplay is a separate observation.
