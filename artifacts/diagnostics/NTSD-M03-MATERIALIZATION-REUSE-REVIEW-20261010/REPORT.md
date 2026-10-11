# M03 Mesh物化与上传：独立验证与有界修复方案

Task：NTSD-M03-MATERIALIZATION-REUSE-REVIEW-20261010。
诊断Change：NTSD-M03-UV-DIMENSION-REVIEW-001。

结论：BOUNDED_IMPLEMENTATION_HANDOFF_READY_WITH_OPEN_PERFORMANCE_GATE。形成一个明确的生产候选接入方案：利用既有canAppend段边界，只有真正追加quad才按段复用纹理尺寸，新段首quad/custom resolver/Strict保留原读取路径。可以交执行会话实现并验证；不是生产已优化、不是所有微测布局通过，也不是M03父项完成。准确指令见execution-handoff.txt；实际派发成功与新turn以dispatch-receipt.json/dispatch-status.json为准，尚无回执时不能声称已发送。

## 当前源码事实与范围

- BattleCentralRenderSystem.MaterializeLatestPublishedFrame / BattleCentralSubmission已有同一Unity frame、publication与显示alpha去重。8项原Editor定向测试新鲜通过，不能再次将已完成消重当新优化。
- CaptureFrame复制到读lease为0的独立submission；publication保持只读。MaterializeCommands还读取camera/viewport/视觉catalog，DisplayMotion会修改捕获命令坐标。跨publication缓存整批命令需要额外失效与无累积偏移证明，本轮不实施。
- BattleDynamicMeshBackend.Build已经用IsCompatible/canAppend判定同一段：texture/material/variant/binding/page/slice规则明确，unresolved也会断段。WriteQuad却对每个有效quad重新调用Unity Texture.width/height，半像素UV公式不依赖位置。
- preparedCatalogResolver为sealed BattleCatalogCentralResourceResolver，资源解析在同步Build内走已配置catalog；自定义resolver可能在Resolve中改变纹理，故不得跨其调用复用尺寸。候选仅复用现有prepared路径，不改变source/array/page绑定，也不解冻ATLAS专项。
- 旧同fixtureNTSD-H07-NATIVE-OWNED-COMPAT-SKIP的OFF report：89相机观测平均1992.056条命令、segment恒1；历史presentation计时90样本Materialize约4.156ms、ResolveAndWrite约2.116ms、Upload约0.413ms、其内WriteQuads约1.081ms。计时为历史elapsed且有嵌套诊断，父子不能相加，不能承诺这些时间全部可消除。该窗Driver GC失败仍保留，不阻断本次独立渲染数学诊断。
- 更旧Batch20生产窗口为SourceTexture2D，故不以array-only假设决定全部生产方案；当前candidate包含SourceTexture2D/array/page，验收真实prepared resolver命中。

## 实际运行与原件

原Editor PID39108 / Unity2022.3.62f3，复用既有6402桥；unity CLI noinstances不是Editor关闭。未启动第二实例、未进入Play、未切场景/改资产/设置。

1. focused-result.json：job b99cff7cfebb42cd9d90c22cc29d28e3，8/8真实通过：冻结帧预备、先capture再materialize、重复提交不重建、Reset/World切换、不反写publication的类别偏移及generation/relation/skipped tick/复活Lives高度。不同测试有各自模式边界，不把8项统称生产全链通过。
2. uv-test-result-01.json：总0；新文件尚未被scope=scripts导入，不算PASS。all刷新后.meta生成，编译/reload完成。
3. uv-test-result-02.json：确实执行1项但正分配校准失败，GetAllocatedBytesForCurrentThread对明确8192B数组返回0，成本0窗。uv-local-cost.json保留FAILED_STOPPED，不能利用stub零值证明0GC。
4. 工具纠正只发生在新Editor诊断文件：复用未修改的BattleScopedGcAllocationRecorder，前后1MiB正control/空control。单位TimeNanoseconds，Count为事件，raw不是bytes。uv-scoped-test-result-final、uv-latched-test-result-01、uv-segment-test-result-final各1/1代表组成功执行，不代表全部性能门通过。

三个有据不同版本都只跑一组，不同版本不可混为同一ABBA或跨运行因果比较：

| 版本 | 布局与测量范围 | 性能结论 |
|---|---|---|
| V1 两槽按纹理复用 | 3布局×OFF/ON/ON/OFF，384sample scopes | 同一/两纹理明显局部信号，但三纹理轮换平均退化14.81%，不采用泛用两槽方案 |
| V2 锁定首array | 4布局×ABBA，512sample scopes | 仅同一array稳定信号；其它布局不满足事前均值/尾部门，整体NO_DISCERNIBLE_LOCAL_SIGNAL，不采用array-only方案 |
| V3 按段清空准备状态的数学模型 | 4布局×ABBA，512sample scopes | 连续array/2D严格局部门通过；三纹理每quad换段平均退化5.0494%，超过事前5%，整体仍NO_DISCERNIBLE_LOCAL_SIGNAL，不准许无条件接入 |

