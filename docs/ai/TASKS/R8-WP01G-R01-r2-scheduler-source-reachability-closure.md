# R8-WP01G-R01 — R2 scheduler source/reachability closure

> 日期：2026-08-23  
> 状态：`COMPLETE / READ-ONLY / NO CODE CHANGE`  
> D-ID：`D-SCHED-006`、`D-SCHED-008`

## Goal

按总计划R2主调度依赖顺序，只读闭合两项最早的scheduler UNKNOWN：

1. `D-SCHED-006`：C++ release `game_tick(...)`两次Z clamp的精确位置、实体集合、double/integer字段写入、
   pending/newborn/pending-free副作用，与Unity两个StageBounds/相关pass的等价性；
2. `D-SCHED-008`：C++ candidate carrier/list的创建、消费、清空/失效和下一tick边界，与Unity candidate store、
   consume-end和tick-tail lifecycle的等价性。

本包只判断“等价 / source-confirmed difference / 仍UNKNOWN”，不修改Unity或C++。

## Scope

- 只读C++ release live source、Makefile、直接caller/callee和字段定义；
- 只读Unity scheduler、stage bounds、candidate collect/consume/store与lifecycle实现；
- 复核R1-SOURCE-003/004/005、R2-PASS-02及已有Change/测试证据；
- 建立逐pass、逐字段、逐实体资格和生命周期表；
- 更新all-diff register、STATE、总计划与handoff。

## Authority / Evidence

- 唯一裁决：`J:\QQFile\NTSD2.4\ntsd_release`实际参与release构建的live source；
- 正式入口：`src/entity/game_tick.cpp::game_tick(...)`；
- C++工程严格只读，不运行、构建、修改、复制或写入；
- Unity现状与测试只能证明实现/回归，不能反向定义C++规则；
- R1-WP02 full trace继续BLOCKED，UNKNOWN是合法结果。

## Files likely involved

### C++ authority（只读）

- `src/entity/game_tick.cpp`；
- candidate collection/consume、entity slot/lifecycle与position字段的直接实现文件；
- `Makefile`和相关header/struct定义。

### Unity（只读）

- `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs`；
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`；
- candidate collection/store/sequence runner相关文件；
- entity registry、pending-free、slot/generation与stage bounds相关文件；
- 现有self-check/Editor tests仅作覆盖证据。

## Unknowns

- 两次C++ clamp是否使用同一active/current-DAT过滤；
- clamp只写double Z还是同步integer Z/Prev2；
- newborn是否在两次clamp之间可见；
- pending-free、inactive、merge-dormant和高槽adapter是否进入clamp；
- candidate carrier是每attacker、每tick还是全局容器；
- consume后何时清空，abort/early-return是否保留尾部；
- Unity end-consumption clear是否比C++提前或延后。

## Deliverables

1. `docs/ai/RESEARCH/R8-WP01G-R01-r2-scheduler-source-reachability-20260823.md`；
2. 两个D-ID的source contract与Unity crosswalk；
3. 明确结论：`EQUIVALENT`、`SOURCE-CONFIRMED DIFFERENCE`或`UNKNOWN`；
4. 必要状态/hand-off更新；
5. 若确认差异，建立下一修复Task建议，但本包不创建Change Record、不改脚本。

## Verification

- 确认所用C++源文件实际参与release Makefile；
- caller、callee、字段定义和所有关键read/write闭合；
- 不依据函数名猜语义；
- Unity映射覆盖pass顺序、资格、字段和early-return；
- scoped docs diff-check和Change Ledger validator通过；
- Unity/C++脚本diff由本包新增为0。

## Stop conditions

- 需要运行、构建、修改或向C++ authority写入；
- 需要修改Unity gameplay、pass ordering、adapter或架构；
- first difference指向R3+模块或需要用户批准的修复；
- 证据不足时停在UNKNOWN，不用旧C#或经验补齐。

## Out of scope

Unity脚本/scene/config修改；Play Mode；Player/IL2CPP；R1-WP02替代方案；T8；Android；服务器；
完整R8最终声明。

## Completion result

- `D-SCHED-006`：`SOURCE-CLOSED / EQUIVALENT WITH APPROVED CAPACITY ADAPTER / RUNTIME_PENDING`；
- `D-SCHED-008`：`SOURCE-CONFIRMED CONDITIONAL DIFFERENCE / UNFIXED`，发生于F1/step-wait跳过
  entity post-frame tail后，C++保留carrier而Unity下一collect重置carrier；
- 已产出source/reachability报告与下一修复Task `R2-CANDIDATE-TAIL-01`；
- 本包未修改任何Unity/C++ gameplay、scene、config或测试脚本，也未运行Unity/C++ executable。
