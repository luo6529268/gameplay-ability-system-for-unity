# H07 原动作路由与状态读取有限检测

状态：DIAGNOSTIC_EXECUTION_VERIFIED / PERFORMANCE_INCONCLUSIVE。只证明下述诊断实现与有限覆盖；不是生产优化通过。H07输入因果链评审仍开放，未形成新的实施派发。

## 问题与范围

沿 ProcessNativeSampledState -> RouteNativeGroundBuiltins 实际调用链检查。ReadNativeInputFrameState 对正式帧每次查 rawProperties["state"] 并严格解析整数；合法固定数据存在重复解释的线索。但直接替换为 frame.state 不等价：原读取缺失/无效返回 -1，转换器 typed 默认可能为 0，且帧字段/字典可变。不能把“热路径有解析”直接当成已证性能瓶颈。

只新增 Editor 显式诊断入口与 artifact 脚本；生产 Writer/Store/World/AI/FrameData/配置均未修改。当前 Editor 单实例，没有 Play、Profiler、完整 Driver 或设备测量。

## 真实执行与纠错

- execute_code 尝试因 CodeDom 命令长度失败，未执行测量；probe-result-01.json 保留。
- 初始测试 job18b13f675820494c90d2facdb01f2954 报 NUnit1/1，但夹具误用默认400槽，600角色注册被拒。local-cost.json 和 analysis.json 全部 INVALID_FIXTURE，不用于千人正确性或性能结论。错误由本讨论会话独立复核发现，fixture-v1-invalid.json 留证。
- 修正为 DesktopExtended1024，并检查每角色 RegisteredWorldForSimulation、objects/claimed1000/reject0。两次属性名编译修正仅发生在第二次测量之前，失败/备份保留。
- 有效 job b691f0d111a64695b34a4632f4880622：单个具名测试1/1 PASS，结果 local-cost-v2.json；独立解析 analysis-v2.json。
- 有效测量后仅加 NUnit Explicit 属性，避免有限诊断被全量回归自动触发/覆盖旧文件；方法体未变。最新 refresh/reload 已结束，Console没有C#编译错误；保留MCP客户端退出日志，不冒称Console完全无日志。没有重跑性能。

## 有效夹具与证据

三个合成状态：idle/walk/run。每个独立World包含1000成功注册角色，capacity1024/reject0，原始读取 RAW -> 合法同值 typed 对照 -> typed -> RAW；每窗8warm+32sample。固定输入、相同初态；外层整轮计时，反射/创建/序列化/断言/checksum在计时外。

typed对照仅改变本次创建的合法帧 UsesLoganFrameNumbers 标志以走已有读取分支，不是已实现的安全生产缓存。所有40轮完整RuntimeChecksum在同一合成状态的四窗一致；所有轮1000次route成功。全部384个测量轮当前线程 allocatedBytes=0、容量不变；12个严格整数读取语义例通过。对象/slots经finally均归零。测试框架临时场景在job结束后移除，scenes-after-v2-tests.json仅原Menu clean、8roots。

| 合成状态 | RAW两窗均值 ms/1000次 | typed两窗均值 ms/1000次 | 判断 |
|---|---|---|---|
| idle | 1.217159 / 1.100294 | 1.027913 / 1.017244 | 本组均值有下降线索 |
| walk | 1.548084 / 1.598463 | 1.421644 / 1.643606 | 两个typed窗口不一致 |
| run | 1.475681 / 1.946684 | 1.798406 / 1.664381 | 原路径自身波动已覆盖差异 |

全部均值/P95/P99由32个原始Stopwatch ticks、frequency10000000独立重算，保留 analysis-v2.json。经过时间不是独占CPU；不能以中位数/事后剔除尖峰改写结论，不能声称抢占/降频/GC停顿已确诊。没有可覆盖三种状态的稳定收益证据；不据此实施state缓存，也不宣布state解析完全无成本。

## 同一问题的跨模块审查

- Writer ReadNativeInputFrameState (1490起) 与 DatParser 转换/FrameData 生命周期直接相关，已纳入。strict parse 与 typed 默认值不同；缓存须处理正式资源重建、身份切换、共享零帧及可变字典，当前不能无条件提前解码。
- Ground builtins (572起) 不只是检测：包含run accumulator、方向、速度、动作/帧选择、计数更新。相同frame不保证可省动作写入；Next/Wait/runtime镜像可能需同步，AnimCounter本身也有副作用。
- NTSD28InputTwoPassModule (82-198) 按预处理/组合状态/路由/legacy投影/AI canonical同步/InputState镜像顺序执行。AI生产提交与动作路由后同步具有不同可见边界，不能仅因方法叫sync而合并。
- BattleCharacterInputStore (122-138,526起) 捕获41个字段，包含history/cooldown/combo/held/previous/AI目标/boundary。原字段多为直接字段，不是每次再抓取整个世界。PublishAiProjectionIfChanged 已比较三个投影值并跳过未变发布。
- LF2Entity.RefreshRuntimeSnapshotAfterCharacterInput (7349) 生产默认已是空方法，不能再次“删除整份快照复制”冒称新收益。
- SimulationAiDecisionModule.RefreshUnifiedExecutionRowAfterCharacterInput (1666起) 已存在no-pending/source-coordinate候选；当前源坐标和source state合同不能被强行省去。旧同窗该阶段约0.23646ms，仅历史细分elapsed；不借此重启95或换名重试。

旧12ms是完整输入链的细分计时经过时间，包含父子计时及实际行为处理；本检测不是Sakura千人混战，也不测AI/组合键完整流程。当前不能把局部差值外推为12ms减少值。真实有用的结论是：已检查相关模块，暂未证明该新线索能构成可准入的新生产方案；原H07目标仍未完成。

## 保护与后续边界

protection-final.json：2361份原脚本/Scene/Settings/Packages保护指纹0变化；只新增本诊断.cs/.meta。原dirty不回退。文档/脚本预修改备份及失败结果见 Operation NTSD-H07-NATIVE-ROUTING-COST-PROBE-20261010。

没有向“处理优化方案”派新任务，监测维持暂停。旧producer暂缓、compat未准入、cursor无收益、95及旧Driver分配失败各保留范围。只读/局部诊断不能降低生产确定性、完整Driver0GC、33ms/drop0/backlog、关闭或Android门。

最终审计：Validate-ChangeLedger第二次 PASSED / 1406 records；第一次两项artifact元数据错误保留后修正，validator本体未改。治理文档diff-check通过，新Editor源码check无空白错误。测后源码与测前逐字比较只新增Explicit属性。audit-final.json保存最终状态；主Task仍IN_PROGRESS，无实施派发。
