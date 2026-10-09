# 第95批源坐标AI行增量刷新：局部收益与Driver正确性/GC检查点

状态 IN_PROGRESS / DRIVER_PARITY_PASS / DRIVER_GC_FAIL / NOT_ADMITTED / DEFAULT_OFF。原FOCUSED/LOCAL通过限定于下面独立范围，不当完整Driver资格。
最新完成仍94/73实际执行；95为同一实施包在途，不新建96拆分资格和性能验证。阶段5/6、34父关闭0、H07 PERFORMANCE_FAIL、Goal active。

## 批次100的正确语义
用户澄清“编号到100特指方案仍没有落实清楚，而不是要求达标”。有效合同第8.1节已按此更正；100仅查具体热点/改法/代码落点、已落实与未落实、必要验证和下一动作是否清楚。清楚而性能FAIL应继续，不因正式窗口未过而暂停。仍不清楚才完成在途/有序恢复后按用户要求暂停并交付外部评审，不进入101；不能重编号或用测试数冒充方案落实。

## 已观察代码事实与当前改法
原source-mode在每个producer/routing边界强制TryCaptureRow，读整行字段；其它canonical mutation已进现有pending publisher。候选default OFF，只对exact LF2Character、源坐标已初始化、无force-full/hooks且State/HitStop与已发布行相同的边界，把SourceRuleXInt/ZInt暂存到既有pending X/Z，直到原TryCommitPending才可见。Y/其它mask保持；未知派生、状态/HitStop变化仍旧full capture。没有新数组/owner/热容量，原initial full capture/source滚动拒绝、身份/epoch/generation、hard-breach/fail-closed、输入/RNG/pass/cadence/publication/排序不变。
既有publisher的角色变更检测用Y，full/index member用HitStop，是直接相关的保守资格门原因，本包不修通用分类器。
四个准确代码域和冷恢复合同见Task/Record/Operation：
- Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
- Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs
- Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleAiUnifiedRowPublisher.cs
- Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs

## 原Editor聚焦结果与失败留痕
原2022.3.62f3 PID74724/6402，Menu clean8、未Play、未切Scene或启动第二Editor。
- 初次test-first编译CS1519为新补丁残留字符，修新增字符/既有hook方法拼写；compile-test-first-01.md保留，不把编译失败当RED。
- RED01 f0b194fc0f1f4c7c802db7bb84b5e8b8：实际11/11缺新API，red-01。
- RED02 bea851a81f3a41d6b3e48fcfdb695b06：新角色变化2/2缺API，red-02。
- GREEN01 b8c7d7aba060496bb189b6c1c01b3903：35实际31pass/4fail，green-01。三源坐标case将live validator放在source latch被清除后的物理读域；byte GC计数正例0不可证明0GC。新测试改为直接indexed合同/完整first10 product、full与candidate旧live validator结果相同；复用现有前后正/空例校准recorder，不改runtime validator/旧测试。
- GREEN02 dfb0ec431e38410c85aa17c1bf04cf19：37实际31pass/6fail，green-02。新计数错误假设无保守回退；两个1000 fixture误用默认World400容量，各case两World共1200拒绝，成本INVALID，禁止将其称1000AI。修为既有MobileExtended cold profile并新增registration/producer/AI read/完整refresh门；候选成功计数与fallback分开验证。
- GREEN03 03716acec7b240c8a2f56f2d201c415b：37/37 PASS、0fail/0skip，green-03/tests.xml SHA 8F31FDCCC27B39E8A3D0689C694D193121CECB32E746153ABECB0C2E62FC10C3。15新增expanded case＋22旧UnifiedAuthority邻域，包括默认OFF、正负/零源位置与物理不同、两边界、RNG/输入/实体/28行字段/first10/index、source X/Z staging且Y保持、stale generation拒绝、force/hook/custom/state/HitStop回退、旧occupancy/fault/roll-forward/生命周期邻域。新测试的通用计数保留全部边界，暖scope单独验证增量delta，不把必要fallback删掉。
真正GC依据为BattleScopedGcAllocationRecorder前后已知分配正例和空例校准、未饱和/未wrap、选定完整CharacterInputAll scope0event；旧GetAllocatedBytesForCurrentThread仍不作为证据。

