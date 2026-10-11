# H07角色输入完整流程有限补充诊断
Task ID: NTSD-H07-INPUT-FLOW-DIAG-20261010
状态：IN_PROGRESS
授权：用户当前直接要求“那我还是建议先全部检验清楚后，再交给另一个会话去执行”。本讨论会话检测当前H07输入点，不派发、不实施优化。
范围：兼容守卫全边界静态/最窄focused；现有42槽输入分段、查询访问计数、同300tick状态对账与当前完整Driver GC调用栈。生产World/Query/AI/writer/动作callee不写、不读活跃Q06体。
脚本只两Editor域：Suite新增独立一窗冷入口/状态/路径；H07InputCostDiagnostics复用已有observer和捕获器，固定300行、42槽/访问数和300冷marker，默认不开启。
唯一新增窗口：Role/DataOrientedCanonical/1000Sakura/seed1314149188/120warm+180steady，同fixture单窗全输入细分ON；原6分支/record/producer/95等OFF。这是独立诊断取证，不续旧ABBA，不改旧失败，不作正常性能或0GC资格。完整GC校准/覆盖及事件均保留，任何非零事件仍FAIL；窗口后停止，不自动追加另一个窗口或修分配。
现有CPU捕获器在tick124后启动八帧/45秒有界capture并finally恢复，300冷tick marker映射；不能截滤GC声称通过，raw0回调时只允许解析同raw、不重跑实景。Profiler开启区间明确记录，耗时只能作为有扰动归因线索。
最小focused：现有4个输入用例及canonical可用性与progress读取同guard的valid/null/unbound/released/replaced身份、profile例外；工具请求仅一窗和退出恢复。
有限出口：可实施范围、必须保留边界、真实可解释波动与未证原因分别交付；不足即明确列出剩余缺口，不虚构“全部清楚”。性能/Android最终门不变。
回滚：只具名诊断增量，通过before字节核对；无删除/覆盖无关dirty，恢复需遵守用户授权。Owner在原Suite有序关闭前释放observer与capture，AssemblyReload也幂等恢复；无新增生产对象。

## 最新有限出口
状态：SCOPED_DIAGNOSIS_COMPLETED_WITH_LIMITS / NO_PRODUCTION_HANDOFF。
原IN_PROGRESS为前时点；当前两Editor工具/编译/8focused/唯一300tick采集/同raw解析及恢复已完成。完整Driver2event，0GC FAIL，timing spike root UNRESOLVED；不能称用户要求的全部根因检验已经完成。准确事实、可讨论guard边界、未解决原因及下一必要证据见同Task REPORT。没有第二窗/生产实现/任务派发或自动后继。

## 最终交付检查
2026-10-10原工作树执行Tools/Validate-ChangeLedger.ps1：exit0 / Change ledger validation PASSED / Records1402；历史Record声明不在当前diff中的警告保留于change-ledger-validation.log，没有新增未覆盖脚本错误。git diff --check：exit0，仅既有/已声明文档LF→CRLF提示，无whitespace错误。两条检查都不等于性能/0GC/Android通过。
