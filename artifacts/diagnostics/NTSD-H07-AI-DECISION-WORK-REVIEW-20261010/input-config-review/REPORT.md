# H07 输入实际配置复核

状态：SCOPED_CONFIGURATION_REVIEW_COMPLETE；父 Task 仍 IN_PROGRESS，未形成可交付生产优化方案。

## 新观察与更正

1. 当前磁盘 Assembly-CSharp.dll 的 DebuggableAttribute DebuggingModes=263，包含 DisableOptimizations；当前 Bee 编译响应 /optimize-、/debug:portable。assembly-metadata.json 保存 SHA、时点与准确 flags。历史12.037463ms窗口程序集 SHA 为75d97ad...，与当前EC5599...不同；没有保留的历史程序集可读，不能据当前标记断言旧12ms有多少由编译模式造成。没有切换 Editor 模式/编译/项目设置，没有 Debug/Release 耗时对照。Unity2022.3官方 API 描述 CodeOptimization.Release 会进行优化：https://docs.unity.cn/2022.3/Documentation/ScriptReference/Compilation.CodeOptimization.Release.html 。这只是后续测量需固定的条件，不是生产业务优化或已证根因。

2. DataOrientedCanonical 与 CanonicalStoreDirect 是两个独立配置。SimulationAiSensingModule.cs:108-109 默认 SnapshotCopy；SimulationWorld.ConfigureAiExecutionProfile:5215-5264 不改变 owned-input mode；专用 setter5198-5212 仅允许无实体/无占槽且不在 tick 的冷边界。全 Assets/NTSD/Scripts 搜索专用 setter 只找到两处已有 Editor 测试调用。ProductionEntityStressHarness 的正常 ApplyAiSensingConfigurationForDiagnostics8058 与 SimulationTickDriver 的空 World 初始化2622 只配置 profile，没有选择 direct。旧实景报告未输出 owned-input mode，因此这是当前启动链静态推导，不能冒称已读到旧场景 actual flag。

旧报告“已存在 direct canonical”是能力存在的事实，不能用来证明千人测量已经使用它；此前以该能力排除实际 SnapshotCopy 的调查过早。本条追加更正，不抹去旧结果。

3. 精确差异：SnapshotCopy 的 CaptureSharedOwnedSnapshot -> TryCaptureCanonicalState -> Store.TryCaptureCommon 把 inputs[slot] 复制到 snapshot.Input，再进入 kernel；Direct 分支以 in inputs[slot] 进入同一个 kernel。二者 kernel 内都保留 AiDecisionInputState input=ownedInput 的局部副本、witness、RNG与最终 commit。能候选消除的是中间完整输入结构体的 materialization，不是全世界快照/整份Runtime/AI查询/后续发布。AiDecisionInputState 声明为10 int+31 byte共41字段；未测实际布局大小或每窗复制次数，不虚报 bytes/耗时。本线索与87/88 producer Runtime刷新、compat17字段guard、nearest cursor均不同，但没有新收益证据。

## 原 Editor 新鲜验证

job631bfaf72077438c8e38467e351c4689，最终summary total3/passed3/failed0（不将progress总发现10636当运行数量）：
- CanonicalInputOverload_ConsumesOwnedRowWithoutSnapshotCopy：独立 owned row 输入有效，不依赖 snapshot.Input。
- OwnedInputMode_DefaultsToSnapshotCopyAndRequiresResetBoundary：现有测试实际检查默认 SnapshotCopy 与空World可配置 Direct；名称不能代替测试覆盖，不冒称已测全部reset/注册后拒绝。
- UnifiedAuthority_DirectCanonicalInputMatchesSnapshotCopyAcrossTicks：3角色/slot0,3,7，16tick 的已列输入、目标、flow与legacy RNG断言通过；实际helper没有覆盖完整native双RNG14字段、完整RuntimeChecksum、全部Prev键、slot复用/失效与实景收益。3/3不能晋升完整等价/生产准入。

所有测试代码与生产代码未修改；没有成本窗、Profiler、JIT准备或全量旧测试。原Editor前后 idle/nonPlay，NTSD_Menu clean/root8。Console返回7条MCP Client handler退出条目（分类Exception），不报告Console0，也没有据此判定测试失败。

## 后继明确缺口与处理顺序（未派发）

先完成 same-input owned-copy候选的最小完整证据，再决定是否交执行。不要同时切编译模式与owned-input模式。
1. 加强/复用等价夹具：Direct/SnapshotCopy使用同初态、同输入、同同步随机cursor；对比全部41输入字段、witness有效payload/exit/visits、native/legacy双RNG与完整checksum；包含coordinate/no-target/普通/held/profile、slot释放复用/foreign/null、oracle追踪与输入内核失败不提交。准确列明真正受影响case，不机械跑旧全部测试。需新增脚本先另建Change/Operation。
2. 确认无实体冷入口：现有 Harness.ApplyAiSensingConfigurationForDiagnostics 在角色注册前，适合显式诊断 request->config->World 的请求/实际模式记录；不能在first-tick observer中强设（已注册，setter会拒绝）。后续如需接线仅这里及明确 Editor owner/report，不修改AI规则、共享writer/store、RNG算法；生产默认 SnapshotCopy保持。
3. 用固定同一编译产物进行成本对照，保留outer CharacterInput与Driver计时、逐tick校验在计时之外。输入细分/Profiler/CPUcycle provider保持关闭。先记录具体实际模式/程序集SHA/已编译优化标记，再评价消除中间copy是否有可辨信号；既有12ms不能当新组OFF。成本证据与完整Driver0GC准入分别汇报，不把旧2event当12ms原因或用它抹去成本观察；也不把成本诊断冒充正式准入。
4. 若该中间copy差异太小或无可辨收益，结束此候选；不得凭能力存在就强推。独立Debug/Release控制仅为测量条件诊断，不能冒称业务修复；本轮不改变共享Editor模式或后台运行新组。

当前不是execution-handoff。生产默认未改，执行会话cursor82仍idle，未发任务、未恢复监测。producer暂缓/compat与95 OFF、33ms/确定性/容量/关闭/设备最终门保持。
