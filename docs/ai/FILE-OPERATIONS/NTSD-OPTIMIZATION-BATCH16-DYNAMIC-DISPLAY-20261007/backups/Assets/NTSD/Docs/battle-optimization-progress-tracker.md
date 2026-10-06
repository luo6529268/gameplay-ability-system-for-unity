# NTSD 全部优化点实施进度总表

> 2026-10-07 子批15 SCOPED_CAMERA_MATERIALIZATION_PASS：[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007.md) / [报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007/REPORT.md)。14新+135回归共149去重Passed；原Battle before132camera/264Build、after143/143和118/118，Build/camera2→1、entity vertex bytes/camera1408→704。两次重进关闭三残留0/Scene不变，非整场/GPU/FPS50%；production与父M03仍RUNTIME_PENDING，专项门保持。早期IN_PROGRESS/RED为事前留痕。

> ID：NTSD-OPTIMIZATION-PROGRESS-001；建立：2026-10-06；更新：2026-10-07。
> 当前：IMPLEMENTATION_STARTED / ANDROID_NOT_CERTIFIED。用户已批准按优化文档开始。
> 本表是统一进度入口；[风险/优先级登记](android-mobile-readiness-priority-risk-register.md)、
> 独立方案负责问题与验收，不把实现存在或编译成功当作性能/设备通过。
> 当前：H-11子批01/02/03/04/05均聚焦通过；子批05新12/12、旧回归120/120，父项完整运行门待验收。
> M-03子批06上传计数聚焦通过：新18/18、旧132/132；完整链/收益/设备仍待验收。
> M-03子批07可选报告出口聚焦通过：新14/14、旧168/168；仅快照/导出，完整链/收益/设备仍待验收。
> M-03子批08显示request分类聚焦通过：新20/20、旧182/182（去重202）；原条件不变，实际上传分组/基线/完整链仍待验收。
> M-03子批09实际Build/API类别归属聚焦通过：新23/23、旧202/202（去重225）；未减少上传，生产分组报告/基线/完整链仍待验收。
> M-03子批10生产计数快照/JSON和受控窗口聚焦通过：新58/58、旧225/225（去重283）；显式非热路径，不自动采样；真实生产基线/完整链/收益待验收。
> M-03子批11受控backend基线：新12/12、相关旧26/26（去重38）；12组各1800采样，局部0B/零growth。1000命令单段与1000物理段CPU有差异，真实生产/完整链/收益仍待验收。
> M-03子批12阶段基线：8阶段+12控制20/20、相关旧34/34（去重54）；高segment的descriptor/native阶段已定位，生产未改；下一最小A/B，完整链/收益/设备待验收。
> M-03子批13局部A/B通过：生产稳定fully-active多submesh批量metadata已接入；改前26/26、改后20/20、回归71/71（91去重）；1000段无诊断mean降46～47%，非整场/GPU/Android。父项完整链/生产窗口/设备及专项门保持。
> 2026-10-07 M-03子批14原Battle两窗口通过：101具名回归、两次自然96tick/128+136camera样本、256+272Build；局部记录分配0B/growth0、关闭三残留0/Scene不变。已证同显示帧两个物化入口，下一先查早期消费者后最小消重；非120FPS/1000AI/GPU/Android证书。

## 状态和更新规则

QUEUED：在计划中，尚未实施；IN_PROGRESS：有具名Task/Change执行；CODE_WRITTEN：
代码已写；COMPILE_PASS：实际编译；FOCUSED_TEST_PASS：具名聚焦检查；
RUNTIME_PENDING：尚缺原Scene/完整0GC等门；VERIFIED：该项所有验收已闭合。
USER_HOLD或WAITING_DECISION/DEVICE不是其他独立小批的全局阻塞。
每次更新写日期、实际产物、证据、剩余门；不用主观百分比冒充实测。
未测获益保持未知。一个子批PASS不得把父项全部关闭。

## 总览

