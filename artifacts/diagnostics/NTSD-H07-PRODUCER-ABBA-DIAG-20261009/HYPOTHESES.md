# 预登记假设与判读
生产方法只在exact LF2Character/AI/基础字段仍current且无mutation hook时略过基础复制；LF2Character.RefreshRuntimeFromEntity仅调用base，base写身份/Frame/Wait/Next、计数/状态和Health字段；后续AI行更新不在skip分支内。当前源码与上次冻结的生产输入一致，旧15项字段别名/陈旧修复和逐tick输入双RNG资格可复用。没有发现可直接证明整tick回退的新增代码副作用。

H1：候选状态和耗时关联。若两ON持续高于两OFF、且实际case/skip/末态相符，支持当前测量配置下稳定关联；尚需专门证据定位因果，不生产修复。
H2：重复状态窗口波动。比较OFF首尾、ON中间的mean/P95/P99/max范围，以及非重叠phase；若状态内波动可覆盖开关差，说明此设计不能分辨小收益，保持OFF/搁置。
H3：逐角色细分计时配置影响。新组所有inputBoundaryTiming=false，边界calls必须0；旧组两侧true。顺序/跨运行同时变化，不能将新旧组差异独立归因为关闭计时。

只有一组combat ABBA，每窗120+180独立初态，保留整tickStopwatch elapsed、可靠同步主线程Driver0GC/容量/11阶段。GC.Alloc0不是GC停顿证书，Stopwatch不是CPU时间或调度证据。进程累计CPU/内存快照仅环境观察，不识别tick抢占或热降频。
必要门通过才解释收益；任何结论本次不推广，旧不准入历史不抹去，完成即停。
