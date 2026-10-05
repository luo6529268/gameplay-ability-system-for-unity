# NTSD28-336B44-Q01-WORDS-REMOVAL-PREWARM-001

状态：`COMPILE_PASS / EDITOR_TEST_PENDING`。归属当前 336B44 总表 Q01 内容接入，不改变战斗规则、DAT、角色图、场景或非战斗流程。

2026-10-05 后续裁决覆盖本 Task 原计划中的“部分缺失继续失败”：用户明确选择只缺一部分也跳过整组 WORDS、继续预热。同一生产入口由已有 `NTSD-WORDS-PREWARM-DEPENDENCY-001` Change Record 负责；本 Task 的严格缺图生产门已撤，现仅负责 Q09 身份测试适配和证据交接。以下原计划作为脚本修改前历史保存，不再裁决生产行为。

用户于 2026-10-05 确认 `LoganRuntime/vfs/sprite/UI/WORDS0～5.png` 六图由其删除，要求调整战斗预热；角色名字以后改用别的方式，本批暂不处理绘制。当前 `GameConfig` 指向该暂存根，`NativeWordsInput.Capture` 因 `resource.dat` 在位而无条件对六图求 SHA，新鲜战斗候选在 WORDS0 文件读取处失败。正式 336B44 根仍保留六图，原生/Unity 的名称字形可作为历史与后续表现对照，但不能要求本批恢复它们。

准确代码范围：`Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs::NativeWordsInput.Capture`；聚焦测试 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09WordsInputIdentityEditorTests.cs`。只允许“六张索引图全部缺失”返回无 WORDS 输入，保持 `resource.dat` 索引/路径合法性校验；六张全在仍按原 SHA 与新鲜度合同捕获，只缺一部分继续失败，不把其它资源缺失变成可忽略。下游现有 `WordsInput == null` 分支跳过字形发布，战斗公共图集的运行就绪门仍要求阴影和 SPARK。角色名字暂缺由用户明确接受；不能生成替代图、临时外部资源依赖、修改 DAT 或覆盖用户删除。其它已删资源分别沿现有消费者处理，本 Task 不扩大。

执行前保护：核对两目标脚本无任务外未提交改动；记录当前 `resource.dat` 与六个路径存在性、正式 EXE 身份、`GameConfig` 根。先建同 ID Change Record，再改脚本。测试只跑现有 Q09 WORDS 身份定向项及必要的相邻候选项；如 Unity Editor 与并行任务冲突，先做生成工程编译与静态确认，Play 留待安全前置满足。运行测试不得重启或打开第二个同项目 Editor。任何测试清理都遵守文件操作审计合同。

验收：当前暂存根候选捕获成功且 `WordsInput == null`，正式根六图仍 `WordsInput.Images.Count == 6`；两端对象图候选数量仍 906，SPARK 不退化；只缺一部分的假设继续 fail-visible；内容新鲜度能区分无 WORDS 与六图恢复；生成编译及定向测试结果如实记录。真实 Battle Scene 预热、Game View 与完整对齐不从这些测试自动推断。回滚只能以前向精确更正恢复旧捕获门；不执行 `git restore` 或恢复已删除图片。保护当前用户/并行 Scene、UI、字体和其它文件。