34项：高12/中14/低8；父项关闭0。H-11预热/容量小批、M-03上传计数/报告/显示request与Build归属及生产计数报告前置已聚焦通过，其余32项未实施本轮优化。
阶段0当前版本基线：受控backend/六phase与原Battle自然小roster计数窗口已交付，完整M0/高负载收益未完成。
子批14只观察；子批15已接入warm CentralOnly host/camera最小消重。现有149聚焦与自然小roster前后窗口不能替代1000实体、动态像素/首可见、完整0GC或设备验收。
禁止项/专项门：EXT-1继续PROPOSED / MODIFY_REQUIRED，无专项M0/instancing；
MONO B0-B9独立实施门、ATLAS资源/bank/预算/格式、Scene/Input Actions、发布凭据仍按原合同。
用户本次总体启动授权不取消这些明确单列的决策。

## 所有条目

| ID | 级别 | 简述 | 本轮状态 | 已有基线（非本轮成果） | 下一验收门 / 方案 |
|---|---|---|---|---|---|
| H-08 | 高 | 全量视觉预热与源纹理回退仍可能造成高驻留及启动峰值。 | QUEUED | OPEN / SOURCE_FALLBACK_EXISTS / PREBAKE_PENDING | [方案](android-mobile-readiness/h-08-prebaked-visual-content-memory.md) |
| H-10 | 高 | 全量音频预热与双声道 PCM 副本增加整局内存和启动成本。 | QUEUED | OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED | [方案](android-mobile-readiness/h-10-battle-audio-pcm-memory.md) |
| H-11 | 高 | 预热与多类硬限、resolver封口、已失效native Mesh再预热已补；其余缓存/全路径0GC仍待闭合。 | RUNTIME_PENDING（子批01/02/03/04/05聚焦通过） | OPEN / STATIC_ALLOCATION_PATH_CONFIRMED（改前） | [方案](android-mobile-readiness/h-11-presentation-capacity-zero-gc.md)；子批05新12/12+旧120/120→其余物化缓存/完整0GC/实际退出重进 |
| M-03 | 高 | metadata局部A/B与warm中央消重通过；实体顶点每相机帧上传减半，动态像素/完整链仍待验。 | RUNTIME_PENDING（子批06～15限定通过） | OPEN / INTERPOLATION_EXISTS / SCOPED_CAMERA_MATERIALIZATION_PASS | [方案](android-mobile-readiness/m-03-dynamic-mesh-upload.md)；子批15共149回归/前后一+二窗→动态实体/首可见/像素、完整0GC、高负载/设备收益 |
| H-07 | 高 | 缺少能覆盖当前代码、内容、插值及音频链的 1000 AI 性能证书。 | QUEUED | OPEN / CURRENT_CERTIFICATE_MISSING | [方案](android-mobile-readiness/h-07-fresh-1000-ai-certificate.md) |
| H-01 | 高 | 正式 DAT、角色图片和公共资源仍依赖项目文件路径，Android 部署链待闭合。 | QUEUED | OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED | [方案](android-mobile-readiness/h-01-content-deployment.md) |
| H-02 | 高 | Menu/Battle 场景列表已接线，但 Android 构建和冷启动闭包仍待验收。 | QUEUED | OPEN / UNITY_SCENE_LIST_AND_EDITOR_CALLBACK_PASS / ANDROID_BUILD_PENDING | [方案](android-mobile-readiness/h-02-android-build-scene-closure.md) |
| H-04 | 高 | Android ARM64/IL2CPP 成品及平台确定性证据尚未齐备。 | QUEUED | OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED | [方案](android-mobile-readiness/h-04-il2cpp-arm64.md) |
| H-03 | 高 | 触屏输入尚未进入正式固定 tick 输入链。 | QUEUED | OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED | [方案](android-mobile-readiness/h-03-touch-input-fixed-tick.md) |
| H-06 | 高 | 碰撞默认仍走 BruteForce，现有 Role-aware/Sweep/树路径需新鲜准入与成本评估。 | QUEUED | OPEN / OPTIMIZED_PATHS_EXIST / PARITY_AND_PROFILING_REQUIRED | [方案](android-mobile-readiness/h-06-collision-broadphase-1000.md) |
| H-05 | 高 | 目标 Android GPU/API 的功能、内存及持续性能未取得当前认证。 | QUEUED | OPEN / CERTIFICATION_PENDING | [方案](android-mobile-readiness/h-05-device-gpu-certification.md) |
| H-09 | 高 | 优化仍需绑定当前权威和限定正确性状态，不能以性能代替回归。 | QUEUED | OPEN / SCOPED_CLOSURE_ACCEPTED / OPTIMIZATION_REGRESSION_GATE_PENDING | [方案](android-mobile-readiness/h-09-ntsd28-correctness-alignment.md) |
| M-01 | 中 | worker 准入、同步回退与单飞背压可能限制吞吐，不能以启用开关代替实测。 | QUEUED | OPEN / PROFILING_REQUIRED | [方案](android-mobile-readiness/m-01-dedicated-worker-throughput.md) |
| M-11 | 中 | Host/Core/Presentation 依赖尚未单向，需按现状重新清点并分批重构。 | USER_HOLD（独立专项门） | OPEN / IMPLEMENTATION_NOT_STARTED / USER_HOLD | [方案](android-mobile-readiness/m-11-mono-core-presentation-layering.md) |
| M-02 | 中 | 资源、模式和 chunk 会拆分物理 segment，CPU 命令数不能替代 GPU batch。 | QUEUED | OPEN / DEVICE_MEASUREMENT_REQUIRED | [方案](android-mobile-readiness/m-02-central-render-draw-segmentation.md) |
| M-12 | 中 | 解码并发数未约束累计像素字节，上传等待与 staging 会推高峰值。 | QUEUED | OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED | [方案](android-mobile-readiness/m-12-decode-upload-byte-backpressure.md) |
| M-13 | 中 | 同 tick 多 cue 声音聚合重复扫描，密集音效可能放大 CPU 成本。 | QUEUED | OPEN / HOTSPOT_CANDIDATE / PROFILING_REQUIRED | [方案](android-mobile-readiness/m-13-sound-event-aggregation.md) |
| M-14 | 中 | 加载多阶段重读 catalog/图片 SHA，启动完整性校验可能重复消耗 I/O。 | QUEUED | OPEN / STARTUP_COST_CANDIDATE / PROFILING_REQUIRED | [方案](android-mobile-readiness/m-14-content-verification-io.md) |
| M-15 | 中 | 本地 battle-kernel 依赖未纳入统一证书会削弱构建可复现性。 | QUEUED | OPEN / CERTIFICATE_INTEGRATION_PENDING | [方案](android-mobile-readiness/m-15-kernel-build-reproducibility.md) |
| M-05 | 中 | logic-only 支持不代表 GameObject shell 成本归零，需区分三类 workload。 | QUEUED | OPEN / LOGIC_ONLY_PATH_EXISTS / MEASUREMENT_REQUIRED | [方案](android-mobile-readiness/m-05-gameobject-shell-cost.md) |
| M-06 | 中 | runtime 容量预热已接入，但过量 shell 预建及峰值计划仍需评估。 | QUEUED | OPEN / RUNTIME_CAPACITY_PREWARM_EXISTS / CAPACITY_PROFILE_REQUIRED | [方案](android-mobile-readiness/m-06-pool-capacity-prewarm.md) |
| M-04 | 中 | 已有中央失败诊断仍需设备故障注入与有界报告闭环。 | QUEUED | OPEN / DIAGNOSTICS_EXIST / DEVICE_FAULT_MATRIX_PENDING | [方案](android-mobile-readiness/m-04-centralonly-fail-closed-diagnostics.md) |
| M-07 | 中 | Automatic 图形 API 的实际能力与回退策略尚未通过设备矩阵冻结。 | QUEUED | OPEN / API_POLICY_NOT_FROZEN | [方案](android-mobile-readiness/m-07-android-graphics-api-policy.md) |
| M-08 | 中 | 缺少当前构建 10～20 分钟热稳态证据。 | QUEUED | OPEN / SUSTAINED_TEST_MISSING | [方案](android-mobile-readiness/m-08-sustained-thermal-performance.md) |
| M-09 | 中 | 证书需统一代码、权威、kernel、内容、插值、音频、设备及配置指纹。 | QUEUED | OPEN / SCHEMA_INTEGRATION_PENDING | [方案](android-mobile-readiness/m-09-performance-evidence-fingerprint.md) |
| M-10 | 中 | 移动取景、黑区和触控仍需 Safe Area/宽屏/平板/旋转矩阵。 | QUEUED | OPEN / DEVICE_MATRIX_PENDING | [方案](android-mobile-readiness/m-10-mobile-viewport-safe-area.md) |
| L-01 | 低 | 正式 Android 包名尚需产品确认。 | QUEUED | OPEN / PRODUCT_VALUE_REQUIRED | [方案](android-mobile-readiness/l-01-application-identifier.md) |
| L-02 | 低 | 正式签名与升级链待安全配置。 | QUEUED | OPEN / SECURE_CREDENTIAL_REQUIRED | [方案](android-mobile-readiness/l-02-signing-keystore.md) |
| L-03 | 低 | 发布 Target API 需在发布时按官方要求冻结。 | QUEUED | OPEN / RELEASE_TARGET_NOT_FROZEN | [方案](android-mobile-readiness/l-03-target-api.md) |
| L-04 | 低 | 屏幕方向策略需明确，避免旋转时布局与输入异常。 | QUEUED | OPEN / PRODUCT_DECISION_REQUIRED | [方案](android-mobile-readiness/l-04-orientation-policy.md) |
| L-05 | 低 | Graphics Jobs/FrameTiming 需按设备 A/B 决定。 | QUEUED | OPEN / A_B_REQUIRED | [方案](android-mobile-readiness/l-05-graphics-jobs-frame-timing.md) |
| L-06 | 低 | HDR 的视觉收益与移动端带宽成本待 A/B。 | QUEUED | OPEN / A_B_REQUIRED | [方案](android-mobile-readiness/l-06-urp-hdr.md) |
| L-07 | 低 | AI profile tooltip 与空值 resolver 语义仍需同步。 | QUEUED | OPEN / SCRIPT_CHANGE_NOT_STARTED | [方案](android-mobile-readiness/l-07-ai-profile-description-drift.md) |
| L-08 | 低 | 图标、版本、AAB 和包体预算等发布流程待收口。 | QUEUED | OPEN / RELEASE_PREPARATION_NOT_STARTED | [方案](android-mobile-readiness/l-08-release-packaging.md) |

