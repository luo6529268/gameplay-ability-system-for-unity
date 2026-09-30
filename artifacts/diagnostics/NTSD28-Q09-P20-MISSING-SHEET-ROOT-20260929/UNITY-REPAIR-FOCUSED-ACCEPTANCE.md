# Q09/P-20 缺角色本体 sheet 的 Unity 聚焦修复验收

状态：`RUNTIME_PENDING / FOCUSED_TEST_PASS`。正式根缺 `c/hid/hid6.png` 的自然回放与完整根逐行对照见同目录 [正式负例验收](ACCEPTANCE.md)。Q07/BATCH-04 仍是最早未闭阶段，D-024 碰撞坐标域待用户取舍；P-20/Q09/BATCH-05 和总目标均未关闭。

正式根 EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。正式缺图根完成自然 Hidan 30 tick LFR，32/32 行 World `stateHash` 与完整根相同，tick 16–31 对应缺失 sheet 的 sprite 命令从 2 减为 1。Unity 原 Editor 修复前目标测试先得到预期 RED：缺 body 时 `Capture` 抛 `FileNotFoundException`；首个测试作业初始化失败且执行 0 例，不计 RED。原始 [RED 作业](unity-missing-body-red-job.json) SHA-256 `8091FD5FE5FCA53DCB0C57BBDA96F05EB253EAA83962064DD7528331B540D7A8`。

生产改动只在 Logan 内容候选为缺失的角色本体 sheet 记录显式身份并纳入双向新鲜度检查，预热跳过该 sheet，现有空 sprite 槽负责省略对应画面。现有完整图片仍按原 SHA-256 标识；head/small 必需图与其他资源错误仍走原拒绝路径。`CharacterAnimtorManager.cs` 中预存的其它 dirty 片段未改。没有修改 DAT、PNG、Scene、Prefab、模式 Asset、正式 EXE/source 或非战斗脚本。

原项目唯一 Unity Editor 的定向 GREEN 共 **7/7**：缺 body 候选及补回失效 1/1；必需 head/small 缺图拒绝 2/2；正式完整根 330 定义/906 引用图候选 1/1；相邻 PNG 身份与 DAT 新鲜度 2/2；真实 Manager/Data/UI 发布链的缺 body、保留 head、补回 body 1/1。最后一项 [原始作业](unity-missing-body-publication-green-job.json) SHA-256 `E96C0114E6796C5A803A66842BF0E9838F37EF1564AE9EFEE1D61AD481824CBD`；相邻两例 [原始作业](unity-adjacent-green-job.json) SHA-256 `58F1D9CE3E3E7CDDBA1DA474F1FC529DCCADF93714769698EF4B975126DD767B`。相邻测试首次失败是外部生成 PNG 夹具缺失，原始 [失败作业](unity-adjacent-fixture-failure-job.json) SHA-256 `5DC45DF66B9443A020BA2A591CC68916E1C2ECE049E265CA049E1E6AF06D0C2E`；测试改用自有字节夹具后该两例通过，不计生产回归。

生成 `Assembly-CSharp-Editor.csproj` 构建 0 error、213 warning；`Tools/Validate-ChangeLedger.ps1` 和 `git diff --check` 通过。原 Editor 最后处于非 Play、`NTSD_Battle` Scene clean；Battle/Menu Scene、`ProjectBattleModeConfig.asset`、`EditorBuildSettings.asset` 的保护 SHA 未变化。

仍需在原 Battle Scene 使用隔离缺图根执行选中缺失帧的 Play 验证，补齐 present→missing 新鲜度及公共资源负例，才能考虑关闭本 Change；正式根 GUI/同世界同视口画面及 P-20 其它分支仍属 Q09 后续出口。本报告不声称 Q07 或 Q09 完成，也不把聚焦 EditMode 等同真实 Battle Play。

2026-09-29 补充验收，覆盖上段“补齐两反例”的待办：原 Editor 的 `PresentBodySheet_BecomesStaleWhenMissing` 已在精确双名作业中 PASS；必需 SPARK 图缺失拒绝在创建测试自有父目录后，精确单名作业 [1/1 PASS](unity-missing-spark-required-negative-green-job.json)，SHA-256 `63DFD856B1566428A6F627C9B438C41FF4B56926D4413EE616496E1EF29DC500`。双名作业中 SPARK 首次失败是必需 WORDS 行指向不存在夹具；第二次失败是 SPARK 父目录不存在，抛出 `DirectoryNotFoundException` 而非断言的 `FileNotFoundException`。两次测试夹具失败分别留于 [原始作业](unity-missing-spark-fixture-failed-job.json)（SHA `15CCB4F4D67C6113A5B729565F40884D33DE5D07E3049FB2B69DFB7E32720B65`）与 [原始作业](unity-missing-spark-directory-fixture-failed-job.json)（SHA `71F516B598BD8B2440911FB7A97859316B93DE9E233D5211C3201B3667FC40C6`）。夹具修复没有修改生产脚本。最终生成 Editor 构建 0 错/213 警告，原 Editor 非 Play、Battle Scene clean，Ledger validator 与 `git diff --check` 通过；Battle/Menu/项目 mode Asset/EditorBuildSettings 四保护 SHA 保持原值。原 Battle 隔离缺 hid6 根自然 Play 仍待，本包继续 `RUNTIME_PENDING`，P-20/Q09 与 Q07 分别开放。

2026-09-29 最终状态覆盖上一段末尾的 Play 待办：原 Battle 隔离缺 hid6 根自然物理输入/完整 Driver 的[限定 Play 验收](UNITY-MISSING-HID6-BATTLE-PLAY-ACCEPTANCE.md)已通过，`NTSD28-Q09-P20-MISSING-BODY-SHEET-DEGRADE-001` 对其声明的角色本体sheet缺失行为改为 `VERIFIED`。本文件保留前期聚焦证据和夹具纠错过程；P-20/Q09 的其它画面出口仍开放。
