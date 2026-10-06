# NTSD 全部优化点实施进度总表

> ID：NTSD-OPTIMIZATION-PROGRESS-001；建立/更新：2026-10-06。
> 当前：IMPLEMENTATION_STARTED / ANDROID_NOT_CERTIFIED。用户已批准按优化文档开始。
> 本表是统一进度入口；[风险/优先级登记](android-mobile-readiness-priority-risk-register.md)、
> 独立方案负责问题与验收，不把实现存在或编译成功当作性能/设备通过。
> 当前：H-11子批01/02/03/04/05均聚焦通过；子批05新12/12、旧回归120/120，父项完整运行门待验收。
> M-03子批06上传计数聚焦通过：新18/18、旧132/132；完整链/收益/设备仍待验收。
> M-03子批07可选报告出口聚焦通过：新14/14、旧168/168；仅快照/导出，完整链/收益/设备仍待验收。
> M-03子批08显示request分类聚焦通过：新20/20、旧182/182（去重202）；原条件不变，实际上传分组/基线/完整链仍待验收。

## 状态和更新规则

QUEUED：在计划中，尚未实施；IN_PROGRESS：有具名Task/Change执行；CODE_WRITTEN：
代码已写；COMPILE_PASS：实际编译；FOCUSED_TEST_PASS：具名聚焦检查；
RUNTIME_PENDING：尚缺原Scene/完整0GC等门；VERIFIED：该项所有验收已闭合。
USER_HOLD或WAITING_DECISION/DEVICE不是其他独立小批的全局阻塞。
每次更新写日期、实际产物、证据、剩余门；不用主观百分比冒充实测。
未测获益保持未知。一个子批PASS不得把父项全部关闭。

## 总览

34项：高12/中14/低8；父项关闭0。H-11预热/容量小批、M-03上传计数/报告/显示request分类前置已聚焦通过，其余32项未实施本轮优化。
阶段0当前版本基线正在清点，完整M0未完成；原Editor运行窗口/API/workload
需明确后才做完整压测。现有聚焦测试覆盖预热与多类缓存封口，不替代1000实体验收。
禁止项/专项门：EXT-1继续PROPOSED / MODIFY_REQUIRED，无专项M0/instancing；
MONO B0-B9独立实施门、ATLAS资源/bank/预算/格式、Scene/Input Actions、发布凭据仍按原合同。
用户本次总体启动授权不取消这些明确单列的决策。

## 所有条目

| ID | 级别 | 简述 | 本轮状态 | 已有基线（非本轮成果） | 下一验收门 / 方案 |
|---|---|---|---|---|---|
| H-08 | 高 | 全量视觉预热与源纹理回退仍可能造成高驻留及启动峰值。 | QUEUED | OPEN / SOURCE_FALLBACK_EXISTS / PREBAKE_PENDING | [方案](android-mobile-readiness/h-08-prebaked-visual-content-memory.md) |
| H-10 | 高 | 全量音频预热与双声道 PCM 副本增加整局内存和启动成本。 | QUEUED | OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED | [方案](android-mobile-readiness/h-10-battle-audio-pcm-memory.md) |
| H-11 | 高 | 预热与多类硬限、resolver封口、已失效native Mesh再预热已补；其余缓存/全路径0GC仍待闭合。 | RUNTIME_PENDING（子批01/02/03/04/05聚焦通过） | OPEN / STATIC_ALLOCATION_PATH_CONFIRMED（改前） | [方案](android-mobile-readiness/h-11-presentation-capacity-zero-gc.md)；子批05新12/12+旧120/120→其余物化缓存/完整0GC/实际退出重进 |
| M-03 | 高 | 显示request三分类/Prepare入口/原去重原因计数已补；API报告已有，尚未减少上传。 | RUNTIME_PENDING（子批06/07/08聚焦通过） | OPEN / INTERPOLATION_EXISTS / PROFILING_REQUIRED | [方案](android-mobile-readiness/m-03-dynamic-mesh-upload.md)；子批08新20/20+旧182/182→实际Build/上传分组、生产报告/基线、完整0GC/脏区 |
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
