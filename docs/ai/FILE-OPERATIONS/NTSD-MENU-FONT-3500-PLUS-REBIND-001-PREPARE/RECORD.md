# NTSD-MENU-FONT-3500-PLUS-REBIND-001-PREPARE

- 状态：PARTIAL（计划中的脚本、测试、治理文档已写；Menu Scene 与 Plus SDF 未写）
- 类型：有备份的代码、测试、菜单场景、Plus 字库与治理文档覆盖。无删除、移动或 Git 丢弃。
- Task / Change：`NTSD-MENU-FONT-3500-PLUS-REBIND-001`。
- 原因及授权：用户在当前会话要求“我创建了新字体，你修改下：MenuFont3500MigrationEditor 都换成：Assets/NTSD/MoreMountains/MMTools/Demos/MMTween/Fonts/Lato/SDF/JifengBladeArtSC-3500-Plus SDF.asset”。此操作只触及该菜单迁移闭包。
- 执行者及根目录：Codex，本仓库 `I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`。
- 操作前逐文件绝对路径、大小、SHA-256、Git 状态、精确备份与备份 SHA：`artifacts/diagnostics/NTSD-MENU-FONT-3500-PLUS-REBIND-001/prechange-manifest.json`，共 9 个现有文件，副本在同目录 `before/`。备份写入后逐项 SHA 与源相等。Plus `.meta` 与 GUID 同在清单；只读不覆盖 `.meta`。
- 拟执行：`apply_patch` 修改 2 个 Editor C# 和治理文档；如 Editor 门槛满足，以 `NTSD/UI/Rebind Menu Font 3500 Plus` 菜单入口、`AssetDatabase.SaveAssetIfDirty` 和 `EditorSceneManager.SaveScene` 精确写 Plus 资产及 NTSD_Menu。运行前再制作独立 Scene/Font SHA 快照；磁盘变化或 Scene 内存脏则拒绝。
- 时间：2026-10-04 02:55 +08:00（准备阶段）。
- 预期影响：Menu 的 3500 → Plus 字体及 Scene-local 材质图集；测试资产定位改为 Plus。旧 3500 SDF 和其它场景不写。

实际执行：Codex 通过 `apply_patch` 修改 `Assets/NTSD/Scripts/Test/Editor/MenuFont3500MigrationEditor.cs`、`MenuCarouselVisualEditorTests.cs` 和 Change Ledger/STATE/handoff/本文件操作索引；新增 Task、Change Record、精确备份及 `prepared-manifest.json`。没有执行 Editor 迁移菜单命令。`postchange-manifest.json` 保存本阶段逐文件后 SHA；脚本后 SHA `F180695A...18B6A72`、测试后 SHA `0582C094...51DFFA0`。Scene `F01144C9...C45116329F`、Plus SDF `DF656A96...8A83A83`、Plus `.meta` 的 SHA 与操作前相同。完整 Editor 项目 `dotnet build` 0 error；Unity 原 Editor 在准备迁移时进入另一任务 Battle Play，故场景与字体保留未执行，未知后续编辑须重新检查准备清单 SHA。其它并发治理文档更新未归因于本次操作，也未被清理。

验证输出：`artifacts/diagnostics/NTSD-MENU-FONT-3500-PLUS-REBIND-001/compile-final.txt` 为完整生成 Editor 工程 0 error；同目录 `change-ledger-validation.txt` 为 validator PASS。Scene/Plus SDF 实际触及清单为空；后续迁移需另记该操作阶段并重新核对完整文件 SHA。
