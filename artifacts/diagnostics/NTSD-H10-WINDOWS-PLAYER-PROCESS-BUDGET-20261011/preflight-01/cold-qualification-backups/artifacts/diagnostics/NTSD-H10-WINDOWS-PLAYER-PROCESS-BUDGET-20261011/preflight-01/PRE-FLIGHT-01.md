# H10 Windows Player 进程预算 PRE-FLIGHT-01

结论：**只读预检完成，方案PLANNED，尚需具名读取补齐入口/部署合同，不能直接实施或构建。** 本次未改脚本/Settings、未编译或运行测试、未build/Player/sidecar。H10父项OPEN；旧resume-03成功及全部历史失败保持。

已确认：
- HEAD8d55c6b68912560c7af755c424b41d3464a1921a、暂存空、原10tracked dirty；全部10真实dirty已另存字节备份，新脚本/.meta/工具/build-01均不存在。
- 原Editor只读状态idle/nonPlay/noncompile/nonupdate/nontest；Menu clean8。两个只读MCP命令可达，没有重启或第二Editor。
- disk Standalone IL2CPP=1、background1/frameTiming0、Burst true。旧builder会改这三类值且SaveAssets，不能调用；新builder仅backend暂态Mono＋归因恢复。
- Request默认sound=inherit，simulationOnly=false时解析为dispatch；计划显式dispatch＋simulationOnly=false＋skipLate=false。Atomic DataOrientedCanonical派生正式SoA/Indexed/Unified，不能继承旧JSON的独立候选flags。
- StartRun禁并发；Configure要求World/claimedSlots/两池active全0；真实普通Menu流程已经有角色，不可直接追加1000。App空players副本能跳过其SetupBattleCharacters，其他bootstrap仍待R2实证。
- StartRun内部TryConfigureEmptyDiagnosticWorld不先Shutdown（主会话提供Driver具名证据）。终态CleanupInternal却调用RecreateWorld，后者实际关闭audio并做map=true诊断完成后创建新World。必须捕获旧owner/owned refs，并分别记录runner终态与其后App真实地图关闭；不能把内部map=true当实际地图11。
- Harness固有MobileExtended1050/LooseQuadtree/Manual/paused/enableFrameChecksum=false，单OID1000角色基准。这与普通LocalFreeRun有区别；不能把其压力结果叫普通同配置、全角色压力或H07 PASS。
- App MenuMain返回分支无ResetAll；普通UI三轮复位和prewarm重入仍需R3。
- 已生成2752个已知部署域的逐文件source/target/SHA，96,956,488B；未复制。其它raw UI/Config消费闭包未证明，manifest目前是已知域清单而非完整可运行保证。

具体可审方案：[PLAN.md](PLAN.md)。下一步准确读域：[READ-SCOPE-REQUESTS.md](READ-SCOPE-REQUESTS.md)。机器证据：source-input-fingerprints.json、settings-scenes-inputs.json、initial-dirty-backups.json、proposed-output-absence.json、deployment-manifest-known-domains.json、editor-readonly-state.json、editor-readonly-scenes.json。

候选有限表：1次Mono Development build、2个独立Player进程；普通和combat1000各同进程3轮，第1轮120warm/180smoke、第2/3轮120warm/1800formal。所有数量/阶段待主审确认后才写执行请求；本次额度为0build/0Player/0sidecar。外部100ms Private Bytes与WorkingSet分列，exact PID+CreationTime+QPC对齐，不读CommandLine/env/credential；320MiB/2GiB/3GiB不变，dev PC不认证8GB低配。

阅读范围只涉及授权harness配置/入口/清理及直接局部定义、原四份诊断路径、随后主会话具名App生命周期片段。为提出准确读域，额外仅rg数据类型/方法声明名称；R1-R4方法体未打开，未读factory/StageWave/producer。一次只读范围打印超过文件EOF发生IndexError，已经读到最后一行；无文件变更或运行副作用，后续打印使用边界裁剪。没有调用历史read_thread，没有读取复制旧敏感工具输出。

