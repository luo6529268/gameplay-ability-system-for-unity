# NTSD-H07-INDEXED-NEAREST-CURSOR-REUSE-20261010 / PLANNED
Operation includes two scripts and named governance documents; before-manifest and intermediate snapshots retain true bytes/SHA/Git.
授权：讨论会话转达直接人类“那你来处理即可，然后再让另一个会话去处理”，2026-10-10具名有限指令。IN_PROGRESS。
仅两脚本：AiSensingKernel新增internal TryFindNearestCursorReuseForDiagnostics及private FindIndexedGroundCursorReuse/FindIndexedAirCursorReuse；AiSensingKernelEditorTests新增冷构建一致索引、明确边界/固定随机/调用间变更/极值、热0B、Explicit单次两布局ABBA测试。所有旧方法及生产调用逐字保持；非Indexed委派原public。原eligibility/LowerBound/handle helpers复用。
每次query/team span栈局部cursor/slot/dx/refresh flags，cursor移动才重新读该侧，选中dx复用。原int手写Abs与溢出、visits、span/left tie/slot tie/10000/249/250/40/state9/ground facts与handle全保持。不增static生产flag/cache/World字段。状态owner为当前栈帧，返回即失效，无跨tick持有、容量增长、资源或11阶段新关闭位置；原stage1停查询后无需drain。测试数组冷owner为测试实例，固定snapshot容量，finally无World/Scene资源。
先缺失API RED，再C#编译+本文件非Explicit全部原/新语义测试+同scope0B。测试反射只冷绑定delegate；断言/日志/序列化不进计时GCscope。
通过后仅Explicit成本一次：每布局1000行，spacing120/12，seed20261010，同immutable snapshot查询1000self，OFF ON ON OFF；每窗8轮warm+32轮采样，各轮外围Stopwatch。GC含整个测量循环且两侧0。任一正确性/GC/容量失败停剩余窗存原件，不重测/调warm/Profiler/修新分配。每布局两个ONmean低于两个OFFmean且ON最大P95/P99<=OFF最大P95/P99才LOCAL_GAIN_SIGNAL，否则INCONCLUSIVE/NO_DISCERNIBLE_GAIN正常结束，不外推整tick。验证前已固定此判据。
不跑完整Driver、旧ABBA或正式长窗，不改其它脚本/资源/Scene/Settings/输入/Server/Gen/Plugins/Q06，不恢复Goal。旧compatibility/producer/95 OFF及旧Driver失败保留。生产接线需另审。本Task只局部候选，不降最终0GC/33ms/确定性/容量/关闭与Android。
风险：stale局部值、左右耗尽/溢出/剪枝、筛选/统计次序、索引夹具遗漏、采样污染/噪声。回滚只另登记Operation后定点移除本Task新增段，使用本Operation真实dirty before；不得HEAD覆盖。旧历史保留，两个脚本前SHA/Git/bytes已备份；每次替换留中间版本。无删除/移动/清旧数据/push。

CODE_WRITTEN: missing-entry RED job09f13dc464a74960879c7ceb0ef97878 expectedFAIL. Added candidate entry/two loops only; old methods preserved. Added complete cold index builder checked by existing ValidateIndexedContract, boundary/random/intercall/extreme/fallback and both-layout warm0B tests. No local cost window yet.
