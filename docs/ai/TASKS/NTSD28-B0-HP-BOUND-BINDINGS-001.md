# Task Contract — NTSD28-B0-HP-BOUND-BINDINGS-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-BASELINE-UNCHANGED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

将 `vitals.effectiveMaxHp` 与 `vitals.baseMaxHp` 从 candidate 晋级 VERIFIED，既有投影分别保持
`NTSDEntityRuntime.HPBound` 与 `NTSDEntityRuntime.HP3`。

权威端 `effective_max_hp` 会随伤害、环境伤害、恢复和融合变化，普通复活从
`base_max_hp` 恢复；只有排队复活会同时重写两者。Unity 的 `HPBound` 与 `HP3` 在对应生产链上
承担相同职责。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只改 maturity/测试；projection字段不换，production HP/伤害/恢复/融合/复活逻辑不改。
- maturity `33/7/7 -> 35/5/7`；neutral真实差异 `9/equal38/occurrence54` 保持不变。
- focused 使用互异值 `HPBound=480`、`HP3=500`，分别断言 effective/base，防止交叉映射。
- 工具 build 0 warning/0 error、21/21、5/5、format；Unity compile 0、联合6/6；Ledger/diff通过。
- 不修改 DAT；`baseMaxMp` 内容差异继续等待 B11 用户策略。

## 回滚

恢复两个 candidate 与 maturity `33/7/7`；不触碰 production vitals writer。

## 实际结果

- contract两项晋级VERIFIED，maturity `35/5/7`；contract SHA
  `112C0B23A91B9A6F470B924906D8D6583844E8B6F90A1DBB106DE33D6E582FCF`。
- 工具build 0 warning/0 error、21/21、5/5、format PASS；Unity fresh compile 0 error；job
  `b050dd2519714976b62eb10f7c1b05bc` 6/6 PASS，HPBound480/HP3=500互异投影通过。
- Unity focused raw SHA `D1628540F66B2996DA53E83AA4179FFCA9ECE736B4556691EF0E24E50378E1D5`；
  authority raw SHA `9AFFE2F61C52A358900F2FE53642B99D10FC7C6BCE69F25752F8C7CE50475E3C`；
  neutral真实差异保持9/equal38/occurrence54，首差异baseMaxMp。
- 一次参数未嵌套的误请求额外运行1601项，10项既有/任务外失败；目标两类测试无失败，随后精确6/6通过。
- production vitals、DAT、资源与Scene未改。
- Change Ledger validator 82 records / 14 governed code files PASS；scoped diff check PASS（仅既有LF→CRLF提示）。
