<!-- CHANGE-RECORD
id: NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001
status: VERIFIED
change-kind: SAME_SOURCE_VISUAL_CANDIDATE_AND_ATOMIC_PUBLICATION
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Animation/Runtime/BMPLoader.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11VisualContentCandidateEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Animation/GameDataManager.cs
code-path: Assets/NTSD/Scripts/UI/CharacterUIResourceManager.cs
code-path: Assets/NTSD/Scripts/UI/SelectRoleItem.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11AtomicPublicationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11NativePublicationPlayProbeEditor.cs
authority: User active goal/D-023/non-battle invariants; NTSD28 formal runtime catalog/DAT/PNG; parent Q02 catalog publication E2 contract; existing ordered shutdown contract.
evidence: VERIFIED_SOURCE_ATOMIC_TRANSACTION_GATES_ONLY / FOCUSED_27_OF_27 / RELATED_26_OF_26 / FULL_SELFCHECK_PASS / PREPARING_OWNER_REVISIT_VERIFIED / REAL_PLAY_5_EACH_4_CHECKS_113_RESOURCES_ZERO / CS_0 / SCENE_CLEAN / E3_AND_FORMAL_CONTENT_PENDING
-->

# E2 同源候选与原子发布

状态IN_PROGRESS。本Record覆盖整个E2，当前首段准确三脚本；后续触及manager/GameData/UI/关闭hook等之前扩充精确path/symbol，禁止凭计划文件名推断已获任意改写授权。父目标范围和E2全部出口保持，不因首段容易测试而缩小。

原状：E1目录fingerprint只有DAT/catalog；newsource sheet入口仍可裸source搭配旧pending配置，图片在读取期间变化也未校验预期hash。当前首段新增不可变LoganVisualContentCandidate，factory自行Read目录+全配置构建，private config map与只读image identity；只包含角色sheet/head/small，不伪装包含背景/WAV。VisualFingerprint含定义指纹及规范化图片path/hash，SourceKey另绑定root。配置拷贝map供后续内部staging，不能让调用方注入任意字典；配置值沿用项目既有LF2数据类，不宣称深度immutable。

BMPLoader新增LoadVerifiedImageData(path,expectedSha256)，复用private LoadDataCore；读取同一byte[]后hash必须相等才decode，防止验证和消费分开读取产生混用。旧LoadBmpData(path)仍无hash强制且原错误/线程行为保持。未改全局source/菜单/场景/内容资源。

候选factory/AssertInputsCurrent只在加载期使用，文件流using释放，managed配置/metadata没有长期World/renderer/worker owner。未来async staging必须另补停止接单、raw worker discard与Unity资源回收阶段；本首段无新增异步任务或Unity资源生产，schema12/20/23不动。

最低首段测试：root隔离、只改PNG的Visual fingerprint变化而Definition不变、head/small纳入manifest、捕获后DAT/image漂移拒绝、精确bytes hash门槛与旧decoder回归、真实Unitycompile/scene/保护。正式全候选仍须拒绝Q03六文件，不绕过，E2原子发布/生命周期/真实Play证据后续补齐。回滚仅本段精确diff，保留用户和Q01/Q02工作，删除按既有规则。

## 当前验证

Task/Record先于代码；RED/GREEN待执行。E2总状态不得升为VERIFIED，直到全部发布/回收出口有证据。

首段写后：新增LoganVisualContentCandidate.Capture/AssertInputsCurrent、private configs和image hash manifest、VisualFingerprint/SourceCacheKey、内部CopyCharacterConfigs/GetImageSha256；BMPLoader.LoadVerifiedImageData/LoadDataCore已写，旧入口调用null expected hash。RED job51f00fedbf594e3bbc79a03a6e931aa9 7项全部因缺新API失败。首段GREEN待执行；原子发布/实际staging接线/停止/回收/UI绑定均未写，整包仍IN_PROGRESS语义，不得宣称E2已完成。

首段验证：jobba66c3acc5964dd39be65a5ffe3675bc真实Unity20/20（7+13），CS0/Scene dirtyfalse/首段下游manager和Data/UI/driver hash不变/原3059中3056不变、其余3累计声明变化/零缺失；Ledger468/25PASS。详细命令、边界、已确认UI绑定与旧提前退休问题见artifacts/diagnostics/NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001/CANDIDATE-INPUT-REPORT.md。E2整体IN_PROGRESS；下一继续同一Task/Change的实际staging、停止/世代、prepared object/UI、无await提交、绑定/退休与真实退出重进；必须在下一段脚本前追加准确code-path。不能以本段focused门槛关闭E2。

## 第二段写前冻结（2026-09-13）

