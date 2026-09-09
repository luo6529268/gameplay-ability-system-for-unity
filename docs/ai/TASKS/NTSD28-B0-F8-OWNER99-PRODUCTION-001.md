# Task Contract — NTSD28-B0-F8-OWNER99-PRODUCTION-001

> 状态：`FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_3_OF_3 / SELFCHECK_REACHED_UNRELATED_CPOINT_AFTER_NEW_CHECK / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_3 / MODE2_MATERIALIZER_OWNER_ONLY / PHYSICAL_F8_EFFECT_WIRING_EXCLUDED`

## 目标

在既有 Unity `Mode2Request==1 -> BattleRandomWeaponDropModule.SpawnMode2RandomWeapons()`
materializer 中，于 `CreateObjectImmediate` 前把 `OPointCreateTask.ownerEntityIndex` 精确写为 `99`，使
route 2 已闭合的实体 initializer 在首次注册前发布 Authority F8 drop owner。

## Authority 与当前原状

- 当前正式 EXE SHA-256：
  `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；playable closure
  manifest：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- playable build closure 中的 `source/ntsd28_playable/src/game_session.cpp`：
  `GameSession28::step()`在战斗 tick 后消费 `drop_objects`，调用`consume_native_f8_drop()`；后者创建
  `NativeFunctionKeyDropSpawn28`，其 header 默认`owner_slot=99`，并把该值原样写入
  `SpawnRequest28.owner_slot`后`spawn_at`。
- Unity正式物理F8目前只写`FunctionKeys.PendingObjectCommand`，尚未生产消费；既有可执行
  materializer来自legacy diagnostic `Mode2Request==1`，task未写owner，故当前生成实体owner为`-1`。
- route 2 `NTSD28-B0-DIRECT-ENTITY-SELF-OWNER-PRODUCTION-001` focused已`15/15`通过，确认显式task
  owner会在首次注册前进入active entity runtime；独立raw-slot backing按设计保持`-1`。

## 修改范围

- `Assets/NTSD/Scripts/Simulation/Passes/RandomWeapon/BattleRandomWeaponDropModule.cs`
  - 只在`SpawnMode2RandomWeapons()`的task构造中、factory调用前写`ownerEntityIndex=99`。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B0F8Owner99ProductionEditorTests.cs`
  - production mode2 tail覆盖lowest-free slot50、occupied-prefix high slot、owner first claimed
    active-slot runtime/raw-trace projection、raw backing sentinel、frame/位置/RNG及normal-drop owner不变。
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
  - 将相同最小owner与normal-drop回归合同并入全量自检；现有更早CPoint blocker如实保留。

## 不变量与排除

- 不连接`FunctionKeys.PendingObjectCommand`到mode2 materializer；正式物理F8 effect wiring仍归B8。
- 不修改候选范围/顺序、OID122 gate、四次位置RNG、stage坐标、frame选择、HP/PP、slot容量或
  lowest-free allocation；`requiredRuntimeSlot`继续保持`-1`。
- `RunNormalDrop()`的task保持owner默认`-1`；不创建共享“random weapon owner”抽象。
- 不增加注册后owner fix-up；不修改普通OPoint、state9996、weapon-piece、hit_Fa8/9/13、content、
  Scene、Prefab、ProjectSettings、pass order或shutdown。

## 验收

- RED先证明mode2 slot50与high slot的active entity owner当前为`-1`。
- GREEN后相同production入口中owner精确为`99`；claimed view指向同一entity且entity runtime为99，
  独立raw backing owner仍为`-1`。
- 相同seed/candidate/stage下，mode2四次RNG、selected frame与位置保持不变；normal drop仍owner`-1`且
  既有六次RNG/位置结果不变。
- runtime/editor compile 0 error；focused Unity测试实际通过；SelfCheck、Play、joint trace状态如实记录。

## 回滚

只移除mode2 task的单字段写入与本包测试/记录；不得回滚route 1 target deconfliction、route 2 initializer、
用户现有工作树或其他随机掉武器实现。
