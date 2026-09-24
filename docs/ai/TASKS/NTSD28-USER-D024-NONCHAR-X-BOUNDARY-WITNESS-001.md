# NTSD28-USER-D024-NONCHAR-X-BOUNDARY-WITNESS-001

Status: FOCUSED_TEST_PASS, defect characterization only. D-024 non-perceptual inspection before Q07.

Authority: formal root NTSD2.8-Logan EXE SHA-256 B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable `BattleWorld28::settle_ordinary_stage_bounds` in `source/ntsd28_core/src/simulation/battle_world.cpp`.

Pre-change: playable grounded non-type0/type3 X cull uses strict `<100`/`>width`; OID122/123 protected branch clamps `[100,width-100]` under its declared participant/mode conditions. Unity `LF2Entity.ApplyPreFrameXBounds` uses `<0`/`>width` and `[10,width-10]` with only `Unk344>0`. These are static source differences, not yet runtime first-difference proof. User's choice of source-rule versus physical position for D-024 object lifetime is pending.

Declared path: add only `Assets/NTSD/Scripts/Test/Editor/NTSD28D024NonCharacterXBoundaryWitnessEditorTests.cs` and its Unity-generated meta. No production code, DAT, Scene, Prefab, camera or ProjectSettings edit. Test exact factor-1 ordinary object and protected OID boundary cases through existing production method, initially as formal-expected RED, then retain a passing characterization of observed current behavior without calling it parity.

Acceptance: original-project Editor compiles; focused test result freshly reports the actual boundary behavior; compare with source constants and mark the first difference at its supported scope. Do not change X cull domain or Q07 status from this test alone. Rollback: remove only the declared diagnostic test/meta after provenance review.

Result: original Editor formal-expected job `cb6bd8d3a2134117b0cb3bada32fae32` RED 3/3: ordinary grounded OID150 at X50 remained alive instead of formal cull; protected OID122 at X50/X790 stayed there instead of formal X100/X700. Retained characterization job `3bdacb2565dd4a8bbd5e34cd01abf5fd` 3/3 PASS. Both are direct production-method EditMode cases at width800/factor1, not full Driver/Scene/EXE parity.
