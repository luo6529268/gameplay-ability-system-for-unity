# PRE-FLIGHT 最后路径绑定请求（同一收敛项）

只读供证/主审窄读请求，不授实现或运行。执行会话目前只定位声明和直接调用行；未展开下列新方法体。

1. `Assets/NTSD/Scripts/Animation/BattleContentSource.cs` SHA `d3de7cd93c965e7ddab03d3098f55284eb727d48fd4d39ad50c0a7535d8e04d5`：构造23–46、ForLoganRuntime49–52、ResolveImagePath73–88及其直接规范化/范围门NormalizeKey95–103、ResolveWithin105–116。需要确认formal分支的DatRoot/ImageRoot与实际VFS绑定、绝对路径/越界处理；不读legacy ForUnityProject/ResolveDatPath/ResolvePublishedFolderPath。ImageRoot是实际成员名，没有VfsRoot属性。
2. `Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs` SHA `6990b2534ca6c832ab50c5fd692e3275ca335b2e2c77c1502bcc85eaa394ea34`：仅ConfiguredContentRoot44与ResolveConfiguredLoganSource50–53的根绑定。已rg见Application.dataPath/..与配置root组合；实际GameConfig.asset:15为`Assets/NTSD/Content/LoganRuntime`，SHA `25c8e08f4d1be55ee55d0404aaf4e46d099afc6c2ba3d9040e1c5203f654d9ff`。不读preparation/publication/cache正文。
3. KillIcon若仍需数据映射闭合：`Assets/NTSD/Scripts/Animation/LoganModeComboInput.cs` SHA `d7f45da0fc34fdfec3d11d8738899c0be44676e77bea9b4732fb9acc7221daa6`，仅`LoganModeKnockoutFeedInput`固定数组声明14与`TypeResourcePath`99–104；不读两个构造、FromProjectSnapshot、Parse或feed producer。当前`Assets/NTSD/Resources/ProjectBattleModeConfig.asset`44–51有七个typeImagePaths（c/sk2/sk2/sk1/sk2/sk2/sk2），其到该数组的实际输入值若无现成原件，保持数据绑定未证明，不为此自行扩开构造。

可复用的已解析集合仅为 `artifacts/diagnostics/NTSD-H08-RESIDENT-VISUAL-MEMORY-REVIEW-20261010/inventory.json`，SHA `c6ab501ea8b79481ab8afc83ba7ee2a9e72fe5066a9f4b6d9b8c1e5f9afc7154`，330定义/773 body引用/703路径。本轮`resolved-input-deployment-comparison.json`逐项与冻结manifest比较703/703、现长/SHA无差；该旧清单scope明确排除UI，不能用于head/small。当前尚未找到现成完整head/small实际解析清单；没有调用parser/Candidate/Unity补造。若主审已有当前Candidate公开数据原件，可直接供证其路径集合/指纹；否则准确保留为部署前冷资格缺口，不声称完整raw UI闭包。

主审已供Candidate Words/Spark/KillIcon及Capture/GetImageSha256边，执行会话不重复读；三个common缺失策略分别保留。本文只为判断真正的部署增量，不要求补六个WORDS或复制legacy目录。原manifest/新formalCSV/旧processor都不改。
