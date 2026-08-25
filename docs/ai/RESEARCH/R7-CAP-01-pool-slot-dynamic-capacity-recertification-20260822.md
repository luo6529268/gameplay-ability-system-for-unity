# R7-CAP-01 — pool / slot allocator / dynamic capacity recertification

> 日期：2026-08-22  
> 状态：`SOURCE-CONFIRMED DELIVERY-CONTRACT DIFFERENCE / NO CODE CHANGE`

## Scope

只读复核：

- C++ fixed 400-slot world、stage/dynamic slot band与最低空槽规则；
- Unity `RuntimeSlotAllocator`、`RuntimeSlotTable` generation/page增长；
- pending-destroy release/admission；
- `Authority400`、`MobileExtended`、`DesktopExtended` profile；
- pure-logic pool、opoint/task pools、GameObject presentation pool与battle allocation seal。

本包不修改 C++、Unity production/test脚本、profile、pool或seal策略。

## Authority

- C++ `include/game_world.h:13,216-263,265,350-353`：`MAX_OBJECTS=400`、固定数组、完整
  `Entity::reset()`、构造时稳定slot identity；
- C++ `frame_advance.cpp`、`game_tick.cpp`、`collision.cpp` 的spawn paths按slot 50→399顺序寻找首个
  inactive dynamic slot；stage/特殊入口另有slot20→399 band；
- C++ fixed capacity是Authority400的行为边界；Unity Mobile/Desktop扩容是用户确认的交付adapter，不能反向
  定义C++规则，也不能被C++ 400上限回退。

## Unity source mapping

### Lowest-free / generation

- `RuntimeSlotAllocator`把0–19、20–49、50+拆成segments；每段以min-heap保存已释放/跳过的低槽，
  `nextUnused`保存从未使用尾部，因此`AllocateLowest(50)`仍严格取最小可用slot；
- `RuntimeSlotTable`在claim和release时都推进generation；长期引用必须使用`RuntimeEntityHandle`，旧generation
  无法解析到复用slot的新实体；
- `ReleasePendingDestroySlots`在分配/peek前执行，只有到既有release boundary才让pending slot重新进入allocator；
- R5-LIFE-001A/01B已认证same-pass cursor、pending/free、old-generation失效和old-object finalization。

### Profile / pool

- `Authority400`：400 slots / 400 active；
- `MobileExtended`：1050 slots / 1000 active，Android默认；
- `DesktopExtended`：初始容量按256页归一化，Standalone默认512，active合同为`int.MaxValue`；
- `BattleLogicReferencePool.PrepareBattleEntityShellCapacity`为character/special/other和共享weapon family各准备
  runtime capacity，并预热opoint tasks；
- Loading prewarm按resolved initial capacity准备GameObject/presentation pool；`PoolMaxSize=200`只在pre-seal懒扩容时
  输出warning，不会阻止继续创建，因此它不是200实体硬上限；
- `BeginBattleAllocationSeal`会同时seal runtime capacity、logic/task pool、GameObject pool和presentation capacity，
  以保证正式战斗窗口0 B并在耗尽时fail closed。

## Fresh focused evidence

EditMode job `4cc1de5fb20b49609ee0824cd64c4af4`：44/44 PASS，覆盖：

- lowest-free late opoint preflight；
- Mobile 1000 saturation、pending mutation、lowest slot reuse；
- occupancy/generation/snapshot与warmed 0 B；
- every concrete logic entity family、shared weapon pool retag；
- pooled reset、held/hit/landing/task/list reuse；
- sealed capacity exhaustion/rejection counters和0 B；
- stress capacity-pressure gate。

focused suite后再次重载脚本域；Unity Console为0条error/warning。完整`BattleRuntimeSelfCheck`于
2026-08-22 22:45:05写入`PASS`。随后出现的两条rest-binding Error仍是self-check negative-control日志。

## D-CAP-001 — DesktopExtended battle-time growth is disabled by the seal

### Observed facts

1. `SimulationWorld.TryGrowDesktopRuntimeSlots`首先调用`runtimeCapacityModule.TryAuthorizeGrowth()`；
2. `SimulationTickDriver.BeginBattleAllocationSeal`在战斗开始前调用`_world.RuntimeCapacity.Seal()`；
3. sealed状态的`TryAuthorizeGrowth()`只增加`RejectedGrowthCount`并返回false；
4. Windows Standalone默认`DesktopExtended`初始容量512；`GameConfig`当前也配置
   `DesktopInitialRuntimeSlotCapacity: 512`；
5. `PooledEntityReuseAllocationEditorTests.SealedWorld_RuntimeSlotExhaustion_DoesNotAllocate`明确验证sealed
   DesktopExtended耗尽后拒绝新slot，并通过0 B与rejection counter验收。

因此当前真实合同是：DesktopExtended**只可在seal之前**增长；seal之后实际硬上限等于已经准备的logical capacity。
默认Windows battle若没有在loading/assembly阶段提前增长，就会在512 slots处fail closed。这与主计划保护条款中的
“DesktopExtended动态增长且没有production active hard cap”不一致。

### Why this is not the PoolMaxSize=200 issue

`LF2ObjectPool.Get`在pre-seal active数量超过`PoolMaxSize`时只记录warning，随后仍调用`CreateNewObject()`。
真正阻止battle-time扩容的是allocation seal与已准备容量，不是200这个配置值。

### Required decision before repair

“战斗过程中严格0 B”与“有限内存设备上的真正无上限实时增长”不能同时成为绝对保证。后续repair WP必须先固定
桌面端交付合同，至少在以下方向中选择一个并写明capacity fault行为：

1. loading阶段确定/配置本局最大容量并一次性预热；battle内0 B，实际上有本局上限；
2. battle外安全点扩页并短暂停tick；允许受控分配，不再声称整个battle窗口绝对0 B；
3. 预留很大的虚拟/分页arena并按页提交，仍受地址空间/物理内存上限约束；
4. 超过预算时做明确的deterministic admission failure，不能继续宣称“无production hard cap”。

当前只登记差异，不替用户选择架构。

## Verdict

- Authority400/MobileExtended、最低空槽、generation、pending reuse和pool reset/0 B未发现新的behavior difference；
- `D-CAP-001`是source-confirmed且已有自动测试证明的交付合同差异；
- fresh-domain compile/error gate与full self-check通过；
- C++ trace、真实Play Mode、Windows Player >512 spawn和Android 1000 real-device仍未验收；
- 本包不授权改profile、seal、allocator、pool、opoint或presentation。

## Next

R7 inventory至此覆盖计划列出的所有优化组。下一步先汇总R7全部新差异/coverage/performance项并拆分repair WPs；
在用户确认容量策略前，`D-CAP-001`只能保持待决，不能通过扩大初始常量伪装关闭。

## 2026-08-23 closure

R7-CAP-01A已选择并验证合同：Desktop没有固定产品级active cap；每局在unsealed loading/reset/preflight
边界按页准备有限容量；active battle seal后strict 0 B；超预算时确定性拒绝，不临时扩容。fresh matrix
11/11 + 33/33（合计44/44）与03:19:45同域full self-check PASS。当前production已经符合，
`D-CAP-001`按交付合同澄清关闭，R7-CAP-01B不需要实施。真实Windows Player >512归R8验收。