实际命令包括git status/rev-parse/diff --cached、rg具名定位、Python有界只读文本/哈希/manifest及get_editor_state/manage_scene只读；无refresh_unity/run_tests/build/进程启动停止。最终保护和validator结果追加于收尾原件。预检阶段结束不等于暂停整个H10，也不授权推进下一优化点。

父项覆盖映射及本Task继续顺序已补在[H10-COVERAGE.md](H10-COVERAGE.md)：多角色/技能最坏集合和完整声音producer动态覆盖仍是当前PC真实缺口，CASE-MATRIX必须在具体实施/构建批准前锁定，不由两个单一负载进程自动满足。


## PRE-FLIGHT-01 addendum-01（当前补充，原文保留）

状态仍PLANNED，未执行代码/Settings/编译/测试/build/Player/sidecar。R1-R3已由主会话具名供证，精确采取方案及来源见 `artifacts/diagnostics/NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011/preflight-01/PREFLIGHT-ADDENDUM-01.md`。新的具体用例为2/16/507三角色三轮，见同目录`CASE-MATRIX-CANDIDATE.md`；四cue当前SHA与旧inventory相等，见`case-evidence-inputs.json`。并非所有producer/角色技能动态穷举；原“全部动态覆盖”措辞按本补充纠正，不降低全catalog静态＋实际运行请求覆盖门。

仍需独立审查：`NEW-READ-DEPENDENCIES-01.md`的R4A/B、R5A/B，`OBSERVATION-SEAM-REQUEST.md`的只读事件转发入口；`EMPTY-WORLD-ENTRY-ORDER.md`说明真实MatchConfirmed后仍自动Set/Load原cfg，因此原空players回调方案不闭合，不能直接实现。不得以反射写原cfg、清场或重复Load补救。候选跨进程模板/实际UI模式引用方案及100ms/250ms/99%采样质量/无自动kill处置均尚待主审冻结。

保存的原校验：ChangeLedger exit0/PASSED1423 records/4534 warnings，3条属于本PLANNED未创建脚本，不称本Change0warning；protection-audit的4094保护/406旧H10/18源程序集Settings无漂移。最终补充审计另存，不覆盖原件。没有恢复旧目标或跳其它优化；H10父项保持OPEN且继续同Task。


## PRE-FLIGHT-01 收敛表（2026-10-11；当前单一接续点）

**PLANNED / READ_ONLY_PREPARATION_CONTINUES。** 以下整合已有方案及主审本次供证，覆盖上文旧待定/同队/seed/identity/alias/样本措辞的冲突；旧文保留历史。本段不授任何实现、第四processor写域、编译、build、Player、sampler或追加运行额度。H10父项OPEN，无其它点/Goal/队列恢复。主审已冻结静态模式来源和review03的普通/千人证据颗粒度；不是运行PASS。唯一剩余准确读请求是FINAL-PATH-BINDING-REQUEST.md，不再泛读UI/legacy/producer找copy理由。

### 当前路径证据和真实部署差量

主会话独立具名读取Candidate SHA35ac697cb8b5a43bc9ae76413f59dd3498218b6c188a06a02e00e99b01d99f9f的ImageInput/Words/Spark/KillIcon/Capture321–361/Copy407–412/GetHash414–420：body/head/small是实际配置fullpath的Ordinal集合，只有body且非requiredUI可带allowMissing。未读missing-body hash helper，不能发明缺失处理。Manager native UI只能以Candidate已捕获的精确fullpath查询SHA；不另猜raw根。未读FirstNativeResourceTable/ReadDimension/parser/feed producer/atlas/legacy正文。

