# NTSD28-B0-REVIVAL-FIELD-BINDINGS-001 — revival triplet diagnostic correction

<!-- CHANGE-RECORD
id: NTSD28-B0-REVIVAL-FIELD-BINDINGS-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 revive_lives_30c/revive_next_lives_310/revive_next_hp_314 and run_revival_pass; Unity BattleRespawnModule HP2Orig/HPOrig/RespawnCount branches; B0 scenario/projection.
evidence: THREE-FIELD-SOURCE-CHAIN-CLOSED / OLD-RESPAWNCOUNT-AS-CURRENT-LIVES-BINDING-CORRECTED / BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / AUTHORITY-RAW-VALID-3-TICKS-6-ENTITIES / UNITY-COMPILE-0 / JOINT-6-OF-6 / DISTINCT-4-7-320-PROJECTION / REAL-TWO-MISSING-DIFFERENCES-CLOSED / MATURITY-31-9-7 / GLOBAL-LEDGER-79-RECORDS-14-FILES-PASS / SCOPED-DIFF-CHECK-PASS / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / AUTHORITY-RAW-VALID / UNITY-COMPILE-0 / JOINT-6-OF-6`

## 改前事实

- authority normal revival decrements `revive_lives_30c`；Unity normal branch checks/decrements `HP2Orig`。
- authority queued continuation assigns current lives from `revive_next_lives_310` and clears it；Unity queued branch
  assigns `HP2Orig=HPOrig` and clears `HPOrig`。
- authority queued continuation sets current/base/effective HP from `revive_next_hp_314` and clears it；Unity queued
  branch sets HP/HP3/HPBound from `RespawnCount` and clears `RespawnCount`。
- 旧 raw projection 却把 `reviveLives` 指向 `RespawnCount`，exporter 又硬编码 `RespawnCount=1`，导致 neutral
  baseline 假相等；两个 next 字段保持 null。

## 计划改动

- contract/projection 原子更正为 HP2Orig / HPOrig / RespawnCount，并将三项全部晋级 VERIFIED。
- exporter DTO 读取 scenario 的三项原生字段并写入对应 Unity runtime；移除 hardcoded `RespawnCount=1`。
- common scenario 显式冻结 1/0/0，并重跑 workspace authority/Unity raw。
- focused test 以4/7/320证明三项分离；不改 production revival 逻辑。

## 验收与回滚

- 验收标准见对应 Task Contract。
- 回滚只恢复 diagnostic/scenario adapter 与 maturity28/10/9。

## 实际结果

- 三字段现均为 VERIFIED：current lives=HP2Orig、next lives=HPOrig、next HP=RespawnCount；maturity31/9/7。
- contract SHA `3899F5E5DF9E28D9E69284D73ECAF9A8B1EB6C1E7E4FEC41B1B520D068F8213B`；
  工具build0/0、21/21、5/5、format PASS。
- common scenario显式1/0/0后，workspace authority capture exit0，validator为3 ticks/6 entities valid，
  raw SHA仍为 `9AFFE2F61C52A358900F2FE53642B99D10FC7C6BCE69F25752F8C7CE50475E3C`。
- Unity compile0；job `34e6199d932240f797443549ee5d468c` 6/6 PASS，4/7/320证明三字段无交叉。
- Unity raw SHA `97617669135904CA18177BEE3F9A99CA195B3C7182E4407D11DA30B02ED37B7D`；
  comparison为3 ticks/6 pairs/282 fields、9 unique differences、38 equal、54 occurrences。
- production revival、DAT、资源、Scene、authority源码和正式EXE均未改。
- Ledger validator 79 records / 14 governed code files PASS；scoped diff check PASS。
