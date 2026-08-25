# R4-COL-004A — oid999 candidate-collection extra gate

<!-- CHANGE-RECORD
id: R4-COL-004A
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision_collect.cpp:107-120,220-371; J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:496-575
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-PREFLIGHT-VERIFIED / CODE-WRITTEN / UNITY-COMPILE-PASS-20260822-0433+08 / FULL-SELF-CHECK-PASS-20260822-043340+08 / PLAYMODE-PENDING / CXX-RUNTIME-TRACE-BLOCKED
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 类型：battle / collision / oid999 / candidate-collect / test  
> 所属 Work Package：`R4-COL-04A`  
> 当前状态：`RUNTIME_PENDING` — normal/role-aware collection base调整与focused fixture通过 Unity compile与full self-check；C++ runtime trace与真实 oid999 Play Mode仍未关闭。

## 1. 目标与范围

仅删除 normal和role-aware frozen candidate collection的 `IsPureTransitionSmoke` extra exclusion。保留 helper与
immediate query usages，保留所有 C++ normal geometry/kind/team/effect/select/vrest/newborn gates。

## 2. Authority / 差异依据

- **C++ VERIFIED**：`collision_collect.cpp:107-120,220-371`无 oid999/transition global exclusion；有有效
  ITR/BDY/geometry的对象按一般 collect链处理。
- **Unity VERIFIED**：`CandidateCollectionPairAllowed`与 role-aware exact common cache均额外调用
  `IsPureTransitionSmoke`。
- **Unity production-data fact**：现有 audit发现当前被 gate 的 oid999 frames无有效 ITR/BDY；这是当前资产
  可达性事实，不是 C++ filter。
- **C++ trace BLOCKED**：R1-WP02未解除；不运行C++ executable。

## 3. 计划最小改动

1. 从两条 collection base gate移除 `IsPureTransitionSmoke`；
2. 不修改 helper或 immediate query calls；
3. 用 synthetic valid geometry验证 oid999作为 target/attacker可记录候选；
4. 比较 normal与role-aware可用路径候选和RNG；
5. 如实记录 role-aware hook不可用或其他 blocker。

## 4. 实际代码写入（待验证）

- `CandidateCollectionPairAllowed` 不再按 `IsPureTransitionSmoke` 排除 attacker或target；
- `BuildRoleAwareFormalExactCommonCache.PairCollectionBaseAllowed` 不再按该 presentation/oid helper拒绝；
- helper及 immediate `QueryBodyHits` 的两个调用点未触碰；
- self-check新增 oid999 transition-state target与 transition-semantic attacker的 synthetic valid-geometry
  matrix，并以 `ForceBruteForce` / `ForceRoleAware` 比较 candidate和RNG。

## 5. 预期副作用与不可回退边界

- 正确副作用：只有 oid999实际具有效 geometry时才可能走后续正式筛选并产生 candidate；
- 不可改变：没有 ITR/BDY、pending、attack exempt、vrest、release special pair和现有kind/effect/select规则仍拒绝；
- 不回退：CentralOnly/Texture2DArray、1.5视觉缩放、容量、30Hz、FrameInputSet、SoA/pool。

## 6. 验收与回滚

- 验收：Task Contract S0～S5；最高状态为 `RUNTIME_PENDING`；
- 回滚范围：仅 `BruteForceSceneQuery`、self-check及关联 R4-COL-004A文档；未提交；
- 如果需要改变 immediate query、DAT/资源或 lifecycle，停止并新建独立包。

## 7. 实际验证

| 检查 | 实际结果 |
|---|---|
| C++ authority | 只读复核 `collision_collect.cpp:107-120,220-371`、`game_tick.cpp:496-575`和 `Makefile:10-31`；未运行、构建、修改或写入 authority。 |
| focused fixture | synthetic oid999 state3005/terminal target与 transition-semantic attacker均有有效geometry；`ForceBruteForce` / `ForceRoleAware`下 candidate count、target slot、itr index、RNG state/call count一致。 |
| Unity compile | 现有 Unity Editor / UnityMCP port 6401在 2026-08-22 04:33 +08:00 force scripts refresh/compile；domain reload期间的 TCP 关闭为预期，随后 Console `error CS` 查询为0。 |
| Full self-check | 通过菜单 `NTSD/验证/运行战斗运行时自检` 请求；`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 04:33:40 +08:00。 |
| 留痕 / diff hygiene | 待所有 R4-COL-004A状态、ledger与handoff更新后进行最终 validator与 `git diff --check`。 |

该结果只确认 frozen collection子范围；`QueryBodyHits` immediate query与C++ runtime/Play Mode仍不在本记录内。