| 输入域 | 本轮真实已知与冻结manifest比较 | 还不能声称 |
|---|---|---|
| body已解析原件 | H08实际C#703路径/773引用，逐项全部在当前manifest，当前SHA/长度0差；原件SHA c6ab501ea8b79481ab8afc83ba7ee2a9e72fe5066a9f4b6d9b8c1e5f9afc7154 | 该原件scope排除UI，不是head/small清单或新Player候选数据 |
| resource.dat/system.dat | 1637B/4cb8d30ca477e5e8ec0aa625813ff2c7a63e1c3af561da3822c1583dfac6818f；3091B/d848cc6d711a01d12a054e2b5adb903938d7afb182854eb39ddbbf2236c31ae2；现源/manifest同 | 未调用parser/Candidate或解码图像 |
| Spark | resource首表第43项sprite\UI\SPARK.png；system scalar99×79；formal-vfs图3600B/15d8843e0ce87ff63f46dff7170d30c23baea0f2799434b26717aadfd5ec881b已包含且现SHA同 | 与独立UIPanels/SPARK.bmp不是同一输入；两配置文件只缺一个会抛异常，不能忽略 |
| 独立BMP | UIPanels/SPARK.bmp 156752B/e11c8104824be415cd66e844dbbe49b48e1e6ea5735880669e45182c34f77306已在独立目标，现SHA/长度同 | 不用BMP替换formalPNG，不扩大旧目录copy |
| Words | resource16起六项WORDS0–5.png；当前候选formalVFS路径/manifest均无；allowUnavailable:true，必要图缺失可警告并返回null | 根解析仍待准确合同；即使确认缺失也不补资源；实际Player必须记可用/跳过及告警，不能说rawUI全可用 |
| KillIcon | Candidate只收非空且存在的feed路径，缺项null；项目模式Asset44–51为c/sk2/sk2/sk1/sk2/sk2/sk2，候选formalVFS三路径均不存在/manifest无 | 尚未把Asset值到feed数组及真实root解析链冒称已核；不补图，不逆读feed producer |
| head/small | Candidate已核会收requiredUI配置fullpath；当前没有现成实际解析全集原件 | 不能从703body、目录扫描或已知2752条manifest推导完整rawUI部署闭包 |

机器原件 `resolved-input-deployment-comparison.json`（SHA71fed1e501594b4c726fda002346d3c0c809316de6df13f6bd4a81d8615390c4）和`kill-icon-input-data-check.json`。**本轮已证明必须新增的部署文件数为0；不等于已经证明没有其它缺口。** 原2752/96956488B known-domain manifest及2334formal CSV不改，仍尚未部署。下一最小根合同：BattleContentSource23–46/49–52/73–88/95–116、Manager44/50–53；KillIcon最多数组声明14/TypeResourcePath99–104。实际属性ImageRoot，不发明VfsRoot。GameConfig实际root=Assets/NTSD/Content/LoganRuntime，配置SHA25c8e08f4d1be55ee55d0404aaf4e46d099afc6c2ba3d9040e1c5203f654d9ff。当前缺少的实际已解析head/small数据保持缺口，不调用Unity或新写解析器生成。

### 完整有限执行表（待完整实施派发；本轮执行次数全0）

