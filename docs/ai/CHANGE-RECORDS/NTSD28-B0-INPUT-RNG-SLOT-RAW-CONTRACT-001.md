# NTSD28-B0-INPUT-RNG-SLOT-RAW-CONTRACT-001 — B0 raw domain contract

<!-- CHANGE-RECORD
id: NTSD28-B0-INPUT-RNG-SLOT-RAW-CONTRACT-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/B0DomainRawContract.cs
code-path: Tools/NTSD28Parity/B0DomainRawContractSelfTest.cs
code-path: Tools/NTSD28Parity/Program.cs
authority: NTSD 2.8-Logan ScenarioLoader28 seven-action input plus GameSession28 completed-tick NativeRandom28 CRT/synchronized state and BattleWorld28 physical slot order; Unity FrameInputSet/DeterministicRng/RuntimeSlotTable are current diagnostic sources, not presumed equivalent streams.
evidence: CONTRACT-FIRST / SOURCE-INVENTORY-CLOSED / CONTRACT-SHA-7185B5D2EFFA6F51D672BADE14FF49D2105A7B4BB7E4DC3D99D83C392DCC68AD / RELEASE-BUILD-0-WARN-0-ERROR / DOMAIN-SELFTEST-12-OF-12 / EXISTING-SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / GLOBAL-LEDGER-88-RECORDS-16-FILES-PASS / SCOPED-DIFF-CHECK-PASS / PRODUCTION-UNCHANGED / AUTHORITY-DIRECTORY-READ-ONLY / EXPORTERS-DEFERRED
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / DOMAIN-SELFTEST-12-OF-12 / PRODUCTION-UNCHANGED`

本包建立独立 raw-domain schema/validator/self-test；不接 exporter，不改战斗生产逻辑。

## 实施前事实

- 权威 source-model可给出七动作input、CRT state/calls和synchronized counter/index/calls/
  lastCallSite/source-native 64-bit table hash，但当前capture没有per-call日志。
- Unity `FrameInputSet`可给出逐slot held mask，`DeterministicRng`当前只有单stream state/call count；
  `RuntimeSlotTable.ReadOnlySlotView`可给出occupant/generation/allocationEpoch。
- 两侧stream不能按名称或便利性直接合并；缺失必须显式保留。

## 验收结果

- contract SHA `7185B5D2EFFA6F51D672BADE14FF49D2105A7B4BB7E4DC3D99D83C392DCC68AD`。
- Release build 0/0；专用12/12、既有21/21、raw5/5、format PASS。
- 首次专用运行因恶意slot乱序fixture直接重挂已有parent的JsonNode异常；改用DeepClone后全绿。
- validator强制producer stream matrix、input七位mask、RNG total delta、slot升序/epoch与snapshot-derived
  birth/death/reuse，并拒绝同epoch换occupant。
- Ledger 88 records / 16 governed files PASS；scoped diff check PASS。
- 没有Unity程序集改动，故未运行Unity compile/test；exporter、非空输入和真实双端raw仍待后续包。
