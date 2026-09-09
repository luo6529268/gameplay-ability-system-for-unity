# NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-003 — type3 final specific-family exit audit

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-003
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan hit_candidates.cpp 110-129 and battle_world.cpp 6615-6651, 6827-6997, 7270-7292, 7362-7398; EXE B1E13AE1, closure 39DDDA15.
evidence: TYPE3-SPECIFIC-MATRIX-CLOSED / COMMON-STANDARD-REST-DEFERRED-B5-F11 / AUDIO-SPARK-DEFERRED-B10 / NEXT-STANDARD-REST-AUDIT / LEDGER-PASS / SCENE-D4266C6D-UNCHANGED / NO-CODE-NO-CONTENT-NO-SCENE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / TYPE3_SPECIFIC_FAMILY_EXIT_READY`

## 退出矩阵

| 段 | Authority | Unity证据 | 结论 |
|---|---|---|---|
| kind-table candidate | `hit_candidates.cpp:110-129` | locked gate与catalog transform tests | specific aligned |
| initial matching pair early-return | `battle_world.cpp:6615-6651` | early helper + HitPlan + focused4 | specific aligned |
| attacker post-hit | `battle_world.cpp:6827-6832` | shared action resolver actual/HitPlan | specific aligned |
| generic target/locked transform | `battle_world.cpp:6854-6945` | generic/transform packages | specific aligned |
| late pair/hold | `battle_world.cpp:6948-6963,7270-7292,7362-7398` | latch reset/hold package | specific aligned |
| common effect handoff | `battle_world.cpp:6964-6997` | effect8..16；type3无legacy direct tail | specific aligned |

## 保留边界

Type3路径仍会继承尚未关闭的共同owner：`apply_standard_hit_rest`的recover/definition-effect/timing-reduction/byte
数值属于B5/F11；builtin/definition audio与spark属于B10；relation/cpoint属于B6；正式内容差异属于H。这些不是
Type3-specific family继续滞留的理由，但在共同owner关闭前不得宣称具体Type3战斗场景已整体完全一致。

下一`NTSD28-B5-STANDARD-HIT-REST-RECOVER-AUDIT-001`；本审计无C#/content/Scene改动。
