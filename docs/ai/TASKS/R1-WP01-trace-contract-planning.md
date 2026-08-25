# Task Contract — R1-WP01：C++ Release / Unity Trace 合同规划

> 日期：2026-08-21
> 状态：已完成规划，已停止；R1 C++ read-only trace acquisition、Unity trace、gameplay 改动与 R2 均未开始。
> 唯一行为权威：J:\QQFile\NTSD2.4\ntsd_release 中实际参与 ntsd_new.exe release 构建的 C++ live battle runtime。
> C++ 正式入口：src/entity/game_tick.cpp 的 game_tick(...)。
> 本文性质：后续实施合同，不是 trace 实现、行为结论或 C++/Unity 对齐证书。

## 1. R1-WP01 结论与边界

R1 必须先建立一条以 C++ release live path 为锚点的三方观察链：

1. C++ release trace；
2. Unity fallback trace；
3. Unity optimized trace。

三方必须使用同一个固定初始状态、同一条输入 journal、同一份 DAT 语义夹具与同一 stage 数据合同。比较器按 tick、C++ 观察点、事件顺序、runtime slot 与字段路径寻找第一个不一致点，而不是用 Unity self-check、历史 checksum 或 FPS 结论替代 C++ 行为证据。

本 Work Package 只完成这份合同、后续工作包拆分、状态记录和 handoff。它没有：

- 修改 Assets/NTSD/Scripts 下任何 gameplay；
- 修改 C++ Release runtime 的源码、构建文件、可执行文件、资源、配置，或向 C++ authority 目录写入 trace/比较产物；
- 运行 Unity Play Mode、长性能、完整 Unity/C++ 构建或实际 trace；
- 启动 R2，或调整现有 pass 顺序、CPoint、WeaponSync、held/link、collision、input、opoint、render handoff 或技能。

## 2. 证据状态与规划前提

| 事项 | 证据等级 | 已检查事实 / 边界 |
|---|---|---|
| C++ 唯一 authority | VERIFIED | 根 AGENTS.md 与重新对齐总纲声明 J:\QQFile\NTSD2.4\ntsd_release 的 live path 为唯一裁决源；该 C++ release 根目录已在本机读取。 |
| release build 参与性 | VERIFIED | J:\QQFile\NTSD2.4\ntsd_release\Makefile 的 TARGET 为 ntsd_new.exe，并列入 src/entity/game_tick.cpp、frame_advance.cpp、physics.cpp、collision_collect.cpp、collision.cpp、hit.cpp、weapon.cpp、cpoint.cpp、src/input/input_handler.cpp 与 src/render/renderer.cpp。 |
| C++ tick 入口与可观察边界 | VERIFIED | game_tick(...) 位于 src/entity/game_tick.cpp:945；其静态调用边界包括 cooldown 992、post_cooldown_input 1004–1005、frame logic 1251–1260、frame advance 1271–1275、第一次 Z clamp 1423–1439、candidate collect 1645–1652、角色 collision 1653–1656、随机武器 1657–1817、对象 collision 1818–1822、CPoint/weapon sync 1823–1825、第二轮 held/link 1848 起、PreFrame/render 2021–2077、post/late/tail 2078–2087。 |
| Unity tick 实现位置 | VERIFIED | Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs 中 RunTick、RunFrameAdvancePhase、RunInteractionPhase、RunPresentationAndCleanupPhase 真实调用 SimulationWorld 的当前 pass。 |
| Unity 预交互的当前内容 | VERIFIED | SimulationWorld.Passes.partial.cs:2226–2348 的 PreInteractionTickAll 在 candidate collect 之前运行 CPoint check、CPoint mismatch tail 与 RunWeaponSyncHeldStep10；它还带有 legacy/proof/skip 诊断开关。 |
| 既有 Authority400 / NTSDParity trace | VERIFIED（历史资料的来源） | Tools/NTSDParity/README.md 明确其 authority 是 ntsd_release_C#，其格式为 ntsd-battle-trace-v3/v4；BattleParitySnapshot 与 BattleParityTraceEditor 也以 Unity/C# 历史 parity 口径生成快照。它们不是 C++ release authority trace。 |
| 当前是否已有 C++ / Unity 同 schema trace | UNKNOWN | 本 WP 未执行 C++ release 或 Unity trace；也没有发现已被证明满足本合同的三方生产者。 |
| Unity 的 PreInteraction 提前执行 CPoint/WeaponSync 是否已造成 C++ 行为差异 | INFERRED | 静态顺序与 C++ step10 位置不同；尚无同 fixture 的 C++ live runtime trace，不能升级为 VERIFIED mismatch。 |
| C++ 输入注入、RNG 完整可见性、fixture bootstrap 与正式 release 运行路径 | UNKNOWN | 必须由后续 C++ trace 与 fixture 工作包在 live path 中闭合。 |

这里的 VERIFIED 仅表示已经读取到的源文件或项目材料事实。它不表示任一 gameplay 已与 C++ 对齐。

## 3. 总体 trace 原则

### 3.1 生产者与比较关系

统一 schema 的 producer.role 只能为下列之一：

| role | 含义 | 行为裁决地位 |
|---|---|---|
| cpp-release | 由实际 ntsd_new.exe release live path 发出的观察 | 唯一行为基准 |
| unity-fallback | Unity 中关闭本次受审 optimized/proof 路径后的完整逻辑观察 | 对照实现，不是 authority |
| unity-optimized | Unity 当前默认或指定 optimized/proof 路径的观察 | 对照实现，不是 authority |

比较必须同时保留下列两类结论：

- C++ → Unity fallback：判断 Unity 基础路径是否已经在 C++ 观察点分叉；
- C++ → Unity optimized：判断默认优化路径是否分叉。

