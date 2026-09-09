# NTSD28 B5 Multi-body Candidate Multiplicity

## Authority

- `source/ntsd28_core/src/simulation/hit_candidates.cpp:225-265`：外层ITR顺序不变，内层遍历目标所有BDY；每个2D与depth均重叠的BDY都追加到 `overlapping_candidates`，并保留独立 `body_source_line`。
- `source/ntsd28_core/src/simulation/battle_world.cpp:4359+`：逐个几何candidate执行后续过滤与选择。
- multiple路径逐个占用固定20容量；nearest路径的同距离candidate可逐个触发同步RNG tie-break。该差异不只是诊断计数。

## Unity

- `BruteForceSceneQuery.HitsTarget(...)`：遍历BDY，但第一个重叠即写 `bodyX` 并 `return true`。
- `BruteForceSceneQuery.HitsTargetCached(...)`：缓存rect路径同样在第一个重叠即返回。
- `CollectCandidatesForPair(...)` 与 `CollectCandidatesForRoleAwareFormalDirection(...)` 各只调用一次 `TryRecordReleaseCandidate(...)`，所以每个ITR/target最多一个candidate。
- `HitCandidateMax` 已经是20，容量常量本身不是本次首差；首差在进入选择/容量之前丢失后续重叠BDY。

## 可观察影响

1. 多BDY重叠时candidate数量与BDY顺序不同。
2. `bodyX` 参与后续reject/select时，首BDY与后续BDY可能得到不同结果。
3. multiple路径的20容量占用及被截断的后续target/ITR可能不同。
4. nearest等距离路径可能少走同步RNG，继而改变后续战斗RNG序列。
5. consumer看到的candidate顺序和首次成功命中对象可能不同。

## 实施路由

下一先审计正式两条collector与兼容查询边界；目标是一个allocation-free、按目标BDY源顺序枚举的共同seam，
让brute与role-aware cache结果完全一致，并维持每个candidate独立进入现有选择逻辑。当前不直接改实现。
