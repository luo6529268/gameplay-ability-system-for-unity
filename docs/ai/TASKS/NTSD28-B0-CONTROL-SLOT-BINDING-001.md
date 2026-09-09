# Task Contract — NTSD28-B0-CONTROL-SLOT-BINDING-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-DIFFERENCE-CLOSED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-02

## 目标

把 schema 历史名 `identity.controlSlot` 从 MISSING 绑定到 Unity
`NTSDEntityRuntime.AnimCounter`。新权威 `Entity28+0x000/control_slot_000` 不是玩家控制槽，而是共享
locomotion cycle/特殊关联槽字段；Unity `AnimCounter` 的走跑cycle、damage attacker-slot写入、
state3003/opoint读取形成同职责闭包。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task、同 ID Change、Ledger、STATE、handoff、总表

## 不变量

- 只补已有字段的diagnostic binding，不新增/修改AnimCounter生产写入。
- `identity.controlSlot` 晋级 VERIFIED；maturity 23 verified / 11 candidate / 13 missing。
- schema字段名本包不改，以保持v2兼容；文档明确其native offset语义，禁止解释成player input slot。
- 不处理其余15个差异。

## 验收

- 工具build0/0、self-test21/21、raw self-test5/5、format。
- Unity compile0；联合focused6/6；projection test用非零AnimCounter证明输出。
- 真实raw中controlSlot差异消失：unique16→15、equal31→32、occurrences96→90。
- Ledger/diff check通过。

## 当前证据

- contract/projection绑定 `control_slot_000→AnimCounter` 并晋级VERIFIED；maturity23/11/13。
- 工具build0/0、self-test21/21、raw self-test5/5、format PASS；contract SHA
  `21E6F6BCB1548D1E32F4DA4C3342939AC5008BA9570BAD559E4DFADE845537FC`。
- fresh Unity compile0；job `68debbf20a7e4cf89b75e4cd4292713c` 6/6 PASS，duration3.3964892s；
  projection以AnimCounter=5证明非零输出。
- 真实raw中controlSlot差异关闭：unique16→15、equal31→32、occurrences96→90，首差异改为
  frame.actionLatch；Unity raw SHA `42450040...58B5C`。
- Ledger74 records/14 governed code files/PASS；diff check无error。