## 局部1000成本（不是完整战斗或Windows性能认证）
同seed/同源位置、16warm+48sample，四块各12、OFF/ON顺序交替；两World都ObjectCount1000、无注册拒绝，每pass producer freeze1000。无AI sampled reads0，AI sampled unified consumer reads48000；源增量成功48000/96000，完整64tick refresh计数64000/128000。各块完整row/index/first10/输入/实体和native RNG状态一致；各测量scope校准后0GC事件。
| isolated场景 | OFF均值ms/pass | ON均值ms/pass | 相对变化 |
| --- | --- | --- | --- |
| 1000实体/无AI | 4.4145229167 | 4.0120979167 | -9.115934% |
| 1000实体/实际AI producer | 12.5260583333 | 12.1754937500 | -2.798682% |

有AI四块中一块12.567775→12.7752ms回退，另三块下降；只称局部均值信号，不称稳定/全面收益，更不能推导完整logic P95、FPS、Android、120FPS或H07通过。固定静态source/fixture DAT、无physics/collector/publication/camera，不等于Sakura实局。该片段约0.35ms改善，绝非整12ms都可消除，也不足单独解决33ms缺口。

## 尚未验证与同包下一动作
完整Driver正确性已验证到两个正式root各12tick与实际Logan catalog两千人布局各32tick OFF/ON，但新增千人资格用例整体仍FAIL，因为完整StepOneTick的可靠GC scope在OFF/ON都有事件。来源尚未定位，不归因候选，不将旧93实际Windows0GC结果和本headless Manual fixture混同；不豁免GC，也不据此重开已过五阶段。下一同95核清这两次常见事件的具体分配域，先读现有call链和旧有效证据；仅需要时准确声明最小分段/归因取证，不能为完整热点重新建一套Profiler或无界采样。根据实据修受影响入口或测试准备，再复测受影响case；不重两个已过正式root、37局部或旧微候选。资格/收益通过后才启动同版固定窗口。没有新actual Scene/window/1800/no-drop证书或生产推广，不以批次数或性能未达停目标。
不解冻EXT1/ATLAS/Mono/默认Role/Burst/新AI行为/新Grid，不改Scene/Prefab/资源/Settings/Packages/Server/Q06body，不做删除/移动/Git discard/commit/push。十dirty备份与1128范围保护检查点见audit/Operation；本检查点不是整批95或Goal完成。

## 聚焦/局部阶段检查点审计（Driver资格前历史）
Tools/Validate-ChangeLedger.ps1实跑exit0，1390 Records、8 governed dirty code均覆盖、0ERROR、4280 WARNING（没有宣称0warning）；详validator-checkpoint-01.json。1128根保护/10改前dirty备份0drift，HEAD a0ec/暂存空。初次九写域diff检查exit2，仅本批Tracker新EOF空行；该失败audit-before-eof-fix-01.json保留，apply_patch仅移除此新增空行，复检exit0（仅CRLF提示），不更改代码/测试结果。不复跑37或任何性能。最终checkpoint after manifest保存当前写域/evidence SHA，但状态IN_PROGRESS，不作95完整Driver/性能完成证书。

