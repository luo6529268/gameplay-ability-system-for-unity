# 第78批：SetPass 上游资源范围只读取证
状态：SOURCE_SCOPE_CLOSED / IMPLEMENTATION_NOT_AUTHORIZED / PERFORMANCE_UNCHANGED。
本批只读脚本、复用已留存的75/77运行证据并追加文档。57已执行（含78静态取证），六项阶段4/6、H07/H11 OPEN、34父项关闭0，Goal保持active；不把文档收口当作优化完成，不调用Goal状态变更。本轮未控制Unity、运行测试或测量。
需求来源：用户问“为什么不先修复SetPass也就是渲染指令的数量问题”。后继优先方向改为已确认的中央纹理绑定瓶颈；不再先开碰撞微候选。资源/ATLAS定向实施仍待准确授权。

## 1. 本轮新闭合的事实
75已确认实际纹理断点，但当时configSource上游是否已裁为本局依赖尚待确认。本轮重新扫描如下调用链，关闭这一未知；不是重做75 GPU/CPU计数诊断。

| 环节 | 本轮源码路径与行号 | 已确认事实 |
|---|---|---|
| 加载入口 | Assets/NTSD/Scripts/UI/LoadingPrewarmController.cs:124–139 | 调用PrewarmConfiguredLoganContentAsync仅传progress，不传本局roster或视觉依赖集合。 |
| 捕获候选 | Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs:129–142 | Capture(source,modeSnapshot)后调用完整LoadLoganContentWithScopeAsync；本段无roster筛选。 |
| 全目录配置与图片 | Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs:321–358 | Read全catalog，Build全部configs；遍历全部wrapper的files/head/small，捕获去重图片。菜单与身体sheet均纳入输入身份，不是本局visual closure。 |
| 配置复制 | Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs:407–411；Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs:1009–1033 | CopyCharacterConfigs重新构建整个Catalog.Entries的配置；失败不允许发布部分配置。本轮明确不存在该入口的roster过滤。 |
| sheet加载 | Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs:1467–1492 | configSource取上述复制全集，复制stagedConfigs并foreach全部Values，收集全部sheet。不能拿“正在显示同一种角色”推定图集输入只有该角色。 |
| 统一atlas输入 | 同文件:1581–1587、1619–1668、2062–2159 | 正文catalog与共同shadow/spark/words一起进入TryBuildUnifiedCentralAtlasPublication；shadow取像素失败时保留SourceTexture2D排除，不能为了合批删除安全回退。 |
| 规划与预算 | 同文件:2919–2946、2958–2974 | 对全部eligibleSources规划，Auto超预算退回SourceTexture2D，未在此调用链传入本局资源过滤。预算守卫本身正确，不应抬预算规避。 |
| 全局发布耦合 | 同文件:406–445 | 同一事务提交逻辑Object publication、菜单UI、TotalCharacterFrameConfig、MergedSprites、SpriteCatalog、CommonVisualCatalog；直接删减stagedConfigs会改变全局配置/UI发布范围，并非纯表现修复。 |
| 消费与容量 | Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs:752–758；Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs:1624–1630 | 中央直接取得manager的两个全局catalog；封印前中央容量取该spriteCatalog.Count。替换绑定集合必须同步容量/身份/lease所有权，不能只在RenderPass换Texture。 |
| 可定位的cold接线 | Assets/NTSD/Scripts/App/AppManager.cs:149–166、187–209；Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs:302–313 | 正式初始化验证contentKey、ApplyMatchConfig、prepare服务/角色、再次校验内容再seal；完整native publication禁止跨active battle boundary。未来应在cold preparation闭合接线，但本报告不宣称现有任一位置已允许新的视觉publication。 |

结论：75所述145页全集规划与当前全目录输入代码相符；上游未按本局roster选定完整视觉依赖。必须保留完整逻辑配置和菜单输入/身份合同，将待修范围限制为战斗视觉绑定集合/其准备接线，不是裁掉全局配置。此新证据排除了“直接过滤CopyCharacterConfigs即可安全降低SetPass”的误修方向。

## 2. 原有运行证据复用，不新增数字
75窗口02实际快照：
artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH75-CENTRAL-SEGMENT-READBACK-20261008/windows-02/00-combat1000-readback/report.json.central-segments.json
SHA256 2F6DAAB62A774DC847ACC8A8A764C11873477BF73F9920AC6056D6A94269DE33。
实际OrderedChunks，2026source/resolved命令、1chunk、2024物理segment；2023相邻断点仅纹理身份，三个source纹理s/saku/SPARK_native，单material/variant/shader。Auto规划145页、2432696320bytes（2320MiB）超536870912bytes（512MiB）回退SourceTexture2D。这是计划atlas大小，不是实测resident/进程峰值。原diagnostic没有commonShadowSource2DRetained标记；不能将潜在非readable回退误写成本窗已发生原因。

77四窗SetPass均未降，Combat平均1995/最大2038/89样本；原REPORT:9–14保留。当前没有新的Unity/GPU capture结果，不把CPU DrawMesh、segment和全帧SetPass互等，也不推定GPU batch组成。外部文章数字继续EXTERNAL_UNVERIFIED。

