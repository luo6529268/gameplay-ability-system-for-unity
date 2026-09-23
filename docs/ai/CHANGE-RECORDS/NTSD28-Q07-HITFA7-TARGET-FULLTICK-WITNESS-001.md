<!-- CHANGE-RECORD
id: NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001
status: CODE_WRITTEN
change-kind: DIAGNOSTIC_TARGET_PRESENT_FULL_TICK_WITNESS
code-path: Tools/NTSD28AuthorityTrace/hitfa7_target_fulltick_main.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28/SimulationTickDriver28/native_ai/frame_motion/physics
evidence: Q07 target-present local seam omits formal action55 dvy1 and generic scenario lacks target slot/initial velocity
-->

# NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001

Before: existing source-model/Unity kind first-difference recapture covers missing-target behavior7. Target-present OID875/action55 has only local AI/physics-seam assertions; the formal frame-motion `dvy:1` lies between them in a full tick. Current generic scenario runner does not inject initial target slot or velocity, and current source raw schema has no previous-Y field. A completed-tick claim is unsupported.

Intended after: single-purpose, fail-closed repository diagnostic witness of formal source-model target-present complete tick, plus original Unity Editor real-driver same-initial-state test. The pre-edit scope changed from modifying the existing shared runner to a new source file because the shared runner has unrelated dirty R15 changes. Record first difference or bounded equality without altering production behavior or the generic scenario contract. Complete details and rollback are in the Task.

Validation pending. This is not a formal EXE observable certificate or Q07 completion.

2026-09-23 code written: new `hitfa7_target_fulltick_main.cpp` uses unchanged paired playable source and strict OID99/action0 plus OID875/action55 legal slots0/1 fixture. After `GameSession28::initialize` it sets only target slot0 and Vy3.8, then calls the real session step twice and emits action/motion/integer/precise/previous-Y/entity-count rows. `Build-AuthoritySourceCapture.ps1 -RunnerSource` built it successfully with formal EXE SHA `B1E13...9033` and source manifest `07CD47...778F`; build manifest and source witness were saved in the artifact directory. Source tick1 was action55, Vy5.2, integerY-20, preciseY-20.600000000000001, previousY-30; tick2 action60, Vy0, integerY-15, preciseY-15.400000000000002, previousY-20. The existing Q07 Editor test file now adds a real-driver same-initial-state two-tick comparison against those rows. Unity compilation and the new test have not yet run; do not claim cross-end equality.

Fixture correction: first Editor test job `2a441428c23c4e1d803468d7a81d79ce` failed before battle setup because the existing diagnostic wrapper requires a frozen Stage23 three-tick scenario with Z in 542..712. The initial two-tick Z100/120 source output is retained as a superseded invalid-for-Unity-fixture observation, not cross-end evidence. The script and scenario have been revised to three ticks with subject/target Z600/620, preserving the 20-unit target depth difference; rebuild and recapture are required before Unity comparison.

The corrected dedicated runner rebuilt successfully (runner SHA `E09082407F562DA711B61DE5C6FCAF1B8A29A6C58E35B62E637C150C332364D1`, binary SHA `DA8297D82DBF792E4866B56EC630C93448131F1851BA4C58FD88D7B8BFFA18FE`, unchanged paired source manifest `07CD47...778F`). The Stage23 three-tick source witness is saved separately. Tick1 action55/Vy5.2/YInt-20/preciseY-20.600000000000001/previousY-30, tick2 action60/Vy0/YInt-15/preciseY-15.400000000000002/previousY-20, tick3 action61/entityCount3. Unity current test code targets this corrected witness, but original Editor compile/run is pending while the Editor is in a separate Play transition; no parity claim yet.

Original Editor jobs `d0e2a28cf96f4dc0a88f33687c87669d` and detailed retry `847a2c1fd4614a61ba0d739740455b40` reached the full Driver but failed with `NullReferenceException` at `LF2ObjectPool.Get` during tick3 late OPoint materialization. The stack identifies a diagnostic renderer-pool prerequisite, not a measured hit_Fa7 motion mismatch. The focused test is now bounded to source/Unity initial + completed ticks1/2, which include the action60 threshold and formal dvy frame-motion/physics order; source tick3 birth is retained as a separate pending OPoint/diagnostic-pool gate. The callback still uses a legal 3-tick Stage23 scenario because the existing wrapper requires it. Recompile and compare are pending.
