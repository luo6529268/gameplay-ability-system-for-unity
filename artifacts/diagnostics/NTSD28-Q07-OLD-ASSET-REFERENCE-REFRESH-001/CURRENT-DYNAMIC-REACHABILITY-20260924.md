# Q07 旧索引角色图片当前可达性复核（2026-09-24）

状态：`CURRENT_FORMAL_CONTENT_VERIFIED / OLD_FALLBACK_STILL_REACHABLE / NO_DELETE_AUTHORIZATION`。本次是只读调用链和定向原项目 Editor 验证；没有删除、移动或修改 DAT、图片、Scene、Prefab、生产脚本。2026-09-22 的 `REPORT.md` 与 `SERIALIZED-OWNER-CLASSIFICATION.md` 是当时快照，下面两处结论已被后继工作改变。

1. 当前序列化 `GameConfig.asset` 的 `BattleContentRuntimeRoot` 是 `Assets/NTSD/Content/LoganRuntime`。`LoadingPrewarmController.PrewarmOnceCoreAsync` 在根非空时调用 `CharacterAnimtorManager.PrewarmConfiguredLoganContentAsync`，空根时才排入旧 `ParseCharacterFrameConfigs` 和 `LoadCharacterSpritesAsync`。`BattleTestBootstrap.LoadCharacterDataAsync` 有相同正式根门控。正式发布的 `LoadCharacterSpritesCoreAsync` 从 `native.Candidate.Catalog.Source` 读取图片并对声明图片进行 SHA 约束；战斗入口 `ValidateConfiguredContentForBattleAsync` 在非空根时要求当前已发布候选。当前配置下的生产预热不应被旧空根分支代替。
2. 旧报告称 `CharacterAssetDeploymentEditorTests` 遍历旧 type-0 DAT/BMP，现已过期。当前 `FormalTypeZeroCharacterDatAndImagesAreDeployed` 从正式和暂存 Logan catalog 分别读取 type-0 对象，核 DAT SHA、解析帧数、声明图片路径及图片 SHA。原项目 Editor job `ed1e857c29b84159bea5bbbe3c583f5c` 在当前内容下 **1/1 PASS**，0 failed/0 skipped；精简结果见 `FORMAL-TYPE0-DEPLOYMENT-ORIGINAL-EDITOR-SUMMARY.json`，原始 job 输出见同目录 `FORMAL-TYPE0-DEPLOYMENT-ORIGINAL-EDITOR-JOB.json`。该测试覆盖正式 type-0 声明内容，不等于全部 330 对象、所有动态技能或正式 EXE 图像验收。
3. 旧报告所称的唯一旧图片 Scene GUID `sasuke_0.bmp` 已由 `NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001` 重绑为正式 `sasu.png`，原项目 Editor 的预览图和 11/11 聚焦测试现也通过。以旧索引图片清单的 **383 个 GUID** 为模式，在当前 `Assets/NTSD` 的 `.unity/.prefab/.asset/.mat/.controller/.playable` 文本文件中扫描，排除正式暂存原始子树，`rg` exit1、匹配行0；命令范围与结果见 `CURRENT-OLD-IMAGE-GUID-SCAN.json`。这只说明所列序列化文件中没有旧图片 GUID，不覆盖脚本组装路径、运行时反射、其它文件类型或 Unity 外部目录。
4. 旧空根分支**仍真实可达**：`GameConfig.BattleContentRuntimeRoot` 被设为空时，菜单预热和 BattleTestBootstrap 会解析 `Assets/NTSD/Config/data.txt`，再按旧 DAT 的图片声明取旧文件。`CharacterAnimtorManager.RefreshAllData` 的 Editor 操作也保留同一空根入口。旧 `data.txt`、旧 DAT 与角色图因此是保留的 fallback/诊断输入；当前配置为正式根与“允许删除所有旧图片”之间还隔着 fallback 退休、测试夹具迁移及相应回归。不能因 GUID 扫描为0而直接删除旧 138 DAT 或 383 张索引图片。

下一有界出口：先冻结空根 fallback 的生产/Editor/test 调用者与用户需要保留的兼容能力，另建精确 Task/Change 才能改变入口；将需要旧内容的测试改用独立夹具并验证正式根不回落后，再生成逐文件删除清单及引用安全证据。旧文件删除仍按仓库明确批准规则执行。Q07 继续 `IN_PROGRESS`，D-024 非体感整链、正式自然技能与最终 Player/EXE 表现仍开放。

验证备注：本报告与状态文档的 scoped `git diff --check` exit0，两个 Scene 磁盘 SHA 保持 `471396E7...7B9` / `785F828C...81E13`。全局 `Tools/Validate-ChangeLedger.ps1` 本轮返回 exit1；唯一 ERROR 是并行 UI 工作新 Record `NTSD28-BATTLE-UI-SCENE-HUD-COMBO-001` 把非代码 `Assets/NTSD/Scene/NTSD_Battle.unity` 声明为受治理 `code-path`。本只读审计没有编辑该 Record 或任何脚本；此全局校验未通过，不能写成 PASS。待该 Record 的所有者纠正后应重跑，不能通过改动无关 UI/Scene 使本包表面通过。

后继验证（同日）：并行 Record 所有者更新工作树后，重新运行完全相同的 `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <project>` 返回 exit0，`Change ledger validation PASSED`，794 Records。上一段的 exit1 是修正前真实结果，保留为时间顺序证据；当前全局校验状态为 PASS。本审计仍未因此取得旧文件删除授权。

并行提交后的现状边界：上一段所列 Battle Scene SHA 是本审计运行时快照；并行 UI/Scene 提交 `d18e1f5c` 后磁盘 Battle Scene SHA 变为 `2EE465D8...B77A`，Menu 仍 `785F828C...1E13`。新 Battle Scene 的禁用预览仍指向正式 `sasu.png` GUID；对同一383旧图GUID、同一文件扩展范围重跑 `rg` 仍 exit1、匹配0。没有将并行 Scene 内容纳入本 Q07 包，也没有用旧 SHA 声称当前 Scene 未变化。

2026-09-25 续审：空根旧资源调用者和 SelfCheck/Editor fixture 的当前代码归属已列于 `EMPTY-ROOT-RETIREMENT-OWNER-MATRIX-20260925.md`。正式序列化根非空只证明当前默认预热选正式内容；旧 `data.txt`、索引 DAT 和图片仍是空根生产分支及历史测试的可达输入。未改脚本或删除资源，Q07退场门仍开。