## 同95完整Driver资格结果
执行前Task/Record明确同一test文件局部扩展、当前dirty原件另存before-driver-qualification-01，原三个runtime/Admission/RawCapture/93Suite保持不变。复用已有正式source/catalog冷数据、临时Pool/Driver、allocation seal和有序关闭owner；Manual/logic-only，无Play/Scene切换或保存。
- compile-01：新增测试缺using/接口限定名，随后误用不存在World.Capacity。只修新using/真实RuntimeSlotCapacityForDiagnostics接口；不是有效RED。移除本新增block/using后的规范化旧测试与备份完全相同。
- driver-green-01，job5e021912280a4a9b813e20d608572572，4FAIL。新断言错误要求所有tick沿用tick1 published数组，实际CommitExecutionCandidate轮换两个cold buffer；修为冷准备两个state及各34数组的引用集合检查，不删容量/0GC门。原件SHA2BD8BD8829B4D87804C86889A11C24CBA9411DF97BA6ED587EBB458D69B6B3CD保留。
- driver-green-02，job4acff4ffcae44867b85cb083b27b7718，4实际/2PASS/2FAIL。两个正式root各12tick OFF/ON适用oid/action/hp/vx、完整state/hash/RNG/row与容量通过；两千人仅OFF运行完32tick后GC tick17失败，不能声称当时已比较ON。XML SHA7260F3A52DFD95FB9A2F8B0A55D81BB9ABCD55E14EF688B80935BD3E7378683F。
- driver-green-03，jobcf1dc0e02c5046f49a548acc383ff7ee，仅重两个千人失败case：2实际/0PASS/2FAIL。取证顺序改为先双方跑完并保存结果、再完整比较、最后严格GC门；没有删除GC断言。XML SHA392E134E09529FE4F5BF31EC3091780625883F5168FCF2DBC3FA303785F8E881。

两千人OFF/ON各32tick共128个完整step：每tick actualAI1000/实体1000，完整extended/lockstep hash、完整native scalar/legacy RNG、28行字段/first10、indexed合同均同；cold两个state/34数组引用保持、无precommit failure/hard breach/collector abort，source roll-forward保持0。D候选61917/C61495 source增量＝oracle验证，双方actualAI consumer reads32000，flag/oracle finally恢复、关闭零对象/slot/borrower与Menu clean8通过。正式root另48step为适用字段的有限权威证据，不能扩为全部战斗规则对齐。

可靠GC结果（每路16warm＋16sample，Scope为完整StepOneTick，hash/序列化/断言在外；前后正/空例校准均有效、scope未wrap/饱和）：
| 布局 | OFF16sample事件总数 | ON16sample事件总数 | 判定 |
| --- | --- | --- | --- |
| Dispersed1000 | 34 | 32 | 两边非0，FAIL |
| Combat1000 | 61 | 32 | 两边非0，FAIL |

大多数sample为2event/tick，OFF D两tick3event、C有一tick28/一tick5；不是校准失效。rawValueTotal的单位未导出，不能称bytes。原始每tickscope/校准与full/candidate状态JSON位于driver-qualification-01，先保存后断言，失败不掩盖事实。新的完整Driver测试整体仍失败，不能报资格PASS、0GC完成、完整性能收益或生产准入。候选默认OFF、H07 PERFORMANCE_FAIL/Goal active；95仍同包在途，latest完成94/73，不因100性能门暂停。

