# HANDOFF — R1-SOURCE-007 全量静态差异盘点闭合

> 交接日期：2026-08-21  
> 状态：COMPLETED（static source inventory complete / runtime acceptance pending）  
> 不代表：任何 Unity gameplay 已修复、C++/Unity 行为已对齐、Unity 编译通过、BattleRuntimeSelfCheck
> 通过、Play Mode 通过、C++ executable trace、性能验收或像素级显示验收。

## 1. 本包完成范围

R1-SOURCE-001 至 R1-SOURCE-007 的唯一目标是建立可追溯的静态源代码盘点，供后续按小批次
修复和验收使用。本包已经完成：

1. COV-001～006 的 C++ Release live-source 行为合同与 Unity source crosswalk 收口；
2. 已发现差异、待测试项、UNKNOWN 和用户已批准 Unity adaptation 的唯一编号登记；
3. C++ `game_tick(...)` 主流程至 input、frame/physics、candidate/hit、CPoint/held/link/opoint、
   lifecycle、render handoff 的 producer → consumer 依赖图；
4. 后续最小闭合 repair batch、Change Record 前置条件、停止条件与分层验收矩阵；
5. R1-WP02 只读 full C++ trace 的 BLOCKED 状态和四个 blocker 的保留。

没有修改任何 Unity/C++ gameplay、renderer、shader、scene、resource、DAT、测试或构建配置；
没有运行 C++ executable、Unity compile、BattleRuntimeSelfCheck、Play Mode、性能测试、trace、
fixture、replay harness 或 comparator。

## 2. 权威和证据口径

- 唯一行为权威始终是
  `J:\QQFile\NTSD2.4\ntsd_release` 内实际参与 `ntsd_new.exe` release 构建的 C++ live
  battle runtime；
- C++ Release 工程严格只读。本包只读取源码、Makefile 和既有项目资料，未写入 authority 目录；
- Unity、历史 C#、Authority400、self-check、checksum、fast-path proof、0 GC/1000 AI 资料仅为
  历史移植、回归、性能或诊断辅助，不能单独裁决 C++ 对齐；
- 本包的“完成”只等于静态盘点和后续验收设计闭合。实际运行时差异、资源绑定、视觉结果和性能
  均仍待后续按验收矩阵验证。

## 3. 产物索引

| 产物 | 用途 |
|---|---|
| `docs/ai/RESEARCH/R1-SOURCE-ALL-DIFF-REGISTER.md` | 唯一全量差异总索引；含 D-/A-编号、状态、证据与后续 owner。 |
| `docs/ai/RESEARCH/R1-SOURCE-INVENTORY-COVERAGE-MATRIX.md` | COV-001～007 覆盖和静态收口条件。 |
| `docs/ai/RESEARCH/R1-SOURCE-007-dependency-graph-and-repair-batches.md` | 跨 pass 依赖图、修复批次、文件边界、风险和停止条件。 |
| `docs/ai/RESEARCH/R1-SOURCE-007-subflow-acceptance-matrix.md` | 单子流程至联合 fixture、编译/self-check、Play Mode、future trace 的分层验收矩阵。 |
| `Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` | 项目主计划中的 R1-SOURCE-001～007 状态与 R2 入口。 |
| `docs/ai/TASKS/R1-SOURCE-007-inventory-closure-dependency-acceptance-matrix.md` | 本包 Task Contract 与 completion record。 |

总登记册目前包含 43 个 D-差异/风险编号：

- D-SCHED：12；
- D-INP：6；
- D-MOV：5；
- D-COL：5；
- D-HIT：3；
- D-LINK：2；
- D-HOLD：2；
- D-CPT：2；
- D-OP：1；
- D-RENDER：5。

另有 A-RENDER-001～004 四个**保护边界**，不是 defect：CentralOnly /
Texture2DArray / dynamic Mesh / URP、1.5× visual scale、fixed-world logic camera，以及
MobileExtended/DesktopExtended 容量策略均不得因对齐工作被回退。

## 4. 未闭合项与状态边界

### R1 静态盘点

已完成。所有当前已发现 D-/A-条目均已有唯一来源、Unity mapping、状态、依赖或最小补证路径。
这不等于所有真实行为差异均已被运行时复现，也不等于 Unity 已对齐 C++ Release。

### R1-WP02 — full C++ trace

仍为 **BLOCKED**；本包不尝试解除它。现存 blocker：

1. **B-R1-WP02-01**：未发现能从未修改 Release runtime 取得 full schema 的既有只读观察通道；
2. **B-R1-WP02-02**：没有既有逐 tick input journal/replay 或 non-interactive CLI；
3. **B-R1-WP02-03**：已知诊断路径可能相对路径写入，非 authority 工作目录的资源加载与无写入
   保证尚未闭合；
4. **B-R1-WP02-04**：C++ source/Makefile 与现有 `ntsd_new.exe` 尚没有精确 build identity。

在用户提供或确认既有、可重复、不会改写 authority 的观察/replay 方案之前，不得自行运行、
复制、重建、插桩、hook、注入、patch C++ Release 工程，或以 Unity trace/comparator 绕过
该 blocker。

### 尚未执行

- R2～R8 gameplay 修复；
- Unity trace、comparator、fixture/replay harness；
- Unity compile、self-check、Play Mode、性能测试；
- C++ runtime / full trace；
- T8 默认 `stage.dat` 部署。

## 5. 后续推荐的第一个实施包

**R2-PASS-01 — 主 scheduler 与 pass 边界。**

理由：D-SCHED-001～005、010 等 producer 顺序会放大 input、held/link、candidate、CPoint、
WeaponSync、render handoff 的所有下游偏差。应先关闭 scheduler/pass boundary，再进入独立
input、frame/physics、collision/hit、relationship/lifecycle、presentation 批次。

在开始前必须同时满足：

1. 用户明确确认进入 R2-PASS-01；
2. 先建立新的脚本 Change Record，写明范围、C++ 依据、Unity files、预期差异、验证、回滚和
   stop condition；
3. 只修改该闭合批次内文件，不触碰已批准 Unity adaptation 边界；
4. 代码完成后按矩阵从 S0/S1/S2 开始，不能把静态阅读或单一历史 self-check 误报为
   C++ runtime 对齐；
5. 若先行条件不足、需要改长期架构/验收标准、或差异落到范围外模块，停止并更新 handoff。

## 6. 持续不变量

- C++ Release live runtime 是唯一 battle authority，严格只读；
- C# 工程及所有旧 parity/diagnostic 不得升级为 authority；
- Unity 维持 30 Hz、FrameInputSet、SoA/ECS、pool/worker/no-GC 方向；
- Unity 维持 CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5× visual scale、逻辑/显示
  相机分离与扩展容量策略；
- T8 默认 `stage.dat` 部署继续暂缓；
- 任何新的脚本代码改动都必须先建立 Change Record，再留下 ledger、STATE、handoff 和实际验证
  证据。

## 7. 交接结论

可以宣称的结论只有：

> C++ Release → Unity 的 R1-SOURCE-001～007 静态源码差异盘点已完成，后续修复顺序、依赖和
> 验收合同已经落盘。

不能宣称：

> Unity 战斗已与 C++ Release 对齐，或全部差异已经修复/验证。

下一步等待用户确认是否启动 R2-PASS-01；在确认前保持停止状态。
