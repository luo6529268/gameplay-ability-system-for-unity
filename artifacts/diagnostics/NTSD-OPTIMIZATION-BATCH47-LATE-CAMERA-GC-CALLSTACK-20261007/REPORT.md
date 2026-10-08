# 第47批迟发相机分配调用点诊断报告

## 当前结论

FINITE_DIAGNOSTIC_COMPLETED / CALLSITE_NOT_REPRODUCED / RUNTIME_PENDING。仅补足首8工具无法覆盖迟发事件的有界捕获能力，本次没有找到新的分配调用点，没有生产优化或FPS收益。H11保留43首次ordinal937的12event有效FAIL，重进PASS及本次0event不能消去它。H07最新千人性能仍未达。阶段4/6、有限产物5/6、34父关闭0、22—47共26已执行子批、Goal active；不以次数停止或按报告交付标complete。

## 实际验证

- 原PID19040 / Unity2022.3.62f3 / MCP6402。新17case有效缺接口/字段RED（job73abb86fc0bf4624995f9c227aa7d85d、共32case，17fail），旧15无失败；源owner当时未改。
- 初版GREEN job815ef33e66054a6898b8fbdfcc9a5d6f：50/50、0skip、2.7053455s，新17＋旧camera15、CPU10、literal8；旧三个热方法IL literals0。
- 采集后R1只修诊断历史读取保护：官方frameCount getter缓存为0时可能调用原生history setter。不能证明本次发生了丢失；只读既有缓存并将备份前移，0/未知拒绝，不初始化/改Prefs/native长度。一个新增保护测试并末次相机fixture33/33（job6eaf29e3755845e8a6a46ab01d1048c8，1.6230392s/0skip）。同包50＋33执行，51去重具名通过，不写成一次51/51新作业；旧18按不受新late setup helper影响复用。R1依据静态API证据直接补测试/实现，没有制造额外RED或重采。
- 修改仅三个声明Editor C#；既有生产collector/backend/default、模拟规则/资源/Scene/settings不改。当前窗口证据使用pre-camera-source-audit-01.json三SHA，R1后CPU owner与测试SHA另在terminal audit，不混称同版。

## 唯一实际camera-01

原Menu clean后正常打开saved Battle11roots clean，只派发一次。2026-10-07T14:02:03.3717743Z启动；预热完成后历史备份在14:04:28Z出现；14:05:12Z采集结束、14:05:28Z生产结果DONE。Play期间没有MCP查询/源码写入，只外部PID/文件观测，无第二次窗口或raw recovery（trigger0无需假解析）。

| 项目 | 实际结果 |
|---|---|
| 状态/范围 | DIAGNOSTIC_NO_EVENT；1800/1800完整camera，含首帧；不是0GC证书 |
| Unity帧/逻辑tick | 3301—5100 /10—1320；不是1000AI/正式1800逻辑tick窗口 |
| camera / observer事件 | 0 /0；invalid /stage invalid /unattributed均0 |
| 校准 | 两recorder前后四次：已知1MiB正例各1event，空例0，全部PASS；rawValue为时间不当bytes |
| 辅助/slot | 全帧Foot2/Health2、两submission slot |
| CPU draw | 11560录制=执行；不是GPU batch证明 |
| 容量/lease | 全1800 capacityGrowth0 /read lease0 |
| 有序关闭 | orderedShutdown true，objects/slots/borrowers0，原十一阶段序列未改 |
| Scene | Battle前后SHA253B2E...F9010同/isDirty false；正常回原Menu8roots clean/idle/nonPlay |
| Profiler | before/after全部相同、enabled/driver/binary/stack/CPU/deep均原false；producer最大buffer536870912B未变，不是history/RSS/renderer预算 |
| history | 300帧容量；8固定元数据ring最后ordinal1793—1800；旧history183362700B、新cpu-gc.raw35646140B独立保存 |
| 调用点 | triggerOrdinal0，未走正事件栈导出；真实触发后的raw→计数/栈对应仍待自然事件证据 |

cpu-gc-state.stopReason保留既有cold caller文字eight-completed-cameras；实际completedCameras与production-window都是1800，不据标签缩小范围。first/counted/fullScope同，不跳帧、不把GetAllocatedBytes旧0当可靠依据。原history备份证明保存到的内容，不据本轮文件断言其与未记录的更早in-memory全量完全一致；R1消除了初始化getter潜在副作用。Profiler历史raw不是Steady/Transition内存测量。

camera-audit-01.json核原结果/校准/每帧辅助、capacity/lease/draw及5原件SHA；pre-camera-source-audit、terminal-source-audit和editor-final保留准确源与保护。原43所有证据不覆盖。

## 留痕、安全与限制

9个准确current-dirty副本先于C#，42非写域与HEAD事前后核对；Q06只hash。pre-camera validator实际1343 Record/14代码diff exit0、4257历史warning/0error；git diff--check0。最终文档更新后另运行治理并追加真实结果。无删除、移动、Git discard/commit/push、新Editor、Scene保存、资源/importer/ProjectSettings/Input/Gen/Plugins/Server写入或EXT1专项M0/instancing。五个fresh摄像机原件与全部失败原件保留。

两次domain reload端口临时拒绝，原PID持续存活后原连接恢复，未重启/重复派发。一次source hunk重复闭括号在Refresh前纠正；两个只读路径定位拼错和冷投影错字段只造成工具读取错误，未写入假结果；实际compile Console error CS筛选0及测试执行证明编译。全局旧log中的历史CS不能当当前编译失败。git diff--no-index exit1只是有差异，非验证失败。

官方2022.3 API核对：[ProfilerAPI SaveProfile(void)](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/ProfilerEditor/Public/ProfilerAPI.bindings.cs)；[ProfilerSettings getter副作用](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/ProfilerEditor/Public/ProfilerSettings.cs)；[maxUsedMemory传输buffer](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Profiling.Profiler-maxUsedMemory.html)。未以Unity master签名替代已安装2022版本。

## 剩余门与下一动作

不重复本次无事件长窗，不由此猜测MCP原因或修未定位生产分支。H11旧迟发12event调用点仍UNKNOWN；事件触发工具已保留，未来真实有效事件才走栈/计数对应，未证明正分支运行。

下一转H07既有ordinary Brute残余候选收集成本：复用45分支成本、38短窗及46资格证据，从实际45—48ms collector循环选择有据且仍在批准路径内的最小候选，先准确Task/Change再修改，不重复计时审计/旧短窗或推广未授权default。最新P9589.403/98.277ms、drop694/660、logic GC UNKNOWN不变；本轮不新增FPS、120FPS/Android/GPU/native全域证书。仍未完成的阶段持续执行，已完成四项不重开。

## 2026-10-07T14:18:58.7414177Z 必要收尾核对

42保护、9准确dirty备份及HEAD全部不变；最终三个Editor源码身份见 terminal-source-audit-01.json，R1后没有重跑camera窗口。原Editor状态1791382733927为idle/nonPlay/noTest/noncompiling，原Menu单Scene8roots clean。最终validator exit0：1343 Record/14 governed code/4257历史warning/0error；diff --check exit0/无消息，validation-final-01.json。只完成诊断留痕，不把NO_EVENT升级为H11通过；下一H07实际热点候选。
