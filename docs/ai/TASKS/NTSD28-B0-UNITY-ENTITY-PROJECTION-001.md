# Task Contract — NTSD28-B0-UNITY-ENTITY-PROJECTION-001

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0-ERROR / EDITMODE-3-OF-3-PASS`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-02

## 目标

基于 `RuntimeSlotTable.ReadOnlySlotView` 与 `BattleCanonicalJson` 建立 Unity 侧 47-field 只读实体
投影。21 个 verified 和 12 个 candidate 输出当前值；14 个 missing 必须输出 JSON `null` 并列入
binding manifest，禁止补零或静默忽略。本包不接 tick hook、不加载 scenario、不写文件。

## 允许文件

- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- 对应 `.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- 对应 `.meta`
- 本 Task、同 ID Change、Ledger、STATE、handoff、总表

## 验收

- 47 leaf path 与 B0 contract 一致；21/12/14 count 固定。
- slot 顺序、allocationEpoch、identity/frame/position/motion/vitals 实值可重现。
- missing 为 null，candidate 显式列出；两次 capture canonical JSON 相同。
- Unity compile、focused EditMode、Ledger、diff check。

不修改 tick/pass、Transform、Scene、DAT、资源、authority 或旧 parity runner。

## 当前证据

- 投影源码独立 Roslyn compile exit 0；47/21/12/14 与 null contract 已写。
- focused tests 已写并修正一次真实 attribute resolution error。
- 通过现有 Unity MCP bridge 刷新同一 Editor；Unity compile 0 error。
- 前两次 focused 分别暴露 fixture 写入时点与 RawRuntime/live runtime 所有权问题；修正后第三次
  `3/3 PASS`。
- 投影现以 ReadOnlySlotView 定义 slot/epoch/occupant，以 `view.Entity.Runtime` 定义 live battle
  truth；不读取独立 raw snapshot buffer 作为逻辑真相。
