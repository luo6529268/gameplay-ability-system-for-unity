# R1-SOURCE-003 — 帧推进、物理、移动、落地与状态生命周期源码合同

> 建立日期：2026-08-21  
> 状态：COMPLETED（静态 source contract；无 runtime trace / Unity 运行时验收）  
> 类型：只读 C++/Unity source 审计；不修改任何 gameplay。  
> 依赖：R1-SOURCE-001（主 tick）、R1-SOURCE-002（输入）已完成静态合同。

## Goal

从 C++ release live path 闭合 frame logic、frame advance、physics、移动/加速度/摩擦、
重力/落地、frame wait/next、state 特判、death/respawn、两次 stage Z clamp 以及
整数/浮点写回；在 Unity 侧建立 `EarlyFrameAdvance`、`FrameLogic`、`FrameAdvance`、
`DeathCleanup`、Z clamp、late entity update 的精确 crosswalk 和差异清单。

## Scope

- C++：`game_tick.cpp` 的 T04–T08/T18 相关调用点、`frame_advance.cpp`、`physics.cpp`
  及这些 live helper 实际调用到的 entity/frame/state/runtime field 定义；
- Unity：`NTSDBattleTickSystem`、`SimulationWorld.Passes.partial.cs`、frame/physics
  adapter、ECS writer/pass、`LF2Entity` / `LF2Character` 对应状态入口；
- 记录 slot 顺序、newborn visibility、early return、double/int 写回、DAT field 使用、
  RNG / lifecycle 副作用和可独立验证的 fixture。

## Out of Scope

- 修改 Unity/C++ frame、physics、movement、state、death/respawn、opoint 或 input；
- 运行 C++ executable、Unity 编译、self-check、Play Mode、trace 或性能测试；
- 把 C# 旧逻辑或 Unity 当前行为升级为 C++ authority；
- 修改 stage.dat 默认资产、中央表现、容量 profile、worker、pool 或 renderer。

## Deliverables

1. `docs/ai/RESEARCH/R1-SOURCE-003-frame-physics-movement-lifecycle-contract.md`
2. `docs/ai/RESEARCH/R1-SOURCE-003-unity-crosswalk-and-diff.md`
3. 更新主差异清单、STATE 与 handoff；必要时补充 R2/R3/R5 的依赖与验收。

## 静态盘点完成结论

- 已完成 C++ `game_tick` F00–F09 与 Unity frame/physics/lifecycle 的 source crosswalk；
- 已登记 D-MOV-001～005，均为静态差异或可达性风险，不得写成已修复或已运行时复现；
- `state9998` structural free、effect998 exact spawn、held/link/cpoint gate、candidate/prev-history
  consumer 属于 R1-SOURCE-004/005 的继续闭合项，不是本包未完成的 gameplay 修改；
- 完整 crosswalk 和 fixture 合同见
  `docs/ai/RESEARCH/R1-SOURCE-003-unity-crosswalk-and-diff.md`。

## Stop Conditions

- C++ release live-source 调用链无法闭合；
- 要继续就必须运行/修改 C++ 或 Unity gameplay；
- 发现需要改变不可回退 Unity 渲染/容量/worker/lockstep 边界；
- 用户提出新的 Change Request。
