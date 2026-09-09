# Task Contract — NTSD28-B6-NTSDSPEC-DEAD-FLUTE-API-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / DEAD_API_RETIREMENT_PACKAGE_DEFINED / IMPACT_SEPARATE / PRODUCTION_HELD`
> 来源：`NTSD28-B6-NTSDSPEC-PRODUCTION-OWNER-INVENTORY-AUDIT-001`、`NTSD28-B6-NATIVE-IMPACT-10111718-OWNER-AUDIT-001`

## 目标

冻结`LF2Entity.FluteForce()`及`LF2WeaponBase`空override的可达性和删除边界，确保移除旧mass lookup时不把
真正kind10/11/17/18 impact缺口误报为已修。本审计只读，不修改脚本、content、Scene或Authority。

## 已观察事实

- 全仓`FluteForce`只有两个声明：base public virtual与`LF2WeaponBase`空override；没有C#调用者。
- 全仓文本搜索Scene/Prefab/asset/test也没有序列化方法名引用；在repo closure内状态为
  `NO_PRODUCTION_OR_TEST_CALLER_OBSERVED`。公共API仍可能被仓库外程序集反射调用，因此删除时必须靠完整编译
  和运行时回归验证，不能把搜索结果扩大成全世界无caller。
- base方法读取`NTSDSpec.GetMassOrDefault(ObjectId)`，使用-140/-160/-180三段阈值、写`Effect.Super`、
  motion与character/heavy action；weapon override完全no-op。
- 当前actual kind10/11由character/weapon hit resolver与`BattleDamageWriter`直接实现，从不调用本API；
  Authority也没有`FluteForce`或mass分派。真实2.8 impact owner已由
  `NTSD28-B6-NATIVE-IMPACT-10111718-OWNER-AUDIT-001`独立冻结。
- 因此该方法是旧2.4/LF2_19兼容残留，不是可复用的2.8适配入口；把impact writer塞进此虚方法会重新按CLR
  shell分散规则，违反shared actual owner合同。

## production包

`NTSD28-B6-NTSDSPEC-DEAD-FLUTE-API-RETIREMENT-001`：

1. 删除`LF2Entity.FluteForce()`完整方法与旧注释；
2. 删除`LF2WeaponBase.FluteForce()`空override；
3. 确认`GetMassOrDefault(ObjectId)`调用归零，但不触碰character初始化mass包或EffectCreate oscillate包；
4. 添加architecture/reflection guard，禁止重新引入名为`FluteForce`的production virtual dispatch；
5. 不实现或委托impact行为；impact仍只由未来shared writer完成。

## 验收

- `rg '\bFluteForce\b' Assets/NTSD/Scripts`只允许本Change的负向architecture test描述，不得有production符号；
- `rg 'NTSDSpec.GetMassOrDefault'`只剩尚未执行的character mass包引用，两个包完成后为0；
- compile、API/reference scan、focused B6/impact、NTSD28、SelfCheck；real/generic character/weapon direct hit tests
  证明impact结果不依赖该虚方法；必要Play在impact atomic包统一执行。
- 无runtime证据前状态最多`RUNTIME_PENDING`。

## 不变量 / 阻塞

- 不改kind10/11/17/18 disposition、actual/HitPlan、motion、stats、NTSDSpec其他字段、content/Scene/Authority。
- 当前B6 runtime栈未清，production held；不重试已穷尽license路径。

## 回滚

本审计仅文档；回滚移除Task/Change/Ledger/STATE/handoff/总表增量。

