# Task Contract — NTSD28-B6-HELD-RELATION-PRODUCER-DOMAIN-CORRECTION-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / ITR_AND_OPOINT_UNION / CURRENT_REACHABILITY_REBASELINED / OWNERS_RETAINED / PRODUCTION_HELD`
> 纠正：`NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001`中把pickup holder误写成完整held-relation holder domain

## 目标

纠正 current held/WPoint reachability 只使用 ITR kind2 pickup producer、遗漏 OPoint kind2 direct-link
producer的问题；用 Direction-B normalized projection 与 indexed definitions重算完整 source-target relation
edge domain，并回链 terminal、kind3、DVX与missing-action owners。

只修改治理记录；不修改C#、Config、Scene、Prefab、资源、ProjectSettings、Server或Authority。

## 根因

前一轮已正确修复 multiline subblock漏计，但把“117条 ITR kind2 所在39个pickup holder definitions”继续
当成了所有能进入 `settle_held_refill_objects()` 的 holder集合。Authority与Unity production还有第二个
独立 relation producer：OPoint kind2在child生成时直接写双方negative/positive held relation，不要求holder自身
拥有 ITR kind2，也不要求child处于ground pickup state。

Authority `BattleWorld28::spawn_from_opoint_intents()`在 `intent.point.kind == 2` 时写：

- child `interaction_state=-1`、`linked_parent_slot=parent_slot`；
- parent `linked_child_slot=child_slot`、`interaction_state=(frame_a==1 ? 101 : 1)`；
- child可为type1/2/3/4/6等被引用definition。

Unity `LF2ObjectPointFactory`调用`AttachOpointHeldObject()`建立对应生产关系。因此只读pickup join不是完整
held/refill consumer domain。

## Corrected current relation domain

基于 frozen projection 与137个`data.txt` indexed definitions：

| producer/domain | 记录或definition | 关系边 |
|---|---:|---:|
| ITR kind2 pickup | 117 records / 39 source definitions / 16 ground target definitions | 624 source-target pairs |
| OPoint kind2 direct link | 62 records / 39 source definitions / 13 target OIDs | 56 distinct source-target pairs |
| 两者重叠 | — | 12 pairs |
| 完整union | 41 source definitions | 668 distinct source-target pairs |

OPoint-only source definitions为 OID52 Kyubi 与 OID58 Deidara Bird。13个 OPoint target OIDs为
101、120、121、122、123、150、213、422、434、437、444、447、500，覆盖type1/2/3/4/6；
type3与type6不会出现在先前ground pickup target join中。

## Corrected WPoint reachability

41个relation-capable source definitions共有7,624条primary WPoint：

| branch | 旧pickup-only | 完整union |
|---|---:|---:|
| kind3 | 770 | 811 |
| kind3且authored DV非零 | 1 | 1 |
| non-kind3 `dvx!=0` | 184 | 186 |
| `weaponact>=1000` | 28 | 33 |

terminal新增5条全部来自 OID52 Kyubi actions 0/1/5/6/115；完整33条分布于23个 source definitions且
`weaponact`全为1000。它们与all-indexed branch计数相等，说明当前所有kind3、non-kind3 DVX及terminal
primary WPoint都至少有一种正式held relation producer。

## Corrected missing-action join

按每条真实source-target edge，而不是把全部target与全部source做无条件笛卡尔积；对每个source的
`weaponact<1000` primary WPoint检查target definition是否声明该action：

- 完整union：16,796 holder-frame/edge rows；2,402个distinct
  `(source definition, weaponact, target definition)` triples；
- 先前pickup domain：16,247 / 2,318，仍是正确子集；
- OPoint-only edges新增549 / 84；
- 完整union包含119条negative `weaponact=-888` rows / 17个distinct triples；
- missing rows按target type为type1=2,941、type2=12,778、type3=10、type4=528、type6=539。

## 对既有owner的裁决

- kind2 pickup audit的117 records / 39 pickup holders / 16 ground targets仍正确；只禁止再把它称为完整held域。
- terminal `Free-not-Destroy`、post-refill/pre-pose顺序与lifecycle dependency完全不变；current witness改为33/23。
- kind3 `RunStep12 + DropRandomly`双owner、authored-DV overlap与四draw顺序不变；current matrix改为811。
- DVX weapon-HP及+0x2F8 owner不变；current non-kind3 DVX matrix改为186。
- missing-action owner audit必须使用668条edge union与16,796/2,402基数，不得只测ground pickup。
- cover2与referenced state12/18的“0”需要在完整edge union上重新确认；在确认前降为
  `RECHECK_REQUIRED`，不能沿用pickup-only join直接关闭。

## 下一步与阻塞

1. 立即以完整668-edge union完成`NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-OWNER-AUDIT-001`。
2. 同步复核cover2与referenced state12/18在OPoint target domain的结果；若仍0再恢复dormant结论。
3. production runtime stack仍未清；terminal/kind3/DVX等owner只更新矩阵，不新增C#行为。

## 回滚

仅移除本correction并恢复pickup-only误称；没有代码、content、Scene或Authority回滚。

## Follow-up closure（2026-09-08）

`NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-OWNER-AUDIT-001`已使用本Task的668-edge union完成：
missing-action=16,796/2,402并冻结owner；cover2与referenced state12/18均重验为0。下一步不再是本Task中的
recheck，后续按lifecycle cleanup→terminal→missing-action production依赖推进。
