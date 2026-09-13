<!-- CHANGE-RECORD
id: NTSD28-B11-SOURCE-CACHE-CALLER-PRODUCTION-001
status: VERIFIED
change-kind: SOURCE_SELECTION_CANDIDATE_CACHE_AND_PRODUCTION_CALLER_INTEGRATION
code-path: Assets/NTSD/Scripts/App/GameConfig.cs
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/UI/LoadingPrewarmController.cs
code-path: Assets/NTSD/Scripts/UI/SelectRoleItem.cs
code-path: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2ObjectPool.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCacheCallerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCallerPlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/NTSD28B11SourceCallerPlaySetup.cs
authority: Active alignment goal/D-023; Q02 E3 parent contract; current source/cache/caller audit; existing Unity framework/non-battle boundaries and eleven-stage shutdown.
evidence: VERIFIED_SOURCE_CACHE_CALLER_LOAD_GATES_ONLY / FINAL_FOCUSED_40_OF_40 / FULL_SELFCHECK_PASS / NATIVE_DIRECT_APP_MENU_REENTRY_PLAY_PASS / EACH_46_RESOURCES_ZERO_SURVIVORS / LEGACY_APP_PLAY_PASS / CS_0 / SCENE_CLEAN / LEDGER_470_40_PASS / FORMAL_CONTENT_NOT_MIGRATED
-->

# E3源码/缓存/caller接线写前记录

状态PLANNED，生产与测试均尚未改。精确写前Task为同ID，逐项冻结以上八个生产脚本和两个测试脚本的职责、失败/关闭/回滚与全部验收；该Task与本Record共同为合同，不能以只加字段/只加缓存helper替代完整E3。

## 原状与设计决定

已观察：LoadingPrewarm固定key命中跳过Execute与ApplyLoaded；sprite cache只有bool；pool完成单独置Ready；直接入口仅IsPrewarmCompleted早退；App未验content source；NTSDSound保留原soundRoot/AudioController，按当前manager收集DAT声音引用；通用loader.CancelTask不能撤销执行后缓存；pool批量预热只在入口检查accepting，yield之后仍创建；candidate字典copy未隔离可变value。

采用既有GameConfig字段选择source（空默认旧源，资产不改），既有manager统一Native预热/验证与世代，既有loader只缓存可核验候选输入并使用完整visual key，Unity publication按现owner核验/重新应用，不缓存bool。Native有序await不放在通用cache命中即跳过的Execute内；此为解决实际owner生命周期并复用E2，不是省略cache/caller验收。通用loader和音频源码不改；菜单输入/展示流程保持。发布配置从捕获DAT重新构建，不暴露缓存模板给可变runtime consumer。

新configured输入工作在stage1及manager销毁失效，迟到纯数据不写cache或发布；资源回收沿E2 stage5。pool批量预热复用原算法，after-yield/before-materialize检查本次scope和generation，关闭/重新准备后不能继续生效；不重排任何关闭阶段。

## 预期验证和副作用

新增临时GameConfig字符串字段默认空，不改变现有asset/正式内容；Q07后续才配置部署根。合法候选的配置/cached inputs和完整Unity发布可被三真实caller一致消费，失败不回退旧内容；旧资源路径和其他cache domain保持。此时没有添加或删除任何DAT/PNG/WAV/Scene/Prefab，没有改变schema12/20/23、33ms、GAS或Mono边界。

先focused RED后写生产，实际命令/测试结果/Play/Scene/保护hash逐次追加。回滚只限该Task的精确符号和测试，保留E2及Preparing owner修复；不执行未批准删除或Git回退。当前不得报告COMPILE_PASS/VERIFIED或Q02完成。

写前API名冻结：GameConfig.BattleContentRuntimeRoot；manager.PrewarmConfiguredLoganContentAsync(Action<string>)返回UniTask<string>完整key，ValidateConfiguredContentForBattleAsync()返回当前key（legacy为null）、AssertConfiguredContentUnchangedAsync(string)强制比较包含legacy-null；CancelConfiguredContentPrewarm撤销原始输入阶段，ConfiguredCandidateCacheHitCount只作诊断。完整候选cache key前缀NTSD.LoganContent.Input::，root locator前缀NTSD.LoganContent.Root::，后缀分别为candidate.SourceCacheKey和规范化RuntimeRoot。测试只清自己GUID根对应的这些cache项，不清其他domain/外部source。

