# H10 resume-01 精确计划（代码修改前）

Task NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 / Change NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001。讨论会话对人类继续H10及已批准PC预算的授权作独立审阅后，派发此次测量夹具修正。仅一次新版本Play额度；原失败组和所有根目录原件不覆盖。

唯一代码写域：H10ResidentClosureDiagnostics.cs 的 PcBudget* 增量及输出路径。复用只读 BattleScopedGcAllocationRecorder，生产和此前其他方法不改。新增 PcBudgetAllocationMeasurementQualification、三个独立计量owner；Queue/PresentSounds在Step期间各自测量，外层publication+dispatch重放时关闭sink内部采样，三个范围排他，不在任何热scope校准/await/写JSON/创建容器。容量256，owner为当前测试线程，每次Begin必须finally End，整个fixture finally Dispose；每个范围前后校准及原样ScopeResult均落盘，首差保留。

修正generic负控为真实data/016.wav；原五formal代表和public builtins不变。资格核真实路径，冷prepare核wrapper/clip与跨轮身份。加强当前Menu组件/序列化字段保护及真实App.BattleBootstrap空地图guard；此夹具不覆盖真实Battle地图或1000实体。

最小实验顺序：当前输入/源程序集/SceneSettings保护及原Editoridle安全状态；最窄编译；无解码的runtime资格与新计量资格（正控>0、empty0、未校准/无效scope不认证、无嵌套Reset）；两项均通过后，仅一次EditMode显式Enter/ExitPlayMode组完成三轮prepare/负载/公开关闭/drain/下一帧Destroy/重入及真实pending取消和自然销毁。失败立即停止剩余实验，只按已声明归属安全清理，不重复找PASS。

每个新状态：三个recorder由单次ExerciseHot/qualification拥有，固定256容量；样本128、每轮阶段最多16、fixture rows96/hosts12/async operations8为事前容量；不增加生产缓存。所有recorder在热测返回/异常finally失效并释放；stage1停止请求、stage5原player释放owned并解绑voices、tail drain/下一帧Destroy后重入，stage8/9/10/11由真实公开driver报告及无地图ownerguard约束。generic跨三轮保留，全部自有host销毁后且无原cache/config/其他prepared/voice/跨key引用才精确RemoveCache并按ID去重释放。未知归属保留且报失败。

风险：计量API无响应、校准污染或嵌套Reset；诊断温身不足；实际播放/异步完成/组件恢复边界。任何真实0GC或正确性首差不得用生产修改、关闭Profiler、换门限消除。原PrivateMemorySize64的0仍不可用，不评价2/3GiB。输出音频仅非零采样才称波形观察，听感false。

回滚源为新Operation真实当前dirty备份，不使用HEAD覆盖；原先受测版本由备份及根目录指纹固定。未经精确后继批准不恢复文件或删除输出。无Q06新读/子代理/Player/Settings/资源修改/旧Goal。