Unity fallback 与 Unity optimized 的比较仅用于诊断“优化独有分叉”，不能取代前两项。两个 Unity trace 相等也不能证明它们与 C++ 相等。

### 3.2 观察而非参与

后续 trace producer 必须只读取已存在的 runtime 真值与已发生事件。它不得：

- 写入 Entity、GameWorld、SimulationWorld、Transform、Camera、RNG、slot allocator、候选 carrier、对象池或输入状态；
- 因 trace 开关改变 pass、slot scan、对象出生可见性、随机数调用次数或 render command 顺序；
- 以 Unity snapshot、C# trace 或历史 hash 覆盖 C++ 观测值。

对 cpp-release producer 另有更严格的物理边界：它只能从**未修改的** C++ Release runtime 经已有外部可观察通道取得数据；采集输出、run manifest、trace 与比较资料必须写入 Unity 仓库中的非 authority 目录或其他经确认的非 authority 目录。不得增加 C++ instrumentation、trace sink、fixture bridge、CLI、环境配置或 C++ 工程内的输出文件。

trace 可以是 fixture-only / opt-in；性能与 0 GC 不属于 R1 trace producer 的验收目标。若“无分配 trace”与可审计完整观察冲突，先保证观察正确，性能在 R7 重新认证。

### 3.3 核心术语

- runtime slot：C++ objects[] 的索引，是跨端直接比较的实体身份主键。
- generation / stable id：Unity 的内部诊断身份，只能放入 unityDiagnostic，不得参与 C++ 等价判断。
- checkpoint：以 C++ game_tick 的一个明确前/后边界命名的观测点。
- source segment：某一 producer 实际执行的源码段或 Unity phase，用于解释映射，不能冒充 checkpoint 的 C++ 语义。
- event ordinal：同一 tick、同一 checkpoint 中事件发生的严格顺序号，从 0 开始。
- field registry：字段语义、物理来源、类型、比较规则与 C++ 证据位置的版本化清单。

## 4. 统一 JSONL schema：ntsd-r1-cpp-unity-trace-v1

每个 trace 为 UTF-8 JSONL。第一行必须是 header，随后每个 logic tick 至少有一个 tick-begin、零个或多个 checkpoint、一个 tick-end。JSON key 按 UTF-8 字典序 canonicalize；固定数值和 enum 不得依赖本地化字符串。

### 4.1 Header 合同

header 必须至少包含以下字段：

| 字段 | 要求 | 是否参与跨端 header 校验 |
|---|---|---|
| kind | 固定为 header | 是 |
| schema | 固定为 ntsd-r1-cpp-unity-trace-v1 | 是 |
| producer.role | cpp-release / unity-fallback / unity-optimized | 是，但角色不同是预期差异 |
| producer.buildIdentity | 可复现的可执行文件/源码/Makefile 或 Unity project build 标识 | 同类格式校验；不得用时间戳判等 |
| authority | 固定声明 cpp-release-live-path | 是 |
| fixture.id、fixture.version | 固定 fixture 标识 | 是 |
| fixture.initialStateSha256 | canonical 初始逻辑状态摘要 | 是 |
| fixture.semanticDatManifestSha256 | DAT 语义清单摘要 | 是 |
| fixture.stageManifestSha256 | stage 数据/无 stage 声明摘要 | 是 |
| fixture.slotDomain | C++ 观测到的 slot 容量及本 fixture 使用的 slot 范围 | 是 |
| journal.schema、journal.sha256、journal.firstTick、journal.tickCount | 输入 journal 合同 | 是 |
| simulation.tickRate、startTick、seed | 固定逻辑 tick 与种子 | 是 |
| fieldRegistry.version、fieldRegistry.sha256 | 字段比较合同 | 是 |
| passMap.version、passMap.sha256 | C++ checkpoint 到 source segment 映射 | 是 |
| traceProfile | full / focused；R1 acceptance 只能使用 full | 是 |
| metadata.runId、createdUtc、machine | 可诊断元数据 | 否，比较器必须忽略 |

推荐的 header 形状如下；它说明目标格式，不授权现在实现：

~~~json
{
  "kind": "header",
  "schema": "ntsd-r1-cpp-unity-trace-v1",
  "producer": {
    "role": "cpp-release",
    "buildIdentity": {
      "target": "ntsd_new.exe",
      "livePath": "src/entity/game_tick.cpp::game_tick",
      "sourceRevision": "captured-by-producer"
    }
  },
  "authority": "cpp-release-live-path",
  "fixture": {
    "id": "R1-FX-...",
    "initialStateSha256": "...",
    "semanticDatManifestSha256": "...",
    "stageManifestSha256": "...",
    "slotDomain": { "capacity": 400, "activeRange": [0, 399] }
  },
  "journal": {
    "schema": "ntsd-r1-frame-input-journal-v1",
    "sha256": "...",
    "firstTick": 1,
    "tickCount": 60
  },
  "simulation": { "tickRate": 30, "startTick": 0, "seed": 12345 },
  "fieldRegistry": { "version": 1, "sha256": "..." },
  "passMap": { "version": 1, "sha256": "..." },
  "traceProfile": "full"
}
~~~

capacity 400 只是在 Authority400 fixture 中的预期值；producer 必须实际写出它观测到的容量。若 C++ 实际 header 与 Authority400 fixture 合同不符，比较器应报告 fixture/header 失败，而不是静默截断。

### 4.2 Tick、checkpoint 与事件记录

每个 tick 必须保留严格序号，以避免同一 pass 内新增对象、slot reuse 或候选消费被压缩成一个 hash：

