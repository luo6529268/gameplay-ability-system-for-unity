# Task Contract — NTSD28-B5-INPUT-HP-COST-COMPAT-STAT-RETIREMENT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / COMPAT_PATH_LIVE / PARTIAL_RETIREMENT_UNSAFE / SHARED_TRANSACTION_PRODUCTION_DEFINED`
> 依赖：`NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001 / VERIFIED`、
> `NTSD28-B2-NATIVE-COMBO-ACTION-TRANSACTION-001 / FOCUSED_TEST_PASS`。

## 目标

确认`LF2Character.TryInputFrameJump`与
`LF2Entity.TryCharacterDatInputFrameJumpCompatibility`中的两个
`ComboCountVic += hpCost`是否可以直接退休，并冻结不污染exact累计的后继production边界。本审计只读，
不修改C#、content、Scene、Prefab、ProjectSettings或Authority。

## Authority transaction

- playable与core build分别在`source/ntsd28_playable/scripts/build.ps1:64`和
  `source/ntsd28_core/scripts/build.ps1:57`编译`input_routing.cpp`。
- `input_routing.cpp:317-428 apply_action(...)`的顺序为action lock、signed/999、source-frame存在、
  encoded-state redirect、F6 resource gate、adjusted MP、`hp=(mp/1000)*10+frame.hp`、affordability与
  fallback、MP/HP/+0x350/+0x34C/effective-max-HP写入、最后`+0x144`与action/facing。
- 正式transaction从不写Unity legacy `ComboCountVic`；HP费用只累计到
  `input_hp_consumed_total`（Entity28+0x34C）。
- Unity `BattleCharacterActionWriter.ApplyNativeInputAction`已实现上述同一transaction，且既有
  `NTSD28NativeComboActionTransactionEditorTests`覆盖redirect、cost、fallback、multiplier/double、
  F6、HPBound与exact累计；无需复制第二套算法。

## Unity reachability and differences

1. `LF2Character.TryInputFrameJump`全仓只有定义、没有调用者；它是dead duplicate，并单独写
   `ComboCountVic += hpCost`。
2. `LF2Entity.TryCharacterDatInputFrameJumpCompatibility`不是dead code：
   `BattleCharacterInputActionResolver`的combo/direct J/K/L调用它，shared character-DAT legacy
   standing动作也调用它；注册实体经`BattleCharacterActionWriter.TryCharacterDatInputFrameJump`
   回调该compat方法。
3. 当前正式Config选择`DataOrientedCanonical`，但`LegacyCanonical`仍可由正式config/command-line选择；
   该compat链属于可配置production path，不能按test-only删除。DataOriented会先执行exact native
   transaction并投影legacy state，但这不授权保留一个语义不同的第二writer。
4. 两个legacy helper只解析`frame.mp`的个位与千位、直接扣PP/HP、写`ComboCountVic`和PP display；
   它们缺action lock、state redirect、adjusted MP、frame `hp`、effective-max-HP、+0x34C/+0x350、
   fallback、+0x144与F6完整边界。
5. 因此“仅把`ComboCountVic += hpCost`改为`InputHpConsumedTotal34C += hpCost`”会把不完整/错误的
   legacy delta写入exact字段，不能作为安全修复；直接删writer又会让仍在扣HP的compat路径失去exact
   accounting。当前两个writer都必须等共享transaction接管后再退休。
6. 本包前生产`ComboCountVic +=`共8处：两个input compatibility、两个negative-recovery、
   standard/reduced actual各一、HitPlan各一、held CPoint一。input后继只能移除前两处。

## 唯一后继production包

`NTSD28-B5-INPUT-HP-COST-COMPAT-SHARED-TRANSACTION-PRODUCTION-001`：

1. 抽取或暴露`BattleCharacterActionWriter.ApplyNativeInputAction`的单一无状态core；实例正式入口保持，
   禁止复制第二套cost/fallback逻辑或每次动作分配writer。
2. `BattleCharacterActionWriter.TryCharacterDatInputFrameJump`改为消费同一exact transaction，并按
   `Attempt.Applied`返回；注册的Legacy/DataOriented实体共享相同generic action规则。
3. unregistered/focused compatibility入口也调用同一core；删除
   `TryCharacterDatInputFrameJumpCompatibility`的旧资源算法和`ComboCountVic`写入。
4. 删除无调用者`LF2Character.TryInputFrameJump`，或将其改为无重复逻辑的单行转发；不得保留第二
   cost writer。
5. 保持caller拥有的combo/edge clear、frame counter、release/builtin selector、movement与pass顺序；
   B6 linked weapon selector、negative recovery、held CPoint和schema不并入。

## Test-first验收

- 先用source closure和行为测试得到RED：两个legacy writer存在，Legacy/unregistered结果不等于exact。
- registered Legacy、registered DataOriented与unregistered三路覆盖signed/999、missing frame、lock、
  redirect、mp/hp/HPBound/+0x34C/+0x350、waiver/F6、fallback priority与last-action差异。
- 证明`ComboCountVic`哨兵保持；exact transaction只执行一次，caller clear/frame-counter合同不变。
- 复跑NativeComboAction、CharacterInput、B2/B5相关、build、full SelfCheck与Legacy override Play。

## 排除与回滚

不改input sample/edge/combo顺序、linked weapon action selector、direct builtin resource policy、content、
Scene、Authority、negative recovery、CPoint或schema。回滚只移除本治理记录；production另建独立Task/
Change并按其记录回滚。
