# Input Lab verification

Input Lab was exercised on the target Windows console with two independent
virtual absolute HID mice and keyboard collections. The normal frontend remains
separate from this optional tool and does not install its driver.

## Observed input route

The reviewed Aliens Extermination TeknoParrot build exposes a read-only JVS
input exchange. The live test observed its actual output while submitting virtual
gun reports through Windows and the profile prepared by `GameSession`.

- Both players reached 9 through 225 on both emulator aiming axes with the normal
  corrected absolute RawInput presentation.
- Each player received a nine-point aiming grid and a 121-point continuous sweep.
- Ten gameplay action routes had matching RawInput press/release events, player
  isolation, the expected emulator button bit while held and a cleared bit after
  release: Coin, Start, Trigger, Special and Flame for each player.
- A temporary comparison with the old windowed route reached only 9 through 81
  on P1's axes and failed the axis-span check. It was not reported as a pass.
- Original profiles were restored and the library hash was unchanged after the
  runs. Virtual devices were disposed.

The input observer is restricted to an exact reviewed emulator executable hash.
It does not modify emulator or game memory. A separate read-only game-state
observer accepts only two known game executable hashes; the tested installation
was outside that allowlist, so game memory results were declined.

## Capture and scene readiness

Legacy GDI screenshots retained an initial loading image while the GPU renderer
advanced. Conclusions about a game being stuck cannot be drawn from those old
captures. Input Lab now uses DXGI Desktop Duplication on the game's monitor and
reports capture failures explicitly. Scene readiness uses Windows OCR and remains
a separate check from input routing. An unknown scene fails closed.

## Scope

These observations establish the Windows and emulator input route for the tested
adapter. They do not establish complete gameplay, physical camera accuracy,
calibration, recoil or compatibility with every game and gun combination.
The first runnable adapter supports direct TeknoParrot absolute RawInput profiles
without external helpers or shared-library preparation. Other adapters require a restoration recipe before live replay is enabled.

The targeted cabinet Service Select regression passes within the 722-check
cross-platform core suite. Windows compilation and native runs cover the optional
tool's Windows-specific input and capture path.
