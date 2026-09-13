# E3 source/cache/caller合同审计

2026-09-13；审计DELIVERED_SOURCE_CALLER_CONTRACT_ONLY。当前Task/Change为NTSD28-B11-SOURCE-CACHE-CALLER-PRODUCTION-001，生产未修改；初始7项focused已实际RED：6FAIL/1PASS；生产仍未修改。E2/Preparing回访已验证的成果保持，不重做。

## 当前源码事实

| 路径/符号 | 观察及后果 |
|---|---|
| LoadingPrewarmController.PrewarmOnceAsync/CreateCharacterConfigTask/CreateCharacterSpriteTask | IsPrewarmed早退、固定NTSD.CharacterConfig/NTSD.CharacterSprites；ApplyLoaded在Execute中，sprite Result只有true。poolTask完成单独置Ready。缓存命中/失败后下一task成功都不能证明当前owner与source有效。 |
| NTSD_ResourceLoader.AddTask/ExecuteTask/ProcessFrame | cache hit直接Result/Completed并调用OnCompleted，跳过Execute；in-flight相同key合并。ProcessFrame以isRunning保护。不能在其Execute内再次泵同一loader并await发布，否则可能自等。 |
| NTSD_ResourceRequest.Register/NotifyCompleted | Register捕获原OnCompleted，合并请求仍会回调每个原consumer。故需要的配置应用必须放在消费者完成路径，不能只放Execute。 |
| NTSD_ResourceLoader.CancelTask/ExecuteTask | CancelTask只置IsCancelled/Cancelled，Execute完成后仍可写cache/Completed；不能把该标志当作native停止后不发布的保证。本包不改通用调度。 |
| BattleTestBootstrap.LoadCharacterDataAsync | 只读IsPrewarmCompleted早退或走旧Parse/LoadSprites；必须按所选来源走同一预热/验证入口。 |
| AppManager.InitializeBattleAsync | 已有Preparing世代guard，但出生前没有content source/key/freshness核验。App在BattleLoading只应验证已发布内容，不热切定义。 |
| SelectRoleItem.GetAllLoadedCharacterIds及NTSDSoundPlayer.PrepareBattleCuesAsync | 前者读当前manager角色；后者每次从manager.CollectBattleSoundIds收集DAT引用，WAV仍走原AudioController/soundRootFolder/cachekey。source选择必须在caller前统一，不能在这些末端临时换目录。 |
| LoganVisualContentCandidate.CopyCharacterConfigs | new Dictionary只复制map，LF2CharacterDataWrapper/characterData value仍同对象。跨owner输入缓存需从捕获的immutable DAT重建发布值，不能复用上次发布可能改过的模板。 |
| LF2ObjectPool.PrepareCapacityAsync | 入口检查accepting/sealed，但每5对象yield后不复查，且末尾直接记目标容量。新增caller的取消合同必须包含after-yield/before-create guard及世代，防止关闭/重新准备后旧预热继续创建。 |

以上均为当前源代码结构事实；未把静态推导写成真实Play结果。生产文件SHA256已冻结在audit-production-fingerprints.json，供实施前后范围核验。

## 已冻结路线

使用既有GameConfig新增默认空的BattleContentRuntimeRoot选择来源，当前序列化资产不改；Q07才启用正式部署。路径以Application.dataPath父目录为相对基准，或接受明确绝对root，后续DAT/image路径仍由已验证BattleContentSource约束。

既有manager共享Native配置预热、当前发布验证和请求世代。复用既有NTSD_ResourceLoader精确key缓存纯候选：root locator仅指向完整root+VisualFingerprint candidate key，不作为Ready；每次命中必须验证DAT/catalog/images当前身份。Unity publication不缓存bool、不把实际应用埋在可能被cache跳过的Execute；Native按await顺序使用E2发布，当前owner只有全部视图/key/资源有效才可复用。新owner须从候选重新发布，配置值与旧owner隔离。原通用loader和其他domain保持。

LoadingPrewarm、直接battle预热与App出生前核验统一读同一来源快照；source变更、失败、取消、销毁/重建与等待期间的内容变化必须显式失效。pool预热只能在前置成功后运行，完成仍要检查真实状态，不能盖过前面失败。候选raw work的写cache/发布权限于stage1失效，Unity暂存继续stage5回收；pool预热受自身/调用方scope及generation保护。

不改菜单选择/随机/排序/输入/布局，不改WAV根目录或全音频迁移，不改Gen/Plugins/asmdef/schema/33ms/顶层关闭顺序。准备准确八生产/两测试脚本合同已在同IDTask/Record登记；不把本审计或初始focused通过写成E3/Q02完成。

## 必须兑现的后续出口

初始7项focused覆盖默认旧源、generic cache执行事实、配置模板隔离、cache hit重建manager、不同root同ID、只改PNG及非法Root拒绝冒用旧发布。仍需补实际三个caller、并发/取消/关闭后迟到结果、pool after-yield、其他domain保持、完整SelfCheck与真实Play退出重进。正式6DAT Converter错误仍Q03，Q07迁移尚未执行；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

## 初始运行证据

focused-red.json记录job1c2f71f5195040aaa1a2ed446e366e61：7项完成，6FAIL/1PASS。mutable candidate alias由Original→RuntimeMutation在第二副本中实际复现；generic cache Execute0/OnCompleted1通过，其余5项因缺GameConfig source入口失败，深层native行为尚未执行。compile-console-after-red.json为CS0，scene-after-red.json为dirtyfalse/root14，production-unchanged-after-red.json确认上述11生产hash未变。

实际命令：现有Editor bridge refresh_unity；run_tests指定NTSD.Test.NTSD28B11SourceCacheCallerEditorTests；get_test_job保存完整失败证据；read_console筛error CS；manage_scene get_active；后续Ledger结果见change-ledger-after-red.log。未运行本包Play或重新完整SelfCheck，因为生产尚未修改；前包的完整SelfCheck/五轮Play证据保持其原范围。
