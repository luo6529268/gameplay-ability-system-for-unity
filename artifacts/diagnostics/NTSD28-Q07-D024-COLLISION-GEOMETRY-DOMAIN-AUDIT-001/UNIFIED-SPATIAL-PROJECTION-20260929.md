# D-024 统一比例空间：用户决定与下一实现出口

状态：`USER_DIRECTION_CONFIRMED / READ_ONLY_CONTRACT_AUDIT / PRODUCTION_PENDING`。本项只读代码、配置与原有正式/Unity 近远结果；没有改生产脚本、DAT、图片、Scene、相机或项目地图。

用户于 2026-09-29 明确：既然 Unity 的实际奔跑距离是按原版画面比例放大，原 DAT 尺寸参与的判定与实体间实际距离也应按比例处理。目标是**同一套战斗空间映射**，而不是“规则坐标判定”与“画面坐标判定”二选一，亦不是修改 DAT token。正式来源坐标、局部 DAT 数值和原有相机分别保持。

当前 X 参考为 `2048 / 1333 = 1.536384096...`（`GameConfig.asset` 的 `BattleFixedViewRunReferenceWidthPx=2048`、`SimulationWorld.FormalRunViewWidthPx=1333`）。同一锚点 `A` 下，中心与局部 X 几何满足 `Xunity=A+(Xsource-A)*rX`、`offsetUnity=offsetSource*rX`、`widthUnity=widthSource*rX`。用 `A=500` **仅演示这组配对测试**：正式韩初始 X500 不变，正式李近 X520→Unity X530.7277，远 X580→Unity X622.9107；韩候选前源 X535→Unity X553.7734。正式近 ITR/BDY 重叠9对应 Unity约13.8275，正式远间隙1对应约1.5364。整数边界必须再按生产合同验证。Unity 旧测试原位李 X520/580 不再是同比例起始间距；它们仍作为“只放大运动/只放大局部框均不成立”的 RED 证据保留。

当前代码首差入口：`AppManager.InitializeBattleParticipants` 从 `Stage.BaseStageWidthPx` 选随机 spawnX、直接写物理 `PS.x`，`SyncParticipantBirthPosition` 随后把相同原值写入 `SourceRuleX`；`SimulationStageWaveModule` 的两类 stage spawn 也同时把同一原值写物理与 source。后续运动走 `FixedViewRunDistanceScale`，但 `BruteForceSceneQuery.LocalRectWorldRect` 和 `ItrWorldRectExeRaw` 使用物理 `Runtime.XInt/YInt` 加未换算的 DAT 局部几何。RoleAware 缓存、brute/fallback、粗筛、即时查询与最近候选的其余消费者见 `CONSUMER-MAP-20260928.md`。这证明目前不存在完整统一映射；尚不能单独证明所有自然战斗首差均由此引起。

地图约束：`GameConfig.asset` 的 `BattleStageWidthPx=800` 是无地图边界时的回退配置。`SimulationStageRenderModule.ResolveUnityStageRuntime` 可从 `BoundaryWallManager.TryGetBattleStageRuntime` 取项目多边形边界覆盖该值；`BoundaryWallManager` 由可行走 world bounds 的宽度换算 stage width。当前 `SunagakureMap.asset` 的多边形与 Battle Scene 的 `BoundaryWallEditor` 也为项目自有内容，不能因 D-024 导入原版背景/模式 DAT 或整体缩放、改写项目地图。`BattleWalkableAreaSnapshot.ContainsGroundPixel` 已以 Unity 实际 ground pixel 判定项目多边形；D-025 的非角色出区 10 秒计时继续以真实物理位置判断。

下一有界实现须先冻结：

1. 从活动地图可行走边界与现有固定相机，取**全战斗共用**的物理锚点和来源锚点；显式定义来源→Unity及逆变换。不能按每个实体自己的出生点作锚点，否则双方距离仍未缩放。若随机出生先在项目地图物理空间挑选，应逆映射 source carrier；正式来源测试的起始站位则前向映射物理位置。保持同一 seed/输入时标明两端初始坐标各属哪个域。
2. 分别定义 X、Y、Z 的投影和整数取整。当前配置高度 `1152/730=1.578082...`，不能拿 X 倍率盲用在 Y 或纵深 Z。特殊 `itr.y=int.MinValue` 的整高语义和溢出/镜像路径应维持。
3. 覆盖角色、非角色、stage/OPoint/复活与关系产生的物理中心，以及攻击/受击局部框、粗筛、精确碰撞、缓存失效、即时查询、最近选择和对象显示；项目可行走多边形保留实际世界边界。若完整 source 区域按倍率不能放进现有边界，记录真实边界后果，不悄悄裁剪倍率或修改地图。
4. 先测未映射 RED，再用**映射后**的正式近 X520/远 X580 两例、比例1、非角色、左右镜像、Y/Z边界和 RoleAware/brute/fallback 做聚焦 GREEN；再做原 Battle Scene 真实 Play 的抓取/伤害与有序退出。正式根 EXE 的原始 X520/580 证据可复用；不要每次重跑已闭内容/菜单案例。

Q07/BATCH-04 与 D-024 仍开放；本报告不是生产实现或 Play 验收。Q09/P-08 的独立根 LFR 入口与此不互相填充出口。