## 批次与证据

| 批次 | 条目 | Task / Change | 状态与证据 | 剩余条件 |
|---|---|---|---|---|
| 15 warm中央相机物化消重 | M-03（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-CAMERA-MATERIALIZATION-015.md) | 149去重Passed、before132camera/264Build、after143/143及118/118；2→1 Build/camera、1408→704bytes/camera；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007/REPORT.md) | 两次重进三残留0/Scene不变；仅entity vertex工作减半，非GPU/FPS/整场50%；动态像素/首可见/完整0GC/高负载/设备仍OPEN |
| 14 原Battle自然窗口与重进 | M-03（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-PRODUCTION-WINDOW-014.md) | 原Editorcompile、101/101；原Battle两轮各96tick，128+136camera/256+272Build；SCOPED_PRODUCTION_WINDOW_PASS；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006/REPORT.md) | recorded allocation0B/growth0、两slot与关闭三残留0；同帧两物化已证但未消重，下一消费者/采样边界；GPU/完整0GC/120FPS/1000AI/Android仍开放 |
| 13 多子网格metadata批量更新 | M-03（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-SUBMESH-BATCH-UPLOAD-013.md) | 原Editorcompile/改前26/26/改后20/20/回归71/71（91去重）；SCOPED_METADATA_AB_PASS / 生产RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006/REPORT.md) | 72000有效Build局部0B/growth0；1000段无诊断mean降46～47%，不合并segment；真实生产/像素/完整链/设备开放 |
| 12 受控阶段成本 | M-03（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-MESH-PHASE-BASELINE-012.md) | 原Editorcompile、8阶段+12控制20/20、旧34/34（去重54）；SCOPED_MESH_PHASE_BASELINE_PASS；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006/REPORT.md) | 36000 Build局部0B/growth0；1000段metadata含descriptor mean1.14～1.20ms，计时税/非GPU；下一最小A/B/真实生产/完整链/设备 |
| 11 受控backend基线 | M-03（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH11-MESH-BACKEND-BASELINE-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-MESH-BACKEND-BASELINE-011.md) | 原Editorcompile、新12/12+旧26/26（去重38）；SCOPED_BACKEND_BASELINE_PASS；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH11-MESH-BACKEND-BASELINE-20261006/REPORT.md) | 100/500/1000及4097命令×1800；局部0B/零growth；非自然publication/素材/AI/GPU。下一分阶段定位/最小A/B、真实生产窗口/完整链/收益/设备 |
| 10 生产计数报告 | M-03（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH10-MATERIALIZATION-REPORT-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-MATERIALIZATION-REPORT-010.md) | 原Editorcompile、新58/58+旧225/225（去重283）；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH10-MATERIALIZATION-REPORT-20261006/REPORT.md) | 43-long冻结/累计与窗口/Reset来源倒退整份拒绝；counter+Reset局部0B，报告非热路径有分配；实际生产窗口/基线/完整链/收益/设备开放 |
| 09 Build/API类别归属 | M-03（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH09-BUILD-UPLOAD-ATTRIBUTION-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-BUILD-UPLOAD-ATTRIBUTION-009.md) | 原Editorcompile、新23/23+旧202/202（去重225）；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH09-BUILD-UPLOAD-ATTRIBUTION-20261006/REPORT.md) | 总组/三分类累计实际Build admission/outcome及成功API payload；局部wrapper+Build0B，非像素成功或完整链；生产分组报告/基线/完整链/收益/设备开放 |
| 08 显示request分类 | M-03（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-MATERIALIZATION-COUNTERS-008.md) | 原Editorcompile、新20/20+旧182/182（去重202）；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006/REPORT.md) | 仅request三类/Prepare entry与原return原因；helper局部0B，非实际Build/上传或成功渲染；实际上传分组/生产报告/基线/完整链/设备开放 |
| 07 上传计数报告出口 | M-03（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH07-UPLOAD-REPORT-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-UPLOAD-REPORT-007.md) | 原Editorcompile、新14/14+旧168/168（去重182）；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH07-UPLOAD-REPORT-20261006/REPORT.md) | Present后冻结/可选frame及summary；新增helper局部0B；publication-alpha分类/完整链/真实基线/收益/设备开放 |
| 06 顶点上传计数 | M-03（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-VERTEX-UPLOAD-COUNTERS-006.md) | 原Editor编译；新18/18、旧132/132（去重150）；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006/REPORT.md) | 两模式各48 Build/Upload局部0B；仅中央实体Mesh API payload，不含GPU流量/辅助Mesh；分组导出/完整链/收益/设备开放 |
| 05 原生Mesh再预热 | H-11（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H11-NATIVE-MESH-PREWARM-005.md) | 原Editor编译；新12/12、旧120/120；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006/REPORT.md) | 已失效Mesh重建前移到seal前；首次与尾部Build/Upload局部0B；真实重进/完整链/预算/设备仍开放 |
| 04 解析器准备封口 | H-11（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH04-RESOLVER-SEAL-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H11-RESOLVER-SEAL-004.md) | 原Editor编译；新15/15、适配旧1/1、旧105/105；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH04-RESOLVER-SEAL-20261006/REPORT.md) | sealed禁止Prepare；缓存满不丢解析/字段/存储保持、strict/Prepared及Configure no-op局部0B；完整链/Scene/设备仍开放 |
| 03 辅助缓存封口 | H-11（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH03-AUX-CAPACITY-SEAL-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H11-AUX-CAPACITY-SEAL-003.md) | 原Editor编译；新15/15、旧61/61；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH03-AUX-CAPACITY-SEAL-20261006/REPORT.md) | marker/bar eligible数量硬限、整帧拒绝/旧有效提交保持及辅助双backend局部0B已聚焦；完整0GC/Scene/设备门保留 |
| 02 容量封口 | H-11（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H11-CAPACITY-SEAL-002.md) | 原Editor编译；新16/16、旧30/30；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006/REPORT.md) | snapshot copy前/mesh mutation前/motion generation前硬限与整帧拒绝已聚焦；其余缓存、全路径0GC、Battle关闭重进与设备门保留 |
| 01 缓存预热 | H-11（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H11-CACHE-PREWARM-001.md) | 原Editor编译；新检查7/7、网格回归8/8；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006/REPORT.md) | 当批硬限/seal/overflow缺口现由子批02部分闭合；父项其余物化缓存、完整0GC、原Battle退出重进和设备门保留 |

