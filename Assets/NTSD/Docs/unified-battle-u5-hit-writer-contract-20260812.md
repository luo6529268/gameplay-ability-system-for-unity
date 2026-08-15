# U5 命中 writer 权威契约与只读执行计划影子（2026-08-12）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`
> 当前结论：权威命中消费边界已经闭合，并建立了默认关闭、固定容量、零分配的只读执行计划影子；`ShadowCompare` 已逐项验证正式 character/object 链实际读取的候选、pair preprocess、全部权威 kind/disposition、预消费副作用、dispatch、OID300 abort、可达 effect、声音和生命周期。DataOriented frozen-plan 调度保持行为/hash/0 B，但正式 A/B 未达到 10% 晋升门槛，默认仍为 `Disabled`。kind `1/3/2/7`、held、cpoint 与全部 damage 原子事务现已分别迁入 world-owned writer；opoint/结构生命周期也已进入 `BattleStructuralWriter`。U5 最终联合回归为 220/220 PASS，完整 self-check fresh PASS。

## 1. 权威 C# 边界

本切片只以以下正式 C# 调用链为依据：

- `BattleCore/Simulation/GameTick.cs`：`SnapshotPrevFrame2` 后依次执行 character hit、随机武器掉落、object hit；
- `BattleCore/Interaction/HitResolver.cs`：character/object 两个入口分别转入 `HitResolve.ResolveLoop1/ResolveLoop2`；
- `BattleCore/Interaction/HitResolve.cs`：`ResolveCandidates`、`PreprocessCandidate`、各 kind 分支和完整伤害/交互副作用。

必须保持的消费合同为：

1. loop1 只处理 character DAT，loop2 只处理非 character DAT；
2. attacker 按固定 runtime slot 升序，候选按 `[0, Mp)` 原顺序消费；
3. attacker 必须 active、存在 `CharData` 且 `Mp > 0`，攻击帧读取 `PrevFrame2`；
4. `AbortRemainingHitPairs` 命中时先清回 `false`，再终止该 attacker 的剩余 pair；
5. victim slot、itr index、victim active/数据与 `vRest` 校验发生在正式写入前；
6. `PreprocessCandidate` 可以改变 kind、link、vRest、RNG 和实际使用的 itr；
7. kind `0/9/6/8/14/15/16/10/11/1/3/2/7` 的状态写入不能被拆成只有扣血的局部迁移；
8. writer 的原子边界还包括 HP/HPMax、kill/combo/damage 统计、frame/frame delay、速度、fall、朝向、link/holder/target、aRest/vRest、声音、正式事件、opoint 与生命周期副作用。

因此，目标 generation 只可作为 Unity 影子诊断身份，不能替代权威 C# 的 target slot 消费语义，也不能新增 gameplay gate。

## 2. Unity 对应关系

现有正式 writer 仍位于：

- `LF2CharacterInteractionResolver` / `LF2CharacterDatInteractionResolver`；
- `LF2CharacterHitResolver` / `LF2CharacterDatHitResolver`；
- weapon、special attack 与 other object 对应 resolver；
- `SimulationWorld.PostInteractionTickAll` 与 `SimulationWorld.ObjectInteractionTickAll` 的既有对象调用链。

本切片新增 `Simulation/Ecs/BattleEcsHitExecutionPlan.cs`，只在测试显式启用 `ShadowCapture` 或 `ShadowCompare` 时，于上述两个 pass 的最前端冻结本 tick 的消费输入：

- pass、attacker handle/stable id、`PrevFrame2`；
- candidate ordinal、target slot、itr index/kind；
- 源 itr 与已记录 itr 的无分配 fingerprint；
- 候选携带的 consume 标志；
- target handle 快照仅供诊断，不参与正式判定。

执行计划使用 `runtimeSlotCapacity * 20` 的固定结构体数组，不调用任何伤害 resolver，不修改 RNG、runtime、候选存储或生命周期。默认模式为 `Disabled`；候选源不可读、数量不一致、帧/itr 无效、容量溢出或同 pass 重复捕获时均 fail closed，并留下结构化原因。