~~~json
{
  "kind": "checkpoint",
  "tick": 17,
  "sequence": 12,
  "checkpoint": "collision.candidate_collect.post",
  "edge": "post",
  "sourceSegment": "cpp.game_tick.step6",
  "mappingStatus": "exact",
  "snapshot": {
    "world": {},
    "slots": [],
    "rests": {},
    "events": {
      "candidate": [],
      "consume": [],
      "lifecycle": [],
      "renderHandoff": []
    }
  }
}
~~~

- tick-begin 在 game_tick 自增后的第一个可观察边界；它必须包含 journal 中实际应用的输入，而非仅原始按键轮询结果。
- checkpoint 的 sequence 由 producer 按实际发生顺序递增，不得按字母排序。
- sourceSegment 指向当前 producer 的真实代码段；例如 Unity 的 PreInteraction 不能伪装为 C++ step10。
- mappingStatus 只能为 exact、composite、pending 或 absent。pending / absent 是有效但不可比较的合同结果，必须在 first-difference 报告中显示，不能被 hash 掩盖。
- tick-end 必须记录本 tick 实际结束状态、RNG 状态/调用计数（若 C++ live path 可观测）以及是否发生 C++ 早退。

### 4.3 C++ 锚点与统一 checkpoint 名

下表定义 R1 的首版 C++ checkpoint 命名。每个名字都源于 game_tick(...) 的静态边界；它们不是对 Unity 已对齐的声明。

| C++ checkpoint | C++ live source 边界 | 必需观察内容 |
|---|---|---|
| tick.begin | game_tick.cpp:945–950 | tick、input phase、world 基础状态、bootstrap 后 RNG 状态 |
| cooldown.post | 990–1000 | aRest/vRest 与相关 cooldown 结果 |
| post_cooldown_input.post | 1002–1005 | 实际进入 C++ input callback 的输入、边沿与 callback 后状态 |
| early_runtime_specials.post | 1006–1246 | C++ 在 frame logic 前执行的特殊状态/输入前处理；字段不足时须标 pending，不得虚构 |
| frame_logic.post | 1247–1260 | 各 slot frame logic 后状态与事件 |
| frame_advance.post | 1261–1278 | 帧推进、opoint/出生、速度/位置/帧号变化 |
| post_frame_advance_cleanup.post | 1280 起至 step5 前 | state 9998、死亡/复活及 slot 活跃变化 |
| stage_z_clamp_1.post | 1423–1439 | 角色 Z、ZInt、stage bounds |
| held_link_1.post | 1441–1643 | 第一轮 link/holder/held 与挂点同步结果 |
| collision_snapshot.post | 1645–1651 | prev_frame2 与任何候选前快照字段 |
| collision.candidate_collect.post | 1652 | candidate 序列、来源 slot、目标 slot、顺序 |
| collision.character_consume.post | 1653–1656 | 角色 collision consume 序列与状态变化 |
| random_weapon_drop.post | 1657–1817 | RNG 调用、候选 OID、slot 搜索、出生/失败事件 |
| collision.object_consume.post | 1818–1822 | 对象 collision consume 序列与状态变化 |
| cpoint.post | 1823–1825 的 run_cpoint_runtime_pass 后 | CPoint 关系、catch/release 变化 |
| weapon_sync.post | 1823–1825 的 weapon_sync_runtime_pass 后 | held weapon frame、位置、速度、holder 关系 |
| positive_link_validation.post | 1827–1846 | 正 link/target/holder 验证后的关系 |
| stage_z_clamp_2.post | 1848–1859 | 第二轮 Z clamp 结果 |
| held_link_2.post | 1860–2019 | 第二轮 held/link 结果 |
| preframe.post | 2021–2066 | 边界、stage/camera/render carrier；具体 camera 比较规则待 R6 决定 |
| stage_wave.post | 2070–2071 | 当前 wave 与即时 stage spawn |
| render_handoff.post | 2072 | shadow/entity/hit-record descriptor 与严格交错顺序 |
| frame_postprocess.post | 2078–2079 | postprocess 结果 |
| late_entity_update.post | 2080–2083 | late frame/opoint、死亡、回收、pending 生命周期事件 |
| random_weapon_tail.post | 2084–2086 | mode2/tail 随机武器事件 |
| entity_postframe_tail.post | 2084–2087 | postframe/heal/catch 清理 |
| tick.end | 2088–2090 或 C++ 早退边界 | 最终世界、slot、事件与早退状态 |

早退时，C++ producer 必须发出 early-return 事件并写出 tick-end；后续未执行 checkpoint 以 absent 表示。Unity 不能靠跳过记录伪装成一致。

### 4.4 字段 registry

字段 registry 不是对现有 C# 字段的机械复制。每个字段必须包含：

- canonicalPath：如 slot.frame、slot.position.xInt、world.stage.zMin；
- valueType：bool、i32、u32、enum、slot-ref、f64-raw、sequence；
- sourceBinding：C++ live 的文件/函数/字段读取点与 Unity 的真实 runtime 读取点；
- comparison：exact、ordered-sequence、capture-only 或 pending；
- evidence：VERIFIED / INFERRED / UNKNOWN；
- absentValue：跨端空值、无 slot 或未适用时的统一表达；
- notes：任何 DAT 语义、整数截断或 Unity adapter 说明。

R1 首版必须覆盖下列逻辑域。某个域未能完成 C++ binding 时，registry 标为 pending，比较器报 coverage gap，而不是把 Unity/C# 字段当成补充 authority。

