# D-024 Y 坐标双域：第一轮消费者审计（2026-10-05）

状态：`READ_ONLY_CONTRACT_DRAFT / IMPLEMENTATION_PENDING`。本文件是原 Battle Scene 单例确认高度画面比例差异后的实施边界审计，不是新增规则 authority，也不表示全消费者已闭合。[自然 Play 原件与比例结果](REPORT.md)。用户已批准保留当前 `BattleVisualScale=1.5` 本体图像尺寸；该例比较同一 action212 的本体相对阴影移动，不要求缩放图像尺寸或更改 DAT。

## 已证事实

- 正式 `physics_integrator.cpp` 先在源规则 Y/precise_y 中积分、落地/平台判定和同步整数；`object_spawning.cpp` 的 OPoint Y 是 `parent.position.y + point.y - parent.frame_center_y`，速度直接取 `dvy`。正式 `render_snapshot.cpp` 普通本体屏幕纵坐标是源 Z + 源 Y - `center_y`。正式根同输入 OID85/action212/Z402 的 Y=-22→-20 自然样本已存在。
- Unity `NTSDEntityRuntime.Y/YInt/Vy` 当前既用于逻辑，又以未缩放数值送往中央和 Legacy 本体。`CharacterMechanics` 对 Y 加原值 Vy，依原值 floor/landing 判定；`LF2Character`、`LF2Entity`、状态/伤害/OPoint 消费者也直接读写源 Y。`BattleSpatialProjection` 目前只有 X、Z 两轴，Z 的 `DepthScale=1152/730` 用于当前固定视野。不能通过单纯修改 Vy 或所有 `Runtime.Y` 赋值来修复显示比例，否则正式落地、帧选择、Y 向抓取和出生会改变。
- 中央 `BattlePresentationShadowBuild.MaterializeCommands` 的阴影位置使用已转换的 ground Z，而本体 `ComputeEntityBottomCenterPivotPixels` 使用 `DisplayZ + YInt`；同入口的稳定头顶血条 anchor 使用 `DisplayZ + YInt`。Legacy `LF2ObjectRenderer.ApplyCppDrawEntityPosition` 调用同一个 pivot 方法。正式视觉尺寸倍率只作用于 sprite 本地宽高/pivot 偏移，独立于全局高度位移。
- `BattlePresentationMotionSampler` 已在**源域**作连续性和 `lround`，再把 X/Z delta 转成视图位移，Y delta 尚为原始整数；`BattlePresentationDisplayMotion.ToWorldBody` 用 raw `delta.Y + delta.ViewZ`，`ToWorldGround` 只用 Z。中央/Legacy 及部分 overlay 共用这个插值出口；如果只改离散 pivot，R120/R30 之间会出现高度跳变。
- `BruteForceSceneQuery` 的普通 bdy/itr Y 矩形以 `Runtime.YInt - frame.centery + localY` 构造，X 轴已按 `BattleSpatialProjection` 投影；`NativePreviousY104`、平台高度交叉与 release full-height 特判仍在源值域。碰撞层与显示若采用不同空间，需明确互相映射；不能盲乘本地 DAT `center_y`/bdy/itr offset 或把平台 floor 值改为视图值。
- 更窄的碰撞消费者追踪：`LocalRectWorldRect` 和 `ItrWorldRectExeRaw` 是普通 bdy/itr 世界 Y 端点出口，`TryBuildBodyBattleVolume` 又把该端点交给 `PhysicsState.BattleVolume`；平台交叉用原始 `NativePreviousY104/YInt`，应保持源值。`CollisionDepthRange` 已将源 Z 端点分别投到视图；Y 如做物理 rect 投影，也需沿“源端点先算、每端点只投影一次”的方式，并保护 int.MinValue/full-height 特判。仅改本体 pivot 不能代表碰撞空间已经对齐。
- `LF2CharacterDatHitResolver` 的 sparkY 路径把已渲染 Z 与源 Y/本地 itr Y 合成后交给 `AddHitRecord`，属于额外的混合坐标消费者；`BattleEntityOverlayRenderer` 的 bleed 插值也共用 `ToWorldBody`。生产改动前必须拆分 spark 的全局高度与本地美术偏移，避免整值乘倍率而二次缩放 Z 或改变 RNG/命中点选择。

## 待落地的共同出口合同

