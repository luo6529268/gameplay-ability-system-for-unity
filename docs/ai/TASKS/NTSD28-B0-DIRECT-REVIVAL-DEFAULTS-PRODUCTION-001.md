# Task Contract — NTSD28-B0-DIRECT-REVIVAL-DEFAULTS-PRODUCTION-001

> 状态：`VERIFIED / RED_1_OF_3 / DIRECT_DEFAULTS_1_0_0 / FOCUSED_3_OF_3 / DIRECT_OWNER_REGRESSION_15_OF_15 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_RUNTIME_RESUMABLE`
> 依赖：`NTSD28-B4-REVIVAL-PARTICIPANT-GATE-OWNER-AUDIT-001 / VERIFIED`

## 目标

在direct participant首次注册快照前写入Authority默认revival字段：`HP2Orig=1`、`HPOrig=0`、
`RespawnCount=0`，关闭B4 gate runtime的直接玩家producer前置。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Runtime/BattleMatchConfigRuntimeAdapter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B0DirectRevivalDefaultsProductionEditorTests.cs`及`.meta`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本Task/Change、Ledger、STATE、handoff与总表。

## 不变量

- 仅`PrepareDirectParticipantRegistration(entity, physicalSlot)`成功路径写默认1/0/0；invalid/null不写。
- 不新增PlayerSlotConfig/MatchConfig/UI/Scene字段，不改变owner/self slot、team/input、HP/MP或注册顺序。
- 不写stage/OPoint/results-reserve；不改B4 consumer、content、Prefab、ProjectSettings、Authority或schema版本。

## 验收

RED覆盖pool-reset sentinel与slot0/19/invalid20/null；GREEN验证在ModuleBind/Initialize/Register后active runtime和raw
backing边界。运行build、focused、相关direct owner回归、SelfCheck、targeted Play、Console/Scene与Ledger validator。

## 回滚

移除adapter成功路径三项默认写入及本Change测试；不得回退B0 owner producer或其他direct participant逻辑。

## 实施与验证（2026-09-09）

- test-first以slot0/19先写陈旧77/88/99，并要求准备后、ModuleBind/Initialize/Register后及首次active snapshot
  均为1/0/0；invalid20/null必须不写。02:51:07 +08 RED为`1/3`：invalid通过，两个valid均
  `HP2Orig expected1 / actual77`。
- production只在`PrepareDirectParticipantRegistration()`通过null/range gate后、与required/self owner相邻写
  `HP2Orig=1`、`HPOrig=0`、`RespawnCount=0`；invalid分支与其他调用链未改。
- fresh builds为runtime 0 error/47 warning、Editor 0 error/104 warning；02:52:22 +08 focused `3/3`，
  02:53:13 +08既有direct owner producer回归`15/15`。
- 02:53:56 +08真实`NTSD_Battle` Play通过3用例：slot0/19 active entity defaults=1/0/0，独立raw backing
  保持0/0/0，invalid不写。Console error=0；active Scene dirty=false/root=13且SHA-256保持
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- 02:54:39 +08 full SelfCheck仍更早停在独立CPoint mode0 victim-Vz断言，未到新增direct断言；不声称通过。
- 本包不提供自定义revival值、不处理stage/OPoint/results或consumer。B4 gate runtime前置现已关闭，严格恢复
  `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001`。