后续顺序沿用[共同启动门](battle-optimization-rebaseline-and-start-gates-20261006.md)：
基线→容量小批→资源闭包/内存决策→实测热点→单独MONO→Android认证。
没有资源/设备/产品选择时保持相应项等待，不无证据改格式或设置。

## 追加进度日志

- 2026-10-07：子批14新增一个只读Editor观察入口，生产算法未改。101确认去重case通过；
  两自然窗口各tick8→104，camera128/136、Build256/272，publication96/96、alpha160/176、repeated0。
  每Build实体Mesh704bytes/stride44，窗口180224/191488bytes；2实体/4command/4segment/1chunk。
  camera begin→end plan generation全+1、slot0→1，两Mesh稳定；LateUpdate force Flush后camera再次物化，
  下一候选是保持早期消费者/采样时刻/first-visible/failclosed的最小几何消重，尚未实现，不承诺50%收益。
  camera envelope/observer及已有tick/Update/Late/PlayerLoop端点全0B；Editor硬门支持false，不等同完整0GC。
  两次11阶段关闭三残留0/Scene clean/SHA不变，已恢复原Menu idle/非Play。
  初探针slot观察混叠FAIL、空筛选、孤儿UnityTest缺最终结果及自动重进未启动记录全部保留；
  第二轮由显式idle菜单接续，不重写第一份PASS，旧trace原字节备份且after字节同。
  34项优先级/父关闭0、EXT1/MONO/ATLAS专项门不变；完整GPU/高负载收益/1000AI/Android仍开放。

