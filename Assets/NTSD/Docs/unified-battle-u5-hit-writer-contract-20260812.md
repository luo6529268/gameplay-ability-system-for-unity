# U5 命中 writer 权威契约与只读执行计划影子（2026-08-12）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`
> 当前结论：权威命中消费边界已经闭合，并建立了默认关闭、固定容量、零分配的只读执行计划影子；`ShadowCompare` 已逐项验证正式 character/object 旧链实际读取的候选、当前 pair preprocess、全部权威 kind 的消费 disposition、kind9/重武器预消费副作用、实际 dispatch 回报、OID300 只终止当前攻击者剩余候选的语义，以及 kind `6/8/14/1/3/2/7/10/11/15/16` 的精确状态副作用。damage `0/9` 已覆盖标准角色四档硬直/倒地、标准角色致死统计与强制倒地、alternate 非致死/致死分支、标准武器类型 `1/2/4/6` 的 effect0/effect4 分支，以及 type3 基础尾链、state3005/3006 同步、D1 直接/活动身份替换和 effect `0/2/3/5/21/22/23/30/5005/5999/6033`。OID `0xD6` 对角色命中后攻击者 HP 归零的字段投影，以及 OID `0xC9` 命中角色后释放自身 runtime slot 的独立生命周期投影均已写入并通过独立 C# 编译，但两者的 Unity 定向测试尚被 Editor 原生 AssetDatabase 崩溃阻断；正式 hit writer 仍由现有 Unity 对象路径唯一持有，本阶段尚未宣称 writer 已迁移。

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
- kind9 定向验证覆盖 preprocess 把 kind9 投影为 kind0、消费时 attacker HP 归零；重武器定向验证覆盖 target/held 双向 link 释放、两组 vRest `45/30`、随机 frame `0..5`、`Vy=-1` 和 RNG state/call count；
- 测试夹具为重武器补齐 frame `0..5`，避免用缺失 DAT 帧造成 `ImmediateFrame` 假阴性；正式生产代码没有为测试改变帧回退语义；
- 测试同时核对 shadow 捕获前后 extended checksum 与 RNG 不变。
- OID `0xD6` 的只读投影和定向测试已加入：预期在标准角色伤害完成后将特殊攻击者 HP 置零，同时保留目标扣血和 hit record。`dotnet build Assembly-CSharp.csproj --no-restore` 已于本轮以 `EXIT=0` 通过；但启动该定向 Unity 测试时 Editor 因 `MDB_READERS_FULL: Environment maxreaders limit reached` 原生崩溃，测试 job `1402bf42fcab4fea86ceaa01d1babd82` 未产生可采信结果，因此本项仍是“逻辑已写、编译通过、Unity 运行时未验证”，不能写成 PASS；
- OID `0xC9` 已按权威 `FreeEntity(attackerSlot)` 建模为独立生命周期输出，而不是普通字段快照：命中角色后必须令旧 handle 失效、slot 变为未占用、generation 精确递增一次、slot occupant 变为 `null`，且攻击者 `Runtime.SlotIndex` 清为 `-1`；观察 world 在 dispatch 前保存，避免正式 free 清空 `Match` 后漏掉观察。正向、缺失观察和未释放伪完成三类聚焦测试已写入；`dotnet build Assembly-CSharp-Editor.csproj --no-restore /m:1 /v:q` 已通过，但 Unity Editor 当前尚未恢复，所以只能记为“逻辑已写、编译通过、Unity 运行时未验证”；
- OID300 定向验证覆盖三个同 pass 候选：第一个攻击者成功 redirect 后只跳过自身第二候选，下一个攻击者仍继续命中；OID300 HP 按权威 C# 保持不变，伪造 abort termination 会 fail closed；
- 命中计划、碰撞/命中见证、character/object 空候选路径与 PreInteraction 关联回归：69/69 PASS，job `9492705296da4e4a83ee2d349fb8ed4e`；其中明确覆盖 `CharacterHit -> RandomDrop -> ObjectHit` 边界；
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore /m:1 /v:q`：0 error；
- Unity fresh compile：0 C# error；
- 完整 `BattleRuntimeSelfCheck`：结果文件于 `2026-08-12 05:15:46` fresh PASS，请求文件已被实际消费。

上述已通过证据证明候选输入、旧 writer 的实际候选读取、preprocess、全部权威 kind disposition、已覆盖预消费副作用、全部非 damage kind 的基础状态副作用、damage 主要基础分支、alternate 致死、type3 D1 身份/状态同步、dispatch 与 OID300 abort 边界一致，且默认关闭的只读影子没有破坏现有 writer；fresh 联合回归与完整 self-check 已通过。它们仍不构成 OID `0xD6`/`0xC9` 的 Unity 运行时结果、复杂特殊 effect、character-DAT type3 effect tail、正式事件/opoint/其余生命周期或正式 writer 已迁移的证据。

## 4. 下一道门禁

下一步仍不是直接让执行计划扣血。候选读取、preprocess、全部 kind 的消费 disposition 与当前两类预消费副作用门已关闭；下一道门禁是在不改变 writer 的前提下继续闭合正式 dispatch 结果和状态写入：

1. 先恢复 Unity 后完成 OID `0xD6` 与 OID `0xC9` 的定向运行验证；C9 的 `FreeEntity(attackerSlot)` 已建模为独立生命周期输出，运行门禁必须同时验证旧 handle、槽位占用、generation、occupant 和 runtime slot，而不能只看对象是否不可见；
2. 继续闭合 damage `0/9` 的剩余特殊死亡条件、特殊 effect、character-DAT type3 effect tail 与剩余 effect 变换；已经关闭的标准致死、alternate 致死、非 damage kind和 D1 identity 分支不重复迁移；
3. 对已覆盖的 HP/HPMax、kill/combo/damage 统计、frame/frame delay、速度、fall、朝向和 aRest/vRest 补齐复杂分支与 fresh 联合门禁；
4. 再闭合声音/正式事件、opoint、spawn/destroy/free/unregister/generation 输出；
5. slot 复用和结构变化时继续保持 target slot 权威语义，generation 只作诊断；
6. 只有完整 writer 数据契约、事件/opoint/生命周期输出全部闭合后，才允许在 ResetWorld 边界切换单一 canonical writer。

本记录关闭的是“迁移前的权威合同与稳定输入计划”子项，不关闭 U5 的真实 hit writer、cpoint/held/link writer、opoint 或结构生命周期任务。
