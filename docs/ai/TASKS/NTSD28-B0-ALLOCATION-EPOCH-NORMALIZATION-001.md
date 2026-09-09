# Task Contract — NTSD28-B0-ALLOCATION-EPOCH-NORMALIZATION-001

> 状态：`FOCUSED_TEST_PASS / CPP-BUILD-0-0 / REAL-DIFFERENCE-CLOSED / AUTHORITY-GUARD-PASS`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-02

## 目标

让 authority source raw writer 遵守既有 `allocationEpoch` contract：该值是每个 runtime slot 自己的
正生命周期 epoch，不是跨槽位全局 `presentation_generation`。writer 需为1000个槽保存上次观察的
presentation generation；同槽首次实体输出1，后续观察到新 generation时递增，空槽不丢失历史epoch。

## 允许文件

- `Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp`
- 必要的 `Tools/NTSD28Parity/TraceContractSelfTest.cs`（只允许增加epoch语义负例/正例）
- 本 Task、同 ID Change、Ledger、STATE、handoff、总表

## 不变量

- 不修改 authority 源码或目录；只修改workspace-owned diagnostic writer并在Temp重建。
- 不修改 Unity slot/epoch实现，不把全局 token 硬塞给 Unity。
- 每个槽epoch必须从1开始，只在同槽实体 presentation generation 变化时递增；绝对 slot capacity
  仍按用户批准不要求两侧相等。
- certificate保持false；formal EXE/source/binary identity继续分离。

## 验收

- C++ build 0 warning / 0 error；authority output guard通过。
- authority raw validator、工具build、总self-test、raw self-test通过。
- 公共authority raw slot0/slot1初始epoch均1；真实比较allocationEpoch差异消失，unique
  differences17→16、equal30→31、occurrences99→96。
- Ledger与diff check通过。

## 当前证据

- writer维护1000槽的observed/presentationGeneration/epoch；每槽首次1，仅generation变化递增。
- C++ rebuild 0 warning / 0 error；runner source SHA `8018CB80...8BEE4`，binary SHA
  `4FD64BC9...82DD1`；authority source manifest保持 `C59BD8D3...F2D75`。
- 公共capture exit0、validator valid 3 ticks/6 entities；slot0/1初始epoch均1；raw SHA
  `9AFFE2F61C52A358900F2FE53642B99D10FC7C6BCE69F25752F8C7CE50475E3C`。
- 真实比较：allocationEpoch差异消失；unique17→16、equal30→31、occurrences99→96，首差异现为
  `identity.controlSlot`。
- authority目录输出guard exit1；工具build0/0、self-test21/21、raw self-test5/5。
- Ledger73 records/14 governed code files/PASS；diff check无error。
