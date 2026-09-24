# Q07 旧 DAT/角色图动态 owner 门槛（2026-09-24）

状态：`CURRENT_DEFAULT_FORMAL / EMPTY_ROOT_LEGACY_REACHABLE / NO_DELETE_CERTIFICATE`。本轮只读核对及诊断工件，未改脚本、DAT、图片、Scene、Prefab或项目设置；不授权删除或移动资源。

当前 `Assets/NTSD/Config/GameConfig/GameConfig.asset` 将 `BattleContentRuntimeRoot` 序列化为 `Assets/NTSD/Content/LoganRuntime`。`LoadingPrewarmController.PrewarmOnceCoreAsync` 在根非空时进入 `PrewarmConfiguredLoganContentAsync`，`CharacterAnimtorManager.RefreshAllData` 和 `CharacterFramePreviewWindow.LoadDataFile` 在正式根下也已转向正式路径或跳过旧索引。`GameDataManager` 单例已不隐式读取旧 `data.txt`。因此已配置的默认战斗路径不以旧138 DAT/383图为正式内容。

旧内容仍有实际可达入口：根为空时，`LoadingPrewarmController` 显式提交 `CharacterConfig`、`CharacterSprites` 任务；`BattleTestBootstrap.LoadCharacterDataAsync` 走旧 `ParseCharacterFrameConfigs`/`LoadCharacterSpritesAsync`；Inspector 手动刷新走相同分支；帧预览窗口会显式读旧 `data.txt`。`CharacterAnimtorManager.ParseCharacterFrameConfigs` 根据旧索引逐 DAT 解密/解析，再由旧 sprite prewarm 从 DAT 的图片声明生成路径和头像。`GameDataManager.LoadDataFile` 仍提供显式旧索引 API；`BattleParityTraceEditor`、`NTSD28UnityRawCaptureEditor`、自检和历史夹具另有明确旧路径或空根用途。不能只凭当前默认根非空就把旧内容删除。

新鲜字节/索引复核：当前旧 `Assets/NTSD/Config/data.txt` SHA-256 为 `DB7A5F7A02B45E8461701D5033F30DA7128A07ED1B07134B5E5822884121B913`，与 Q01 冻结 `unityRegistry` 哈希一致；当前包含137条对象索引，其中42条type0，对应137个互异旧DAT路径。Q01清单另有1份未索引 `Assets/NTSD/Config/effect/weapon4.dat`，其静态无已知reader结论仍以独立Q07审计为准。对Q01旧资源清单逐文件重新SHA：138份DAT和383张旧索引角色图片共521/521同路径存在且字节与Q01相同；383张图片各有Q01已解析的对象声明 owner。证据逐行在 [current-old-indexed-byte-check.csv](current-old-indexed-byte-check.csv)，汇总在 [current-old-index-summary.json](current-old-index-summary.json)。既有动态声明图因此仍对应当前文件字节与索引；这只是入口/内容证明，不代表这些旧对象在当前默认正式战斗中实际出生。

两项近期测试迁移后的[383图静态重扫](../NTSD28-Q07-OLD-CHARACTER-IMAGE-CURRENT-OWNER-001/post-formal-test-indexed-image-reference-scan.csv)发现GUID owner 0、精确文本路径0；与本报告的DAT动态声明 owner 不矛盾。旧Q01的另174张图片不在这383张索引图的替换集合中，含菜单/HUD/地图/阴影等保留或独立审阅资源，不能并入批量角色图退场。

下一实施门槛：先在独立 Task/Change 中决定并验证空根战斗预热、测试 bootstrap、手动 Editor 工具和原始 trace/self-check 的替代内容或明确保留方式；不得因正式根当前非空便直接剪掉兼容分支，也不得让缺失旧文件在菜单或测试中静默降级。随后以新版逐文件 owner 图、运行时验证和用户对准确删除清单的授权处理旧DAT/图片；`data.txt` 与174张其他图片单独分类。Q07及总目标仍开放。
