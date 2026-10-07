# 第29批 H07 现有 Role-aware collector 实际1000AI准入

状态：PLANNED。需求为用户报告0.7FPS并要求未优化好继续；第26批有效smoke逻辑431/467ms，正式诊断CandidateCollect426.55ms/92.75%。本批不再做同构GC观察，只对已经存在的ForceRoleAware候选做实际完整Driver工作负载对比。六项首阶段范围及第13节次数更正保持，不重开已收口H06有限评估或其余28项，不新增算法/切生产默认。

唯一代码路径：Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs。复用原Suite owner、Preparing、StartRun/StopAndCleanup、11阶段关闭、Session reload、clean Scene恢复与新建输出；新增独立Batch29菜单与两固定candidate request，原Batch26六request/strict workload/GC拒证规则原样。准确候选为formalCollectorMode="role"（现有parser→ForceRoleAware），不强制Direct/Sweep/tree或改变阈值。

冻结矩阵：Dispersed1000及Combat1000各一次120warm+180sample，1000真实activeAI/baseRoster、seed0x4E545344、spawn25、DataOrientedCanonical、33ms/3ms/max2、正常renderer/sounddispatch、完整Driver和原计时开关，其他请求字段与第26批对应有效smoke逐字段相同，仅collector和新outputPath变化。保留requireZeroGcAfterWarmup=true及原harness/workload fail-closed，不将旧不可靠counter0B认证为0GC；失败原件保留、不自动继续第二窗或重跑。正式120+1800矩阵未被这两候选窗替换，H07不因此关闭。

原基线直接复用第26批00/01 smoke的request/report/final-checksum及源/内容指纹；启动前核对生产保护与现有源码，不能因为HEAD同就认为全部依赖同。候选终态离线比较tick300的schema与input/RNG/metadata/World/slots/aRest/vRest/stats/events/overall全部hash及原JSON，不忽略任何首差。只末tick摘要不能证明每tick/native正式等价，因此即使同也禁止生产默认推广；收益或一致性不成立即不采用，不反复参数搜索。

测试先行：同文件三新增request测试（两固定request完整JSON只允许两字段不同、越界拒绝），首次必须RED；实现后只运行这三项+原六request回归，共9，不重跑87/全角色/27CPU桥。原Editor2022.3.62f3/PID19040/6401，单一saved Menu clean/非Play/无测试/编译/压力owner后才修改和启动；startup600/run7200秒观察截止只是诊断，不以超时擅自重启。

证据：逻辑/collector/visible分位数与原基线差异、角色collector实际tick计数、activeAI下限、capacity拒绝/关闭残留、同tick各hash及依赖指纹。Editor速度噪声和最终checksum范围按实际说明，不称Android/120FPS/GPU/全域0GC。H11第28批严格FAIL保持，具体调用点未知，不将早期事件归因生产或删scope。

脚本前独立Change NTSD-OPT-H07-ROLE-COLLECTOR-ADMISSION-029及Operation同本Task ID，保存唯一Editor脚本/准确进度文档当前脏字节。共享stress request不写不删；现有terminal只允许原已核实本Goal Batch26 owner的新终态替换，先备份SHA，未知owner则拒绝。旧report不覆盖，新windows-01仅CreateNew。Scene/资源/settings/Q06/Gen/Plugins/Server/EXT1/Mono/ATLAS均不改。恢复需另获准确批准，不能用HEAD覆盖脏工作。
