# 第八批 M-03 显示请求分类检验报告

当前 RUNTIME_PENDING / REQUEST_COUNTER_FOCUSED_PASS；父M-03 OPEN，ANDROID_NOT_CERTIFIED。
[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006.md)；
[Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-MATERIALIZATION-COUNTERS-008.md)；
[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006/RECORD.md)。

## 本批结果与计量边界

唯一生产文件BattleCentralRenderSystem.cs，相对dirty before新增112行/删除1行；
新增BattleCentralDisplaySampleKind及diagnostics的11个long计数、3个scalar helper。
原same-frame/same-sample predicate、ResolveAlpha取样、Prepare/Build/上传及提交算法不变。
有效CentralOnly queued request以“前一有效request”的world/publication version/displayAlpha分类，
首次/world或version变化优先归publication；同identity而alpha改变归alpha；其余重复。
queued version不是逻辑tick，同tick不同publication仍属于publication变化；没有新增逻辑计时。

| 类别 | 计数 | 含义 |
|---|---|---|
| request | MaterializationRequestCount及三类RequestCount | 有效CentralOnly显示入口请求，包含原gate跳过 |
| Prepare attempt | MaterializationAttemptCount及三类AttemptCount | 即将调用PrepareFrameImmediate，不代表实际Build/上传或成功渲染 |
| 原return原因 | SameUnityFrameReuseCount / SameSampleReuseCount / ReentrantSkipCount | 同Unity帧/同样本/忙碌跳过；不保证返回plan有效或已成功复用像素 |

forced cached-plan也可attempt，但不进行Mesh Build/上传；各计数不能换算为GPU draw/batch。
只有主线程显示入口写，Queue/worker不写新计数；累计跨per-frame诊断reset保留。
ResetRuntime按既有shutdown阶段5清全部计数和最后观察world引用，无新owner/resource/lease。
没有production导出接线或按类别实际Build/upload计数；不能把冻结benchmark workload当生产动态样本。

## 实际验证

现有原Editor PID19040/TCP6401，Unity2022.3.62f3；只refresh/compile和限定EditMode。
新增fixture不创建Camera/manager/材质，synthetic cached plan没有submission，不声称实际渲染通过。

| 证据 | 实际结果 |
|---|---|
| fixture编译修复 | 首次NUnit不支持NonParallelizable的2个CS0246；再误用Backend的2个CS0117。仅移除标记/改为现有MeshBackend，原件保留 |
| 首次job 673c93b2a4384b899aa7fae5f43cffda | 旧DLL实际选中0项，不算RED或PASS |
| 有效RED f478898e79d04960b658409c5c9dd45d | 完成20项、20条missing helper/counter失败；最终result=null，保存observed failures，不伪造summary |
| actual compile | production DLL21:36:33、test DLL21:36:35晚于生产源21:36:25；Editor CS0；reload retry原件保留 |
| GREEN1495592fcaf448968d18c1f57f6fd727 | 新20/20，0失败/跳过 |
| REGRESSION01af1db25eb243fe873421f9c238d973 | 旧182/182，0失败/跳过；旧168加子批07新14，限定benchmark方法不运行真实collector |
| 去重 | 202个具名case，不用全suite total约9138充当本次覆盖 |
| 局部managed分配 | 64次预备后512次cached request+attempt scalar helper当前线程0B，反射/delegate创建/断言在窗口外 |
| post Editor | 21:42 Menu root8/clean/idle非Play，CS0；没有Scene切换或第二Editor |

覆盖版本wrap/world切换/精确alpha、request-baseline非lastBuiltAlpha、分类优先级/独立total、
same-frame新publication仍原gate、same-sample、busy、force cached无上传、Legacy/expectedWorld拒绝、
per-frame累计保留、ResetRuntime清计数及world引用。分类观测开销与真实Play alpha比例未测。
local0B不能证明整个物化—上传—录制—提交链0GC；测试通过不证明帧率或Android通过。

## 保持的边界与剩余门

34项仍高12/中14/低8，父项关闭0；H-11/M-03小批聚焦通过，其余32项未实施本轮优化。
M-03下一：实际Build/vertex上传按分类正确关联与生产报告出口、同布局生产基线、
完整链0GC、有证据dirty区/像素排序/资源生命周期及1000AI/GPU/Android验收。
A1未变chunk跳过仍未来设计，未降低上传次数；未启A2/A4实例表示。
未运行真实Battle Play/SelfCheck/完整M0/EXT-1专项M0/Profiler/Frame Debugger/GPU capture/Player/设备。
EXT-1仍PROPOSED / MODIFY_REQUIRED，MONO USER_HOLD；PERF/ATLAS/MONO/EXT-1正文未改。
33ms/3ms/max2interval/pass/RNG/checksum、publication/排序/segment/fail-closed、
slot/lease/GPUconsumer证明与有序关闭十一阶段保持。Q06活跃方法体未读取或修改。
Scene/Prefab/资源/Settings/InputActions/Server及原7批非owned内容保护，未执行Git写操作。

## 静态证据与恢复

21:39:56八份dirty备份与211保护SHA匹配；HEAD保持5a5cde34b9685739326b638f9c5550b69eda7ef6。
首次保护辅助检查误用sha256属性，按manifest实际expected字段重跑0差异，并非保护文件改变。
21:42:42 Ledger PASSED；21:43:01校正输出leading whitespace计数后PASSED1304 Records/17code，
4237既有warning/0error，未清理历史内容。21:42 git diff --check exit0，仅LF/CRLF提示。
最终限定链接/源哈希/完整after及保护复核另在static-validation.json和Operation after.json。
恢复需另授权，从before-backups只逆向本批hunk，不能用HEAD覆盖前7批或用户dirty。
文件操作VERIFIED仅表示留痕/保护范围，不晋升本RUNTIME_PENDING或M-03/Android状态。

21:45限定链接126无缺失，34条保持高12/中14/低8，新fixture20个[Test]/meta无尾空白。
最新状态同步Task/Change/Ledger/STATE/handoff与独立方案、进度及风险入口；历史事前记录保留。
最终static-validation.json记录新增索引留痕后链接复核、Ledger/diff、source/backup/protected SHA；
after.json只覆盖声明8个current文件、本批新文件与8份backup，不包含自身递归SHA。

最终21:47:01 Ledger再次PASSED1304/17、4237warning/0error；全diff exit0。
新增INDEX留痕后127限定链接无缺失；最终source与compiled manifest一致、8backup/211保护SHA保持。
