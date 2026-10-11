# H10 native绑定直接helper回调合同窄读

Task: NTSD-H10-NATIVE-BINDING-CALLBACK-COVERAGE-20261011
Creation status: IN_PROGRESS

最新批准：人类在讨论会话01a11fe2-bfaa-72f3-a9ed-c8260f63b193针对四个直接helper的窄只读提案回复“开始吧”，本轮具名指令据此授权R10–R13必要guard/return/直接调用/数据操作。不是整个Q06解冻，不允许代码、规则、工具、预算或资源修改，不追溯合法化旧越界片段。

唯一写域：本新artifacts/diagnostics目录，CREATE_NEW_ONLY，排他新建TASK、批准清单、指纹/Git保护、实际读取证据、JSON和报告。所有既有文件、C#、Scene、Settings、资源与旧报告零写域。无Editor/Unity、测试、编译、测量或子代理。

## 准确读取范围

- R10 Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs::WriteCurrentFrameId(int frameId)，声明1237。
- R11 Assets/NTSD/Scripts/Animation/Character/LF2FrameCache.cs::GetNativeFrameDataById(int frameId)，声明98。
- R12 Assets/NTSD/Scripts/Animation/Character/FrameTransistor.cs::SyncDirectFrameData(int frameWait,int frameNext,int directWaitCounter=int.MinValue)，声明80。
- R13 同文件SyncWaitCounterFrame(int frameId)，声明116。

只取判定是否间接触发state_exit的条件/返回/调用和必要数据读写；按声明限定区间，不输出大文件/周边帧、伤害、RNG、生成或关闭事务，无广域-C/-A/-B。小方法若必要行覆盖全部可执行语句，如实记录，不伪称只读单行。标准库按已有签名合同，不无限审查理论副作用。额外项目helper/属性setter若在许可外且无合法合同，精确记录并停止，不猜无回调或扩大读取。

新增基线文件：LF2FrameCache.cs预期SHA fe2fc55a5042628a4f1faa9643c526e9ca07b9df3cecaf8b404aeddb640c10c2；FrameTransistor.cs预期SHA29096ccdceffa9443ccba8cd8c5565fe86bfc5e2097336942dd64ae4e12e2f4e，以本轮实际SHA核实，不将其冒称上一Task120项保护。

## 判定与退出

复用上一获批binder/R5–R9有限行、合法resolver/常量与旧1000cue；R1–R4静态覆盖不重开。明确仅本条native绑定链的静态绕行/可达/未知及条件，不扩大所有模式。无运行不得报告漏声、可听或Player通过。若无已证具体缺陷，不提出生产补丁；如发现只给准确最小写域、行为、验证/回滚供独立审阅，不实施。剩余许可外符号只具名交回，不自动派下一任务。

前后核原SHA、逐行证据、HEAD/staged/tracked diff和Task外dirty。无代码改动，ChangeLedger不适用，无新增owner/cache/容量/关闭阶段。Menu UNKNOWN、预算UNDECIDED、218.313758MiB原账本、PCM/每局owned成果及历史失败边界保持，不重测、不改cue/格式或恢复Goal/队列/其他点。终态另存RESULT/REPORT，完成有限报告即交回。