新增声明六脚本。manager保留legacy公开/内部签名，裸非null source入口停止使用并要求candidate；增加LoadLoganContentForOwnersAsync及生产包装，Core从candidate自身取配置，不让外部混搭。NativePrewarmOperation嵌套加载事务持有既有staging容器；未发布Sprite/Texture/atlas资源归其所有，提交后转移到既有publishedOwned集合。generation/取消和Driver+App实际生命周期gate在开始、排队、materialize及commit检查。新source不允许Running/Stopping/已有对象/已seal，以及App battle或选择角色阶段；旧入口保持原语义。

ProcessAndCreateSpritesAsync旧静态签名保留，共享新内部producer core；native传candidate image hash和materialize gate，解码使用LoadVerifiedImageData。头像新路径用同hash读取和原raw->Texture/Sprite处理，不全局更改UI去黑/混合算法。所有新Unity对象创建后立即登记owner，取消后禁止再创建。Native UI/配置与源fingerprint必须在提交前完成；body/common/atlas沿原builder，无新的渲染框架或美术重构。

GameDataManager新增PreparedObjectPublication（预建config/object/type/index lookup，保留既有background引用与lookup），Commit只换引用；objectsByTypeLookup取消readonly以支持预建字典原子替换，legacy算法保持。准确registry_index另存，不用list位置冒充。CharacterUIResourceManager只新增native整批引用交换与source key，空头像不保留旧条目；旧Set/Clear保持既有功能并使native标记失效。

SelectRoleItem仅新增内部PrepareNativeResourceRebind，生成在commit前已准备的Action：随机项不改，active角色更新资源图/名称；Idle只在Image仍引用旧head时替换图片，不改Idle文本/倒计时图片。既有state/input/导航/排序/OnEnable/OnDisable/Layout代码全部不改。manager在commit前找现有views并生成动作，交换完所有lookup后重绑，最后Ready/event与旧资源退休；无新全局event/subscription。

Driver.ShutdownBattleRuntime仅阶段1调用既有manager的CancelNativeContentPrewarm（无单例创建、仅关入口/世代），阶段5在dedicated worker Join之后ReleaseCancelledNativeContentStaging；OnDestroy同一幂等fallback。raw IO/CPU任务只消费私有bytes/Color数组，不读写World/pool/Unity对象；取消撤销后续materialize许可，其迟到结果丢弃。不会在主线程阻塞等待需要Unity continuation的线程池任务，不改变dedicated simulation worker Join硬门槛。停止期间未发布Unity资源于阶段5同步回收，已发布资源仍按原catalog lease/manager owner退休。失败/取消完整保留旧发布；后继异步finally不得销毁已提交新资源。

测试准确范围：实际native Core/full body-common-atlas-UI staging，完整key/object/config/UI一致，缺图/漂移/取消/世代拒绝、同ID换源与无head清旧引用、原资源在重绑后释放、legacy签名与source入口拒绝、真实生命周期gate/停止hook。测试仅自有inactive managers/临时GameConfig和shadow descriptor/临时UI图像，保存恢复静态配置；不改变用户Scene/Prefab/材质资产。实际Play退出重进及零残留仍为本E2必须出口，focused不自动升级VERIFIED。

回滚仍限本Change精确diff，保留前段candidate和其他已验证子包。第二段原子发布/回收测试先RED再实现，E3缓存caller/正式Q07切换不在这次调用方接线里偷偷完成。

第二段写后：manager嵌套NativePrewarmOperation、LoadLoganContentAsync/ForOwners/Core、UI staging/verified producer、TryCommitNativePublication/CanComplete gate/Cancel/Release/OnDestroy已写；GameData预建object/type/index视图、UI整批视图、SelectRoleItem仅资源重绑Action、Driver阶段1/5两个hook已写。旧ProcessAndCreateSpritesAsync及LoadCharacterSpritesAsync签名保留；原子事务借用shadow不转ownership，新增UI资源归同一publication。第二段RED job786ce3dfa2214b1195470370a813d04f实际10项缺入口失败；当前编译/focused/runtime验证待执行。

编译纠正：第二段首个compile出现CS0117，Driver实际继承SingletonBehaviour，不能调用MMSingleton.TryGetInstance；已改成无创建的SimulationTickDriver.Instance。c55c86a3c7c4401c94aed8d63a3b1a03运行的是未更新旧程序集（10项仍缺API），不是新实现GREEN证据。Task/首段报告中的该审计事实已更正并保留纠正说明；再次编译和运行验证待执行。

## 第二段所有权补正（写前）

复核确认旧LoadBMPAsSpriteAsync唯一为LoadAllCharacterUISpritesAsync创建动态头像/小图，未进入publishedOwned集合；native切源若只换UI字典会丢失旧资源owner。当前同一manager脚本新增legacyOwnedUiSprites/Resources及head→id元数据，私有UI loader在创建时登记、捕获generation并在stale/disposed时禁止上传，正常旧UI显示处理不变。LoadAll只登记head id；不在普通legacy替换时提前销毁旧头像。native commit预分配的上一publication ownership合并这些确实由loader创建的资源；重绑后才退休，再清tracker。OnDestroy幂等回收剩余owned UI。外部SetCharacterUISprites传入的借用Sprite不凭缓存内容反推ownership，导入素材/RandomIcon不销毁。

