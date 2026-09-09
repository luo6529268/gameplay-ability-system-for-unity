# Task Contract — NTSD28-B0-REVIVAL-FIELD-BINDINGS-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / AUTHORITY-RAW-VALID / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-DIFFERENCES-CLOSED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

原子更正三项复活字段：`reviveLives→HP2Orig`、`reviveNextLives→HPOrig`、
`reviveNextHp→RespawnCount`。旧 diagnostic 的 `reviveLives→RespawnCount` candidate 被 neutral
场景硬编码 `RespawnCount=1` 掩盖，必须与两个 MISSING 字段一起修正，不能留下双占用。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Tools/NTSD28AuthorityTrace/Scenarios/neutral-common-two-entity.json`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只修 diagnostic/scenario adapter；不改 `BattleRespawnModule` 或任何 production revival writer。
- common scenario 显式写 `reviveLives30c=1/reviveNextLives310=0/reviveNextHp314=0`，随后重跑
  workspace authority source capture；不得写 authority 目录。
- maturity 从 28 verified / 10 candidate / 9 missing 变为 31 / 9 / 7。
- distinct focused 值必须投影为 current lives=4、next lives=7、next HP=320，证明无交叉占用。
- 工具 build 0/0、21/21、5/5、format；authority raw 3 ticks/6 entities valid；Unity compile 0、联合6/6。
- 真实 raw 目标 unique 11→9、equal36→38、occurrence66→54；baseMaxMp 仍保持首差异。
- Ledger validator 与 scoped diff check 通过。

## 回滚

恢复旧 candidate/missing、projection null、exporter hardcoded `RespawnCount=1` 和场景隐式默认；
恢复 maturity28/10/9。不修改 production revival 行为。

## 实际结果

- contract/projection 已原子绑定 HP2Orig / HPOrig / RespawnCount，maturity31/9/7；contract SHA
  `3899F5E5DF9E28D9E69284D73ECAF9A8B1EB6C1E7E4FEC41B1B520D068F8213B`。
- common scenario 已显式冻结复活三字段1/0/0；workspace authority capture重跑exit0、3 ticks/6 entities
  valid、certificate false，raw SHA保持 `9AFFE2F6...0475E3C`，authority目录零写入。
- 工具 build0/0、21/21、5/5、format PASS；Unity fresh compile0；focused job
  `34e6199d932240f797443549ee5d468c` 6/6 PASS，distinct 4/7/320投影通过。
- Unity raw SHA `97617669135904CA18177BEE3F9A99CA195B3C7182E4407D11DA30B02ED37B7D`；
  真实差异11→9、equal36→38、occurrence66→54，首差异仍为 `vitals.baseMaxMp`。
- production `BattleRespawnModule`、DAT、资源、Scene 与 authority 源码均未改。
- Change Ledger validator 79 records / 14 governed code files PASS；scoped diff check无错误，仅既有换行提示。
