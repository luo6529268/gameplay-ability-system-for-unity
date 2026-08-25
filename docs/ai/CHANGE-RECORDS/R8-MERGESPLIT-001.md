# R8-MERGESPLIT-001 — OID7/8→51 merge / dormant / split production Play witness

<!-- CHANGE-RECORD
id: R8-MERGESPLIT-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOid5152MergeSplitPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1008-1154 / USER-APPROVED-R8-WP01G-R08
evidence: CPP-RELEASE-SOURCE-CROSSWALK / R08-4500-TICK-MERGE-SPLIT-CENTRAL-CLEANUP-PASS
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-24  
> 类型：TEST-ONLY / lifecycle / CentralOnly runtime certification

## 1. 状态与范围

- 当前状态：`VERIFIED / UNITY S4 / TEST-ONLY PROBE`；
- 所属 Work Package：`R8-WP01G-R08`；
- D-ID：`D-LIFE-001`，并补齐`D-RENDER-003`的dormant/split剩余证据；
- 只允许新增一个Editor-only production Play probe及meta；production gameplay默认0改动；
- 不属于本Change：AI、T8、IL2CPP、Android、服务器、C++ executable/full trace、DAT、pass order或架构重构。

## 2. Authority / 需求依据

- 用户于2026-08-23明确批准执行`R8-WP01G-R08`并恢复总目标；
- C++ authority为只读`game_tick.cpp:1008-1154`：低槽0..19 maintenance、OID7/8 merge gate、
  OID51/frame290/Unk338=4500、partner inactive、frame9..260 split gate、原slot reset/reactivate、frame112；
- Unity对应`SimulationWorld.Passes.partial.cs::Oid5152RuntimeMaintenanceAll`和现有production factory/pool；
- `OidMergeDormant`保留原slot/generation是已批准Unity adapter；本Change只验证可观察等价性，不改变该适配。

## 3. 修改前状态

- 静态C++→Unity crosswalk未发现production gameplay差异；
- focused OID5152 32/32与七组full self-check已有历史PASS；
- 尚无真实production world完整tick的merge→dormant→split、CentralOnly pixel、slot/generation/cleanup证据；
- 正式OID51 DJA是否可达、4500 tick实际耗时和资源加载时点仍需执行期只读确认。

## 4. 计划改动

