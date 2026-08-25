# R4-COL-02 — caught-cpoint / `hurtable` consume guard 只读预检

> 日期：2026-08-22  
> 状态：`VERIFIED`（C++ release source / Unity source preflight）；尚未修改脚本、未运行 C++ executable、未运行 Unity 测试。  
> 对应差异：`D-COL-002`。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。

## 1. C++ C07-B 合同

`src/entity/collision.cpp:57-80` 的有效 frozen candidate consume 顺序是：

1. `s_vrest[target][attacker] > 0` 时只 skip 当前 candidate；
2. `attacker.hit_confirm2 != 0 && target.obj_type==0` 时结束整个 attacker；
3. 若 target 的 `prev_frame2.cpoint.kind==2`、`target.catcher_idx` 指向 active catcher，且该 catcher
   的 `caught_idx == attacker slot`，再读取 catcher 的 `prev_frame2.cpoint.hurtable`；
4. catcher prev2 frame/cpoint 缺失，或 `hurtable==0` 时 `goto next_pair_outer`，即只 skip 当前 candidate；
5. 只有未被 skip 的 candidate 才进入 runtime ITR replacement、effect21和 kind dispatch。

`next_pair_outer` 位于 pair loop 末尾、`next_attacker` 之前，所以 C07-B 不能实现为整个 sequence abort。

## 2. Unity 已有 adapter 与真实缺口

- `BruteForceSceneQuery.TargetBeingCaughtPairBlocked(...)`（6392-6412）已经逐项映射：
  - target prev2 cpoint kind2；
  - target `CatcherSlotIndex`；
  - `FindEntityByRuntimeSlotForQuery` 查询 active catcher；
  - catcher `CaughtSlotIndex == attacker slot`；
  - catcher prev2 cpoint 缺失或 `hurtable==0` 都返回 blocked。
- `SimulationQueryAndLinkModule.FindEntityByRuntimeSlotCurrent(...)`（191-196）仅返回
  `IsActiveForCurrentPassInternal` 的 entity，因此 Unity adapter的“catcher active”语义已闭合。
- `BruteForceSceneQuery.IsReleaseConsumerPairBlocked(...)`（6255-6265）公开为 internal shared helper，
  当前只调用上述 C07-B helper。
- `RuntimeConsumeItrAllowed(...)` 与部分 query compatibility path 已调用该 helper；但
  `BattleHitCandidateSequenceRunner.TryConsumeCandidate(...)` 没有调用，故 frozen candidate 在 unified
  runner中仍可进入 `ResolveRuntimeItrForPair`、`ResolveCandidateDisposition` 与 writer。

## 3. 最小适配位置与顺序

对正常有效 candidate，runner 当前已在 R4-COL-01 写成：

```text
ItrIndex defensive validity
  -> target resolve
  -> CanConsumeRecordedCandidate (vrest / active target)
  -> C07-A HitConfirm2 whole-attacker abort
  -> runtime ITR replacement
```

本包只需在 C07-A 之后、runtime ITR replacement之前加入：

```text
BruteForceSceneQuery.IsReleaseConsumerPairBlocked(attacker, target)
  -> return false  // shared runner interprets as skip current candidate and continue
```

这严格保留 C++ C07-A → C07-B 顺序，并复用现有 helper，不修改 candidate collection。`return false`
与 C++ `goto next_pair_outer` 同义；不得误用 `return true`（那会改成 next-attacker abort）。

## 4. focused fixture 设计

1. **kind0 two-candidate continuation**：first target有 prev2 kind2 / active reciprocal catcher /
   catcher prev2 `hurtable=0`；second target普通。冻结两条后 consume，first HP/vrest不变，second正常受击。
   这同时证明“只 skip current candidate”。
2. **exact + shared**：上述 fixture分别使用 exact `LF2Character` 和 current-DAT character fallback shell，
   确认两个消费入口都经过 shared runner。
3. **positive conditional control**：同样 first target/catcher关系但 `hurtable=1`，kind0应正常写 damage，
   证明不是无条件 cpoint隐藏。
4. **kind6 direct writer control**：blocked case必须不写 `HitConfirmCounter`；source/static确认 gate位于
   `ResolveCandidateDisposition` 前，故 kind1/kind3/pickup/attack等所有 disposition同样不能抵达 writer。

## 5. Unknowns / 不纳入本包

- C++ runtime trace仍 BLOCKED，真实 cpoint技能 Play Mode未验证；
- D-COL-003 effect21、D-COL-005 kind1 target type、CPoint relation/action/raw-frame、held/link等是独立差异；
- 不改 `TargetBeingCaughtPairBlocked` 本身，除非 focused fixture证明它与C++ C07-B字段语义不同；
- 不改 scheduler、candidate collect、CPoint writer、render、容量、pool/worker或C++。
