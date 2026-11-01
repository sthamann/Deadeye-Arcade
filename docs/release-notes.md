Correct Saturn control audits and improve the optional Terminator replay recipe.

- Read only Trigger, Reload and Start for reviewed Beetle Saturn lightgun ports. Ignore unused generic bindings when checking missing controls and shared-key conflicts. Deleted required actions and a real shared Start key still fail the audit.
- Improve the optional Terminator Salvation replay recipe to use the mapped Reload button to skip its reviewed cinematic and allow the briefing to complete. Intro navigation remains separate from gameplay-action verification.
- Retain conservative verification for core overrides, physical guns and complete game actions. A successful launch or received packet does not certify all gameplay controls.