- 2026-10-06：子批13生产仅4新增/1删除；stable fully-active多submesh复用batch API，
  小prefix/单段保留原路径，避免inactive high-water tail反复批量；segment/排序/UV/bounds/上传payload不改。
  改前26/26、改后20/20、相关71/71，117成功执行/91去重Passed；有效Build72000/warmup2560。
  当前线程局部managed0B/growth0，20合同投影一致。1000高segment无诊断mean1.878～2.000→0.996～1.051ms，
  约降46.33～47.48%；metadata inclusive约1.159～1.209→0.315～0.340ms。
  顺序synthetic A/B不是自然publication/素材/1000AI/GPU/整场/Android；单段instrumented mean约增2～3%保留，不无证据归因。
  首次新增translated bounds exact断言失败改测试容差，两端通过；result=null原件保留、不算生产RED。
  原Menu前后clean/8roots/idle/nonPlay，九backup/393保护保持。父M03与34项优先级/专项门不解冻；
  下一真实中央物化/publication-alpha显示窗口、像素/完整链0GC/实际收益；生产RUNTIME_PENDING。

- 2026-10-06：子批12复用现有两recorder，8阶段+12控制20/20、相关旧34/34，共54去重Passed。
  六phase86400样本、36000 Build（warmup1280另记），局部managed0B/growth0，payload/segment/Mesh身份保持。
  高分段SetSubMeshes含descriptor/native mean1.14～1.20ms约占Upload85%；非纯API/GPU。
  阶段1000组mean比顺序控制高0.33～0.46ms，非配对timer收益；原Menu clean/8roots/nonPlay。
  下一同分段/排序/UV/尾部/高水位/bounds合同的native metadata最小A/B，未冻结算法或资源/专项门。

