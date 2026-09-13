# Q02 E2：同源候选与原子发布

状态 IN_PROGRESS / CANDIDATE_FOCUSED_PASS / ATOMIC_PUBLISH_PENDING。前置 NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001 已完成目录与完整配置候选门槛验证（Unity39/39、native330/330）。父Task为 NTSD28-B11-CATALOG-PUBLICATION-CONTRACT-001；E3缓存/caller和Q07正式资源迁移仍是后继。

## 先读现状，再冻结准确写范围

继续从 CharacterAnimtorManager.ApplyLoadedCharacterConfigs、BeginSpritePrewarmInvocation、LoadCharacterSpritesFromSourceAsync、TryCommitSpritePrewarmInvocation、LoadAllCharacterUISpritesAsync、资源退休/OnDestroy，以及GameDataManager和CharacterUIResourceManager的真实读写方追踪。保持现有框架与所有非战斗操作。

现有已确认问题：TryCommitSpritePrewarmInvocation先发布config/sprite并设IsPrewarmCompleted=true，之后await加载UI头像；GameDataManager的object/type lookup独立；头像dictionary以id为key且双null保留旧值；newsource裸参数可与旧pending config搭配。这些不能作为新source正式事务。

## 必须完整处理的合同

- 一次候选绑定LoganObjectCatalog及其一次成功构建的配置字典，禁止裸source搭配任意旧配置。registry_index不能在转换成旧ObjectDefinition后丢失或被Dictionary位置冒充。
- E1 DefinitionFingerprint/SourceCacheKey只覆盖对象DAT/catalog，**不覆盖图片**。正式prewarm缓存身份还必须覆盖实际sheet/head/small资源身份；不能让只改PNG但DAT不变时命中旧图片。阶段不同的fingerprint需命名清楚，不能冒称全资源内容校验。
- config、object/type lookup、SpriteCatalog、头像/小图应先全部staging，再进入一个无await且准备好所有分配/失败检查的发布段；已准备世代与实际发布世代一致才设完成并发事件。声音定义引用要跟随同源配置，WAV整体迁移不自动加入本包。
- 中途失败/取消/重复预热不污染当前发布，不保留同ID的旧头像；旧图资源按真实owner/引用释放，不用全局ClearCache或批量Destroy掩盖残留。
- 禁止active battle中切换定义来源。先识别真实lifecycle owner/World Running条件，不能只根据场景名、Renderer数量或单例存在推断。
- 背景、stage、普通UI、美术例外、SPARK等不随“对象来源”无条件切换。明确GameDataManager背景lookup和Q08/Q09职责，不以新对象目录已发布宣称所有辅助目录也切换。
- 新增staging资源/队列若确有必要，必须接既有十一阶段关闭合同；停止接单/worker join/任务丢弃/资源回收/postcondition有focused与真实退出重进证据。不得重排顶层关闭，不新增第二套manager框架。
- 保持旧公开入口和Editor夹具语义。菜单选择/随机/排序/UI表现不改；E3只做必要来源/完成gate适配。无法保证范围时明确指出受影响部分，不能借迁移扩大功能修改。

## 写前与验证

本Task只冻结依赖与硬边界，不能替代准确Change Record。开始脚本前补齐实际路径/符号、发布前后读者、不可回退点、异常/取消/关闭和回滚方式。

E2可用隔离合法目录验证成功、失败、取消、相同ID跨source、图片单独变化与资源归属，不等待Q03修完正式Converter；但正式全量构建当前仍拒绝6个DAT，不能绕过以部署。E2完成后接E3缓存/生产入口，Q02整体出口不缩小；正式全场/图片完整验收继续Q07/Q09/Q12。

## 当前源码审计补记与实施顺序