| 域 | 必需 canonical 字段 / 事件 | 初始比较原则 |
|---|---|---|
| World | tick、objectCount、inputPhase、gameMode、stage bounds、RNG state/call count、battle early-return | 整数/enum exact；RNG 未闭合时 pending |
| Slot identity | slot、active、oid、runtime category | exact；不比较 Unity generation/stable id |
| Frame / physics | frame、prevFrame、prevFrame2、x/y/z、xInt/yInt/zInt、vx/vy/vz、facing、frameDelay、hitStop | 整数/enum exact；浮点先 capture，只有 C++ 语义归一化确认后才进入 equality |
| Combat | HP、PP、aRest、vRest、attack/hit counters、damage/kill/combo 所需统计 | exact 或 ordered pair matrix；pair 方向必须写入 |
| Relations | link、holder、held weapon、target、caught、catcher、相关 slot ref | slot-ref exact；无效值必须统一编码 |
| Collision | candidate collect、consume、reject/release 与处理顺序 | ordered-sequence；每项带 eventOrdinal |
| Lifecycle | activate、deactivate、spawn、despawn、slot reuse、pending flush | ordered-sequence；原因码无 C++ binding 时 capture-only |
| Render handoff | shadow/entity/hit-record descriptor、sort key、slot/order、anchor/rect/facing | 必须 capture；camera/perspective 的最终 equivalence policy 由 R6 前单独裁定 |

禁止把 Unity Transform、GameObject instance id、Sprite、Mesh、Unity material、GC/性能计数或 C# stable id 写入 required C++ equality 域。它们可保留在 producer.unityDiagnostic，且比较器必须默认忽略。

### 4.5 数值和不存在值规则

1. 整数、bool、enum、slot ref 与显式 bitmask 一律 exact compare。
2. 不设置全局 float epsilon。x/y/z/vx/vy/vz 先同时输出 raw 表示与经 C++ 证据确认的 logical normalization；没有 normalization binding 的字段只能 capture-only。
3. raw floating value 必须带类型和可重现表示；不能用本地化 ToString 或省略精度。
4. slot ref 的无效值、inactive slot、null frame data 与未执行 pass 必须分别编码，不能都折叠为 0。
5. 候选、consume、lifecycle、render descriptor 一律按 eventOrdinal 比较，不能先排序后比较。
6. hash 只用作快速定位；命中 hash 后必须展开到 checkpoint、slot、字段/事件。只有 hash 相同不能签发对齐结论。

## 5. C++ checkpoint 到当前 Unity pass 的静态映射

下表是本 WP 的 source-level crosswalk。C++ 和 Unity 的文件位置为 VERIFIED；“当前映射”是静态映射，均为 INFERRED，直到同 fixture trace 证明其时序与字段结果。

| C++ checkpoint | 当前 Unity 位置 | 映射状态 / 风险 |
|---|---|---|
| tick.begin | NTSDBattleTickSystem.RunTick 221–242 的 BattleFlow 前后 | pending：C++ game_tick 内与 Unity BattleFlow 的完整边界尚未闭合。 |
| cooldown.post | TickCooldowns 254–256 → SimulationWorld.RunBattleEcsCooldownPass | composite：函数名称相近，字段与 slot 顺序待 trace。 |
| post_cooldown_input.post | PostCooldownHumanInput 257–259；随后 CharacterInput 294–296 | composite：C++ callback、Unity human/AI/character input 的边沿与调用边界需分别记录。 |
| early_runtime_specials.post | RuntimeMaintenance 282–284、InputClear 285–291、CharacterInput 294–296、EarlyFrameAdvance 298–300 | pending：C++ 1006–1246 包含多个特殊状态循环，不能仅按名称合并。 |
| frame_logic.post | FrameLogic 301–303 → FrameLogicBeforeAdvanceAll | candidate exact source label，行为仍待 trace。 |
| frame_advance.post | FrameAdvance 304–306 → SerialTickAll | candidate exact source label，出生/slot cursor 待 trace。 |
| post_frame_advance_cleanup.post | DeathCleanup 307–309 → PostFrameAdvanceDeathCleanupAll | composite：C++ cleanup 域比 Unity 当前 phase 名称宽，需 field binding。 |
| stage_z_clamp_1.post | StageBounds 310–312 → ClampCharacterZToStageBoundsAll | candidate exact source label；边界来源/整数转换待 trace。 |
| held_link_1.post | PreInteraction 313–315、HeldLinkValidation 316–318、HeldProcess 322–324 | composite：当前 Unity 以多个 phase 承担这一域，不能先假定等价。 |
| collision_snapshot.post | CollisionSnapshot 325–327、PairVRest 328–330 | composite：C++ prev_frame2 与 rest cooldown 的具体边界必须独立输出。 |
| collision.candidate_collect.post | CandidateCollect 331–333 | candidate exact source label；candidate 内容和顺序待 trace。 |
| collision.character_consume.post | CharacterHitConsumePostInteraction 341–343 → PostInteractionTickAll | candidate exact source label；C++ category/type gate 待 trace。 |
| random_weapon_drop.post | RandomWeaponDrop 344–346 | candidate exact source label；RNG 原子调用序列待 trace。 |
| collision.object_consume.post | ObjectHitConsume 347–349 → ObjectInteractionTickAll | candidate exact source label。 |
| cpoint.post / weapon_sync.post | PreInteractionTickAll: RunCpointCheckStep10 2286、RunCpointMismatchTailStep10 2311、RunWeaponSyncHeldStep10 2345；调度发生于 candidate collect 前 | INFERRED static order risk：C++ 在 object collision 后的 step10/10.5 才运行。此处不是 VERIFIED behavior mismatch，R1 trace 必须首先保留 witness。 |
| positive_link_validation.post | HeldLinkValidation 316–318 → ValidateHeldLinksAll | mapped source exists，但相对 C++ step11 的位置当前不同，待 trace。 |
| stage_z_clamp_2.post / held_link_2.post | 第二个 StageBounds 319–321 与 HeldProcess 322–324 | composite；当前发生在 candidate collect 前，待 trace。 |
| preframe.post | PreFrameBounds 361–363 → ApplyPreFrameBoundsAll | candidate source label；C++ camera behavior 不在此 WP 裁定。 |
| stage_wave.post | Stage 364–366 → CurrentWaveStageTickAll | candidate source label。 |
| render_handoff.post | RenderDispatch 367–369 → RenderDispatchAll；中央表现由 StageRender partial 中 BattlePresentation 处理 | composite：只比较 logical descriptor / command handoff，不比较 Unity Mesh 或 Camera Transform。 |
| frame_postprocess.post | FramePostProcess 370–372 → RunBattleEcsFramePostProcessPass | candidate source label。 |
| late_entity_update.post | LateEntityUpdate 373–375 → LateEntityUpdateAll | candidate source label；newborn/reuse 事件必须有顺序。 |
| random_weapon_tail.post | RandomWeaponDropTail 376–378 → Mode2RandomWeaponDropTailAll | candidate source label。 |
| entity_postframe_tail.post | EntityPostFrameTail 379–381 → EntityPostFrameTailAll | candidate source label。 |
| tick.end | RunTick finally 269–275 之后的 observable state | pending：Unity shadow refresh 不是 C++ gameplay checkpoint，必须标为 diagnostic-only。 |

