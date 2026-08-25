# R7-BROAD-01 — role-aware / Loose Quadtree recertification

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING / NO-CODE CONDITIONAL CERTIFICATION`

## 1. Scope

复核 C++ candidate pair discovery 与 Unity BruteForce、Loose Quadtree、role-aware formal collector。
优化只允许减少不可能产生candidate的pair，不得改变C++ authority pair顺序、双方向narrow phase、
candidate/RNG结果或pair-vRest时点。

## 2. C++ authority

- `src/entity/collision_collect.cpp:363-372`：slot `i=0..MAX-1`、`j=i+1..MAX-1`；
- 每个unordered pair先decrement pair-vRest，再依次collect `i→j`、`j→i`；
- `:242-359`：每个direction按current/collision frame ITR/BDY coarse prefilter，随后按ITR index、
  BDY index执行exact geometry和candidate acceptance；
- candidate顺序由authority slot pair、direction与ITR顺序共同决定。

## 3. Unity mapping

### BruteForce

`CollectCollisionCandidatesBruteForce`按runtime-slot roster的`i/j`顺序依次调用
`CollectCandidatesForPair(a,b)`、`CollectCandidatesForPair(b,a)`；exact collector继续按ITR index与BDY
geometry执行。pair-vRest由独立`TickCollisionPairVRestAll`预先处理，不能被broadphase过滤。

### Role-aware / Loose Quadtree

- participant保留authority ordinal；body bounds进入index，attack ITR bounds查询body index；
- invalid/degenerate attack/body进入conservative fallback pair；
- nested direct、sweep direct或tree只生成可能的unordered pair；
- pair keys按authority ordinal排序、去重；
- exact loop按sorted pair执行first→second与second→first，并复用相同candidate acceptance；
- occupancy/generation/geometry失败时formal collector abort，恢复RNG state/call count后完整回退brute；
- no-ITR fast path只在角色表证明无attack ITR时清空candidate，pair-vRest保持独立。

未发现新的candidate behavior source difference。

## 4. Fresh test evidence

| Job | Coverage | Result |
|---|---|---|
| `b5ea30da3c4e42468977e3ab10868fe6` | shadow / zero-ITR / conservative fallback | 9/9 PASS |
| `7798184d88024764971712a9a780029e` | formal role-aware：authority order、双方向、strict boundary、exact cache、fallback、RNG restore、generation、1000 synthetic、direct/sweep/tree、0 B | 58/58 PASS |
| `201e1b9127004d349b14b06df2aa4e6b` | LooseQuadtree nearest、mutation/reuse、1000 layout、participant buffer、0 B | 16/16 PASS |

合计：83/83 PASS，0 fail/skip。

运行上述tests后，同Editor域full self-check在`R3-INP-01` OID7/8→51 fixture连续失败；没有脚本变化，
触发Unity domain reload后，同一Assembly于2026-08-22 22:13:06 full self-check恢复PASS。判定为
EditMode跨测试静态状态污染，登记`D-TEST-001`；不能把污染失败写成broadphase gameplay回归，也不能
隐去验收隔离缺口。

## 5. Production deployment fact

- `GameConfig.asset` 的`BattleCollisionBroadphaseName`为空；
- `CollisionBroadphaseBackendResolver.Resolve`无override/配置时返回`BruteForce`；
- 普通`NTSD_Battle`因此没有默认使用LooseQuadtree；
- 1000 entity stress可显式选择LooseQuadtree，但不能代表production已部署。

登记`D-PERF-002`：性能优化部署/配置缺口，不是C++ gameplay difference。是否切换production默认必须在
独立配置合同、real battle parity与性能矩阵后决定，不能为了FPS直接改配置。

## 6. Result / boundaries

- role-aware/LooseQuadtree获得source + fresh Unity conditional certification，最高`RUNTIME_PENDING`；
- C++ runtime trace与真实Play Mode未取得；
- production backend仍为BruteForce；
- `D-TEST-001`必须在后续测试治理包定位具体未清理static owner；
- C++ authority、candidate规则、pass/RNG/slot/capacity与已批准Unity边界均未修改。

## 2026-08-23 D-TEST-001 closure

独立R7-TEST-001已把污染二分到AI shared-shadow fixture写入static `LF2FrameCache.EmptyFrame.state`后未恢复，
并关闭另一unified-refresh fixture对旧污染的隐藏依赖。final class 66/66、AI matrix 286/286，二者后同域
full self-check均PASS；fresh 03:07:32 PASS。该closure为test-only，不改变本Broadphase认证或production backend事实。
