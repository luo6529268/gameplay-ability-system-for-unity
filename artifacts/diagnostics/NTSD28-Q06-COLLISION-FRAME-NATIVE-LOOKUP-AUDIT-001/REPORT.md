# 碰撞快照读取审计

状态：SOURCE_AND_CALLER_MAP_RECORDED；生产修复未实施。用户再次确认 Scene 的 HUDBg x30 是其本人或其他任务修改，保留。

## 已观察事实

当前工作树 Assets 的 C# 词法扫描：36 处 GetCollisionFrameData 出现，包含 1 声明和 35 调用，7 文件，无 override；精确行、方法起始签名和文件 SHA 在 caller-inventory.json。没有发现 NTSD 战斗 runtime 之外的直接生产调用。词法搜索不证明外部反射调用不存在，也不把测试中的手工 Frame.Prev2D 指针构造当作正式语义。

| 消费领域 | 文件与调用数量（不含声明） | 必须保留的区别 |
|---|---|---|
| 普通/空间索引/role-aware/Shadow 候选、立即查询、持有者 itr | BruteForceSceneQuery 21 | current 与 snapshot 分别参与资格、几何和缓存；不能只换共享 getter |
| 实际候选消费 | BattleHitCandidateSequenceRunner 1 | snapshot 中的 itr index，冻结候选关系另有职责 |
| getter 自身的碰撞 Z 便捷入口 | LF2Entity 1 | 显式 frame 的 Z overload 不自动改写 |
| 命中投影及预处理 | BattleEcsHitExecutionPlan 5 | actual/Shadow 两边均错误时，内部零差异不证明原版一致 |
| kind1 抓取 | BattleCpointWriter 3 | kind1 双端 snapshot；kind2 孤立校验 current 是不同规则 |
| 配对过滤快照 | BattleHitCandidatePairSnapshotFactory 2 | current、previous、snapshot 是三份读帧角色 |
| 抓取 no-op/跳过证明 | BattleInteractionPipeline 2 | getter 改后必须验证跳过资格与非跳过路径一致 |

原版 README_SOURCE.md 声明当前源码对应发行 EXE；playable/scripts/build.ps1 第55/74/76行包含 dat_parser.cpp、battle_world.cpp 和 simulation_tick_driver.cpp。后继 source witness build 会重新核验 EXE/75文件身份，不用旧 hash 冒充本次测量。

## 原源码与 Unity 对应

- simulation_tick_driver.cpp:693–694 先 snapshot_actions 再几何候选；894 推进 catch。battle_world.cpp:1592 只写 tick_action_snapshot=action，不保存 DAT 指针。
- battle_world.cpp:4030 同时点查 current 与 snapshot；4109/4269/4283 使用当前实体 definition 的 snapshot 动作号。不可用时返回并记诊断，未 fallback current。implicit 空 frame 存在不意味着含 itr/bdy。
- battle_world.cpp:817/819 火花锚点也从当前 definition 查询 snapshot；不是从 Unity 表现缓存取几何。
- battle_world.cpp:5762/5823 kind1 双端读 snapshot；5778 起孤立 kind2 分支分别读两个 current。不能把抓取全部统一成 snapshot 或 current。
- LF2Entity:4682 仍 HasFrame(Prev2)+Prev2D，失败后 HasFrame(N)+D；有旧范围/声明准入及当前帧回退。该读法也可能沿用旧 definition 的缓存指针。
- BruteForceSceneQuery:6667/6675 的 GetAuthoredCurrentFrame/GetAuthoredPrev2Frame 都仍使用旧 HasFrame。CollectCandidatesForPair、RoleAwareFormalExactCacheIsCurrent 等有实际调用；仅替换 LF2Entity getter 不能完成候选生产闭环。
- BattleHitCandidatePairSnapshotFactory:Capture current HasFrame 和 previous GetFrameDataById 也仍是旧语义。需要源过滤调用证明后和碰撞读取一起迁移，不能以失效 default 快照掩盖差异。
- ItrAllowed 及 RuntimeConsumeItrAllowed 还有 caller-local `?? attackerFrame`/`?? attackerCollisionFrame`。它们是否会在正式缺帧路径绕过拒绝，必须在实际 collector/consumer 联合向量中验证，不能只删除所有 `??`。
- SimulationWorld.CaptureCollisionActionSnapshotsOnlyAll 实际调用 CaptureCollisionFrameSnapshot，含既有 suppression/role roster 适配。此次不调整其顺序或压制政策。

## 定义身份与未证事项

当前定义替换后，新的 collision 查询必须读取新的 definition；一个函数在替换前已捕获的局部 throwFrameSnapshot 是否继续使用，是另一个时点问题。不能据此保留全局 Prev2D 旧指针，也不能用“当前定义”概念抹掉函数局部输入快照。

此前 Task 将 Unity ApplyThrow 的 sourceNextFrame 缓存说成应保留的合同，现改为待原路径证明：当前 advance_catch_relations 中未读到 Unity ThrowInjury==-1 的变身分支；需要查询正式字段可达性、后续原变身路径及既有例外，再决定该旧逻辑去留。此次不修改 throw/raw setter，不以历史 C# 行为裁决。

IsPureTransitionSmoke 仍有旧 MaxFrameIdExclusive 门，是 reader 后续专项的候选；不借本 getter 审计修改 oid999 全部生命周期。没有做新 Play、SelfCheck 或 Unity 编译，源码阅读不升级为已对齐。

## 后继顺序与出口

1. COLLISION-FRAME-SOURCE-WITNESS-001：精确 source runner，以相同初值分离 snapshot/current，含 implicit/high/missing 和诊断定义替换；记录候选与 kind1 抓取推进。
2. 准确 Unity Task/Record：优先 LF2Entity collision lookup、BruteForce current/Prev2 helpers、配对 factory，覆盖 actual/Shadow、ordinary/role-aware、kind1/孤立kind2及 no-op。验证没有 current fallback 或陈旧 definition pointer；必要相关生产文件另列才编辑。
3. 抓取 action/throw/raw/held 等 reader 回访；随后 display/post，再 Q07 正式 DAT/角色图。Q07 尚未部署，不因诊断夹具通过跳过资源验证。

总目标仍 ACTIVE。未改变非战斗逻辑、Unity/GAS框架、Scene、资源、Server、schema或关闭合同；禁止 computer-use。