- 2026-10-06：子批11只新增Editor fixture，生产无改；原Editor12/12局部采样、相关26/26，38去重Passed。
  每组64预热/1800采样（共21600），RTX4070/D3D11/i5-13600KF/Unity2022.3.62f3/URP14具名；
  1000兼容Ordered p50约0.70ms，Strict/交错variant1000物理段约1.79/1.85ms，bytes均176000/Build。
  4097两chunk两API/721072bytes，stride44、当前线程managed0B/容量growth0，非slot/GPU流量。
  直接Build绕过queued同样本门，不称生产重复显示仍上传；自然publication/alpha比例仍未知。
  TestRunner临时untitled scene，原Menu前后clean/8roots/非Play，未主动切场景/Play/Profiler/完整M0。
  两次fixture compile错误最小纠正并保留，七dirty备份/324保护保持；父M-03 OPEN。
  下一复用phase timing定位高segment成本，再最小A/B；专项门、34项优先级不变。

- 2026-10-06：子批10原Editor有效RED完成58，missing入口失败列表25 capped，result=null不伪造summary；
  新58/58、旧225/225，共283去重case。既有central仅9新增，新report199行/fixture309行。
  生产43long冻结导出、累计scope/同source同epoch窗口与end/start复算；
  Reset catch-up/不同source/空baseline/非累计端点/全部43counter倒退整份拒绝，无clamp或partial。
  projection修改不反写report；token不留World/资源，阶段5Reset递增epoch，原gate/Build/上传/关闭不变。
  显式Capture/JSON非热路径有分配、不自动采样；原counter+epoch Reset预备64/512局部managed0B。
  286保护/8before备份保持，外部HEAD2cccd597保留；无Play/完整M0/Profiler/GPU/设备。
  下一具名生产平台/API/workload/窗口和同布局baseline，再据证据做dirty优化；父M-03 OPEN。

