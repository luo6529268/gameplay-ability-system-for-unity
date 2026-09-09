# NTSD28-B0-FRAME-COUNTER-BINDING-001 — frameCounter diagnostic binding correction

<!-- CHANGE-RECORD
id: NTSD28-B0-FRAME-COUNTER-BINDING-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan frame_machine.cpp FrameMachine28::step; Unity LF2Entity.RunCommonFrameTick and BattleEcsCharacterFrameTickPass; real raw first-difference.
evidence: RELEASE-BUILD-0-WARN-0-ERROR / TOTAL-SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / UNITY-COMPILE-0-ERROR / JOINT-EDITMODE-6-OF-6-PASS / REAL-FRAMECOUNTER-1-2-3 / UNIQUE-DIFFERENCES-18-TO-17 / MATURITY-22-11-14 / CONTRACT-SHA-B186E4C6 / GLOBAL-LEDGER-PASS / NO-BATTLE-RUNTIME-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-DIFFERENCE-CLOSED`

修改前真实 raw：authority frameCounter 为 tick1/2/3，Unity 投影恒为0。源码确认 Unity
`AttackingCounter` 执行同一驻留递增/超过wait清零合同，而现绑定的 `FrameWaitCounter` 仅用于直接
frame write 边界。预期只修投影与 contract maturity，不改 production counter 写入。

回滚：把 contract/projection 恢复到本包前的 candidate/FrameWaitCounter 和21/12/14 header/test；
不触碰其他 runtime 字段或 raw comparator。

首次真实重跑仍为frameCounter=0，未满足本包验收。进一步证明 exporter 继承的
`NeedClearInput=true` 会走 entry-clear early return，而 driver 外层仍返回 true；已建立前置包
`NTSD28-B0-UNITY-COMPLETED-TICK-BOUNDARY-001`。在该包关闭前，本包不得晋级 focused pass。

## 最终结果

- completed-tick前置包已关闭；每个 raw tick exact character frame-tick delta=2。
- contract/projection使用AttackingCounter；FrameWaitCounter保持原runtime/checksum语义不变。
- maturity从21/12/14变为22/11/14；contract SHA为
  `B186E4C6904B3D319FE7C84CA49FEFBE6333FD63CE9EDBE8D3999840C43834C3`。
- 工具build0/0、总self-test21/21、raw self-test5/5、format PASS；Unity compile0，联合focused6/6。
- 真实双端raw中frameCounter逐tick1/2/3且不再是差异；总差异18→17、equal29→30、
  occurrence105→99。没有改production battle counter writer。
- Ledger72 records/14 governed code files/PASS；diff check无error。
