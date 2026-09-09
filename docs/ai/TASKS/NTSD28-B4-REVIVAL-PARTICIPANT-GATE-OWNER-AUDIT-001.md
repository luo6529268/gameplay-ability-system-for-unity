# Task Contract — NTSD28-B4-REVIVAL-PARTICIPANT-GATE-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / KILLCOUNT_NOT_REVIVAL_AUTHORITY / THREE_ENTRY_POINTS_SPLIT / DIRECT_DEFAULT_PRODUCER_MISSING / FOUR_RUNTIME_ROUTES_DEFINED / PRODUCTION_HELD`
> 来源：`NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001`
> 依赖：`NTSD28-B3-LEGACY-1100-1299-CHILD-PROPAGATION-RETIREMENT-001 / VERIFIED`

## 目标

只读闭合Authority revival的参与者、计时、branch和字段合同，穷尽Unity三个KillCount/team/slot gate，确认
production producer与后继依赖，防止把“删除KillCount条件”误当成完整revival对齐。

## Authority合同

### C25h render-phase arm

`advance_reaction_timers_slot()`先将`render_phase_008`向0移动，再仅在以下条件全部满足时写30：

- body未因negative relation或non-type3 motion hold跳过；
- current definition type=0、physical slot `0..19`；
- HP<=0、current frame state14；
- `revive_lives_30c>1`且移动后的`render_phase_008<1`。

它不读owner、ordinary credit gate或battle group。Unity的canonical C25 tail已经实现这组条件。

### C07 revival

`advance_native_revivals()`首门只要求active、HP<=0、render phase `1..4`、current state14；随后严格分支：

1. `lives<2 && nextHp>0`：queued continuation；消费next lives/HP、controller slot/group、visual，action219，
   motion hold10，并在tick driver生成OID998 action6。
2. `lives<2 && nextHp<=0`：slot>19删除；slot0..19保留给result handling。
3. `lives>=2`：必须有本slot physics floor；先减life，再按同group type0 peers和`sumX!=0`决定两次同步RNG，
   恢复MP/HP、render phase20、action212、Y=floor、Vy=0。

不读KillCount或team5特殊门。normal revival即使nextHp>0也优先走lives分支，不得被queued branch抢占。

## Unity现状

- `LF2Character.ApplyObjectSpecificFrameTickBeforeWaitAdvance()`以`KillCount>=0 || relationTeam==5 || slot>=20`
  提前写HitStun30；production C25 tail随后会把它减成29，污染本来已正确的canonical arming。
- generic `LF2Entity.RunNativeC25FrameBodyForWorldPass()`在非native直调兼容路径复制同一错误条件；production
  native C25虽绕过它，仍是错误兼容行为。
- `BattleRespawnModule.PassesRespawnGate()`在state14/HP/render1..4后又以slot<20且KillCount<0且group!=5拒绝；
  Authority无此条件。
- caller按`RespawnCount<=0`而非`lives<2 && nextHp>0`选branch；no-count分支对lives<2无条件Free，
  会错误删除primary slot；stored branch忽略lives>=2优先级。
- normal branch仍有独立差异：legacy RNG范围`51/31`和offset`-26/-16`，Authority为mod51/mod31后
  `-25/-15`；Unity Y固定-300而非本slot physics floor。queued branch还缺+0x360 controller/group精确处理。

## Reachability与producer

- Direction-B normalized projection含235个state14 FRAME，规则可达。
- 当前direct `PlayerSlotConfig/MatchConfig/AppManager/BattleMatchConfigRuntimeAdapter`没有revival字段；pool reset
  把`HP2Orig/HPOrig/RespawnCount`置0，而Authority playable/scenario direct participant默认分别为`1/0/0`。
  生产写入者除respawn自身外为0，所以必须先补direct default producer。
- Unity ObjectPoint缺Authority reserve/join/join_reserve数据合同；stage/content producer归B7/H，不能借本B4
  gate包扩展内容schema。当前T8默认stage资产部署仍暂缓。

## 严格后继路线

1. `NTSD28-B0-DIRECT-REVIVAL-DEFAULTS-PRODUCTION-001`：direct participant在首次注册快照前写默认`1/0/0`，
   不新增UI/Scene字段。
2. `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001`：删除两个旧frame arm与C07 KillCount/team
   gate，按physical slot/lives/queued HP重排三分支；保留已正确canonical C25 arm。
3. `NTSD28-B4-REVIVAL-QUEUED-CONTINUATION-PRODUCTION-001`：controller/group/visual/action219/hold/OID998精确闭合。
4. `NTSD28-B4-REVIVAL-NORMAL-FLOOR-RNG-PRODUCTION-001`：floor、RNG callsite/range、position和HP/MP exact写入。
5. `NTSD28-B4-REVIVAL-EXIT-AUDIT-001`：同seed/tick双端trace、direct/transient/lives/queued/slot reuse/Play出口。

Route1通过前不得实施B4 gate；route2/3不得被route1结果冒充完成。OPoint/stage revival producer继续回链B7/H。

## 回滚

本审计只修改治理文档；回滚移除Task/Change与摘要，不涉及code/content/Scene/Authority。
