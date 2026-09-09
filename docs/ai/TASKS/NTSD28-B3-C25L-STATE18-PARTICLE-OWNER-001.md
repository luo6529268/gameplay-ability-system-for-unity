# Task Contract — NTSD28-B3-C25L-STATE18-PARTICLE-OWNER-001

> 状态：`RUNTIME_PENDING / PROGRAMMATIC_ORDER_VERIFIED / RESOURCE_SPAWN_PENDING`
> 依赖：`NTSD28-B3-C25K-M-PREREQUISITE-AUDIT-001 / VERIFIED`

## 目标

从mixed `SpawnLateTransitionEffects`中拆出Authority C25l state18/19 branch，并在每槽frame-zero OPoint之后、cleanup与C25m previous-action commit之前执行和flush；保留state13/200兼容branch的原位置与虚拟tail覆写语义。

## 权威

- `simulation_tick_driver.cpp`：C25k后调用`materialize_state18_broken_weapon_particles`，随后才写previous action。
- `battle_world.cpp`：旧state18/19才适用；离开态请求7，持续态global-delay>0零RNG，否则sync bound4选择1；每粒分配成功前不消费tuple。
- EXE `B1E13AE1...9033`；playable closure `39DDDA15...6109`。

## 修改范围

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs`
- 新focused test
- `BattleEcsLateTailNoOpEditorTests.cs`中旧mixed-tail计数断言
- 治理文档

## 不变量

- state13/200 branch仍由原virtual tail边界执行；不移动N30 input trigger。
- C25m `MirrorLatePrevFrame`本包不移动。
- sustained state18仅在delay-clear时消费一次selection RNG；离开state18不消费selection RNG。
- OID999或slot不可用时不消费对应粒子tuple。
- 不修改C25k、terminal pending、C25n/o、DAT/资源、Scene、Input或Authority。

## 验收

red compile/focused；programmatic decision、RNG gate、placement与Prev commit；相关late tests；broad NTSD28；SelfCheck；Scene/Console/Ledger。
