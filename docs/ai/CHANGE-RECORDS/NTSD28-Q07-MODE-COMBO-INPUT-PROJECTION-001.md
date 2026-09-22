<!-- CHANGE-RECORD
id: NTSD28-Q07-MODE-COMBO-INPUT-PROJECTION-001
status: CODE_WRITTEN
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LoganModeComboInput.cs
authority: formal Logan playable game_session.cpp mode child resolution and native_combo_hud.cpp exact complete combo block
evidence: artifacts/diagnostics/NTSD28-Q07-MODE-COMBO-LIVE-TUPLE-AUDIT-001/REPORT.md
-->

# NTSD28-Q07-MODE-COMBO-INPUT-PROJECTION-001

Created before script modification. Unity currently has an inactive default combo carrier and no production projection for the formally staged `mode.dat` child. This package adds only an immutable, non-publishing input projection. It must not set the World tuple, alter Q06 producers, change content/session identity in isolation, consume menu/results blocks, or change Scene/ProjectSettings. Expected side effects are only a new C# source and its `.meta`; no runtime behavior changes until a separate atomic identity/activation package. The Task Contract states authority, exact paths, acceptance, limitations and rollback. Record actual diff, focused validation, compile, risks and next dependency below after implementation.

Actual diff: added `Assets/NTSD/Scripts/Animation/LoganModeComboInput.cs` and `.meta` only. `Capture` selects the first formal mode child, validates its path inside the DAT root, parses exactly one complete `<combo>` block through existing `Lf2DatTokenizer`, and captures parent/child raw and projected semantic SHA-256. `AssertInputsCurrent` rejects changed or removed inputs. Missing parent `mode.dat` keeps the legacy/unconfigured no-input result. No World/identity/candidate/Scene/ProjectSettings/resource code was changed.

Validation: PowerShell `Add-Type -Path` compiled the pure source plus `Lf2DatProperty.cs`, `Lf2DatTokenizer.cs` and `BattleContentSource.cs`; formal and staged fingerprints matched and the tuple was `1/1/50/1`. Corrected reflection checks rejected an incomplete combo and path escape. The first reflection command failed because its catch filter expected `TargetInvocationException` while PowerShell exposed `MethodInvocationException`; this was a harness error, not a parser assertion failure. `git diff --check` passed. The original Unity Editor assembly is older than this new source, so **Unity compile, focused Unity tests, Play and same-tick formal comparison remain pending**. Next independent Task must integrate this input into joint content identity/cache freshness and initialize the existing World carrier after `ApplyMatchConfig` resets runtime state, before tick 1, with nonbattle preservation checks.

`Tools/Validate-ChangeLedger.ps1` exited 0: 680 records, the one governed code file in the diff is covered by this Record. Its numerous warnings refer to older Records whose paths are not in the current diff, not an uncovered file in this package.

Additional in-memory focused check: a nonexistent root returns no mode input and recapturing the same staged files yields identical input and semantic fingerprints. This used no created Unity project, no temporary files and no Editor execution.
