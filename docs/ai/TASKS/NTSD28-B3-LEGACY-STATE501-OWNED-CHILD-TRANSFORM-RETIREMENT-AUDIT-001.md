# Task Contract — NTSD28-B3-LEGACY-STATE501-OWNED-CHILD-TRANSFORM-RETIREMENT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_STATE501_TRANSFORM / DIRECTION_B_GAMEPLAY_ZERO / RELEASE_GAMEPLAY_ZERO / HUD_RADAR_ONLY_TWO_TOKENS / UNITY_SYNTHETIC_BRANCH_CONFIRMED / PRODUCTION_RETIREMENT_DEFINED`
> 来源：`NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001`
> 依赖：`NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001 / VERIFIED`

## 目标

只读确认Unity early-frame `state==501` definition/owned-child transform是否存在于当前NTSD 2.8-Logan
playable live path及双方正式battle content，并冻结最小退休边界。重点判断`child.KillCount == owner physical slot`
扫描是否为Authority行为，不把旧test或HUD radar allowed-state token当作battle frame规则。

## 范围

- Authority：`SimulationTickDriver28::step()`、`BattleWorld28` definition/frame/lifecycle调用链、当前75-file
  source-capture子闭包，以及正式`resources/runtime/decoded_dat`。
- Unity：`BattleEarlyFrameAdvanceModule`、其production caller/diagnostics、focused/SelfCheck夹具，以及Direction-B
  normalized projection。
- 本包只修改治理文档，不修改C#、Scene、Prefab、Config、资源、ProjectSettings或Authority。

## 边界

- state500、native C05 teleport、8000..8999 encoded definition transition保持独立。
- CPoint `throwinjury==-1` self transform及其owned-child传播已有B6 throw合同明确排除，不能借本包处理。
- 11xx/12xx late-lifecycle child propagation是严格下一独立B3包。
- `TransformOriginalObjectId/TransformTargetObjectId` carrier与snapshot/schema不在本审计删除。

## 验收

1. 区分battle object frame state与`data/frame/INKHUD*.dat` radar allowed-state列表。
2. 记录current Direction-B与release gameplay frame state501计数、Authority source/live调用链结果。
3. 穷尽Unity production reader/writer、diagnostic和test owner，给出最小production退休包及RED矩阵。
4. 更新Ledger、STATE、handoff与总表；审计不能冒充runtime或full parity验证。

## 回滚

仅移除本Task/Change及摘要；没有运行行为、内容、Scene或Authority回滚。

## 审计结论（2026-09-09）

### Authority与content

- 当前75-file source-capture子闭包的`ntsd28_core/src`与`ntsd28_playable/src`对
  `state==501`、`case 501`及state/501近邻语义做穷尽文本复核，结果为0个production语义分支。
- `SimulationTickDriver28::step()`的C25逐slot definition入口调用
  `BattleWorld28::apply_definition_transition_slot()`；该函数只处理`8000 <= state < 9000`，target为
  `state-8000`，并显式把9996/8000000..9000000留给相邻分支。它不读501、不扫描owner或其他slot。
- Direction-B冻结`normalized-projection.tsv`的FRAME scalars中`state=501`为0；同口径`state=500`也为0。
- 正式release decoded树的精确行`state: 501`只有2条，分别位于`data/frame/INKHUD.dat:153`与
  `INKHUD2.dat:122`的`<radar>` allowed-state列表。它们由`native_frame_hud.cpp`写入
  `radar.allowed_states`，不是object catalog frame；因此release gameplay frame计数仍为0。

### Unity现状与差异

- 唯一production行为owner为
  `Simulation/Passes/EarlyFrameAdvance/BattleEarlyFrameAdvanceModule.cs`：它在fast handle path、fallback和
  forced-legacy path中采集501实体，解析`TransformTargetObjectId`，替换self definition/ObjectId/action，并扫描
  全部active entity；`child.KillCount == self physical slot && child.HP>0`时还替换child definition/ObjectId，
  按`YInt<0`选action212或0。
- `LF2States.RudolfTransform=501`是未被production调用的符号常量，不构成行为；
  `LF2StandardFrames.RudolfTransform=240`是action id且与本状态值无关。
- `EarlyFrameAdvanceOptimizationEditorTests.State501OwnerChildrenDeadAndMissingReplacement_MatchLegacy`与SelfCheck
  GT-04使用synthetic wrappers主动固化上述Unity-only行为；它们不能作为Authority证据。
- CPoint `throwinjury==-1` self/child transform是另一条入口，并已被B6 throw合同明确保持，不并入本包；
  11xx/12xx child scan也保持下一B3 route。

### 后继

唯一下一包为`NTSD28-B3-LEGACY-STATE501-TRANSFORM-RETIREMENT-001`：删除early-frame fast/fallback/legacy
state501采集与self/child mutation，保留state500、native teleport、8000..8999 definition transition、CPoint、
constant和所有carrier/schema。RED必须证明state501 self及slot/stable-id-marker children的identity/frame/runtime
bitwise不变，且state500与teleport回归不变。
