# Task Contract — NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / FREE_NOT_DESTROY_OWNER / POST_REFILL_PRE_POSE_GATE / CURRENT_33_WITNESS / CORRECTED_RELATION_DOMAIN / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001 / VERIFIED`

## 目标

冻结 Authority held/refill pass 中 `WPoint.weaponact >= 1000` 的精确控制流、Unity structural
despawn owner、无副作用边界、同 tick slot reuse 与 trace/test 责任；在 relation lifecycle cleanup
取得运行时证据之前不修改生产脚本。

## Authority 闭环

- 正式 `source/ntsd28_playable/scripts/build.ps1 -Target playable` 的 source list 明确包含
  `ntsd28_core/src/simulation/battle_world.cpp`；该文件属于当前 82-file playable closure
  `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- `BattleWorld28::settle_held_refill_objects()`按物理 child slot 升序扫描。对 reciprocal negative
  relation，先执行 state17 OID122/123 refill；若当次耗尽，则双方 unlink、双方 action/counter 清零、
  child 获得一次同步 RNG X kick 后 `continue`，不会进入 terminal。
- 未耗尽/非 refill 才读取 holder 当前 frame 的 primary WPoint。`weapon_action >= 1000` 时先递增
  `terminal_weapon_actions`，随后调用 `despawn(child_slot)` 并立即 `continue`。
- terminal 分支位于 child action、facing、hold timer、anchor/pose、DVX、kind3 与其 RNG 之前。因此它：
  不读取 child target frame，不写 child action/pose/motion/weapon HP，不消费 RNG，也不触发后续 release。
- `BattleWorld28::despawn()`先 `clear_entity_links(slot)`，再 reset slot，最后清 rest relation column。
  没有 destroy event、weapon break effect 或音频；被释放的最低 physical slot 可被同 tick 后续 producer复用。

## Current Direction-B reachability

强制使用冻结 normalized projection 与 `data.txt` indexed subset：

- ITR kind2 pickup子域的39个 holder definitions中有28条，分布于22个 holder definition；
- OPoint kind2 direct-link把Kyubi与Deidara Bird纳入完整held source union；Kyubi actions0/1/5/6/115
  新增5条，故完整current为33条、23个source definitions；
- 33条 `weaponact` 全部精确等于1000，并与all-indexed terminal计数相等；
- formal current witnesses包括 Deidara274、Pain274、Rock Lee254、Tayuya279、Sasuke274 等。

这证明 current content 已有 formal holder/terminal 组合，不再是 dormant rule；真实玩家输入到达与表现销毁
仍须由 focused Play witness证明，不能从语料计数直接宣称运行时已对齐。

## Unity 当前差异

- `SimulationQueryAndLinkModule.HeldObjectProcessAll()`在 reciprocal preflight 后把 holder WPoint直接交给
  `BattleHeldObjectWriter.RunStep12()`；没有 terminal structural gate。
- production `LF2WeaponHeldStateResolver.Act()`正确先执行 refill/exhaustion，但随后把1000当普通 child
  frame写入并继续 facing/pose；generic writer也会先写 frame/pose。frame1000不存在时还会沿 null/default
  child frame继续产生额外状态变化。
- `BattleStructuralWriter.Destroy()`会进入 `DestroyEntityLikeExeCoreForStructuralWriter()`；真实
  `LF2WeaponBase.Destroy()`调用 `RunDiePhase()`并播放 `WeaponBrokenSound`。Authority terminal despawn没有这项
  observable side effect，所以 `Destroy`不是可接受 owner。
- `BattleStructuralWriter.Free()`使用 `CurrentEntityImmediate` structural command，归还 renderer/logic pool并经
  registry立即失效 generation；它不调用 `DestroyEvent()`/`Destroy()`，是现有最窄等价 despawn seam。
- 现有 registry release 尚未清其他 active entity 的 held/catch引用；直接接 terminal 会遗留 stale holder并打开
  same-slot ABA。因此 terminal production硬依赖
  `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001` 的 Unity runtime绿灯。

## Production owner 与顺序

后续单一包：`NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-PRODUCTION-001`。

1. `WeaponActResult`增加非持久化、非snapshot/checksum的 `TerminalDespawnRequested` outcome bit。
2. real weapon path在 `ProcessDrinkConsumption()`及其 `ForceDrop` early return之后、
   `DirectWriteHeldFramePreserveWaitCounter()`之前检查 `wpoint.WeaponAct >= 1000`，只置 outcome并返回。
3. generic `BattleHeldObjectWriter.RunStep12()`在任何 frame/pose write之前置同一 outcome；weapon path收到该
   outcome后不得继续 `Thrown`/kind3 tail。
4. `SimulationQueryAndLinkModule.HeldObjectProcessAll()`是唯一 structural consumer：收到 outcome后调用
   `world.StructuralWriter.Free(held)`，刷新仍存活 holder的必要快照并立即继续下一 physical slot；禁止对已回池
   `held`再调用 `RefreshRuntimeSnapshot()`。
5. C09/C20两个 production call site在有 structural trace sink时分别使用稳定 pass label，保证同 tick两次
   held pass的 first-difference 可区分；不得新增 RNG、allocation或 Unity singleton。
6. dead `LF2WeaponPointFactory/LF2WeaponPointModule`没有 production caller，不得成为第二 structural owner；
   source guard持续锁定其不可达性，后续若重新接线必须另审 terminal outcome消费。

## 验收矩阵

- 顺序：普通终止、OID122/123未耗尽后终止、OID122/123当次耗尽优先且不终止；
- side effects：child action/frame wait/facing/pose/motion/weapon HP与RNG cursor在terminal前后保持，破碎音效与
  destroy count不增加，free/unregister/generation release精确各一次；
- lifecycle：slot0、extended high slot、renderer-backed同步主线程与logic-only worker eligible world；
- relation：holder/child、第三方 held/catch、encoded `0x2000+slot` source全部由前置cleanup原子清除，
  HolderCopy/Kind4 count与无关 owner/spawner保持；
- scan/reuse：同一pass下一child仍处理；当前child slot立即不可解析；同 tick后续producer复用最低slot且获得新
  generation，old pending-unregister finalization不移除newborn；
- trace：C09/C20 pass label、cursor/source slot、free及generation release顺序可区分；current corpus guard保持
  full-held-union=33、all-indexed=33、23个source definitions且值均1000；
- 验证：Unity compile、focused、B6 regression、W05 structural、SelfCheck、targeted Play与同seed/input/tick
  first-difference。缺任一运行证据时最多 `RUNTIME_PENDING`。

## 不变量 / 阻塞

- 不改 Direction-B Config、Scene、Prefab、resource/importer、Authority、capacity、pass placement、refill数值、
  kind3/DVX/cover2/damaged-drop、catch算法或 shutdown顶层顺序。
- 不把 structural `Destroy`误当 native `despawn`，不以 frame1000 transition代替实体移除。
- 前置 lifecycle cleanup尚未实施且当前 Unity Test Runner栈受许可/已有 Editor占用阻塞；本轮只完成owner审计。

## Relation-domain correction（2026-09-08）

本Task最初的28/22是正确的ITR-pickup子集，但不是完整held域；
`NTSD28-B6-HELD-RELATION-PRODUCER-DOMAIN-CORRECTION-AUDIT-001`补入OPoint kind2后，production
matrix以33/23且全1000为准。Free-not-Destroy owner、post-refill/pre-pose顺序与lifecycle依赖不变。

## 回滚

仅移除本治理记录；没有脚本、content、Scene或Authority回滚。
