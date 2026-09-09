# NTSD28 B5 first-BDY action / encoded response audit

## Authority live path

- `source/ntsd28_core/src/simulation/battle_world.cpp`的
  `BattleWorld28::resolve_confirmed_unarmored_hit(...)`在vrest gate之后、普通damage/resource/reaction尾部之前，
  从目标`frame.action`取得**当前帧**并读取其第一个`bdy`。它不读取candidate实际重叠的BDY。
- 该文件SHA-256为
  `EB37E8EC1186BF0349A3B3B5759666C3A8F44F0E1640C159E06F805189FBCCB1`；
  `source/ntsd28_playable/scripts/build.ps1`第74项直接编译该translation unit，属于playable closure
  `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- `SimulationTickDriver28::step(...)`的type0/non-type0两次consumer loop只将原始kind 0/4/5/50送入
  ordinary classifier；成功first-BDY/encoded response不生产ordinary combo，并只break当前attacker的
  candidate loop，后续attacker继续。
- type1 armor的bypass、MP不足和armor HP broken fallback会重新进入该unarmored continuation；真正
  `ReducedDefense`或可用`ReducedType1Armor`不进入本响应。

## Exact order and writes

1. first BDY `kind`在`[1000,1999)`：目标action=`kind-1000`；`respond=-1`投影攻击者group、
   `respond=0`投影group 1、其他值原样投影；攻击者hold=3、目标hold=-3。
2. `kind`在`[1999,2999)`：目标action=`kind-2000`，包括1999→-1边界；group投影相同，但不写hold。
3. 上述两段都直接返回applied，跳过普通damage/resource/reaction/combo。
4. `kind`在`[1000000000,1999999999)`：减去base后按`chance/targetAction/attackerAction/effect`
   的`2/3/3/1`十进制位宽解码。chance 1..99调用同步RNG，upper bound 100、callsite=chance；
   roll < chance才成功。chance 0或>=100不消费RNG并直接成功。
5. 编码成功时，双方action小于999才写入并把各自frame counter清零。effect 1/3转group；2手工扣
   raw ITR injury并累加target HP-consumed和attacker score；4施加hold；5/7=hold+group；
   6=hold+manual damage；0及其他effect只有action写。成功后同样跳过普通尾部和combo。
6. positive chance失败仍保留一次同步RNG消费，但继续普通未减伤伤害路径。

## Authority test evidence

`source/ntsd28_core/tests/battle_world_tests.cpp` 10963-11329覆盖：

- 33%成功/失败、callsite=33、失败后普通伤害；
- effect 1..7组合、manual damage可把HP降到0但统计仍按raw injury写17；
- 只认第一个BDY，secondary encoded/1xxx必须忽略；
- type1 resource fallback进入响应；
- 1000/1033/1998/1999/2000/2998与2999 exclusive边界；
- 成功响应跳过combo和同attacker剩余candidate，但不阻断后续attacker。

## Production reachability

- 当前Authority `resources/runtime/decoded_dat`逐帧只计第一个BDY：55帧使用1xxx（只包含
  1033/1055/1058），157帧使用encoded范围，共212帧、29个DAT文件。该目录由正式runtime catalog消费。
- 当前冻结Unity `Assets/NTSD/Config`逐帧只计第一个BDY：`chars/criminal.dat`已有22帧落在
  1xxx响应范围。故1xxx行为缺失在现有Unity正式内容内已经可观察；encoded内容迁移仍归H/B11，
  但其runtime规则不能因此被硬编码为不支持。

## Unity current production surface

- `Lf2DatConverter.ConvertToFrameData`只把第一个BDY kind写入
  `LF2FrameData.primaryBodyKindForEffectSuppression`；`ConvertToBodyBox`只投影X/Y/W/H，BDY `respond`
  没有强类型carrier，legacy `BodyBox`也没有respond字段。
- 该first-kind目前只被`BattleDamageWriter.ResolveNativeEffectActionOverrideForProjectedTarget`用于另一条
  latched-frame kind50/52 suppression；没有当前帧first-BDY response consumer。
- `BattleHitCandidateSequenceRunner`在damage disposition先执行consume effects再dispatch，成功后的
  whole-attacker abort只有OID300 redirect；没有在普通writer之前运行的first-BDY响应或per-attacker终止信号。
- `BattleDamageWriter`及`BattleEcsHitExecutionPlan`没有1xxx/2xxx、encoded decode、同步RNG transaction、
  action/group/hold/manual-damage projection，也无法表达chance失败后继续普通damage。

## First difference and next split

`FIRST_BDY_ACTION_AND_ENCODED_RESPONSE`是hit-group eligibility之后下一个独立、production-reachable的B5
first difference。下一包先执行`NTSD28-B5-FIRST-BDY-RESPONSE-OWNER-AUDIT-001`，冻结：

- 复用现有first-kind还是建立一般化first-BDY value，以及`respond`的parser/carrier/copy/测试边界；
- pure decode/decision与同步RNG消费的单一owner、chance callsite合同；
- active armor/reduced defense排除与type1 fallback进入条件；
- shared runner在consume effects之前的actual writer和per-attacker abort；
- HitPlan投影、RNG shadow/commit、writer-effect diff与普通fallback路径；
- first-body-only、边界值、effect 0..7、统计、zero-allocation、SelfCheck和真实Play witness矩阵。

owner audit完成前不得只在`BattleDamageWriter.ApplyStandardCharacterDamage`添加1xxx动作跳转；那会遗漏
non-character目标、encoded RNG、type1 fallback的统一顺序、consume effects前置要求、HitPlan和candidate abort。
