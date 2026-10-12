# M12 Android-first 解码／上传字节背压：有限实施候选

Task: NTSD-M12-ANDROID-DECODE-UPLOAD-BACKPRESSURE-20261011
Change: NTSD-M12-ANDROID-DECODE-UPLOAD-BACKPRESSURE-001
状态：PRE_FLIGHT_READY_FOR_REVIEW / PLANNED。M12继续开放；本文件不授脚本或Unity运行权限。只供主会话独立审阅后在既有人类启动授权下派发同一Task，不另开专项。

## 事实、必要性与能力边界

当前manager SHA 6990b2534ca6c832ab50c5fd692e3275ca335b2e2c77c1502bcc85eaa394ea34。CPU并发clamp(processorCount,1,4)，上传SemaphoreSlim(1)。LoadCharacterSpritesCoreAsync在CPU名额上等待；两个body worker解码／Prepare／plan后先Release CPU、再等待upload，因此已完成数组不受CPU并发数上界限制。CreateBodySheetTexture把processed数组放入stagedAtlasSources；clamped cell也追加独立数组；WhenAll之后才调用统一atlas。因此只控任务数不够，retained不得持同一额度等到WhenAll。

本地本轮重读旧inventory并对703条PNG做33字节header/长度核验：尺寸0差，总压缩28,102,250B，330定义/773声明/703唯一路径。完整源SHA0漂移复用主审派发的独立检查，未在本轮重复全文件SHA审计。inventory SHA c6ab501ea8b79481ab8afc83ba7ee2a9e72fe5066a9f4b6d9b8c1e5f9afc7154。逐条计算见static-model.json。

PNG路径依次有rgba byte[]、Color[]、Color32[]，分别4/16/4B/px。decoder使用IDAT MemoryStream+ToArray和两行buffer；MaxPixels=32*1024*1024，CRC/Adler/格式检查仍由既有decoder裁决。BodySheetSpritePlan不持BmpData；但两个worker目前跨upload等待仍使用或保留BmpData引用。可在所有plan完成后复制width/height、置本worker私有bmpData=null；不修改公开BmpData，不动alpha/转换规则。释放引用不代表发生GC或进程内存下降。

最大win2.png为2001×8768，两种像素数组alone350,895,360B；Kirin为10891×1559。旧基本模型W0=24*px+4*fileBytes+8*width+65536，对64/128/256MiB分别有34/8/2条超限。新窄读发现HashNativeClampedCellPixels还分配8+4*cellPx的byte[]，同时存在Color32[cellPx]；候选保守权重W=W0+8*maxDeclaredCellPx+8，每group取成员最大cell，OFF取本声明。即使实际不clamp仍保守预留。W最大439,799,644B（约419.425MiB）；>128MiB为9条，不能沿用旧W0的8条。此为静态调度权重，不含所有对象、Rects/字典、nativeTexture/atlas/未回收垃圾的完整内存上界。

全703源Color32 payload加总2,003,918,132B约1.866GiB，只是理论全留存口径，不是实测整局。M12能限制同一owner的解码／待上传并发叠加和浮点数组引用时长；不能单独保证手机能加载当前最大资源或当前全量atlas，不能通过改名账本声称总峰值下降。H08共享已经正常默认ON，复用其资源身份合同；不重做共享、不裁图、不删catalog、不改规则。

## 决定1：准确最小代码域与获取顺序（建议）

仅两个生产脚本、一份新Editor测试；现有小fixture最多只添加参数化入口，不改旧断言与旧输出。
| 路径 | 拟修改符号/职责 |
|---|---|
| Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs | NativePrewarmOperation；LoadLoganContentWithScopeAsync；LoadCharacterSpritesCoreAsync的两个native任务入口；ProcessNativeBodySheetGroupAsync；ProcessAndCreateSpritesForCandidateAsync；CancelNativeContentPrewarm；准确finally收尾。嵌套NativeDecodeAdmission/Reservation与冷配置快照，不新建全局框架 |
| Assets/NTSD/Scripts/Animation/Runtime/BMPLoader.cs | 新internal VerifiedImageLayout/ReadVerifiedImageLayout/LoadVerifiedImageData受限内部overload；LoadDataCore仅internal受限分支，现有public方法合同及manual decoder不改 |
| Assets/NTSD/Scripts/Test/Editor/M12DecodeUploadBackpressureEditorTests.cs（及Unity生成meta） | 新字节门、真实小PNG/慢upload、取消/失败/迟到、唯一候选比较；复用现有fixture接口 |
| Assets/NTSD/Scripts/Test/Editor/NTSD28B11AtomicPublicationEditorTests.cs | 仅新增小fixture配置/只读快照及受控decode/upload等待入口（冷诊断默认null），既有ON/OFF像素与shutdown断言原样调用 |
| Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCacheCallerEditorTests.cs | 仅如需要新增M12冷配置入口以复用真实public配置三代owner用例；旧用例/共享cache合同不重写 |

