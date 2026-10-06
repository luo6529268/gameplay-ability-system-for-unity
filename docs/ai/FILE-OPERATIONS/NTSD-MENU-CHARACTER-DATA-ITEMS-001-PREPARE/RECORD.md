# NTSD-MENU-CHARACTER-DATA-ITEMS-001-PREPARE

状态：PLANNED。类型：精确备份后的代码编辑、治理追加与新聚焦测试文件创建；无删除、移动、Git丢弃。

用户授权：2026-10-06当前会话“根据data.txt文件来生成对应的角色Item，默认是随机角色Item……只是让你看看角色Item的生成，其他逻辑还是我们自己项目的逻辑”。执行者Codex，工作目录 I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity。

Task/Change：NTSD-MENU-CHARACTER-DATA-ITEMS-001。具体代码覆盖范围为三UI脚本；新文件 CharacterSelectionBoardEditorTests.cs（及Unity生成其.meta）。Ledger/STATE/handoff/本索引仅追加。精确十文件路径、SHA/Git状态、字节大小和前镜像见 artifacts/diagnostics/NTSD-MENU-CHARACTER-DATA-ITEMS-001/prechange-manifest.json，before副本逐项SHA相等。Menu/Battle Scene和GameConfig为只读保护、不会保存。

拟执行：apply_patch；dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly；原Unity Editor资产刷新与run_tests定向测试；测试截图写同ID diagnostics目录中唯一新PNG，TestRunner输出保存新文件。原Editor仅在idle/非Play/clean时运行测试；无第二Editor。

恢复：检查目标未被后继编辑后使用精确before副本，文档追加纠正；新测试文件若需删除须另记授权。执行时间、命令结果、后镜像与实际影响待追加。


执行完成（2026-10-06）：EXECUTED / VERIFIED（限定）。三UI脚本最小编辑，新聚焦测试和Unity生成.meta；治理文档追加阶段/更正/结果。备份未删除且SHA核对相同，Scene/配置只读保护未写。四次生成工程编译最后0error；原Editor三项直接通过、实际截图、清理已完成测试的桥残留元数据。validator最终及git diff --check结果见Task diagnostics，逐文件后SHA/保护项/新增清单见 postchange-manifest.json。无删除/移动/Git丢弃/提交/push，无第二本项目Editor。
