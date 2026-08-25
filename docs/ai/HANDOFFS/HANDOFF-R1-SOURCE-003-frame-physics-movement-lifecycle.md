# HANDOFF — R1-SOURCE-003 帧推进 / 物理 / 移动 / 生命周期源码盘点

> 交接日期：2026-08-21  
> 状态：**COMPLETED（静态 source inventory）**  
> 不代表：C++ executable trace、Unity 编译、self-check、Play Mode 或 gameplay 已对齐。

## 1. 本包完成内容

- 已从 C++ release live source 闭合 F00–F09：
  state400/401、500/501、frame logic、frame advance、physics、各类 landing、state9998、
  respawn、first/second Z clamp 以及 late frame tick。
- 已写 Unity crosswalk：
  `docs/ai/RESEARCH/R1-SOURCE-003-unity-crosswalk-and-diff.md`。
- 已更新 C++ behavior contract：
  `docs/ai/RESEARCH/R1-SOURCE-003-frame-physics-movement-lifecycle-contract.md`。
- 总登记册新增 D-MOV-002～005；D-MOV-001 也已纳入同一 source package。

## 2. 已登记静态差异

| ID | 结论 | 关键 evidence | 后续所有权 |
|---|---|---|---|
| D-MOV-001 | Unity 在 frame advance 前 clear current keys；C++ F03/F09 仍读取本 tick key。 | C++ `frame_advance.cpp:80-83,941-951,977-980`；Unity `SimulationWorld.Passes.partial.cs:599-612`。 | R2/R3，需 input + frame fixture。 |
| D-MOV-002 | C++ landing raw frame write；Unity `ImmediateFrame` 会提前改 PN/attacking/transistor。 | C++ `physics.cpp:157-223`；Unity `LF2Entity.cs:1196-1212`。 | R2；先由 004/005 闭合 history/candidate consumer。 |
| D-MOV-003 | Unity respawn 前全体 integer sync；C++ 仅 physics 成功尾部 sync。 | C++ `physics.cpp:326-342`；Unity `SimulationWorld.Passes.partial.cs:654-661`。 | R2/R5。 |
| D-MOV-004 | Unity-only `ThrowFrameGuard` gate；C++ 已读 live path 无同等 read。 | C++ src search；Unity `LF2Entity.cs:5278-5295,5767-5775`。 | R1-SOURCE-005 先查 writer/reachability。 |
| D-MOV-005 | exact-character ECS FrameTick 没有 C++ state2000 facing branch。 | C++ `frame_advance.cpp:884-887`；Unity `BattleEcsCharacterFrameTickPass.cs:90-209`。 | R1-SOURCE-003 fixture plan / R2；先确认 DAT reachability。 |

所有项目均是“静态差异已确认”或“可达性待验”，没有一个可报告为已修复或 runtime VERIFIED。

## 3. 明确转交的未知项

- `state9998` 的 Unity structural free 是 immediate 还是 deferred、同 tick slot reuse/newborn
  visibility；
- effect998 的 C++ direct slot/int-Z write 与 Unity factory/pool 的 exact correspondence；
- CPoint/held/link gate 对 D-MOV-003 的 integer sync 是否存在依赖；
- candidate / collision 是否会消费 D-MOV-002 影响的 frame history；
- `ThrowFrameGuard` 是否可由 snapshot restore、held/release 或 production path 写成非负值。

这些由 R1-SOURCE-004（candidate/collision）和 R1-SOURCE-005（CPoint/held/lifecycle）继续；
不要在没有此证据时直接改任一 gameplay 分支。

## 4. 变更和验证

- 只修改/新增了 `docs/ai/` 文档；没有改 Unity gameplay、测试、场景、DAT、资源或 C++。
- 未运行 Unity、C++ executable、build、self-check、trace、Play Mode 或性能测试，符合本包的
  只读范围。
- 已执行 `git diff --check` 针对本包 research 文档；无 whitespace error。

## 5. 推荐下一包

**R1-SOURCE-004 — candidate collect / collision / hit / grab / weapon contract。**

它应先建立 C++ T10/T11/T13 与 Unity snapshot → pair-vRest → candidate →
character/object consume 的逐层 contract，重点确定：

1. D-MOV-002 的 frame history 是否影响 candidate / hit filter；
2. C++ candidate carrier 的创建、排序、去重、vRest、消费和释放时点；
3. Unity broadphase/optimized path 仅作 adapter 时必须保留的顺序不变量；
4. candidate result 写入和 hit/weapon/grab 的字段合同。

不得先移动 pass、删 snapshot、删 quadtree 或修改 hit 逻辑。

## 6. 持续约束

- C++ release `J:\QQFile\NTSD2.4\ntsd_release` 继续只读，不启动 executable，不向
  authority directory 写入；
- R1-WP02 full trace 仍是 BLOCKED；它不阻止 source inventory，但不能被假装为 trace；
- 保持 CentralOnly、Texture2DArray/dynamic Mesh/URP、扩展容量、30 Hz、FrameInputSet、
  SoA/ECS/pool/worker/zero-GC 交付边界；
- T8 `stage.dat` 默认部署仍暂缓；
- 直到 R1-SOURCE-007 完成，不得开始 R2 gameplay patch。