每窗8warm+32sample，每轮4000quad；expected与float-bit校验在计时/GC scope外，计时无内部counter/hook，每次round清空状态。三个版本前后校准均响应，1408完整sample scopes无invalid/wrap/saturation且GC事件0；不证明native allocation、完整Backend/Driver或全Unity0GC。各41语义比较含UV微小/零负宽高、NaN宽、null/destroyed、普通2D/array与两轮间resize；不等于实际vertex/pixel覆盖。

V3四窗均值（单位ms/4000次独立UV计算，不是帧耗时）：

| 布局 | OFF1 | ON1 | ON2 | OFF2 |
|---|---:|---:|---:|---:|
| 单array连续段 | 1.184759 | 0.474331 | 0.507250 | 1.122766 |
| 单Texture2D连续段 | 1.164331 | 0.507084 | 0.618003 | 1.048906 |
| 两纹理逐quad换段 | 1.233059 | 1.128569 | 1.155788 | 1.188216 |
| 三纹理逐quad换段 | 0.977450 | 1.081128 | 1.119175 | 1.117091 |

完整P95/P99/每轮ticks/scope在原json。summarize_evidence.py独立重算全部均值/分位数、真实ABBA顺序、1408scope，并核真实job计数；输出analysis-summary.json。ABBA不是独占CPU或自动因果证明，V3窗口明显波动，不能把单个比例外推整帧。

## 最终修复设计与未验证项

微测说明连续段重复native getter可消除；负控说明“每quad先走cachehelper再决定命中”不能无条件采用。最终指令因此明确：利用现有canAppend为入口，新段首quad完全原路；第一次真正append才准备尺寸，随后同段复用。改变的是调用点的选择，不再设计第四版孤立微测。此最终接法尚未写入生产，也尚未有真实Backend成本证据，执行会话必须补足，不能继承微测PASS。

两int＋ready局部状态随Build失效，换段/异常/新Build/空frame不可泄漏；UV公式按原float运算，UV不同仍逐quad算边界。独立backend实例flag默认OFF，不改World/资源/camera/Upload/Index/透明排序/容量规则。只三个准确文件：Backend、现有Catalog resolver Editor tests、现有same-sample pixel tests。没有批准生产resolver、catalog、RenderSystem或全局配置改动。

所缺生产证据已写进指令的有限退出条件：真实prepared resolver命中；完整vertex/index/submesh/segment/bounds/diagnostics和publication一致；换段/无效UV/custom中途resize/Build间resize/Strict/4097/资源销毁恢复等边界；实际像素ON/OFF及反序负控制；一次完整Build含native upload的四布局ABBA，可靠同步GC校准与容量，连续布局收益、换段/Strict负控不得超过自身OFF范围。任何硬门失败停止，性能不明确保留OFF并结束，不重跑找PASS。通过也仅建议限定准入，不自动开生产默认或追加1800/Driver/设备认证。

## 保护与最终审计

仅新增一个Explicit诊断C#与Unity生成.meta；生产源码未改。Operation中的原6文档＋Ledger真实dirty备份与3次工具修订备份完整保留。analysis-summary.json核155项Rendering/Presentation/Scene/Settings保护文件无漂移，Menu同一场景clean/8roots，临时Texture自有创建并finally销毁。Console末5条为已知桥断连Exception，不是C#编译错误；精确新测试实际执行通过证明对应程序集可用，不声称Console清空或无错误日志。

Validate-ChangeLedger和scoped diff结果追加见validation-final.json。Task/Change的VERIFIED只修饰诊断执行与证据复核；生产性能仍OPEN。H07~12ms/producer/95/历史失败保持，M03父项及33ms/Android最终门不关闭。下一步只执行本M03具名指令，不自动推进H08或旧队列。

## 实际派发与收尾
2026-10-10已将完整execution-handoff.txt发送既有处理优化方案会话（01a1201b-7930-7c93-8c84-5bc7baada43f/local），新turn 01a12535-e296-7631-935b-d964417ebd67 active/inProgress，cursor :83，error null；不是仅草案。监测automation已ACTIVE，每5分钟只读、例行安静、任务终态审查后暂停。Validate-ChangeLedger PASSED，scoped diff exit0；旧全仓历史warnings保留。生产结果待执行会话，不自动开启候选。
