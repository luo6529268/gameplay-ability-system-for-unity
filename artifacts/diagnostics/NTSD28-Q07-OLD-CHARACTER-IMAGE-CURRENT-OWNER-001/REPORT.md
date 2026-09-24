# Q07 旧角色图片当前 owner 回访（2026-09-24）

2026-09-24 新鲜静态重扫：以同一383张旧索引图的路径/GUID为对象，遍历当前 `Assets` 内9,825个所选文本/序列化文件（含 `.meta`，排除图片自己的 `.meta` 自声明），383张同路径仍存在，逐文件序列化GUID owner为0，精确路径文本引用为0。逐行结果见 [post-formal-test-indexed-image-reference-scan.csv](post-formal-test-indexed-image-reference-scan.csv)，所有 `deleteAuthorized=False`。这覆盖了两项测试迁移后的静态引用现状；旧CSV的“2张文本引用”仅是改动前快照。扫描不解析旧 DAT 动态图片声明、不证明空根 legacy 加载不可达，也不批准删除任何文件。

后继修正：`NTSD28-Q07-INSPECTOR-FORMAL-REFRESH-001` 已使 Inspector 正式根刷新复用现有正式预热；`NTSD28-Q07-FRAME-PREVIEW-FORMAL-INDEX-001` 已使角色帧预览窗口在正式根下不再加载旧 `data.txt`；`NTSD28-Q07-LEGACY-GRID-TEST-FIXTURE-001` 已移除两个旧 BMP 网格测试精确路径并在原Editor 3/3通过。下方表格及383图CSV是这些改动前的 owner 快照，其中网格测试“两路径”与CSV“2张文本引用”已过期；不得把它们当作当前删留结论。空根显式 legacy、其他历史工具/测试引用与逐文件删除门仍开放。

后继测试迁移（覆盖下方旧表相关行）：`NTSD28-Q07-FORMAL-CHARACTER-DEPLOYMENT-001 / VERIFIED` 已将 `CharacterAssetDeploymentEditorTests` 的42旧DAT/BMP断言改为当前158条正式type0逐DAT/声明图片SHA验收，原Editor聚焦+相邻3/3 PASS。旧表“42角色测试仍依赖旧内容”已过期；空根legacy用途和动态资源可达性仍开放，详该包ACCEPTANCE。

状态：`CURRENT_DEFAULT_SOURCE_CONFIRMED / LEGACY_AND_TEST_DEPENDENCIES_OPEN / NO_DELETE_CERTIFICATE`。本轮只读核对，未改 DAT、图片、Scene、Prefab 或脚本。旧 `NTSD28-Q07-OLD-ASSET-REFERENCE-REFRESH-001` 的 695 路径图是 2026-09-22 快照，不能把其中“佐助旧 BMP 仍有 Scene GUID owner”当作当前事实。

当前 `Assets/NTSD/Config/GameConfig/GameConfig.asset` 的 `BattleContentRuntimeRoot` 为 `Assets/NTSD/Content/LoganRuntime`。`CharacterAnimtorManager.PrewarmConfiguredLoganContentAsync` 通过该根创建正式内容候选，并使用项目 `ProjectBattleModeConfig` 快照；`LoadingPrewarmController` 在该根非空时取消旧 `CharacterConfig` 和 `CharacterSprites` 任务。因此，**当前序列化默认预热**不需要先加载旧 `data.txt` / 旧 BMP。此结论只针对这条入口，不能推出所有测试、手动编辑器操作或空根配置下的旧资源均不可达。

旧图退场的具体剩余 owner：

| 路径/入口 | 当前观察 | 退场条件 |
|---|---|---|
| `NTSD_Battle.unity` 的禁用 `BattleCentralEditorPreview` | `sourceSheet` GUID 为正式 `vfs/c/sasu/sasu.png.meta` 的 `b5d608d7fe42c474ea0f2bb728a122a2`，裁剪 `y=881`；`rg` 对旧 Sasuke GUID `6d174fff55a50784d9bbf85531fb7d86` 的唯一 Assets 命中是旧 BMP 自身 `.meta`。 | 旧的 Scene GUID 阻塞已解除；这只证明该预览引用，不证明旧 BMP 可删。先复用已留存的原/隔离 Editor 预览证据并核对当前场景状态。 |
| `CharacterAnimtorManager.RefreshAllData`、`ParseCharacterFrameConfigs`、`LoadCharacterSpritesAsync`、`LoadAllCharacterUISpritesAsync` | 显式 Editor 刷新仍可从旧 `data.txt` / DAT 的图片声明组装路径并读旧图片。`LoadingPrewarmController` 在配置根为空时也保留旧任务。 | 后续独立 Task 决定并实现旧入口替代/退休，保护预览、测试和空根行为；修改脚本前建 Task/Change。 |
| `CharacterAssetDeploymentEditorTests` | 显式断言 42 个旧 type-0 DAT 及其旧 BMP 声明文件存在。 | 在旧数据退场包中迁移此历史夹具或标定其保留用途；不可先删资源再把测试失败视为无关。 |
| `BattleSpriteGridSeparatorEditorTests` | 仍有 `naruto_0.bmp`、`sasuke_0.bmp` 两个精确旧图片路径。 | 明确其网格算法验收是否迁到正式 PNG 夹具；正式 PNG 布局/透明规则需要对应断言，不做扩展名替换。 |

2026-09-22 图中 `OLD_INDEXED_IMAGE` 为 383 行。本轮新鲜扫描已重建**这 383 张旧索引图**的静态 GUID/精确文本引用部分；没有重建全 695 路径或动态读取图，故旧图表的 Sasuke 行明确失效。当前工作树中旧图片或 Scene 文件未被本轮修改，但这也不是完整不可达证明。删除授权仍为零；不得批量移动或删除旧 `Config`、`Sprite`、HUD、Menu、地图、阴影资源。下一 Q07 可执行包应先收窄旧资源加载入口和受它约束的测试/编辑器功能，再形成逐文件替换/保留清单。正式内容的自然技能、可观察表现、Q08～Q12 出口仍独立开放。

## 当前逐文件静态引用扫描

证据为 [current-indexed-image-reference-scan.csv](current-indexed-image-reference-scan.csv)：以旧 Q01 图的 383 个 `OLD_INDEXED_IMAGE` 路径/GUID 为待核对象，重新扫描本工作树 `Assets` 内 2,901 个文本 Scene、Prefab、Asset、材质、动画、脚本及配置文件（不把图片自身的 `.meta` 当引用 owner）。383 张图片当前同路径均存在；扫描到**序列化 GUID owner 0 张**、**精确路径文本引用 2 张**，后者均在 `BattleSpriteGridSeparatorEditorTests.cs`，对应 `naruto_0.bmp` 和 `sasuke_0.bmp`。扫描结果逐行保持 `deleteAuthorized=false`。这比旧快照的“1 张存在 Scene GUID owner”更符合当前场景状态；没有用旧快照的 SHA/GUID 是否相同字段冒充新鲜哈希复核。

此扫描识别的是字面 GUID 与完整路径，不解析加密旧 DAT 声明、运行时拼接路径、`Resources.Load` 或反射。`CharacterAnimtorManager` 的旧 `data.txt`/DAT→图片路径链和明确的空根/手动入口仍存在，因此 `381 张没有静态引用` **不等于** `381 张可以删除`。下一步应先解决这些实际消费者的目标行为，随后再生成涵盖动态声明、引用重绑和精确删留名单的退场证书。
