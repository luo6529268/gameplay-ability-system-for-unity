# R7-CAP-01A — Desktop capacity / strict 0 B / admission contract

> 日期：2026-08-23
> 状态：`DECISION COMPLETE / CURRENT CODE CONFORMS / R7-CAP-01B NOT REQUIRED`

## Goal

把“Desktop无固定生产上限”和“active battle tick严格0 B”固定为可同时实施的合同，裁决D-CAP-001是否
需要01B production代码，避免通过解除seal或增大常量伪装解决。

## Contract under evaluation

1. `DesktopExtended.MaxActiveRuntimeEntities`保持`int.MaxValue`，没有产品级固定active cap；
2. 每局仍必须有有限的prepared capacity；它来自config/CLI/预战预估并按256-slot page归一化；
3. Desktop只在unsealed loading/reset/preflight边界动态扩页，可扩到资源/整数允许范围；默认512只是hint；
4. `BeginBattleAllocationSeal`后runtime slots、logic/task/GameObject/presentation容量冻结；tick内不得增长或`new`；
5. active battle超预算时按最低空槽规则确定性admission failure并记录counter，不暂停、不unseal、不GC；
6. 若未来要battle-time controlled growth，必须是用户另行批准的模式，并明确放弃“整个active battle严格0 B”；
7. Authority400=400、MobileExtended=1050 slots/1000 active不变。

## Evidence / verification

- source确认config与`-ntsdDesktopRuntimeSlotCapacity`可选择任意pre-battle hint；
- pre-seal Desktop high-slot/page growth、lowest-free/generation/pending reuse；
- sealed exhaustion/rejection 0 B；
- logic/presentation/task pool family预热；
- fresh capacity matrix与same-domain full self-check。

## Stop conditions

- 现有production无法在unsealed boundary扩到>512；
- seal后会隐式allocate或非确定性复用；
- 当前需要新增preflight API才能兑现已写合同；此时先建R7-CAP-01B Change Record；
- 用户要求active battle中真正实时增长，同时仍要求绝对0 GC：记录不可同时满足并请求重新定调。

## Out of scope

- Android真机、R8 Windows Player、C++ trace；
- 修改profile常量、allocator、pool、seal、scene或GameConfig。

## Result

已选择并确认上述合同。当前production实现已经满足：Desktop没有固定产品级active cap；每局在
unsealed loading/reset/preflight边界准备有限、按页归一化的容量；active battle seal后容量冻结并保持
strict 0 B；预算耗尽时确定性拒绝，不临时unseal或分配。默认512只是reservation hint，不是产品硬上限。

Fresh证据：capacity/pending/generation矩阵job `fdf01d6739ac47748158eb42d6d81926`为11/11 PASS；
logic pool/reuse/capacity-pressure矩阵job `e61ed948fc544caf8cc93b31f7859126`为33/33 PASS；合计44/44。
同域full `BattleRuntimeSelfCheck`于2026-08-23 03:19:45 PASS。

因此`D-CAP-001`按“交付合同澄清”关闭；无需创建或实施`R7-CAP-01B` production改动。真实Windows
Player >512与R8场景仍属于R8验收，不影响本合同决策的关闭。
