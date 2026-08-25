# R8-AIROWGEN-001 — dormant split AI unified-row generation lifecycle repair

<!-- CHANGE-RECORD
id: R8-AIROWGEN-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1098-1153 / R8-WP01G-R08-R02
evidence: CPP-RELEASE-SOURCE / FOCUSED-2-21-37-PASS / R08-4500-TICK-PLAY-PASS
-->

> 创建日期：2026-08-24  
> 最后更新：2026-08-24  
> 类型：battle / lifecycle / ECS adapter

## 1. 状态与范围

- 当前状态：`VERIFIED / UNITY S4 / FULL SELF-CHECK BLOCKED BY UNRELATED R-HC-01`；
- 所属Work Package：`R8-WP01G-R08-R02`；
- 关联：`R8-MERGESPLIT-001 / VERIFIED`；`B-R8-R08-03 / CLOSED`；
- 不属于本次：AI策略、其他OID、DAT、render、T8、IL2CPP、Android、服务器、C++ full trace。

## 2. Authority / 需求依据

- C++ release `game_tick.cpp:1098-1153`在cooldown归零且frame离开9..260 gate后，原位恢复self，并reset/reactivate
  partner原slot；HP/HPMax平分、frame112、position/facing/team与ObjectCount顺序由该live path定义；
- Unity approved adapter保留dormant partner的原slot/generation，表现与query排除但allocator不可复用；
- R08真实运行已在4500th cooldown tick进入split，并于`partner.Reset()`首次relation field写入抛出stale row异常；
- Evidence：authority source为`VERIFIED`，Unity first difference为`VERIFIED(runtime)`，修复方案仍`UNKNOWN`。

## 3. Unity 原状与已确认差异

- `TrySplitOid51BackToPair`直接对仍绑定各SoA store的dormant partner执行`Reset()`；
- `NTSDEntityRuntime.Reset`通过属性setter发布relation/link等字段；
- dormant partner已不在当前`BattleAiUnifiedRowPublisher` included row中，但`BattleRelationLinkStore`仍保留原generation；
- `CaptureChangedField`无条件调用publisher，`ValidateRow`因row非current而抛异常；
- 异常中断split与probe cleanup，报告final object/pool未恢复；退出Play后由Unity域重载清理，不能当作验收通过。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `SimulationWorld.Passes.partial.cs` | `TrySplitOid51BackToPair` | 直接reset dormant partner | 使用明确的lifecycle store边界完成原generation reset/reactivate |
| `BattleRelationLinkStore.cs` | bind/release/capture lifecycle | 已绑定owner字段变化无条件发布 | reset事务内保持store正确且不发布到不存在的unified row |
| `BattleAiUnifiedRowPublisher.cs` | current-row验证 | stale row fail-fast | 保持fail-fast；仅在必要且有证据时增加明确lifecycle API，禁止静默忽略 |
| `Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs` | focused tests | 只有occupancy epoch回归，无OID5152 membership覆盖 | merge不得roll-forward dormant；split active snapshot下不抛异常且原generation恢复 |

## 5. 不可回退边界

- 不关闭或削弱`ValidateRow` fail-fast；
- 不增generation、不释放dormant slot、不改变allocator；
- 不改merge/split gameplay字段、pass order、30Hz、FrameInputSet、CentralOnly、容量、pool/worker/0GC；
- 不用OID51条件分支绕过通用lifecycle合同。

## 6. 实际改动

production尚未写入。已先在
`Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs`新增两个focused reproduction：

1. `UnifiedAuthority_Oid5152MergeInvalidatesMembershipBeforeNextRollForward`：要求merge后下一input pass不能roll-forward
   旧Included rows，必须full rebuild；
2. `UnifiedAuthority_Oid5152SplitReactivatesOriginalGenerationWithoutStaleRow`：先构造正式字段合同的OID51+dormant
   partner，激活排除partner的unified snapshot，再要求split不抛异常、原handle generation仍解析、下一tick full rebuild。

两个测试都走真实`SimulationWorld`、runtime slot/store/publisher与`Oid5152RuntimeMaintenanceAll`；只在fixture写初始
状态，不调用private merge/split helper。下一步fresh compile后运行这两个用例，保存旧production失败证据，再写修复。

### 旧实现 focused baseline

