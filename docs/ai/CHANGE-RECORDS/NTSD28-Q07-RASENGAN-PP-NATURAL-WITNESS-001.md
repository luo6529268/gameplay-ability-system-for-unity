<!-- CHANGE-RECORD
id: NTSD28-Q07-RASENGAN-PP-NATURAL-WITNESS-001
status: VERIFIED
change-kind: Q07_NARUTO_NATURAL_PP_DIAGNOSTIC_FIELD_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UserRasenganPhysicalPlayProbeEditor.cs
authority: formal root NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable frame/input resource chain
evidence: artifacts/diagnostics/NTSD28-Q07-RASENGAN-PP-NATURAL-WITNESS-001/ACCEPTANCE-20260925.md; original Editor focused PP Play PASS
-->

# NTSD28-Q07-RASENGAN-PP-NATURAL-WITNESS-001

Before edit: nested `NaturalProbe` uses `actor.Runtime.MP` for its initial cost gate, optional raise, and row `mp`. `BattleCharacterActionWriter` writes `character.Health.PP`, which binds `NTSDEntityRuntime.PP`; the old probe therefore cannot establish formal `current_mp` parity. Current formal source/release LFR shows MP500→350→250 for natural Naruto Rasengan→Rasenshuriken under the stated input. The exact script-only correction and single-case validation are in the same-ID Task. No DAT value, Unity production, Scene, Prefab, ProjectSettings or nonbattle edit is authorized. On completion, record actual symbols/results and distinguish synthetic device input from formal EXE human/pixel proof. Rollback concerns only this scoped diagnostic diff under repo approval rules.

After edit: only nested `NaturalProbe` in the declared Editor diagnostic script changed. It now requires initial `actor.Health.PP >= 200`, does not raise the unrelated `Runtime.MP`, records `Health.PP` as `mp`, and explicitly records `Runtime.MP`, `InputMpConsumedTotal350` and `InputLocalResourceEnabled49D034`; old reports remain untouched. Original Editor imported the script with 0 Console errors; one focused first253 Play PASS. At natural skill entry tick780 action240 PP500/consumed0, tick781 action239 PP350/consumed150, first253 tick807 PP350, conversion tick808 action301 PP250/consumed250. The root formal EXE controlled LFR matches action/MP/consumed at the corresponding relative events (ticks6/7/33/34). `Runtime.MP` remains500 and is not the skill-resource comparator. Editor exited Play; Battle/Menu Scene and GameConfig hashes unchanged. Details, JSON SHA and scope limits in `artifacts/diagnostics/NTSD28-Q07-RASENGAN-PP-NATURAL-WITNESS-001/ACCEPTANCE-20260925.md`. Record `VERIFIED` closes this focused diagnostic only; Q07/R07/R18 and full native physical/pixel proof remain open. Rollback is the exact diagnostic script edit only under repository approval rules.
