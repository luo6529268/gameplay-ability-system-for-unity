# NTSD28-B0-UNITY-DOMAIN-RAW-EXPORTER-001 — Unity B0 domain raw exporter

<!-- CHANGE-RECORD
id: NTSD28-B0-UNITY-DOMAIN-RAW-EXPORTER-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan B0 raw-domain contract plus the accepted source-model capture for input masks and producer availability; Unity FrameInputSet, DeterministicRng and RuntimeSlotTable are diagnostic sources only and no cross-stream equivalence is asserted.
evidence: UNITY-COMPILE-0 / JOINT-EDITMODE-9-OF-9-JOB-1F2D909D62DD42B18B5B9A0356FDE9DC / UNKNOWN-KEY-FAIL-CLOSED / REAL-INPUT-MASKS-17-2-1-96-0-12 / UNITY-DOMAIN-RAW-VALID-3-TICKS / UNITY-ENTITY-RAW-SHA-ADD7AEABCCEFC77BDB621219107DB8228AD3C230DADB61270A641CBCDF0ED97D / UNITY-DOMAIN-RAW-SHA-D888201F168A14BF73BB8F06CC822856BDC8AC039E840A46A599218BA04BBFC8 / DETERMINISTIC-RERUN-BOTH-OUTPUTS / UNITY-RNG-INITIAL-0-DELTA-1-2-1 / SLOT-CAPACITY-400-ACCEPTED-EXCEPTION / SLOT0-1-EPOCH1 / CROSS-ENTITY-RAW-20-DIFFERENCE-27-EQUAL-75-OCCURRENCES / DOTNET-BUILD-0-0 / DOMAIN-12-12 / EXISTING-21-21 / RAW-5-5 / FORMAT-PASS / GLOBAL-LEDGER-90-RECORDS-16-FILES-PASS / SCOPED-DIFF-CHECK-PASS / EDITOR-DIAGNOSTIC-ONLY / PRODUCTION-RULES-UNCHANGED / UNITY-DOUBLE-RNG-NOT-FABRICATED
-->

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0 / JOINT-9-OF-9 / REAL-DOMAIN-VALID`

本包在现有Editor exporter中接入非空输入和domain raw；不改变生产tick/input/RNG/slot。

## 实施前事实

- `SimulationInputButtons`位序已与B0 contract一致；`SimulationFrameInputModule`消费`Buttons`并写入每tick
  complete-packet key，故scenario event应构造成完整held state而非pressed edge。
- Unity `DeterministicRng`仅有`State/CallCount`单stream，不能映射成authority CRT或synchronized。
- `SimulationWorld.TryGetRuntimeSlotReadOnlyViewForDiagnostics`可无副作用取得claimed occupant与allocationEpoch。

## 验收结果

- fresh Unity compile0；job `1f2d909d62dd42b18b5b9a0356fde9dc` 9/9，Console error0。
- exporter/assembly SHA为`C41F4B...4E8E8`/`B28AC7...07C6D`；entity/domain SHA为
  `ADD7AE...ED97D`/`D88820...BBFC8`，两次确定性；domain validator valid3 ticks。
- masks `(17,2)/(1,96)/(0,12)`；Unity RNG initial0、delta1/2/1；capacity400、slot0/1 epoch1、events空。
- 跨端entity raw为20 differences/27 equal/75 occurrences，首差tick3 slot0 controlSlot 2/1。
- .NET 0/0、12/12、21/21、5/5、format与Ledger90/16、diff通过；production未改，双RNG未伪造。
