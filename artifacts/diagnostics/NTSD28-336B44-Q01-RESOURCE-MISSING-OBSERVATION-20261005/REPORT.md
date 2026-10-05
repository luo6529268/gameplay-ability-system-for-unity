# LoganRuntime 图像缺失现场观察（2026-10-05）

2026-10-05 再核：`WORDS0～5.png` 六路径仍全缺；直接 `BattleTestBootstrap.LoadCharacterDataAsync` 和菜单 `LoadingPrewarmController.PrewarmOnceCoreAsync` 都进入 `PrewarmConfiguredLoganContentAsync`。因此下文“只限制 Game View 对照”的早期描述应读作**同时限制经该生产预热链进入战斗**；这是代码与文件状态推断，未运行新 Play。正式 `d3d11_renderer.cpp` 会在实体 nameplate 命令中选择 WORDS 字形，Unity 中 `BattleCommonVisualCatalog`/中央资源解析器也保留 nameplate 字形路径，不能把这六图直接裁成纯菜单素材或不经裁决跳过。用户确认保留删除，本任务不恢复图、不改 DAT/加载器；战斗规则审计独立继续。

本轮只读 `git status --short -- Assets/NTSD/Content/LoganRuntime/vfs/sprite` 发现 36 个已跟踪路径标为 `D`，均为图片或 `.meta`，包括正式战斗 WORDS0～5、combo_hits、INKHUD 部分图和 kill 部分图；逐路径状态原件见 [git-status-sprite.txt](git-status-sprite.txt)。`Test-Path` 确认 WORDS0 与 WORDS5 当前不存在，`sprite/UI/SPARK.png` 当前存在。`docs/ai/FILE-OPERATIONS` 的 `RECORD.md` 中未搜索到 WORDS0 或 combo_hits 对应操作记录；具体执行者和删除原因未由本轮独立查明。用户随后确认“是，先保留这些删除”，故该现场按用户或其他任务的进行中工作保护。本轮没有删除、恢复或覆盖这些资源。

此前 Q01 的“活跃战斗七图 7/7 暂存且 SHA 同版”是当时快照，不能当作目前磁盘在位结论。当前六张 WORDS 图不在磁盘上；正式版文件及 Git 跟踪历史仍可用于后续核对。按用户本轮确认保留这批删除；不运行依赖这些图的 Unity Game View 对照，也不把缺图画面判作战斗逻辑首差。若后续明确将这批删除作为最终内容改动交付，仍需按项目逐文件审计合同补记原因、路径及恢复依据。

## 对正式内容预热的当前影响（只读生产调用链）

影响不止 Game View 对照。未修改的 `Assets/NTSD/Config/GameConfig/GameConfig.asset` 当前将 `BattleContentRuntimeRoot` 配为 `Assets/NTSD/Content/LoganRuntime`。原 Battle Scene 的 `BattleTestBootstrap.LoadCharacterDataAsync()` 必须调用 `CharacterAnimtorManager.PrewarmConfiguredLoganContentAsync()`；正常加载界面的 `LoadingPrewarmController.PrewarmOnceCoreAsync()` 在此根非空时也调用它。管理器会验证已缓存候选的新鲜度，失败时重新执行 `LoganVisualContentCandidate.Capture(source, modeSnapshot)`。该捕获在 `resource.dat` 存在时无条件调用 `NativeWordsInput.Capture`；正式索引 16～21 为 `sprite/UI/WORDS0.png`～`WORDS5.png`，构造每个 `ImageInput` 时使用 `HashFile`→`File.OpenRead(path)`，没有允许缺失 WORDS 的分支。当前 `resource.dat` 存在、WORDS0 缺失，因此**只要正式内容预热执行到 WORDS 捕获，就会在 WORDS0 文件读取处失败**；缓存验证同样会重新读取 WORDS，并因 IO 异常退回重捕获。此结论是当前代码与磁盘状态的确定性路径推断，未运行新的 Unity Play，不能冒称已观察实际异常堆栈或排除更早的独立加载错误。其他被删图片的具体消费者未在此逐项裁决。

来源：`CharacterAnimtorManager.cs:44-52, 99-131`，`BattleTestBootstrap.cs:489-495`，`LoadingPrewarmController.cs:131-140`，`LoganVisualContentCandidate.cs:19-37, 40-88, 292-330, 415-420`，`BattleContentSource.cs:30-37, 73-80`，[资源索引快照](../NTSD28-336B44-Q01-RESOURCE-INDEX-CLOSURE-20261005/resource-index-closure.csv)。这是一项受用户确认保留的进行中资源改动对当前运行前置的影响；不据此修改 DAT、删除授权或战斗规则。

## 36 项删除的其余入口归类（后续只读复核）

本地提交 `984130d9` 的这 36 项是 17 张 PNG、17 个逐图 `.meta` 和两个目录 `.meta`。其中六张 WORDS 是上文已证的正式内容预热硬读取；`resource.dat` 仍在，首次缺失会在 `NativeWordsInput` 构造 `ImageInput` 时失败。其余 11 张按当前生产代码的范围如下：

| 图片 | 数量 | 当前消费边界 |
| --- | ---: | --- |
| `sprite/combo_hits.png` | 1 | 正式 `<combo>` 声明有此图，但 336B44 普通 GUI 的显示锁存默认关闭；Unity 所检生产表现代码也无该图的读取者。连击计数仍生效，缺图不是当前预热硬读取或已证普通画面首差。详[显示门槛复核](../NTSD28-336B44-Q09-COMBO-DISPLAY-GATE-20261005/REPORT.md)。 |
| `sprite/frame/INKHUD/{BARS,FRAME,team0～4}.png` | 7 | 原生普通战斗 HUD 素材；Unity 战斗脚本对这些精确路径的搜索无直接读取者，且普通 HUD 属用户保留的自有表现例外。仅为所检源码范围的负证，不声称所有动态路径均不可达。 |
| `sprite/kill/{c,sk1,sk2}.png` | 3 | 原生 KO 图文属用户排除项。Unity `NativeKillIconInput.Capture` 对缺失文件记录 `null`，`BuildKillIconPublicationAsync` 遇 `null` 跳过；因此这三张缺失不会以与 WORDS 相同的方式阻断候选预热。它们仍会改变可选图标发布结果，不能称与所有表现无关。 |

此归类只说明当前源码/配置下的直接消费者与预热门槛，未运行新的 Unity Play，也未恢复、删除或覆盖任何图片。`WORDS0～5` 的预热影响仍真实存在，故不能把整批删除笼统记录成“完全不影响验证”；同时不因用户已批准保留的普通 HUD/KO/诊断图缺失扩大 Q09 修复范围。
