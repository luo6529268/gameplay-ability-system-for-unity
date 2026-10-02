# Q07/D-024 kind8 纵深同步的比例域候选

状态：`STATIC_FIRST_DIFFERENCE_CANDIDATE / FORMAL_REACH_AND_UNITY_SCENE_PENDING`。本报告只读当前正式 playable 源码、正式及暂存 DAT、Unity 生产写者与现有测试；没有运行同条件正式 EXE 或 Unity Play，也没有修改脚本、DAT 数值、图片、Scene 或配置。

权威身份：本轮重算根目录正式 `NTSD2.8-Logan.exe` SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。当前 playable `BattleWorld28::resolve_special_relation_hit` 在 `source/ntsd28_core/src/simulation/battle_world.cpp:5800-5817` 规范化 `dvy` 为同步模式；只要不是 `-1`，它令攻击者精确纵深为目标精确纵深 `+1.0`，整数镜像留给后续物理。这里的 `1.0` 是正式规则坐标差，不是 Unity 固定完整背景视口中的显示像素。

Unity 生产命中路径由 `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs:354-365` 在 Kind8 disposition 调用 `BattleKind8ControlRelationWriter.TryApply`。写者 `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleKind8ControlRelationWriter.cs:51-67` 在同步模式非 `-1` 时，分别写 `attacker.Runtime.SourceRuleZ = target.Runtime.SourceRuleZ + 1.0` 与 `attacker.Runtime.Z = target.Runtime.Z + 1.0`。前者符合正式源规则；后者在 D-024 的非 1:1 视口下绕开了 `world.SpatialProjection.SourceDeltaToViewZ(1.0)`。`BattleSpatialProjection.cs:9-10,43-58` 定义正式视口 1333×730；项目 2048×1152 固定完整背景时纵深倍率为 `1152/730 = 1.5780821917808219`。若两实体原先共用投影锚点、正式源坐标已初始化且此 kind8 实际应用，期望物理相对纵深约 `+1.578082`，当前写者却强制为 `+1.0`，差约 `0.578082` 显示单位。整数镜像不应在 kind8 写入阶段提前刷新。

这不是只凭类名找到的死代码：共用候选消费者确实分派到该写者。正式 `c/lee/lee.dat` frame24 有 `itr kind:8` 且没有 `dvy:-1`；正式与当前 `LoganRuntime/decoded_dat/c/lee/lee.dat` 原始 SHA-256 均为 `775079F67116E0EA490DD57FC6FB226E1813C6D645807E4D49273639929C99BE`。该 DAT 行只提供当前内容候选，不证明 frame24 在所选初态自然命中、满足候选资格或根 EXE 到达写者。

现有 `NTSD28B5Kind8AtomicProductionIntegrationEditorTests.Actual_DvySelectsPreciseAxesAndNeverWritesIntegerMirrors` 建立 `new SimulationWorld()`，其投影默认 `Identity`，断言源 Z `+1` 与原物理 Z；因此既有 kind8 聚焦通过不能否定 D-024 固定视口下的比例候选。其他旧版 kind8 证书也不能自动升级为 336B44 的画面比例证据。

下一证据门：先以正式可达 kind8 DAT/动作、同 seed/初态/输入在当前 336B44 playable 完整 tick 与正式根 EXE 证明实际 `kind8 applied` 和源 Z `+1`；再在原 Battle Scene 的 2048×1152 投影下以相同源坐标记录攻击者/目标物理相对 Z、整数镜像和下一物理 tick。仅当定向 RED 成立时，在共用 kind8 写者使用世界 `SpatialProjection` 做相对纵深出口，并做聚焦回归、完整 Driver、退出清理及非例外可见画面验证。不得改 DAT 值或把源坐标乘倍率，也不应调整用户固定相机设置。若正式 frame24 不自然命中，应找其它正式可达 kind8 入口，不能拿静态 DAT 行冒充 Play 证书。
