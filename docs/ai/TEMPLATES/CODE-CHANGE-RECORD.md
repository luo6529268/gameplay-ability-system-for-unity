# <Change-ID> — <简短行为标题>

> **NTSD24_AUTHORITY_SUPERSEDED（2026-09-02）：** 本文包含 NTSD 2.4、旧 `ntsd_new.exe`/`game_tick(...)`、固定 30 Hz 或 Authority400 等旧权威假设，仅作为历史证据；不得据此定义当前战斗规则、pass、timing、slot、RNG、字段、生命周期、表现或“已对齐”状态。任何恢复先读 `docs/ai/CURRENT-AUTHORITY.md`；当前权威是 NTSD 2.8-Logan 正式 EXE 及其对应 playable 源码，旧结论一律 `REBASELINE_REQUIRED`。

<!-- CHANGE-RECORD
id: <Change-ID>
status: PLANNED
code-path: Assets/NTSD/Scripts/<path>.cs
authority: <C++ release live path / user requirement / N/A governance>
evidence: PENDING
-->

> 创建日期：YYYY-MM-DD  
> 最后更新：YYYY-MM-DD  
> 类型：battle / render / input / test / editor / tool / governance

## 1. 状态与范围

- 当前状态：`PLANNED`
- 所属 Work Package：
- 不属于本次范围：
- 关联 Change ID：

## 2. Authority / 需求依据

- C++ release 文件、类型、函数和 release build 参与性：
- 或用户明确需求：
- Evidence 等级：`VERIFIED` / `INFERRED` / `UNKNOWN` / `N/A`

## 3. Unity 原状与已确认差异

- Unity 文件、类型、方法：
- 改前执行顺序 / 字段语义：
- C++ 或用户要求的目标行为：
- 已确认差异：
- 依赖模块和前置条件：

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `<relative path>` | `<symbol>` | | |

## 5. 不可回退边界

- 中央表现 / `CentralOnly` / Texture2DArray / 动态 Mesh：
- `Authority400`、`MobileExtended`、`DesktopExtended` 容量合同：
- 30 Hz、`FrameInputSet`、slot/generation、SoA/ECS、对象池、worker、0 GC：
- 其他已关闭 Change ID：

## 6. 实际改动

完成代码写入后填写：

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| | | | |

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | | | `PENDING` |
| focused test / self-check | | | `PENDING` |
| Play Mode / 集成 | | | `PENDING` |
| C++ authority 对照 | | | `PENDING` |
| 可选 full trace | | | `BLOCKED` / `PENDING` |

不能独立测试的子流程必须写明依赖、联合验收入口与“待测试”原因。

## 8. 风险、回滚与未关闭项

- 已知风险：
- 未关闭项：
- 回滚方式：
- 若 superseded，后续 Change ID：

## 9. Git / 交接

- 修改前工作树基线：
- 实际 diff 范围：
- 提交 hash（若已提交）：
- `Tools/Validate-ChangeLedger.ps1` 结果：
- 交接需优先阅读的文件：