初始focused先覆盖默认legacy选择、generic cache命中实际跳过Execute事实、candidate配置value别名隔离、Native同source命中/manager重建、不同root同ID、PNG单独变更、坏Root不以旧publication通过验证。随后补实际caller、并发取消与pool after-yield矩阵；初始测试通过不关闭E3。

## 初始RED实际结果

真实Unity job1c2f71f5195040aaa1a2ed446e366e61完成7项，6FAIL/1PASS。CandidatePublicationCopies测试实际将Original改成RuntimeMutation后另一副本也读到RuntimeMutation，明确复现value别名；generic cache test通过，实际Execute次数0/完成回调1。其余5项在缺GameConfig.BattleContentRuntimeRoot入口处失败，后续native cache/换源断言尚未执行，不能称这些行为已验证。

CS0、Scene dirtyfalse/root14；audit-production-fingerprints的11生产文件hash全部不变。当前只有新focused测试脚本，生产未改。证据在artifacts/diagnostics/NTSD28-B11-SOURCE-CACHE-CALLER-PRODUCTION-001/focused-red.json及SOURCE-CALLER-AUDIT.md。下一继续同一Task/Change实现完整source/cache/caller与取消/池预热合同，补实际入口/并发/Play覆盖；不新建较小替代目标、不止于让初始7项变绿。

## 首次生产写后

八脚本均已写：GameConfig root字段；candidate从捕获DAT重建配置值；manager configured入口/完整key cache+root locator/当前publication核验/请求世代/低层E2 CanContinue委托；LoadingPrewarm消费者完成后应用旧配置、sprite不缓存bool、前置成功才pool、最终核验source再Ready；直接Start/LoadCharacterData和App Initialize出生前后验证同key；driver stage1取消configured输入；pool分批预热generation/predicate检查。原低层E2四参ForOwners签名保留，新WithScope私有核心增加发布许可；通用loader/音频未改。首次compile/focused及新增caller/取消/Play验证待执行，E3未完成。

首轮focused job8128aedb33be4dc4be0bf736898a1faa完成29项，2FAIL：PNG漂移的InvalidDataException不继承IOException，显式补捕获；EditMode中manager销毁回调不作为Play证明，cache重建夹具在非Play时显式调用生产幂等OnDestroy再销毁，Play仍走真实Unity回调。新测试TearDown同样只在EditMode显式调用生产cleanup，避免测试自身资源残留。补充根解析先于in-progress标记，防止非法路径同步抛错后复用旧task；同source共用加载的进度回调为每个订阅者转发，作用域失效后停止并释放引用。candidate不再保留无用途的可变parsed模板，只缓存captured DAT及图片identity。

第二轮focused job36aefadfeb33459b947ae489e6e5de2a已35/35（E3 13+E2 14+Preparing8）。写前扩大准确资源重绑范围：SelectRoleItem仅现有PrepareNativeResourceRebind内的旧Sprite身份判定，使用ReferenceEquals保留已销毁Unity对象的可证明原引用，不修改状态/输入/UI流程。manager新增当前publication的Sprite→角色ID metadata，在UI.Clear清空字典后仍可重绑旧owned头像；metadata在提交前准备并与publication交换，在owner销毁/legacy commit释放。补实际Idle头像cache清除/重建断言。这是E3 cache/owner重建必需的资源引用适配，生产准确范围现九脚本；不重构非战斗模块。

直接Start写前补记：将已有异步内容准备至启动尾部放在同一try/catch，失败时仅在同driver/World/准备世代且尚未Stopping/Stopped的情况下调用已有App有序关闭；避免出生后复验失败留下Preparing实体。不会关闭后继World或改变关闭顺序；已失效/正在关闭的旧续体直接返回。新增focused还将覆盖实际菜单PrewarmOnceAsync、直接LoadCharacterDataAsync、共享请求/缓存隔离与pool暂停后取消，真实App入口留Play验证。