`ShadowCompare` 在正式旧 writer 消费期间只观察候选读取，并逐项比较 pass、attacker handle、candidate ordinal、target slot、itr index、已记录 itr fingerprint 以及两个原始 consume 标志。Legacy candidate range 原本不携带 attacker handle；现在仅在观察模式已开启时从 runtime slot table 补取该诊断身份，默认 `Disabled` 路径不会新增 handle 查询。比较器不参与命中判定，也不改变 target slot 的权威语义。任何多读、少读或内容不一致都会令当前计划 fail closed。

候选读取后又增加了四个严格位于旧链周围的只读观察边界：

1. 记录当前 pair preprocess 后真正使用的 itr fingerprint、kind 投影、`ZeroAttackerHpOnConsume` 与 `ReleaseHeavyHeldTargetOnConsume`，并与只读计划投影比较；
2. 在旧 `ApplyReleaseSceneQueryConsumeEffects` 调用前记录状态并预测调用后结果，调用后立即观察 attacker/target/held handle、HP、link/holder/target、两组 vRest、held frame/Vy 以及 RNG state/call count；
3. 消费副作用预测使用已经验证的当前 pair preprocess 结果，而不是碰撞收集时冻结的原始标志；这保持了权威 C# 在正式消费前重算当前状态的时序；
4. preprocess 后由独立只读投影计算该 candidate 应进入的 disposition，并与四条真实 consumer 实际选择比较：kind `0/9` 为 damage、`6` 为 hit-confirm、`8`、`14`、`15/16`、`10/11` 为各自攻击分支、`1`/`3` 为 grab、`2/7` 为 pickup，未转换的 `4/5` 和其他 kind 为权威 no-op；消费 gate 拒绝与 OID300 redirect 也分别编码；
5. 在四条真实 consumer dispatch 前后记录实际成功/失败，并独立投影 OID300 redirect 是否应在成功 dispatch 后终止候选；合法终止只跳过计划中同一 attacker 的剩余 entry，不得吞掉下一个 attacker；
6. dispatch 的 Unity `bool` 不是 C# 权威的 writer 成功定义：权威 `ApplyCandidate` 的各 kind 分支为 `void`，而 Unity 的 kind14 可以已经写入 block flag 后仍返回 `false`。因此 writer-effect oracle 独立比较前后状态，不能用 dispatch `bool` 代替状态正确性；
7. writer-effect oracle 已覆盖 kind6 hit-confirm、kind8 heal/位移/帧、kind14 四向阻挡、kind1/3 抓取、kind2/7 拾取、kind10/11 衰减/计数、kind15/16 移动与 link 释放；damage `0/9` 还覆盖标准角色 HP/HPMax/统计/四档受击、alternate safe slice、标准武器 `1/2/4/6`，以及 type3 基础尾链、状态同步和 D1 身份替换；
8. 观察器不替代 writer、不推进 RNG、不写 rest/link/runtime，也不改变 abort 或候选消费顺序。缺少 prepare/observe、前态不合法、伪造 disposition/终止或任一字段不同均结构化 fail closed。

本切片同时修正了两处由 disposition 核验暴露的正式 Unity 行为差异：

- 权威 preprocess 的相关顺序为 kind4 转换、重武器 held release 判定、kind5 替换。Unity 的 kind5 替换代码位置仍早于 held release，但 held release 资格现在只由原始 kind0，或 `WeaponCount > 0` 时可转换的原始 kind4 提供，因此 kind5 替换为 kind0 不会再倒灌触发前序 held release；
- 未转换的 kind4 按权威 switch default 不再进入攻击 writer；weapon 与 special attack 的 kind6 现在与 character 路径一致，只写 `HitConfirmCounter = 3` 后返回。
- 旧 `LF2CharacterInteractionResolver` 的 kind1 路径依赖 `CaughtA` 门槛，可能在权威候选已经成立后不写双方速度、朝向、精确对位与抓取槽位；精确状态 oracle 实际检出该差异后，kind1 已与 DAT character/special 路径统一到 `TryApplyKind1Grab` 权威 writer。

## 3. 当前验证证据

