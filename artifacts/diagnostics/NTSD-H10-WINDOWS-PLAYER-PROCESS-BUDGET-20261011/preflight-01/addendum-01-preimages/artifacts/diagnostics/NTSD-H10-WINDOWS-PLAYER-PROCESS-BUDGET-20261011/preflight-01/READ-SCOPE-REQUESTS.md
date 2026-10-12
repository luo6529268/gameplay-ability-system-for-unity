# PRE-FLIGHT-01：下一步准确只读请求（尚未读取方法体）

这些是完成方案所必需的具名依赖，提请主会话独立审查；不是生产改动请求，也不是泛读Q06授权。当前未实施/构建/运行。

| ID | 路径/符号/候选边界 | 必要性与将改变的决定 |
|---|---|---|
| R1 | Assets/NTSD/Scripts/App/MatchConfig.cs::MatchConfig 全部数据字段；App/PlayerSlotConfig.cs::PlayerSlotConfig 数据定义（只字段/构造默认，不扩其它runtime） | 明确空players副本如何完整保留background/mode/seed/content/profile及引用身份；字段不闭合则不能实现clone。 |
| R2 | Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs::Start（62-233，先读Menu/App-owned early-return、SuppressEntityCreationForProductionStress、ProductionStressServicesReady赋值分支），对应OnDestroy529附近仅静态ready复位；App/BattleBootstrap.cs::TryPrepareMapConfiguration43、ClearPreparedMapConfiguration106、DisablePresentation134及IsRuntimeMapCleared40的最小段 | 证明空players App路线不会另造测试角色，决定suppression应如何设置而不阻断readiness；明确真实map完成。不得由此扩读SetupTestCharacters、factory或StageWave。 |
| R3 | Assets/NTSD/Scripts/UI/CharacterSelectionController.cs::OnEnable121-130、ResetAll487-522、StartBattle455-486；LoadingPrewarmController.cs::PrewarmOnceAsync114-123/PrewarmOnceCoreAsync124-178的重入门；必要时仅MainMenuController的GameStart回调绑定段先rg定位再给范围 | MenuMain返回不会ResetAll，三轮需要确定已公开UI复位/seed/背景和prewarm复用方式；避免第二轮仍旧Confirmed导致无效流程。 |
| R4 | Assets/NTSD/Scripts/UI/LoadingPrewarmController.cs::PrewarmOnceCoreAsync124-178中的资源入口；CharacterUIResourceManager.cs::InitializeSingleton65附近的raw路径入口；GameDataManager.cs::LoadDataFile93及ResolveObjectFilePath218只在该正式Menu链实际调用时读其路径解析段 | 现有旧builder的全目录copy不能定义当前必需raw UI/Config清单。先确认实际消费路径，再补manifest中真实必需文件；非到达路径不继续。若入口转到其它Loader，先报告新的路径/符号，不自行递归展开。 |

已获主会话具名补证而不需新泛读：App.InitializeBattleAsync136-224/SetupBattleCharacters284-298/SetMatchConfig384-401/TryShutdown473-502；Driver.TryConfigureEmptyDiagnosticWorld2512-2540、CreateProductionWorld2622-2665、RecreateWorld2491-2499及Shutdownstage1/5。App范围已本轮精确复核，Driver补证按主会话读取来源标注。

还需主审锁定的方案参数，不是已获执行额度：两进程各3轮=1smoke+2formal（而非3formal）；采样间隔/最大gap失效规则；超时只优雅退出、未自授kill；harness Manual/MobileExtended/帧checksum关闭的限定证据用途。32/33ms及原最终门不修改。
