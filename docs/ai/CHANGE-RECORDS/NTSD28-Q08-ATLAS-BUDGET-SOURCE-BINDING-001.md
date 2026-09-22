<!-- CHANGE-RECORD
id: NTSD28-Q08-ATLAS-BUDGET-SOURCE-BINDING-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingDevicePolicy.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCommonAtlasBindingEditorTests.cs
authority: formal Logan content battle presentation must publish within declared atlas budget using existing valid source texture bindings
evidence: ATLAS-ORDERED-STREAMING-PARTIAL.md; 145-page Auto atlas exceeds 512MiB and real Scene still crashes at page119
-->

# NTSD28-Q08-ATLAS-BUDGET-SOURCE-BINDING-001

Created before script edit. Before: prewarm stages source textures/sprites and decoded pixels; Auto's array-budget rejection falls to unlimited ordered atlas pages, duplicating image residency and crashing before Q08 result assertions. Existing central `SourceTexture2D` is validated for oversized/excluded sheets and shares the fallback material with ordered pages; mesh segments preserve command sequence. Planned after: Auto plan over the existing `AtlasMemoryBudgetBytes` selects already-staged source bindings with explicit effective-mode diagnostics; smaller/explicit atlas modes retain prior behavior. Risks: source fallback can increase draw segments and may differ at sampling edges, so focused binding tests, full-content Scene runtime and representative pixel/order acceptance are separate gates. Exact scope, rollback and validation are in the Task.

Implemented in the three declared scripts: `BattleRenderingDevicePolicy` recognizes `SourceTexture2D`; both character-only and unified publication paths select it when Auto's planned atlas bytes exceed the budget, retaining existing source bindings without allocating an atlas. Explicit ordered and array paths remain selectable. The focused unified-publication test initially failed 3/4 because only the character-only path had changed; after the unified-path correction the class passed 4/4 in the isolated Unity project (`ATLAS-SOURCE-BUDGET-FOCUSED-AFTER.xml`, process exit 0, SHA-256 `A07582DA72A525982E8A07E84553852B23634497ACF13D454A556C8DE0FA3622`).

The formal-content D3D11 Battle Scene second-result test then passed 1/1 in the isolated project, process exit 0 (`UNITY-SECOND-BATTLE-SOURCE-BUDGET.xml`, SHA-256 `E70E58EAA8D64150B314F843E8320F97D3CAD5CB3F5317A9A552B692D7692B77`). External two-second sampling recorded maximum private bytes 13,242,007,552. No atlas allocation trace line appeared in this run. This is targeted Scene result-path acceptance, not representative pixel/order or formal EXE visual-parity acceptance. Q08 and the master goal remain open. Rollback is limited to this Record's three declared script hunks while preserving the prior ordered-page streaming change.

Follow-up source-order test: initial target RED 3/4 showed the test had incorrectly expected a copied binding; corrected to require exact retained source binding object. Isolated Unity EditMode class then PASS 4/4, exit 0, ATLAS-SOURCE-BUDGET-ORDER-FOCUSED-AFTER.xml SHA-256 3B99B46D43B8ED3E91F64D6F57CA6D8C4A056F1CF42448122A78F13B37DB7FDA. Interleaved shadow/spark/word/shadow commands resolved as four ordered source-texture segments with preserved command indexes. Scene pixel/EXE comparison remains pending.