PngPixelDecoder.cs只读，不修改算法/常量。PrepareBattleSheetPixels、PrepareBodySheetSpritePlan、CreateBodySheetTexture/CreateBodySheetSprites算法保持；如需收集实际数组bytes，仅围绕调用和已有stagedAtlasSources.Count增量计数，不新增全局引用或改变clamp去重。BMPLoader保持原公开入口；native PNG路径才使用新内部guarded overload。native共享ON与显式OFF均逐实际decode收费；legacy以及非PNG原路保持，报告为不受本限定byte门覆盖；当前703路径全PNG不能外推未来其他格式。普通UI/common atlas加载不在本项，禁止声称全加载峰值受控。

获取顺序：
1. 既有Candidate.AssertInputsCurrent后，单个调度者按原分组／声明顺序，在worker用固定64KiB buffer流式核文件SHA、长度与头部，生成本次只读descriptor。只读header不是完整decoder资格；decoder继续检查所有格式/CRC/Adler。描述不持fileBytes或像素；无需一次预建无限任务或全量raw数组。
2. 在主线程检查owner/generation有效后await byte reservation；等待时不持CPU或upload。本producer仅一个待准入请求，天然FIFO；不能让后方小项越过大项，取消移除唯一waiter。事件驱动UniTaskCompletionSource唤醒，不轮询、不持锁await。
3. 取得byte额度后再await CPU；失败/取消必须finally归还reservation（不启动decode）。先await旧operation的drained完成再为新generation发body任务，避免同manager旧新两个独立额度同时解码；不通过跨manager全局cache/锁实现。
4. worker的内部guarded loader重新开流，分配raw数组前核长度等于descriptor、精确长度读取，随后SHA及header/尺寸再次匹配descriptor才解码。文件变长/变短/等长替换必须失败；不要在原File.ReadAllBytes之后才检查体积。raw读取不得溢出预估、不得在队列等待期间持raw数组。full hash坚持验证实际解码字节，不以先前header代替。
5. 完成pixels和所有plan后复制标量，清本worker的BmpData强引用；释放CPU；仍持reservation等待upload。await upload后先到main并检查世代，再执行原Texture/Sprite方法。
6. uploaded arrays归原stagedAtlasSources，分别记retained source与clamp的实际4*length；完成本worker上传/本地像素引用移交后release upload并exact-once归还W。W不等atlas消费，retained永不参与可返回额度等待。
7. 调度／worker任何失败先关闭本operation admission、唤醒waiter并标invalid；仍await已启动全部tasks的drain，后finally回收本operation。此finally须包住调度循环本身，防header/admission失败跳过WhenAll。原previous publication保持、禁止部分commit。

reservation所有权用reference token或有唯一状态的lease，不允许可复制struct导致双归还；主线程短状态更新不跨await/不跨worker写Unity数据。未启动、等CPU、等upload、异常、取消、成功都只能release一次；负数／超上界是硬失败。completed tasks原清单仍有限于实际声明数，不因本项另扩Task列表；gate waiter容量1、活跃lease最多CPU4+upload1+等待CPU1，但字节门比计数优先，不能用这个6推导数组内存。

## 决定2：手机导向参数与超大资源（建议，不是已批准值）

推荐普通并发权重C=134,217,728B（128MiB），沿用原CPU上限4、upload1，不把开发机核数当手机预算。选择理由是将>=64MiB工作限制到至多两个同窗，>=C/2的资源不能继续累积；相较64MiB对当前多张中型图全面串行的代价更小，最终成本必须固定对照验证。C只限制调度权重；没有称128MiB是真机全进程可用额度。

