# 真实事件观察入口的最小追加请求

仍为同Task PRE-FLIGHT-01，未读以下新正文、未改生产。仅公开总dispatch计数无法证明053重触发、j4→043顺序或每请求在闭包内；轮后AudioSource扫描还会漏掉已结束的短cue。不能把总数增长充当具体技能事件。

已rg定位，提请主会话具名供证或允许窄读：

- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs` 46–64：仅`PendingSoundEvent`数据定义与`ISimulationSoundPresentationSink`接口；不扩任何World方法。
- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs` 1816–1833：仅`BindBattleSoundPreparationOwner`与`SetSoundPresentationSinkForDiagnostics`的owner/实例限制和恢复合同；不扩ApplySettings/Q06。
- 已冻结的`Assets/NTSD/Scripts/Test/Editor/H10ResidentClosureDiagnostics.cs` 3796–3824：仅旧`PcBudgetSink`事件转发测量适配；旧源码/recorder算法完全不改、不运行。若此范围超过PcBudgetSink类末尾，裁到其结束，不读下一个producer/emitter。

拟裁决：能否在新probe自己的类中用固定容量只读tee观察声音并原样一次转发给已prepared owner，不替换preparation owner、不改变聚合/消费顺序、不截断声音、不创建热字符串/扩容；stop前恢复原sink。若公开口不支持，明确缺口，不能悄悄新增生产hook。forward耗时/观察成本计入本Player、不扣除；自身0GC和恢复必须新资格通过。旧63scope只证明旧sink，不自动证明这个新tee。

本方案不要求所有producer技能动态穷举，也不新增测试进程；只是让已具名运行请求及技能代表有真实证据。表中行范围来自声明定位，尚未打开正文。
