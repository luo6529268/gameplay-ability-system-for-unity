# NTSD28-B6-WPOINT-DVX-WEAPON-HP-PRESERVATION-PRODUCTION-001 — Task Contract

Goal13b第3步，用户复用原Goal13包2授权。IN_PROGRESS / TEST_FIRST，先记录后改脚本。
Authority当前正式B1E13AE1 EXE对应playable BattleWorld28::settle_held_refill_objects battle_world.cpp:8025-8063 DVX分支只写frame/motion/relation/+2F8，不写weapon_hp。Unity ThrowHeldWeapon→OnThrownInternal→LF2Weapon.OnThrown多写definition weapon_hp。

## 范围与不变量
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs仅ThrowHeldWeapon末尾：non-kind3跳过OnThrownInternal，kind3仍调用；共享API及LF2Weapon.OnThrown不删不改。
- Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointDvxWeaponHpPreservationProductionEditorTests.cs及meta，新focused与scoped Play。
- 本Task/同ID Record、Ledger/STATE/对齐总表、Temp产物。
真实type1/2/4/6 nonkind3投掷保留损伤sentinel；RNG/frame/motion/relation/ReleaseTick与原来一致。generic本来无callback保持原样，heavy type2 RNG迁移不属本包。kind3 overlap仍重置definition HP并保留既有native prefix/tail。refill耗尽仍优先。
不触碰+2F8 carrier/writer/consumer、联合schema、Scene/InputActions、资源、terminal/kind3算法、C25守卫或旧tests。

## test-first / 验收
RED real四type×左右×nonkind3/kind3 overlap；sentinel7不同于definition31，断言保留/重置分别正确且callback次数、精确RNG和frame/motion/relation/ReleaseTick不变。generic四type为控制，refill122/123耗尽/未耗尽为控制。
scoped Play current-DAT weapon先经生产damage writer损伤，再生产pickup consumer拾取，再真实driver触发当前nonkind3 DVX动作；记录weaponHP三边界与清理。动作/碰撞夹具注入须如实说明，不宣称物理输入。
完成后三包共享一次：guard17、pkg1 72、pkg2、B6(275+全部新增)、refill9、kind3 92、terminal80、fullSelfCheck、双build0error、validator、SceneSHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。Editor b1b02287/2022.3.62f3/NTSD_Battle，禁止第二实例。

## 风险、生命周期、回滚
callback目前只重置WeaponFlightCounter，不能机械删API，kind3仍依赖；检查callback observer证明分支区别。无新runtime manager/queue，test finally回收owned handles/observer，普通有序退出。
任何旧测试/控制组失败、清单外diff、Scene变化、需改kind3/terminal或删除callback均立即停止。不扩范围。
回滚必须用户批准，仅本包备份增量和新增fixture/治理，不回退守卫或包1。

Goal13b最终状态：VERIFIED_NONKIND3_DVX_HP_SUBSET / RED8_FAIL_16_CONTROL_PASS / FOCUSED24_PASS / DAMAGE_PICKUP_THROW_PLAY_200_199_199_199 / KIND3_CALLBACK_RETAINED / B6_388_PASS / SELFCHECK_PASS / BUILDS0 / SCENE_UNCHANGED。详细证据以同ID Record最终共享验收为准；原BLOCKED/PLANNED等历史事实不删除。GOAL14_USER_HOLD。
