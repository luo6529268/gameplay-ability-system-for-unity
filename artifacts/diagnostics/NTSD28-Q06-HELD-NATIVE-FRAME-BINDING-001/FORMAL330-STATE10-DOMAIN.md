# Formal330：held联合引用域的child state10复核

2026-09-15。READ_ONLY_CONTENT_DOMAIN_RECHECK / SAME_FORMAL330_INPUTS。

本次只读重算结果：在前次相同的正式330 ITR kind2＋OPoint kind2联合静态关系域上，parent primary WPoint选中的child `state=10` 为 **0行／0 distinct triples**；`state=12` 和 `state=18` 也各为0。不能把这个内容结果解释成source存在相应unsupported-state guard；本次没有用旧任务叙述定义 `settle_held_refill_objects` 的规则。

## 输入身份

两份输入本轮实际读取并重新计算SHA，与前次完全一致：

| artifact | SHA-256 |
|---|---|
| `artifacts/diagnostics/NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001/native-formal-catalog.json` | `44b77f1201f764941ad800318f01e66fa72c0caa433281eed62c91a74ff1d243` |
| `artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/native/authority-content-rerun.jsonl` | `5f5c6c34ce907bb6d7855b8a1e69d410b3cd577807f202332b8a0da30ed0146f` |

catalog为330条已成功加载、无重复OID的正式indexed entries。native capture共有405份文档，按 `entries[].sourcePath` 规范化分隔符后join `path`，只纳入这330个definition。前次核对正式catalog.csv＋330 DAT的331份当前输入hash全部匹配；**本次仅重验上述两份artifact hash，不把前次原文件hash核对写成本次重新执行。**

## 相同算法与域

- 每definition同ID frame只取首个显式声明；frame字段读取精确key的最后值。
- 每parent frame只取第一个 `kind==wpoint` 的subblock，用native `normalized.weaponact/cover`；不按目录名或任意WPoint出现计数。
- ITR kind2 source definitions与type1/2/4/6中具备state1004/2004及BDY的ground target definitions建立保守candidate边。这没有声称攻击边沿、几何、时序都已满足。
- OPoint kind2按实际OID引用建立边：目标必须在正式catalog，初始action须显式存在或落在native隐式0..998域。缺失OID620/621的5条记录不形成有效edge。
- 对联合边 `(sourceOID,targetOID)`，遍历source所有primary WPoint，检查 `weaponact<1000` 的已声明child frame state。隐式零帧的state0不能产生10/12/18；未声明非法action不能伪造非零state。
- 每行计数为 `(sourceOID,parentFrame,targetOID)`，distinct triple为 `(sourceOID,weaponact,targetOID)`；不把原始WPoint条数或全表state计数混为关系域计数。

本轮重新得到：ITR候选边1,924，OPoint有效distinct边158，交集49，union **2,033**，parent definitions **151**。与前次完全相同。

## 0与非0 controls

| 检查 | 数量 |
|---|---:|
| 联合图上选中已声明child action的holder-frame/edge行 | 396,825 |
| 其中child state10 | **0** |
| 其中child state12 | **0** |
| 其中child state18 | **0** |
| 其中child state1001 | 252,931 |
| 其中child state2000 | 76,603 |
| 其中child state0 | 1,556 |
| 全330 definition中显式state10帧（不做held join） | **2,318** |
| 全表state10所处definition数 | **157** |

全部已声明child state的非零计数为：0=1556、15=4064、1000=14162、1001=252931、1002=100、1003=32、1004=17057、2000=76603、2001=11268、2004=17050、3001=12、3002=98、3003=4、3005=1869、3006=6、9998=13。合计396,825。

真实非零held对照（路径均相对正式 `resources/runtime/decoded_dat`）：

1. OID901 `custom/1genma/genma.dat` frame125从第545行开始；第547行OPoint `kind=2, oid=123, action=20`，第548行primary WPoint `weaponact=20, cover=0`。OID123 `w/8.dat` frame20从第30行开始，`state=1001`。这是实际直接引用边上的非零child-state对照。
2. OID901同definition frame168从第823行开始；第825行OPoint `kind=2, oid=120, action=20`，第826行primary WPoint `weaponact=20, cover=1`。OID120 `w/4.dat` frame20从第86行开始，`state=1001`。
3. 全表state10存在的反混淆对照：OID901 `custom/1genma/genma.dat` frame130从第572行开始，`state=10`，第575行primary WPoint `weaponact=30, cover=0`。这是**parent自己**的state10，不是被选中的child state10；OID901也不是本联合图的held target。不能拿它推断Unity检查child state10的分支会触发。

不存在“选中child state10”的正例，因此不能提供正例路径；上述全表state10只能用作解析与owner区分的control。

## 对当前任务的限定结论

同一正式330 ITR2＋OPoint2静态域的选中child-state上界里，Unity `Falling(12)`／`BeingCaught(10)`条件没有内容正例。可记录为 **NO_SELECTED_CHILD_STATE10_12_18_IN_FORMAL330_ITR2_OPOINT2_DOMAIN**。不能将其表述为source拒绝10/12/18、对应Unity分支已正确、或整条held规则已完成。

这个join还刻意保留了所有parent动作，没有证明每个动作在持有关系期间的时序可达。因此非零计数是保守上界，不是实际执行次数。零结论限定于“primary WPoint选中的child frame”；如果Unity某分支在WPoint赋帧前读child当前state，或其他hit/throw/definition变换在关系保持期间改变child动作/definition，则还需要对应caller时点与状态producer闭包，不能仅用本表宣布该分支全runtime不可达。其他relation producer、动态definition替换、synthetic输入及未来内容也不由本表排除。

本轮执行：只读 `Get-Content`、内存Python JSON join/hashlib/Counter统计；只新增本指定文档。无脚本修改、Unity调用、source build、资源或正式源修改。未持久化新的机器图文件，完整输入仍为上述已固定hash的两个artifact。
