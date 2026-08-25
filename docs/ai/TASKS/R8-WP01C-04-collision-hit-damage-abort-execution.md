# R8-WP01C-04 — collision candidate / hit / damage / abort execution

> 日期：2026-08-23  
> 状态：`VERIFIED / UNITY PRODUCTION PLAY S4`  
> Change ID：`R8-HITPLAY-001`

## Goal

在真实 `NTSD_Battle` Play world 和 production pass 中，联合认证 C++ Release 已静态闭合的
`candidate collect → character consume → random-weapon boundary → object consume` 顺序，以及角色、
武器、特殊攻击命中后的伤害、统计、durability、vrest、`HitConfirm2` 和 attacker abort 语义。

## Scope

- 新增一个 Editor-only 显式 Play probe；
- 在 production world 的 paused/worker-idle 边界注册通用 source-derived fixtures；
- 使用真实 `CaptureCollisionFrameSnapshotsAll`、`CollectCollisionCandidatesAll`、
  `PostInteractionTickAll`、`ObjectInteractionTickAll` 与 `EndCollisionCandidateConsumption`；
- 覆盖 character、weapon、special attacker 各一条正向命中；
- 覆盖 multi-candidate order、`HitConfirm2` 整 attacker abort、caught/hurtable gate、
  effect21 + state18/19 abort；
- 记录 raw frame response、HP/HPBound、kill/combo/damage stats、weapon durability 与 vrest；
- 每个场景后清理 probe entity/candidate/rest/global stats 并恢复 driver pause 和池计数；
- 若发现 first-difference，只登记独立 production repair，不在认证 probe 中修改 gameplay。

## Authority / Evidence

- C++ Release 只读 source：
  - `src/entity/game_tick.cpp:1647-1824`：snapshot/collect → type0 consume → random weapon → type>0 consume；
  - `src/entity/collision_collect.cpp:250-373`：candidate geometry、kind/type/effect gate、pair/vrest 与顺序；
  - `src/entity/collision.cpp:29-225` 及各 type tail：vrest → `HitConfirm2` abort → caught gate →
    runtime itr conversion → effect21 abort → kind dispatch/field mutation；
  - `src/entity/hit.cpp`：character/weapon/special damage 与 reaction；
  - release `Makefile` 纳入上述 translation units；
- Unity 既有 R4 修复：`R4-COL-001～003/005A`、`R4-HIT-001～004` 已有 source、compile、
  focused/self-check 证据，但除 `D-COL-004B` 外尚缺同一 live production world 的联合 S4；
- `D-COL-004B` 已由 WP01C-02 取得 landing no-immediate-hit S4，本包只做回归引用，不重复裁决。

## Required matrix

1. **character positive hit**：候选由 production collector 生成并由 character consume；验证 HP/HPBound、
   combo/damage/kill ownership、raw response、vrest 与 hit-confirm 字段。
2. **weapon positive hit**：由 object consume，验证 scaled vital/stat、raw durability、type tail 首写和 vrest。
3. **special positive hit**：由 object consume，验证 type3 vital/stat 在 type3 tail 前生效且无 type0-only score。
4. **multi-candidate / `HitConfirm2` abort**：slot/candidate 顺序固定；首候选触发确认后，后续 character
   candidate 不得被消费。
5. **caught/hurtable negative witness**：合法 caught relation 且 catcher `hurtable=0` 时只跳过当前 candidate，
   不扩大为 attacker abort。
6. **effect21 state18/19 negative witness**：runtime kind0/effect21 遇当前 state18/19 必须终止整个 attacker。
7. **pass/order report**：至少输出 collect、character consume、object consume 三个边界的 candidate/字段表。

## Deliverables

1. `Temp/NTSD_R8_WP01C_04_CollisionHitDamage.result.json`；
2. 三类 attacker 正向行及三组 gate/abort 负向行；
3. candidate order→consume→field mutation 的结构化报告；
4. cleanup/pause/rest/global-stat 恢复证据；
5. fresh compile、focused tests、full self-check 与 ledger validator；
6. persistent runtime evidence 和 D-ID 状态更新。

## Verification

1. fresh Unity compile 0 error；
2. R4 collision/hit focused Editor suites PASS；
3. explicit clean Play probe required matrix PASS，Console 无非预期 error；
4. world object/claimed、object/reference pool、runtime rest、global stats 和 pause 恢复；
5. full `BattleRuntimeSelfCheck` 与 `Tools/Validate-ChangeLedger.ps1` PASS。

## Stop conditions

- candidate order、RNG、abort、vrest 或字段首写时点出现 production first-difference；
- 需要修改 production collector、resolver、damage writer、scheduler、DAT/scene 或已批准 adapter；
- 需要角色、技能或 OID 专项分支；
- kind1 non-character 正式 DAT 可达性成为必需前置但仍为 UNKNOWN；
- 需要运行、构建、修改、复制或写入 C++ authority。

命中后保存最短 witness、创建独立 repair Change Record 并停止相应场景；不得在 probe 中修行为。

## Out of scope

- death/respawn（WP01C-05）、random weapon/late effect（06）、synthesis（07）；
- D-HIT-005 CLR shell/current-DAT reachability；
- render 图片/SceneView（WP01D）、1000 实体（E）、Player（F）；
- C++ full trace、T8、Android、服务器。

## Authorization

用户于 2026-08-23 明确回复：`批准执行 R8-WP01C-04，恢复目标`。

## Result

- fresh compile 0 error；
- hit focused 178/178、W06 11/11、role-aware 9/9；
- final clean Play在当前production配置下冻结10个candidate，required matrix全部PASS；
- objects/claimed/pools、RNG、stats、sounds、baseline rests、mode和pause全部恢复，Play Console 0 error；
- 13:19:39 full self-check和69/68 ledger validator PASS；
- persistent evidence：`RESEARCH/R8-WP01C-04-collision-hit-damage-runtime-evidence-20260823.md`；
- production gameplay/scheduler、DAT/scene、render与C++均0改动。

本Task只关闭04的Unity S4；当前hit-plan mode为Disabled，未取得本轮ShadowCompare/worker-active/C++ full
trace证据。WP01C-05～07继续独立。