候选方案是**保留 `Runtime.Y/YInt/Vy` 为正式源规则真相**，给现有 `BattleSpatialProjection` 增加 `SourceDeltaToViewY`/垂直倍率（当前参考视野取 `1152/730`，默认视野恒等），在当前战斗呈现的公共 pivot/健康 anchor/插值出口只投影**实体全局 Y 位移**，保留 sprite 局部美术尺寸 `1.5`。OPoint/武器/人物的 Y 积分、出生、floor、平台及原始 hitbox 分支继续用源值。显示与影子位移的比例由单一投影函数决定，避免分散倍率。逻辑碰撞若要输出物理视图 rect，必须先按正式源域完成取整/特殊 full-height，再对矩形端点作一次投影；是否确需这种视图 rect、及它是否改变边界命中，要在现有碰撞生产消费者中逐项核实，不能仅凭本文件实施。

普通矩形路径已收窄：`BruteForceSceneQuery.WorldRect` 的 Y 端点在正常 bdy/itr 与 `BattleVolume` 中使用；同文件的实际交叠判断只比较两个 WorldRect 的 `Y1<Y2` 次序。对整数端点，倍率 `1152/730>1` 的同一严格单调投影在正常范围内保留端点次序及矩形相交关系；仍需按当前 `ClampRect`/溢出/full-height 约定做边界测试，而不能把此数学关系冒充已运行的命中回归。`participant.CollisionY` 与 `NativePreviousY104` 用来判断源状态更新/平台接触，不进入该矩形交叠比较，应保持源值。

实施前尚需核实：所有实际使用 `Runtime.YInt`/`GetRuntimeYInt` 的**呈现**消费者（技能火花、漂字、脚标、挂点、state9997 owner）是否共享 pivot 或要另走统一全局 Y 投影；`BruteForceSceneQuery` 的所有 Y 相关候选/平台分支、ECS hit snapshot 是否消费物理 rect 或源 rect；`OPoint`、持有关系与出生的 source Y 在中央呈现中是否重复投影；默认1倍和当前固定视野两套整数端点是否保持正式 first-diff 同态。上述未证项是生产修改的 hard gate，而非独立扩张到新玩法的任务。

最窄验证应复用本次 OID85 正例，在同一原 Battle Scene 的 R30 与已有 R120 入口验证离散/插值高度占比；再以已有着地和一次 Y 向 bdy/itr 邻例守住正式规则字段及碰撞，最后核退出 Scene/hash。只需这些有区分力的案例，不跑全部角色矩阵。正式 GUI 逐像素同输入捕获若缺失，仍按源码公式+正式 trace+Unity 原 Scene 生产呈现命令的证据等级报告。

## 2026-10-05 续查：分域与实施顺序

用户确认保留现有 `BattleVisualScale=1.5` 的本体图片显示尺寸。此例只要求**同一个正式源 Y 距离**占正式 730 高视口与 Unity 1152 高视口的相同比例；当前固定视野对应的位移倍率是 `1152 / 730`。因此共用投影的输入必须是正式源规则的**高度距离**，不能把已投影的 Z、整张 sprite 的本地像素或整个屏幕 Y 再乘一次。默认/较小参考视野仍为恒等映射。

| 路径 | 源规则域保持 | 项目视图域出口 | 核查依据与界限 |
| --- | --- | --- | --- |
| `NTSDEntityRuntime.Y/YInt/Vy`、`CharacterMechanics` 与正式 `physics_integrator.cpp` | Y 积分、重力、落地、平台、整数同步均不变 | 共用 `BattleSpatialProjection.SourceDeltaToViewY` 只接收已取整的全局高度距离 | 改 Vy 会改变正式 tick 结果，禁止。 |
| 正式 `object_spawning.cpp` 与 Unity OPoint/武器/人物出生 | `parentY + pointY - centerY`、dvy、持有关系仍用源 Y | 出生后的本体沿相同视图投影，不对 task Y 二次换算 | 出生规则和图片大小分别验。 |
| 中央/Legacy 本体、血条与影子 | `YInt` 仍为源整数，Z 保留已存在的 D-024 深度投影 | 本体 pivot 和血条锚点是 `viewZ + viewY + localArtOffset*1.5`；影子只用 viewZ | OID85 同动作同Z本体减影子 RED 原件已证。 |
| `BattlePresentationMotionSampler/DisplayMotion` | 连续性及 native `lround` 仍先在源 Y 算 | 离散源 Y delta 只投一次 `SourceDeltaToViewY`，本体/血迹/身体脚标消费 viewY+viewZ；地面阴影只消费 viewZ | R30 alpha=1 样本已证；R120 中间 alpha 尚待。 |
| `state9997` owner、持有武器挂点 | owner/holder Y 关系与 DAT wpoint 仍走源规则；`BattleVisualScale` 继续定义被批准的本地图片偏移 | owner 的**全局** Y 高度在 owner-relative pivot 出口投影；本地美术挂点只按现有 1.5 倍 | 不能把 wpoint 本地美术偏移再当作整段全局运动。 |
| `BruteForceSceneQuery` 正常 bdy/itr、`BattleVolume` | 先按正式源 Y 取整、算矩形端点和 full-height sentinel；平台/上一帧 Y 分支仍用源值 | 两侧普通矩形各端点经同一单调垂直投影一次；full-height 不投；默认倍率返回原整数 | 当前正常 `WorldRect` Y 交叠只比较端点顺序，受控极值/邻接还未运行。 |
| `BattleEntityOverlayLayout` 的 revive counter | 源 `Z + Y - centery - 7` 公式已由正式 `render_snapshot.cpp` 证实 | 只变全局 Y，Z 已投影；名字/地面标签不含全局 Y | WORDS 资源用户已删除，名字替代另案；不因本项恢复图片。 |

