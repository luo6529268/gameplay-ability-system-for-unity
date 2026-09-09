# Task Contract — NTSD28-B6-KIND2-PICKUP-CORPUS-CORRECTION-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / CURRENT_CORPUS_CORRECTED / OWNER_AND_THREE_PACKAGES_RETAINED / PRODUCTION_HELD`
> 纠正：`NTSD28-B6-KIND2-PICKUP-RELATION-OWNER-AUDIT-001` 的 Direction-B corpus 小节

## 目标与原因

以 Direction-B 正式冻结的`normalized-projection.tsv`和当前`data.txt`纠正先前kind2 pickup owner audit的
current corpus漏计。Authority `HitCandidateBuilder28::append_pair_geometry()`遍历frame的全部ITR，随后world
classifier逐candidate执行kind2 gate；不存在只读取Naruto clone单一记录的规则。

只修改治理记录；不修改C#、DAT、资源、Scene、Prefab、ProjectSettings或Authority。

## 新鲜测量

| 项 | Direction-B current（纠正后） | 2.8 release | 旧current值 |
|---|---:|---:|---:|
| kind2 ITR tuple | 117 | 375 | 1 |
| 含kind2的definition | 39（均为indexed type0角色） | 148 | 1 |
| kind7 ITR tuple | 0 | 0 | 0 |
| indexed type1/2/4/6 state1004/2004 frame | 31 | 53 | 17 |
| 上述target frame的WPoint / nonzero weaponact | 0 / 0 | 0 / 0 | 0 / 0 |

current target分布是type1/1004=5、type1/2004=10、type2/1004=2、type2/2004=3、type4/1004=11。
旧表遗漏的15个type1 frame来自indexed OID447、449、501、502、506，其中OID449贡献10个state2004 frame。
Naruto clone frame65仍是合法kind2实例，OID120 frame64仍是合法type1/state1004 target，但该pair并非唯一。

## 对 owner 的影响

- OID120属于`weapon_throw {120,124}`，任一可达kind2 holder拾取时Authority写holder101而Unity写1；
- exact OwnerSlot缺失、prior-nonzero relation下+35C无条件增加、kind7额外pickup和公共WPoint tail缺失仍成立；
- 两端candidate target ground frames仍全部没有WPoint，target WPoint override仍是规则/synthetic边界；
- kind7仍无两端内容语料。

原三包`carriers/system rules -> pure core -> atomic actual/HitPlan/legacy`保持。focused/Play必须覆盖多个current
holder、OID120、OID449 state2004 overwrite与原Naruto-clone witness，不能再用单一fixture代表current corpus。

## 不变量 / 回滚

- 不改变Authority规则、内容权威、lifecycle/positive-validation依赖或production hold。
- 原owner record只纠正current corpus数字和“唯一”措辞，规则/owner/三包结论不supersede。
- 回滚只移除本correction与交叉引用；没有代码、内容或Scene回滚。

