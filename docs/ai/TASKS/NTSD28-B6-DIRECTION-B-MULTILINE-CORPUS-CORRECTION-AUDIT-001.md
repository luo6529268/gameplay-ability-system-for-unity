# Task Contract — NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / ROOT_CAUSE_MULTILINE_OMISSION / CURRENT_REACHABILITY_REBASELINED / OWNER_RULES_RETAINED / PRODUCTION_HELD`
> 前置纠正：`NTSD28-B6-KIND2-PICKUP-CORPUS-CORRECTION-001 / VERIFIED`

## 目标

审计近期B6 current-content计数方法，纠正只匹配单行DAT subblock造成的系统性漏计；以Direction-B冻结的
`normalized-projection.tsv`、138-DAT raw manifest、当前`data.txt` indexed subset与正式Unity
`Decryptor -> ParserV2 -> Converter`语义重新基线WPoint/CPoint/ITR reachability。

只修改治理记录；不修改C#、Config、Scene、Prefab、资源、ProjectSettings、package、Server或Authority。

## 根因与强制口径

当前DAT同时使用：

```text
wpoint: ... wpoint_end:
```

和：

```text
wpoint:
  ...
wpoint_end:
```

CPoint/ITR同理。旧临时正则只匹配第一种，因而把current WPoint写成312、CPoint写成33、kind3 ITR写成1，
而release decoded文件主要是单行格式，所以产生看似合理但不对称的`current/release`数字。

从本纠正起：

- Direction-B运行时/行为计数必须读取冻结normalized projection，或使用与正式Decryptor/ParserV2/Converter
  等价并覆盖单行+多行subblock的解析；
- `data.txt`用于区分137个indexed definitions与138-DAT全manifest；unindexed DAT不能冒充production definition；
- raw explicit-field inventory与converter后runtime value必须分列，尤其CPoint front/back legacy alias；
- `rg`单行raw命中只可作为格式诊断，禁止再作为完整current corpus或dormancy证明。

## Direction-B corrected inventory

### 顶层数量

| domain | 138-DAT normalized tuples | indexed tuples | indexed primary records |
|---|---:|---:|---:|
| WPoint | 7,995 | 7,977 | 7,969 |
| CPoint | 1,426 | 1,426 | 1,426 |
| ITR | 4,437 | 4,400 | 不适用：Authority geometry遍历全部ITR |

WPoint有8个非primary extra records；held consumer读取primary WPoint。CPoint当前每frame至多一个，因此tuple与
primary相同。

### ITR / relation / impact

indexed ITR kind计数：kind0=2334、kind1=4、kind2=117、kind3=249、kind4=488、kind5=353、
kind6=275、kind7=0、kind8=405、kind10=15、kind11=5、kind14=114、kind15=40、kind16=1，
kind17/18=0。

- 117条kind2分布于39个indexed type0 holder definitions；该项已由前置correction单独冻结。
- 249条kind3全部有catching/caught pair、respond均0；234条的source catching action实际是state9/kind1 CPoint。
- kind10/11当前并非0：20条全部来自OID36 Tayuya actions243..247，每frame为3条kind10+1条kind11。
  因此native impact owner不再是release-only；kind17/18仍仅规则/synthetic。

### CPoint

current raw/normalized共有1,426 blocks：kind1=776、kind2=650；kind1中state9=760、`vaction<=0`=0、
negative decrease=204、negative decrease且`throwvx!=0`=1、`throwvx!=0`或dircontrol非零=88。

raw explicit inventory：

| field | explicit | nonzero |
|---|---:|---:|
| injury | 298 | 223 |
| cover | 142 | 106 |
| fronthurtact | 170 | 170 |
| backhurtact | 170 | 170 |
| aaction | 9 | 9 |
| taction | 9 | 9（均为-232） |
| faction/baction/uzaction/dzaction/z/recover/drain/gain | 0 | 0 |

170条front/back均为kind2且同raw block没有explicit injury/cover；现有converter alias使normalized/runtime
`Injury` nonzero由223扩大到393、`Cover` nonzero由106扩大到276。该alias事实与退休结论保留，但current
witness从4扩大到170。

kind1 throw path有58条`throwvx!=0`，其中48条`throwinjury>0`；held settlement有223条正injury，cover分布
为0:205、1:15、11:3。旧current 2/6/9等小样本计数全部不得继续使用。

### WPoint held path

39个具备kind2 pickup的current holder definitions共有7,366条primary WPoint：