本表没有授权修复任何顺序。任何 trace 表明的 first difference 必须先作为 R1 witness 记录；只有用户确认后，才可由 R2 处理调度器或 adapter。

## 6. Fixture、DAT、stage 与 FrameInputSet / input journal 合同

### 6.1 固定初始状态 bundle

每个 R1 fixture 必须是可单独复制、可重放的 bundle，至少包含：

| 文件 | 内容 |
|---|---|
| fixture.json | id、版本、tick rate、seed、mode、difficulty、slot domain、fixture policy |
| initial-state.json | 每个 slot 的初始 active/oid/team/control、已确认的初始逻辑字段与 canonical digest |
| semantic-dat-manifest.json | C++ DAT 输入与 Unity adapter 输入的语义映射、必要字段、各自 payload digest、缺口 |
| stage-manifest.json | stage source、payload digest、selected stage/wave、bounds 与明确的 no-stage 声明 |
| input.journal.jsonl | 每 tick、每玩家的离散输入记录 |
| expected-coverage.json | 本 fixture 预期经过的 checkpoint、事件域与允许 absent 项 |

原始 DAT 文件不要求 byte-identical。Unity 的读取/适配差异是已知前提；但若一个行为字段来自 DAT，则 semantic-dat-manifest 必须说明：

1. C++ 读取的 source 文件与字段语义；
2. Unity 适配后的 source 文件/字段；
3. 两者被认为等价的理由；
4. 当前证据等级。

任一必需 DAT 字段无法建立语义映射时，fixture 状态为 UNKNOWN，不得用“DAT 文件不同是正常的”跳过该字段。

### 6.2 初始 slot、seed 与 stage

- Authority400 fixture 必须固定 slot 域、初始 active slot 与 slot reuse 起点；slot 不能由每次加载顺序随机决定。
- seed 必须在 bootstrap 前写入，并在 bootstrap 后记录 C++ / Unity 实际 RNG state 与 call count。两者不同首先是 witness，不可由旧 C# RNG 结论修补。
- stage 必须有明确 payload digest。若使用无 stage 夹具，则 stage-manifest 必须显式写 no-stage 与所有直接提供的 bounds；不得生成或部署默认 stage.dat 来使测试变绿。
- 初始逻辑状态 hash 只覆盖 field registry 中已绑定的字段。未绑定字段必须逐项列在 coverage gap，不得静默省略。

### 6.3 输入 journal

input journal schema 固定为 ntsd-r1-frame-input-journal-v1。每条记录至少包含：

| 字段 | 含义 |
|---|---|
| tick | 应用于该 logic tick 的连续正整数 |
| playerSlot | 玩家/控制槽位，不是 Unity GameObject id |
| heldMask | 该 tick 完整 held bitmask |
| pressedMask | 本 tick 明确按下边沿 |
| releasedMask | 本 tick 明确释放边沿 |
| source | human、ai-decision 或 fixture |
| ordinal | 同一 tick 输入写入顺序 |

button 位含义必须在 header 中显式列出 right、left、up、down、attack、jump、defend。FrameInputSet 已具有 held/pressed/released 三个概念，但它只是 Unity 侧接口事实；C++ input_handler 的注入、轮询与组合键边沿映射仍是 UNKNOWN，必须由 R1-WP04 用 C++ live path 证明。

AI 不得被隐式视为“无输入”。fixture 要么关闭 AI，要么将该 tick 的 AI 决策/输入作为可观察 journal/event 输出；AI 规则本身的 C++ 对齐仍属于后续 R3 行为分析。

## 7. Comparator 与 first-difference 合同

### 7.1 比较顺序

比较器必须按下列总序执行，而不是整帧 hash 的字典序：

1. header/fixture/journal/registry/pass-map 合同；
2. tick；
3. checkpoint sequence；
4. checkpoint presence、edge、sourceSegment 与 mappingStatus；
5. world 字段 registry 顺序；
6. event domain、eventOrdinal；
7. runtime slot 升序；
8. slot 内 field registry 顺序；
9. rest/stat pair 的明确二维顺序。

第一项差异即 first difference。比较器仍可继续生成摘要，但不得把“后续差异更多”覆盖首个 witness。

### 7.2 三方结论

| 状态 | 含义 |
|---|---|
| all-match | C++、Unity fallback、Unity optimized 都满足当前字段规则 |
| fallback-diverges | fallback 首先与 C++ 分叉；optimized 是否同样分叉另报 |
| optimized-only-diverges | fallback 与 C++ 一致，optimized 与 C++ 分叉 |
| both-unity-diverge | 两条 Unity 路径都与 C++ 分叉，且可在同/不同 checkpoint 或字段发生 |
| contract-gap | fixture、header、source mapping 或 field binding 不足，不能判断行为 |
| capture-only-difference | 已记录但当前未裁定的域，例如尚未决定的 camera policy；不能记为对齐成功或失败 |