推荐两级策略：W<=C正常FIFO；C<W<=E只在当前reserved=0时独占，独占时不放入其他decode，E固定439,799,644B取当前可达body+最大声明cell核算上界，不能按新文件自动抬高。W>E在任何像素解码前明确拒绝整个候选publication，旧publication保持；不得skip必要sheet或静默绕门。这保留当前大源的可执行路线，同时不把约419MiB例外伪称手机安全预算。固定E为当前内容兼容上界，并非RAM/device预算；输入／cell模型变化须重审而不是提升上限刷PASS。

备选“所有W>C直接拒绝”较严格，但按当前模型会阻断9个已在正式目录的body资源，会改变当前可加载内容覆盖；不建议在普通生产入口直接采用。若主审决定手机路线必须绝不超过128MiB，则本M12只能候选OFF留方案，另由用户决定大源处理；本轮不裁资源或扩大解码器/atlas工程。

candidate cold开关默认OFF，snapshot在operation开始冻结，活动中拒绝切换。先按下面有限门取得证据；正常默认推广是否包含在下一实施派发由主会话明确，不能依据本预检自动打开。H08 reuse=true及其显式OFF独立不变。设备机型未知不是源码准备阻塞；PC320MiB音频、2/3GiB进程门绝不挪用。

预算口径应并列输出：reservation权重/实际decoder与queued数组强引用payload/retained source+clamp/原nativeTexture及atlas/其他系统与运行时余量。后三项尚无本轮整局上界，不填伪造数值。

## 决定3：生命周期与有限门（建议）

新增状态只属于manager当前NativePrewarmOperation与body任务：cold descriptor不缓存跨世代；一个gate、一waiter、有限leases、fixed counters/peak；失效为CancelNativeContentPrewarm、generation变化、disposed、CanContinue失效、输入hash失败、worker异常。冷diagnostic hook只归fixture且默认null；不会进入battle tick，不要求冷加载零分配，也不豁免战斗热路径0GC。

阶段1：原CancelConfigured/CancelNative链同时停止新byte admission并取消等待，旧世代禁止materialize/publication。阶段2：dedicated simulation worker的既有Stop/Join合同不变。源图片worker是UniTask线程池任务，当前manager只有WhenAll与取消后finally；不能把它们写成已经被阶段2Join。阶段5：原staging/publication清理继续只处理原Unity对象，旧worker只许结束本地解码、归还其token；不得在阶段5后新建Unity对象或发布。operation ledger要保留到真正drain才释放，不能关闭时假清零。阶段6–11无新接线；最终operation waiters/reserved/strong refs归零、无晚发布、幂等，场景才能按原owner合同结束。

必须由主审核对一个准确生命周期seam，不能假定通过：SimulationTickDriver.ShutdownBattleRuntime中stage1取消／stage2Join／stage5 ReleaseCancelledNativeContentStaging的实际调用及后置条件，限这些调用段、不读Q06。本轮未获此driver方法体读域，未读取。既有测试在head回调后关闭，此时body WhenAll已经完成，不能证明“真实decode pending时sourceworker joined beforestage5”。若原硬合同要求所有source线程任务也必须在stage5前Join，而现有seam做不到，则需要用户确认准确owner/异步关闭写域，本两生产文件方案不得自行重排十一阶段。此项是明确接口事实缺口，非要求关闭整个H08/H01父项。主审可独立读证，不开无限预检包。

