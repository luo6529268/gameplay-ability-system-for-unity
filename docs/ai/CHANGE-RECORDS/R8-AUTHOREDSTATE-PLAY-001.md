# R8-AUTHOREDSTATE-PLAY-001 — authored state production Play witness

<!-- CHANGE-RECORD
id: R8-AUTHOREDSTATE-PLAY-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleAuthoredStateResidualPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\frame_advance.cpp:884-887; src\entity\game_tick.cpp:375-382; official DAT
evidence: PLAY-PASS-20260824-122217 / SELF-CHECK-PASS-20260824-122841
-->

> 创建日期：2026-08-24  
> 当前状态：`VERIFIED / TEST-ONLY`  
> 类型：TEST-ONLY / Editor / Play acceptance

## 1. 状态与范围

- 所属 Work Package：`R8-WP01G-R11`；
- 只新增production Play witness，不修改production战斗代码；
- 不属于本次范围：不存在的type0 state2000、不可达OID999 frame399、Unity专属类型壳错配、C++ executable trace。

## 2. Authority / 需求依据

- C++ Release live source确认state2000 facing与state8xxx transform时点/字段；
- 正式DAT只读盘点确认可用样板，不以旧C#或Unity self-check定义行为；
- 用户授权对需要验收的任务执行测试。

## 3. Unity 原状与已确认差异

- state2000 exact代码与focused/self-check已有证据，但旧文档错误等待不存在的type0样板；
- state8xxx通用实现、GPU/Game/Scene证据已有，但旧probe运行时缺失恢复后的正式type0资产，错误记录为0 authored frame；
- 现状应通过正式production pool/full tick重新裁决。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/BattleAuthoredStateResidualPlayModeProbeEditor.cs` | Editor-only probe | 不存在 | 生产pool、完整tick、Central command与cleanup witness |

## 5. 不可回退边界

- production gameplay、DAT、CentralOnly/atlas/render architecture、容量、30Hz、FrameInputSet、slot/generation均不改；
- 不增加角色/OID专项production分支；测试OID只是正式样板选择。

## 6. 实际改动

- 新增Editor-only production probe及meta；
- 使用正式runtime catalog、production factory/pool和完整`StepOneTick`创建OID150/OID32样板；
- 对worker发布的逻辑snapshot在主线程显式调用既有`MaterializeCommands`后验收Central body command；这只修正验收取样时点，不改production表现链；
- 清理恢复实体、slot、pool、RNG、sound、driver pause与presentation publication。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | Unity fresh compile | Editor DLL 2026-08-24 12:19:48；0 C# error | `PASS` |
| focused/self-check | full self-check | 2026-08-24 12:28:41 `PASS` | `PASS` |
| Play Mode | R11 authored-state production probe | 12:22:17 PASS；state2000两方向、state8032 DAT/frame/offset/catalog/command/UV、cleanup均通过 | `PASS` |
| C++ authority | release source + official DAT static crosswalk | 已闭合 | `VERIFIED` |
| full trace | R1-WP02 | 未获得 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- 首轮命令断言发生在worker逻辑snapshot尚未主线程物化时；catalog lookup已命中。验收改为调用既有materialize边界后PASS，不是gameplay修复；
- R1-WP02 full trace仍BLOCKED，不由本Play证据替代；
- 回滚仅限本test文件与文档，需用户批准。

## 9. Git / 交接

- 工作树已有大量用户/历史改动，全部保留；
- 提交hash：无；
- validator：`Tools/Validate-ChangeLedger.ps1` PASS（95 records / 122 governed code files）；scoped `git diff --check` PASS。
