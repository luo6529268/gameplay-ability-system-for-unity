> **2026-10-10 最新派发实态：独立评审 SCOPED_REVIEW_COMPLETE / HANDOFF_DISPATCHED；已发送 NTSD-H07-INDEXED-NEAREST-CURSOR-REUSE-20261010，执行会话turn01a1246d-a4b3-78a3-8fdd-edaffb253f18已active/inProgress，无error，cursor76；原automation已ACTIVE。准确两脚本/不接生产/有限局部对照范围见execution-handoff.txt及dispatch-receipt.json。下面READY_FOR_DISPATCH为派发前历史；没有C#、Unity或收益通过结论。**

> 评审状态：SCOPED_REVIEW_COMPLETE / READY_FOR_DISPATCH（2026-10-10T14:08:39.757371+08:00）。具体候选/出口见REPORT与execution-handoff；派发尚待回执。下方 IN_PROGRESS 保留为建立时历史。

# H07 输入阶段：Indexed 最近目标查询定向评审

Task ID：NTSD-H07-INDEXED-NEAREST-REVIEW-20261010
状态：IN_PROGRESS / READ_ONLY_CODE_REVIEW
父项：原 H-07 / H07-INPUT；不增加父项或历史批次数。

## 用户需求和授权

当前讨论会话用户明确要求“那你来处理即可，然后再让另一个会话去处理”。本会话先复用现有检测、核查输入阶段中可等价减少的工作，形成准确方案后直接发送既有执行会话“处理优化方案”（01a1201b-7930-7c93-8c84-5bc7baada43f / local）。不重复请求发送许可。

## 本会话范围

只读核查 SimulationAiDecisionModule 的 IndexedCanonical 实际入口、BattleCharacterInputWriter/Store 的调用关系、AiDecisionKernel 最近目标调用点、AiSensingKernel 的 Indexed Ground/Air 两段及对应索引构建/测试。复用既有 CPU 诊断的同窗成本，不启动 Unity、测试、Profiler 或新的性能窗口。

允许在本 Task 新目录保存评审报告、离线算法等价模型证据、源码指纹和准确后继指令；允许按文件操作合同备份并追加当前 H07 方案及唯一进度/恢复入口。离线模型不是 C# 编译、Unity 测试、生产收益或正式 EXE 验收。

## 待回答问题与有限出口

1. 找出具名重复工作、读写所有者和失效范围，不能只依据耗时选择任意重构。
2. 明确不改变索引/候选访问顺序、相同距离选择、ground facts 与 air override、字段及 RNG 的候选改法。
3. 核对现有索引与既有候选，拒绝重建空间索引、跨 tick 缓存、跳过仍有副作用的查询。
4. 交付一个准确最小候选及 C# 正反例/局部成本验证，或明确有据不实施。未获得实际收益不承诺节省毫秒数。

## 保持的边界

不读取活跃 Q06 方法体；不改生产、测试或工具脚本，不改资源/Scene/Prefab/Settings/InputActions/Server/Gen/Plugins。producer 复用 USER_DEFERRED/OFF、兼容跳过候选 OFF、95 OFF，H06 不续办，旧 Goal/34 项自动队列不恢复。输入/双 RNG/full checksum、33ms、publication 只读、透明顺序/segment/fail-closed、容量/0GC、十一阶段关闭和 Android 最终验收保持。

代码实现由后继具名 Task/Change/Operation 承担。本评审不把现有全 Driver GC 失败改写为通过，不把局部候选完成写成整个 12ms 或 H07 完成。

报告：artifacts/diagnostics/NTSD-H07-INDEXED-NEAREST-REVIEW-20261010/REPORT.md
文档更新前须先登记同名 Operation、逐文件真实 dirty 备份/指纹；不得以 HEAD 覆盖现有工作。
