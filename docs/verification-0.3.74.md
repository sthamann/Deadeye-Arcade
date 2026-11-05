# Player metadata and targeted Vyper recovery

## Scope

- Embedded player-mode metadata for 43 documented editions, matched by stable Steam app ID, exact TeknoParrot profile or title plus platform. Unknown imported notes cannot become verified metadata. Online co-op, alternating play and linked cabinets are distinct from local simultaneous modes.
- Player labels on game tiles, the featured game and details, with source links and a local multiplayer filter. No library rewrite is required.
- User-invoked Windows device restart for one freshly resolved Vyper container. Only VID 3673, PID 0100–0103 composite nodes qualify. Hubs, child interfaces and wildcard IDs are rejected. No power policies, firmware, calibration or stored mappings are reset.

## Validation

The local core suite passed 802 checks, including actual subprocess cancellation and Dolphin UDP routing. The added cases cover platform isolation, renamed Steam entries, online versus local modes, unverified notes and restricted USB instance shapes. The Windows target compiles and the UI build passes seven device identity cases. GitHub Windows CI passed 827 core checks and 32 InputLab checks in [run 42014093664](https://github.com/sthamann/Deadeye-Arcade/actions/runs/42014093664).

The library filter and player source details were inspected in the rendered English UI; the German labels and layout were checked at narrower viewport sizes. This does not verify actual two-gun gameplay.

## Boundaries

The reported Vyper idle failure has not been reproduced in a controlled long-idle run. A targeted USB restart provides recovery, not proof of its root cause. Windows may request elevation; canceling it is supported. Physical Start, trigger and aiming tests for Walking Dead and both Windows remakes remain separate acceptance steps.

See [configuration research](lightgun-configuration-research.md) for existing tools and the plugin compatibility constraints checked during this work.

## Installed artifact check

The published 0.3.74 installer was downloaded from this repository, matched to its
published SHA256SUMS entry and installed successfully on Windows. The library
hash was unchanged by installation. The installed export enumerated the existing
library and resolved the documented player metadata.

A P1 Vyper restart was exercised through the installed UI and the Windows prompt.
PnPUtil exited with code 0. Deadeye registered its input transport successfully
afterward and discovery read the Vyper settings again. This verifies the recovery
path; it does not reproduce the original long-idle failure or verify physical aim.
