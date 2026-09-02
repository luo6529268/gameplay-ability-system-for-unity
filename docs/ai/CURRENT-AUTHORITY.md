# NTSD 当前权威恢复入口

> 决策 ID：`GOVERNANCE-NTSD28-LOGAN-AUTHORITY-MIGRATION-001`  
> 生效日期：2026-09-02  
> 状态：`USER_CONFIRMED / DOCUMENT_MIGRATION_ACTIVE / UNITY_REBASELINE_NOT_STARTED`

这是任何新任务、上下文压缩恢复、交接或历史文档检索后必须首先读取的唯一权威入口。
若其他文档、旧 Change Record、旧 Handoff、测试名或注释与本文件冲突，以用户当前明确要求和
本文件为准；不得沿用 NTSD 2.4 的结论继续修改 Unity。

## 1. 当前唯一战斗行为权威

- 根目录：`J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan`
- 正式发行 EXE：`NTSD2.8-Logan.exe`
- EXE SHA-256：`1277B70BA030A1F33B625EEA20B43834325B280CEC555650BF43CD90A64DAF75`
- EXE FileVersion：`2.8.3.3`
- EXE ProductVersion：`2.8.3.3-development`
- EXE OriginalFilename：`Ntsd28Playable.exe`
- 正式启动器：`Start_NTSD2.8-Logan.cmd`
- 对应源码声明：`source\README_SOURCE.md` 明确说明 `source\` 是当前发行 EXE 对应的 C++ 源代码快照。
- 唯一 runtime 资源根：`resources\runtime`；正式启动器同时把 `--resource-root` 和
  `--complete-vfs-root` 指向该目录。

裁决优先级如下：

1. 用户在当前任务中的明确要求。
2. 上述固定 SHA 的正式 `NTSD2.8-Logan.exe` 的实际可观察行为。
3. `source\README_SOURCE.md` 声明对应正式 EXE、且实际进入 playable 构建闭包的源码。
4. 正式启动参数及 `resources\runtime` 中被正式 EXE 消费的数据。
5. 新权威目录中的 tests、diagnostics、候选 build 和研究记录，仅可作辅助证据；不能覆盖正式 EXE。
6. Unity 当前实现、self-check、旧 trace、旧对齐结论和历史文档只能作为待重新核验的实现或证据。

源码重建输出 `source\ntsd28_playable\build\Ntsd28Playable.exe` 不会自动覆盖根目录正式 EXE。
因此“源码可编译”或“候选 EXE 行为”不能自动晋升为正式发行行为；任何晋升必须由用户明确确认，
并更新本文件中的正式 EXE 指纹。

## 2. 当前源码恢复入口

| 领域 | 当前入口 |
|---|---|
| 正式 host 与每步调用 | `source\ntsd28_playable\src\game_session.cpp` 的 `GameSession28::step()` |
| 战斗主 tick 与 pass 顺序 | `source\ntsd28_core\src\simulation\simulation_tick_driver.cpp` 的 `SimulationTickDriver28::step(...)` |
| World、实体、关系与生命周期 | `source\ntsd28_core\src\simulation\battle_world.cpp` 的 `BattleWorld28` |
| 帧状态与帧运动 | `source\ntsd28_core\src\simulation\frame_machine.cpp`、`frame_motion.cpp` |
| 物理积分 | `source\ntsd28_core\src\simulation\physics_integrator.cpp` |
| 碰撞候选与命中消费 | `source\ntsd28_core\src\simulation\hit_candidates.cpp` 及 `battle_world.cpp` 的消费路径 |
| 输入路由与 AI | `source\ntsd28_core\src\simulation\input_routing.cpp`、`native_ai.cpp` |
| 对象生成与 OPoint | `source\ntsd28_core\src\simulation\object_spawning.cpp` 及 `battle_world.cpp` 的 tick 尾部 |
| 逻辑表现快照 | `source\ntsd28_core\src\simulation\render_snapshot.cpp` |
| 正式 D3D11 表现与插值 | `source\ntsd28_playable\src\d3d11_renderer.cpp`、`presentation_interpolation.cpp` |
| 战斗流程与程序入口 | `source\ntsd28_playable\src\battle_flow.cpp`、`main.cpp` |

这些文件只是入口。处理具体行为时仍须沿调用链追到字段定义、读写者、前置条件、分支顺序、
RNG、slot 生命周期和最终可观察副作用，并确认文件实际进入 playable 构建闭包。

## 3. 已观察的新基线；Unity 尚未据此改动

以下是对当前权威包的只读观察结果，不等同于 Unity 已经对齐：

- `resources\runtime\decoded_dat\data\system.dat`：正常 `fps_value: 33`，F5 快速模式
  `fps_value_f5: 3`。因此旧文档中的“权威固定精确 30 Hz / `1f / 30f`”不能继续作为
  NTSD 2.8-Logan 的规则结论；Unity 当前 `SIM_DT` 状态必须在后续独立任务中重新盘点。
- 引擎 profile 使用 `maximum_slots=1000`、物理 frame id `0..999`，transient allocation
  范围 `50..999`。旧 `Authority400` 只能保留为历史 Unity 诊断 profile，不能代表新权威容量。
- 当前权威存在两个 RNG 流：MSVCR80 CRT 流，以及同步的 3000-byte table 流；不得把旧单流
  假设直接移植为新结论。
- playable 支持 30/60/120 render FPS，正式启动器请求 120；表现插值不能反写逻辑状态。

这些基线会使旧 NTSD 2.4 的 pass、timing、slot、RNG、frame、碰撞、输入、生命周期、render
handoff 和“已对齐”结论全部进入 `REBASELINE_REQUIRED`。在完成新权威源码闭环和必要运行证据前，
不得把旧验证状态直接继承为 NTSD 2.8-Logan 的完成证明。

## 4. 历史权威的废止边界

以下内容已被本决策废止为“当前行为权威”，只允许用于历史比较、迁移线索或回归夹具：

- `J:\QQFile\NTSD2.4\ntsd_release`
- `ntsd_new.exe`
- `src\entity\game_tick.cpp::game_tick(...)` 及该旧工程的 release live path
- `J:\QQFile\NTSD2.4\ntsd_release_C#`
- 基于上述旧权威形成的 Authority400、旧三方 trace、旧 Change Record、旧 Handoff、旧
  “VERIFIED / CLOSED / 已对齐”结论

