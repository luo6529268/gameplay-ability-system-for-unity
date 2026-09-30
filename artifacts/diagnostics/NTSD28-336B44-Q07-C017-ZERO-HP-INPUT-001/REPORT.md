# NTSD28-336B44-Q07-C017 zero-HP input

Status: `UNITY_FULL_TICK_PASS / RUNTIME_PENDING`. Parent: BATCH-04/Q07. This is a scoped input-rule result, not Q07 closure or formal-release parity.

Authority: selected root `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`. Its declared playable `source/ntsd28_core/src/simulation/input_routing.cpp::InputRouter28::step_sampled` has no global type-0 `current_hp<=0` input clearing or early return. Current `tests/input_routing_tests.cpp` supplies a zero-HP standing Attack positive control, a state-14 negative action control that retains the sampled Attack, and a poison pulse that reaches exact zero HP before standing Attack. Those source tests are implementation evidence, not a same-state observation of the root formal EXE.

Unity first difference: canonical `NTSD28NativeComboStateMachine.ProcessSampledInput` cleared sampled keys and history at HP<=0, and `NTSD28InputTwoPassModule.ProcessNativeSampledState` skipped remap and all existing action routes at HP<=0. Only these two global HP branches were removed. State-specific action gates, including the rowing redirect HP rule, were left intact. Three existing Editor test classes were updated with the standing, sampled-state, and terminal-state controls. No DAT, resource, scene, input asset, framework, or nonbattle flow changed in this package.

Original Unity 2022.3.62f3 Editor on the original project:

| Evidence | Result |
| --- | --- |
| Exact new test RED, job `7c92a09e2120458581c0fb4be4deb752` | 2/2 failed before production change: pure sampled Attack mask 0 rather than 16; standing action did not choose authored 60/65. A preceding 0-selected-tests attempt had a test-assembly compile error and is excluded from RED. |
| Exact new test GREEN, job `ed0ff8782bf64545a3f46502ef837a94` | 2/2 passed after removing the two global branches. |
| Old B1E13 expectation detection, job `dc502297183f4c2e9c55e977f2c39bf1` | 2/2 failed as expected: old tests required unconditional zero-HP clearing and no standing Attack. |
| Rebaseline first run, job `842329d2bfd8495181e66fd5165ae7c4` | Sampled-state case passed. State-14 case kept frame 14 but expected `AnimSub=17` while normal per-tick decay produced 16; assertion corrected. |
| Four exact C017 cases, job `50a9c3bea5f64fffab12286687e8cd31` | 4/4 passed, 0 failed, 0 skipped: pure sampled/history, canonical second-pass state retention, standing Attack, state-14 action rejection. Original Editor compiled and completed domain reload before this run. |
| Corrected complete-tick cases, job `166dfdeeb710403fa3e50ba19fbff35f` | 3/3 passed, 0 failed/skipped after compile/reload: zero-HP standing Attack, zero-HP state14 Attack rejection with sampled input retained, positive-HP standing control. The external UI Attack maps through the existing crossed `SimulationInputButtons.Jump` bit; held input crosses the actual phase-1 deferral to phase-0 sample. Earlier jobs `bcaf4cc5f2c34359966f8e46cad6190d`, `9d7d1ec0c46b4ae8b8340eaa25517953`, and `d7e15eede1034b0fa913aab106791bdc` failed because the synthetic fixture assumed immediate tick1 sampling and then sent the wrong external button bit. They do not establish a C017 production difference. |
| Complete-tick production poison pulse, job `26faf2a6988b499db12325c140ffbc31` | 1/1 passed: seeded HP5 and runtime poison timer33/type1/strength5; first battle tick decremented timer to32 and HP to exactly0 while standing, second tick sampled held external UI Attack and selected authored 60/65. A previous bridge run request timed out without a usable result and is excluded. This tests the runtime poison transition, but not DAT-delivered Pur contact or root EXE same-state. |

Remaining evidence: original Battle Scene Play through DAT-delivered natural exact-zero poison if selected for C017 completion, plus same-seed, same-state formal root EXE versus Unity trace. The corrected three-case result proves a synthetic complete Unity tick path, and the additional one-case result proves the production poison countdown within that path. Do not promote this result beyond `UNITY_FULL_TICK_PASS / RUNTIME_PENDING` until those gates are met. No full-suite claim is made.

Initial focused checkpoint: Change Ledger validator passed with 1033 records and 78 governed code files in the existing dirty diff. Scoped `git diff --check` for the five owned scripts and current status docs passed. At this checkpoint only the four exact focused cases had run.

After the complete-tick addition, the Change Ledger validator passed again (1035 records, 82 governed code files across the current dirty diff; existing unrelated warnings), and scoped `git diff --check` passed. Battle/Menu Scene, GameConfig and ProjectBattleModeConfig SHA-256 matched the prior protected baseline. The earlier four-case claim above is historical; the new three-case job is listed separately, not a full-suite run.

The same validator and scoped whitespace check passed again after the one-case poison-pulse test and status update; all four protected Scene/config SHA-256 values remained equal to that baseline. Only the selected new test ran after its edit.
