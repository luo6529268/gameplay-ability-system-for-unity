# E2首段：候选输入绑定与校验解码

Change状态FOCUSED_TEST_PASS；**E2整体仍IN_PROGRESS，原子发布未实现**。本段没有发布新source或迁移资源。

## 已实现

- LoganVisualContentCandidate.Capture自行读取LoganObjectCatalog并调用完整配置builder，不接受外部任意配置字典；目录、配置与图片输入来自同一个source。
- 角色sheet、head、small图片的实际字节hash全部进入只读manifest；VisualFingerprint绑定DefinitionFingerprint和图片路径/hash，SourceCacheKey再绑定来源根。背景、普通UI、SPARK、WAV未被伪称覆盖。
- AssertInputsCurrent在加载期重核catalog/DAT和图片输入，发生变化就拒绝旧候选。元数据只读；内部LF2定义值仍沿用现有可变数据类，不宣称深度immutable。
- BMPLoader.LoadVerifiedImageData在**同一次读取所得byte[]**上验证预期SHA-256，再让原解码路径消费这些字节。旧LoadBmpData仍以null expected hash进入同一Core，原线程/格式/错误行为保持。

捕获manifest不等于图片已解码、SpriteCatalog已生成或global ready。CopyCharacterConfigs/GetImageSha256供E2后续内部staging使用，尚未接进现有全局预热。

## 实际验证

RED job `51f00fedbf594e3bbc79a03a6e931aa9`：7项全部因新API缺失失败。GREEN job `ba66c3acc5964dd39be65a5ffe3675bc`：**20/20 PASS**（本段7、既有PNG worker13）。原始结果为candidate-red-result.json/candidate-result.json。

覆盖：sheet/head/small完整输入、只改有效PNG时Definition不变而Visual身份变化、DAT变更使旧候选失效、不同root不共享source key、缺图拒绝捕获、实际解码bytes哈希一致及不一致门槛、旧PNG/BMP路径回归。正式候选仍正确报告6个DAT转换失败；该负向门槛不等于Q03六项已修复。

Unity编译CS error0，NTSD_Battle dirtyfalse/rootCount14；初始3059保护文件中3056原样，3个仅累计已声明变化，无缺失。首段前后manager、GameDataManager、CharacterUIResourceManager、SelectRoleItem、SimulationTickDriver均hash不变；本段既有脚本只改BMPLoader，另新增candidate/tests。Ledger468/25PASS，git diff --check通过（仅既有换行转换提示）。没有完整SelfCheck、整场Play或原子发布/关闭运行时证据。

实际命令：既有Temp/Goal13_bridge.py的refresh_unity、run_tests(EditMode，NTSD.Test.NTSD28B11VisualContentCandidateEditorTests和NTSD.Test.Editor.NTSD28B11PngWorkerDecodeEditorTests)、get_test_job、read_console(error CS)、manage_scene(get_active)；Python hashlib保护核对；Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path。

## 当前审计事实：后续不能遗漏

1. SimulationTickDriver.Instance无创建副作用；LifecycleState是实际生命周期入口，World.ObjectCount/封闭的RuntimeDataCatalog可确认已绑定战斗数据。Running/Stopping禁止切源，不能只查场景名/是否有Renderer。
2. TryCommitSpritePrewarmInvocation当前会先尝试退休旧资源，再替换config/sprite并置IsPrewarmCompleted；LoadAllCharacterUISpritesAsync发生在其后。native事务必须改变这条新source路径的准备/提交/退休边界，不能把旧方法名当原子性证明。
3. uGUI SelectRoleItem只在选择动作调用UpdateCharacterDisplay，Update仅闪烁；Image.sprite可持续引用旧头像。更新CharacterUIResourceManager字典后直接销毁旧Sprite会影响已绑定头像。必须处理资源引用重绑/retirement边界，不改变状态机、输入、随机、排序或布局。已读取unity:ui与ui-ugui技能，实际UI脚本未改。
4. async prewarm的raw IO/CPU与Unity资源需区分真实owner；Stopping后不能再创建Unity资源或发布，OnDestroy迟到回调不能代替清理。未来manager/driver/UI脚本动手前，必须把精确停止/取消/丢弃/回收阶段补入当前Change Record；不改变既定十一阶段和dedicated worker Join硬门槛。

下一步仍执行同一个 `NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001`：候选接入实际staging，准备object/UI视图，冻结停止和失效世代，再实现无await提交、头像重绑与旧资源退休，完成focused和真实退出重进。E3缓存/caller以及Q07正式迁移保持后继。不得把本段20/20升级为E2完成。

Accessor更正（第二段实际编译发现）：Driver继承SingletonBehaviour<SimulationTickDriver>，实际无创建读取是Instance属性；MMSingleton的TryGetInstance仅用于CharacterAnimtorManager/GameDataManager等。先前首段审计将两类基类混淆，现已按Driver类声明及SingletonBehaviour属性实现纠正；不改变首段20/20的输入绑定证据，但该证据原本不覆盖Driver gate。
