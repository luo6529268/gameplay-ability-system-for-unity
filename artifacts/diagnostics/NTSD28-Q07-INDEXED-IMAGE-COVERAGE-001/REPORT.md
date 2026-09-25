# Q07 正式索引图片逐字节覆盖与 smallb 消费边界（2026-09-24）

> **2026-09-24 权限更正（覆盖下文 Q09 HUD 待接线建议）：** 对齐总表 §0.2、§1.2 和 P-17 已明确将完整原生角色 HUD 列为 `USER_EXCLUDED`。104 张 `smallb` 的 1,010/1,010 部署与 SHA 事实保留，但其原生 HUD 消费不属于当前实施出口；不应以差额要求 Unity 候选从 906 扩到 1,010。此前新增的 smallb 运行时发布由 `NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001` 精确纠正。下文静态首差仍是对照事实，不是待修授权。

状态：`1010_INDEXED_IMAGES_STAGED_BYTE_IDENTICAL / 104_SMALLB_NOT_IN_CURRENT_CANDIDATE / Q09_BATTLE_HUD_READER_OPEN`。本轮只读比对与诊断工件；未修改图片、DAT、脚本、Scene、Prefab或ProjectSettings。逐图路径、对象ID、引用角色与SHA在 [indexed-formal-staged-image-sha.csv](indexed-formal-staged-image-sha.csv)，汇总见 [summary.json](summary.json)。

以Q01的正式 indexed object 图片声明为输入，当前共有2,005条引用、1,010个不同的VFS图片路径。重新读取正式 `resources/runtime/vfs` 与项目 `Assets/NTSD/Content/LoganRuntime/vfs`：两侧各1,010/1,010文件存在；正式文件1,010/1,010匹配Q01冻结SHA；项目暂存文件1,010/1,010逐字节匹配当前正式文件。因此这组对象声明图片的**磁盘部署**已完整，并不代表每张都被Unity发布或实际显示。正式整个VFS其余PNG和旧174张非索引图不由本结论覆盖。

906与1,010的差额精确归类：Q01原声明不同图片集合 `bmp:file` 703、`bmp:head` 97、`bmp:small` 106，三者并集906；额外 `bmp:smallb` 104张与前三类零交集，四类并集1,010。当前 `LoganVisualContentCandidate.Capture` 只从 `CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog` 的 `files/head/small` 收图，所以现有候选906与上述集合吻合，104张 `smallb` 没有进入该生产候选。不可将差额误报为文件缺失，也不可认为 `head` 或 `small` 能代替它。

正式 playable 闭包 `source/ntsd28_core/src/rendering/render_snapshot.cpp` 的战斗 HUD 构建在角色type0且有HUD槽时优先读取定义的 `bmp.last("smallb")`，缺少才回退 `bmp.last("small")`，并检查所选图片文件可用；`source/ntsd28_playable/src/d3d11_renderer.cpp` 消费该 HUD 图像。当前Unity `Assets/NTSD/Scripts` 对 `smallb` 无生产读取，`CharacterUIResourceManager`只有head/small；当前源码搜索未找到 `GetSmallSprite` 的生产调用，已见选人UI调用的是 `GetHeadSprite`。因此这是正式战斗HUD图片选择/发布的静态对齐缺口，归Q09表现消费及R17回访；需在独立Task中沿HUD快照、资源预热、发布身份、布局和实际像素建立合同，不能只把104张图作为菜单头像或简单换路径。当前Battle HUD/项目既有UI例外和非战斗菜单不得顺手重做。

Q07结论限定为图片内容就位。Q07的正式资源整链、旧资源退场和其他自然技能仍开放；Q09需对实际HUD头图建立权威同场景可见验收。`SPARK.png`仍属于已记录的Q09原始ID/图集接线，不能只替换旧`SPARK.bmp`文件名。没有资源删除授权。

2026-09-25 current-disk rerun: `current-indexed-image-audit-20260925.json`逐SHA重读本报告CSV的1,010个唯一正式引用图片路径，正式/暂存均1,010/1,010存在、正式对Q01和暂存对正式均零差异。角色关联发布的906张与用户排除的原生HUD `smallb` 104张边界不变。这只确认当前文件字节，不证明Unity importer、发布、自然技能像素或旧图可删。