下一实施阶段按次序，全部NOT_RUN：
| 门/目的 | 具名测试与固定断言 | 如何改变决定 |
|---|---|---|
| 新鲜安全/编译 | 原Editor唯一实例，idle/nonPlay/no test/clean Scene不保存；精准import/原程序集编译0CS error；不启动同Library实例 | 不满足即不运行fixture |
| header与实际字节绑定 | M12 VerifiedDescriptor_RejectsMutationBeforePixelAllocation；复用PNG rgba/palette/bmp独立expected pixels、MalformedOrUnsupportedPngDoesNotReturnPixels、DecoderFailureHasNoPartialImageState。cap+1/负数/long溢出、截断IHDR/同长SHA替换、压缩坏图、实际guarded worker | 所有错误在预定层失败，无partial图片/旧publication变化；不改变PNG语义 |
| 异步真实gate | M12 Admission_FifoCancelFailureAndExclusive；真实异步lease调度，C/ C+1/ E/ E+1，首waiter取消、CPU等待取消、worker异常、双Dispose、停止接单、故障后的drain | ordinary sum<=C；exclusive count1且其他0；>E分配worker调用0；finally余额0；不以计数器镜像代替执行顺序断言 |
| 真实小source worker | M12 NativeWorkers_SlowUploadPreservesPixelsAndBounds：fixture六图固定256²/384²/512²/256²/384²/512²、原RGBA规则、8MiB测试上限，上传持有5个PlayerLoop帧。另构造小BMP只验证旧route不误收费 | 对独立expectedRGBA、rect/pivot/UV/catalog/同图共享身份全比；读取实际数组length和decode开始/结束序列，预算不足时不得调用decode；同组retained另外累计 |
| 取消/重入/关闭 | M12 NativeWorkers_CancelWaitingAndLateGenerationCannotPublish；真正阻塞worker/byte等待，取消后late completion、reenter。复用NativeCoordinatorOn_CancelGenerationReleasesSharedStagingAndRetryPublishes、NativeCoordinatorOn_DriverShutdownRecyclesGroupedUnpublishedImages、DriverShutdown_CancelsAndRecyclesUnpublishedImagesInOrderedStages | 分别证明token精确归还、旧generation不创建Unity对象/不commit、阶段顺序和owner恢复；不以原head取消测试代替新decodepending门 |
| public入口及ON/OFF合同 | NativeDefaultAdmission_ConfiguredEntryPreservesFallbackAndRebuildsOwners；SameSourceConcurrentRequests_ShareOnePublicationAndNotifyBothCallers；CancelDuringInputCapture_DoesNotPublishOrStoreLateCandidate；ReplacedGlobalOwner_CannotPublishItsLateInputResult；NativeCoordinatorOn_MultiDefinitionSource2DAndAtlasPixelsRemainEquivalent | H08默认/冷OFF、cache同owner共享、旧owner失败与完整publication不变 |
| 唯一同输入比较 | M12 CompareFixedMixedSheetsOnce；同六图/同shader/render/同开关外设置，baseline原byte OFF与candidate ON，固定A-B-B-A四次加载一组，无预跑正式大图；慢upload5帧固定。每窗30s、组180s超时 | 正确性/容量/关闭全PASS后比较；实际未上传强引用payload峰至少下降25%，retained最终payload相同，whole-load总耗时两次各自记录；B总和<=A总和*1.20。无收益或额外超时/失败即结束本候选采用，不重测 |
| 审计 | 原Scene/GameConfig/单例/cache引用恢复、只释放本fixture owned资源、源码/输入fingerprint不漂移；适用ChangeLedger | 失败保留现场与原件，禁止全局cleanup/覆盖旧XML |

对照主要回答“真实未上传数组积压是否下降，代价是否可接受”，不是整进程峰值证书。8MiB仅为了小fixture触发同production算法，128MiB和E准确边界通过纯admission整数/小数据测试，不解码最大约403MiB基本模型资源。采冷load总时长、worker/upload各自范围，父子不相加，不从两次均值推P95，不把Unity帧数当tick。没有新hot hook、Battle recorder或H10录制器；原hot0GC只按未修改的路径复用，遇实际新增hot调用再精确收紧而不是取消门。

所有旧pixel/源共享/decoder大资源证据均按指纹复用；只运行上列受影响小用例，不全330、不最大图解码、不Player、不Android、不120+1800、不整个BattleSelfCheck。最大正式sheet旧测试LargestFormalSheetLoadsOnWorkerAndMatchesReferenceHash本次明确不运行。实际执行命令/Job/XML/原始数据必须下一阶段落盘，当前表是建议测试选择，不伪造已执行。

## 交付与回滚

本轮只有静态核验、10份真实治理前件、Task/Change/Operation和本PLAN/RESULT；源码/Unity/编译/测量为0。具体新增参数与关闭seam由主会话在上述三项内收敛后派同Task实施。M12保持PLANNED/OPEN，不恢复任何旧Goal/队列。
实现回滚必须先备份当时dirty字节并做Operation，只撤本M12 increment；候选OFF回到原路线，不改变H08共享默认，不用HEAD覆盖他人修改。未执行回滚、删除、Git提交或push。
