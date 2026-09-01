# Task Contract — CLIENT-FORMAL-KERNEL-REST-STATE-SEAM-001

> 状态：`FOCUSED_TEST_PASS / CUT_D_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / S0_NOT_VERIFIED`
> 创建：2026-08-30

## 1. 目标

不移动`RuntimeRestStore` source/GUID，只拆除它对Client checksum/snapshot类型的反向依赖。增加零分配、确定顺序的BCL traversal，把现有checksum与snapshot capture/restore adapter移到store外部，并保持所有既有行为和hash不变。

## 2. 允许文件

- `RuntimeRestStore.cs`；
- `BattleLockstepChecksumModule.cs`；
- `Lockstep/BattleWorldRestSnapshot.cs`；
- `Lockstep/BattleStateSnapshotRestore.cs`中既有rest adapter调用；
- 必要focused tests与`BattleRuntimeSelfCheck`。

## 3. 验收

- 消费57-line corpus及SHA-256 `E10CF6D96104F69F574AA73503AFF9F03C0AD85633E66AE02054A435D86434E8`；
- Authority400 hash、checksum schema4、dense/sparse snapshot/restore、StageSpawn lease、S0、lockstep、SelfCheck不变；
- dense/sparse traversal顺序确定，prepared sparse为`O(capacity + entries)`，warmed 0 B；
- `RuntimeRestStore.cs`不再引用Client checksum/snapshot types。

## 4. 禁止

禁止source/GUID move、package/version/manifest/lock/asmdef、battle rules、30Hz、RuntimeSlotTable、Registry allocation、Scene/资源/Input Actions、TargetTick/InputDelay、transport/Socket/数据库/公网、snapshot schema/recovery policy、formal AI、S1 wire与marker修改。

## 5. 回滚

只回退本seam的四个runtime文件与新focused test；保留全部既有治理、Cut C和用户改动。

## 6. 关闭证据

Unity compile0、seam5/5、related38/38（含S0 8/8与existing lockstep9/9）、extra lockstep21/21、fresh SelfCheck、Server Release/build/consumer/suites及双Ledger/Queue/matrix治理全部通过。该结果只关闭seam，不授权source move或S0晋升。