**最新证据覆盖说明（2026-08-12 13:56）：**本节下方保留的早期 job、独立编译和 Editor 崩溃记录属于排障历史；涉及 OID `0xD6`、OID `0xC9`、OID `5/52`、OID100、历史帧/cpoint、state1002/2000/3000、active-holder 与 held-pair vRest 的当前状态，以最新 Unity job `0c79401c82684ad6ac9168cf77a4244e` 的 118/118 PASS 和 fresh 完整 self-check PASS 为准，不再标记为“Unity 运行时未验证”。

- Unity fresh compile：0 error；
- `dotnet build Assembly-CSharp.csproj --no-restore`：0 error；
- `BattleHitExecutionPlanEditorTests`：alternate 致死补齐后 96/96 PASS，job `562ce635bbf64029a5b1319f45ec6dcd`；
- 命中计划、character/object 空候选与碰撞命中见证联合回归：112/112 PASS，job `798b5f79820c400cbe61497a1de3c186`；
- 完整 `BattleRuntimeSelfCheck`：`2026-08-12 08:52:47` fresh PASS，请求文件已被实际消费；
- 覆盖默认关闭、character/object 独立顺序、候选冻结、重复捕获 fail closed、候选生命周期结束 fail closed、正式旧链逐候选一致、全部权威 kind/disposition、未转换 kind4 no-op、kind5 held-release 边界、kind6/8/14/1/3/2/7/10/11/15/16 精确 writer-effect，以及 damage 标准/alternate/object/type3 已列分支；故意篡改 itr index/preprocess/disposition/abort/writer-effect 和缺失观察均必须 fail closed；`ShadowCapture`/`ShadowCompare` 预热后 managed allocation 为 `0 B`；
- 标准角色 damage 已验证 fall `10/30/50/70` 对应 frame `220/224/226/186`、最终 fall `20/40/60/0`、倒地 X/Y 击退和 `SFX_006`；致死分支已验证 HP/HPBound、攻击/受击 combo、holder kill、world kill、damage stats、强制 fall、倒地帧、击退、rest、RNG、声音与 hit-record age。标准对象 damage 已验证类型 `1/2/4/6`，effect0/effect4 的声音、随机帧与 vRest 差异，以及 heavy `fall<=40` 时 HitCount/纵向击退保持权威分支；type3 已验证 relation、holder-copy、motion 清零、rest、hit-record，effect `0/2/3/5/21/22/23/30/5005/5999/6033` 的 frame、主/追加声音、PP 扣减及下限，state3005 双方同步，以及 D1 直接身份替换、D1+state3006 组合同步、OID8/held-D5 从活动 D1 复制身份；
- effect20 对非角色 DAT 在权威 `CollisionCollect` 中即被拒绝，无法进入 type3 special-attack writer；执行计划已据此从可达支持集合排除该输入，没有用直接调用绕过碰撞前置条件；
- oracle 实际检出并修复 weapon/special/held 三条直接切帧原语未同步 `Runtime.Frame` 的生产双帧镜像遗漏；
- OID `100` 的正式武器命中尾链已按权威 C# 修复；该修复包含在上述 96/96、112/112 和 fresh self-check 证据中；
- OID `100` 的角色、DAT 角色与特殊攻击三条正式命中链已补齐权威分支顺序：当击退成立、目标水平击退处于 `(-5, 5)` 且 `dvx == 0` 时，只追加固定方向的 `5` 并立即返回，不得再进入 OID100 的 `2.5x`、最小 `10` 与 `SFX_039` 尾链；除此以外即使解析后的 `dvx == 0` 也仍执行该尾链。正/反两组 shadow 用例已写入，Editor C# 工程独立编译通过，但尚无新的 Unity 运行时结果；
- kind9 定向验证覆盖 preprocess 把 kind9 投影为 kind0、消费时 attacker HP 归零；重武器定向验证覆盖 target/held 双向 link 释放、两组 vRest `45/30`、随机 frame `0..5`、`Vy=-1` 和 RNG state/call count；
- 测试夹具为重武器补齐 frame `0..5`，避免用缺失 DAT 帧造成 `ImmediateFrame` 假阴性；正式生产代码没有为测试改变帧回退语义；
- 测试同时核对 shadow 捕获前后 extended checksum 与 RNG 不变。
- OID `0xD6` 的只读投影已按正式 `ObjectInteractionTickAll` 对象命中 pass 完成 Unity 定向验证：标准角色伤害后特殊攻击者 HP 归零，同时目标扣血和 hit record 保留；
- OID `0xC9` 已按权威 `FreeEntity(attackerSlot)` 建模为独立生命周期输出，而不是普通字段快照：命中角色后旧 handle 失效、slot 未占用、generation 精确递增一次、slot occupant 为 `null`，且攻击者 `Runtime.SlotIndex` 清为 `-1`；观察 world 在 dispatch 前保存，正向、缺失观察和未释放伪完成三类聚焦测试均已进入最新 Unity PASS；
- OID `5/52` 已确认只在权威 opoint 创建阶段把新对象的 `HP/HPMax/HP3` 初始化为 `10`、`PP` 初始化为 `5`，不能在每次命中时重复重置。Unity DAT 角色命中链中错误的逐次重置已移除，shadow 用例同时断言正常扣减 `HP/HPBound` 且 `HP3/PP` 保持不变；该用例已进入最新 Unity PASS；
- 标准角色伤害 shadow 已补齐两条历史帧分支：上一帧为 `Frozen` 或碰撞快照 `PrevFrame2` 为 `Falling` 时必须强制进入 fall80 倒地路径；有效 reciprocal catcher/link 且 `PrevFrame2.cpoint.kind == 2` 时，按双方朝向选择 `fronthurtact/backhurtact`。测试夹具先把历史/被抓帧作为正式碰撞快照，再切回受击时当前帧，避免手写 `PrevFrame2` 被快照 pass 覆盖；两组均已进入最新 Unity PASS；
- 标准 damage 的攻击者状态尾链已继续闭合：`state1002` 先消费反弹帧 RNG，再消费 hit-record 的两次 RNG；`state2000` 按双方 X 相对位置决定击退；`state3000` 在 hit-record 前切 frame 10、清 `Attacking/Vx`，标准分支读取 frame10 的 `dvz`。alternate damage 另行覆盖 `state1002` 的 `Vz *= -2/3`、`state2000` 的朝目标移动 X/Z 速度衰减和不改 `Vz` 的 `state3000` 尾链；对应聚焦用例已进入最新 Unity PASS；
- active holder 与 `HolderCopy` 已在 writer-effect 快照中拆成两个独立契约：前者由 `LinkState < 0 + HolderStableId` 决定并接收攻击者命中后的 `FrameDelay`，后者只承担 kill/combo 统计归属。standard/alternate/object damage 的只读投影均按该边界比较，不再因为攻击者被持有而整体排除；
- type3 通用标准尾链现与权威分支一致：active 非角色攻击者固定选择 frame20；character 或 held 非角色攻击者只在 effect2/20 选择 frame20，否则 frame30。有效 holder 提供 relation/holder-copy 并接收 FrameDelay，失效 holder 不再错误回退到攻击者自身；OID8/D1/D5 仅在专用 Karasu 替换条件成立时走 identity projector，其余目标回退标准尾链；
- type3 object-hurt 前段已覆盖 HP<=0/空中目标、`PrevFrame` Frozen、`PrevFrame2` Falling 的 fall 计算/复位，以及 state1002 在 hit-record 前消费 RNG 并写反弹速度、state3000 写 frame10/清 `Attacking/Vx`/读取 frame10.dvz 的顺序；
- 权威击飞还要求在受击者持有另一实体时写 `heldTarget -> attacker = 45` 与 `victim -> heldTarget = 30` 两个方向的 vRest。核验实际发现 `LF2CharacterHitResolver` 与 `LF2CharacterDatHitResolver` 漏写，`LF2SpecialAttack` 的关系门槛又比权威更严格；三条正式 consumer 现统一调用 `LF2HitResolveRuntimeData.ApplyKnockdownHeldPairVrest`，只校验 active slot 与 `heldTarget.HolderStableId == victimSlot`，不额外要求权威没有的 held negative-link gate。shadow 为两个方向使用独立字段，相关聚焦用例与 active-holder FrameDelay 均已进入最新 Unity PASS；
- 正式 `LF2Weapon.ApplyHitEffects` 同样曾漏掉上述 held-pair vRest；现已在权威击飞帧写入后调用同一 helper。valid relation 精确写 `heldTarget -> attacker = 45` 与 `victim -> heldTarget = 30`，holder 不匹配时保持 `0/0`，两组 shadow 均为零差异；
- 标准对象 damage 的只读投影已移除普通负 link 武器、空中武器和 `bdefend=100` 的人工排除：耐久在 `bdefend=100` 时精确写 `-1`，空中重武器低 fall 使用随机 frame `0..5`，type4/6 先走 `abs(Vx) * 0.55` 动态击退而不是错误落入 effect22/23，普通负 link 只保持关系且仍完整结算；
- type3 普通尾链不再局限于 standing/state1002/state2000，也不再排除 `bdefend=100`；除 state3005/3006 继续由专用同步投影处理外，其他目标 state 都按权威先执行 object-hurt，再由 kind0 type3 tail 覆盖 frame/relation/motion；
- 最新 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`：0 error、76 个既有 warning；本条只证明脚本编译，不替代 Unity 运行时测试。
- OID300 定向验证覆盖三个同 pass 候选：第一个攻击者成功 redirect 后只跳过自身第二候选，下一个攻击者仍继续命中；OID300 HP 按权威 C# 保持不变，伪造 abort termination 会 fail closed；
- 命中计划、碰撞/命中见证、character/object 空候选路径与 PreInteraction 关联回归：69/69 PASS，job `9492705296da4e4a83ee2d349fb8ed4e`；其中明确覆盖 `CharacterHit -> RandomDrop -> ObjectHit` 边界；
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore /m:1 /v:q`：0 error；
- Unity fresh compile：0 C# error；
- 完整 `BattleRuntimeSelfCheck`：结果文件于 `2026-08-12 05:15:46` fresh PASS，请求文件已被实际消费。
- 最新聚焦 job `b5beed1352a24bc3ab9abff9f42d33e7`：`BattleHitExecutionPlanEditorTests` 165/165 PASS，0 failed、0 skipped，耗时 4.300 s；
- 上一轮完整 `BattleRuntimeSelfCheck`：`Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `2026-08-12 14:25:36` fresh 返回 `PASS`；本批最终 self-check 将在继续闭合 writer 后重新运行。

上述证据证明候选输入、正式 writer 的实际候选读取、preprocess、全部权威 kind disposition、预消费副作用、全部非 damage kind、damage 分支、可达 effect、声音、dispatch 与 OID300 abort 边界一致，且默认关闭的只读影子没有破坏生产 writer；OID `0xD6`、OID `0xC9`、OID `5/52`、OID100、历史帧/cpoint、state1002/2000/3000、active-holder 与 held-pair vRest 已完成 Unity 聚焦验证。W05A～W05E 的 opoint/生命周期合同已接入分段 `BattleStructuralWriter`，全部可达 damage 已迁入 `BattleDamageWriter`。最新联合 EditMode job `b55c2edd04964be7b784f7bec65ab0f5` 为 220/220 PASS，完整 `BattleRuntimeSelfCheck` 于 `2026-08-12 20:34:10` fresh PASS。

## 4. 后续门禁

U5 的命中/交互/生命周期 writer 所有权已经关闭，后续进入 U6：

1. 只在 ResetWorld/初始化边界切换 canonical SoA 真值，不允许同 tick 双 writer；
2. 保持 HP/HPMax、统计、frame/frame delay、速度、fall、朝向、aRest/vRest、RNG、事件与结构命令顺序为不可拆分事务；
3. 对象 resolver 逐步退为 adapter，不能在 SoA writer 之外继续拥有隐藏战斗真值；
4. slot 复用与结构变化继续保持 C# target-slot 语义，generation 只用于 Unity 身份和 stale-handle 防护；
5. U6 每批仍需 checksum、聚焦测试、fresh self-check 与 1000 AI/零 GC 门禁，不能用 U5 证据替代。

interaction/held、cpoint、damage、结构生命周期与 battle results 的最终记录分别见同日期 U5 文档。本记录只关闭 U5，不宣称 U6 的 SoA 真值迁移或 U9 性能验收完成。