历史文档可以保留当时事实，但必须带 `NTSD24_AUTHORITY_SUPERSEDED` 标记，并链接回本文件。
历史记录中的旧路径不得被解释为当前实施指令，不得因为上下文压缩、搜索命中或旧状态名而恢复。

## 5. 本次迁移不自动决定的事项

- 本次只迁移战斗规则、逻辑顺序、字段语义、时序、生命周期与可观察行为的权威。
- `GOVERNANCE-S0-UNITY-CONTENT-AUTHORITY-DIRECTION-B-001` 已冻结的 Unity
  `Assets/NTSD/Config` 内容数值权威暂不改变；是否用新包 `resources\runtime` 取代内容数值
  权威，需要用户另行明确决定。
- 本次只修改文档和治理恢复入口，不修改 C#、Scene、Prefab、DAT、资源、ProjectSettings、
  C++ 权威目录或任何运行行为。
- 旧的 30 Hz、400-slot、单 RNG、pass 顺序和已完成状态只被标记为待重新核验；不在本次文档迁移
  中直接改 Unity 实现。

## 6. 后续恢复强制流程

1. 先读本文件和根 `AGENTS.md`，再读目标模块文档。
2. 检索到 `NTSD24_AUTHORITY_SUPERSEDED` 时，只把该文档当历史证据。
3. 从第 2 节最接近问题的当前入口追踪正式 build closure 和完整调用链。
4. 明确区分“新权威已观察”“Unity 当前状态”“推断”“未知”和“用户确认”。
5. 新建独立 Task/Change 后才允许修改 Unity 脚本；先建立新权威差异和验收条件。
6. 没有新权威证据的行为保持 `UNKNOWN / REBASELINE_REQUIRED`，不得沿用旧结论补写。
