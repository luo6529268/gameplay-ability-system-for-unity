# Q07/C040 formal playable full-tick held-pose control

Status: `VERIFIED_SCOPED_SOURCE_FULL_TICK`; C040, Q07 and the overall goal remain open. Authority is the selected formal root `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` and its corresponding playable source. This diagnostic was compiled from that playable source closure and reads the formal `resources/runtime` DAT; its temporary EXE is not the formal root EXE.

Four separate `GameSession28` instances used mode0, seed682973786, Hinata OID41/action125 at source X500/Y0/Z400 and Bee OID75/action132 or137 at X550/Y0/Z400. Both had HP/MP500. Before the full tick, the diagnostic set reciprocal catch slots 0↔1 and Bee hold5 or hold0. Both input packets were neutral. It then called **one complete `GameSession28::step()`**, including the earlier physics/frame passes, and read `last_tick().catch_settlement` and the final World entities.

| Bee initial action / hold | Bee action / hold after tick | Bee source XYZ after tick | Settlement active / synchronized | Split reached |
| --- | --- | --- | --- | --- |
| 132 / 5 | 132 / 4 | (529, 1, 399) | 1 / 1 | yes: current CPOINT X58 vs vaction X41 |
| 132 / 0 | 130 / 0 | (529, 1, 399) | 1 / 1 | no: action changed to vaction |
| 137 / 5 | 137 / 4 | (529, -7, 399) | 1 / 1 | yes: current center Y71 vs vaction Y79, current CPOINT X33 vs vaction X41 |
| 137 / 0 | 130 / 0 | (529, 1, 399) | 1 / 1 | no: action changed to vaction |

The new diagnostic [source](../../../Tools/NTSD28Q07Diagnostics/c040_held_pose_full_tick_control.cpp) compiled against the current 28 Core + playable source set with `g++` exit0 and no output diagnostics. The exact argument vector is in `compile-argv.txt`. Independent [run 01](run-01/full-tick.csv) and [run 02](run-02/full-tick.csv) both exited0; their CSV SHA-256 is identical: `4432FE6A054BE571772329E250171BDFDDBDA6491F819DF5E7268461F03F831B`. This extends the earlier standalone settlement-pass certificate: positive hold survives the earlier passes and the frame center/CPOINT split reaches settlement under this **controlled initial relation**.

The initial reciprocal relation and hold were explicitly set after session initialization. This is not a natural physical-key selection, and the formal root LFR input does not encode these internal fields. No Unity original Battle Scene full Driver or visible pixel was tested by this package. The next comparable step is a separate original Scene test with the same initialized relation, hold, action and formal DAT; this result must not be relabeled as root-EXE or final C040 parity. No production code, DAT value, Scene, Prefab, image, audio or non-battle file changed.

Change Ledger validation [passed](change-ledger-validation-final.txt) and covered the new diagnostic. Final `git diff --check` [exited 0](git-diff-check-final2.txt). No full Unity test suite was run for this source-only controlled case.
