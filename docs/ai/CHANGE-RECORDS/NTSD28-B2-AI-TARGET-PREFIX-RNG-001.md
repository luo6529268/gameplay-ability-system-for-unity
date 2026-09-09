# NTSD28-B2-AI-TARGET-PREFIX-RNG-001 — target prefix synchronized RNG sites

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-TARGET-PREFIX-RNG-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AiTargetPrefixRandomEditorTests.cs
authority: NTSD 2.8-Logan select_ordinary_target, step_main state3000 prefix, step_abnormal_target sites 0x13/0x14/0x15/0x18/0x19.
evidence: TEST-FIRST-7-OF-7-EXPECTED-FAIL-CLOSED-JOB-C1BEBE7B63B04C8D815B6656906694D7 / FIRST-IMPLEMENTATION-6-OF-7-FIXTURE-BOUNDARY-PRECONDITION-JOB-F19617DCAEB24944AE744FC59798EA3A / FINAL-7-OF-7-JOB-0CE190E05F894FD7ACA700B94D94B868 / RELATED-AI-185-OF-185-JOB-6E9C2D283947471AB36518D2D2C27B70 / SITES-0X13-0X14-0X15-0X18-0X19 / CACHED-TYPE-CHECK-AFTER-RNG / STATE7-NO-0X15 / BOUNDARY-NOT-FORCE-ATTACK / WARM-4096-ZERO-ALLOC / SELFCHECK-PASS-2026-09-03T09-31-53 / CONSOLE-0 / LEDGER-121-RECORDS-68-FILES / PRODUCTION-CAPTURE-UNCHANGED / PICKUP-0X16-0X17-EXCLUDED
-->

> 状态：`FOCUSED_TEST_PASS / SITES_13_14_15_18_19_READY / PRODUCTION_UNCONNECTED`

## Authority 关键顺序

- `0x13`：cached active后消费，随后才检查definition type0。
- `0x14`：threat scan后、所有target state early return前必消费；四个force字段在当前authority
  production source无writer，默认0。
- `0x15`：仅target state3000且subject state!=7时消费。
- `0x18/0x19`：abnormal target按相对X二选一，位于`0x14`之后。

## 边界

`0x16/0x17`依赖pickup state/selection差异，另包处理；本包不改production capture/commit。

## Test-first

- focused job `c1bebe7b63b04c8d815b6656906694d7`：7/7按预期失败，均为同步模式遇到
  尚未赋site的`Rand(int)`而fail closed；无编译错误或其他异常。
- 首次实现job `f19617dcaeb24944ae744fc59798ea3a`为6/7；唯一失败夹具把左目标置于x=-100，
  先命中现有x<30边界早退，未到`0x19`。夹具整体平移到场内后重跑，不改变production逻辑。

## 实际改动

- synchronized candidate模式下，cached target先按active/HP门消费`0x13`，再判type0；失败时恢复fresh target。
- 公共前缀用`0x14`；同步模式不再把Unity boundary flags冒充authority四个force-attack字段。
- state3000同步路径先检查subject state7，再决定是否消费`0x15`；命中后继续写legacy
  `KeyAttack`，它经既有桥接表示native defend。
- abnormal目标相对X两分支分别使用`0x18/0x19`；legacy CRT模式仍通过原调用保持旧结果。
- 新增7个测试，含cached non-character、force/boundary隔离、state7、state3000、左右abnormal和4096 zero-allocation。

## 验证

- final focused job `0ce190e05f894fd7aca700b94d94b868`：7/7。
- 全AI相关 job `6e9c2d283947471ab36518d2d2c27b70`：185/185。
- 完整SelfCheck：09:31:53 `PASS`；清理预期负例后Console error 0。
- Change Ledger：121 records、68 governed code files。

## 未关闭项

- production snapshot/commit未切换，真实AI仍走legacy CRT。
- pickup `0x16/0x17`、ordinary `0x3C...`、held/profile及69个surplus仍按后续包处理。
