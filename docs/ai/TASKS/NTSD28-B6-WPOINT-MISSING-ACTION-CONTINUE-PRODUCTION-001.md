# NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-PRODUCTION-001 — Task Contract

Goal13 包1 / IN_PROGRESS / TEST_FIRST。用户明确授权串行两包，此包须focused及Play通过后才能启动包2。

Authority：当前正式B1E13AE1 EXE对应playable BattleWorld28::settle_held_refill_objects，battle_world.cpp:7971-7994。literal action先写，missing child frame诊断continue，declared frame无文本WPoint使用全零record；随后才写facing/hold/pose及DVX/kind3。refill耗尽先于terminal，terminal先于action。
Unity原状：real旧current-frame-null gate阻止refill及恢复；写missing action后仍写pose及投掷；generic action/pose耦合。只能移除上述多余副作用，不修改terminal/kind3已闭合算法。

## 写入清单
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/Interaction/SimulationQueryAndLinkModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointMissingActionContinueProductionEditorTests.cs`
- 本Task/同ID Record、CHANGE-LEDGER.md、STATE.md、对齐总表及Temp产物；新test自动meta。

## 不变量、验证、回滚
瞬态UnsupportedWeaponAction不进入snapshot/checksum。耗尽→terminal→literal action→missing continue→pose/DVX/kind3。missing不Free/unlink，不改facing/hold/pose/motion/HP/ReleaseTick、不抽样；有效无文本WPoint仍正常。
focused先RED：real/generic正值/-888、current missing、missing→valid、无文本WPoint、refill current missing、type1/2/4/6 DVX和kind3、下一slot及C09/C20。Play用current Sakura/Sakon及真实driver，明确动作夹具方式。
两包最后共享B6、refill9、Goal11 92、Goal12 80、fullSelfCheck、双build0error、validator、SceneSHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。instance b1b02287 / 2022.3.62f3 / NTSD_Battle，无第二Editor。
任何既有测试失败、需改terminal/kind3语义/删除callback、误Free/unlink、清单外diff或Scene变化立即停。generic nonkind3 heavy RNG、+2F8/schema全部排除。
无新queue/manager/lifecycle owner；瞬态结果栈内消费，observer只属于test，finally解除并清理owned entities。遵守十一阶段关闭。
回滚必须用户批准，仅反向本包备份对应增量与新增文件，不回退Goal1-12/用户工作。不可回退边界为既有已验收terminal/kind3/refill。

最终停点：BLOCKED / Goal13硬停止。focused72通过，但Sakon-888在C09/C20保留后遭后续C25 frame exit Free；只读定位见Record。包2未启动，共享回归未运行，不自行扩大到C25。

Goal13b最终状态：VERIFIED_MISSING_ACTION_SUBSET / DEPENDS_ON_HELD_LIFECYCLE_GUARD / RED_COMPLETED72_FAILED_CAPPED25 / FOCUSED72_PASS / DUAL_PLAY_TWO_RUNS_PASS / B6_388_PASS / SELFCHECK_PASS / BUILDS0 / SCENE_UNCHANGED。详细证据以同ID Record最终共享验收为准；原BLOCKED/PLANNED等历史事实不删除。GOAL14_USER_HOLD。