| 文件 | 类型 / 方法 | 目标职责 |
|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/BattleOid5152MergeSplitPlayModeProbeEditor.cs` | 新Editor-only probe | 用正式OID7/8/51、production factory、完整tick自然触发merge/split；采集slot/generation/query/ECS/CentralOnly/pixel/runtime/checksum/cleanup |

## 5. 不可回退边界

- 不直接写OID51、`OidMergeDormant`、Unk328/32C/330/334、ObjectCount或split结果；
- 不调用或反射private merge/split helper；
- 不直接写Unk338=0；DJA不可达时完整推进4500个固定tick；
- 不释放dormant partner slot、不推进generation、不修改DAT或增加OID专项production分支；
- 不回退CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5×scale、fixed camera、extended capacity、
  30Hz/FrameInputSet、SoA/ECS、pool/worker/0GC。

## 6. 验收

1. merge前OID7/8 active，记录post-fixture Unity logic+shell ObjectCount，slot/generation有效；
2. 完整tick后self OID51/frame290/PP500/Unk3384500，HP/HPBound/midpoint符合C++；
3. partner dormant、Unity ObjectCount相对post-fixture精确减1、退出query/ECS/central command/pixel，
   原slot/generation/stable/rest保留；
4. dormant期间allocator不占其low slot；
5. DJA有效则正式输入释放；否则4500完整tick自然归零；frame9..260不提前split；
6. split pass写OID7/8原slot与frame112、当前HP/HPBound各半、PP0及formal reset/position/velocity/facing/team；
   完整tick末按正式DAT `wait0→next113`观察frame113/state8；
7. CentralOnly无ghost/双画，checksum与cleanup恢复；
8. compile、focused、full self-check、Play、Console0与ledger validator全部PASS。

## 7. 实际改动与证据

尚未写入脚本。只读reachability审计确认`data.txt`声明OID7/8/51，但当前项目中以下正式Unity DAT均不存在：

- `Assets/NTSD/Config/chars/rock_lee.dat`；
- `Assets/NTSD/Config/chars/chiyo.dat`；
- `Assets/NTSD/Config/chars/sasori.dat`。

`CharacterAnimtorManager.ParseCharacterFrameConfigs()`只按`data.txt`相对路径解析本地文件，缺失时跳过wrapper；
因此无法满足“正式OID7/8/51 + production factory”的R08前置。相邻`I:\GitHub\Unity_GAS\ntsd_proto`
存在同名加密DAT，但本Task禁止新增/修改DAT，且该目录不是已确认Unity适配资产来源，未复制、未解密、未写入。
只读SHA-256分别为rock_lee `EE08D029...5C888`、chiyo `F18FE185...EECE`、sasori
`7C505382...5505`。初次只搜索`ntsd_release`源码树而未找到同名DAT；后续扩大到实际运行目录父级后，
在`J:\QQFile\NTSD2.4\chars`找到正式运行资源。correction：三份runtime DAT分别为rock_lee
101703 bytes/hash `ED627F64...AAE80`、chiyo 89614/hash `21D4AE82...2D8A`、sasori 97492/hash
`18DEAECA...8CEA`，与`ntsd_proto`三份的长度/hash全部不同。因此`ntsd_proto`不能作为权威或当前Unity
适配资产直接部署；两侧均保持只读，未复制/解密/写入。
当前Unity仓库的全部Git refs中也没有这三个`Assets/NTSD/Config/chars/*.dat`路径的历史提交，且无ignore规则，
因此不能通过现有仓库commit安全恢复。

Blocker：`B-R8-R08-01 — formal Unity OID7/8/51 DAT resources missing`。
恢复条件：用户通过既有Unity资产流程恢复上述三个DAT，或明确确认一个允许读取/复制且符合当前Unity DAT适配
合同的来源；恢复后先验证文件hash/loader可达性，再继续probe。

### 2026-08-24 blocker closure / resumed preflight

- 用户明确恢复目标；`R8-CHARASSET-001`已将OID7/8/51纳入正式Character catalog，资源契约定向测试1/1 PASS；
- OID7 `rock_lee.dat`与OID8 `chiyo.dat`均存在正式state2 frames `9/10/11/19`，可用authored frame9形成merge前置；
- OID51 `sasori.dat`存在frame290，`state15 / wait2 / next999`，且没有`hit_ja`；
- C++ `input_handler.cpp:2835-2868`与Unity `BattleCharacterInputActionResolver`都确认：DJA组合完成后，若没有可跳转
  `hit_ja`但`Unk328==1`，仍清零`Unk338`；下一次maintenance在frame290（不属于9..260）自然split；
- 因此probe不需要等待4500 tick，也不直接写`Unk338=0`。脚本写入前的source/DAT/reachability合同已闭合。

### 2026-08-24 actual test-only implementation

- 新增已登记的`BattleOid5152MergeSplitPlayModeProbeEditor.cs`及稳定meta；production gameplay 0改动；
- 在已暂停的真实production world中，从正式Character catalog和正式logic/render pools创建OID7/8，并申请真实空闲
  self low slot0..9、partner slot10..19；只写允许的战斗开始前HP/team/position/authored frame9；
- 通过完整driver tick自然产生OID51/frame290与dormant；验证handle generation、partner slot不对allocator开放、
  CentralOnly merged body提交和dormant body抑制；
- 通过production InputBuffer依次注入物理D/J/A对应的内部`att/def/jump`边沿，每一步仍走完整tick；只观察
  CharacterInput写`Unk338=0`，下一完整tick由maintenance自然split；
- 验证原OID/slot/generation、frame112、half HP/HPBound、PP0、位置/速度、central visibility与完整cleanup；
- fresh compile、运行时报告、focused regression、self-check和validator尚待执行。

首次full asset refresh未进入运行时，发现probe-only编译错误：两个`RuntimeEntityHandle.Generation`证据字段误声明为
`int`，并误从`NTSDEntityRuntime`读取不存在的`Generation`。已统一为`uint`并通过正式current-handle查询生成
checkpoint generation；production仍0改动，保留该失败证据后继续fresh compile。

第二次fresh compile通过、Console compiler error=0。正式Play request已消费，但在probe创建fixture前被production
sprite prewarm异常阻塞：`Duplicate battle sprite key (56,112)`。全量审计确认只有OID56范围106-120与112-200
重叠；C++ renderer按声明顺序first-range-wins，而Unity异步写入加catalog duplicate guard不支持该语义。
触发production first-difference stop condition，建立`B-R8-R08-02`与独立`R8-SPRITERANGE-001 / PLANNED /
APPROVAL PENDING`。R08 probe保持已编译但未产生merge/split报告；不得在本test-only Change内修production。

### 2026-08-24 B-R8-R08-02解除与probe首轮恢复

- `R8-SPRITERANGE-001`已获批并实现；fresh compile0、overlap 2/2、atlas/catalog 29/29、正常Play 25秒
  error/warning0，原duplicate key不再出现；全DAT映射探针审计137 definitions/12487 entries，没有OID56 range/source
  mismatch，其唯一FAIL是与本Change无关的state8000 dynamic command witness（workerPath=false）；
- R08 probe重新启动后没有创建fixture，12,000 Editor updates超时；报告start/end tick `1789→4026`、baseline未采集，
  证明它停在`ValidateProductionCatalog`之前/之中，而不是merge行为FAIL；
- 现有probe对catalog readiness failure静默return，无法区分OID wrapper、frame state或加载时点。下一test-only改动只
  缓存并记录变化后的readiness reason；不改production、fixture前置或验收条件。
- 加入readiness reason后第二轮确认catalog没有失败；probe仍在baseline前等待且tick持续推进。只读callback审计定位
  到另一已加载Editor probe的request poller会在没有request文件时清除driver pause。为避免修改历史probe扩大范围，
  当前R08 probe只在自身`running + pauseRequested`期间、每次Observe最前重新断言pause；其Observe是后注册回调，
  因而在显式diagnostic tick之间保持安全边界。此修正不写production driver、不改变tick实现或验收结果。
- pause ownership修正后，R08首次真正执行fixture与完整tick：before为OID7 slot0/gen1 frame9/state2，partner为slot10；
  第一个`self OID51/frame290`断言已通过，下一联合断言`partner dormant && ObjectCount==baseline+1`失败；cleanup
  完整恢复。现有probe在该断言后才采样partner，故证据尚不能区分dormant writer或ObjectCount哪一项先差异。
  下一次只把after-merge self/partner和mid-tick object/slot计数提前到断言前采集，再重跑定位；production保持不改。
- 提前采样重跑后确认：OID51/frame290、HP150/bound190/PP500/metadata与OID8 dormant均正确；唯一失败是global
  ObjectCount实际5、旧断言期望3。frame290正式DAT含kind2 OID213 opoint，同一live tick还可能有stage/random结构事件；
  因此尚不能把global count增量归因于merge writer。下一次只增加structural writer before/after spawn/register/last source
  诊断，区分OID213与并发生产生成；不改断言前的production行为。
- structural诊断第一次编译因probe缺少`NTSD.Simulation.Ecs` using产生一条test-only CS0246；production无诊断。
  只补正确namespace后重编译，不改变探针逻辑。
- structural重跑为`SpawnDelta=0 / RegisterDelta=0`，确认额外ObjectCount不是frame290 opoint或同tick随机生成。
  结合项目既有1000实体验收合同`worldObjectCount == requestedEntityCount * 2`，定位为probe错误地假设每个production
  character只贡献一个SimulationWorld object。修正为在BuildFixture末记录post-fixture count，并严格要求merge后
  `postFixtureObjectCount - 1`；dormant仍必须true、claimed slot仍必须保留。该修正适配已批准Unity shell结构，未放宽
  merge减量合同。
- post-fixture计数修正后的首次完整执行已越过dormant/ObjectCount断言；报告实际为`HP=150 / HPBound=190 /
  XInt=530`，但`ZInt=376`，而旧探针把fixture Z固定为`340/344`并断言最终`342`。只读源码闭合确认：C++
  `game_tick.cpp:1081-1085`先写合体中点，随后`1423-1438`按当前`bg.zboundary_min/max`执行角色Z边界；Unity同样
  从场景`BoundaryWallManager`覆盖`Runtime.Stage.ZMin/ZMax`并在完整tick执行StageBounds。因此固定340可能落在当前
  production stage范围外，最终376是后续合法边界夹取，不能据此判定merge writer错误。下一test-only修正将在
  `BuildFixture`从当前`Runtime.Stage`选择留有4px余量的合法Z，再继续严格断言完整tick后的中点；不修改production、
  StageBounds、场景或验收语义。
- 动态合法Z夹具重跑确认`stage=376..772 / self=572 / partner=576`，不再触发边界夹取；合体后完整tick实际
  `ZInt=575 / Vz=0`。C++顺序为`game_tick.cpp:1081-1085`先写midpoint574且只清self.vx，不清self.vz，随后
  `game_tick.cpp:1247-1275`进入frame_advance，`physics.cpp:36-40`执行`z += vz`，再由`physics.cpp:55-70`
  地面摩擦将原fixture `vz=1`降为0。因此完整tick权威终值是575，不是合体瞬间574；Unity实测与该source chain
  一致。下一test-only修正把联合断言明确为`midpoint + same-tick physics`，仍严格检查HP/HPBound/X/Z/Vz，
  不改变production或验收强度。
- same-tick physics断言修正后，R08已通过merge runtime、merged Central command与dormant Central suppression，
  首次进入DJA链；FAIL为第一步`expected ComboDja=1 / actual=0`。只读C++ `input_handler.cpp:2798-2809,
  2835-2839`和Unity `BattleCharacterInputActionResolver.cs:233-240`均确认DJA顺序是defend→jump→attack。
  当前probe却先queue `att`，随后才queue `def`与`jump`，属于probe物理输入顺序写反；下一test-only修正只把
  队列顺序改为`def → jump → att`，每一步仍走production InputBuffer与完整tick，不修改输入实现或组合窗口。
- correction：按键名与C++内部cooldown语义不是一一同名。Unity `NTSDInputStateModule.ApplyNewPressEdges`
  明确把physical `att→CdDefend`、`def→CdJump`、`jump→CdAttack`，既有组合回归也用`att→def→jump`形成DJA；
  因而上一轮把probe改成`def→jump→att`是错误推断，实测第一步仍为0。进一步只读闭合发现真正原因：
  `SimulationTickDriver.StepOneTick()`先从Local provider取得canonical `FrameInputSet`，`ApplyFrameInputSet`为全部按键写
  complete packet；`NTSDInputStateModule.UpdateFromBuffer`在同tick存在complete packet时会忽略probe直接写入的普通
  buffer event。因此探针输入被正式中立帧覆盖。下一test-only修正恢复physical `att→def→jump`，并通过公开
  `StepOneTick(FrameInputSet)`提供该roster slot的canonical held buttons；仍走production ApplyFrameInputSet/InputBuffer/
  HumanInput/CharacterInput，不直写cooldown/combo/runtime，不修改production。
- canonical FrameInputSet重跑已越过`ComboDja=1/2`，证明正式输入边界生效；第三步后`Unk338=4497`而非0。
  correction：OID51 frame290正式DAT为`hit_ja=0`。C++ `input_handler.cpp:2847-2867`仅在`hit_ja!=0`分支内
  才可能进入`dja_check_328`；`hit_ja==0`会走interrupt tail，不能清Unk338。现有full self-check
  `BattleRuntimeSelfCheck.cs:27230-27242`也明确要求missing DJA target不得fall through到merged release。
  因此前一preflight“无target仍由Unk328清零”的结论被实测和源码共同否定。按原Task允许的fallback，下一probe-only
  改动将分批执行4500个公开`StepOneTick(FrameInputSet.Empty)`，中间不build presentation，`Unk338==1`的最终tick
  build presentation并要求maintenance自然split；不得直写Unk338或调用split helper。
- 已确认Unity logic+shell口径下post-fixture ObjectCount为6、merge后5；自然split应恢复post-fixture 6，不能使用
  旧的`baseline+2=4`。split断言将严格改为`afterFixtureObjectCount`，与merge精确-1合同成对，不放宽生命周期。
- 4500 tick fallback已真实执行到final maintenance：`oid51HitJa=0`、`releaseMode=cooldown-4500`，merge runtime、
  merged Central body与dormant suppression均先通过。split进入`partner.Reset()`时，relation/link setter向已排除的
  AI unified row发布，`BattleAiUnifiedRowPublisher.ValidateRow`抛
  `stale slot generation after commit`。这是production异常而非probe断言，触发`B-R8-R08-03`；异常还中断cleanup，
  报告final world/pool未恢复。已退出Play，未强杀Editor；新建`R8-WP01G-R08-R02 / R8-AIROWGEN-001 /
  PLANNED / APPROVAL PENDING`。本test-only Change不得顺带修改production。
- `R8-AIROWGEN-001`获批修复并通过focused后，R08正式Play再次完成4500 tick；旧stale-row异常已消失，证明
  row-membership invalidation已让split越过dormant partner reset。当前失败后移到probe的联合断言：它把
  self/partner OID恢复、dormant清除和全局ObjectCount恢复绑在同一句，并在断言前没有采样split结果。最新报告
  `afterFixtureObjectCount=6 / finalObjectCount=8 / finalClaimedSlots=4`不足以判定production split是否失败，因为
  cooldown期间可能存在其他正式生命周期事件，且cleanup在断言后才启动。下一test-only改动必须先采集final tick前后
  ObjectCount、claimed slots、structural writer delta和两实体状态，再把语义断言拆开；诊断证据出来前不改production。
- 分项诊断重跑已确认production split成功：final tick为self OID7、partner OID8、dormant=false，slot0/10与generation1
  均保持；`ObjectCount 14→15`、claimed slots `8→8`，同tick`spawnDelta=0/registerDelta=0`。因此旧绝对
  `ObjectCount==afterFixtureObjectCount(6)`只是在4500 tick长时运行后失效；严格合同应为split final tick局部`+1`且
  claimed不变。
- 同一结果还显示pre-split当前aggregate `HP/HPBound=190/190`，split后双方`95/95`；C++ live source
  `game_tick.cpp:1133-1136`明确按split当时的当前值各除2，而不是固定按merge初值150/190。C++在该pass写frame112后
  继续执行frame advance；正式OID7/OID8 DAT的frame112均为`wait:0 next:113`，所以完整tick末严格期望frame113/state8，
  不是pass瞬间frame112。下一probe-only修正使用这些动态前值和tick末frame113，production仍不改。
- 修正后最新R08已达到`mergeSplitPassed=true / splitBodiesSubmitted=true`：merge、dormant、4500 cooldown、split
  runtime与双方Central command全部通过。唯一FAIL发生在`FinishSuccess`后的probe cleanup：baseline时只有1个正式逻辑
  实体，4500个完整tick期间正式世界生成了额外对象；现有cleanup只释放self/partner夹具，却把剩余衍生对象导致的
  `finalObjectCount=11 / baseline=2`记为失败。下一test-only修正将在baseline采集generation-safe handles，并在清理时
  多轮释放不属于baseline的post-baseline实体，再严格比较world/slot/object-pool/logic-pool基线；不得阻止生产生成、
  改随机武器/opoint生命周期或清理运行前实体。
- generation-safe baseline cleanup实现并fresh compile0后，最终正式Play报告于`2026-08-24T01:48:32Z`写入PASS：
  `cooldownTicksAdvanced=4500`、`mergeSplitPassed=true`、`splitBodiesSubmitted=true`、`cleanupCompleted=true`；清理
  释放5个post-baseline实体，最终world/claimed/object pool/logic pool=`2/1/1/1`与baseline完全一致，RNG恢复、
  cleanup error为空。`B-R8-R08-03`由`R8-AIROWGEN-001`关闭，R08 Unity S4收口。
- 退出Play后Unity Console出现1条场景关闭警告`Some objects were not cleaned up when closing the scene`及MCP client
  disconnect提示；该警告发生在已写入PASS且probe计数恢复之后，未出现脚本异常。故R08行为与cleanup结果为PASS，
  但不虚构本轮“退出Play warning 0”。
- `git diff --check`无whitespace error；`Tools/Validate-ChangeLedger.ps1`最终PASS（91 records / 111 governed files）。

## 8. 风险与停止条件

- 正式wrapper/resource无法加载、只能直接写结果、出现production first difference、需要改变受保护adapter、
  或DJA不可达且4500 tick无法合理完成时立即停止；
- production first difference必须拆独立repair Change，不能在test-only probe内顺带修改；
- C++ full trace继续独立BLOCKED，不影响Unity S4执行，但不得因此升级为C++ runtime完整认证。

## 9. Git / 回滚

- 工作树存在大量用户和历史修改；不清理、不覆盖、不回退；
- 回滚只涉及本Change新增probe/meta及留痕状态，且需遵守用户批准规则；
- 提交hash：未提交；脚本0改动；validator待最终交付运行。
