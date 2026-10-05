# NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-PREPARE
状态：RUNNING。类型：授权代码/治理文档最小编辑；新增战斗 WAV/.meta 另建精确清单，不删除/移动任何既有资源。
Task：NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006。执行者：/root，工作目录 I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity。开始时间：2026-10-05T18:58:40.216798+00:00。
用户授权：2026-10-06 指定 03_声音资源，要求修复并先对齐所有角色/战斗音效；DAT 数据禁止修改。
覆盖范围：manifest-before.json 的14项，只做新增说明或目标行为 hunk，保留所有既有未提交内容。
操作前：逐文件SHA、Git状态与原字节备份在 manifest-before.json / before；备份SHA逐项相等。
拟执行：Python精确替换/追加、apply_patch；Unity MCP原Editor6401 Refresh/具名测试，禁止全套/第二Editor。
恢复：以 before 的实际字节或精确反向hunk恢复本任务内容；任何恢复执行仍需独立操作记录和授权，不使用HEAD覆盖用户修改。
未执行：资源替换、DAT/Scene/Input/模式/非战斗文件编辑、删除、Git丢弃。
后续：追加真实脚本、命令、输出、after SHA及范围检查；本RUNNING不是完成证据。

实际新增精确测试路径：Assets/NTSD/Scripts/Test/Editor/NTSD28BattleAudioCatalogEditorTests.cs、NTSD28BattleAudioAlignmentEditorTests.cs 及Unity自动生成对应meta；两个Change Record状态修改前已备份在before-status-update。代码hunk由root+bounded luna worker执行，不含DAT/Scene；ECS后续集成仍按manifest前原字节保护。
