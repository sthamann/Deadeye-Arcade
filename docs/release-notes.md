Improve native game diagnostics and extend the optional input replay tool.

- Reject a launch check when the actual renderer does not respond to Windows messages. Reports retain the response and native error rather than treating a visible, frozen window as success.
- Correct GPU screenshots on HDR/fullscreen output by requesting BGRA8 and waiting for a real image update.
- Add a read-only Input Lab capture command for games without a replay adapter. It creates no virtual guns and changes no game configuration.
- Recognize additional observed arcade Start/Credits prompts and allow launcher-to-renderer transitions during scene readiness.
- Extend reviewed startup sequences for Raccoon Rampage, Scarlet Dawn and Tomb Raider. Insert credits before Start and use four coin pulses for the observed Tomb Raider cabinet.
- Document additional native checks and unresolved issues. Raccoon Rampage and Scarlet Dawn reached two-player gameplay; Tomb Raider gameplay entry and Rez Infinite startup remain unresolved.

Input delivery, a rendered menu and verified gameplay remain separate results. The optional input tool does not certify physical tracking, calibration, recoil, all buttons or a complete playthrough. Its driver is not installed by the ordinary frontend.
