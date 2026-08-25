# R4-COL-04A — oid999 candidate-collection extra gate preflight

> 日期：2026-08-22  
> 类型：只读 source preflight；未修改 Unity/C++ gameplay。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> 对应差异：`D-COL-004` 的 candidate-collection 子范围。

## 1. C++ release 合同（VERIFIED）

`Makefile:10-31` 将 `collision_collect.cpp` 列入 release `ntsd_new.exe`。其中：

- `sub419f80_pair_allowed`（`107-120`）仅做 active/char_data、attacker `attack_exempt`、pair vrest、
  和一个 attacker oid205 / victim oid9 frame301字段组合的特例检查；没有 oid999、state3005、pic999、
  next1000 或“transition semantic”全局排除。
- `sub419f80_collect_pair`（`220-371`）只要求 current/collision frame所需 ITR/BDY存在、几何相交，
  再经过 kind/team/effect/select规则记录候选。没有 `oid==999` 特判。
- `game_tick.cpp:496-575` 确认 release 会在 state-transition tail 中实际生成 oid999；这不授权 Unity
  以生成语义本身从下一 tick 的 collision collect中移除它。新生对象是否同 tick 可参与另由 scheduler
  / newborn边界决定，不能用全局 oid999 filter替代。

因此，若 oid999 current/collision data实际满足 ITR/BDY/geometry和其他既有规则，C++ collect path允许它
作为 attacker或target进入候选。空BDY/空ITR自然被正式几何门槛拒绝，不需要额外“纯烟雾”规则。

## 2. Unity 现状（VERIFIED）

`BruteForceSceneQuery.IsPureTransitionSmoke` 对 oid999将以下条件直接归为 true：out-of-range frame、
`Runtime.SpawnSemantic=TransitionEffect`、collision frame state3005，或 `pic==999 && next==1000`。

它被用在两条 candidate collection正式入口：

1. `CandidateCollectionPairAllowed`（`6282-6299`）——常规/回退 collect方向；
2. `BuildRoleAwareFormalExactCommonCache`（`3845-3857`）——role-aware cached collect方向。

二者都会在 C++ 的 ITR/BDY/geometry前将 oid999 attacker或target排除。`CheckDeployableResolvedGeometryRisks`
仅说明当前 Unity-adapted production `broken_weapon.dat` 的 state3005 / terminal smoke gated frames没有有效 ITR/BDY；
这说明当前 DAT 下该差异**可能不可达**，不能证明额外全局 gate等价于 C++。

`IsPureTransitionSmoke` 的另两处使用在 `QueryBodyHits(in BattleVolume, ...)` immediate query。该 API有
production weapon caller，且不是本包的 frozen candidate collect path；需要另行追踪 C++ immediate caller，
本包不擅自删除它们。

## 3. 最小实现方向（INFERRED，待 Task Contract 后验证）

只从 candidate collection路径移除额外 `IsPureTransitionSmoke` 排除：

```text
CandidateCollectionPairAllowed: 只保留 pending/attack-exempt/vrest/release special pair gates
RoleAwareFormalExactCommonCache.PairCollectionBaseAllowed: 只保留 pending gate
```

保留 `IsPureTransitionSmoke` helper及 immediate query的两个调用点，直到其独立 source contract闭合。
这样 normal and role-aware collection仍走相同 C++ geometric/kind/team/select gates，不会因为“看起来是烟雾”
而提前跳过一个具有有效碰撞数据的 oid999。

## 4. 必需 fixture

1. synthetic oid999 **target**：state3005或terminal smoke标志，但显式有效BDY；普通 attacker的有效 ITR
   必须记录 target candidate；
2. synthetic oid999 **attacker**：transition spawn semantic或terminal state，但显式有效ITR；普通 target
   必须记录 candidate；
3. normal direct collect 与 role-aware cached collect的 candidate slot、itr index、顺序、RNG state/call count
   必须相同；
4. production-data audit继续只报告“当前 gated frames无有效 geometry”，不得将其改写为 C++ filter。

## 5. Unknown / stop conditions

- **VERIFIED（Unity focused fixture）**：`FormalCollectorMode.ForceRoleAware`配合现有 role-aware direct
  diagnostic可稳定进入 role-aware formal collection；synthetic oid999 attacker/target均与
  `ForceBruteForce` 得到相同 candidate / RNG结果，覆盖 exact common-cache 的 base eligibility。
- **UNKNOWN**：immediate `QueryBodyHits` 中的两个 `IsPureTransitionSmoke` usages的 C++ caller对应关系；
  不纳入本包。
- 若移除candidate gate后需要改变 newborn、opoint、render或 `IsPureTransitionSmoke` 的 immediate-query语义，停止并拆包。

## 6. Non-scope

oid999资源/DAT变更、C++ executable、immediate query、D-COL-005、D-HIT、R5+、render、性能、T8、
服务器、Android、Play Mode。
