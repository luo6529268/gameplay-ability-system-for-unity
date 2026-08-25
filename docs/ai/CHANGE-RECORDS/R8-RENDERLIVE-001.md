# R8-RENDERLIVE-001 — central liveness / identity / visibility joint Play witness

<!-- CHANGE-RECORD
id: R8-RENDERLIVE-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralLivenessIdentityVisibilityPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1008-1154,2061-2083 and src\render\renderer.cpp:517-685,1300-1438 / USER-APPROVED-R8-WP01G-R07B
evidence: CPP-RELEASE-SOURCE-CROSSWALK / UNITY-PLAY-S4 / FOCUSED-51-OF-51 / SELF-CHECK-PASS / CONSOLE-0 / LEDGER-PASS
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / editor / render

## 1. 状态与范围

- 当前状态：`VERIFIED / TEST-ONLY`
- 所属 Work Package：`R8-WP01G-R07B`
- 只覆盖：`D-RENDER-003`的pending/generation/T+1子集、`D-RENDER-004`、`D-RENDER-005`；
- 不属于本次范围：OID7/8→51 dormant/split（R08）、R07C、AI、P1/P2、T8、IL2CPP、Android、服务器；
- 关联 Change ID：`R8-OPLIFE-001`、`R6-PRES-002`、`R6-PRES-003`、`R8-SPRITEMAP-006`、
  `R8-SPRITEMAP-007`。

## 2. Authority / 需求依据

- C++ release `game_tick(...)`定义render前后pass边界、active entity选择和late object-point时序；
- C++ release `renderer.cpp`定义body/shadow选择、OID223/224 shadow gate与slot-stable painter顺序；
- 用户于2026-08-23明确批准执行`R8-WP01G-R07B`；
- Evidence等级：C++源码合同`VERIFIED`，C++ full runtime trace仍`BLOCKED`。

## 3. Unity 原状与已确认差异

- `BattlePresentationShadowBuild`已有pending、first-presentation、generation、current-DAT OID223/224、
  EntityVisible/ShadowVisible gate；既有self-check分别覆盖synthetic exact矩阵；
- `BattleOpointLifecyclePlayModeProbeEditor`已有正式opoint birth/release/generation producer；
- `BattleCentralGameVisibilityPlayModeProbeEditor`已有正式central command/submission诊断；
- 原状缺口不是已确认production gameplay差异，而是缺少同一实际producer、tick、slot/generation和central
  command/submission的联合Play证据；现阶段production first-difference=`UNKNOWN`。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/BattleCentralLivenessIdentityVisibilityPlayModeProbeEditor.cs` | 新Editor-only Play probe | 不存在 | 通过正式factory/完整tick记录pending、late T+1、same-slot generation、OID223/224 shadow gate、visibility与cleanup联合证据 |
| 同名`.meta` | Unity asset identity | 不存在 | 稳定导入新probe |

## 5. 不可回退边界

- 中央表现保持`CentralOnly`、Texture2DArray、动态Mesh、URP；不恢复Legacy owner；
- 保持1.5×visual scale、fixed-world camera和现有容量合同；
- 保持30Hz、FrameInputSet、slot/generation、SoA/ECS、对象池、worker和0GC；
- C++ authority只读；不改DAT、scene、URP asset或production gameplay/renderer。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/BattleCentralLivenessIdentityVisibilityPlayModeProbeEditor.cs` | 新Editor-only联合probe | 以正式factory生成pending fixture、OID223/224/control；完整tick观察FrameLogic pending→free、Late opoint同槽新generation、T/T+1 central diagnostic、same-Z slot order和cleanup；追加逐实体frame诊断与Temp request轮询入口；request先解除Editor全局pause及临时driver pause，记忆并在cleanup恢复driver原pause，再在正式driver推进至少5 tick后消费；tick执行按当时production worker active状态选择worker或同步driver诊断入口 | 仅在用户主动菜单或明确写入本probe专用Temp request时临时注册测试实体并写Temp JSON；不改production默认路径 |
| 同名`.meta` | Unity asset identity | 为新probe固定GUID | 仅触发Unity导入/编译 |

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | Unity全量asset refresh后读取Console error | 新probe实际导入；0 error | `PASS` |
| focused test / self-check | central24 + lifecycle9 + worker18；full self-check | jobs `f049...` 24/24、`1f7e...` 9/9、`60e...` 18/18；21:47:26 self-check PASS | `PASS` |
| Play Mode / 集成 | R07B joint probe in NTSD_Battle CentralOnly | PASS：tick202→203；slot51 gen1 pending/free后由late OID999同槽gen2；T冻结不含新gen、T+1有body/shadow中央命令；223/224 body提交且shadow=`CommandSuppressed`无命令/提交；baseline角色body/shadow提交；完整cleanup。最终Play为sync full tick，worker边界由18/18 focused补证 | `PASS` |
| C++ authority 对照 | read-only source crosswalk | live pass与renderer gate已闭合 | `PASS` |
| 可选 full trace | R1-WP02 | blocker仍存在 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- 已解决的探针风险：worker/camera在同tick可先发布无frame failure plan；非强制self-check materializer会因publication
  version已消费而复用该旧plan。最新报告已证明正式fixture存在于14实体`PublishedFrame`，故probe改用production
  `PrepareFrame(world)`强制消费最新publication；进一步确认原probe违反“每tick一次immutable snapshot”边界，
  已改为下一个正式identity tick后才读取，新结果仍须以重跑确认，而不能把该诊断升级为通过；
- identity tick首轮还确认原`X/Z=200000`隔离点会经过production StageBounds清理，三个formal identity
  句柄均转为`GenerationMismatch`，并非render隐藏；fixture已移到当前stage合法边界内的分隔位置，待重跑；
- 合法stage位置运行确认OID223/224的正式合同是快照`ShadowVisible=true`、命令阶段按current-DAT gate
  返回`CommandSuppressed`且无shadow command/submission；这不是额外隐藏。旧OID203 control因其本身为special attack
  且可能被LinkState gate合法抑制，已替换成baseline正式角色作为普通阴影对照；
- 未关闭项：dormant/split只归R08；C++ full trace仍BLOCKED；
- 若发现production first-difference，立即停止认证并拆独立repair Task/Change，不在probe中修gameplay；
- 回滚方式：仅删除本Change新增的Editor-only probe及meta，并将记录标为`ROLLED_BACK`；不得回退其他用户修改。

## 9. Git / 交接

- 修改前工作树基线：大量用户/历史修改与未跟踪文件存在；本Change不覆盖、不清理、不回退；
- 实际diff范围：仅本Editor-only probe、`.meta`及R07B文档/Change留痕；production代码0改动；
- 提交hash：未提交；
- `Tools/Validate-ChangeLedger.ps1`结果：83 records / 98 governed code files，PASS；
- 交接优先阅读：R07B Task、R07B Handoff、本Record。
