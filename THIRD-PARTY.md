# Third-party notices

Deadeye Arcade's application code is licensed under the [MIT License](LICENSE).
Dependencies retain their own licenses. Games, ROMs, emulators and manufacturer
utilities are not bundled with Deadeye Arcade.

| Component | License / source |
| --- | --- |
| React and React DOM | [MIT](https://github.com/facebook/react) |
| Lucide icons | [ISC](https://github.com/lucide-icons/lucide) |
| .NET Runtime and WPF | MIT and additional component notices; distributed runtime licenses are included under `licenses/`. [Runtime](https://github.com/dotnet/runtime), [WPF](https://github.com/dotnet/wpf) |
| Microsoft WebView2 SDK | [Microsoft license](https://www.nuget.org/packages/Microsoft.Web.WebView2). The separate WebView2 Runtime is distributed by Microsoft. |
| System.IO.Ports / ProtectedData | [MIT, .NET Runtime](https://github.com/dotnet/runtime) |
| NSIS installer runtime | zlib/libpng license and compression component licenses; included in `licenses/nsis-COPYING.txt`. Unmodified upstream source: [NSIS download](https://nsis.sourceforge.io/Download). |
| Vite / TypeScript / Playwright | Development tools; not run as Windows processes in the portable app. |

The generic cover placeholders are original application assets. README recordings
show the running interface with a curated selection of game artwork and preview
clips. Third-party game artwork remains subject to its owners' rights. The
application license does not grant rights to games, ROMs or artwork.

## Separately downloaded setup assets

- [Dolphin Lightguns Accuracy INIs](https://github.com/ProfgLX/Dolphin-Lightguns-Accuracy-Inis)
  are GPL-3.0 assets. The app downloads a verified revision into the user's data
  directory and preserves its LICENSE alongside generated, game-specific profiles.
  These profiles and their upstream assets are not bundled as MIT application code.
- The RS3 calibration DLL is obtained separately from the manufacturer's public
  calibration package. Its exact package and DLL hashes are checked before use.
  It retains the manufacturer's terms and is not redistributed in the application
  source or release packages.

- [OpenAL Soft](https://openal-soft.org/) 1.25.2 is downloaded separately, only when
  a matching game lacks OpenAL. The app checks the official package hash, copies
  the matching implementation DLL and keeps the upstream COPYING file. OpenAL
  Soft retains its LGPL terms; it is not bundled as MIT application code.
- [Hypseus Singe](https://github.com/DirtBagXon/hypseus-singe/releases/tag/v2.11.1)
  2.11.1 support images and a sound file can be restored separately for the exact
  recognized installed executable. Its upstream LICENSE is retained. No emulator
  or game data is included in the Deadeye installer.

- [DDrawCompat](https://github.com/narzoul/DDrawCompat/releases/tag/v0.7.2)
  0.7.2 can be downloaded separately for the original Windows House of the Dead 2.
  Its archive and DLL hashes are pinned. The app keeps the upstream 0BSD license
  in the game folder; a copy is included in `licenses/ddrawcompat-0BSD.txt`.
  The implementation DLL is not bundled with Deadeye.

## Optional input diagnostics

The separately built [Deadeye Input Lab](tools/Deadeye.InputLab/README.md) uses
[HIDMaestro 2.1.0](https://github.com/hifihedgehog/HIDMaestro/releases/tag/v2.1.0)
under its MIT license. Its upstream license and third party notices are included
in that tool's portable package. This includes the upstream notices for its
Windows SDK and WinRT dependencies. HIDMaestro is not a dependency of the normal
frontend and its driver is not installed by Deadeye Setup.

The optional Aliens observer uses published build identifiers and memory address
facts from [DemulShooter](https://github.com/argonlefou/DemulShooter/blob/master/DemulShooter/Games/Game_GvrAliens.cs).
It reads matching game state only and declines unknown executables. It does not
redistribute or patch game binaries.

Input Lab's GPU screenshots use the MIT-licensed Vortice.Direct3D11 3.8.3,
Vortice.DXGI, Vortice.DirectX, Vortice.Mathematics and SharpGen runtime bindings.
Their upstream MIT notices are retained in the tool's `licenses` directory.
Sources: [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows),
[Vortice.Mathematics](https://github.com/amerkoleci/Vortice.Mathematics),
[SharpGenTools](https://github.com/SharpGenTools/SharpGenTools).
