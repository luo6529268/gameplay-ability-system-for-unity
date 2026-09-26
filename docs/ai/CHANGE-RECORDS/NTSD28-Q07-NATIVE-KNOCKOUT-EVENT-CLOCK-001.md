<!-- CHANGE-RECORD
id: NTSD28-Q07-NATIVE-KNOCKOUT-EVENT-CLOCK-001
status: FOCUSED_TEST_PASS
change-kind: Q07_NATIVE_KNOCKOUT_EVENT_CLOCK
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardReducedKnockoutProducerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NegativeEnvironmentRecoveryProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07MenuSceneCallbackPlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10KnockoutModeSoundEditorTests.cs
authority: formal root NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable BattleWorld28 record_native_knockout plus C24 sequence boundary
evidence: docs/ai/TASKS/NTSD28-Q07-NATIVE-KNOCKOUT-EVENT-CLOCK-001.md; artifacts/diagnostics/NTSD28-Q07-NATIVE-KNOCKOUT-EVENT-CLOCK-001/ACCEPTANCE.md
-->

# NTSD28-Q07-NATIVE-KNOCKOUT-EVENT-CLOCK-001

Pre-change: `SimulationWorld.RecordNativeKnockout` serializes `CurrentTickIndex`, which is advanced before hit. The formal event writer uses `sequence_` and advances it at C24 after ordinary hits. The original Editor same-state Naruto KO event is born on completed tick8 in both runtimes but stores formal7/Unity8. Unity has an existing `NativeFrameSequence` carrier advanced at C24 and included in runtime reset/snapshot/checksum. Existing focused B5 test and two Play probes assume KO time equals host tick; this assumption is invalid for an ordinary pre-C24 hit.

Declared behavior: only the common KO event timestamp source changes to the native frame sequence at the call site. All producers share the writer; pre-C24 events see prior sequence and post-C24 events see current sequence without producer-specific branches. Add/adjust focused test assertions for both phases; keep attribution, birth, mode audio and structural state untouched. Side effects are event expiry/row timing and checksum/snapshot values of newly produced events. No DAT, resource, Scene, mode Asset, UI production or nonbattle code change. Acceptance and exact rollback are in the Task.

Post-change code written: `SimulationWorld.RecordNativeKnockout` now reads `NativeFrameSequence` at the shared event write, explicitly narrowing to the existing int event field. The B5 ordinary/reduced lethal test differentiates host tick8 from pre-C24 sequence7; the B5 resource-recovery test differentiates host tick9 from post-C24 sequence8. The existing Q07 Menu Play and Q10 mode-audio probes now check their ordinary pre-C24 event against the prior native sequence while retaining completed-tick collision/audio checks. These are the five declared script paths only. Original Editor compile, focused runs, exact Naruto recapture, feed lifetime and governance are pending; do not claim Q07 complete.

Focused validation: original Editor refreshed and both changed assemblies are newer than sources; Console errors 0. Two clock-path tests passed 2/2 (job `7990a04b97504912b48ede5251f7c9a3`); Q08 event lifetime/snapshot/checksum/reset 3/3 (job `0f1c7f586f154bbc8b3ac627792cd468`); Q10 selected cue conditions 5/5 (job `6dd8d8d1f9464e5b8f7710428082a289`). Exact two-Naruto original-Editor 30tick recapture PASS; old raw/domain/input-RNG payloads each 30/30 equal, first KO completed tick8 now event time7 and all other formal fields equal. Q09 row projection test job `937aed37549049acb40e2924ea3b038e` failed in SetUp reading user-excluded `mode/ntsd.dat`, not from KO clock behavior; no DAT restored. Initial Q10 method filter selected 0 tests and is not counted. Menu/Battle/GameConfig disk SHA unchanged, Editor idle EditMode. Q07/Q10 Play probes, Q09 row visible end tick, formal pixels and Q07 aggregate exit remain pending. Governance/diff check recorded below.

Governance: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` exited 0 with 855 Records and 12 governed code files covered; `git diff --check` exited 0. The validator warnings are historical metadata path warnings; no errors. See `change-ledger-validation.log` and `ACCEPTANCE.md`.

Play follow-up: one existing real-Menu physical-J probe with the updated pre-C24 event assertion returned `PASS` under unique run `q07-ko-native-clock-menu-play-20260926-1`; authored Naruto frame513 KO, native attribution, timer350/transition2, production selection return and zero pool borrowers were observed. The original Editor returned to idle EditMode on Battle Scene; Console errors 0, request restore flag false, Menu/Battle/GameConfig disk hashes unchanged. The new run has different initial state and KO completed tick102; do not merge its tick count into the exact Editor/source tick8 parity claim. Play report and scope in `ACCEPTANCE.md`. Q09 row fixture/pixel remains open.

Q09 dependency follow-up: `NTSD28-Q09-PROJECT-MODE-TEST-FIXTURES-001` replaced the excluded mode-DAT test input with the approved project Asset snapshot. Original Editor affected 9/9 focused tests now pass, including the row's synthetic 30-tick boundary; the old SetUp failure is resolved. This is not an exact same-state production event-to-pixel lifetime witness. Q07 remains `FOCUSED_TEST_PASS`, with that acceptance still open.
