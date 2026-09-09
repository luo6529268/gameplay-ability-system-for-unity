# Task Contract — NTSD28-B4-REVIVAL-QUEUED-CONTINUATION-PRODUCTION-001

> 状态：`VERIFIED / RED_4_OF_11 / FOCUSED_13_OF_13 / RELATED_47_OF_47 / TARGETED_PLAY_13_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / NORMAL_ROUTE_NEXT / PRODUCER_B7_H_PENDING / SCHEMA_DEFERRED`
> 依赖：`NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001 / VERIFIED`

## 目标

只闭合Authority state14 queued continuation：在`lives<2 && nextHP>0`时先验证`+0x360` controller，随后按
原顺序消费queued lives/HP、复制controller battle group、选择并发布revive visual、写action219/frame counter0/
motion hold10，并保持既有同C07 OID998 action6生成合同。

## Authority合同

- `BattleWorld28::advance_native_revivals()`先读取`ai_target_slot_360`。值为`-1`时改读physical slot1；若
  slot1 inactive，native固定pool backing的group为0，continuation仍执行。
- 非`-1`controller必须是范围内active slot；缺失时整个queued mutation deferred，queued字段与实体状态均不消费。
- 成功顺序：`lives=nextLives`、MP=0、base/effective/current HP=nextHP、清nextHP/nextLives、
  `battle_group=controller.group`。
- visual先读`revive_visual_id_184`；小于1时OID30..36或39取140，OID37取114。仅visual>0时同时写
  `revive_visual_runtime_318`和`revive_visual_runtime_180`。
- 最后写action219、frame counter0、motion hold10；tick driver随后生成OID998/action6，位置Z+1、同group、
  facing=false、motion=0。生成不可用不回滚机械continuation。

## Unity原状与允许修改

- `Runtime.Unk360`已是+0x360 carrier；`RelationTeam`是battle group；`RenderPicOffset`是+0x318；
  `HP2Orig/HPOrig/RespawnCount`分别是lives/nextLives/nextHP；`AttackingCounter`是frame counter；
  `FrameDelay`是motion hold。
- 当前queued helper不检查controller，固定group=1，只为OID30..36写140，缺OID37/39与explicit visual，
  也没有+0x184/+0x180独立runtime carrier。
- 允许修改：
  - `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs`
  - `Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs`
  - `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
  - `Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalQueuedContinuationProductionEditorTests.cs`及`.meta`
  - 必要的既有C07 fixture与本Task/Change、Ledger、STATE、handoff、CURRENT-AUTHORITY、总表。

## 不变量与排除

- 不改变route2 gate/lives-first branch、terminal primary/transient或normal revival。
- 不修改normal floor/RNG/HP/MP规则，不改pass顺序。
- 新`ReviveVisualId184`与`ReviveVisualRuntime180`只补runtime carrier、CopyTo与Reset；不在本包变更
  lockstep/checksum/parity公开schema。+0x318继续使用既有`RenderPicOffset`。
- OPoint `join/join_reserve/join_pic`生产数据仍缺，归B7/H；本包只闭合consumer并用synthetic carrier验证。
- 不改content、Scene、Prefab、ProjectSettings、Authority或对象容量。

## Test-first验收

1. RED覆盖explicit active controller group、explicit missing/out-of-range atomic defer、`-1` fallback slot1 active/inactive。
2. visual覆盖explicit77、OID30/36/39→140、OID37→114、OID38/other no-write；成功与defer均验证queued字段、
   action/counter/hold及spawn次数。
3. 新carrier验证CopyTo/Reset；既有C07/OID998 placement回归必须保持。
4. 运行focused、相关C07/B0 runtime-field回归、build、SelfCheck、目标Play、Console/Scene与Ledger validator。
5. 本包不宣称queued producer已可由当前content生成，也不宣称normal revival或B4 exit完成。

## 回滚

恢复queued helper的旧固定group/visual逻辑，移除两个新runtime carrier与新增tests/SelfCheck；不得回退route2
gate/branch修正或C07 placement。

## RED证据（2026-09-09）

- focused v1实际`passed=4 / failed=7 / total=11`。
- explicit active controller与`-1` fallback active/inactive分别暴露固定group=1；explicit missing/out-of-range
  均错误消费queued字段并调用spawn，而非atomic defer。
- OID39应140、OID37应114但均保留旧23；OID30/36→140及OID38/44 no-write已先行通过。

## 实施与验证（2026-09-09）

- `NTSDEntityRuntime`新增独立`ReviveVisualId184`与`ReviveVisualRuntime180`，并纳入canonical
  `TryCopyCanonicalStateTo()`和`Reset()`；+0x318继续使用既有`RenderPicOffset`，公开checksum/parity
  schema未改。
- queued helper现于任何mutation前读取`Runtime.Unk360`：explicit slot必须active；`-1`投影physical slot1，
  slot1 inactive时返回group0且继续。成功后按Authority顺序消费queued lives/HP、复制controller group、
  应用explicit/default visual，再写action219、AttackingCounter0与FrameDelay10。
- visual矩阵精确覆盖explicit77、OID30/36/39→140、OID37→114、OID38/44保留旧+0x318/+0x180。
  spawn callback证实机械state与group/visual/action/counter/hold在OID998调用前已完成；既有OID998实现未改。
- 03:51:59 +08 focused v1为`13/13`；新增queued、route2 gate、C07 placement、C25 timer-owner和runtime
  snapshot联合回归为`47/47`。
- fresh `Assembly-CSharp.csproj`为0 error/47 warnings；`Assembly-CSharp-Editor.csproj`为
  0 error/104 warnings。
- 03:54:08 +08真实`NTSD_Battle` Play通过13个逻辑case；Console error=0。Play前后Scene dirty=false、
  root=13，SHA-256保持
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- 03:54:53 +08 full SelfCheck仍提前停在既有CPoint mode0 victim-Vz断言，未到本包新增SelfCheck；专项
  EditMode/Play直接覆盖本包目标。
- OPoint `join/join_reserve/join_pic`生产入口仍缺并明确留B7/H；carrier checksum/parity schema按合同后置。
  下一严格route为`NTSD28-B4-REVIVAL-NORMAL-FLOOR-RNG-PRODUCTION-001`。