- fresh compile：Editor DLL晚于test source，Console `error CS`=0；
- job `aebfc0fa94ad4b3bac8d2b0230aee229`：2/2按预期未通过；
- split用例精确复现R08相同异常链：`partner.Reset → relation/link store → unified publisher.ValidateRow` stale row；
- merge用例在目标断言前失败为`expected OID51 / actual OID7`，说明CharacterInput改变了merge初始前置，尚未形成有效
  membership baseline。下一test-only修正只在snapshot建立后重新固定允许的frame/HP/team/position fixture，再运行旧实现；
  不改production、不直接写merge结果。

### 只读预检结论

- runtime slot和四类store generation没有被dormant流程推进或释放；
- publisher从CharacterInput保持active到RuntimeMaintenance，partner因dormant不在当前Included row；
- 现有`InvalidateAfterOccupancyChange()`会`EndPass()`；下一tick roll-forward显式要求publisher active，所以失效后会
  自动走完整rebuild。这是register/release已经使用的生产安全边界；
- 推荐把该能力抽成`InvalidateAfterRowMembershipChange()`，merge进入dormant前与split reset前各调用一次；
- store保持绑定并更新原generation镜像，publisher inactive时publish方法自然no-op；下一tick完整rebuild重新纳入partner；
- 不采用store全量unbind/rebind、不吞`ValidateRow`、不增generation、不release slot。

### Production实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `BattleAiUnifiedRowPublisher.cs` | `InvalidateAfterRowMembershipChange` | 将既有EndPass失效语义命名为row membership边界；occupancy API复用它 | publisher inactive；下一tick禁止roll-forward并full rebuild |
| `SimulationWorld.Passes.partial.cs` | `TryMergeOid7Or8Into51` | partner进入dormant前失效current row set | 防止下一tick沿用仍包含partner的Included rows |
| `SimulationWorld.Passes.partial.cs` | `TrySplitOid51BackToPair` | dormant partner Reset前失效current row set | 四类原generation store可继续更新，但不向已失效快照发布 |
| `AiDecisionSoAShadowEditorTests.cs` | 两个OID5152 focused tests | merge membership和split original-generation完整覆盖 | test-only |

未修改`ValidateRow`、store bind/release、slot generation、allocator、AI decision、DAT、render或pass order。

### 当前编译外部阻塞

全资源refresh发现与本Change无关的S0 HOLD文件
`InProcessLockstepAuthoritySessionEditorTests.cs:168/175`存在两条既有CS0019：`int % SimulationInputButtons`。
本Change不越权修改S0；因此最新R02 source尚未进入Editor DLL，compile/focused/R08验收暂时被外部错误阻塞。

该外部阻塞随后以`S0-INPROC-AUTHORITY-001`既有Record下syntax-only括号修复关闭；S0行为/HOLD未变。fresh
force-all后`Assembly-CSharp-Editor.dll`晚于R02 test source，Unity Console全部error=0；本Change推进为
`COMPILE_PASS`。focused/self-check/R08仍待执行。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | Unity force-all；Editor DLL `01:28:44Z`晚于test source；Console全部error=0 | PASS | `PASS` |
| focused | merge/split2/2、unified authority21/21、live-slot/0-GC37/37 | 全部PASS | `PASS` |
| self-check | `BattleRuntimeSelfCheck` request，结果`2026-08-24T01:33:25Z` | 在OID5152前被独立`R-HC-01` deployable geometry风险阻塞 | `BLOCKED / UNRELATED` |
| Play | R08 4500-tick merge→dormant→split | merge/dormant/split、原slot/generation、Central双body与cleanup全部通过 | `PASS` |
| C++ authority | `game_tick.cpp:1098-1153`只读source | split字段/顺序已闭合 | `VERIFIED(source)` |
| full trace | R1-WP02 | 未获得 | `BLOCKED` |

修正：首次focused test source曾成功编译并运行，job `aebfc0fa94ad4b3bac8d2b0230aee229`精确复现split stale-row；
merge fixture随后修正。production代码写入后初次被上述S0 CS0019阻塞，syntax-only修复后fresh compile现已PASS。

focused修复后证据：

