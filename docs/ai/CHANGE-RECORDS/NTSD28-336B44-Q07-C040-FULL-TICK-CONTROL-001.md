<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C040-FULL-TICK-CONTROL-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/c040_held_pose_full_tick_control.cpp
authority: 336B44 playable GameSession full tick catch settlement, formal Hinata and Bee DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C040-FULL-TICK-CONTROL-001.md
-->

# NTSD28-336B44-Q07-C040-FULL-TICK-CONTROL-001

Created before the diagnostic script. The existing standalone settlement pass and original Editor focused writer tests are already scoped PASS, but natural full-tick probes showed the hold expired before settlement. This package adds a separate full `GameSession28::step` controlled diagnostic with explicit reciprocal relation and positive/zero hold, preserving the old outputs. Expected side effects are only a new source diagnostic and raw evidence. It must not modify battle production, DAT, resources, Scene or non-battle behavior. Exact acceptance, risk and forward-correction rollback are in the Task.

Actual code: only the declared new `c040_held_pose_full_tick_control.cpp`. It initializes two formal-content combatants and reciprocal relation after `GameSession28::initialize`, performs one neutral-input full `step()` for each of four action/hold cases, and writes before/after action, hold, XYZ, relation, settlement and center/CPOINT discriminator columns. Current playable-closure `g++` compile exit0, no diagnostics; two independent runs exit0 and four-row CSV SHA-256 both `4432FE6A054BE571772329E250171BDFDDBDA6491F819DF5E7268461F03F831B`. Hold5 reaches settlement with hold4 and retains action132/137; hold0 selects action130. Bee137 source Y-7 versus Y1. Exact data and limits are in the [report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C040-FULL-TICK-CONTROL-001/REPORT.md). No Unity/runtime production edit, deletion or data change. Original Scene, root internal-state equivalence, natural key and pixels are unverified; C040/Q07 remain open.

Governance: `Tools/Validate-ChangeLedger.ps1` exited0/PASSED for the current dirty worktree ([output](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C040-FULL-TICK-CONTROL-001/change-ledger-validation.txt)). The first `git diff --check` exited2 only because the newly inserted master status had an extra blank EOF line; that line was moved to the top and the blank removed. The second `git diff --check` exited0 with only Git line-ending notices ([output](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C040-FULL-TICK-CONTROL-001/git-diff-check-final.txt)). No full Unity test suite was run for this source-only controlled case.
