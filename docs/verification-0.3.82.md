# Verification scope

A native two-minute Shadows Rising check exposed a wrong renderer identity. Deadeye tracked the visible Shell.exe GameShell window while Transformers2.exe, owned by the same session, was responsive and in the foreground. The TeknoParrot vendor profile specifies TransformersShadowsRising, a shell GamePath, no second GamePath, and the executable-name pair shell.exe;Transformers2.exe.

The session now resolves that exact shell layout to the sibling Transformers2/Transformers2.exe. The launch command remains the configured vendor route. PID, process start time and job ownership still control tracking and cleanup; a matching name outside the job cannot become the game. Target lifetime, renderer readiness and overlay return now share the actual runtime path. A missing runtime fails explicitly instead of accepting the bootstrap.

Core cases cover the actual sibling layout, unrelated profile, unsupported shell layout and Windows path separators. Local core and Windows build results, release CI, installed native observation and physical checks are recorded independently.
