<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-PENDING-PRE-C25-MOTION-GUARD-001
status: VERIFIED
change-kind: NATIVE_PENDING_MOTION_PHYSICS_ADMISSION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06PendingMotionEditorTests.cs
authority: battle_world.cpp apply_frame_motion:1684 and step_physics:1804 reject lifecycle pending before frame lookup/hold countdown; simulation_tick_driver calls dead normalization independently afterward.
evidence: State18 source pending vectors and 276 downstream raw position differences in both full-tick modes; exact/native fallback callers traced.
-->

# C25前 pending 运动资格

准确三脚本，先RED。ApplyNativeFrameMotionForWorldPass在读帧/改速度前拒绝canonical pending；ExecuteNativePhysicsForWorldPass拒绝并清本tick physics completed标记；World的C06入口pending不得进入exact Ecs shortcut，走上述拒绝路径。仍执行source独立的NormalizeNativeDeadCharacterResources（type0 HP<=0清bound/PP），保留C25/state18/encoded lifecycle后续处理；不通过全局IsActive过滤整个entity。现有RunNativePhysics virtual调用层/各实体具体动力学实现、Mono/GAS框架/legacy独立SimTU兼容入口不重构。

覆盖所有7种DAT类型、exact/fallback两frame-advance模式、正/负/零hold timer、HP0/500、非零frame dvx/dvy/dvz；pending不得改变位置/速度/timer，normalization依源仍执行。非pending对照验证hold正常推进；pending source的既有full-driver子粒子raw与NativeRandom逐字段回归。新增测试不扩展旧RNG为全tick零：用户C17随机掉武器例外另记任务。

验收实际RED→compile/focused、原失败pending场景/两后端、完整SelfCheck、真实Play源位置/后续C25恢复及关闭。marker不是持久字段，清现有值不加schema；15/23/26/2/2与raw47/3保持。无新模块/队列，仍既有停止接单与关闭十一阶段。回滚仅本差量且先批准，保留用户场景HUDBg x30及其他任务文档修改，不编辑非战斗/资源/Gen/Plugins/Server。

实际RED：7899bfe09c0b48feabdc35b85935e56c，6中2PASS/4FAIL；两模式各42 pending场景被运动/hold改变，两个原full-driver pending分组再现276 raw位置首差。正对照仍推进。原XML/JSON归red，开始两生产入口修复。

实际生产已写：LF2Entity两入口pending gate及World exact shortcut gate，CS0；job 1c195c0277b6430c87377439917b4d3e 实际6/6 PASS（green-6.xml），84 pending admission和12完整driver向量，276旧raw首差归零。下一本文件测试脚本补现有Editor请求式Play probe：真实场景暂停后运行上述两模式pending完整driver，核对Scene checksum与pool借用恢复，随后既有Q05关闭；不修改Scene或输入资产。最终SelfCheck/Play仍待。

Play-12-pass.json：实际Scene启动暂停后两frame模式12逻辑夹具完整driver/raw子粒子一致，Scene checksum与Renderer2→2保持；Shutdown-pass.json World/slots/两pool0、两帧Stopped。SelfCheck请求02:58:40.7052445Z，结果02:59:21Z PASS，已归档；CS0、Scene SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。Ledger PASSED（546 records/12 governed diff files；历史未差量路径仅WARNING）。

评审补充：PendingFullDriverParticlesMatchOriginal尚仅CompareChildren与RNG；需在当前24大回归终态后，同一三路径范围补原sourceAfter的position/motion、runtimeStateCode和resolutionPending/code显式断言，证明C25后恢复与不移动；不机械比较未初始化fixture的无关birth字段，相关差异另行记录。之后重跑本6 focused及本Play12（新增断言确实运行），不用重复原1550或全SelfCheck，除非发现生产改变。保持IN_PROGRESS，未最终关闭。

## 最终限定出口（2026-09-14 03:23Z）

VERIFIED / PENDING_MOTION_AND_PHYSICS_ADMISSION_ONLY。实际证据按改动职责使用，不推广整个B或Q06完成：

- source state18 1550行、formal96，以及weaponHp210重复一致；原正式EXE SHA B1E13AE…9033与75源码manifest07CD47…778F保持。
- 原82测试58PASS/24FAIL；旧non-RNG276实际含252 pending位置和24 weaponHp（此前全归pending的说法已纠正）。pending原6中4FAIL→6PASS，补源实体位置/速度/current/latch/previous/collision/state/pending/code后仍通过；weaponHp原10FAIL→全绿。
- job5a104f86aa1147a3921e27253a5fcc8f：24组22PASS/2FAIL，1514向量只剩weaponHp；修复后jobce0f7a50c7524df990c290e4d9a12831实际30/30 PASS，含weapon10（两caller420原向量、7type真实池复用、手工fallback）、pending6、原SpawnVitals12、两失败chunk5。24组最新合计1514向量0差异/legacy546次恰为批准例外；22组来自前一终态、2组来自修复后，不能称同一run24全绿。direct1507原通过且不受普通OPoint初始化差量影响；full-matrix-final-validation.json记录来源。
- 最终真实Play：pending12含新增源状态；weaponHp84（7type×kind1/2×pointHp负/零/正×两actual factory）；正式Logan999组合96（两factory、原raw47/RNG）。全部PASS，Scene checksum保持，每组Renderer借用2→2。各目录Play-final-pass.json。synthetic父/777及旧内容Scene边界已声明，非图片/物理按键全验收。
- Shutdown-final-pass.json：4→4恢复、World/slots/logic/render borrower全0，两帧Stopped并正常退出。最后SelfCheck请求03:21:56.9990036Z、结果03:22:37Z PASS，归weaponHp目录SelfCheck-final-pass.result。CS error0；Editor idle非Play/非compiling，Scene dirtyfalse/root14/SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。
- Ledger与diff-check已通过（历史未出现在diff的声明路径WARNING不作失败），交付前再运行。无正式资源/Scene/InputActions/非战斗/Unity-GAS/Server/Gen/Plugins修改，无computer-use、commit/push或用户工作清理。

后继边界：普通OPoint的effect/reserve/join/join_reserve/join_pic及type0/5-parent credit门、defend等明确仍由OPoint remaining consumer Task承担；generic出生1/0/0不是最终所有OPoint复活字段。当前frame父事务仍需高动作/成本fallback连续完整driver与真实Play联合，不能用端点2676或本组合覆盖代替。其后回原reader/display-post/Q07。15/23/26/2/2、raw47/3及全部已批准例外保持。
