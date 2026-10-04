# NTSD28-336B44-Q09-WORDS5-GAMEVIEW-001

状态：`VERIFIED`（仅组5原 Unity Game View 子门）。父项：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 Q09`，仍开放；前置 `NTSD28-336B44-Q09-WORDS5-GROUP-MAP-001 / FOCUSED_TEST_PASS`。

权威与目标：正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；对应 playable `render_snapshot.cpp::native_glyph_resource_slot` 对战斗组5姓名牌选 `WORDS5`，复活数字选 `WORDS0`。同文件姓名牌门允许 battle_mode0、slot0、group5 玩家显示；Unity `AppManager` 的玩家 team 经 `BattleMatchConfigRuntimeAdapter.ResolveBattleTeam` 保持正数5。用户默认stage.dat部署仍暂缓，因此用现有 Menu→Battle 的模式0匹配在原Editor定向可见验收，不伪称正式剧情自然入口。

声明脚本路径：`Assets/NTSD/Scripts/Test/Editor/NTSD28Q09NameplateNaturalPlayProbeEditor.cs`。在现有姓名牌 Game View 探针中增加仅诊断的组5入口，复用现有内容预热、Additive Battle 加载、中央命令、截图与有序卸载。现有组1、Q07输入诊断和菜单配置默认参数保持原行为。为组5报告记录运行时 RelationTeam、当前 tick 的 OverlayGlyph 资源键、`WORDS5` 绑定及屏幕截图路径；禁止仅凭选图静态条件宣称实际像素显示。

实施前保护：先核原Editor idle、非Play、Battle Scene clean；记录 Battle/Menu/GameConfig/Mode Asset 磁盘SHA与脚本SHA。修改前建本 Task 与 Change Record，并在 Ledger/STATE/handoff 登记。仅编辑声明脚本及本包证据/状态文档，不动DAT、PNG、Scene、菜单/结果页生产代码、Unity/GAS框架。不能使用computer-use或第二Unity项目。

验收：生成Editor工程编译0错；原Editor刷新后新程序集晚于脚本；从保存的 Menu Scene 进入Battle，仅跑一次组5案例，要求 P1 关系组5、当前tick中央姓名牌资源sheet5、`WORDS5`绑定有效、真实Game View对应像素可见，随后有序退出、Editor idle/非Play/Scene clean且保护SHA不变。若失败，保存原始FAIL与首差，不把静态条件晋升为运行时；只对失败所指战斗路径另建修复包。正式根EXE实际GPU与默认剧情组5仍属Q09/Q12开放出口。

回滚：仅对本包诊断入口作前向补丁，不使用Git restore/reset；既有用户/其他任务脚本与场景修改原样保留。输出唯一runId文件，不覆盖已有证据。

实际出口：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q09-WORDS5-GAMEVIEW-001/REPORT.md)包含原始JSON、截图、紫色像素对照、原 Editor 停止/Scene clean与四SHA。模式0组5的同tick命令/绑定/真实画面通过；正式根GPU Present、默认剧情stage入口及父项Q09/Q12仍待。
