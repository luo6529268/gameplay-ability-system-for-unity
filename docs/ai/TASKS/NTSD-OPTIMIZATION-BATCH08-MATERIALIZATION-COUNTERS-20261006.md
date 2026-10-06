# 第八批 M-03 显示物化请求分类 Task Contract

Task NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006 / RUNTIME_PENDING；Change NTSD-OPT-M03-MATERIALIZATION-COUNTERS-008 / RUNTIME_PENDING。
用户授权：本轮“开始执行下一批的任务”；按M-03首项补诊断，不扩大到优化算法/专项M0。

## 原状与范围

现有MaterializeLatestPublishedFrame按同Unity帧、publication及displayAlpha跳过重复请求；
PrepareFrameImmediate内还可复用已发布plan。当前缺少publication变化/alpha变化/完全重复分类。
queued publication identity包括world/frame/tick/mode，不等同于逻辑tick编号。
新增计数只观察有效CentralOnly queued显示请求；按前一有效请求的world/version/alpha分类。
分类不驱动渲染分支、不改变既有条件、时钟或调度；每次分类仅标量与引用比较，无分配。
物化attempt只表示调用PrepareFrameImmediate，不证明Build、上传、成功submission或GPU draw。

准确生产路径：Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs。
准确符号：BattleCentralRuntimeDiagnostics及新sample-kind枚举；
MaterializeLatestPublishedFrame、ResetRuntime。其他生产文件不改。
测试：Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleMaterializationCountersEditorTests.cs/.meta。
八个已有脚本/文档精确清单及dirty备份见Operation before.json。

## 合同与生命周期

request总数/三类、attempt总数/三类、同Unity帧复用/同样本复用/忙碌跳过分开；
首次请求或world/version变化归publication，只有identity不变alpha不同归alpha，其余归重复。
三类按前一请求比较，不按上次成功Build；计数不会证明实际复用成功或像素提交。
仅主线程显示物化入口写入；Queue/worker不新增写入计数或锁。
现有静态diagnostics持有最后观察world引用，ResetRuntime在既有shutdown阶段5清空；
不新增owner/资源/worker/lease，不重排十一阶段关闭。
每帧诊断reset不得清这些累计计数；ResetRuntime必须清全部计数和观察baseline。
0GC断言限定新增scalar observer helper，不替代完整物化—上传—录制—提交合同。

33ms/3ms/max2interval、pass/RNG/checksum/publication、插值取样/排序/segment/fail-closed、
slot/lease/GPUconsumer证明、资源表示不改；不读取Q06活跃BattlePresentationShadowBuild方法体。
PERF/ATLAS/MONO/EXT-1正文、Scene/资源/Settings/InputActions/Server均不修改。
EXT-1 PROPOSED / MODIFY_REQUIRED、MONO USER_HOLD保持；不启动专项M0/A4/真实Profiler/GPU测量/Play。

## 验证与回滚

test-first reflection fixture：RED在缺少新diagnostics/helper处失败，再最小生产实现GREEN。
覆盖分类/同tick不同版本/world切换/wrap/准确alpha/attempt与request隔离；
集成验证同帧/同样本/busy gate和forced cached-plan attempt，避免资源manager自动创建。
原Editor PID19040/TCP6401仅compile和具名EditMode；新fixture及既有182项限定回归；
新增helper预备后的局部managed0B；Menu clean/非Play、CS0、备份/211保护SHA、Ledger/diff/限定链接。
实际Build按类别计数/动态Play alpha/完整链0GC/1000AI/Android及真实收益保持开放，不把此批诊断当优化完成。
另获授权方可回滚：从精确dirty before只逆向本批hunk，保留旧修改，不从HEAD覆盖。

[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006/RECORD.md)；
[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006/REPORT.md)；
[统一进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。

2026-10-06实际结果：有效RED20 missing helper/counter预期失败；GREEN20/20、旧182/182，去重202。
原Editor实际生产/测试DLL重编译及CS0；64预备后512次cached scalar request+attempt局部managed0B。
两个fixture编译问题仅测试中移除不支持attribute、改用现有MeshBackend修复；首次0项/reload保留不算通过。
实际生产只一个文件112插入/1删除（相对dirty before），原skip条件和Prepare算法不变；新20case。
仅request分类/Prepare entry及skip原因，不是实际Build/上传分组或新成功渲染证据。
211保护/8backup SHA保持；完整链0GC/生产动态alpha/真实基线/1000AI/Android继续开放，最终静态另见报告。
