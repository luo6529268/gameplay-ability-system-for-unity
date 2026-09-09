# NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-002 — type3 post-hit re-exit audit

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-002
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan battle_world.cpp type3 continuation through common effect tail; EXE B1E13AE1, closure 39DDDA15.
evidence: AUTHORITY-BATTLE-WORLD-6615-6651 / UNITY-DAMAGE-BEFORE-PAIR-DIFFERENCE / EXISTING-TYPE3-OWNERS-RECONFIRMED / NEXT-EARLY-BRANCH-PACKAGE / LEDGER-PASS / SCENE-D4266C6D-UNCHANGED / NO-CODE-NO-CONTENT-NO-SCENE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / EARLY_BRANCH_DIFFERENCE_ROUTED`

## 审计范围

复核type3 candidate、attacker post-hit、generic target、locked kind transform、matching pair、motion-hold release、
公共effect override和type0-only direct tail的完整顺序；不得以前一审计结论代替当前源码与Unity现状检查。

## 审计结果

| 段 | 正式owner | Unity owner | 结论 |
|---|---|---|---|
| kind-table candidate gate | `hit_candidates.cpp:110-129` | locked candidate gate/collection tests | 已验证保持 |
| unarmored attacker post-hit | `battle_world.cpp:6827-6832` | `ApplyNativeType3AttackerPostHitAction` | 已验证保持 |
| target generic/locked transform | `battle_world.cpp:6854-6945` | `ApplyKind0Type3Tail`及两条helper | 已验证保持 |
| late pair/hold | `battle_world.cpp:6948-6963,7270-7292,7362-7398` | pair reset/hold helpers | 本轮实现后已对齐 |
| common effect/type0 direct tail | `battle_world.cpp:6964-6997` | effect override；type3无legacy direct tail | 已验证保持 |
| initial matching 3005/3006 early branch | `battle_world.cpp:6615-6651` | 当前无前置early-return | `CONFIRMED_DIFFERENCE` |

正式early branch在damage/resource/status/audio/hit-record之前只写standard rests、双方latch-hit_Uj pair reset与
hold release并立即返回。Unity当前先执行`RecordDamageEffectSound`、vital/status/join/object-hurt，再在尾部reset，
会错误改变HP、HPBound、统计、HitCount/Fall/HitState、声音和hit record。

下一`NTSD28-B5-TYPE3-MATCHED-PAIR-EARLY-BRANCH-001`只负责将该gate前置到actual/HitPlan并以test-first
证明无damage/audio/status/record副作用；本审计无C#/content/Scene改动。