Play probe写前：新增已声明的NTSD28B11SourceCallerPlayProbeEditor，消费独立Temp request并进入/退出现有NTSD_Battle。使用临时合法catalog/DAT/PNG和运行时GameConfig克隆（仅root字段变化），正式资源资产不动；direct模式由现有Start加载，app/menu模式用既有stress suppress只准备资源，再实际App.InitializeBattleAsync。menu模式另销毁无World实体的旧manager，以真实LoadingPrewarmController.PrewarmOnceAsync验证cache命中重建后再启动App。场景关闭前后核对三published key、Preparing/Running、关闭后两帧Worldnull/实际pool0、native owned Sprite/Texture卸载零残留；恢复原GameConfig static并销毁clone。每种入口至少一轮，退出重进补一轮；必须诚实标注隔离候选不是正式330对象内容验收。

当前写后验证：focused-v3 job42c7004f675c4ad0a2885e1fcd4757b7为39/39（E3 13、E2 14、Preparing8、ordered4）；追加已销毁manager的Idle头像重绑断言后focused-v4 jobc10fea1a49c84b09a5e7ab7b44722952为13/13。新Play probe已写，完整SelfCheck请求已发，实际native三caller Play仍未执行。当前RUNTIME_PENDING，不能关闭E3；九生产/两测试脚本实际范围保持，通用loader和声音源码不变。

Play首轮e3-direct-1实际FAIL，启动器报selected Logan source is not current publication，未进入Running；未据此关闭E3。测试注入原在Editor update的Run阶段，可能晚于Start加载。写前补充仅测试脚本NTSD28B11SourceCallerPlaySetup.cs（UNITY_EDITOR && UNITY_INCLUDE_TESTS）：BeforeSceneLoad从已准备request读取temp root及已验证GameConfig asset path，克隆配置并先设置唯一static入口；不改原asset、不创建runtime manager。Editor probe在进入Play前准备root/IDs/asset path，运行时只使用该setup，并输出setup失败/当前key/生命周期诊断。真实生产脚本不因测试时序问题改动，失败artifact保留。

真实三caller现已PASS：e3-direct-2/e3-app-1/e3-menu-1均World4、三key一致、两帧仍Stopped、actual pool0、46资源零残留；menu确有candidate cache hit1且Ready。最终owner审查补记：configured scope必须同时属于当前全局manager，不能让仍存活但已被替换的旧manager在输入任务迟到时写cache/发布。新增该身份guard及实际pending→替换global owner测试；低层E2显式ForOwners仍保持独立诊断语义，不修改非战斗生命周期。随后做最终focused和menu重进复验。

## 最终限定出口

最终focused jobca5bbc179ca44add941bf701f0f779a9为40/40（E3 14/E2 14/Preparing8/ordered4），完整SelfCheck最终新结果PASS。native e3-direct-2/e3-app-1/e3-menu-1/e3-menu-2均PASS：实际World4、三published key一致、定义名来自所选隔离source、关闭至RuntimeMapCleared、两帧后仍Stopped/Worldnull、实际pool0、每轮46 owned资源卸载后全灭；menu两轮均candidate cache hit1并Ready。默认旧内容e3-legacy-regression-1真实App启动/关闭及E2四项回归PASS，旧入口保持可用。

最终CS0、NTSD_Battle dirtyfalse/root14、Ledger470/40PASS；3059保护中3047原hash不变、12声明既有脚本变化、零缺失/范围外变化。新测试setup仅BeforeSceneLoad安装临时GameConfig clone，实际asset未改；首轮late-injection失败及所有RED报告完整保留。准确证据/命令见IMPLEMENTATION-REPORT.md和Q02-EXIT-REPORT.md。

本Record VERIFIED仅确认source/cache/三个caller、失败取消/重建/资源重绑与关闭合同；Q02加载基础据此交付，R17仅相应加载子条件PARTIAL_RETURN。不能宣布B11完整迁移/全部音视/战斗规则或总目标完成。下一Q03 Task NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001 / READY_CONTRACT；正式六DAT转换/联合字段/schema/生产consumer与Q07部署仍按队列后继。
