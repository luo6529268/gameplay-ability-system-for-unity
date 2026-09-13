# Q02后继：正式目录、来源与缓存发布事务

状态 DELIVERED_LOAD_INFRASTRUCTURE_ONLY / E1_E2_E3_SCOPED_VERIFIED。父目标 NTSD28-UNITY-BATTLE-REALIGNMENT-001，BATCH-02/Q02。前置路径/raw PNG/native range/PNG alpha精确入口子包已验证；正式全局切换仍未进行。

## 恢复入口

先读 docs/ai/CURRENT-AUTHORITY.md，再读本Task与 artifacts/diagnostics/NTSD28-B11-CONTENT-SOURCE-PATH-CONTRACT-001/Q02-A-AUDIT.md。该审计已定位共享caller，不重复笼统盘点；从其未闭合的事务设计继续。原生 ObjectDefinitionCatalog28::load_extracted_root 读取 catalog.csv（registry_section/index/id/type/source_path/published_folder），GameSession28 的 data.txt 是背景/辅助入口，不能以它代替对象目录。

## 要冻结的准确合同

1. 源身份与对象catalog：字段/格式、唯一性、registry_index顺序、portable解码DAT路径、失败语义；从实际原生调用链确认。不可用只读helper或data.txt成功代表目录全量有效。
2. 显式source如何贯穿配置解析、图片处理、SpriteCatalog、头像、固定prewarm cachekey和声音cue引用；旧pending async如何被判过期，失败/取消保留哪个已发布世代。复用现有prewarm generation与staging，不创建第二套runtime框架。
3. menu预热、测试直入和battle setup的实际消费者必须消费同一个已验证来源。选择/随机/排序、UI表现、普通加载流程等非战斗功能保持；仅可声明必要的内容适配。若无法在此边界内实现，暂停受影响部分并报告具体冲突。
4. 不清全局resource cache，不以global lookup非空或旧固定cachekey视为新源就绪；不在SetupBattleCharacters尾部临时换目录，不用旧数据兜底伪装新源完整成功。
5. 当前source入口是BuildCharacterDataFromSource与LoadCharacterSpritesFromSourceAsync；PNG alpha source和range source必须是同一个批次身份。不要单独把新source传给旧配置集合。
6. 配置、图片和头像需要原子发布到既有owner或有可证明的gate；声音cue路径适配与WAV整体迁移分开，后者未自动获D-023授权。

## 准确范围先声明，再改代码

Q02-A候选路径为 Animation/GameDataManager.cs、Animation/Manager/CharacterAnimtorManager.cs、UI/LoadingPrewarmController.cs、Load/NTSDResourceLoader.cs、UI/CharacterUIResourceManager.cs、AppManager.InitializeBattleAsync/SetupBattleCharacters、NTSDSoundPlayer.PrepareBattleCuesAsync。这些仅是读链入口，不是允许无边界修改的清单；实施前另建准确Task补记和独立Change Record，冻结实际符号、non-battle不变量、失败/回滚/关闭阶段及测试。

Q02可用隔离catalog/资源验证目录、staging和发布失败事务。Q03的6文件9帧Converter拒绝不准通过忽略帧/填零绕过；全正式内容切换仍在Q07，依赖Q05/Q06完成。Q02的基础事务验证不等待整场视听；Q07完整内容就绪不能凭夹具通过签发。

## 出口

正式catalog身份/validation与330对象目录证据；旧源回归；成功/失败/取消/重复载入及不同source同ID的缓存隔离；实际生产入口一致性、退出重进/关闭阶段与Scene clean。未触及的menu/UI/Editor行为保持，有明确文件保护证据。若需拆实现包，应在本Task列出顺序与依赖，不能遗失跨包原子发布约束。

Q03字段合同可独立准备；R15/R17按本包实际证据部分回访，Q07/Q09/Q10后续条件保持。总目标和Q02均未完成。

## 已冻结执行顺序（2026-09-13）

E1 `NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001`：正式catalog CSV/ID/index/物理DAT路径读取、source及definition fingerprint，配对Logan parser/builder一次构建全部配置候选。Unity旧ParseCharacterFrameConfigs跳失败行并返回partial不得用于新源。Native GameSession28 initialize在catalog.success=false时返回false，Unity新源失败也不返回可发布集合。该包不发布global、不修改菜单。正式330目录由真实source-linked ObjectDefinitionCatalog28 probe对照；当前Converter6失败如实保留，不绕过。

E2（E1后冻结准确Record）：把目录候选、同源config/sprite/UI staging接入现有prewarm generation和TryCommit，准备所有依赖后单个无await发布段；IsPrewarmCompleted须位于完整事务出口，不能在头像await之前提前为true。显式source必须来自候选，不允许裸source搭配旧pending配置。保留旧入口/Editor默认语义，newsource会话禁止active battle中切换。现有UI dictionary只按id、null保留旧值，需整批替换和准确旧资源释放责任。声音cue读同源definition但WAV迁移仍单列。每个修改前需完整shutdown/rollback声明。

E3（E2后冻结）：LoadingPrewarm/测试直入/battle setup最小接线与source世代cache key；缓存命中会跳过Execute，必须校验已发布source身份并能应用已缓存候选；pool完成不得掩盖前面失败。resourceLoader的全局cache/其他domain不清空、不重构。仅内容接入必要适配，选择/随机/排序/UI效果不变。失败、取消、重入、相同ID不同source和非战斗回归后才关闭Q02。

E2/E3前置不等待正式全内容Converter通过，可用准确隔离候选验事务；Q07正式切换等待Q03/Q05/Q06。完整Q02出口仍是上面的事务整体，不因E1成功而缩小。下一先E1实现及验证。

## 当前出口

E1限定门槛已VERIFIED（native330/330、Unity39/39）；E2具体恢复入口docs/ai/TASKS/NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001.md / READY_CONTRACT。E1 DefinitionFingerprint仅catalog/DAT，E2/E3不得把它用作完整PNG/头像/声音内容identity。Q02仍未交付。

最新出口：E2原子发布及Preparing关闭回访已限定VERIFIED（详见各Record）；下一实际Task为NTSD28-B11-SOURCE-CACHE-CALLER-CONTRACT-AUDIT-001 / READY_AUDIT。E3生产入口/缓存一致性仍未做，不能把父Task或Q02整体关闭。

当前E3审计已DELIVERED，生产Task/Change为NTSD28-B11-SOURCE-CACHE-CALLER-PRODUCTION-001；初始RED6FAIL/1PASS，生产未改。下一实现统一来源/candidate cache/三个caller/停止及pool续体合同，不能只关闭前置helper。

## 父事务当前出口

E3 SOURCE-CACHE-CALLER-PRODUCTION-001已限定VERIFIED，补齐本Task的实际caller/cache/关闭/旧源回归出口。final40/40与完整SelfCheck、native三caller及menu重进、legacy App回归均PASS；详见E3 Record/IMPLEMENTATION-REPORT/Q02-EXIT-REPORT。Q02基础交付，不是正式330对象内容已可用；Q03六DAT/联合字段与Q07正式迁移后继保持。