| 阶段 | 固定对象/入口 | 必须满足与停止条件 |
|---|---|---|
| S 安全前置 | 原项目唯一Editor、当前实际Menu与Scene清洁状态；全路径/指纹/源目录/owner库存 | 重新核idle/nonPlay/no compile/update/test、无他人操作、Scene clean；旧状态不算本次证书。activeTarget已Windows64、API/disk backend均IL2CPP，原background/frameTiming/Burst保持；任何未知并发差异停止 |
| Q 首次编译与窄资格 | 仅拟新增builder/probe/sampler，原Editor一次具名资格组；实现前记录确切测试方法/命令 | 构建上下文/保留输出/错误输入拒绝、原raw/alias/capacity、packet值历史、tee全载荷exactly-once及所有错误分支、有效GC正控/empty/tee、phase/采样/恢复门。全部通过才B；首次失败回审，不重复同版资格找PASS。旧63scope及其它focused不能替代新tee |
| B 唯一build | build-01/NTSD-H10-Budget.exe，Mono Development Windows64，两Scene固定Menu/Battle | 精确一次BuildPipeline；旧callback全部严格门保留，仅获准后精确H10一次上下文选2334清单。非H10原1373合同保持；保留H10路径上下文缺失/已消费/丢失即拒绝，不fallback。finally恢复backend且实字节核验后，才可copy generic417到独立域；完整部署/产物SHA冻结。失败不重build/删除产物 |
| P1-O1 | 普通进程轮1，P1 Naruto OID2，另16/507中性 | seed682973786；三个公开槽team1/2/3、Confirmed/inputId、0电脑、真实VS0/1、原LocalFreeRun；120warm+180smoke；一次L→D→K→neutral，自然053持续代表；门全过并真实关闭/零残留才O2 |
| P1-O2 | 同PID轮2，P1 Gaara OID16，另2/507中性 | 相同配置/入口资格；120warm+1800formal；一次L→D→J→neutral，自然gaa/j4→043多cue代表；全过并关闭后才O3 |
| P1-O3 | 同PID轮3，P1 Kimimaro OID507，另2/16中性 | 120warm+1800formal；一次L→W→K→neutral，kim/j1 stereo代表；全组三轮通过、源/config模板完整冻结和进程收尾后才P2 |
| P2-C1 | 第二进程轮1：真实Menu/prewarm/VS读取本轮mode，P1成功模板独立clone仅players改非null空；App.Set/Load，不点Fight | suppression实际false且不写；地图prepared/id/真实boundary、World/slot/两池空、audio owner准备后原AreReady/StartRun；harness seed1314149188、combat1000 AI、最低有效type0单OID；120warm+180smoke；首差停止 |
| P2-C2/C3 | 同第二PID轮2/3，重复真实公开关闭重入 | 分别120warm+1800formal；不新开进程代替重入。原harness MobileExtended1050/LooseQuadtree/Manual/paused/checksumfalse、maxCatchUp/backlog请求1/8如实记录；normal33ms/2interval合同不改，不能称普通Host/H07通过 |
| E 结束核对 | 两进程各三轮全部原件及失败证据/Scene与源/Settings/ChangeLedger | 按scope裁决限定Windows预算；不自行关闭H10父项、追加组/进程/构建、推进其它点或恢复Goal |

额度始终**1build、2Player、每PID三轮（1冒烟+2正式）**，不是三轮全正式。普通三个目标技能只投一次，下一状态最早为实际观察到经过2tick，不承诺精确两消费tick；InputSystem正常Queue/afterUpdate，无手动Update/StepOneTick/暂停/重press。记录每个可见真实packet tick及每槽四值；漏tickUNKNOWN，关键消费无法证明则INPUT_OBSERVATION_INCONCLUSIVE并停止，不以旧Manual6/13/15tick或44声作新预期。目标声需在声明窗口自然发生且有本次真实source/clip/startSequence与播放/输出证据；不足不再换一组。

### 已冻结的声音证据域和固定容量

主会话本次明确可固定review03表：普通四目标全窗事件明细4096/轮（溢出FAIL）；千人全窗遍历所有原请求，完整counters/membership/order摘要，明细只首个非空原batch前64event及同批最终最多64source启动变化。必须带tick/phase/完整batchCount/未采数/同槽中间启动不可见；不是稳态代表、千人全事件逐声认证或完整trace。全窗生产play/reject/drop/load/build等实际计数独立保留。没有可确认实际播放样本时门未过，不换batch。

raw1000含19builtin/2mode；1050是旧不同clip数；Ordinal先原raw后缺失单属slash alias1968，原raw精确身份优先，不折叠大小写/注册生产alias。实际新prepared集合与真实接受另核，alias观察命中不代替生产会员资格。1024counter、2048index、普通target/voice4096、unknown32、phase256/process、输入转换32/process、8192tick×最多8实际槽值/process、cold voices64、owned2048/round≤6144/3round全部冷构造。超过或未知key立即令证据无效，不裁生产请求/扩容。旧703/1000/1050清单均不冒称新Player实测。

