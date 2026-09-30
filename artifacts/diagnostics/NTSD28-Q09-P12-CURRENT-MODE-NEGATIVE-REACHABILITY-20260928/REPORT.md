# Q09/P-12 当前项目 etc-mode 负例出口对账（2026-09-28）

状态：`READ_ONLY_CURRENT_CONFIG_CLASSIFIED / NEGATIVE_BATTLE_BRANCH_ALREADY_WITNESSED`。本报告只收窄现有配置下的 P-12 定向验收范围，不关闭 P-12、Q09、BATCH-05 或总目标。

当前保存的 `Assets/NTSD/Resources/ProjectBattleModeConfig.asset` SHA-256 为 `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，序列化 `selectedModeEtcMode: 1`。`ProjectBattleModeConfig.LoadDefault().Capture()` 在 `CharacterAnimtorManager` 准备所选 Logan catalog 时冻结此值，`BattleRuntimeDataCatalog.Prepare` 接收该 Snapshot；`BattlePresentationShadowBuild` 将其复制到每个发布帧，Legacy `LF2ObjectRenderer` 读同一目录。当前生产 `Assets/NTSD/Scripts` 搜索没有此字段的运行时写入或第二份所选 etc-mode 来源；`ProjectBattleModeConfig` 的公开接口只读，测试可创建配置副本不等于保存的默认入口会选择 0。原 Battle 香燐请求已经实际量到 Asset/目录的 1/1 和相同指纹（`NTSD28-Q08-P12-PROJECT-ETC-MODE-CARRIER-001`）。这是**当前默认配置和已查生产调用链**的结论，不是所有可编辑配置永远不能设为 0 的全局证明。

已有的原 Editor 聚焦测试 `BattlePresentationCommandWriterEditorTests.State9997BodyPlacement_UsesOwnerOnlyForSelectedModeAndValidOwner` 对 etc-mode 0/2、owner -1/9 和缺失 owner 做了 fallback 正反；其三项精确测试 3/3 的证据属于 `NTSD28-Q09-P12-STATE9997-BODY-PLACEMENT-001/FOCUSED-20260928.md`。此外，当前**选中 etc-mode1**下的无效 owner9 已分别由完整 Driver 原 Battle CentralOnly/LegacyOnly 香燐自然子体及 Legacy body 像素差分验到 fallback、保留物理朝向，见 `NTSD28-Q09-P12-KARIN-OWNER9-FALLBACK-001/ACCEPTANCE-20260928.md`。无 owner -1 的正式 OID998 原 Battle 中央及 Legacy 目标像素也有独立限定证据。故 Task 所列“mode0 **或** invalid-owner 的 Battle 负例”已由**invalid-owner**路径满足；不必为当前默认配置再克隆 Asset、启动 Play 重跑一个 mode0 负例。

结论只改变下一动作：若以后保存的项目模式 Asset 改为 0/2、或生产入口加入可选 etc-mode 写者，再重开对应配置的真实 Battle 负例。当前 P-12 继续等待正式根 EXE 与获批准固定全景/项目背景条件下可比的可见画面证据，以及其它新证明可达的表现首差；不把已通过的默认 owner、fallback、Legacy body 和目标像素重复作为验收前置。本轮只读现有 Asset、源码、既有测试及报告；没有运行 Editor/Play/正式 EXE，也没有修改脚本、DAT、PNG、Scene、Asset、相机或非战斗行为。
