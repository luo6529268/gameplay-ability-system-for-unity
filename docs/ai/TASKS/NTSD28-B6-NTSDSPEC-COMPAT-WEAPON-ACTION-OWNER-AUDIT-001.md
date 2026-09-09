# Task Contract — NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / SHARED_SELECTOR_PACKAGE_DEFINED / DEFAULT_PROFILE_UNAFFECTED / PRODUCTION_HELD`
> 来源：`NTSD28-B6-NTSDSPEC-PRODUCTION-OWNER-INVENTORY-AUDIT-001 / VERIFIED`

## 目标

冻结旧`NTSDSpec`四个held weapon布尔调用的真实可达边界，并定义compatibility input resolver迁移到
NTSD 2.8-Logan link-state + linked-definition stats selector的唯一生产包。本审计只读，不修改脚本、
content、Scene、Prefab、资源或Authority。

## 已观察的生产profile边界

- 当前`GameConfig.asset`明确选择`DataOrientedCanonical`，production source缺省也返回该profile；因此
  正常生产tick由`BattleCharacterActionWriter.RouteNative*()`裁决，不读取`NTSDSpec`。
- `LegacyCanonical`仍可由正式command-line/config显式选择，并由
  `BattleCharacterInputActionResolver -> LF2Character.ProcessReleaseInput() -> LF2CharacterActionResolver`
  执行。它是可配置compatibility production path，不得称为test-only dead code。
- `NTSDSpec`四个表达式位于`LF2CharacterWeaponLinkResolver`，再经`LF2Character`三组wrapper供legacy
  resolver调用；canonical writer不调用这些wrapper。

## Authority selector

Authority `input_routing.cpp`只读取holder `interaction_state`与linked object definition `<stats>`：

| 当前状态 | relation | 动作字段 / fallback |
|---|---:|---|
| standing/walking | 0 | ordinary RNG actions 60/65 |
| standing/walking | x01 | 101+direction=`light_throw/45`；其余`normal_attack1/20`或`normal_attack2/25`，同步RNG一次 |
| standing/walking | 4 | `light_throw/45` |
| standing/walking | 6 | direction=`light_throw/45`；neutral=`weapon_drink/55` |
| standing/walking | 2 | `heavy_throw/50` |
| running | 0 | ordinary `run attack/85` |
| running | x01 | neutral=`run_attack/35`；direction=`light_throw/45` |
| running | 4 | `light_throw/45` |
| running | 6 | neutral=`weapon_drink/55`；direction=`light_throw/45` |
| running | 2 | `run_heavy_throw/50` |
| airborne state4 | x01 | neutral=`jump_attack/30`；direction=`sky_light_throw/52` |
| airborne state4 | 4/6 | `sky_light_throw/52` |
| forward state5 | x01 | `jump_attack/40` |
| forward state5 | 4 | any-but-not-all directions=`sky_light_throw/52` |
| forward state5 | 6 | any direction=`sky_light_throw/52` |

字段缺失或零使用call-site fallback；linked definition缺失也使用fallback。relation2在state4/state5、以及
其他未列relation均无动作。合法关系生产域为1/2/4/6/101；是否可攻击、可跑投、可站投不是object-ID属性。

## Unity 当前与差异

- canonical writer已经具备上述9字段和fallback selector，但helper为private，legacy不能复用。
- 当前/release全部definition扫描，这9个stats action字段entry均为0，因此两端正式语料都使用fallback；
  nonzero字段只有synthetic parser/selector fixtures。
- legacy对合法relation1/4/101的大多数fallback偶然匹配，但忽略全部nonzero linked stats。
- legacy heavy standing/running硬编码50，忽略`heavy_throw`/`run_heavy_throw`。
- legacy relation6 standing总选52；running neutral也选52。Authority两处neutral都应为55。当前OID122/123
  是type6，故在显式LegacyCanonical profile下存在current-content可达差异。
- legacy state5 relation4只检查“任一方向”，没有排除四方向同时按下；Authority relation4拒绝all-directions，
  relation6接受。
- 四个旧NTSDSpec布尔调用只落在legacy最终else：合法relation1/4/6/101早已被前分支消费；relation2
  standing/running由heavy分支消费，state4/state5应按Authority无动作。也就是说这些布尔值没有合法
  relation-state决策所有权，只会给malformed/旧壳制造额外动作，必须删除而不是换成另一张ID表。

## 唯一production包

`NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001`：

1. 从`BattleCharacterActionWriter`抽出无状态、零分配的shared linked-action selector（或等价单一owner），
   canonical与legacy都消费相同field/fallback规则；不得复制第二套switch。
2. legacy从经runtime slot验证的held entity取得current DAT wrapper；不存在/失效时沿native fallback，不从
   CLR weapon class或object ID推断action field。
3. standing/running/state4/state5按上表修正relation6 neutral、heavy fields与relation4 all-directions；
   保持每个call-site现有action write/counter/facing/velocity所有权，除已明确列出的selector差异外不改输入边沿。
4. 删除`LF2CharacterWeaponLinkResolver`的`GetHeldObjectId`、三组bool方法，删除`LF2Character`对应wrapper，
   使四个NTSDSpec调用表达式归零。
5. relation2 state4/state5及未知relation明确no-op；不得以旧Attackable=true恢复攻击。

## 验收矩阵

- Legacy/DataOriented分别覆盖standing、walking、running、state4、forward state5；relations
  0/1/2/4/6/101/unknown，neutral/horizontal/depth/mixed/all-directions。
- linked stats zero、nonzero、missing definition、stale target slot、same slot、generic current-DAT shell；
  验证fallback/action、frame counter、AttackingCounter、facing与Vy副作用。
- relation1/101 normal attack RNG固定seed/call-site/call count；selector本身0 RNG/0 allocation。
- current OID122和123 Legacy neutral standing/running由旧52 RED到55；DataOriented输出保持55。
- heavy fields150/160 synthetic；link2 state4/state5 no-op；old ID100/101/150/213不能影响结果。
- `rg 'NTSDSpec\.(IsWeaponAttackable|Can.*ThrowWeapon)'`为0；compile、focused B2/B6、NTSD28、
  SelfCheck及必要Legacy override Play。runtime前最多`RUNTIME_PENDING`。

## 排除与阻塞

- 不改input sampling/cross-key compatibility、combo、movement cadence、held relation producer、WPoint、
  CPoint、damage/impact、content、Scene或Authority；邻接输入问题如被RED发现必须另建owner。
- 当前两个B6代码包尚未进入Unity Test Runner；本包保持production held，不重试已穷尽的license路径。
- zero stats corpus不授权删除parser/carrier；它们是2.8 schema规则并已有synthetic证据。

## 回滚

只移除本治理记录及Ledger/STATE/handoff/总表增量；无行为文件回滚。

