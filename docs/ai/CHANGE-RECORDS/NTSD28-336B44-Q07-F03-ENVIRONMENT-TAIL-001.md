<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F03-ENVIRONMENT-TAIL-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterFrameAdvancePass.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4State1218EnvironmentCreditEditorTests.cs
authority: selected formal 336B44 playable BattleWorld28::step_physics final-frame environment_state_320 tail
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F03-ENVIRONMENT-TAIL-001.md
-->

# NTSD28-336B44-Q07-F03-ENVIRONMENT-TAIL-001

Created before scripts. Formal physics tail tests final selected action's frame state, clearing +0x320 unless that state remains12; frame-suppressed kind2 returns before this tail. Unity direct landing currently leaves the post-damage marker1, and ordinary nonstate12 retains pre-existing marker. Add test-first RED in the existing owned B4 class, then one shared type0 final-frame tail called by the three character physics paths after action selection; preserve C031 equality and C032 cue timing. No DAT/WAV/Scene/config/nonbattle modification. Rollback only the exact declared hunks after review. Full formal EXE same-state and natural Play evidence remains separate.

Original Editor RED job `d60d5e991325426294dfad4725fdbf33` executed the two rebaselined cases: penetrating state12 final action230 expected EnvironmentState320=0 but got1; ordinary nonstate12 expected0 but got8. The shared type0 `LF2Entity.ApplyCurrentDatType0EnvironmentPhysicsTail` is now written and called after action/airborne resolution by the three declared character physics paths, before integer sync. It clears the marker when final frame is missing/nonstate12; the existing kind2/hold/interaction early returns skip it. Noncharacter paths are unchanged. Status `CODE_WRITTEN / UNITY_GREEN_PENDING`; compile, focused result and old fixture corrections remain.

Original Editor intermediate job `7f6b159e01734bd78cf981f197980806` rebuilt zero-error and passed the penetrating-contact case; the combined airborne/nonstate12 case failed first at airborne marker expected8/actual0. Call-chain inspection found its synthetic frame table contains action170 but omits the airborne destination action182. Unity selects182, so the formal final-frame-null tail legitimately clears the marker; the test fixture cannot prove state12 retention without defining that selected frame. Before further test edit, the already-declared B4 test file may add the actual 180..183 falling destination frames, assert the airborne final frame is state12, and update the other old post-landing `EnvironmentState320==1` expectation to0. Then rerun the two cases and the whole class. No production path change is authorized by this fixture correction.

After that fixture correction, original Editor job `f6465a85a7dd4aa380ffa9b574649d22` passed all 21 tests across the B4 environment and contact-action classes. F03's direct path and C031 adjacent action are green. Before final status, the already-declared B4 test file's DataOrientedCanonical complete-tick equality/penetration pair may add an initial marker10 and assert final marker10/0 and HP100/90 respectively, alongside its existing C032 sound and action assertions. This is a focused canonical tail witness, not a production change or broad test matrix.

The focused canonical test enhancement was applied; original Editor job `509f05f863fe4000b8d64c145ca5da86` executed both cases and passed 2/2. Equality retained EnvironmentState320=10/HP100; strict penetration selected action230 and ended at marker0/HP90 with one channel6 cue. The earlier 21/21 adjacent result remains valid. An initial test request timed out at the MCP command layer while a job started; it was not used as pass evidence. F03 code and focused complete-tick behavior pass. Formal root EXE paired state, natural Battle Play and whole Q07 exit are unverified, so status is `RUNTIME_PENDING`. Rollback is limited to the declared four script-file hunks and coordinated record update; do not touch protected assets.