观察错误/容量/未知只设固定状态，不在callback抛新增异常；完整原List同步forward一次且顺序载荷不变，生产异常原样传播；外层冷边界停后继。诊断capacity失败不叫生产丢音/关闭回归。所有新表由probe所有，保存自有值历史；不得持可变生产List/Players当快照。GC资格/计量和新路径0GC尚未执行。

### 安全恢复门（不得用例外绕过）

- 修改前另做真实Task/Change/Operation与当前dirty备份；完整派发必须明确第四processor与唯一暂态ProjectSettings backend权限。本预检旧备份不能恢复后来用户内容。所有新文件/CreateNew，build目录已存在即拒绝，失败产物保留。
- builder一次上下文绑定精确输出/nonce/requestSHA/manifestSHA/Scenes/target/options，Prepared→CallbackEntered→Completed；finally清上下文/自己的回调并恢复API backend。API与磁盘实字节核对；只有本任务已归因的单backend差异且当前SHA等于本任务临时版本才按另记Operation恢复前件，未知并发差异停止不覆盖。不改background/frameTiming/Burst/defines/EditorBuildSettings，不SaveAssets/切target/第二Editor。
- ordinary ready后warm前capture精确Driver/owner/原override；原override只允许null或同owner，未知拒绝。sample后App关闭前同一身份条件恢复。combat StartRun同步Configure后首Update前安装，提前保留旧owned refs；runner终态可能已清音频，返回时仅核同Driver/owner/当前tee再restore，然后App真实map11。无新hook重排关闭；身份变则不覆盖他人sink，不伪造restorePASS。
- stage1原owner禁止新tick/input；probe停自己的输入和订阅，stage2无worker，3不spawn，4沿原owner，5只观察audio，6–10不手动回收，11必须App真实map而非harness内部map=true。每轮World/slot/borrower/owned归零、pending/tail/后帧Destroy通过才下一轮。generic按精确key/clip跨轮同引用；不GC.Collect/UnloadUnusedAssets/ClearCache/globalDestroy。仅解绑自己的回调/移除自己新建的keyboard/释放自己的缓存与句柄。
- sampler只精确PID+CreationTime，100ms绝对QPC，read-start/end/deadline/late/UTC齐全；Private和WS分列。private非正/身份复用/API失败无效；最大gap250ms、连续域≥99%slot覆盖、首末边界距离≤250ms，否则不发预算PASS。跨界归严格bucket，无法精确划steady则全runner active/terminal按2GiB。所有样本保留，不宣称未采瞬时峰，不扣probe成本。
- AudioClip≤335544320B；Private steady≤2147483648B；transition100ms采样峰≤3221225472B。旧218.313758MiB仅同输入对象门复用，新部署SHA/actualprepared/ownership必须吻合；不把对象预算当整进程或目标PC认证。
- build watchdog45min只记录不killEditor；UI480s/选择120s/Battle准备180s/每warm+sample600s/关闭tail120s/每进程3600s。首差、超时、预算或正确性/GC失败停止余轮/第二进程，已知captured App公开安全关闭后非零Quit；无响应保留现场，不自动kill/restart/retry/清缓存。实际退出失败仍失败，不用进程消失代替十一阶段后置条件。

### 未完成和证据复用

当前只剩准确Source/path与head/small/feed输入数据绑定、部署差量裁决，以及第四写域直接人类确认和完整实施派发；后三者不能被本表自授。已冻结模式/队伍/粒度不再另生候选分支。复用resume03的限定Editor播放/关闭/0GC、旧PCM/1050对象账本及所有失败原件，因本轮无代码/资源/Settings改动；新Player/进程预算/新tee资格均未执行，旧证据不顶替。H10父项OPEN，H07正常33ms/无持续backlog/dropped0与Android最终条件原样保留。其它可做只读准备完成后回交主会话，不转下一优化。
