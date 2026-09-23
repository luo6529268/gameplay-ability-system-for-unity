# NTSD28-Q09-KILL-ICON-INPUT-IDENTITY-001

Status: `PLANNED` before script modification (2026-09-22; historical snapshot). Parent BATCH-05/Q09, R17. Task Contract: `artifacts/diagnostics/NTSD28-Q09-KILL-ICON-INPUT-IDENTITY-001/TASK-CONTRACT.md`.

Authority and original state: formal `render_snapshot.cpp:2019-2030` selects the mode feed's pic_type0..6; formal `d3d11_renderer.cpp:2076-2094` draws available icons. The selected three PNGs are exact-byte staged. Unity `LoganModeComboInput.KnockoutFeed` captures their paths, but `LoganVisualContentCandidate` currently hashes actor images and WORDS only; icon PNG byte changes can reuse a prior visual key. Existing WORDS changes in that same file are user work and must be preserved.

Declared exact code paths/symbols: `Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs` (new optional native kill-icon input projection, visual fingerprint branch, `Capture`, `AssertInputsCurrent`); `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KillIconInputIdentityEditorTests.cs` plus meta (focused identity and path cases). Expected side effect: visual cache key changes to versioned V3 only when a selected kill-feed record exists; icon file mutations, appearance and removal invalidate a captured publication candidate. Existing no-feed V1/V2, 906 actor-image set, World, tick, input, ordinary HUD, menu and previously published values stay unchanged.

Acceptance and risks: exact formal/staged match; repeated path and absent-image semantics; mutation/appearance/removal and path containment tests; current-source compile; original Editor focused NUnit when current source has loaded. Risk is visual cache invalidation or over-strict missing resource handling; allow formally optional missing PNG while tracking presence. No image decode or visual parity is claimed here. Rollback: review and reverse only this package's exact script/test diff; no reset/clean/delete without authorization.

<!-- CHANGE-RECORD
id: NTSD28-Q09-KILL-ICON-INPUT-IDENTITY-001
status: COMPILE_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KillIconInputIdentityEditorTests.cs
authority: formal NTSD2.8-Logan playable render_snapshot.cpp knockout feed icon selection and d3d11_renderer.cpp icon consumption
evidence: artifacts/diagnostics/NTSD28-Q09-KILL-ICON-INPUT-IDENTITY-001/ACCEPTANCE-PENDING.md
-->

Actual change (2026-09-22): the declared candidate script gained `NativeKillIconInput`, V3 visual fingerprint selection when the selected feed exists, and a prepublication icon freshness check. The declared new Editor test plus meta covers formal/staged identity and optional icon mutation cases. No renderer, World, scene, menu, ordinary HUD, or old resource changed. Current source compiled offline with the new test in the evaluated input. Compiled icon capture selected matching formal/staged seven-slot fingerprints; full candidate direct call failed to load `UnityEngine.CoreModule` outside Unity, and original Editor assemblies are older than this edit. Focused Unity NUnit, full candidate publication and pixels are pending; do not advance to VERIFIED. See `ACCEPTANCE-PENDING.md`. Rollback remains reviewed, package-specific diff reversal.

Current-state correction (2026-09-22): original Editor PID 33236 imported the new test and completed a successful Tundra script build; `Library/ScriptAssemblies` runtime/Editor DLL timestamps at 09:44Z are later than this package's 09:38Z files. Advance to `COMPILE_PASS` only. No later domain reload or focused NUnit result is confirmed, and the earlier WORDS TestRunner still has no result file. See appended acceptance evidence; do not send a concurrent test request.
