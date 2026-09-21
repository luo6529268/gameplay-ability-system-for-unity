<!-- CHANGE-RECORD
id: NTSD28-Q06-FUSION-PREPARED-WORLD-CATALOG-001
status: VERIFIED
change-kind: FUSION_PREPARED_WORLD_CATALOG
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeDataCatalog.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionPreparedWorldCatalogEditorTests.cs
authority: Current GameSession28 prepared fusion data and approved composite identity.
evidence: RED4 then Unity9/9 job20024fe8ede64b78a70e919d4c6bf2d3, CS0, actual formal capture, independent review; artifact ACCEPTANCE.md.
-->

# Prepared fusion World catalog

VERIFIED / PREPARED_DATA_WIRING_ONLY. Parent NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001. Prerequisites immutable parser/input freeze/composite identity VERIFIED.

Authority: current formal NTSD2.8-Logan EXE B1E13A...19033 and source GameSession28 load fusion catalog before constructing tick options; FusionSystemRules consumed by BattleWorld28::advance_native_fusions. This task publishes that already-frozen table to existing per-World prepared data, not activates old fusion consumer or changes authority.

API: BattleRuntimeDataCatalog.Prepare and SimulationWorld.PrepareRuntimeDataCatalogForBattle gain optional trailing LoganObjectCatalog loganCatalog=null. Prepared catalog holds only immutable FusionCatalog and LoganContentIdentity (no filesystem locator/reader); both null explicitly means legacy/test content without declared Logan input, NOT locked fallback. Native missing-only fallback remains selected by captured LoganObjectCatalog. Validate complete current identity plus object definition count/ordered id/type consistency before clearing existing state. Reject mismatched paired object catalog before mutation. Sealed replacement fails as before; Unseal followed by legacy Prepare clears both new references. No new lifecycle owner or resource; references live with existing World/runtimeDataCatalog, unseal remains ordered shutdown step4, disposed World owns release. Tick reads no IO/singleton.

Host: CharacterAnimtorManager exposes internal immutable PublishedLoganCatalog from its committed candidate. SimulationTickDriver supplies it through existing pre-seal Prepare; require its ContentIdentity agrees with GameDataManager committed identity before preparing, never alter publication transaction order or UI/config selection. Raw capture explicit Logan fixture and replay both pass their actual captured catalog so identity/table originate together; legacy diagnostic path unchanged.

Exact6 paths predeclared. Focused tests first with reflective API discovery for measuredRED; verify table/identity pairing, sealed rejection, reprepare legacy clears old state, mismatch has no mutation, immutable captured table survives underlying file edit. Existing catalog allocation test reused once. No new carrier/schema/runtime fusion behavior; no Scene/resources/nonbattle/framework edits. No new manager/worker/queue/shutdown sequence. Rollback only reviewed task hunks, no destructive Git.

Validation: current Editor compile, focused new tests plus existing single runtime catalog regression; actual related raw capture (existing Q05FormalLogan test) for publication/cleanup. No unrelated SelfCheck/Play sweep. On completion record exact evidence and retain full fusion transaction pending. Broader source4/Play validation deferred until behavior integration.

Review refinement (same declared paths): stage Logan resolver results before mutation and reject non-null wrappers whose characterId differs from indexed object. Null remains permitted for intentionally partial diagnostic loads. This verifies wrapper identity only, not DAT bytes; exact source configs rely on existing committed manager/dataScope contract. Resolver throw must leave prior catalog intact. Host bidirectional ReferenceEquals check before Unseal.

## Final evidence

# Prepared fusion World catalog acceptance

VERIFIED / PREPARED_DATA_WIRING_ONLY.

Six declared scripts implemented. Existing World runtime catalog now holds frozen LoganFusionCatalog and matching LoganContentIdentity. Host uses committed candidate, verifies same identity reference in both managers before Unseal, then passes catalog through pre-seal preparation. Both current raw capture and replay pass the actual dataScope catalog. Legacy null does not invent formal fallback; successful legacy reprepare clears prior new references.

Logan preparation checks complete V3 component pairing, ordered definition IDs/types and non-null resolver wrapper IDs before clearing old state. Resolver results are staged once; resolver exception and mismatches leave previous dictionaries/generation/references unchanged. This validates identity pairing, not arbitrary resolver DAT bytes. Formal host/config provenance relies on existing publication contract. Null configs remain permitted for partial diagnostic scopes; table availability is not proof all fusion participants/targets have runnable configs.

Evidence:
- Initial four focused tests failed as expected, missing fourth captured-catalog parameter, job e0c3bc80f51e4d248523c24f48497467; red-results.xml preserved.
- Final joint job 20024fe8ede64b78a70e919d4c6bf2d3: 9/9 PASS, 12.1205033s. New7 + existing RuntimeDataCatalogFreezesManagerLookupsBeforeWorkerExecution1 + FormalLoganCaptureUsesActualInputsAndRestoresExistingManagerReferences1. results.xml.
- New cases: frozen table/identity references and no later file read; sealed replacement refusal; legacy reprepare clearing; mismatched definitions no mutation; bad wrapper and resolver throw no mutation; actual World forwarding and Seal.
- Existing catalog test retains zero-allocation read-loop evidence; existing formal capture executes actual three-tick production diagnostic path and restores manager references. unity-prepared-formal.raw.jsonl archived.
- New captured full V3 content header equals previously verified header (header-preservation.json). Full raw 47/3 status not upgraded; previous composite neutral comparison retained, not rerun unnecessarily.
- Editor completed fresh compile/domain reload; Console error-CS query0. Independent read-only final6 diff review found no blocker.
- Scene file SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6 unchanged; no resources/Scene/nonbattle/framework changes, no computer-use/commit/push.

Boundary: new fusion table and identity are immutable; existing config wrappers are not deep-frozen. Existing legacy resolver failure semantics not redesigned. No new lifecycle owner/queue/disposable; references follow World, existing ordered Unseal phase unchanged. Production Host guard source-reviewed; this package has no new full-scene Play/cadence/renderer/shutdown run, since fusion behavior remains unactivated. No fullSelfCheck or unrelated battle matrix repeated.

Next: precise persistent fusion carrier/schema and producers (display190, AIalias/drop, global feature pair;318 reuses RenderPicOffset), then complete source4 merge/defuse transaction including atomic refusal and suspended-partner preservation. Do not use this getter/data seam as completion of fusion behavior. Q06 incomplete, Q07 formal resources not migrated, total goal ACTIVE.
