# NTSD28-336B44-Q09-WORDS5-GROUP-MAP-001

状态：`FOCUSED_TEST_PASS`（共用组5映射的代码与聚焦门；真实剧情画面和正式根 GPU 仍在 Q09/Q12）。父目标：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-01 Q01 → BATCH-05 Q09`。

当前权威：根目录正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应的 playable live path。`render_snapshot.cpp::native_glyph_resource_slot` 对战斗组 1～5 选同编号 `WORDS`，复活次数单独选 0；`game_session.cpp` 的剧情出生将 type0 的非 -1、非 1～4 `team` 映为 5。正式剧情 DAT 存在 `team: 5`；默认 `stage.dat` 部署仍按用户要求暂缓，不借此加入资产。六张正式/Unity 暂存 `WORDS0～5.png` 当前逐 SHA 同版。

现状与首差：Unity `BattleEntityOverlayLayout.ResolveRelationSheet` 只接受 1～4，普通第 5 组姓名牌落到 sheet0；特殊 Com 独立选择 sheet5。`BattleRuntimeSelfCheck.CheckBattleEntityOverlayLayoutContracts` 仍把普通 group5→sheet0 记为旧预期。生产发布的 `BattlePresentationShadowBuild` 将 `entity.RelationTeam` 传给该布局，故这是战斗画面共用选择器的有条件静态首差，不是 DAT 或图片缺失。根 EXE 相同场景的 GPU Present 和用户项目默认剧情资产尚无直接证书。

声明代码路径与符号：`Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs::ResolveRelationSheet`；`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs::CheckBattleEntityOverlayLayoutContracts`；`Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs` 新增聚焦用例。仅把普通关系组 5 映到 sheet5，保持 0/负/大于5 的 sheet0 fallback、复活次数 sheet0、special Com、模式/可见性和其他战斗规则不变。不改 Scene、HUD、菜单、正式 DAT/PNG、背景/模式 DAT、stage 资产或 Unity/GAS 框架。

执行与验收：先写聚焦 RED：同一 group5 P1 的复活次数 `x2` 两字仍从 sheet0，姓名牌字母从 sheet5；普通 group0 与 1～4 和特殊 Com 旧例继续通过。再修共用选择器与旧 SelfCheck 预期。优先运行独立纯 C# RED/GREEN、生成 Editor 编译、原 Editor 单项 EditMode；若原 Editor 场景或编译状态不安全，仅保留编译与未验项，不启动第二 Editor。必要的真实剧情场景/正式根 GPU 留 Q09/Q12，不把此小包当完整画面验收。运行 ChangeLedger validator 与 scoped diff check。

风险与回滚：第 5 组显示色可能改变，正是当前权威要求的效果。回滚只对本 Change 的三处增量作前向补丁，不用 Git restore/reset；所有用户/其他任务未提交 HUD 与 Scene 修改原样保留。若当前根正式版同条件可见行为反证该源码映射，停止晋升并记录 first difference。
