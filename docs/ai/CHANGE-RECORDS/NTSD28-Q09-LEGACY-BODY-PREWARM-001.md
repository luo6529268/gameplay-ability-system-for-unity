<!-- CHANGE-RECORD
id: NTSD28-Q09-LEGACY-BODY-PREWARM-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2ObjectPool.cs
authority: formal NTSD2.8-Logan state9997 visible-body path and original Battle LegacyOnly missing SpriteRenderer RED
evidence: docs/ai/TASKS/NTSD28-Q09-LEGACY-BODY-PREWARM-001.md
-->

# NTSD28-Q09-LEGACY-BODY-PREWARM-001

Pre-script record. `LF2ObjectPool.CreateNewObject` currently instantiates `EntityObject.prefab` or its minimal fallback with `LF2ObjectRenderer`, but neither supplies an `EntityModel` body `SpriteRenderer`. The original Battle LegacyOnly natural Karin OID314 reaches `LF2ObjectRenderer.SetLogicObject` and cannot show its body. The declared edit will conditionally add a missing body component during pool object creation before the allocation seal, based on the same resolved backend as the battle World. It will not alter the prefab or the CentralOnly path. See the Task Contract for exact scope, side effects, validation and rollback. No production script edit is claimed yet.

CODE_WRITTEN / COMPILE_PASS: `LF2ObjectPool.CreateNewObject` now uses `BattlePresentationBackendResolver.Resolve(Cfg)`, the same resolver used by `SimulationTickDriver.CreateProductionWorld`, and adds one missing `SpriteRenderer` to `EntityModel` only when mode is not CentralOnly. This runs during pool creation/preparation before battle allocation sealing, including the fallback object path. Existing body components are reused; no DAT, prefab, Scene, Asset, camera, tick or nonbattle script was edited. `dotnet build Assembly-CSharp-Editor.csproj --no-restore` exited 0 with 227 warnings and 0 errors; log `artifacts/diagnostics/NTSD28-Q09-LEGACY-BODY-PREWARM-001/generated-editor-build.log`. The original Editor accepted `refresh_unity` and its runtime assembly timestamp now exceeds the source edit; its bridge temporarily disconnected during domain reload. Original Editor compile/Play and CentralOnly no-extra validation remain pending.

VERIFIED_SCOPED_LEGACY_PLAY: original Editor imported the code, returned idle/clean, and ran one opt-in original Battle LegacyOnly probe. The natural full Driver step3 OID314/action50/state9997/owner8 had a real body SpriteRenderer enabled and right-facing. Its world X/Y equaled the archived same-input CentralOnly body command exactly (0/0 difference); content/mode fingerprints matched. Cleanup restored objects/slots/borrowers 4/2/2, pause and non-Play/clean Scene; both Scene and saved GameConfig SHA-256 values remained unchanged. The default CentralOnly Asset still selects the branch that skips the new component, verified by the exact code predicate and Asset value; no fresh CentralOnly Play was needed for this additive Legacy-only change. Report and immutable raw RED/GREEN: [Acceptance](../../../artifacts/diagnostics/NTSD28-Q09-LEGACY-BODY-PREWARM-001/ACCEPTANCE-20260928.md). No root EXE same-view/GPU claim and no P-12/Q09 aggregate closure.
