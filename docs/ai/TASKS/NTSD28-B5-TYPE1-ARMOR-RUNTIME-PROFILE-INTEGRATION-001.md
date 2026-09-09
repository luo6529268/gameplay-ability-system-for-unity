# Task Contract — NTSD28-B5-TYPE1-ARMOR-RUNTIME-PROFILE-INTEGRATION-001

> 状态：`VERIFIED / PRODUCTION_PROFILE_CONNECTED / HIT_SELECTION_UNCONNECTED`
> 依赖：`NTSD28-B5-TYPE1-ARMOR-RUNTIME-INITIALIZATION-AUDIT-001 / VERIFIED`

## 目标

接通首armor block的runtime profile读取、角色出生/reuse初始化与C25i现有恢复owner；snapshot shell显式跳过
definition初始化，保留随后恢复的snapshot runtime字段。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
- `Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorRuntimeProfileIntegrationEditorTests.cs` 与 `.meta`
- 本Task/Change、Ledger、STATE、handoff、总表与armor manifest

## 不变量

- 只读取当前definition的首armor；无block或首项null返回无profile。
- 普通ModuleBind出生/reuse写`RuntimeArmorHp118=hp`、`ArmorRecoveryTimer11C=recover>0?recover:-1`；
  无profile重置为`0/-1`，不得保留池中stale值。
- snapshot shell调用ModuleBind时必须跳过definition初始化；snapshot restore仍是runtime真值owner。
- C25i顺序、body/relation/hold gate和kernel公式不变，只把原固定false profile seam接到typed data。
- 不接type1 selection/damage/break transaction，不部署content/Scene。

## 验收

test-first覆盖首block、无profile、recover 0、reuse stale clear、snapshot-skip、C25i production reload与warm
zero-allocation；随后compile、focused、B5、NTSD28 broad、SelfCheck、Console、Scene与Ledger。

## 验收结果

- test-first red：`fd23ee46ae4a4eb18c7809654e04ef4e`，7/7按预期失败。
- focused：`3f842c7c401e467a88a91b498f8a6f3c`，8/8通过，含C25i production与warm zero-allocation。
- B5：`0f91f25c5de84c3994de90eab04611e0`，324/324通过。
- NTSD28 broad：`366b59e0a0a3408cb6562b2a0b423228`，800/800通过。
- SelfCheck：2026-09-06 01:06:53Z `PASS`；Console仅7条既有rest-binding故意失败日志，无C#编译错误。
- Scene SHA-256/length/mtime保持
  `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B` / 205625 /
  `2026-09-05T15:44:52.7794120Z`。
- selection/damage/break/content仍未接。
