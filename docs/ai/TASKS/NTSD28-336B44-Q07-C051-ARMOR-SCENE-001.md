# Q07/C051 护甲正反例原 Battle Scene 限定 Play

状态：`VERIFIED / SCOPED_SCENE_PASS`。父 C051/Q07 仍开放。当前权威为根正式 336B44 EXE、对应 playable 源码和非排除正式 DAT。前置 [正式源/根可达报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-REACH-001/REPORT.md)与[原 Unity 完整 Driver 对照](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001/REPORT.md)分别证实 OID78/action466→OID447/action54/effect23 对 OID97 护甲、OID2 无甲控制可达，以及原 Unity 两例各 12 tick×11 字段与正式根零差。它们尚未证实原 Battle Scene Play 的运行、退出和场景稳定性。

唯一代码范围：现有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C051OroScenePlayProbeEditor.cs`。复用其启动、Play clone 预 Start roster、稳定暂停、生产 `SimulationTickDriver.StepOneTick`、SessionState、只写一次结果与四 SHA/Scene clean 检查，仅增加两组精确护甲/无甲配置及原 Editor 请求入口。原 OID20→888 左右菜单案例保持原样；生产 C#、DAT 数值、图片、Scene、Prefab、GameConfig、模式 Asset 和非战斗功能不改。不启动第二 Unity 项目，不用 computer-use。

同初态：seed 682973786、mode0、actor OID78/action466/source X500/面右、target OID97 或 OID2/action0/source X550/面左、Z400、HP/MP500、队伍1/2、12 tick 中性输入。逐 tick 至少导出父/目标动作、目标 HP/MP、OID447 子体槽/动作/X、目标速度和运行时护甲值，与正式根同 tick 的选定字段比较；tick3 预期护甲 HP495/action0，无甲 HP450/action180。任何首差先保留完整结果并追初态/比例域；不改 DAT 来凑结果。

当前工具列表未暴露 Unity MCP 且 Pipeline CLI 未接入；原 Editor 本地 Unity-MCP 桥可直连刷新；若采用脚本已有的 `Temp` 请求监听，必须在创建唯一临时请求前按 `docs/ai/file-removal-audit-contract.md` 记录自动清理路径、内容、SHA、原状态、恢复源与执行结果。只允许新建并消费本包的请求，不得覆盖旧结果或旧请求。Edit/Play 切换前核原 Editor 单 Scene clean、空闲、编译完成，退出后复核 Menu/Battle/GameConfig/Mode Asset 四 SHA、Scene clean、LoganRuntime Git diff。

验收为原 Editor 编译0 error、护甲/无甲各 12 个生产 Driver tick 与当前根逐 tick 选定字段对照、Play 正常退出且受保护内容不变。此出口仅关闭受控 Scene 包，不自动关闭 C051/Q07；自然物理键、逐 hit 内部状态、完整 World/表现另验。回滚仅该脚本中本包新增分支，须先按文件操作合同审计并获适用授权；所有诊断原件保留。
实测更正与出口：原 Editor 有本地 Unity-MCP 桥可直连，虽然当前会话未暴露 Unity MCP tool；经该桥读取状态、刷新导入脚本，并通过已审计的唯一临时请求顺序运行两例。原 Battle Scene 各12tick×11字段对当前336B44正式根分别132/132零差，tick3护甲HP495/action0、无甲HP450/action180；两次正常退出、Scene clean、四保护SHA稳、LoganRuntime Git无差异。仅此受控Scene出口VERIFIED；逐hit内部字段、自然物理键与完整World/表现待。[验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-SCENE-001/ACCEPTANCE.md)。