1. Driver明确owner：SimulationTickDriver.Instance只取既有实例，不创建；LifecycleState及World.ObjectCount/封闭data catalog决定是否允许新source，Running/Stopping禁止，Preparing已有实体或sealed也禁止。AppManager.State是附加入口gate，不可代替driver。
2. 既有发布：TryCommitSpritePrewarmInvocation会先回收旧publication资源再换config/sprite并置Ready；native事务必须预分配退休记录、无await换全组、更新界面绑定后才回收旧native资源。不能原样复用其提前退休时点。
3. UI是uGUI：SelectRoleItem.UpdateCharacterDisplay只在选择动作时赋Image.sprite，双null不清旧图；OnEnable/Disable绑定输入，Update仅闪烁。必须最小资源绑定通知或有证明的无live UI gate，不能只清dictionary后Destroy旧Sprite。已读unity:ui/ui-ugui技能；UI布局/选择输入流程不改。
4. 停止：新source staging不能在Stopping生成Unity资源；先审查raw IO/CPU job与Unity资源owner，再在准确Record中声明取消/丢弃/主线程回收。原simulation dedicated worker的Join门槛不可改，不能以OnDestroy异步迟到替代清理。

当前先写候选输入绑定（同一个E2 Change，不另将E2缩小）：LoganVisualContentCandidate内部只从LoganObjectCatalog及完整builder构造，捕获sheet/head/small哈希、区分Definition与Visual fingerprint；BMPLoader明确verified入口在实际读取的字节上核验expected hash再解码，旧LoadBmpData入口保持。测试覆盖PNG单独变化、DAT变化、不同root和hash不符；这不是原子发布完成。
后续仍在本Task继续：candidate→实际staging、停止/世代、prepared object/UI view、无await提交、UI绑定/退休、真实退出重进，E3接cache/caller。每段新脚本先扩充同ID准确Record，已写代码证据不等于整包VERIFIED。

## 首段恢复游标

候选输入绑定和verified decoder已写且Unity20/20（7+13）通过，CS0/Scene dirtyfalse。继续同一E2任务，不新建替代目标；详细未实现项及源码事实见当前Change Record与CANDIDATE-INPUT-REPORT.md。下一先冻结native staging停止/世代和对象/UI视图的精确写范围，随后接无await提交/资源重绑与回收。新source global仍未切换，E2不得标VERIFIED。

第二段已在同ID Record写前声明manager/GameData/UI资源视图/SelectRole资源重绑/Driver阶段1与5/test六脚本；立即按该范围进行test-first实现。首段已验证保持，整体E2不缩小。

Accessor更正（第二段实际编译发现）：Driver继承SingletonBehaviour<SimulationTickDriver>，实际无创建读取是Instance属性；MMSingleton的TryGetInstance仅用于CharacterAnimtorManager/GameDataManager等。先前首段审计将两类基类混淆，现已按Driver类声明及SingletonBehaviour属性实现纠正；不改变首段20/20的输入绑定证据，但该证据原本不覆盖Driver gate。

## 最新恢复出口

实际原子发布第二段已写，fresh focused27/27，两次真实Play各4项/113资源零残留；CS0、Scene dirtyfalse/root14、Ledger469/31PASS。整包RUNTIME_PENDING。先执行R16回访NTSD28-PREPARING-SHUTDOWN-OWNER-CAPTURE-001（Task/Record已PLANNED，生产未改），修复真实Preparing退出缺owner并验证过期启动续体失效，再回本E2验出口。不要重做candidate/staging，也不要把E3/Q03/Q07未做项遗漏。

## R16返回后的当前出口

Preparing服务owner回访已VERIFIED；完整SelfCheck PASS、关联focused26/26、Preparing2/Running2/App1实际Play共5次，每次本E2四项4/4及113资源零残留。E2现VERIFIED_SOURCE_ATOMIC_TRANSACTION_GATES_ONLY，原pending为历史。下一docs/ai/TASKS/NTSD28-B11-SOURCE-CACHE-CALLER-CONTRACT-AUDIT-001.md / READY_AUDIT。Q02/正式内容/总目标均未完成。
