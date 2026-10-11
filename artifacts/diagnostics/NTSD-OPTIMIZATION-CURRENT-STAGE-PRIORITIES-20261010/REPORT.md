# 当前阶段优先级更正与M03首次剩余范围评审

Task：NTSD-OPTIMIZATION-CURRENT-STAGE-PRIORITIES-20261010
状态：DOCUMENTATION_COMPLETE / M03_INITIAL_REVIEW_COMPLETE / IMPLEMENTATION_NOT_SELECTED

## 已完成的排程更正

用户明确Android真机/GPU认证、IL2CPP/ARM64属于后期。当前将战斗CPU、CPU侧渲染准备及驻留内存排在前面，AB部署、Android构建、设备/API/热态认证及发布留到后期。
34项原ID全部保留，原风险等级12高/14中/8低单列；M03原为已提升的高，不因M前缀改成中。当前排序为评审投入顺序，不是已证收益排名。
P1：M03、H08、H10；P2：M12、M05、M01、M02、M06。另父目标保留2、伴随保障6、已评审暂不实施2、用户暂缓1、后期15，合计34。
总表、34单项卡、唯一进度、决定/说明/依赖/新会话提示及authority/state/handoff同步。全部采用前置更正，旧正文与结果未删除。

## M03第一项已查到什么

1. 复用Batch13 metadata批量更新、Batch15 camera消重、Batch86共享绑定的限定证据。这些不是新的本轮收益；13/15原报告仍有RUNTIME_PENDING范围，86是所选CPU域通过，不能泛化完整生产/设备。
2. 当前BattleCentralRenderSystem.MaterializeLatestPublishedFrame（约615行）已有sameUnityFrame和same publication+alpha复用；PrepareFrameForMaterialization有同tick/mode/alpha再次守卫。不能把这两项已存在能力再当新优化。
3. BattlePresentationShadowBuild.MaterializeCommands（2647行）已有CommandsMaterialized早退和当前构建spriteCaptureCache；现有复用先保留，不无据新增跨tick缓存。
4. 当前每次需要物化的显示样本会沿捕获帧→MaterializeCommands→DisplayMotion.Prepare/Apply→backend解析写入→Upload推进。publication未变但插值alpha变化仍要更新显示位置，不能跳帧或取消插值；上传有限值检查/透明顺序/segment/fail-closed保持。
5. BattleDynamicMeshBackend.BattleMeshChunk.Upload（687行）对活动顶点做finite校验并SetVertexBufferData上传，metadata批量更新已经存在。仅看到全活动范围上传，不足以证明全部顶点都无需更新。

## 可复用的成本线索及准确限制

来源：artifacts/diagnostics/NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-20261010/windows/00-combat1000-off/report.json，已有千人OFF窗；本轮只读取JSON，没有新运行。以下均presentation scope90个样本，与逻辑tick180样本不同，不能相加到logic tick或冒称GPU时间：

| 已有计时段 | 均值ms | P95 ms | P99 ms |
|---|---:|---:|---:|
| Materialize/BuildCommands | 4.155968 | 5.543120 | 8.502043 |
| Materialize/Mesh/ResolveAndWriteCommands | 2.115957 | 3.059110 | 4.254906 |
| Materialize/Mesh/UploadChunks | 0.413000 | 0.497935 | 0.615224 |

源码当前BuildCommands计时包住MaterializeCommands以及DisplayMotion准备/应用，并非全部都是纯命令创建；ResolveAndWriteCommands是父段，ResolveCommands与WriteQuads是其子段，不能父子相加。历史数据是经过时间，不证明独占CPU成本或可全部消除。该OFF完整Driver的2个GC事件失败独立保留；此表不是0GC/收益/设备验收。

## 本项接下来准确处理什么

锁定原M03的命令物化与顶点准备成本，优先检查同一publication在alpha-only显示样本中，资源/拓扑/命令的哪些字段实际不变、当前capture/materialize如何处理及已有复用是否覆盖；仅把位置必需刷新与其它重复工作区分清楚。这里是待验证问题，不是已证实存在可直接上线的缓存方案。
先沿现有capture/frame所有权和命令构建链确认字段失效、排序/资源边界，再决定一个有限候选或必要诊断；在这之前不派生产任务，不开启新的性能窗。不重复13/15/86、不借M03启动GPU/Android验收、不自动解冻EXT1/ATLAS/Mono专项。任何同因必要跨文件改动形成准确范围后处理，不能仅因目录不同跳过。
退出条件：能够指出真实可消除工作和正确性/收益验证范围才进入实现；若只是必要位置刷新、既有复用或无可辨收益线索，则具名记录该子方向结论。当前尚无新生产改法或收益结论。

H07输入约12ms仍未解决，H07-LATE暂无必要优化，旧失败/暂缓保留；后期认证后置不降低最终33ms/drop/backlog/确定性/容量/适用0GC/十一阶段关闭要求。

## 实际验证

只读命令：Get-Content、rg、Python JSON提取；辅助侧审限4份既有报告，无写文件或Unity调用。主代理独立核现有更近H07 OFF报告和3份源码，不采用把Batch91称为最新窗口的表述。
文档验证：完整34唯一ID、12/14/8原等级、全部单项当前阶段与表一致、链接存在、44真实备份SHA、旧内容完整保留；结果见audit.json。git diff --check结果见diff-check.txt。
未运行Unity、测试、Profiler、性能测量或构建；没有修改生产/测试/资源/设置。未新增项目Change Record；本轮仅文档和Operation内部审计工具，不涉及项目脚本交付，Validate-ChangeLedger不适用。
