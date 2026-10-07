# 第28批 H11 实际分配事件的帧级归属

状态：PLANNED。第27批校准4/4及两个原CPU桥2/2复用，不重跑；原camera-01有效记录camera2/observer13event但只有汇总，不能定位发生在哪个相机/slot/前后observer。本批是已暴露缺口的最小归属诊断，不是生产优化候选或新父项。

准确唯一代码路径：`Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs`。既有recorder原样复用；不增加其他Profiler工具、调用栈抓取、全局设置、递增容器或Runtime manager。只扩展已预分配的FrameSample值字段，记录每个相机完整scope及前/后observer的event count和valid标志；scalar赋值仍在已有完整范围内，不能排除前几个camera或子链。窗口结束在scope外校验字段求和与总数相同，导出原完整frames，足以判定事件与哪个相机/slot/阶段相关；没有堆栈就不声称已定位到最终生产方法。

新增隔离第28批菜单/输出，原一次1800distinct camera矩阵不变；原Battle自然运行、tick8后观察、Foot/Health活动、两slot、CPU录制/执行、容量和lease/有序关闭/Scene保护门均保留。正常Unity配置、资源和Q06方法体不读不改。前后正反校准、固定256容量/溢出拒证、GPU完成边界不变。

脚本前另建Change Record、Ledger/STATE/handoff/index，并保存唯一脚本及精确进度文档的当前脏字节和保护清单。原EditorMenu clean/idle/无在途测试和窗口才可编辑。编译后只运行新增元数据一致性检查（窗口结束真实字段求和），原1800camera一次；不重跑helper四项/CPU桥/1000AI/全历史。没有新NUnit行为算法，不拿compile代替实际归属证据。

若仍有事件，严格0GC仍FAIL；字段证明归属不足时保留待确认，不猜测、伪造冷启动例外或盲改生产。若无事件，也只本次限定scope通过，不撤销第27批失败及其具体原因待确认。只有当前结果可解释并满足H11必要范围才能收口。第26批1000AI性能首热点426.55ms/92.75%仍独立待优化，不能把这15event当0.7FPS主因。

验收/恢复：完整1800frames、有效前后校准、分配字段求和等于总计、活动辅助及两slot、原十一阶段三残留0、双SceneSHA同/原Menu恢复、保护副本、ChangeLedger和diff-check。只本批新CreateNew输出，旧失败保留，无共享stress request/terminal、删除/移动/弃改/push或专项门解冻。需要生产修复时先根据准确调用证据独立声明Change/path，不从本诊断扩大到排序/渲染架构。
