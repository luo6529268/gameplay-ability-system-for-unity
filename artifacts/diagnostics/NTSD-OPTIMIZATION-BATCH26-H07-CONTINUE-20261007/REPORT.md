# 第26批 H-07 授权继续

## 当前执行证据

最新追加：两个smoke均120warm+180sample、min activeAI/baseRoster1000、StoppedCleanly/valid/workloadValid/restored=true；Combat logic mean431.0003/P95691.91995/P99747.890417ms、dropped4337/maxcatchup2，仍未达性能。第一个Dispersed formal-01（120+1800）已实际开始预热，当前2/6窗口完成；不因已完成两个smoke而缩减原正式矩阵或宣称H07完成。正式4窗和最终关闭/Scene恢复仍待。

保护分类更正：live-protection-checkpoint-01.json原始diff中仅共享`Temp/NTSD_ProductionEntityStress.result`变化，这是Task/Operation预先声明并备份的Runner终态输出（当前PASS指向本批Combat报告），不是未知文件覆盖。分类原件live-protection-classification-01.json确认另外177路径SHA一致、原terminal副本SHA保持、10主备份保持及HEAD同；不删除旧178-path manifest或隐去原差异。最终保护和Suite退出在终态后核。

### 首个有效窗口（并非性能达标）

Dispersed1000 smoke已StoppedCleanly、harnessValidity=true、workloadValid=true，120warm+180sample，58次采样观察min active AI/baseRoster均1000。实际逻辑300step/build300、eligible=committed300000/fallback0、refresh600000/read300000；容量关键拒绝delta0。窗口cleanup对象/实体/slot/两池active均0，restored=true。Combat1000已正常开始推进warmup，旧第二轮pool拒单故障未复现；整个Suite还在运行，最终十一阶段及Scene恢复尚待。

当前固定brute collector基线logic mean467.5241ms / P95706.75826ms / P99804.877996ms，明显未满足33ms预算；droppedBacklogTicks4701/maxcatchup2。CandidateCollect mean429.1872ms，约占logic mean91.8%，因此本窗主要逻辑成本位于候选碰撞收集，而不能只归因中央渲染；不据此提前切生产默认。Editor visible mean1116.8773ms仅Editor帧；FrameTiming GPU83samples mean1.0663ms仅该API观测，不是capture batch/安卓证明。raw GC gate true仍仅未校准读数，Suite zeroGcPassed=false/UNKNOWN，不作为完整0GC通过。

当前1/6窗口完成，首阶段仍4/6交付；四formal与Combat终态待当前live suite继续，H07未优化完成。后续按本批有效报告热点选最小有证候选，不停于旧次数也不重复无变化失败。原件见windows-01和live-checkpoint-01.json。

最新运行状态：六固定request新建，原Battle Play的Dispersed smoke已推进到112/120 warmup（随后固定180 sample），尚无本批terminal；首阶段交付仍4/6、H07未优化完成、Goal active。live progress为运行中计数，report.json可能仍为初始Starting快照，不能将其0tick误判为运行未开始。压力/退出/性能结论在真实终态后补，不修改代码或刷新Editor打断本次长窗。

原Editor RED8项6预期失败，确认诊断Preparing入口缺失和双refresh错误。GREEN-01执行87项，一处旧Shadow JSON常量118000未同步，失败原件保留；只修120000。GREEN-02作业`c8f10c9591b94e47aa97cacff008e30c`已取得完整结果：87/87 Passed、0失败/跳过。真实压力/性能尚待启动，不把聚焦通过等同于H07完成。

修改仅三个diagnostic脚本：空Preparing World调用既有owner准备接口；expected pass由当前成功step数确定；authority/rollback refresh=2×read；Suite有效性包含harnessValidity，并把未校准GC记UNKNOWN。未改Driver/World/AI/pool生产规则，也未切collector默认。

ChangeLedger1322记录/8差异代码路径验证PASS、0errors，4277历史warnings保留；diff-check exit0。178保护文件/10当前脏字节备份SHA一致，HEAD8107196b稳定。原Menu idle/非Play已核。

旧Batch24 smoke逻辑mean443.8275ms、CandidateCollect mean408.9495ms，约占92.1%；旧harnessValidity=false，因此仅用于下一观察的瓶颈线索，不晋升证书或正式收益。固定六窗先恢复可信基线，后续按实证瓶颈决定优化方向，不反复扩展验证矩阵。

状态：IN_PROGRESS / WINDOWS_SUITE_RUNNING。用户解除原修复到限停点；累计次数保留。已取得原Editor编译及87/87聚焦证据，尚未取得压力窗口有效终态或性能合格结果。准确范围与六窗口见Task。

当前 H-07 未优化完成，不以诊断修复冒充性能达标；旧窗口harnessValidity=false/Combat启动失败及未校准0B均保留。次数只作为复盘检查点，无效方案根据证据换方向，不重复同构失败，不改变规则及专项门。
# 2026-10-07 首个正式窗口终态更正

本节为最新事实，下方运行中快照保留为历史。Suite PARTIAL / DONE，已完成两个valid smoke和首个Dispersed120warm+1800sample；后三个formal未执行。正式窗口boundary全局collection三代各1，ZeroGcGateFailed/harnessValidity=false，不发合格certificate。tick本身collection0/旧API0B不是校准证书，不把全局Editor边界collection归因选定表现链。实际logic平均459.889333/P95696.606875/P99853.699059ms；CandidateCollect平均426.549868ms，约占92.750546%，仍为brute模式，H07未优化完成。

最终十一阶段关闭true、objects/slots/borrowers0、两Scene SHA同、原Menu恢复；teardown.restored=true，StoppedWithResidue是报告状态名，不代表此例有活跃实体残留。原Editor已非Play/idle，无测试/编译。Goal仍active，首阶段4/6；可靠GC采样Task27仅已冻结，不改规则/默认/专项门、不自动重复同一整套失败窗。原件final-suite-checkpoint.json、windows-01/suite-result.json及02-dispersed1000-formal-01/report.json。