额外**不得并入第一批的混合坐标**：正式 `battle_world.cpp::emit_kind0_spark` 在源域先算 `targetZ + hitY + jitter`，`render_snapshot.cpp` 直接发布 `event.world_y`。Unity 活跃 ECS `ProjectKind0HitRecord` 则把当前已投影的 `target.Runtime.ZInt` 与源 `hitY+jitter` 混合，随后 `BattlePresentationShadowBuild` 将组合值直接当屏幕 Y 使用；旧 `LF2CharacterDatHitResolver.SpawnSpark` 也有同类混合写法。仅有最后的 `AnchorZ` 无法安全反推事件发生时的源 Z，宿主可能不同且记录可存活多 tick。修复应在事件**生成当刻**同时保留正式源规则记录和只用于呈现的投影位置，或给渲染快照增加足够的事件源锚点；先核当前 ECS 写者/历史解析器的实际可达性与现有 trace 字段。禁止直接对 `AnchorZ` 总值乘倍率、改 RNG 调用次数或修改 DAT。火花问题作为同一 D-024 纵向合同的后续独立出口，不由本体单例宣称通过。

最小实施顺序：① 先登记准确代码路径的独立生产 Change Record，修改共用 Y 投影、本体/血条/owner/插值及正常 bdy/itr 的双域出口；② 用一个 OID85 高度正例、一个着地邻例、一个 Y 矩形邻接（含 sentinel）验证规则字段不变与画面比例；③ 仅在这些通过后处理实际可达的混合火花写者，做单次命中事件/画面检验。若某分支没有正式可达阳性，则保留条件门，不扩成全角色矩阵。任何一步都不能把本体 `1.5` 倍图片尺寸、相机、地图、DAT 或非战斗系统改掉。

## 2026-10-05 候选与火花续查

真实候选路径的 Y29 相交、Y30 接触、Y31 分离在原 Editor 的 identity 与固定视口各一参数实例 2/2 PASS；直接查询及 ForceRoleAware 生产收集同为 1/0/0，[原件](original-editor-y-boundary-20261005.json)。这覆盖普通候选的纵向边界，不是命中后的伤害响应或实际落地验收。

上文 `emit_kind0_spark` 是旧称；当前 336B44 playable 构建闭包中的准确函数为 `battle_world.cpp::append_confirmed_native_spark`（`build.ps1` 将 `battle_world.cpp` 列入 core source），它在源域先把 `target.position.z` 加到裁定后的 `base_y`，再按 CRT Y、X 两次产生源事件。Unity 活跃 `BattleNativeHitSparkWriter.Append` 计算的是已投影 `victim.ZInt` 加源 `y+jitter`；`BattleEcsHitExecutionPlan.ProjectKind0HitRecord` 的预测也以同一混合式填 `HitRecordZ`，中央 `BattlePresentationShadowBuild` 直接把记录的 `AnchorZ` 当视图 Y。现有[自然 Play 见证](../NTSD28-336B44-Q09-C040-GAMEVIEW-WITNESS-001/q01-spark-natural-20261004-01.json)在第25相对 tick 发布两个 HitRecord 和一个实际火花命令，说明当前内容的火花消费可达；它没有同初态正式/Unity 同一火花 `world_y` 数值配对，因此目前仍是**静态公式首差候选**，不可宣称画面错位数值已测定。下一只对一例真实命中保存事件生成时的正式源 `world_y`、Unity 源规则值和视图命令；阳性后再为实际活跃 writer 建独立脚本 Record，保留源记录与视图锚点两域，避免对混合后的 `AnchorZ` 整体乘倍率。
