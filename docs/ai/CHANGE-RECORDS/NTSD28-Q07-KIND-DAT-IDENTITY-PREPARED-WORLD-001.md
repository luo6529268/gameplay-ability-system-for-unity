<!-- CHANGE-RECORD
id: NTSD28-Q07-KIND-DAT-IDENTITY-PREPARED-WORLD-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_DAT_IDENTITY_AND_PREPARED_WORLD
code-path: Assets/NTSD/Scripts/Animation/LoganObjectCatalog.cs
code-path: Assets/NTSD/Scripts/Animation/LoganContentIdentity.cs
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeDataCatalog.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28TraceContentIdentity.cs
code-path: Tools/NTSD28Parity/TraceContentIdentity.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07KindIdentityPublicationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TraceIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionCompositeContentIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07ModeComboPublishedActivationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCallerPlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeWeaponPieceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06SpawnVitalsEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State18SpawnEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07CloneCentralPixelProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07MenuSceneCallbackPlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08FormalSourcePixelPlayModeTests.cs
authority: formal NTSD 2.8-Logan playable KindCatalog28, GameSession28 and D-023 kind.dat; Q07 parser package
evidence: original Editor compiled; focused jobs f4929ac422444d028a5dfb98ccb7e8b7e8b7 3/3, e13957dd28f44e9baf7cab3206223363 16/16, b72251d3c6fa4837977d9152fbc65106 17/17; parity self-test 162/162; Ledger PASS
-->

# NTSD28-Q07-KIND-DAT-IDENTITY-PREPARED-WORLD-001

Before: kind file/parser selection is available but absent from the published battle identity, candidate freshness, World DAT lookup and trace header. Current five-component V2 vectors and native source-model remain historical. The Q09 additions already in `LoganVisualContentCandidate.cs` are protected and must be retained.

After target: seven-component V3 with distinct kind raw/semantic components, one frozen selected kind catalog in prepared World, exact publication freshness and version-aware trace validation. Do not change the locked combat constants in this package; consumer migration is independent.

Expected side effects: formal and staged content fingerprints/cache keys change; old sessions/trace headers remain valid only under their exact V1/V2 schema and cannot compare as current V3. Missing kind file uses the native locked fallback but has a distinct raw identity from the formal selected file. No Scene/resource bytes or nonbattle path change.

Invariants: current published object/fusion/optional-mode/kind and World identity all refer to one capture; the no-mode fixture has its own V3_KIND_ONLY tag rather than a fake mode fingerprint. Selected malformed kind file aborts rather than silently falling back. No post-seal reconfiguration. Preserve historical constructors, source-model V2 output, existing dirty work and ordered battle shutdown.

Exact focused test paths will be appended to the metadata before edits. Validation planned: original Editor compile, focused identity/candidate/World/trace tests, parity self-test, targeted Battle Scene smoke, Ledger validator and scoped diff. Report measured and unmeasured gates separately. Rollback per Task Contract; no reset/clean/delete is authorized.

Implemented: `LoganObjectCatalog` now captures selected or fallback kind input and uses a versioned content preimage. `LoganContentIdentity` preserves V1/V2 constructors/vectors, adds mode-present V3 and no-mode V3_KIND_ONLY with two kind fingerprints, and changes the formal selected semantic/projection to `B8B13894...5A45` / `96DE8D089438B1B8`. `LoganVisualContentCandidate` checks kind freshness before publication while preserving the pre-existing Q09 kill-icon edits. `BattleRuntimeDataCatalog.Prepare` verifies captured components and freezes the exact `KindCatalog`. Unity trace emitter and offline parity validator recognize both V3 variants and reject mixed headers. Combat candidate/transform literals and formal native V2 source-model remain untouched. Fixed formal fingerprint probes use the independently calculated V3 selected-kind vector; old V2 vectors remain in historical tests.

Validation: original Unity Editor refresh/import compiled `Assembly-CSharp.dll` and `Assembly-CSharp-Editor.dll` without `error CS` in the current compile segment. New focused test job `f4929ac422444d028a5dfb98ccb7e8b7` passed 3/3; identity/trace/fusion/mode/state18 adjacent job `e13957dd28f44e9baf7cab3206223363` passed 16/16; impacted Q06 spawn/weapon vector job `b72251d3c6fa4837977d9152fbc65106` passed 17/17. A first selector attempt used the wrong namespace and selected 0 tests; it is not counted. Offline `NTSD28Parity self-test` passed 162/162 and `dotnet build` had 0 warnings/0 errors. Direct `& ./Tools/Validate-ChangeLedger.ps1` passed (697 Records, 52 governed diff files); `git diff --check` passed. A first validator invocation through Windows PowerShell failed before validation because its default `$PSScriptRoot` was empty; the direct current-shell invocation succeeded. Evidence JSON is under `artifacts/diagnostics/NTSD28-Q07-KIND-DAT-IDENTITY-PRECHANGE-001/`. The new test `.meta` GUID is `8d643100733759c41a1e41192b5144d8`. Saved Menu/Battle Scene SHA stayed `6CF124A17F692325CED5774C3C10C3324AA047439BC6C3EB08856188A78AB4B1` / `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`.

Limits: no actual Battle Scene Play or Player cold-start after V3 was run; no native V3 authority source-model or same-seed trace exists yet. The Q06 focused test rewrote its tracked `Authority400-actual-births.json` and `MobileExtended-actual-births.json` outputs from their committed older catalog hash to current V3; they were left in place and are not treated as fresh formal equivalence proof. Current formal `kind.dat` combat consumers still use their old literals. Therefore this Record is `FOCUSED_TEST_PASS`; Q07/R15/goal remain open.

Real Scene addendum: original Editor's saved Battle Scene was used to trigger existing `NTSD28Q07MenuSceneCallbackPlayProbeEditor` with run ID `q07-kind-v3-menu-smoke`. Its final result `artifacts/diagnostics/NTSD28-Q07-MENU-SCENE-CALLBACK-PLAY-001/q07-kind-v3-menu-smoke.json` is PASS: formal V3 semantic `B8B13894...5A45`, Menu prewarm, Naruto OID2, additive BattleRunning, matching publication keys, ordered unload and Menu return, `Stopped` and pool borrowers 0. The request returned `requested:false/restorePending:false`. Post-run saved Menu/Battle Scene SHA remained `6CF124A17F692325CED5774C3C10C3324AA047439BC6C3EB08856188A78AB4B1` / `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`. This supersedes the preceding sentence's "no actual Battle Scene Play" limitation for the menu-driven publication path only; World.KindCatalog was checked in focused preparation, not inspected in this Play result. Player cold-start, native V3 same-seed trace and actual kind candidate/transform consumers remain pending.
