# 第十批 M-03 生产计数报告 Task Contract

Task NTSD-OPTIMIZATION-BATCH10-MATERIALIZATION-REPORT-20261006 / RUNTIME_PENDING；Change NTSD-OPT-M03-MATERIALIZATION-REPORT-010 / RUNTIME_PENDING。
用户本轮“开始执行下一批的任务”，按第九批交接实施生产分组报告；父M-03 OPEN。
只原Editor19040/6401具名EditMode和实际compile，不切Scene/Play或运行完整M0/Profiler/GPU/设备。

## 原状、准确路径与最小设计

现有diagnostics11个request/attempt/return累计及四组8-long Build/API已通过；
benchmark v5为benchmark-local接受样本，生产缺冻结报告。复用BattleCanonicalJson，不改v5。
既有生产：Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs，
BattleCentralRuntimeDiagnostics新增reset epoch、启动创建的内部identity token、CaptureMaterializationReport；
ResetMaterializationCounters仅增加epoch递增，原gate/Build/API算法不变。
新生产：Assets/NTSD/Scripts/Animation/Rendering/BattleCentralMaterializationReport.cs/.meta。
新测试：Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleMaterializationReportEditorTests.cs/.meta。

报告显式非热路径主线程Capture→ToProjection/ToJson，冻结43个long，不留World/backend/资源/lease。
累计cumulative-since-reset；窗口window-delta需同source token/epoch的两个累计快照；
baseline空/不同source/Reset/窗口再作baseline或任一counter倒退，整份窗口拒绝(out=null/reason)，不clamp。
窗口保留start/end原值复算；zero只是已观测counter0；无FPS/GPU/成功pixels/1000AI/Android判据。
每次固定43-long copy及显式字典/JSON有分配，禁止每帧调用；没有自动采样器、Mono/菜单/文件writer。
source token启动创建不含World，epoch只Reset变；纯managed报告允许跨关闭留证，
shutdown阶段5Reset仍清累计/变epoch，无新owner/queue/Unity资源或关闭阶段。
计数writer与capture同主线程由调用者保证，窗口不是wall-clock或显示帧/tick/pub数分母。

## 验收、边界和恢复

先reflection fixture RED再实现/GREEN，相关旧225回归，不全量benchmark/真实Profiler。
聚焦：冻结43long/JSON单位/三组Build失败及拒绝payload、累计/窗口、Reset/来源/倒退/嵌套窗口拒绝、
projection修改不影响report、显式Capture不修改计数/不触发Build；原counter+epoch Reset热路径预备后0B。
0B不覆盖报告copy/JSON或完整渲染链；诊断真实开销/动态alpha比例/收益未测。
33ms/3ms/Hostmax2、checksum/pass/RNG/publication/插值/排序/UV/segment/fail-closed/slot/lease/GPUconsumer/
11阶段保持；不读Q06活跃方法体。EXT-1 PROPOSED/MODIFY_REQUIRED、MONO USER_HOLD不解冻。
不改PERF/ATLAS/MONO/EXT-1正文、bank/预算/资源格式、Scene/Settings/InputActions/Server。
八before备份核SHA后写脚本，286保护含前九批/活跃Q06 hash-only/原资源设置及外部jsonl；
外部HEAD2cccd597保留；恢复另授权仅本批hunk，不从旧HEAD覆盖。
完整生产基线/完整链0GC/dirty/像素与关闭重进/1000AI/Android仍待验收。
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH10-MATERIALIZATION-REPORT-20261006/RECORD.md)；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH10-MATERIALIZATION-REPORT-20261006/REPORT.md)。

## 实际结果（上述为事前合同）

八before22:12:05核SHA；reflection fixture实际compile/RED完成58、25 capped缺入口失败记录保留。
实现后原Editor生产/测试DLL均晚于source，CS0；GREEN58/58与相关旧225/225，共283去重case，0失败/跳过。
完整43long映射/冻结/JSON及窗口拒绝门已聚焦通过；预备64后512次counter+epoch Reset局部managed0B。
报告copy/projection/JSON明确非0GC，不自动采样；未跑Play/完整M0/Profiler/GPU/设备。
既有central9行新增、新report199行、新fixture309行；原上传/gate/模拟/排序/资源不改。
父M-03保持OPEN，下一先具名确定生产采样窗口/同布局baseline，完整链0GC/dirty/1000AI/Android待验收。
最终Ledger/原Editor/链接/diff/protected及准确after见报告与Operation，恢复仍另授权。