### 7.3 first-difference 输出

first-difference report 固定为 ntsd-r1-first-difference-v1，至少输出：

~~~json
{
  "status": "fallback-diverges",
  "fixture": { "id": "R1-FX-...", "initialStateSha256": "...", "journalSha256": "..." },
  "firstDifference": {
    "tick": 17,
    "checkpoint": "cpoint.post",
    "checkpointSequence": 15,
    "sourcePass": {
      "cpp": "cpp.game_tick.step10.cpoint",
      "unityFallback": "unity.preinteraction",
      "unityOptimized": "unity.preinteraction"
    },
    "runtimeSlot": 42,
    "eventOrdinal": null,
    "field": "slot.relation.holderSlot",
    "comparison": "exact-slot-ref",
    "cppValue": { "value": 3, "representation": "i32" },
    "unityFallbackValue": { "value": -1, "representation": "i32" },
    "unityOptimizedValue": { "value": -1, "representation": "i32" }
  },
  "reproduction": {
    "inputTickRange": [1, 17],
    "replayKind": "prefix-not-yet-minimized",
    "fixtureBundle": "R1-FX-..."
  }
}
~~~

report 还必须带：

- C++、fallback、optimized 的 build identity 与完整 header 摘要；
- 在首差异之前最近一个 all-match checkpoint；
- 前后各一个 checkpoint 的上下文窗口；
- 触发该 slot 的 candidate / consume / lifecycle / render handoff 事件窗口；
- contract-gap、capture-only 域和未覆盖 checkpoint；
- 最短已知重现步骤。

“最短”有严格定义：初次报告只能写 prefix-not-yet-minimized，重现步骤为从 fixture 起始到 first difference 的完整 journal 前缀。只有在至少两次独立重放一致，并完成 journal prefix/事件 delta reduction 后，才可标记 minimized。若缩减改变 C++ trace，本报告保留原始前缀，不能伪称最短。

## 8. 后续可实施 R1 Work Package

以下工作包均需用户在 R1-WP01 后单独批准。它们不等价于批准 R2。

### R1-WP02 — C++ Release read-only trace acquisition

**Goal**
从未修改、实际构建为 ntsd_new.exe 的 C++ Release live runtime，以只读外部观察方式取得 cpp-release trace 或明确证明无法安全取得。

**Scope**
只允许检查和使用 C++ Release 现有的 stdout/stderr、现有诊断/日志开关、进程可读输出、既有命令行、既有输入方式或外部只读观察能力。所有采集结果、stdout/stderr 重定向、run manifest 和后续比较资料都必须写入非 authority 目录。

严禁修改、重建、复制替换或重新配置 C++ 工程中的源码、Makefile、可执行文件、DLL、资源、DAT、设置和输出目录；严禁新增 C++ trace sink、fixture bootstrap、输入 bridge、CLI 参数或诊断文件写入。

**Authority / Evidence**
唯一行为证据为 J:\QQFile\NTSD2.4\ntsd_release 的 Makefile、game_tick.cpp 与实际 ntsd_new.exe trace。C#、Unity、旧 NTSDParity 只可辅助命名或格式设计。

**Files likely involved**
只读检查：J:\QQFile\NTSD2.4\ntsd_release\ntsd_new.exe、现有运行脚本/说明、现有 stdout/stderr 或诊断输出、`src/entity/game_tick.cpp`、相关 live entity/input/render 模块和 Makefile。写入范围仅限 Unity 仓库的非 authority 采集目录、`docs/ai/` 状态/hand-off 和后续获准的独立比较资料目录。

**Unknowns**
现有 Release 是否已经提供足以覆盖 checkpoint 的非侵入输出；是否可把输出定向到非 authority 目录；是否存在不修改 C++ 的可重复输入方式；是否能取得 seed、初始 slot、DAT/stage 语义、RNG、tick/pass/slot 字段；窗口/GUI runtime 的安全自动化方式。

**Deliverables**
只读采集能力清单和安全性证明；C++ Release 文件/可执行身份 manifest；若现有通道足够，保存于非 authority 目录的一条 raw trace/日志及其采集 manifest；若不足，结构化 blocker，明确缺失字段、可见通道和禁止采用的修改方案。

**Verification**
验证 Release target 与文件身份；采集前后 C++ authority 目录未被写入或修改；输出文件仅出现在非 authority 目录；若能运行同一既有输入两次，观察到的原始输出可重复。仅当已有输出实际覆盖合同字段时，才标为可用于 R1；否则标 blocker，不能凭静态源码补全。

**Stop conditions**
不存在安全、只读、非侵入的观察通道；采集会向 C++ authority 目录写入；必须修改/重建/配置 C++ 才能取得所需字段；缺少可重复运行环境或必要输入；C++ source 与实际 executable 身份无法闭合。

**Out of scope**
Unity trace、comparator、R2 调度修复、性能优化、用 debug/diagnostic binary 替代 release 行为、对 C++ 进行任何插桩/构建/配置改动、服务器与 Android。

### R1-WP03 — Unity Authority400 fallback / optimized trace instrumentation

**Goal**
让 Unity 的 Authority400 world 以同一 schema 输出 unity-fallback 与 unity-optimized trace，并如实标记真实 source phase。

**Scope**
只增加读观察点、field projection、producer mode 选择和 trace 输出。fallback 与 optimized 必须共享同一个 world bootstrap、FrameInputSet journal 与 tick 入口；不能为得到一致而重新排序或改变 gameplay。