- job `acda2ca1d5ae4893b29b24859e152ea9`：新增merge/split 2/2 PASS；
- job `19a60c8b21a64ec88c3050be9321a815`：unified authority/roll-forward/occupancy/发布回归21/21 PASS；
- job `f236bb87021c49f18287b303a49eaa79`：CharacterInput live-slot/0-GC回归37/37 PASS；
- 扩大整类job `fb28b78df3ea4064837360822dffd0d3`为104 pass/1 fail，唯一失败
  `DataOrientedProfile_MatchesLegacyFullDispatcherForPosition38`；独立job
  `08d8b3ecaaa9460db5e2d3e0da74a8b5`仍同样失败，且该fixture不经过OID5152 merge/split或membership invalidation，
  记录为既有独立AI fixture失败，不纳入本Change修复或PASS依据。

full self-check已实际运行；首个失败为`CheckDeployableResolvedGeometryRisks`的`R-HC-01`，列出恢复资源中的
`bdy h=-999`与多组`itr w=0`未分类形状。调用栈在全局geometry审计处终止，尚未进入OID5152检查；该失败与
publisher/membership代码无调用关系。本Change不修改DAT/parser/geometry风险分类，因此如实记为外部BLOCKED，
继续以focused与R08真实Play验证本修复。

### R08 repair observation（2026-08-24）

- 修复后的正式R08 Play已再次完成4500个fixed tick，`cooldownTicksAdvanced=4500`、
  `cooldownReleasePassed=true`；旧的`partner.Reset → unified publisher.ValidateRow` stale-row异常未再出现；
- merge runtime、OID51 Central body和dormant partner suppression继续通过；
- 当前失败已经后移到probe的`Next full maintenance did not restore the original OID pair`联合断言。该断言同时检查
  self/partner OID、dormant标记和全局`ObjectCount==afterFixtureObjectCount`，而失败前没有采集split状态，无法判断
  是production split字段失败，还是4500 tick期间其他production lifecycle使旧绝对ObjectCount基线失效；
- 下一步只在`R8-MERGESPLIT-001`下增加test-only分项观测并重跑。此证据足以证明本Change关闭了原production异常，
  但在R08完成slot/generation、Central visibility与cleanup验收前，不能把本Change升级为`VERIFIED`。

### R08 final acceptance（2026-08-24）

- final result：`Temp/NTSD_R8_WP01G_R08_Oid5152MergeSplit.result.json`，mtime
  `2026-08-24T01:48:32Z`，status=`PASS`；
- 4500 cooldown ticks真实执行；self/partner由51+dormant恢复7/8，slot0/10与generation1保持，dormant=false，
  split final tick局部ObjectCount严格`14→15`且claimed slots `8→8`；
- C++当前aggregate HP/HPMax各半为双方95/95，formal frame112经同tick DAT wait0推进到frame113/state8，PP0、
  position、velocity、opposite facing均通过；
- merged body提交、dormant suppression与split双body提交均通过；清理仅释放5个post-baseline handles，运行前handle保留，
  final world/claimed/object pool/logic pool恢复`2/1/1/1`，RNG恢复且cleanup error为空；
- 因此`B-R8-R08-03`关闭，本Change按C++ source + compile + focused + direct production Play证据升级为`VERIFIED`。
  该结论只裁决row-membership lifecycle修复，不声称R1-WP02 full trace或全战斗系统完成。
- `Tools/Validate-ChangeLedger.ps1`最终PASS：91 records / 111 governed code files covered。

## 8. 风险、回滚与未关闭项

- 风险：relation/link修复后可能暴露vital/frame-motion/input store的同类reset生命周期问题；必须作为新first difference
  停止，不能一次性改成全局静默容错；
- 风险：merge端如果不同时失效row membership，active snapshot可能在下一tickroll-forward后继续包含已dormant partner；
  因此focused必须覆盖merge与split两个方向，而不能只压掉当前异常；
- 未关闭项：本Change无；全局`R-HC-01` self-check blocker与R1-WP02 full trace仍属独立事项；
- 回滚方式：若获批后实现失败，只回滚本Change的production/test diff并保留失败证据；需用户批准回滚；
- 当前没有脚本diff可回滚。

## 9. Git / 交接

- 工作树已有大量用户/历史修改，不清理、不覆盖；
- 本Record创建时production脚本0新增改动；
- 提交hash：未提交；
- validator：待本轮文档同步后运行；
- 优先阅读：本Record、对应Task、R08 Task/Record/最新handoff和R08 result JSON。
