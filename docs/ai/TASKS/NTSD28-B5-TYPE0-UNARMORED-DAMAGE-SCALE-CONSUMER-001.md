# Task Contract — NTSD28-B5-TYPE0-UNARMORED-DAMAGE-SCALE-CONSUMER-001

> 状态：`VERIFIED / TYPE0_UNARMORED_SCALE_WEAK_ALIGNED`
> 依赖：`NTSD28-B5-DAMAGE-SCALE-EFFECT-AUDIT-001 / VERIFIED`

## 目标

实现2.8无护甲HP injury纯函数，并只接入当前type0 standard character damage owner：先按target
`IncomingDamageScale340`执行32位`injury*100/scale`，再在attack-effect source
`WeakTimer12C>0`时整数除2。

## Authority 合同

- scale仅在`>0`时生效；乘法保留低32位，signed除法向0截断。
- attacker weakness读取攻击效果源，不读取resource attacker或target；严格发生在target scale之后。
- KO、HP/HPBound、combo与damage stats均消费effective injury。
- 四个display step仍消费raw ITR injury，不得改为effective injury。
- 本包是unarmored type0；selected armor不应用weakness且不在范围。

## 边界

- 不改`stats.defend`/mode producer、Config、Scene、Prefab或Authority。
- 不迁weapon/type3的旧`FallDamageDiv`兼容路径；另行审计迁移。
- 不处理effect、resource transaction、armor、cpoint、audio或spark。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type0UnarmoredDamageScaleEditorTests.cs`

## 验收

test-first compile red；scale边界、weak顺序、32位overflow、negative/zero、production HP/stat/KO与raw
display分离、zero allocation；相关B5 hit、精确NTSD28 broad、SelfCheck、Scene/Console/Ledger。

已取得fresh compile red：4个预期`CS0117`，均指向尚不存在的
`BattleDamageWriter.ResolveNativeUnarmoredHpInjury(...)`。

首次focused中8项行为通过，zero-allocation测试的XOR哨兵因对称结果抵消为0；仅把测试聚合改为
乘加哨兵，生产算法不变。

## 回滚

移除pure resolver与type0 production调用，恢复raw injury直接进入`ApplyStandardVitalAndStatWrites`，移除
focused test。

## 验证结论

- fresh compile 0 error。
- focused `c5839f1d809f4f0c9d2a86e265eda417`：9/9；首次XOR哨兵抵消仅修测试聚合。
- 全B5+完整hit-plan related `48bbea13d4eb402aab55479c1d328ffd`：284/284。
- 精确94-class NTSD28 broad `d33507bb8e6b4b12a9226d8c60e688e2`：664/664。
- BattleRuntimeSelfCheck `2026-09-05T14:14:56Z` PASS；清理预期negative-path日志后Console 0 error。
- Scene SHA `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、
  203477 bytes、mtime unchanged；Change Ledger 256 records/223 governed code files PASS。
- stats.defend/mode producer、weapon/type3、selected armor与effects仍未处理。