**Authority / Evidence**
C++ trace 合同和 C++ live source 是行为锚点。BattleParitySnapshot、BattleParityTraceEditor、Authority400 与现有 fast-path diagnostics 仅是可复用的历史工程材料。

**Files likely involved**
Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs、SimulationWorld.Passes.partial.cs、SimulationWorld.StageRender.partial.cs、BattleParitySnapshot.cs、SimulationTickDriver.cs、FrameInputSet.cs，以及新的 R1 trace adapter/editor harness 文件。

**Unknowns**
当前每个 proof/fast path 的可独立回退开关、worker 路径与 single-thread trace 的合同、central presentation descriptor 的稳定读取点、Unity runtime 字段到 registry 的最终 binding。

**Deliverables**
两个 Unity producer、每条 checkpoint 的 sourceSegment/mappingStatus、fallback/optimized mode manifest、focused trace-only test entry。

**Verification**
Unity 编译与 focused trace test；同一 fixture 的 fallback/optimized trace header 等价（除 producer.role/path）；关闭/启用目标优化不改变 trace instrumentation 的观察位置；不启动 Play Mode 作为此 WP 的唯一验证手段。

**Stop conditions**
必须改 gameplay 才能输出 trace；fallback 无法定义为完整逻辑路径；Unity 的默认 producer 与 worker/legacy 路径所有权不清；任何 producer 写回 runtime。

**Out of scope**
按 C++ 修复 pass、CPoint、WeaponSync、held/link、collision、input、opoint、renderer 或技能；1000 AI 性能目标。

### R1-WP04 — 固定初始状态、DAT 语义夹具与 input journal

**Goal**
实现可同时供 C++ release 和 Unity 消费的 fixture bundle，并让每个 logic tick 的输入可记录、可重放、可摘要。

**Scope**
建立 fixture schema、semantic DAT manifest、stage manifest、fixed slot roster、seed/bootstrap capture 与 FrameInputSet/C++ input adapter。只做数据合同和装载桥接，不修正战斗行为。

**Authority / Evidence**
C++ release DAT 读取与 bootstrap live path 定义字段语义；Unity DAT adapter 只能声明对应实现。DAT 原始字节不同不自动构成错误，也不自动构成等价。

**Files likely involved**
新增 R1 fixture/journal 文件与加载器；C++ battle bootstrap/input live path；Assets/NTSD/Scripts/Simulation/Input/FrameInputSet.cs、SimulationFrameInputModule.cs、现有外部 DAT/trace fixture editor 代码；Tools/NTSDParity 可被审查为历史格式输入。

**Unknowns**
所有必需 DAT 字段的完整 C++→Unity semantic mapping、C++ player slot/control binding、stage no-data 的正式语义、AI journal 的最小可控入口。

**Deliverables**
至少一个 no-input 基础 fixture 和一个有输入边沿的基础 fixture；bundle digest；journal validator；coverage manifest。

**Verification**
同 bundle 在两端生成相同 journal/hash 合同；slot/seed/stage/DAT 缺项 fail closed；input journal 重放两次不发生 tick 漂移。此验证只证明 fixture 合同，不证明 gameplay 对齐。

**Stop conditions**
需要伪造默认 stage.dat；无法在 C++ live path 固定初始 slot；DAT 字段只能靠 C# 推断；输入注入会改变正常 gameplay 逻辑。

**Out of scope**
stage.dat 默认资产部署、AI 行为修复、网络服务器、R2 调度改动。

### R1-WP05 — 三方 trace normalizer / comparator

**Goal**
根据本 schema 实现 streaming 三方比较器，输出可审计的 first-difference 或 contract-gap。

**Scope**
解析 header、canonical JSON、field registry、checkpoint/event 顺序与三方值；实现 C++→fallback、C++→optimized、fallback→optimized 三种结果。可以复用旧 NTSDParity 的非行为性序列化经验，但不得继续将 C# trace 当 authority。

**Authority / Evidence**
R1-WP01 schema、C++ trace producer 的 header/source binding、field registry。旧 v3/v4 comparator 仅是历史实现参考。

**Files likely involved**
新的 R1 compare tool、fixture/registry JSON、可能的 Tools/NTSDParity 共用 canonical JSON 工具或其替代；不得修改 Unity gameplay 或 C++ gameplay。

**Unknowns**
跨语言 f64 logical normalization 的 C++ binding、event reason code 的完整枚举、producer 缺失 checkpoint 的最终诊断分类。

**Deliverables**
命令行/测试入口、first-difference JSON、contract-gap JSON、malformed/ordering regression cases、可读摘要。

**Verification**
人为构造 header、tick、pass、event 顺序、slot 与字段的正/负样本；确认 first mismatch 位置稳定，且 comparison 不被 hash、字段排序、Unity generation 或机器元数据掩盖。

**Stop conditions**
需要在 comparator 中补写 C++ 行为规则；必须将 capture-only 字段强行判等；无法区分 contract-gap 与 gameplay witness。

**Out of scope**
修复任何 mismatch、执行长期性能测试、宣称 C++ 完整对齐。

### R1-WP06 — first-difference replay harness

**Goal**
把固定 fixture、三方 producer 与 comparator 串成可重复的一键 witness 流程，并生成最短已知重现信息。

**Scope**
只编排现有 producer/fixture/comparator，不改变战斗规则。支持完整前缀 replay、二次一致性重跑与可选 journal 缩减。

**Authority / Evidence**
C++ 实际 ntsd_new.exe trace 是起点；Unity 两条 trace 是比较对象。R1-WP02 至 WP05 的已验证交付物是前置条件。

**Files likely involved**
新的 R1 harness 脚本/工具、fixture bundle、输出目录约定、CI/Editor-only request 文件；不直接修改 NTSDBattleTickSystem 的 gameplay。

