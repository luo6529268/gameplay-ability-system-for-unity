# Task Contract — NTSD28-B5-NONCHAR-UNARMORED-DAMAGE-SCALE-CONSUMER-001

> 状态：`VERIFIED / TYPE1_5_UNARMORED_SCALE_WEAK_ALIGNED / TYPE6_SKIP_VERIFIED`
> 依赖：type0 scale consumer、nonchar owner audit均`VERIFIED`

## 目标

让weapon normal与type3/type5 normal unarmored HP/stat写入复用已验证的2.8
`IncomingDamageScale340 -> attack-effect source WeakTimer12C/2` resolver，并同步hit-plan projection。

## Authority 合同

- type1/2/3/4/5进入unarmored damage block；type6跳过HP/resource block。
- effective injury只进入HP/HPBound/combo/damage stats；weapon durability、display step、status producer仍
  读取raw ITR injury及各自字段。
- `FallDamageDiv`不得与`+340`叠加或作为本路径fallback；它不是当前2.8 damage truth。
- attacker weakness读取physical attack-effect source，不读取owner-resolved resource attacker。

## 边界

- 不移除`FallDamageDiv` carrier；其Results/cpoint/environment/alternate/kind16 owners分包迁移。
- 不补stats.defend/mode/child producer，不处理armor/effect/resource/audio/spark。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5NonCharUnarmoredDamageScaleEditorTests.cs`
- 必要的旧fixture/SelfCheck期望更正。

## 验收

test-first compile red；type1/2/4 weapon、type3/5 special-other、type6 skip、raw durability/display隔离、
conflicting FallDamageDiv不参与、hit-plan shadow一致、zero allocation；相关hit、精确NTSD28 broad、
SelfCheck、Scene/Console/Ledger。

test-first focused：6项中type6 skip通过；type1/2/4与type3/5共5项按预期失败，实际值仍证明
weapon使用旧`FallDamageDiv`、type3/5使用raw injury。

首次SelfCheck在R4-HIT-03旧标准武器fixture失败：fixture只写`FallDamageDiv`却期望缩放。仅将
type1/2/4 standard、lethal type2及bdefend type4夹具改写为`IncomingDamageScale340`；type6的
conflicting legacy值保留用于证明skip，其他未迁移owner夹具不动。

## 回滚

恢复weapon/type3 raw/FallDamageDiv damage写入与旧projection，移除focused test并恢复被更正夹具。

## 验证结论

- fresh compile 0 error；test-first 5 red/type6 green。
- focused `d3be6225e18d4e25929c3f823a6e0337`：6/6。
- 全B5+完整hit-plan related `fa60f06fb8e1456489915d6d0e6eb555`：290/290。
- 精确95-class NTSD28 broad `27dcfed6cd3a4adb9f138ebfce02b859`：670/670。
- 首次SelfCheck捕获R4-HIT-03旧fixture；定向更正后`2026-09-05T14:37:47Z` PASS。
- 清理预期negative-path日志后Console 0 error；Scene SHA
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、203477 bytes、
  mtime unchanged；Change Ledger 258 records/224 governed code files PASS。
- Results/cpoint/environment/alternate/kind16的`FallDamageDiv` owner仍未迁移。
