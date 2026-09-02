# Simulation 目录物理分层计划

> Change ID：`SIMULATION-DIRECTORY-REORGANIZATION-001`
>
> 性质：纯物理目录与导航结构重组，不修改 gameplay 或运行时架构。

## 1. 目标

将 `Assets/NTSD/Scripts/Simulation` 从“71个根脚本与若干宽子目录”的平铺结构，整理为按责任域导航的目录。保持所有 namespace、类型、API、程序集、序列化 GUID 和运行行为不变。

## 2. 目标结构

```text
Simulation/
├─ Core/
├─ Host/
├─ Runtime/
├─ Passes/
│  ├─ EarlyFrameAdvance/
│  ├─ Interaction/
│  ├─ LateLifecycle/
│  ├─ Oid5152/
│  ├─ RandomWeapon/
│  └─ Respawn/
├─ Ai/
│  ├─ Kernel/
│  ├─ Runtime/
│  └─ Snapshots/
├─ DataContracts/
│  ├─ BodyBox/
│  ├─ BloodPoint/
│  ├─ CatchPoint/
│  ├─ ObjectPoint/
│  └─ WeaponPoint/
├─ Ecs/
│  ├─ Core/
│  ├─ Stores/
│  ├─ Writers/
│  ├─ Passes/
│  ├─ Hit/
│  └─ Results/
├─ Input/
├─ Lockstep/
│  ├─ Session/
│  ├─ History/
│  ├─ Snapshot/
│  ├─ Replay/
│  └─ Checksum/
├─ Stage/
├─ Presentation/
├─ Spatial/
└─ Diagnostics/
```

## 3. 文件边界

- 唯一逐文件映射：`docs/ai/MANIFESTS/SIMULATION-DIRECTORY-REORGANIZATION-001.csv`。
- 当前156个C#文件中移动142个。
- `Input/Presentation/Spatial` 已合理放置的14个文件保持原位。
- `.cs` 与同名 `.meta` 必须作为原子对移动；移动前后 GUID 必须一致。
- 不新增、删除、合并或拆分生产类型。
- 不修改 namespace；目录名不作为运行时依赖方向的替代品。

## 4. 分批执行

### R0：Preflight

1. 冻结manifest、Change Record和Task Contract。
2. 记录全部source的内容SHA-256与meta GUID。
3. 执行runtime/editor compile和路径/架构focused基线。
4. 扫描源码路径字符串与`Path.Combine`构造。

### R1：根目录分层

移动根目录71个脚本到Core、Host、Runtime、Passes、Ai/Runtime、DataContracts、Stage、Diagnostics、Lockstep/Checksum或Input。移动后Unity刷新、compile和source-path focused必须通过。

### R2：现有宽子目录分层

- `Ai`：Kernel/Runtime/Snapshots。
- `Ecs`：Core/Stores/Writers/Passes/Hit/Results。
- `Lockstep`：Session/History/Snapshot/Replay/Checksum。

移动后再次做manifest/GUID/compile/focused验证。

### R3：路径读取者更新

仅更新会读取源码物理文件的Editor测试与SelfCheck入口。禁止改变断言语义；路径应改为新owner位置，而不是递归模糊匹配。

### R4：最终验收

1. runtime/editor compile 0 error。
2. architecture与所有路径敏感测试通过。
3. AI、checksum、worker、ordered shutdown保持移动前基线。
4. 完整EditMode和BattleRuntimeSelfCheck实际执行；既有任务外baseline单列。
5. 真实Play/Stop后无cleanup warning、Scene不脏。
6. scoped diff、manifest、GUID与Change Ledger审计。

## 5. 停止条件

- meta缺失或GUID变化。
- 除明确路径读取者外任何C#内容hash变化。
- namespace、类型、API或asmdef变化。
- compile error。
- focused、checksum、worker或shutdown出现新差异。
- Unity Scene/Prefab/DAT/ProjectSettings出现本Change未声明的写入。

任一条件触发时停止后续批次，按manifest逆序恢复当前批；不得用Git破坏性命令回退。

## 6. 非目标

- 不继续拆分SimulationWorld或其他大文件。
- 不删除legacy/shadow/fallback路径。
- 不重命名namespace或类型。
- 不修改ECS、AI、Lockstep、Presentation的依赖方向。
- 不宣称目录移动带来性能收益。

## 7. 执行结果（2026-09-02）

- R0～R3已执行：142/142目标文件存在，旧source为0，内容SHA差异0，GUID差异0；
  Simulation根目录C#为0，旧源码路径引用为0。
- Unity compile为0 error；移动前后20项路径/架构矩阵均为18通过加同两个任务外
  package版本失败。AI/worker/checksum/shutdown/architecture组合为200通过加1个既有
  position38失败。
- 无MCP轮询干扰的完整EditMode执行1585/1585，5个剩余失败均记录为任务外baseline；
  fresh SelfCheck仍停在既有central-render P4断言。
- 两轮真实Play/Stop均等待25秒，退出后Scene不脏、Console warning/error为0。
- R4技术验收已完成；repository-wide Change Ledger validator仍因任务外
  `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001.md`缺少`code-path`而失败。因此本Change
  保持`BLOCKED / IMPLEMENTATION_COMPLETE`，不将技术完成包装成全局governance可交付。
