# R1-SOURCE-001 — C++ 主 tick 源码行为合同与 Unity 静态差异盘点

> 建立日期：2026-08-21  
> 状态：`COMPLETED（静态主 tick contract；不代表 R1 全部完成）`  
> 类型：只读源码审计；不修改 Unity/C++ gameplay。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 中参与 `ntsd_new.exe` release 构建的 C++ live battle runtime。  
> 正式入口：`src/entity/game_tick.cpp::game_tick(...)`。

## Goal

为后续所有 Unity 脚本修复建立 C++ 主 tick 的可审计行为合同，并在 Unity 侧建立完整 source-pass crosswalk 与差异清单。此 Work Package 不解决任何单个技能，也不实施 R2 调度改动。

## Scope

- 只读 C++ release source、Makefile/live-path 依赖和 Unity 当前 runtime source；
- 从 `game_tick(...)` 开始，记录 pass 顺序、slot scan、前置条件、字段/关系副作用、整数/浮点转换、生命周期与 render handoff 边界；
- 映射 Unity 的 `SimulationTickDriver`、`NTSDBattleTickSystem`、`SimulationWorld` 与各对应 service/pass；
- 为每个确认差异写明状态、依赖、不可回退边界和后续验收方式；
- 将完整 R1 工作拆分为后续可独立完成的 source-inventory Work Package。

## Authority / Evidence

- `VERIFIED` 仅用于已从 C++ release live source / build list 闭合的事实；
- `INFERRED` 仅用于有直接源码线索但调用链尚未闭合的判断；
- `UNKNOWN` 是合法结论，不能用旧 C#、Unity self-check、Authority400、checksum 或性能数据补全；
- `R1-WP02` 的只读 full trace 继续 BLOCKED，不能通过 instrumentation、hook、patch、重建或运行 C++ executable 绕过；该 blocker 不阻断本 Work Package 的源码合同工作。

## Out of Scope

- 修改 C++ source、Makefile、executable、DAT、资源或 C++ authority 目录；
- 启动 `ntsd_new.exe`、`ntsd_diag.exe` 或任何 C++ executable；
- 修改 Unity gameplay、pass 顺序、CPoint、WeaponSync、held/link、collision、input、opoint、render handoff 或技能；
- Unity trace、comparator、fixture/replay harness、Play Mode、性能测试、编译或 self-check；
- 将任何历史 C# 结论升级为 VERIFIED。

## Required Deliverables

1. `docs/ai/RESEARCH/R1-SOURCE-001-main-tick-contract.md`
2. `docs/ai/RESEARCH/R1-SOURCE-001-unity-crosswalk-and-diff-inventory.md`
3. 本任务文档中的后续 source-inventory Work Package 列表与验收边界
4. `STATE.md` 的实际进度、未知项和 blocker 更新
5. 结构化 handoff

## Contract Schema

每一个发现必须至少记录：

| 字段 | 要求 |
|---|---|
| C++ authority | 文件、函数、调用关系、Makefile/release 参与性、证据等级。 |
| Unity 映射 | 文件、类型、方法/pass、字段或 adapter。 |
| 前置条件 | slot、state/frame、关系字段、对象类别、输入或 stage 条件。 |
| 顺序 / 数据 | 读写字段、branch、early return、slot order、float/int 语义和副作用。 |
| 差异状态 | `待盘点` / `待处理` / `逻辑已对齐，待测试` / `已验证` / `UNKNOWN` / `不适用`。 |
| 验收 | 代码级核对、focused test、Play Mode、联合验收或 future full trace。 |
| Unity 边界 | CentralOnly、中央表现、Texture2DArray、容量 profile、30 Hz、FrameInputSet、pool/SoA/worker/0 GC。 |

## 后续拆分

| ID | Goal | 当前关系 |
|---|---|---|
| `R1-SOURCE-001` | 主 tick、scheduler、C++ checkpoint 与 Unity pass map | 当前 Work Package |
| `R1-SOURCE-002` | 输入、组合键、AI 输入与 frame boundary | 依赖 001 pass map |
| `R1-SOURCE-003` | frame advance、physics、移动、落地、状态维护 | 依赖 001/002 |
| `R1-SOURCE-004` | candidate collect、collision/hit、抓取与武器交互 | 依赖 001 |
| `R1-SOURCE-005` | CPoint、held/link、opoint 与生命周期 | 依赖 001/004 |
| `R1-SOURCE-006` | renderer handoff 与 Unity 中央表现行为合同 | 依赖 001/005 |
| `R1-SOURCE-007` | 汇总全量差异、依赖图与 R2–R6 子流程验收矩阵 | 依赖 001–006 |

## Verification

- 每个 C++ source 坐标均能回到 release build list 与 `game_tick(...)` live call chain；
- Unity 映射是实际调用者/被调用者和字段，而非按名称猜测；
- 未对 Unity/C++ runtime 写入；
- 所有未闭合结论明确标记 `INFERRED` 或 `UNKNOWN`；
- 本 Work Package 的文档与 `STATE.md`、handoff 一致。

## Stop Conditions

- authority 无法从 C++ release live path 闭合；
- 发现必须运行 C++ executable、实现 trace 或修改 gameplay 才能继续；
- first mismatch 指向 scope 外模块；
- 需要改变长期架构、pass ordering、验收标准或 Unity 已交付边界；
- 用户提出新的 Change Request。