| branch | corrected current |
|---|---:|
| kind3 | 770 |
| kind3且至少一轴authored DV非零 | 1（OID7 Rock Lee action255，dvx100/dvy-1） |
| non-kind3 `dvx!=0` | 184 |
| `weaponact>=1000` | 28 |
| cover=2 | 0 |

全部indexed definitions则有33条terminal WPoint、811条kind3、186条non-kind3 DVX。terminal 28条holder记录
覆盖Pain、Deidara、Hidan、Jiroubou、Kabuto、Kakuzu、Kankuro、Kidomaru、NCKakuzu、Puppet Sasori、
Reaper、Rock Lee、Sakon、Sasori、Sasuke/CS2、Shikamaru、Shino、Tayuya、Temari、Tenten、Yamato等。

16个current indexed supported held-target definitions有31个state1004/2004 frames。把holder primary WPoint的
`weaponact<1000`与这些target frame表联结得到16,247条missing-action行、2,318个distinct
holder/action/target组合；Authority写child action后diagnostic/continue，而Unity会用null/default geometry继续
pose/release，需独立owner audit。referenced state12/18组合仍为0，cover2仍0，因此这两项dormant结论保留。

### catch post-vaction join

current 137 indexed definitions / 42 type0 / 249 kind3 ITR中，234条source catching action为state9/kind1；
得到9,228个initial kind2 pairs，其中40个post-vaction不再是kind2。40个全部来自OID417
`specialattack/tree.dat` attack73 -> catching72 / caught130，source action72 CPoint `vaction=17`；target action17
存在但无kind2 CPoint。故settlement preflight已有current formal witness，不再只依赖release OID555/vaction517。

## 对既有B6 owner的裁决

Authority源码与Unity代码对照得出的规则/字段/owner继续有效；以下仅纠正current corpus与优先级：

| 原记录族 | 纠正 |
|---|---|
| kind2 pickup | 使用前置correction的117/39 holders、31 target frames；三包不变。 |
| CPoint 27 schema / old hurt consumer | current blocks 1426、front/back170、A/T各9；schema与retirement更强，不再用33/4/1。 |
| catch relation exact fields | current kind3为249且全部source pair authored；missing CatchSourceSlot90是广泛current witness。 |
| catch control-flow / settlement preflight | current kind1/state9为776/760；negative=204；OID417产生40个current invalid post-vaction pairs。 |
| throw / held injury | current throwvx58、positive throwinjury48、positive held injury223；现有code/owner scope保留但测试矩阵扩大。 |
| held kind3 | current770 holder rows且已有1条authored-DV overlap；DVX->kind3 continuation是current-content reachable。 |
| held DVX weapon HP / +2F8 | current holder non-kind3 DVX=184；不再使用3条小样本。 |
| terminal WPoint | current holder terminal=28；从dormant future package提升为当前可达，下一先做structural owner audit。 |
| damaged held drop / cover2 | corrected current仍为0，dormant分类保留。 |
| native impact / NTSDSpec inventory | current kind10/11=15/5，均为Tayuya243..247；三包owner保留，Play不再受release内容导入前置。 |
| positive-link retirement / lifecycle | 不依赖DAT计数，结论不变。 |

`NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001`与
`NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001`仍为RUNTIME_PENDING；本纠正没有发现需要回退其已写
规则，但focused/SelfCheck/Play与corpus guards必须按新基数扩展，不能以旧小样本完成。

## 下一步与阻塞

1. `NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-OWNER-AUDIT-001`已闭合；terminal已是current-content
   reachable且会调用despawn/link cleanup/slot reuse，production等待lifecycle cleanup runtime绿灯。
2. 下一只读owner审计held WPoint missing-action preflight/continue；再逐条修订kind3/DVX/impact/catch测试矩阵。
3. 既有production runtime栈未清，仍不叠加新C#行为。Unity licensing阻塞状态不变。

## 回滚

仅移除本治理correction并恢复旧corpus状态；没有代码、content、Scene或Authority回滚。

## Held relation producer-domain correction（2026-09-08）

上述39/7,366/770/184/28与16,247/2,318均是正确的ITR-kind2 pickup子域，但不是完整held域。
`NTSD28-B6-HELD-RELATION-PRODUCER-DOMAIN-CORRECTION-AUDIT-001`补入OPoint kind2 direct-link后，
完整current为41 sources/668 edges、primary WPoint7,624、kind3=811、non-kind3 DVX=186、terminal=33，
missing-action=16,796 rows/2,402 triples。其他CPoint/ITR/impact/catch计数不受影响。