Driver检查点02留痕：audit/validator-checkpoint-02.json；1128范围保护、10初始dirty备份0drift，三个runtime相对检查点01未变；diff check exit0、HEAD原a0ec/暂存空；validator exit0/1390 Records/8 governed code/0ERROR/4280WARNING。原Menu clean8 idle恢复。after-manifest-checkpoint-02保存当前十写域和所有本包证据SHA，旧01保留，不是95完成或Goal停止。
## GC归因03/04收尾（非性能/0GC准入）
用户新问H07最终目标；本轮不再启动新测试/性能窗口或实现修复，只收尾已经在途作业，当前验收标准与Goal active未变。
03 job45bb10ecbc73486788749ebffa8fddcc实际1/1PASS，04 job1643644f2bdf4bce9f4fee3fa5325a80实际1/1PASS；均是有效归因，不是原完整Driver资格通过。03 XML E5E13A7FC1CDC5176324507A53E113E3EE163AC09F08BD9A1794DBB77DDD7A0F；04 XML A6043F41AA7A79FD9061F26EF04293F1CB9D44D6287867AC0BA71D37DF3B3511。03/04 JSON SHA分别C5D2E00AC7AE10F68D71FBB21735C3EFA59B631E8AC9C311FAD419A6D445BE61 / 34A04D3EDF644021C32E10611790098FDBAAB341F4F52100E533C63C68CDD005。
同D1000/OFF32tick，16warm+16sample前后正/空例控制有效/未饱和，marker GC.Alloc/unit TimeNanoseconds，只解释事件数不按bytes。stage准备每sample1event；等序分解wrapper StopWorker/SoundDispatch/Inspector各0event，core通常1event、tick17/31各2。两种分解的32tick完整extended/lockstep/native/legacy state与既有OFF基线完全相同，actualAI1000/固定行容量/关闭零对象/slot/borrower保持。具体stage/core分配调用点仍待确认，stage空walkable临时List是静态候选不是已证调用栈；不凭该线索改runtime或豁免分配。仅原测试文件新block与using变更，移除整个Driver新增block+5using后的旧37tests规范化字节完全同before-driver-qualification-01。原两千人完整Driver0GC FAIL、三个runtime/default OFF和95在途不变；未重测37/正式root/旧Windows。
首次03编译CS1061缺System.Linq及误在编译中提交0test job b2444c7261e94cd492a35883cdc7eb73原件保留，0test不算PASS；修仅新增using，之后编译/idle确认再运行。原Menu clean8/非Play恢复；未启动下一阶段或更改目标。checkpoint03的保护、validator、当前SHA另记录，不覆写01/02。
## GC调用点取证05收尾与用户方案质询（2026-10-09）
本轮不改变H07目标/权限或0GC门；用户询问“有完整优化思路吗、95批为何仍不能闭合”，本轮只收尾已启动作业，不再新增候选/性能窗口。既有03/04有效分域证据保留。
05三个单case均实际1FAIL/0skip：job7ccc00cb482843b7afeae2a37ea65900（XML49F2053FF35A2E8CEAA4FA0B2F41F613BE0817C2F76B36A4D664E62880095F2E）、29daf4eb04584e6dbdd7cb3610f0947d（D059CBACE7EDC779A5DE874DFEC9B21FF5210AF7E548A9BBA0E99AD3289DA5BC）、f4fc1fc4f7ff49bdb7abca978815d4a0（751EA14EF353343281FCFADF3F89DE71A64DAA0EC048858ABF401C043F4B3043）。各scoped tick17校准有效/3event，但导出unique envelope均0，不能确认callsite、不能称0GC/资格或性能通过。原始raw分别4B、12,643,012B、21,203,416B；最后raw能检出marker名称字节，仅证明名称存在，不证明对应sample/stack已核实。后继若继续此归因须先恢复已有raw检查线程/边界frame（目前导出从previousLastFrame之后开始），不再盲重跑相同fixture。
所有Profiler设置before/after十字段完全同，已有history新absent备份且未清空/回载。终态原Editor idle/nonPlay/nonTest/nonCompile，Menu clean8；三个runtime和default OFF保持。本轮新增只同测试文件的私有capture helper/默认为null可选参数；原37及正式root、两千人严格0GC断言保留。测试工具当前仍FAIL，不能报告本包交付完成。最新94/73完成，95第74包在途，阶段5of6/H07性能FAIL/Goal active。
方案问题的证据不是“缺少更多同构测试”：93实际Role千人OFF logic均值48.31/57.64ms、P9563.75/88.61ms；CandidateCollect16.05—23.04ms、CharacterInput12.28—17.13ms是大热点。92/93局部group候选未建立完整收益，已不采用；95实际AI的isolated输入只约0.35ms/2.80%信号，未证明整tick收益。必须把方案落实与最终性能验收分开，不能用批号/诊断/测试数量代替高收益主改法。此前“全套方案闭合”尚无证据；模式化千人AI策略/空间查询的新方向未准确冻结，不以原版AI一致单独否决，也不得凭本次质询自动实施。已过H11限定CPU阶段与SetPass5受控窗口不重开，正式1800只在冒烟有达标可能时执行。
本轮验证、保护与当前SHA存checkpoint04，不覆写01—03；仍无新windows/1800/production promotion、无Q06 body/Scene/资源/Settings/Packages/Server改动，无破坏性Git。