**Unknowns**
C++ executable 的非交互启动协议、Windows/Unity 同时运行的资源/窗口限制、最小化算法能否在所有 fixture 上保持确定性。

**Deliverables**
单 fixture run manifest、三方 trace、comparison report、first-difference witness、前缀或已验证 minimized journal。

**Verification**
同 fixture 至少两次独立运行得到相同 first difference 或 all-match；缺 trace/缺 pass/不连续 tick 必须 fail closed；报告可在不阅读聊天记录的情况下复现。

**Stop conditions**
跑 harness 需要改 gameplay；同一 C++ fixture 重跑不确定；mismatch 落在未覆盖/未裁定域；需要变更长期验收标准。

**Out of scope**
R2 修复、批量角色专项调试、1000 AI 压测、服务器功能。

### R1-WP07 — R1 acceptance fixtures 与证据登记

**Goal**
建立中性、模块化的 R1 最小 fixture 集，证明 trace 合同覆盖主 tick 的关键域，而不把某一个角色/技能修复当作通用结论。

**Scope**
覆盖空场/基础 tick、人类输入边沿、frame/lifecycle、candidate/consume、held/cpoint/weapon、stage/render handoff 六类最小场景。角色名称仅作为数据 fixture 标识，不形成专项 gameplay 修复计划。

**Authority / Evidence**
每个 fixture 的 C++ release live trace、对应 Unity fallback/optimized trace、field registry 与 first-difference report。历史 self-check 仅可帮助选择回归素材。

**Files likely involved**
R1 fixture bundle、coverage manifest、trace reports、docs/ai 状态/证据记录；必要的 trace-only test/harness 文件。

**Unknowns**
最小 fixture 是否能覆盖 C++ 早退、特殊 runtime state、全部 collision kind 与 renderer 的全部可见分支；这些缺口必须显式登记。

**Deliverables**
fixture matrix、每项 coverage/unknown 表、至少一个 all-match 或明确 first-difference witness（取决于实际结果）、R1 evidence register 更新。

**Verification**
每个 fixture 都通过 header/fixture/journal validation，三方 trace 连续且可解析；只对已覆盖且可比较的域给出结论；没有把 no mismatch 扩大为整体战斗对齐。

**Stop conditions**
fixture 需要默认 stage.dat 部署；需要修改 battle rules 才可运行；C++ trace/Unity trace 任一方仍不具备；用户要求直接进入 R2。

**Out of scope**
R2–R8 gameplay 实施、整体 C++ release 对齐宣称、T8 默认资产部署、Android 验收与服务器实现。

## 9. 依赖、顺序与当前推荐

建议的实施依赖为：

~~~text
R1-WP02 C++ producer ─────┐
                            ├─> R1-WP06 replay harness ─> R1-WP07 acceptance evidence
R1-WP03 Unity producers ──┤
R1-WP04 fixture+journal ──┤
R1-WP05 comparator ───────┘
~~~

R1-WP02、R1-WP03、R1-WP04 在 schema 固定后可分别实施，但任何一个都不得替代另两个。R1-WP05 可以先完成 parser/negative tests，最终 integration 依赖真实 producer。R1-WP06/07 必须等待前置项全部具备。

推荐第一个实施型工作包是 **R1-WP02：C++ Release read-only trace acquisition**。原因不是它先修逻辑，而是从未修改的 C++ Release runtime 获取的证据，才可能把后续 Unity 指标从“历史自洽”变成“对 C++ 可比较”。如果现有外部通道不足，R1-WP02 的正确结果是 blocker，而不是 C++ instrumentation。开始前仍需用户确认；R1-WP01 不授权实施它。

## 10. R1-WP01 验证与停止记录

已执行的验证仅限只读/文档范围：

- 完整读取用户指定的六份状态材料；
- 静态读取 C++ release Makefile、game_tick(...) 与 Unity NTSDBattleTickSystem / SimulationWorld 的相关调用点；
- 静态确认旧 NTSDParity / Authority400 是以 C# / Unity 历史 parity 为前提的诊断资料。

未执行且不能据此声称完成：

- C++ release 构建或 ntsd_new.exe 运行；
- C++ trace、Unity fallback trace、Unity optimized trace；
- Unity 编译、BattleRuntimeSelfCheck、Play Mode、性能或 0 GC 测试；
- R2 或任何 gameplay 行为修复。

### 10.1 Stop-condition review

| 本 WP stop condition | 结果 |
|---|---|
| authority 无法从 C++ release live path 闭合 | 未触发。已静态闭合到 Makefile target、game_tick(...) 与相关 release 模块；这不是 runtime 行为验证。 |
| 发现需要直接实现 trace 或修改 gameplay | 对“规划工作包”已触发边界但不是阻塞：当时合同把 trace implementation 拆到后续工作包；2026-08-21 的 R1-WP02 read-only amendment 已禁止通过修改 C++ 实现 trace。 |
| first mismatch 指向 scope 外模块 | 当前不适用：尚未生成任何 C++/Unity trace 或 first-difference witness。 |
| 需要改变长期计划、架构、pass ordering 或验收标准 | 未触发。本文只细化既有 R1 的证据合同，明确禁止用其修复 pass。 |
| 用户提出新的 Change Request | 未触发。当前用户请求正是限定 R1-WP01 规划。 |

### 10.2 R1-WP02 C++ Release read-only amendment（2026-08-21）

用户明确要求 C++ Release 工程保持只读。R1-WP02 因此只允许从未修改的 C++ Release runtime 以只读方式获取 trace，并在非 authority 目录保存采集结果和比较资料。此 amendment 覆盖本 Task Contract 中任何可能被理解为“新增 C++ instrumentation / trace sink / fixture bridge / C++ 文件输出”的旧表述。

规划完成后应停止，等待用户确认下一工作包。
