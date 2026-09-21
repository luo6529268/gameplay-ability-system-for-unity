<!-- CHANGE-RECORD
id: NTSD28-Q06-ORDINARY-STAGE-DISPLAY-BIRTH-001
status: VERIFIED
change-kind: ORDINARY_STAGE_DISPLAY_BIRTH
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeDisplayWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageWaveModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06OrdinaryStageDisplayBirthEditorTests.cs
authority: Formal BattleWorld28.spawn_at and GameSession Stage SpawnRequest.hp; audit DISPLAY-BIRTH-RETURN-AUDIT-001.
evidence: Seven genuine display REDs plus one corrected snapshot fixture; joint21 PASS; fullSelfCheck08:15:15Z/PlayRenderer2/Q05close08:15:56Z PASS; see ACCEPTANCE.
-->

# NTSD28-Q06-ORDINARY-STAGE-DISPLAY-BIRTH-001

CURRENT: VERIFIED / ORDINARY_STAGE_DISPLAY_BIRTH. Final artifact: artifacts/diagnostics/NTSD28-Q06-ORDINARY-STAGE-DISPLAY-BIRTH-001/ACCEPTANCE.md. All four declared script paths covered. Independent final review PASS/GO. Historical staged statements below retained. Parent display still waits for state9996 direct-spawn resource semantics; Q06/goal active, Q07 not migrated.

IN_PROGRESS / FOCUSED_RED_FIRST. Four exact paths declared before edits. Initial work is test fixture only; production waits for observed RED.

Before: ordinary Initialize does not initialize eight display fields; Stage factory/finalHP override leaves prior OPoint displayHP. Intended after: pure display birth initialization at finalHP assignment, keeping all non-display truth untouched. Snapshot shell temporary initialization is overwritten by full runtime restore; test must prove preserved snapshot display, not manually repair after restore.

No persistent schema, runtime manager or shutdown-stage change. Scope, invariants, acceptance, dependencies and rollback in same-ID Task. Preserve closed OPoint/weapon-piece/fusion/reset responsibilities; clone direct-spawn resource mismatch remains separately pending, so parent display/Q06/goal not closed. No Unity tests or script modifications observed at record creation.

Initial actual RED jobdbc0225d91f24b2e859214beab782b4f:8 executed/8 failed. Seven genuine display differences (ordinary clean0 instead137, dirty101 retained, pooled/factory/result500 instead137, directStage0); one fixture assumption incorrectly required a different CLR reference after snapshot transfer while existing shell could be retained. Correct snapshot fixture by freeing original after capture/clear-local, asserting target slot empty before restore, and requiring restored markers after actual reconstruction; pool may legally return the same CLR object. This correction does not alter production restore.

Production now implementing only the declared display initializer and four finalHP publication sites (characterInitialize, Stagefactory, Stagecontract, Resultsreserve). OPoint/pieces/fusion/reset untouched. Root added factory type3 representative to existing test in addition to type0. No source matrix rerun: previous display980 spawn137 evidence has matching current formalEXE/closure hashes.

Actual implementation and joint validation: jobf957ffb0f4f24a4aa217fbad037dd7a3 21/21 PASS,0.9658268s (new8 + olddisplay13 including980 source vectors). Missing-shell restore now empties slot before restoration and permits legitimate pool object reuse; eight markers preserved. Production3 paths changed, no source/resource/Scene edits. Test-only next stage within declared fixture: renderer ordinary Initialize137 and renderer Stage type3 factory final137 in actual Play, main Scene checksum/borrowers guards, then existing Q05 closure. Add no runtime owner. Stable package once fullSelfCheck; no unrelated all-character tests.
