# R8-HITPLAY-001 — live collision / hit / damage / abort S4 probe

<!-- CHANGE-RECORD
id: R8-HITPLAY-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1647-1824;src\entity\collision_collect.cpp:250-373;src\entity\collision.cpp;src\entity\hit.cpp;Makefile
evidence: R4-COL-001..003/005A and R4-HIT-001..004 have source/compile/self-check evidence but no joint production Play S4
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / editor / live production collision-hit certification

## 1. 修改前状态

- C++ collect→type0 consume→random-weapon boundary→type>0 consume source 合同已只读闭合；
- Unity R4 collector/sequence/damage writer 修复已分别通过 compile/focused/self-check；
- 除 WP01C-02 的 landing no-immediate-hit 外，缺同一 live production world 中 character/weapon/special、
  ordering、gate/abort、vital/stat/durability/vrest 的联合 S4；
- 当前未发现新的 source-confirmed production 差异。

## 2. 允许改动

- 仅新增 `Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs` 及 meta；
- 仅更新本包治理/证据文档；
- 禁止修改 production gameplay、scheduler、DAT/scene、render/URP、C++ authority。

## 3. 预期副作用与保护

- probe 只在 driver paused 且 worker idle 时注册自有 logic fixtures；
- global KillStats/DamageStats、runtime rests、candidate carrier 与 pause 先保存后恢复；
- 所有 probe entities 必须注销，pending destroy flush，world/pool/claimed 恢复；
- fixture 只编码 C++ source 合同，不依赖具体角色、技能或 OID 特判；
- 若发现 first-difference，本 Record 不扩权修改 production，必须另建 repair Change ID。

## 4. 验收

- Task required matrix 全部 PASS；
- fresh compile、focused suites、clean Play、full self-check、validator；
- 报告明确 collect/character consume/object consume 的逐边界状态；
- first-difference 出现时本 Record 保持 `BLOCKED` 或 `RUNTIME_PENDING`，不伪报完成。

## 5. 回滚

- 删除新增 probe/meta，并将本 Record 标记 `ROLLED_BACK`；
- 未提交，不触碰其他用户修改。

## 6. 实际改动（待编译）

- 新增 Editor-only 显式 Play probe 及 meta；
- 在 paused/worker-idle live world 注册 21 个通用 source-derived fixture，按隔离坐标构造 10 个候选；
- production pass 顺序为 collision snapshot→pair-vrest→collect→character consume→random-weapon no-op
  boundary→object consume→candidate end；
- matrix 覆盖 character/weapon/special 正向伤害、HitConfirm2 attacker abort、caught/hurtable
  current-candidate skip、effect21 state18 attacker abort、kind10 raw frame182；
- cleanup 备份/恢复 RNG、global stats、pending sounds、baseline pair vrest、hit-plan mode、probe entities、
  pools/claimed slots 与 driver pause；baseline有活动candidate或实体数超过64时 fail closed；
- production gameplay、scheduler、DAT/scene、render与C++均未修改。

当前状态 `CODE_WRITTEN`；尚未取得 Unity compile、focused、Play 或 self-check 证据。

首次 clean Play 在任何 gameplay pass 前 fail closed：live world 已越过 reset boundary，production 正确拒绝
把 hit-plan mode 临时切换成 `ShadowCompare`。cleanup 的实体/池/stats/RNG/sounds/rests 均恢复，但探针又
尝试恢复相同mode而记录 cleanup error，因此该次报告为 `FAIL`。修正仅限 Editor probe：不再切换mode，
当前启动配置若本来是 ShadowCompare 才校验 shadow diagnostics；其余模式以真实字段矩阵验收并明确标记
`hitPlanComparisonAvailable=false`。production 仍为 0 改动，必须重新编译和 clean Play。

第二次 clean Play已执行到三类positive writer之后，behavior断言此前均通过；探针随后读取不存在的
`DamageStats[3]/KillStats[3]`而越界。live数组仅有合法槽0～2，cleanup再次全部恢复。只将special fixture
改为共用合法槽1：character pass验证`DamageStats[1]+10/KillStats[1]+1`，object pass验证累计
`DamageStats[1]+20`且kill仍仅一次。production仍0改动，需第三次compile/clean Play。

## 7. Final verification

| 层级 | 结果 | 状态 |
|---|---|---|
| compile | Editor DLL 13:18:02；C# error 0 | `PASS` |
| hit focused | job `3c6bc7b30d5b4ef5843d3156ecc99d1a` 178/178 | `PASS` |
| W06 | job `dc692c9dbf0146f7aba998c7933d89b4` 11/11 | `PASS` |
| role-aware | job `8ad0b8bc603c48d3b657738faea46821` 9/9 | `PASS` |
| final clean Play | 10 candidates、三positive、三gate/abort、raw frame、pass boundary | `PASS` |
| cleanup | objects4→4、claimed2→2、pools2→2、RNG/stats/sounds/rests/mode恢复 | `PASS` |
| Play Console | error 0 | `PASS` |
| full self-check | 2026-08-23 13:19:39 | `PASS` |
| ledger validator | 69 records / 68 governed code files | `PASS` |
| production changes | 无 | `NOT APPLICABLE` |

final报告：`Temp/NTSD_R8_WP01C_04_CollisionHitDamage.result.json`；persistent evidence：
`docs/ai/RESEARCH/R8-WP01C-04-collision-hit-damage-runtime-evidence-20260823.md`。

本Record状态为`VERIFIED`，只裁决WP01C-04 Unity production Play S4。启动配置的hit-plan mode是Disabled，
worker inactive，C++ full trace继续BLOCKED；不得扩大为完整战斗对齐。