当前源码仍每个有效segment一CommandBuffer.DrawMesh（BattleRenderFeature.cs:57–86）。segment追加要求同chunk、非Strict、连续quad和IsCompatible（BattleDynamicMeshBackend.cs:258–265、475–486）；array模式已允许不同slice相容，但不同Texture2D source身份不兼容。无需预设必须先GPU Instancing，也不能预设共享绑定后全帧只剩一次draw/个位数SetPass。

## 3. 定向修复的范围与必要缺口
建议优先准备“完整本局视觉依赖 + 预算内共享绑定”，沿用现有中央quad/backend、资源格式、预算守卫和原分段算法。角色/共同shadow/spark需兼容实际绑定身份；对ATLAS bank/资源格式或segment合同的任何变动仍需另批。
不得只选本帧三张纹理；完整集合必须覆盖后续动画、OPoint/技能、变身/生成与选定模式确实可达的视觉资源。此轮只闭合全集来源，不声称已经得到完整闭包、具体页数、支持所有角色或确定能落入512MiB。超大sheet/非readable共同纹理的既有排除仍需保留，不能用局部三texture测试假装生产完整。

拟议后继读/改范围候选（仍未获实施权限）：
- CharacterAnimtorManager：战斗视觉输入选择、既有TryBuildUnifiedCentralAtlasPublication复用、绑定catalog与资源owner的cold准备/退休。
- AppManager.InitializeBattleAsync：准确cold接线与取消/失败保护；不改Scene或其他菜单业务。
- BattleCentralRenderSystem与SimulationTickDriver冷容量入口：仅在确需时适配被批准catalog的取得/容量，保持publication、slot/generation、read lease和11阶段关闭。当前getter直接取全局manager，不能绕过captured catalog身份。
- 现有Editor聚焦测试和BattleOptimizationWindowsAiSuiteEditor：将候选置于真实生产准备链进行同请求对照，而不是harness-only的三源纹理演示；不得提前把测试开关提升为生产默认。
实际实施前需独立Task/Change/逐方法写域/回滚，明确逻辑catalog保全、输入identity、冷准备容量、旧/新资源双预算与在途lease生命周期。以上只给出证据支撑的候选写域，不把未来API/新类或布局冻结为合同。

继续保持：不增预算、不改format/bank/原算法分段语义、不重排透明命令、不跨chunk合并、不GPU自走动画、不instancing/M0、不读Q06排序/first-visible活跃方法体、不改33/3/max2/checksum/RNG、不写Scene/Prefab/原资源/importer/ProjectSettings/Gen/Plugins/Server。不使用全局全集裁剪或强制TextureArray策略绕过缺资源/预算fail-closed。

未来必需验收仅在获批候选后实施：同一1000同角色workload有效A/B，完整视觉依赖无missing/unresolved/stale、checksum及适用authority门保持、容量封印后无增长、完整表现0GC、显示层级/UV/slice/tint/first-visible无回归、旧新资源预算和关闭重进/lease零残留；分别报告logical run/physical segment/CPU draw/全帧SetPass。个位数中央战斗SetPass作为目标而非保证，不能从局部计数推全帧或120FPS达标。

## 4. 本包验收与留痕
Operation与Task均为NTSD-OPTIMIZATION-BATCH78-SETPASS-RESOURCE-SCOPE-20261008；before-manifest含四dirty文档精确副本和九source SHA、引用77原604guard manifest。仅四文档顶部追加、创建本报告/Task/Operation/audit；无新脚本Change/无Ledger实现项。源指纹如下：
- Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs — 35AC697CB8B5A43BC9AE76413F59DD3498218B6C188A06A02E00E99B01D99F9F
- Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs — 235BB5E74EB599CA63C016C3EED8FC2B714C1ED28E0531E4D9FF9EE4AF513976
- Assets/NTSD/Scripts/UI/LoadingPrewarmController.cs — 057FD4D65B9317110ED5BCF356F92BBEB7A5F9217E09C715E95A036CF20CB32B
- Assets/NTSD/Scripts/App/AppManager.cs — AFE95938388DF429D1368E6121E901C02D32DF4709D1E353D63E165660B459B9
- Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs — 3DD59431EE5516FA15449CE4364C272CDD5741D5857E374C085397EBFD3AEF61
- Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs — 247BD7E73DFA8D4F004968E0E7E0D93917E28EB39FE8E26D6A361602D0CB118D
- Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs — F5519E8F5AD9634244C5D1DC1E67A6B3415054FAF8F9EAFAC9C694E096E52292
- Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs — F1649C0F78F1B897FF0E7846F3F5E7FBEF63B3283E1A3BC1E95F6B3AEB99A58B
- Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs — 6FE1054C0FE8F02223B706E218D559E90C9215B35001EC274A733591DA8470E7
最终备份/尾部原文/604guard/九source/HEAD/staged/diff证据记录在final-audit-01.json；最终实际结果在Operation追加。本包没有Editor状态新检查，77末Menu clean仅旧证据，不冒充当前live idle。没有新的编译/test/SelfCheck/Play/性能/Android证书。

