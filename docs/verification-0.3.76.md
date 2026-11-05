# Verification scope

The local core suite passed 830 checks, including production Dolphin packet routing,
per-model Atomiswave pump press/release replay, actual Super Scope/3DO input fields,
and modern Windows prompt classification. This is software evidence, not a physical
lightgun or complete gameplay acceptance.

The preceding native library batch exposed a shared foreground failure: modern
Windows security PickerHost windows and GameInput foreground windows were not
recognized by the classic dialog detector. The detector now stops the unattended
batch for these exact hosts without dismissing or approving their prompts.

A separate problem in the maintenance invocation gave games administrator rights
along with the console transition. Cxbx explicitly rejects this mode with a warning.
The follow-up invocation elevates only the Windows console transition; game tests
run as the normal signed-in user. This is a testing workflow correction, not a
change to security policy or a reason to disable GameInput.

Windows build and installed-game results are recorded separately after execution.
The attempted Claude review did not finish; no independent Claude approval is claimed.

Targeted console checks as a normal user passed launch, foreground and cleanup for
Heavy Weapon Deluxe, Ex-Zodiac and HOTD 4 on the preceding installed version.
Big Buck Hunter World still failed foreground behind Windows Terminal. The
startup activation code had marked rejected SetForegroundWindow calls as already
presented; it now retries for at most twenty seconds from each owned renderer's
first appearance, never repeatedly stealing focus after success and never
approving a blocking security prompt. Native confirmation of that change remains
required.
