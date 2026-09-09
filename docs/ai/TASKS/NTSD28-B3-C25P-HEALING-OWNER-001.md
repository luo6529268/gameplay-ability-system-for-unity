# Task Contract — NTSD28-B3-C25P-HEALING-OWNER-001

> 状态：`VERIFIED / PER_SLOT_HEALING_OWNER / GLOBAL_DUPLICATE_REMOVED`
> 依赖：`NTSD28-B3-C25K-P-OWNER-AUDIT-001 / VERIFIED`

## 目标

将当前近似正确但位于全局`EntityPostFrameTailAll`的healing writer迁入`BattleLateEntityLifecycleModule`的每槽survivor尾部，使C25p跟随C25o式active-survivor边界，并移除global post-tail中的重复owner。

## Authority

- `simulation_tick_driver.cpp`：`resolve_pending_lifecycle(slot)`消费后continue，否则`advance_native_healing_slot(slot)`。
- `battle_world.cpp::advance_native_healing_slot`：仅active、非pending、type0、HP>0；encoded timer、ordinary timer、state1700 arm顺序固定。
- formal EXE SHA：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。
- playable closure：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。

## 修改范围

- `Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterPostFrameTailPass.cs`
- 新增focused Editor test及必要的既有post-tail fixture语义更新
- 同步Ledger/STATE/handoff/总表/manifest

## 不变量

- current DAT非type0或HP<=0不推进任何healing timer。
- encoded与ordinary timer顺序、每8 tick恢复8、上限与整千完成语义不变。
- state1700最后写HealTimer=1100。
- global post-tail继续执行F7、carrier/transient清理与snapshot，但不再写HP/HealTimer/CatchTimer。
- 不修改C25k-o、random-drop例外、Config/DAT、资源、Scene、Prefab、Input或Authority。

## 验收

- red test证明旧global owner和缺失逐槽kernel/placement。
- focused C25p、post-tail/ECS相关测试通过。
- `.*NTSD28.*` broad通过；Unity compile 0；SelfCheck PASS；Console error 0。
- Scene hash/length/mtime不变；Change Ledger validator通过。

## 回滚

仅反向应用本Change ID列出的精确diff；不得用Git restore/reset覆盖其他用户修改。
