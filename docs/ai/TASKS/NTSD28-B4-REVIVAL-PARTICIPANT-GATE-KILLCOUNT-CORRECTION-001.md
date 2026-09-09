# Task Contract — NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001

> 状态：`VERIFIED / RED_10_OF_16 / FOCUSED_16_OF_16 / RELATED_34_OF_34 / TARGETED_PLAY_20_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / QUEUED_ROUTE_NEXT`
> 依赖：`NTSD28-B0-DIRECT-REVIVAL-DEFAULTS-PRODUCTION-001 / VERIFIED`、
> `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-OWNER-AUDIT-001 / VERIFIED`

## 目标

只修正B4 revival route 2：退休Unity两个旧state14 frame-arm入口中的KillCount/team/transient推断，保留
canonical C25h arm；移除C07的KillCount/team门，并按Authority的`lives`、`nextHP`和physical slot重排
queued continuation、terminal transient removal与primary retention、normal revival三分支。

## Authority与Unity原状

- Authority `advance_reaction_timers_slot()`在body eligible、type0、slot `0..19`、HP<=0、state14、
  `revive_lives_30c>1`且toward-zero后的render phase<1时写30；不读KillCount或battle group。
- Authority `advance_native_revivals()`入口只要求active、HP<=0、state14、render phase `1..4`。随后先判
  `lives<2`：`nextHP>0`进入queued continuation，否则仅删除slot>19；`lives>=2`始终进入normal revival。
- Unity `LF2Character.ApplyObjectSpecificFrameTickBeforeWaitAdvance()`与
  `LF2Entity.RunNativeC25FrameBodyForWorldPass()`的direct compatibility分支各自按
  KillCount/team5/transient slot提前arm；`BattleRespawnModule`又重复同类gate，并按nextHP先选branch，
  normal helper还会删除`lives<2`的primary。

## 允许修改

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalParticipantGateCorrectionEditorTests.cs`及`.meta`
- 必要时只纠正既有C07 focused fixture的branch前置，不改变其pass-order目标。
- 本Task/Change、Ledger、STATE、handoff、CURRENT-AUTHORITY与新对齐总表。

## 不变量与排除

- 不改canonical C25h `AdvanceNativeReactionAndStatusTail()`的条件、toward-zero顺序或timer ownership。
- 不实现queued continuation的controller/group/visual精确字段；不修改OID998细节。
- 不修normal revival的physics floor、RNG range/offset、position或HP/MP exact差异。
- 不新增/删除revival carrier，不改snapshot/checksum schema、content、Scene、Prefab、ProjectSettings、Authority、
  tick/pass顺序或对象容量。
- terminal `lives<2 && nextHP<=0`只删除slot>19；slot0..19保持active并留给result handling。

## Test-first验收

1. C25 production矩阵同时覆盖DataOriented/Legacy：slot0/19、KillCount -1/999、group1/5且lives2均arm30；
   transient slot20或lives1不arm。direct compatibility只衰减timer，不自行模拟canonical C25h arm。
2. C07矩阵覆盖KillCount -1/999、group1/5、slot0/19/20、lives1/2、nextHP0/80、render0/1/4/5：
   gate与三分支结果完全由Authority字段决定。
3. 先运行新增测试得到精确RED，再作最小production修改；随后运行focused、新旧C25/C07回归、build、
   SelfCheck、目标Play、Console/Scene与Change Ledger validator。
4. 本包只证明gate/branch；queued细节、normal floor/RNG及B4 exit trace继续保持后继状态。

## 回滚

只恢复本Change删除的两个旧frame-arm块、C07额外gate与旧branch选择；删除新增focused fixture并恢复相应
SelfCheck断言。不得回退B0 direct默认producer或canonical C25h实现。

## RED证据（2026-09-09）

- Unity focused v1实际结果为`passed=10 / failed=6 / total=16`。
- 两个legacy frame入口分别把预期0写成30。
- C07的KillCount=-1/group1 queued case被额外gate拒绝；normal的同类case也未执行。
- lives2/nextHP80在group5时错误进入queued并生成effect；primary slot19/lives1/nextHP0被错误释放。
- C25 DataOriented/Legacy canonical arm、transient/lives gate、C07 transient free与render0/5边界均先行通过。

## 实施与验证（2026-09-09）

- `LF2Character.ApplyObjectSpecificFrameTickBeforeWaitAdvance()`与
  `LF2Entity.RunNativeC25FrameBodyForWorldPass()`的direct compatibility分支只删除旧HitStun30写入，
  保留state14 dead的`AttackingCounter=0`。
- `BattleRespawnModule`删除C07 KillCount/team gate；caller改为lives-first：`lives<2 && nextHP>0` queued、
  `lives<2 && nextHP<=0 && slot>19` free、同条件primary保留、`lives>=2` normal。normal helper不再自行
  删除low-life primary。
- 03:27:06 +08 focused v1为`16/16`；同一Unity Test Runner job合并新增fixture、既有C25 timer-owner与
  C07 placement，结果`34/34`。
- fresh `Assembly-CSharp.csproj`为0 error/47 warnings；`Assembly-CSharp-Editor.csproj`为
  0 error/104 warnings。
- 03:28:51 +08真实`NTSD_Battle` Play通过20个逻辑case，覆盖两种C25 profile与C07四分支；Console error=0。
  Play前后Scene dirty=false/root=13，SHA-256均为
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- 03:30:09 +08 full SelfCheck仍在更早独立CPoint mode0 victim-Vz断言停止，未到本包新增SelfCheck；该失败
  不记为本包失败，专项EditMode/Play已直接覆盖目标行为。
- 旧`BattleDeathRespawnAiIntegerPlayModeProbeEditor`仍是R8历史综合probe，包含后继normal floor/RNG与
  synthetic direct-countdown假设，本包未运行也未把它作为证据；其2.8 replacement/retirement归B4 exit。
- 本包只关闭participant gate与branch ownership；下一严格route为
  `NTSD28-B4-REVIVAL-QUEUED-CONTINUATION-PRODUCTION-001`。
