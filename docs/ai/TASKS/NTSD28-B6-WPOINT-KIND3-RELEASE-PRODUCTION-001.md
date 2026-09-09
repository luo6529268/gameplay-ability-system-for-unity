# NTSD28-B6-WPOINT-KIND3-RELEASE-PRODUCTION-001 — Task Contract

> Goal11 / 2026-09-09 / PLANNED / TEST_FIRST_PRODUCTION，任何脚本修改前建立。

用户明确授权沿用candidate/owner correction并补Goal10新边界。Authority battle_world.cpp settle_held_refill_objects:8020-8077，正式EXE B1E13AE1、playable closure39DDDA15。
C09/C20分别由simulation_tick_driver.cpp:682/908调用；回链NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001。
旧owner audit中BattleRandInt为同步流的说法已由Goal10源链证伪：它是world.Rng LCG；本包只局部使用world.NativeRandom，不修改全局入口。

## 精确授权文件
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs：仅RunStep12 continuation、kind3 type2 prefix、DropRandomly WPoint/NativeRandom/final writes。
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs：仅Act中kind3 type2 prefix及ProcessDrinkConsumption末尾标记refill exhaustion。
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs：仅WeaponActResult增加非持久化RefillExhausted bool。
- Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointKind3ReleaseProductionEditorTests.cs与同名.meta：新focused矩阵及同文件内scoped Play probe/请求入口/取证，只用于测试。
- Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs：仅BATTLE-AUDIT3-12具名real damaged-kind3断言Vz -0.4..0.4改-2..2。
- 本Task/Record、Ledger、STATE、对齐总表、Temp产物。禁止其他文件/Scene/Prefab/Config/资源/importer修改。
新增脚本.meta只标识新测试脚本，Unity生成不修改其他importer。

## 精确生产合同与不变量
1. active negative reciprocal relation与当前pose/refill前置仍由原owner处理；refill exhaustion独立RefillExhausted outcome阻断后续kind3，其他ForceDrop继续原行为。
2. DVX actual/generic完成后若Kind3继续；非kind3 return保持。
3. type2 DVX且Kind3 prefix由同world.NativeRandom以0x0041865E/6抽一次；非kind3 type2仍用原BattleRandInt。
4. kind3先清双方active relation，再无条件NativeRandom四draw：0x00418726/6、0x0041873A/7、0x00418756/4、0x00418772/5。
5. final action为第1draw；X/Y/Z authored非零优先，否则draw分别-3/取负/-2。authored X不按facing反转；Z无0.2缩放。
6. 普通kind3不Free、不释放generation、不清历史TargetSlot/HolderSlot/catch/owner字段；现有compat缓存清理/ReleaseTick/Zz保持。
7. 不修改耗尽kick的旧RNG、damaged-drop legacy前置RNG、OnThrown weaponHP副作用、+2F8、terminal/missing-action、nonkind3 DVX、schema、C25恢复。
8. 无新runtime owner/queue/worker，RefillExhausted是调用栈outcome不入snapshot/checksum；shutdown顶层顺序不变。Play probe只在请求期间运行，finally移除observer并unregister自建实体，随后正常有序退出Play，不创建替代manager。

## Test-first与回归
新focused走真实HeldObjectProcessAll/原writer，包括real/generic ×1/2/4/6 ×左右 ×zero/single-axis/mixed。
验证精确NativeRandom callsite/bounds/results/count、旧Rng不变（损伤/耗尽已有prefix除外）、DVX→kind3终值、双held pass只释放一次、不free/generation不变、slot0/399与排除字段哨兵。
refill OID122/123未耗尽可继续kind3，耗尽只旧kick一次且native0，原耗尽结果保持；damaged ForceDrop仍legacy前置后kind3，不误判耗尽。
先跑实际RED再改production，GREEN后完整B6分类、held-refill7+C09placement2=9、既有NativeRandom/关系/slot相关回归、full SelfCheck PASS；两套build0 error、validator通过。
既有RNG断言任何失败或不属本包的回归失败立即停；禁止为绿色扩大文件清单/全局RNG/后置族。

## scoped Play与现场
指定instance gameplay-ability-system-for-unity@b1b02287，2022.3.62f3/NTSD_Battle；不第二实例。
使用当前Rock Lee OID7数据（standing hit_Da255；action255 WeaponAct40/Kind3/Dvx100/Dvy-1/Dvz0），在实际Play world建立scoped角色与真实weapon，通过现有拾取生产入口建立held关系，再由真实driver逐tick推进255释放；记录输入/动作触发方式，不把直接writer测试冒充自然物理按键。
probe需要证明真实关系已建立、C09/C20释放只一次、逐tickNativeRandom次数/值、authored覆盖与最终motion，并可区分释放瞬间与后续physics更新。
优先沿已存在输入/动作入口触发255，必要的纯测试注入写入报告；不改DAT或伪造production行为。
现场observer与实体有明确归属，报告cleanup数量；无关ambient spawn单列且不冒充probe残留；退出Console0 error、场景dirtyfalse、Scene SHA保持D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。
未具备Play证据不能VERIFIED；硬停止按用户原文，不自动降级Play。
完成后GOAL12_USER_HOLD，terminal不得启动。

## 回滚
需要用户明确批准，仅反向本包local branches/NativeRandom/outcome/唯一Vz期望及新test/governance增量；不回滚前置B6refill/lifecycle或用户既有内容，无schema迁移/外部发布。


最终状态（2026-09-10）：VERIFIED_KIND3_SUBSET；以同ID Record最终验收为准。事前PLANNED事实保留。Goal12 USER_HOLD，等待复核。
