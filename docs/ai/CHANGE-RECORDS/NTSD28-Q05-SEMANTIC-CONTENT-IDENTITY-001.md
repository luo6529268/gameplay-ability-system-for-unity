<!-- CHANGE-RECORD
id: NTSD28-Q05-SEMANTIC-CONTENT-IDENTITY-001
status: FOCUSED_TEST_PASS
change-kind: SEMANTIC_CONTENT_IDENTITY_INTEGRATION
code-path: Assets/NTSD/Scripts/Animation/LoganContentIdentity.cs
code-path: Assets/NTSD/Scripts/Animation/LoganObjectCatalog.cs
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Animation/GameDataManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05SemanticContentIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11AtomicPublicationEditorTests.cs
authority: Q03 VERSION-IDENTITY-AND-CAPTURE-CONTRACT / semantic-identity-test-vectors.json; D-022/Q05 approved same-window DAT semantic identity binding; Q02 atomic publication and source cache contracts retained.
evidence: RED_14_FAIL / MAIN_75_PASS / VISUAL_6_PLUS1_PASS / SELFCHECK_PASS / INDEPENDENT_HASH_PASS / MENU_PUBLICATION_PLAY_PASS / JOINT_SCHEMA_PENDING
-->

# Q05 解码语义身份接线

准确7脚本：新不可变LoganContentIdentity、catalog/candidate、现有两个publication owner、新测试与原Atomic测试。raw DefinitionFingerprint与VisualFingerprint算法保持；新增SHA256(CurrentTag ASCII + NUL + raw32 bytes)与固定首8字节little-endian ulong（零→1），CurrentTag=NTSD28_LOGAN_DAT_SEMANTICS_V2。保留完整raw/tag/semantic摘要；仅内部测试向量入口接受不同tag，正式catalog只用当前tag。

Catalog SourceCacheKey改包含semantic，candidate SourceCacheKey使用同semantic再加原raw visual指纹；AssertInputsCurrent同时校验当前semantic和raw/image，旧decoder candidate不能复用。现有缓存caller沿统一key，无新增缓存/loader/队列。CharacterAnimtorManager PublishedLoganContentIdentity直接投影已有publishedLoganCandidate，GameDataManager在现有prepared-reference事务附带同一个immutable identity，失败不发布新身份；不改事务顺序/非战斗UI/资源/关闭阶段。

Identity提供CreateLocalValidationSessionIdentity，复用现有LockstepSessionIdentity构造/协议1和catalog ulong，不改wire/输入时序/外部Server。测试从实际已发布owner取得identity创建本地会话并让StrictDelayedInputBuffer验证匹配/不匹配内容；完整raw/semantic对象保留在publisher。没有引入生产联机启动流程或声称外部Server自动接线。

先反射断言缺API的RED、冻结3向量/异常raw/tag/字节序/零保护/同raw不同tag/不同DAT/root独立；然后生产。验证catalog/candidate实际源码、原子发布成功/失败/取消与缓存caller、正式330 catalog身份、完整SelfCheck、必要配置源Play。原138/合成fixture既有非native路径保持，正式资源不切。当前缺失Temp PNG测试夹具按既有生成器恢复，不改Assets资源。

本语义版本代表本Q05已批准decoder全模型，raw32+版本覆盖确定性解析语义，不另称runtime全部消费；额外frame/metadata内容hash、trace绑定及snapshot guard按父步骤3/4继续检查。当前12/20/23/1/1未发布中间态，不升版本或提前Q07。旧6MISSING字段不可凭身份晋升。双OPoint owner capture/restore guard独立后继。

保留Unity/GAS、非战斗、33ms/3ms/十一阶段/Scene/InputActions/Gen/Plugins/外部包/stage.dat USER_HOLD与例外。禁止computer-use，仅现有Editor桥接/结果/日志。没有新lifecycle owner，identity为不可变加载期值，随既有publication引用生命周期。预变更字节/SHA保留，回滚须批准，仅准确差量，不覆盖前包/用户工作。

## 已写

14项RED均失败于缺少身份API，原job JSON完整保留；首次尝试附加PNG测试名不匹配未运行，另以准确NativePng测试1PASS恢复Temp夹具。读取含中文失败消息时Python默认GBK导致输出解码错误，改-X utf8取同job而非重启。7脚本已写，raw算法保持、semantic进入两cache key/原子publisher，实际已发布owner用于本地session。编译/focused/实际catalog/SelfCheck/Play待。

## 限定出口

主75/75 PASS；正式330定义/906引用图片capture，rawDefinition4EFE1D2A...C58C，semanticDB579550...4407，ulong3900ECBC509557DB经独立Python一致。Visual旧六DAT拒绝断言独立NTSD28-Q05-FORMAL-VISUAL-CANDIDATE-ADMISSION-FIXTURE-001修正、6+1闭合。完整SelfCheck10:59:15Z >10:58:32Z请求PASS。隔离源实际menu重进Play缓存命中1、三key同、World4、46资源全部释放/borrower0/两帧Stopped；实际sourceKey semantic也由Python从catalog/DAT bytes独立复算匹配。raw两算法源码保持，不改外部协议/架构/资源。完整命令/失败历史/正式capture与隔离Play边界见artifact REPORT。下一NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001，guard/相关内容hash/版本/trace/replay后继。