- 2026-10-06：子批09有效RED23缺wrapper/group/helper；唯一生产文件125插入/2删除后新23/23、旧202/202，去重225。
  request kind通过局部参数关联实际staging Build；保留原两参Prepare反射入口None。
  disposed/null resolver/sealed拒绝不重复旧diagnostics；进入后失败在外Clear前只计已完成API，
  empty/null/unresolved completed不等于pixel成功，force cached-plan不计Build；未改变原fail-closed。
  总组+三组8-long随原diagnostics复用，per-frame保持/Reset清零；scalar64预备512次局部0B，
  Ordered/Strict各预备后144 wrapper+Build/96API/3456顶点当前线程managed0B，非完整链。
  250保护/8dirty备份保持；未启Play/完整M0/Profiler/GPU/设备；原排序/slot/lease/33ms与专项门不变。
  下一生产分组报告读出口、真实同布局基线、完整链0GC/dirty设计/1000AI/Android；父M-03 OPEN。

- 2026-10-06：子批08有效RED20缺helper/counter失败；唯一生产文件112插入/1删除后新20/20、旧182/182，去重202。
  有效CentralOnly request按前一request分类，world/version变化优先于alpha；attempt只表示Prepare入口，
  force仍可cached-plan，原同帧/同样本/busy条件不变；三个return原因计数不是成功像素复用证明。
  64预备后512次新增cached scalar helper局部managed0B，ResetRuntime清累计与world baseline。
  原Editor实际compile/CS0；未启Play/Profiler/完整M0/设备，无上传减少或FPS收益证据。
  剩余实际Build/上传按类和生产报告、真实同布局基线、完整链0GC/dirty设计/1000AI/Android；父M-03 OPEN。
  8份dirty备份/211保护SHA保持；EXT-1/MONO/ATLAS专项门不变，不读Q06活跃body。

- 2026-10-06：子批07有效RED14缺失字段/helper失败；唯一生产文件新增36行后新14/14、旧168/168，去重182。
  等待/重试/warmup/异常快照、独立样本/大字节值/不可用/导出、v5判据均聚焦；
  central/Legacy各128次helper当前线程managed0B，不包括CaptureFrame/report原有分配。
  原Editor Menu非Play、编译CS0；CPU Mesh API payload仅接受样本口径，非GPU/生产submission或全程累计。
  剩余publication/alpha分组、真实同布局基线/完整链0GC/脏区与1000AI/Android，父M-03继续OPEN。

- 2026-10-06：用户批准下一批；子批07上传计数可选报告出口已建Task/Change与八文件备份。
  有效RED14项均因缺失字段/helper失败，生产只新增快照/报告；GREEN/回归待实际结果。
  不改v5必测/判据、上传算法或专项门；不把报告接线当减少上传/GPU/Android证据。

- 2026-10-06：用户继续下一批，M-03子批06先补真实SetVertexBufferData成功API调用计数；
  新18项缺失字段原状RED后18/18 GREEN，旧132/132相关回归，去重150项。
  每进入Build重置次数/顶点数/字节数；stride在Mesh创建期取实际值，不靠segment推算。
  空/null、跨chunk、重复、缩小/Clear、准备/索引排除、解析缺失、拒绝保留旧诊断、
  第一次/第二chunk异常的已完成API口径均通过；不是允许提交部分结果。
  Ordered/Strict预热后各48 Build包含Upload局部managed0B，不覆盖物化/RenderPass/GPU。
  上传算法和A1待实施状态不变；无性能收益或1000AI/Android证书，父M-03/H-11仍开放。
  原EditorMenu clean非Play；九个准确备份及前五批/专项保护复核；没有EXT-1专项M0或资源决策。

