<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C017-DEAD-AI-SELFCHECK-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: selected formal 336B44 playable InputRouter28::step_sampled no-global-HP rule, C017 current scoped test evidence
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C017-DEAD-AI-SELFCHECK-001.md
-->

# NTSD28-336B44-Q07-C017-DEAD-AI-SELFCHECK-001

Created before editing scripts. The current full self-check is RED at the old DataOrientedCanonical HP=0 AI input expectation. Formal 336B44 `InputRouter28::step_sampled` does not globally suppress zero-HP sampled input. The fixture seeds previous jump/attack=0 and current sampled jump/attack=1, and its Legacy branch already expects previous=1. Current DataOriented observed previous=1, current=0, attack window=0, frame=0. Only the stale previous-key expectation may change; no production behavior or other checks are in scope.

Validation and rollback are specified in the Task. The new full-run result must be reported exactly; if another assertion fails, full self-check remains FAIL. Root EXE same-state and natural Play are not proven by this correction.

2026-09-30: changed only the previous-key expectation from profile-dependent 0/1 to 1 and clarified its message. Original Editor rebuilt `Assembly-CSharp.dll`, then a first exact-test request used an incorrect `.Editor` namespace and selected 0 tests; this is not counted as PASS. The corrected exact job `06b62661a68d438ba138d5f0fd0f93d2` passed 4/4 C017 zero-HP controls including the production poison-to-zero complete-tick case. A fresh full self-check passed the old AI assertion and failed later in `CheckStateTransformLandingMatrix` at the F02 high-Vx type4 landing expectation. The entire self-check remains FAIL. Raw evidence is in the report; no production path was changed.
