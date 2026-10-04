# LoganRuntime 图像缺失现场观察（2026-10-05）

本轮只读 `git status --short -- Assets/NTSD/Content/LoganRuntime/vfs/sprite` 发现 36 个已跟踪路径标为 `D`，均为图片或 `.meta`，包括正式战斗 WORDS0～5、combo_hits、INKHUD 部分图和 kill 部分图；逐路径状态原件见 [git-status-sprite.txt](git-status-sprite.txt)。`Test-Path` 确认 WORDS0 与 WORDS5 当前不存在，`sprite/UI/SPARK.png` 当前存在。`docs/ai/FILE-OPERATIONS` 的 `RECORD.md` 中未搜索到 WORDS0 或 combo_hits 对应操作记录；具体执行者和删除原因未由本轮独立查明。用户随后确认“是，先保留这些删除”，故该现场按用户或其他任务的进行中工作保护。本轮没有删除、恢复或覆盖这些资源。

此前 Q01 的“活跃战斗七图 7/7 暂存且 SHA 同版”是当时快照，不能当作目前磁盘在位结论。当前六张 WORDS 图不在磁盘上；正式版文件及 Git 跟踪历史仍可用于后续核对。按用户本轮确认保留这批删除；不运行依赖这些图的 Unity Game View 对照，也不把缺图画面判作战斗逻辑首差。若后续明确将这批删除作为最终内容改动交付，仍需按项目逐文件审计合同补记原因、路径及恢复依据。

## 对正式内容预热的当前影响（只读生产调用链）

影响不止 Game View 对照。未修改的 `Assets/NTSD/Config/GameConfig/GameConfig.asset` 当前将 `BattleContentRuntimeRoot` 配为 `Assets/NTSD/Content/LoganRuntime`。原 Battle Scene 的 `BattleTestBootstrap.LoadCharacterDataAsync()` 必须调用 `CharacterAnimtorManager.PrewarmConfiguredLoganContentAsync()`；正常加载界面的 `LoadingPrewarmController.PrewarmOnceCoreAsync()` 在此根非空时也调用它。管理器会验证已缓存候选的新鲜度，失败时重新执行 `LoganVisualContentCandidate.Capture(source, modeSnapshot)`。该捕获在 `resource.dat` 存在时无条件调用 `NativeWordsInput.Capture`；正式索引 16～21 为 `sprite/UI/WORDS0.png`～`WORDS5.png`，构造每个 `ImageInput` 时使用 `HashFile`→`File.OpenRead(path)`，没有允许缺失 WORDS 的分支。当前 `resource.dat` 存在、WORDS0 缺失，因此**只要正式内容预热执行到 WORDS 捕获，就会在 WORDS0 文件读取处失败**；缓存验证同样会重新读取 WORDS，并因 IO 异常退回重捕获。此结论是当前代码与磁盘状态的确定性路径推断，未运行新的 Unity Play，不能冒称已观察实际异常堆栈或排除更早的独立加载错误。其他被删图片的具体消费者未在此逐项裁决。

来源：`CharacterAnimtorManager.cs:44-52, 99-131`，`BattleTestBootstrap.cs:489-495`，`LoadingPrewarmController.cs:131-140`，`LoganVisualContentCandidate.cs:19-37, 40-88, 292-330, 415-420`，`BattleContentSource.cs:30-37, 73-80`，[资源索引快照](../NTSD28-336B44-Q01-RESOURCE-INDEX-CLOSURE-20261005/resource-index-closure.csv)。这是一项受用户确认保留的进行中资源改动对当前运行前置的影响；不据此修改 DAT、删除授权或战斗规则。
