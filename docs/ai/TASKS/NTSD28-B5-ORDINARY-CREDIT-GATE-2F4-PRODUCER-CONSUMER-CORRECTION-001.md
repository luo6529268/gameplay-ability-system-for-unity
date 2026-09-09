# Task Contract — NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001

> 状态：`VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_287_OF_287 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_2F4_READERS_PRODUCERS / LEGACY_STATS_WRITER_NEXT`
> 依赖：`NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001 / VERIFIED`及其route1～4、
> `NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001 / VERIFIED`。

## 目标

将Unity仍误读/误写`KillCount`的Authority `Entity28+0x2F4 ordinary_credit_gate`行为迁移到已有
`NTSDEntityRuntime.OrdinaryCreditGate2F4`：standard/reduced/CPoint damage legacy-stat gate、legacy/ECS PP
threshold与两个OPoint materializer的type0 child传播必须只读写exact carrier。非type0 child不产生+0x2F4。

本包只纠正gate的所有权，不退休`ComboCount*`、`KillStat`、world `DamageStats/KillStats`等旧统计写入，
不删除`KillCount` carrier/schema；这些属于后继route6/7。

## Authority 与现状

- Authority `battle_world.cpp` normal/reduced damage在`6685/6706/7120/7141`、caught damage在`6123`
  只读target/caught `ordinary_credit_gate_2f4 == -1`。
- C25 resource helper在`2311..2317`用`+0x2F4==-1 ? 500 : 150`决定ordinary MP threshold。
- OPoint materializer在`7750..7756`只对type0 child写
  `parent.+2F4 > -1 ? parent.+2F4 : parent physical slot`；non-type0 child不写。
- Unity standard/reduced actual与HitPlan、CPoint writer、legacy/ECS recovery仍读`KillCount`；两个factory仍把
  parent `KillCount`写给type0和部分non-type0 child。exact carrier已经存在并进入reset/copy/checksum/parity。
- kind10/11 flute的`ComboCountAtk += 11`、其他legacy stats writer与state501 compatibility scan不在本包；
  前者归route6退休，后者已有B3 retirement且残余helper另行处理。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs`
- `Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs`
- `Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs`
- 新focused test及为exact sentinel更新所必需的既有tests/SelfCheck。
- 本Task/Change与治理恢复文档。

## 不变量与排除

- 不改伤害、HP/PP、fall、armor、rest、RNG、credit owner chain、pass order、对象slot分配、owner/group/holder。
- legacy entity/world stats的写入值与顺序暂时保留；仅其eligibility gate从错误carrier切到exact +0x2F4。
- type0 child只写exact +0x2F4；fallback必须用parent当前physical runtime slot，而非StableId、owner或HolderCopy。
- non-type0 child不写+0x2F4，也不再由OPoint factory写`KillCount`；其reset sentinel保持-1。
- 不改kind10/11 flute stats、state501 helper、carrier/store/snapshot/checksum/parity/schema、content、Scene、Prefab、
  ProjectSettings、Authority或用户例外。

## Test-first 验收

1. standard/reduced/CPoint各用`KillCount`与exact gate相反的sentinel，证明结果只随exact gate。
2. legacy与data-oriented PP recovery在相反sentinel下均按+0x2F4选择500/150 threshold。
3. logic OPoint覆盖parent gate -1/nonnegative、parent slot50、type0/non-type0及两跳；child exact值、legacy
   `KillCount` sentinel与owner/holder均精确。
4. source guard要求本包声明的15处KillCount误绑定归零，同时保留flute/state501/schema等排除项。
5. focused、damage/HitPlan/recovery/OPoint相关回归、两套build、full SelfCheck与真实`NTSD_Battle` Play；
   Console与Scene不变。

## 回滚

仅恢复本包reader/writer到旧`KillCount`并回退focused fixture；不得回退前置B2～B5修正或删除exact carrier。
