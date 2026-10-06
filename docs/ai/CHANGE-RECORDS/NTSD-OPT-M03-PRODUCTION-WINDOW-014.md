<!-- CHANGE-RECORD
id: NTSD-OPT-M03-PRODUCTION-WINDOW-014
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs
authority: user next optimization batch; bounded original Battle presentation observation only; formal336B44 unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006/REPORT.md
-->

# M-03 原 Battle 生产窗口与两轮关闭重进验证

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006.md)；[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006/RECORD.md)。
脚本编辑前PLANNED。现状：子批13局部metadata优化已A/B，真实production window未验。
只新增Editor显式验证入口，借助真实saved Battle Driver/RenderPass/publication/slot/现有计数，
不新增生产统计、不强制物化、不改变逻辑或渲染算法；原Scene自然小roster两轮96tick观察。
已有tick/Update/presentation/PlayerLoop分配分开读；新增整相机managed envelope包含URP/其它callback，
不是单中央方法或完整链0GC。诊断快照/JSON在窗外有分配。
停止接收观察在EXITING；按现有11阶段关闭硬门；domain reload保存EditorSession非热状态，无新Runtime owner。
副作用仅Editor Session/Scene打开状态和Play的临时runtime及唯一新证据；Scene asset、配置/资源不写。
不可回退边界及恢复、compile/focused/runtime/未验证门见Task；EXT1/MONO/ATLAS/Q06边界保持。
尚未写/编译/运行，不把提案或Editor通过当设备/全域行为通过。
CODE_WRITTEN：新增一个Editor观察入口与独立GUID，原production文件无编辑。
每轮最大2048记录硬限，显式开始/两轮自然96tick；只camera回调读取plan/slots/scalars/native descriptor，
无强制Build/新lease/CPU回写。报告导出与场景生命周期窗外执行，compile/运行待验。
静态签名复核发现既有MaterializationReport.ToJson只有无参，探针改为无参；不改生产API。
首轮refresh仅reload、Editor DLL未更新：新meta误写33hex GUID，Editor拒绝导入新script。
修为唯一32hex GUID 8b618d81574442b3b0df7cd31569f9af，再compile；不删除meta或任一用户文件。
两次idle/CS过滤0不能证明compile：以DLL时间与实际具名测试/菜单识别作为后续证据。
COMPILE_PASS：新Editor DLL23:30:53、6173184bytes/SHA3324C41F...，production DLL保持B2B46521...。
首组77e81fe0c9ea4df88701dfb58d070ce1 82/82 Passed：LatestFrame13/SubMesh6/Report58/MemoryBoundary5。
另两类namespace误写，未命中，不声称执行；第二空筛选job也保留。已查真实NTSD.Test后单独补跑。
补跑0166abb...发生Enter/Exit域重载后Editor回Menu clean/nonPlay而bridge job孤儿RUNNING，最终case输出缺失。
该补跑状态UNCONFIRMED，不称通过/生产失败；保留raw和旧trace before16byte。只清自己已孤儿的bridge SessionState，
不终止Editor或删文件；下一改用显式probe菜单运行独立自然窗口，非重跑旧job。
菜单前新增真实TestRunnerApi.IsRunActive硬检查（本地TestFramework1.1.33源码已核），
metadata clear不能代替该检查；并明记Editor collection/playerLoop hardgate支持false。
最终guard版Editor DLL23:42:00/6174208/SHA3A899B46...，production程序集不变，CS0。
仅16 capacity与3具名pure motion追加筛选（不再包含两个UnityTest），不会重新覆盖旧trace。
6609b86402e64b6f9bc9403d40cc5111 19/19 Passed/1.3532022s；与前82共101具名通过。
原Menu idle/clean后只打开原Battle clean/11roots，不保存Scene；菜单启动本批两轮自然96tick观察。
真实TestRunnerApi.IsRunActive guard通过才能EnterPlay；回传孤儿的两旧UnityTest仍UNCONFIRMED。
首轮natural01真实tick8→104/135samples，production counter window270Build（publication94/alpha176），
2实体/4总命令、stride44、0growth/0拒绝/0失败、已有分配与camera envelope/observer全0B；关闭三残留0/Scene clean/SHA同。
探针FAIL仅双slot必须end-camera同时观察的错误断言；当前TryGetReusableBackend排除current、交替slot，
stage PresentLatestFrame force Flush与camera物化每camera两Build使end-only采样只见一个。未证生产错误。
v1源码与FAIL保留，新路径corrected-02只改观察：begin/end计划分别记，slot允许实际1～2。
真实重复物化是下一批候选：不能无证据删早期消费者或改变采样时刻/latency；本批不改生产链。
落实STARTUP600s/总900s，失败保存已有samples；域重载失败退出前只从现有对象找Driver、无自动singleton创建。
corrected-02 cycle01 PASS：128samples/256Build(publication96/alpha160)，begin/end generation逐帧+1，
两slot实际被观察；关闭三残留0、Scene clean/SHA同。自动cycle02未见EnterPlay或文件；衔接原因待确认。
Editor仍idle/nonPlay时先备份v2，再只增具名Second Cycle When Idle入口，前置第一份PASS/第二份不存在，
真实Runner停止、原Scene clean；仅可接续cycle02 STARTUP或无session，不改camera热逻辑/生产代码。

2026-10-07 VERIFIED（仅本Editor观察入口/两自然窗口；以下PLANNED/中间状态为历史，不晋升生产父项）：
最终Editor DLL6175232bytes/23:58:13/SHA53AAAC6511A651A89F95F2935D6EDDA3A9225F3CC9FC9395794EC8FDECBE6690，
脚本SHA8D9A6901975072636E7474878764159B066115B572E7BDA5EB3E8329D6E1F7D3；meta32hex保持。
cycle01使用前v2观察逻辑编译版本，cycle02新增显式idle接续菜单，camera逻辑不变；编译指纹分别保留。
原Editor101确认具名去重case Passed；两个旧UnityTest缺最终输出UNCONFIRMED，不计入。
corrected-02 cycle01/02 PASS：各tick8→104，samples128/136、Build256/272、pub96/96、alpha160/176，
repeated0/failed0/rejected0/zero growth，上传180224/191488bytes、stride44，两slot、隔离与合法bounds通过。
全部样本begin/end generation+1/slot0→1，stage force Flush与camera再物化实证；早期采样消费者尚待查，
不能无条件删Flush/改alpha时刻/承诺50%收益。本批没有生产算法修改。
分配计数各自0B，Editor硬门false，不等于全链0GC；CPU lease0不是GPU完成证据。
两次11阶段关闭三残留0、Scene clean/SHA同，恢复原Menu8roots/idle/nonPlay；父M03/H11仍OPEN/RUNTIME_PENDING。
447保护/七dirtybefore备份及v1/v2/trace额外备份保持；遗留trace字节同，原件与异常保留；HEAD/staged不变。
实际验证：已有Editor MCP refresh/run_tests两确认job、显式probe两菜单、Scene状态/输出JSON；
PowerShell只读SHA/links/git diff --check，Tools/Validate-ChangeLedger.ps1详报告最终审计。
未Unity第二实例、SelfCheck/Player/GPU capture/设备/1000AI/完整回放，没有旧任务重启或EXT1专项M0。
最终ChangeLedger exit0/1310 Records/7脚本COVERED/0error，全库4268warning、本项0；
静态447保护/7owned准确备份与3额外备份无漂移、links0missing/diff-check0、34=12/14/8；原件回链报告。

