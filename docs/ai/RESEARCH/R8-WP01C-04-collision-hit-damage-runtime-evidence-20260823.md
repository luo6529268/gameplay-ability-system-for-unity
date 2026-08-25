# R8-WP01C-04 — collision / hit / damage / abort runtime evidence

> 日期：2026-08-23  
> Change ID：`R8-HITPLAY-001`  
> 结论：`UNITY PRODUCTION PLAY S4 PASS`；不等于 C++ full trace 或完整战斗对齐

## 1. Authority contract

只读复核的 C++ Release live source 固定了以下顺序：

1. `game_tick.cpp` 保存 `prev_frame2`；
2. `collision_collect_candidates` 按 slot pair 和双向 attacker 顺序冻结 candidate；
3. type0 attacker consume；
4. random weapon drop；
5. type>0 attacker consume；
6. candidate 消费之后才进入 CPoint/weapon sync。

`collision.cpp` 的 candidate 内部顺序为 vrest gate→`hit_confirm2` character-target attacker abort→
caught/hurtable current-candidate skip→runtime ITR conversion→effect21 state18/19 attacker abort→writer/tail。
本次没有运行、构建、修改、复制或向 C++ authority 写入。

## 2. Unity live probe

新增 `BattleCollisionHitDamagePlayModeProbeEditor.cs`，在真实 `NTSD_Battle` production world 已暂停且
worker idle的边界注册 21 个通用 source-derived fixture。fixture 分散到隔离坐标，不依赖生产角色、技能或
OID 特判；正式 collector 冻结 10 个 candidate：

- character positive：1；
- `HitConfirm2` 两目标 abort：2；
- caught/hurtable first-skip/second-continue：2；
- effect21 current state18 attacker abort：2；
- kind10 raw-frame：1；
- weapon positive：1；
- special positive：1。

真实调用顺序是：

`CaptureCollisionFrameSnapshotsAll → TickCollisionPairVRestAll → CollectCollisionCandidatesAll →`
`PostInteractionTickAll → RandomWeaponDropTickAll → ObjectInteractionTickAll →`
`EndCollisionCandidateConsumption`。

## 3. Final Play result

报告：`Temp/NTSD_R8_WP01C_04_CollisionHitDamage.result.json`，13:18:44 `PASS`。

| witness | 结果 |
|---|---|
| frozen candidates | 10，全部为预期 slot order |
| character | HP `5→-5`，HPBound `100→97`，combo 10，vrest 3，lethal frame186；holder kill 1/combo10，world kill+1/damage+10 |
| HitConfirm2 | first/second HP均100，整 attacker 在writer前abort |
| caught/hurtable | first HP100/vrest0，second HP90/vrest>0，只跳过当前candidate |
| effect21 state18 | first/second HP均100，整 attacker 在writer前abort |
| kind10 raw frame | frame/runtime frame182；PN41、attacking9、wait73保持 |
| random weapon boundary | 四个active weapon fixture使该pass严格no-op；object count与RNG均不变 |
| weapon | HP `100→80`，HPBound `100→94`，combo20，durability `100→90`，vrest3，HitConfirm2=1 |
| special | HP `100→90`，HPBound `100→97`，combo10，vrest3，HitConfirm2=1；无新增type0-only kill |
| pass separation | character pass后object HP仍100/100；object pass后才变80/90 |

本轮产生6个pending sound和9次RNG draw；cleanup后均恢复基线。objects `4→4`、claimed `2→2`、
object pool `2→2`、logic pool `2→2`，global stats、RNG state/call count、pending sounds、baseline pair vrest、
hit-plan mode和pause全部恢复，cleanup error为空。final Play Console 0 error。

启动配置中 `BattleHitExecutionPlanMode=Disabled`、dedicated worker inactive，因此本轮是 production pass/field
S4，而不是 ShadowCompare 或 worker-active 证据。报告明确写入
`hitPlanComparisonAvailable=false`；不得把 `hitPlanValid=true`解释为本轮做过legacy/optimized shadow比较。

## 4. Superseded probe-only failures

1. 首次 Play 在pass前尝试把live hit-plan mode切为ShadowCompare，production正确拒绝非reset-boundary
   模式切换；probe cleanup的重复恢复动作产生cleanup error。只删除probe的mode切换，production 0改动。
2. 第二次 Play已执行完整passes及此前所有behavior断言，最后因probe读取不存在的stats索引3而越界；
   live数组只有0～2。改为special与character共用合法槽1，并按pass验证`damage +10→+20`、kill只+1。

两次失败均保存为历史事实，不能作为behavior PASS；两次均没有 gameplay first-difference，第二次cleanup
已完全恢复。

## 5. Fresh verification

| 层级 | 结果 |
|---|---|
| compile | probe导入；Editor DLL 13:18:02，C# error 0 |
| hit focused | job `3c6bc7b30d5b4ef5843d3156ecc99d1a`，178/178 PASS |
| W06 order/hit | job `dc692c9dbf0146f7aba998c7933d89b4`，11/11 PASS |
| role-aware collision | job `8ad0b8bc603c48d3b657738faea46821`，9/9 PASS |
| final clean Play | 10-candidate matrix、pass separation、cleanup、Console 0 error PASS |
| full self-check | `Temp/NTSD_BattleRuntimeSelfCheck.result`，13:19:39 PASS |
| ledger | 69 records / 68 governed code files PASS |
| diff check | scoped `git diff --check` PASS |
| production changes | 无 |

self-check 后 Console 的两条 rest bind/release error 是既有负向自检故意触发的 fail-closed 日志，不属于 final
Play；self-check result仍为PASS。

## 6. Evidence boundary

- 本证据只将 `D-COL-001～003`、`D-COL-005A`、`D-HIT-001～004` 提升为“既有逻辑+
  WP01C-04 Unity S4 PASS”；
- `D-COL-005B` 的正式 DAT non-character kind1 reachability仍为UNKNOWN；
- `D-HIT-005` CLR shell/current-DAT reachability仍为UNKNOWN；
- R1-WP02 C++ full trace继续BLOCKED；
- worker-active、death/respawn、random weapon实际生成、late effect、T8和Android均未由本包关闭。
