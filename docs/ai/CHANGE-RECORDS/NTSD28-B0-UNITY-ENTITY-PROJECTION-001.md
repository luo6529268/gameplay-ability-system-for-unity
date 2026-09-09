# NTSD28-B0-UNITY-ENTITY-PROJECTION-001 — Unity 47-field 只读投影

<!-- CHANGE-RECORD
id: NTSD28-B0-UNITY-ENTITY-PROJECTION-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
authority: NTSD28-B0-ENTITY-FIELD-SCHEMA-001 47-field strict contract; RuntimeSlotTable ReadOnlySlotView; user-approved B0 continuous execution.
evidence: UNITY-COMPILE-0-ERROR / FOCUSED-EDITMODE-3-OF-3-PASS / LIVE-ENTITY-RUNTIME-SELECTED-OVER-RAW-SNAPSHOT-BUFFER / FIELDS-47-BINDING-MANIFEST-21-12-14 / MISSING-ENCODED-NULL / GLOBAL-LEDGER-PASS / NO-TICK-SCENE-RESOURCE-OR-AUTHORITY-WRITE
-->

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0-ERROR / EDITMODE-3-OF-3-PASS`

实现只读 canonical projection；missing 使用 null，candidate 使用 manifest，不得写回 runtime 或把
projection 测试报告成双端 parity。

实际结果：

- 新增 slot read-only view→canonical JSON 投影及 3 个 focused Editor tests。
- manifest 固定 `47 = 21 verified + 12 candidate + 14 missing`；14 missing 全部输出 null。
- 投影只读 `RuntimeSlotTable.ReadOnlySlotView`、`NTSDEntityRuntime` 和当前 Frame DAT state；不写
  runtime/Transform/presentation。
- 初次外部 Assembly-CSharp build 为 0 error/22 existing reference warnings，但生成 csproj 未包含新
  文件，不能作为本包 compile 证据。
- 独立 .NET SDK Roslyn + Unity netstandard/Assembly-CSharp reference 对投影源码实际编译：exit 0。
- focused test 首次混编捕获 `[Test]` 被 `NTSD.Test` 命名空间遮蔽的真实源码错误，已改为
  `[NUnit.Framework.Test]`；后续 mixed net35 NUnit/netstandard harness 因 mscorlib/netstandard
  reference 冲突未进入可靠 Unity test 执行，不包装为 focused pass。
- 当前 `Library/ScriptAssemblies` 时间早于本包源码，且存在多个 Unity 进程；按规则未启动另一实例。
  随后通过现有 Unity MCP bridge 的 `refresh_unity` 在同一 Editor 内完成 refresh/compile/domain reload，
  未启动第二实例。
- Unity compile：0 error；仅有两个既有 `ProductionEntityStressEditorTests` obsolete warning。
- focused 第一次真实运行：3 completed / 2 pass / 1 fail。失败显示 Register 前写入被 canonical
  registration reset，fixture 写入时点错误；移动到 Register 后。
- focused 第二次运行仍为 2/3：证明 `ReadOnlySlotView.RawRuntime` 与 live entity runtime 是独立
  snapshot buffer。源码追踪确认 `BattleWorldEntityRuntimeSnapshot` 分别捕获
  `view.Entity.Runtime` 与 `view.RawRuntime`；投影改为 slot/epoch 从 view、战斗真值从
  `view.Entity.Runtime`。
- focused 第三次：`3 total / 3 passed / 0 failed / 0 skipped`，duration 1.2299162s。
- 全局 Ledger：`68 records / 11 governed diffs / PASS`；diff check 仅既有换行 warning。

## 2026-09-05 correction

本记录最初在MP/PP同值fixture下未能区分native current MP面。
`NTSD28-B3-C25C-E-CURRENT-MP-BINDING-CORRECTION-001`用MP200/PP173红灯纠正投影：
`vitals.currentMp`当前读取`Runtime.PP`，不再读取`Runtime.MP`；其他47-field结构和历史执行证据不变。