SelectRoleItem预计算重绑时接收旧owned-head元数据；Idle只替换仍指向该角色旧owned/current head的Image，保留倒计时/Idle文本。非全局测试owner仅处理能证明属于该owner的图片引用，不改其他场景UI；生产全局owner覆盖其角色资源显示。修改前已扩充此精确symbol合同，新增测试验证旧动态头像handoff及借用shadow保留。此为E2原子资源交接必要收口，不扩展菜单输入功能。

## Play验证脚本写前范围

新增Editor-only NTSD28B11NativePublicationPlayProbeEditor.cs，仅消费本任务Temp request（改requested=false，不删文件），自动进入/退出Play并输出本包artifact。先观察真实NTSD_Battle driver有活动World，验证public native入口拒绝active battle；随后用App已有ordered shutdown/UnloadBattle（必要时仅在Play创建临时host Scene以允许卸载唯一battle Scene），或无App时用Driver+BattleBootstrap已存在的关闭接口完成真实world停止，不伪写生命周期字段。

停止真实world后复用已编译的AtomicPublication NUnit夹具，在真实Play执行完整预热换源/取消重试/Driver关闭hook/legacy owned UI交接四条；每条保存恢复测试临时GameConfig/单例，收集实际owned Unity资源引用并验证TearDown后全灭。两次独立Play进入/退出重进，各自runId和报告；结束退出Play，原Editor Scene/资源资产不保存不修改。脚本不改生产规则/配置/输入，不创建新Codex任务。此证据明确是隔离合法候选的真实Play资源事务，不是全正式DAT/整场战斗对齐证书。

实际focused第二段job2fcb44c427944c4498b16645a539cca9为27/27（atomic14/candidate7/alpha6），详见atomic-result-v4.json。真实Play Running路径已关闭至RuntimeMapCleared且World/slots/borrower为0，但play-running-cycle-2在首条fixture teardown发现24/34资源残留，尚不通过。写前补正测试SetUp：仅临时激活/停用自有manager一次，使其经历Unity Awake/OnDestroy生命周期，激活期间保存恢复MMSingleton静态owner；仍由实际DestroyImmediate(GameObject)触发生产OnDestroy，不手工代替生产回收，不修改生产生命周期。Preparing路径另发现实际cached owner缺失，已建NTSD28-PREPARING-SHUTDOWN-OWNER-CAPTURE-001待审计Task，E2不能因Running通过而忽略该项。

## 第二段当前验收与恢复出口

生命周期夹具纠正后真实Play cycle3及cycle4各4/4，登记113资源各全部销毁；active battle gate拒绝、实际Running World4关闭至RuntimeMapCleared且World/slots/borrower0。fresh focused jobd088c453b2184143a8b00aad32a94e45（atomic-result-v5.json）27/27，Unity脚本编译错误0，NTSD_Battle dirtyfalse/root14，Ledger469/31PASS，保护3059中3052不变/7声明既有脚本变化/零缺失。失败报告全部保留：旧程序集CS0117、UI preview夹具、selfcheck common前置、Play NUnit上下文、未激活manager生命周期，均不得删除当作从未失败。

本E2仍RUNTIME_PENDING：真实Preparing的owner缺口尚未修复，下一先执行已PLANNED的NTSD28-PREPARING-SHUTDOWN-OWNER-CAPTURE-001 Task/Record及其focused RED。正式全量6DAT Converter失败仍Q03，E3 cache/caller及Q07迁移未执行。完整证据和实际命令见ATOMIC-PUBLICATION-REPORT.md。不能标E2/B11/总目标完整完成。

## R16回访返回后的限定关闭

NTSD28-PREPARING-SHUTDOWN-OWNER-CAPTURE-001已VERIFIED：新增owner/世代guard、focused26/26、完整SelfCheck PASS、Preparing2/Running2/App1共5次真实Play；每轮本E2四项4/4及113资源零残留、实际pool/factory0残留、关闭后两帧仍Stopped。至此本E2同源候选/原子发布/取消/重绑退休出口VERIFIED_SOURCE_ATOMIC_TRANSACTION_GATES_ONLY；前面的RUNTIME_PENDING是保留的历史状态，现以本条返回覆盖。

不得升级为Q02/B11/总目标完成；正式6DAT Converter、E3 cache/caller和Q07部署仍未做。下一Task NTSD28-B11-SOURCE-CACHE-CALLER-CONTRACT-AUDIT-001 / READY_AUDIT。
