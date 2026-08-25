# R8-FUNCTIONKEYMODE-001 — mode-configured F7/F8/F9

<!-- CHANGE-RECORD
id: R8-FUNCTIONKEYMODE-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/App/GameConfig.cs
code-path: Assets/NTSD/Scripts/App/BattleFunctionKeyModeRule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleFunctionKeyInputLatch.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleFunctionKeyModeEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleFunctionKeyPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\core\main.cpp:157-205; src\entity\game_tick.cpp:223+,310+,2086-2089; user mode-policy requirement
evidence: EDITMODE-4-OF-4-AND-18-OF-18 / PLAY-PASS-20260824-122606 / SELF-CHECK-PASS-20260824-122841
-->

> 创建日期：2026-08-24  
> 当前状态：`VERIFIED`  
> 类型：input / battle / config / test

## 1. 状态与范围

- 所属 Work Package：`R8-WP01G-R12`；
- 用户授权新增F7/F8/F9，但要求按`GameConfig.asset`模式配置；
- F1/F2与其他调试键明确排除。

## 2. Authority / 需求依据

- C++ Release正常入口处理物理F7/F8/F9；
- F7为一tick init-stats，F8/F9为mode2 request1/2；
- Unity已存在mode2核心，只缺模式策略、物理edge接入和F7 flow字段；
- Evidence：source VERIFIED，Unity实现待写。

## 3. Unity 原状与已确认差异

- `Mode2Request`可被测试/内部API写入，但没有production物理F8/F9入口；
- 没有`InitStatsRequest`与F7 postframe写入；
- `GameConfig.asset`没有模式白名单；
- 直接在Update写实体会破坏30Hz/帧同步边界，因此禁止。

## 4. 计划改动

- 独立规则对象与无分配edge latch；
- driver在LocalFreeRun捕获，tick边界消费；
- Flow/snapshot/checksum/parity完整增加init-stats字段；
- postframe应用F7，现有mode2继续负责F8/F9；
- focused与Play验收。

## 5. 不可回退边界

- 不改FrameInputSet、pass顺序、RNG、slot/generation、pool、CentralOnly、atlas、容量、AI；
- 不让未journal键盘命令进入LockstepBuffered/Manual；
- 不实现F1/F2/A→B→C。

## 6. 实际改动

- `GameConfig`增加exact mode rule，默认资产只启用标准本地`0/1`；
- 新增无分配`BattleFunctionKeyInputLatch`：F7奇偶折叠，F8/F9 latest-wins且同帧F9覆盖F8；
- `SimulationTickDriver`只在LocalFreeRun捕获物理F7/F8/F9，并在可推进tick边界消费；
- Flow新增`InitStatsRequest`，F7在entity postframe写HP3/HPBound/HP/PP=500并清exit countdown；F8/F9复用既有Mode2生产链；
- init-stats加入checksum、parity、snapshot与restore；schema按合同升级；
- `BattleTestBootstrap`只在当前mode未保留F7时继续使用旧测试forced-running快捷键；
- 新增focused与production Play probe。Play fixture显式切换标准`0/1`并在cleanup恢复场景原始`0/0`，不把未配置mode自动放行。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | Unity fresh compile | Assembly-CSharp 12:02:54、Editor 12:23:42；0 C# error | `PASS` |
| focused | mode/latch/flow | job `7c4e0d2675f74d12aacca145f75aa302`，4/4 | `PASS` |
| snapshot regression | checksum/snapshot/restore | job `dca455601f2a4997be98eae4baaa7db8`，18/18 | `PASS` |
| self-check | full BattleRuntimeSelfCheck | 2026-08-24 12:28:41 `PASS` | `PASS` |
| Play Mode | 标准模式F7/F8/F9 production tick | 12:26:06 PASS；F7=500，F8=9，F9=7/7 eligible，2 transitioned，request与cleanup通过 | `PASS` |
| C++ authority | release source static crosswalk | 已闭合 | `VERIFIED` |
| full trace | R1-WP02 | 未获得 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- dedicated worker提交snapshot前看见request，snapshot/checksum/restore回归已通过；
- 配置缺失与mode mismatch继续fail closed；
- F9验收按tail执行当时的`CountsAsRandomWeaponDropCandidate()`资格；不会强改已转换出候选类型的对象；
- 回滚仅限本Record登记文件，需用户批准。

## 9. Git / 交接

- 工作树已有大量用户/历史改动，全部保留；
- 提交hash：无；
- validator：`Tools/Validate-ChangeLedger.ps1` PASS（95 records / 122 governed code files）；scoped `git diff --check` PASS。
