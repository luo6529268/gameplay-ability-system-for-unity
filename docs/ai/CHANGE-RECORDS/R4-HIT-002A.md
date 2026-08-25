# R4-HIT-002A — kind10/11 character raw-frame write

<!-- CHANGE-RECORD
id: R4-HIT-002A
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp:1193-1237
evidence: SOURCE-CONTRACT-VERIFIED / CODE-WRITTEN / UNITY-COMPILE-PASS-20260822-0543+08 / FULL-SELF-CHECK-PASS-20260822-054354+08 / PLAYMODE-PENDING / CXX-RUNTIME-TRACE-BLOCKED
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 类型：battle / test  
> 当前状态：`RUNTIME_PENDING` — 最小脚本、Unity compile与full self-check已通过；C++ trace和真实Play Mode仍未关闭。

## 1. 状态与范围

- 所属 Work Package：`D-HIT-002 / R4-HIT-02A`。
- 目标：将 character kind10/11的两条 Unity route从有额外副作用的`ImmediateFrame(182)`收窄为
  保留PN、attacking和wait counter的 raw-frame writer。
- 不属于本次范围：kind16、weapon raw-frame、CPoint、held/link、opoint、candidate、input、scheduler、AI、render、
  DAT/资源、C++ authority、Play Mode与C++ trace。
- 关联 Change ID：前置 `R4-HIT-001`；后续预留 `R4-HIT-02B`～`R4-HIT-02D`。

## 2. Authority / 需求依据

- C++ release live path：`src/entity/collision.cpp:1193-1237` 的kind10/11 character branch。
- C++ verified行为：阻尼/统计后原始写 `frame=182`，没有同branch的prev、attacking或wait counter写入。
- Unity current source：两个 `ApplyFluteCharacterForce` 使用`ImmediateFrame(182)`，而该 helper会写
  `Frame.PN`、清`AttackingCounter`并刷新wait/next。
- Evidence 等级：`VERIFIED` source contract；`UNKNOWN` C++ runtime trace / real Play Mode。

## 3. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs` | `ApplyFluteCharacterForce` | force后通过`ImmediateFrame`切182 | force后通过现有 raw writer切182，保留PN/attacking/wait |
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs` | `ApplyFluteCharacterForce` | shared route同上 | shared route同上 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | `CheckKind10And11CharacterStatsWithoutDamage` | 只覆盖stats | 加入 raw-frame副作用矩阵 |

## 4. 不可回退边界

- 不修改 CentralOnly / Texture2DArray / dynamic Mesh render ownership；
- 不修改 Authority400、MobileExtended、DesktopExtended 容量合同；
- 不修改 30 Hz、FrameInputSet、slot/generation、SoA/ECS、对象池、worker和0 GC保护；
- 不修改全局 frame helper；已关闭的landing raw-frame contract不被重构或覆盖。

## 5. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs` | `ApplyFluteCharacterForce` | `ImmediateFrame(182)`替换为既有`DirectWriteRawFramePreserveWaitCounter(182)` | 保留原kind10/11 force/stat顺序；停止隐式PN/attacking/wait重置。 |
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs` | `ApplyFluteCharacterForce` | shared character-DAT route同样替换 | exact/shared route得到同一raw-write合同。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | `CheckKind10And11CharacterStatsWithoutDamage` | existing exact/shared × kind10/11 fixture预置并断言PN、attacking、wait；同时继续断言frame182和既有stats | 能捕获未来误回到`ImmediateFrame`或其他隐式writer的回归。 |

## 6. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| C++ source | 只读复核 `collision.cpp:1193-1237` | raw frame contract已确认 | `PASS` |
| focused self-check | exact/shared × kind10/11 raw side effects | 四组合随完整self-check执行并通过；frame182/PN/attacking/wait与既有stats均满足 | `PASS` |
| Unity compile | 现有Editor/UnityMCP scripts refresh | Unity 2022.3.62f3 / port6401完成refresh/domain reload；filtered `error CS`=0 | `PASS` |
| full self-check | `NTSD/验证/运行战斗运行时自检` 菜单入口 | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，2026-08-22 05:43:54 +08:00 | `PASS` |
| Play Mode | flute character scenario | 用户/后续独立验收 | `PENDING` |
| C++ runtime trace | R1-WP02 | 不可用，保持 blocked | `BLOCKED` |

## 7. 风险、回滚与未关闭项

- 风险：全局helper替换或误带入kind16/weapon会改变超出本包的历史raw writer；本次只写两处callsite，full self-check已确认existing Unity data mirror不破坏现有断言。
- 回滚：若本包失败，仅回滚本Record列出的三份脚本及关联文档，不触碰工作树其他用户修改。
- 未关闭项：C++ trace、真实Play Mode、frame advance/presentation joint、`02B`～`02D`。

## 8. Git / 交接

- 修改前工作树基线：dirty；未回退、移动、清理任何预存用户/历史改动。
- 计划脚本范围：仅metadata中的三项。
- 提交 hash：未提交。
- `Tools/Validate-ChangeLedger.ps1`：文档最终更新后需重跑；脚本写入前的规划状态校验已通过。
- 交接优先阅读：本Record、`TASKS/R4-HIT-02A-kind10-11-character-raw-frame-contract.md`、
  `RESEARCH/R4-HIT-02-raw-frame-writer-split-preflight-20260822.md`。
