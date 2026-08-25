# R4-COL-04A — oid999 candidate-collection extra gate

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source、Unity compile、full self-check已通过；C++ trace / Play Mode待补。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R4。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> 对应差异：`D-COL-004` 的 candidate-collection 子范围。  
> 前置调查：`RESEARCH/R4-COL-04A-oid999-candidate-collection-preflight-20260822.md`。

## Goal

删除 Unity frozen candidate collection中 C++ release不存在的 oid999/transition-smoke全局排除。有效 ITR/BDY/
geometry的 oid999必须由既有 C++ kind/team/effect/select规则决定是否记录，而不能因表现/生成语义提前跳过。

## Scope

允许仅修改：

1. `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
   - `CandidateCollectionPairAllowed`；
   - `BuildRoleAwareFormalExactCommonCache` 的 `PairCollectionBaseAllowed`；
   - 不删除 helper，不修改 immediate `QueryBodyHits` 调用点。
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - 新增 synthetic oid999 attacker/target有效geometry fixture；
   - normal direct与可用 role-aware cached collection一致性断言。

禁止：

- 修改 C++、oid999 DAT/资源、`IsPureTransitionSmoke` immediate query调用、newborn/opoint、scheduler、
  render、pool/容量、D-COL-005、D-HIT、R5+。

## Authority / Evidence

### VERIFIED

- C++ `collision_collect.cpp:107-120,220-371` 无 oid999/transition global filter；
- Unity normal与role-aware collect均存在额外 `IsPureTransitionSmoke` gate；
- 当前 production data audit只证明被 gate 的 oid999 frames无有效 geometry，不证明 gate等价。

### UNKNOWN

- role-aware cached path是否能由现有 self-check稳定显式覆盖；
- immediate query两处 extra gate的 C++ route，明确不在本包。

## Files likely involved

| 文件 | 责任 |
|---|---|
| `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs` | normal/role-aware candidate base gate。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | synthetic oid999 valid-geometry collection matrix。 |
| `docs/ai/CHANGE-RECORDS/R4-COL-004A.md` | 改动、验证、未关闭项与回滚记录。 |

## Deliverables

1. 两条 frozen collection base path去除 extra smoke gate；
2. oid999 attacker/target synthetic valid-geometry fixture；
3. Unity scripts refresh/compile、full self-check、ledger validator、diff check真实证据；
4. Change Record、ledger、STATE、diff register、主计划和handoff更新。

## Verification

| 层级 | 验收条件 |
|---|---|
| S0 source | C++ collect无 oid999 global filter；Unity仅移除对应 collection extra gate。 |
| S1 target | state3005/terminal oid999 target有有效BDY时，normal attacker候选被记录。 |
| S2 attacker | transition-semantic oid999 attacker有有效ITR时，normal target候选被记录。 |
| S3 parity | normal direct和role-aware cached可用时的 candidate slot/itr/order/RNG一致。 |
| S4 regression | current production-data geometry audit、Unity compile、full self-check、ledger validator、diff check通过。 |
| S5 boundary | 最高 `RUNTIME_PENDING`；immediate query、Play Mode/C++ trace保持未关闭。 |

## Stop conditions

- 必须修改 oid999 DAT、资源、newborn/opoint/scheduler或 immediate query才可建立 candidate fixture；
- role-aware coverage需要长期 collector架构改造；
- source发现 C++ release其实在未读取的 live prefilter中同样按 transition semantic排除；
- 需要回退 CentralOnly/Texture2DArray、容量、30Hz、FrameInputSet、SoA/pool。

## Out of scope

R1-WP02、C++ executable、oid999 immediate query、D-COL-005、全部D-HIT、R5～R8、T8 default `stage.dat`、
服务器、Android、长时间性能与Play Mode。

## 实施进度（2026-08-22）

- 已从 normal `CandidateCollectionPairAllowed` 与 role-aware exact common cache的
  `PairCollectionBaseAllowed` 移除 `IsPureTransitionSmoke` 额外排除；helper与 immediate query两处调用未动。
- self-check使用 synthetic valid geometry覆盖两种原本会被 helper排除的 oid999：state3005/terminal target
  与 `SpawnSemantic=TransitionEffect` attacker；每种均在 `ForceBruteForce` 和 `ForceRoleAware`
  formal collector下比较候选 count、target slot、itr index及 RNG state/call count。
- 2026-08-22 04:33 +08:00：现有 Unity Editor（UnityMCP port 6401）force scripts refresh/compile后，
  Console `error CS` 查询为0；`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入
  04:33:40 +08:00。
- immediate `QueryBodyHits` 的 oid999 helper usages仍为独立未处理差异；本包保持 `RUNTIME_PENDING`，
  不将 Unity regression扩大为 C++ runtime trace或Play Mode对齐。