- 2026-10-06：子批05新12项原状10预期RED/2控制PASS；PrepareCapacity三行生产hunk修后12/12，
  旧四批/mesh/物化/解析/辅助相关120/120通过（本批去重132）。四容量/两chunk native恢复、
  有效Mesh身份保持、选择性/零容量/sealed拒绝/Unseal均聚焦；stride44/UInt16/初始化合同不变。
  17/4097 × Ordered/Strict四组，恢复后首次Build及8轮稀疏→空→密集共25 Build/Upload局部0B，
  不将fixture native毁损恢复当真实无Domain Reload重进，原native突发丢失策略未改。
  原Editor Menu clean/nonPlay/CS error0；H-11完整0GC/其余缓存/预算/Scene/Android与专项门保持。

- 2026-10-06：用户批准下一批；子批05原生Mesh恢复前移预热已建Task/Change与八文件精确备份。
  新12项原状10预期RED/2控制PASS，单生产Prepare hunk已写；GREEN和旧回归待实际结果。
  不读Q06 body，不改已有突发Mesh丢失恢复政策或专项门，不把局部GC当完整链/重进/设备证书。

- 2026-10-06：子批04有效RED新15项10控制PASS/5预期失败，旧trusted helper签名1项失败；
  生产仅3行Prepare sealed门、既有测试helper仅2行28参数MotionAnchor适配，修后新15/15+旧1/1，
  旧resolver/前三批/几何/物化/common等105/105通过（去重120项）。32种资源64轮，
  capacity0/1/17 × strict/Prepared的Configure no-op+Resolve局部0B；满缓存仍完整解析，不丢draw。
  初两次fixture无效失败与域重载连接错误保留原件，未作为生产首差；主PID未变，端口现6401。
  原Editor Menu clean/nonPlay，父H-11完整链0GC/其余物化缓存/运行时/预算/设备仍开放。

- 2026-10-06：用户批准下一批；子批04资源解析器Prepare封口已建Task/Change与八文件准确备份。
  test-first检查显式准备绕过seal和缓存满不丢解析，未更改Q06/资源/排序/预算或专项门。

- 2026-10-06：子批03新15项原状全RED，四生产hunk修后15/15 GREEN、旧61/61；
  辅助marker/bar按实际eligible数量硬限，禁用/过滤不误拒绝，封口后禁Prepare，沿用原Seal/End。
  64次交错帧辅助双backend BuildFromFrame（含自身预检/mesh上传）局部0B，非整条central链。
  新扫描CPU耗时未测；父H-11完整0GC/命令物化其余缓存/真实Battle退出重进/GPU/设备继续开放。

- 2026-10-06：用户批准下一批；子批03辅助marker/bar缓存封口已建Task/Change及11文件备份，
  按实际eligible数量与禁用过滤边界test-first，不读Q06 body、不改渲染顺序/资源/GPU生命周期。

- 2026-10-06：H-11子批02原状16项预期RED，四生产hunk修后16/16 GREEN、旧30/30回归；
  每slot snapshot/mesh/motion逻辑限、sealed禁止再预热、整帧拒绝/旧提交与CPU lease保持。
  varying-alpha DisplayMotion.Prepare64次含自身预检局部0B；并非完整渲染链0GC或性能收益。
  原Editor Menu clean/nonPlay，未测GPU/设备，父H-11与专项门继续开放。

- 2026-10-06：用户批准下一批；H-11子批02容量封口已建Task/Change和11文件精确备份，
  新RED/实现/验收按实际结果追加，原第一批事实和专项门保留。

- 2026-10-06：用户批准开始；建立34项统一总表，选择H-11最小预热子批，
  操作前9个精确路径/保护哈希已保存；尚未声称编译、测试或M0通过。
- 2026-10-06：H-11子批01落地两处现有缓存预热，原Editor test-first为6个预期RED/7项，
  修复后7/7 GREEN，已有submesh/bounds回归8/8。仅高slot插值lookup64次局部0B；
  不等于全渲染链0GC/帧率收益或Android通过。完整M0、真实Battle关闭重进仍未执行。
