# NTSD28-B0-CONTROL-SLOT-BINDING-001 — native +0x000 / AnimCounter binding

<!-- CHANGE-RECORD
id: NTSD28-B0-CONTROL-SLOT-BINDING-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 control_slot_000 readers/writers in input_routing.cpp and battle_world.cpp; Unity AnimCounter locomotion/damage/opoint readers/writers; real raw first difference.
evidence: BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / JOINT-EDITMODE-6-OF-6 / NONZERO-ANIMCOUNTER-PROJECTION / REAL-CONTROLSLOT-DIFFERENCE-CLOSED / MATURITY-23-11-13 / GLOBAL-LEDGER-PASS / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

本包只修contract/projection/maturity/test。schema名保留controlSlot以兼容，但正式释义是native
Entity+0x000共享cycle/关联字段，Unity对应AnimCounter；不得用于玩家输入ownership判断。

回滚：恢复该字段MISSING/null与22/11/14；不触碰AnimCounter production writers。

## 实际结果

- contract与projection绑定AnimCounter，字段从MISSING晋级VERIFIED；当前maturity23/11/13。
- 工具build0/0、self-test21/21、raw self-test5/5、format PASS；contract SHA
  `21E6F6BCB1548D1E32F4DA4C3342939AC5008BA9570BAD559E4DFADE845537FC`。
- Unity compile0；联合job `68debbf20a7e4cf89b75e4cd4292713c` 6/6 PASS，duration3.3964892s；
  focused projection把AnimCounter5输出为controlSlot5。
- 公共真实raw中该字段双端均0，差异16→15、equal31→32、occurrence96→90；首差异变为
  frame.actionLatch。没有修改任何AnimCounter生产写入。
- Ledger74 records/14 governed code files/PASS；diff check无error。
