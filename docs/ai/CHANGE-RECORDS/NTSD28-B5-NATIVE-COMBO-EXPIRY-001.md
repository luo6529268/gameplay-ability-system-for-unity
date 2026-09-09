# NTSD28-B5-NATIVE-COMBO-EXPIRY-001

<!-- CHANGE-RECORD
id: NTSD28-B5-NATIVE-COMBO-EXPIRY-001
status: VERIFIED
change-kind: BATTLE_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleNativeComboExpiryModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NativeComboExpiryEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan simulation_tick_driver.cpp expire_ordinary_combo_entries after complete slot tail; EXE B1E13AE1, closure 39DDDA15.
evidence: missing-expiry RED; focused 5/5, B5 831/831, NTSD28 1178/1178, fresh SelfCheck PASS, Console 0, Scene unchanged.
-->

> 状态：`VERIFIED / CORE_COMBO_EXPIRE_ROUTED / FORMAL_TUPLE_INACTIVE`

## 原状与计划

pass descriptor已有`CoreComboExpire`，但production tick没有调用。先以直接module和tick placement focused测试取得
缺类型RED，再实现active-slot、inclusive elapsed扫描并在`LateEntityUpdate`返回后调用。默认record absent仍no-op。

## 边界

不使用input combo timeout，不清lastTick，不把bound当expiry gate，不在C25逐slot body内重复运行；
不实现B6 caughtact、B10 presentation或H tuple activation。

## 实施结果

- 新增`BattleNativeComboExpiryModule.Expire`，只在record present且respond非负时扫描active runtime slots。
- 只处理positive count；future lastTick保留；`elapsed >= respond`时仅清count、保留lastTick；不读取bound、
  caughtact或input combo timeout。
- `NTSDBattleTickSystem`在`LateEntityUpdate(tickIndex)`结束后、任何残余兼容尾及session逻辑前调用一次；
  不进入C25逐slot body重复执行。

## 验证

RED：新增focused fixture后Unity Console精确得到6条缺失`BattleNativeComboExpiryModule`的
`CS0103`；Test Runner job `42515b2be2544abbb71b58bbe3d23e69`因此发现0 tests。该证据取得于
任何production expiry实现写入前。

| 层级 | 证据 | 结果 |
|---|---|---|
| focused | job `e785a2a62b3745ddbf6ccf0ebf0067e2` | `5/5 PASS`；inclusive/future/zero/negative/placement/lastTick与4096 warm `0 B` |
| B5 broad | job `4f793750fa484dc9904358f01d45c078` | `831/831 PASS` |
| NTSD28 broad | job `40589b84156742149af22e55c15a5416` | `1178/1178 PASS` |
| SelfCheck | `Temp/NTSD_BattleRuntimeSelfCheck.result`，2026-09-07 05:14:48 +08 | fresh `PASS` |
| Console | SelfCheck预期日志清空后的error查询 | `0` |
| Scene | active `NTSD_Battle`与磁盘文件 | `isDirty=false`、`rootCount=13`；SHA-256 `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`、mtime未变；未Play/未保存 |
| scoped diff | tracked目标`git diff --check` + 新文件人工格式核对 | `PASS`；仅既有LF/CRLF提示 |
| change ledger | `Tools/Validate-ChangeLedger.ps1` | `PASS`；350 records / 304 governed code files |

本包不激活正式tuple；B6 caughtact与B10 presentation仍未实现。因此B5的普通producer与expiry行为路径
已经可注入验证，但玩家正式模式是否启用及最终显示仍受H/B6/B10后续包约束。
