# NTSD28-Q07-DEAD-NONCHAR-HITFA-MOTION-GATE-001

Status: `FOCUSED_TEST_PASS / CURRENT_COMBINED_TREE`. Parent BATCH-04/Q07/D-024 remains open. The later type-3 correction resolved this Task's downstream tick3 difference; historical package-local status is preserved below.

Authority and trigger: the formal paired playable `NativeAi28::step_non_character_hit_fa` returns before the common target-motion branch when `subject->current_hp <= 0` (`source/ntsd28_core/src/simulation/native_ai.cpp`); its full-session indexed `w/e.dat` hit_Fa5 child has tick1 X103 and post-friction Vx2. The original Unity Editor complete Driver currently yields source-rule X105 after the earlier birth-HP and action first differences were fixed. Unity `LF2Entity.RunHitFa2Or4Or12Or14FrameLogic` instead calls `ApplyHitFa2Or4Or12Or14NoTargetCatch` when HP is nonpositive; that method adds 2 to nonnegative Vx. This static explanation must be confirmed by an original-Editor RED velocity assertion before the production edit.

Exact write scope: `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HitFa5FullDriverEditorTests.cs` adds the first-tick formal post-friction child-Vx assertion before the X comparison; `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` changes only the common noncharacter hit_Fa 2/4/12/14 nonpositive-HP gate to return without no-target motion. This is a shared semantic gate, not an OID219 special case. Do not edit DAT values, content assets, scenes, nonbattle scripts, cadence, pass order, or camera.

Acceptance: original Editor RED first-tick child Vx; apply the one-branch fix; recompile in the original Editor and run the paired positive/negative eight-tick full-Driver cases, plus narrow neighboring hit_Fa dead/alive cases if present. Check protected Scene/config SHA-256, ledger validator and `git diff --check`. A later first difference keeps Q07 open and needs its own task. Source paired playable is not root EXE same-world or natural Battle Play evidence.

Rollback: only the two declared code edits, after the required explicit approval for destructive Git restore; preserve all pre-existing dirty work.

Test-first result: original Editor complete-Driver group1 job `7fdb72d46e214e79b4075de3871d063a` failed exactly on new tick1 child Vx expected2/actual4. The one-branch production edit is written; post-edit validation pending.

Post-edit neighbor check addition within the declared test file: expose the existing `BattleRuntimeSelfCheck.CheckCurrentDatFrameLogicSharedRouting` as a focused Editor assertion. Its representative live hit_Fa 3/4/14 checks can run independently because the full SelfCheck currently halts at an unrelated alternate-damage odd-dvx expectation before reaching them. Keep that whole-SelfCheck failure visible; do not change its damage assertion under this task.

Scoped outcome: original Editor post-edit group1 completed tick1 formal HP/action/Vx/X/Z/target checks; group3 negative eight-tick run passed. New first difference is group1 completed tick3 child action formal2/Unity1. Focused live hit_Fa representative check passed 1/1; full SelfCheck failed on a separate alternate-damage expectation and remains visible. Four protected hashes and ledger/diff checks passed. See same-ID `PROGRESS.md`; no natural Play or full positive parity is claimed.

Current-tree correction (2026-09-26): independent `NTSD28-Q07-TYPE3-DEAD-SERIAL-COUNTER-001` then passed the exact positive/negative eight-tick complete-Driver pair 2/2 and its type-3 neighbor 1/1; the later state1002 test-only correction recorded a fresh full SelfCheck PASS. The preceding scoped outcome is the original run, not the present acceptance state. Natural Play, root EXE same-world and aggregate Q07/D-024 remain open.
