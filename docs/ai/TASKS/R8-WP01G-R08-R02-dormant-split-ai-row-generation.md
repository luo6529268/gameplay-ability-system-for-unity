# R8-WP01G-R08-R02 — dormant split AI unified-row generation repair

> 建立日期：2026-08-24  
> 状态：`VERIFIED / UNITY S4`  
> Change ID：`R8-AIROWGEN-001`  
> Blocker：`B-R8-R08-03 / CLOSED`

## Goal

修复OID7/8→51在完整4500 tick cooldown结束、dormant partner按原slot/generation恢复时，
`partner.Reset()`向relation/link store发布字段变更却命中已排除或过期的AI unified row，导致
`ValidateRow`抛出stale slot generation异常的问题；保持C++ split字段/顺序与已批准Unity
dormant-slot adapter的可观察结果不变。

## Scope

### 允许

1. 只读复核C++ `game_tick.cpp:1098-1153`的split reset/reactivate/字段顺序；
2. 建立最小focused fixture，复现“同一handle generation、dormant从active AI row退出、reset字段写入、原slot恢复”；
3. 检查`BattleRelationLinkStore`、`BattleAiUnifiedRowPublisher`和runtime store绑定生命周期；
4. 选择最小生命周期修复，使reset期间不向不存在/过期的unified row发布，恢复后以原generation重新建立current row；
5. 修改`SimulationWorld.Passes.partial.cs`及确有必要的store/publisher边界和focused tests；
6. fresh compile、focused generation/lifecycle tests、full self-check、R08完整Play、Console0、ledger validator。

### 禁止

- 不修改、运行、构建或写入C++ authority；
- 不改变OID7/8/51的merge/split gate、HP、frame、cooldown、slot或generation结果；
- 不通过关闭`ValidateRow`、吞异常、全局禁用AI publisher或给OID51硬编码绕过；
- 不推进dormant partner generation，不释放其low slot，不改变allocator；
- 不修改DAT、CentralOnly/Texture2DArray/URP、pass order、30Hz、FrameInputSet、worker、容量或AI决策策略；
- 不处理T8、R1-WP02 full trace、IL2CPP、Android、服务器或其他角色技能。

## Authority / Evidence

- C++ release source：`src/entity/game_tick.cpp:1098-1153`；
- Unity first difference：`SimulationWorld.TrySplitOid51BackToPair`调用`partner.Reset()`；
- 异常链：`Runtime.RelationTeam → BattleRelationLinkStore.CaptureChangedField →
  BattleAiUnifiedRowPublisher.PublishRelationLink → ValidateRow`；
- 真实R08报告：4500 tick推进至tick4853时抛异常，merge runtime与Central dormant证据已先通过；
- Evidence：C++字段顺序`VERIFIED(source)`；Unity异常`VERIFIED(runtime)`；最小修复形态`UNKNOWN`。

## Files likely involved

- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`；
- `Assets/NTSD/Scripts/Simulation/Ecs/BattleRelationLinkStore.cs`；
- `Assets/NTSD/Scripts/Simulation/Ecs/BattleAiUnifiedRowPublisher.cs`；
- `Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs`的两个最小focused lifecycle/generation用例；
- `BattleOid5152MergeSplitPlayModeProbeEditor.cs`仅用于重跑，不在本repair中放宽断言。

## Unknowns

1. vital/frame-motion/input stores是否会在修复relation/link后暴露同类reset写入问题；
2. row-membership invalidation后，本tick后续非AI pass是否存在必须读取旧unified row的隐藏消费者；当前静态调用链未发现，
   仍需focused与完整R08证明。

## Read-only preflight closure（2026-08-24）

### 已确认原因

1. `CharacterInputAll`构建并激活`BattleAiUnifiedRowPublisher`，该publisher不会在CharacterInput finally结束；它被保留
   用于同tick后续字段增量，并通常在下一次prepare时roll-forward；
2. unified snapshot只把`IsActiveForCurrentPass`实体标为Included，`OidMergeDormant=true`会排除partner；
3. dormant adapter没有释放runtime slot/generation，也没有unbind frame-motion/relation-link/vital/input stores；这是保留
   原handle与镜像字段的既有设计；
4. final split发生在CharacterInput之后的RuntimeMaintenance。当前publisher active，但partner不在Included row；
   `partner.Reset()`首个relation字段变化经store携带原generation发布，`ValidateRow`因`included=false`而fail-fast；
5. 所以“stale generation”是current-row membership失配，不是slot generation已错误递增。

### 推荐最小修复

1. 在publisher增加语义明确的`InvalidateAfterRowMembershipChange()`，实现仍为结束当前pass；
2. 既有`InvalidateAfterOccupancyChange()`复用该实现，保持register/release行为不变；
3. merge在partner进入dormant前触发row-membership invalidation，防止下一tick错误roll-forward仍包含partner；
4. split在`partner.Reset()`前触发同一invalidation，使四类store继续更新原generation镜像，但不向已失效快照发布；
5. 下一tick`TryRollForward...`因publisher inactive必然失败并执行完整snapshot rebuild，恢复后的partner以原generation
   重新进入Included row。

不推荐临时unbind/rebind全部store：它扩大到四类owner绑定事务，容易出现部分store镜像缺字段或重绑顺序差异；
不推荐修改`ValidateRow`为静默忽略：会掩盖真正的current-row错误；不推荐推进generation或release/reclaim slot：
会破坏已批准dormant adapter。

## Deliverables

1. 更新后的`R8-AIROWGEN-001` Change Record；
2. 一个先失败后通过的focused lifecycle/generation测试；
3. 最小production修复；
4. fresh compile、focused、self-check与R08完整报告；
5. Ledger/STATE/main plan/handoff同步。

## Verification

1. focused A：unified authority active时merge使publisher失效；下一tick必须full rebuild且dormant partner不在Included；
2. focused B：unified authority active且partner已dormant时，split reset/reactivate不抛异常、原handle generation仍可解析；
3. focused C：split后的下一tick必须full rebuild、partner以原generation重新进入Included、published state验证通过；
4. existing occupancy invalidation、roll-forward、publisher/store/generation回归全部PASS；
5. full self-check须新增实际unified-authority split覆盖并PASS；
6. R08自然推进4500 tick，split恢复OID7/8原slot/generation、post-fixture ObjectCount和Central bodies；
7. cleanup恢复world/slot/pool/driver，Console0；
8. `Tools/Validate-ChangeLedger.ps1` PASS。

## Stop conditions

- 需要改变slot generation、allocator、AI决策架构或全局publisher容错语义；
- 修复relation/link后出现另一store的独立first difference；
- 需要修改OID split gameplay字段/顺序或C++ authority；
- focused无法独立复现，且必须扩大到无关系统；
- 用户提出新的Change Request。

## Out of scope

AI行为树/状态树改造、通用AI性能、其他lifecycle、DAT/资源、渲染架构、T8、IL2CPP、Android、服务器、
C++ executable/full trace。

## Authorization

用户已于2026-08-24明确批准执行`R8-WP01G-R08-R02 / R8-AIROWGEN-001`并恢复目标。授权仅覆盖本Task中的
focused reproduction、通用row-membership invalidation最小修复与既定验收；不授权扩大到AI策略、generation、
allocator、DAT、render、T8或其他模块。

## Completion evidence（2026-08-24）

- production：通用row-membership invalidation已在merge进入dormant前和split reset前接线；generation、allocator、
  store binding、ValidateRow与AI策略未改；
- compile：fresh force-all，Editor DLL晚于source，Console error=0；
- focused：merge/split2/2、unified authority21/21、live-slot/0-GC37/37 PASS；
- self-check：实际运行，但在目标检查前被独立`R-HC-01` geometry审计BLOCKED；未伪装为PASS；
- direct Play：`Temp/NTSD_R8_WP01G_R08_Oid5152MergeSplit.result.json` status PASS；4500 tick、原slot/generation、
  当前HP/HPMax各半、Central dormant/split与generation-safe cleanup全部通过；
- `B-R8-R08-03`关闭。Position38既有AI fixture失败、R-HC-01和R1-WP02 full trace均保持独立，不属于本包。
