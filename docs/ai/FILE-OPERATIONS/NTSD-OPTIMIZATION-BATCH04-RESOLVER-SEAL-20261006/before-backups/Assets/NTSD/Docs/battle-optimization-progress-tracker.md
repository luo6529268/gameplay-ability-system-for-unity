# NTSD 全部优化点实施进度总表

> ID：NTSD-OPTIMIZATION-PROGRESS-001；建立/更新：2026-10-06。
> 当前：IMPLEMENTATION_STARTED / ANDROID_NOT_CERTIFIED。用户已批准按优化文档开始。
> 本表是统一进度入口；[风险/优先级登记](android-mobile-readiness-priority-risk-register.md)、
> 独立方案负责问题与验收，不把实现存在或编译成功当作性能/设备通过。
> 当前：H-11子批01/02/03均聚焦通过；子批03新15/15、旧回归61/61，父项完整运行门待验收。

## 状态和更新规则

QUEUED：在计划中，尚未实施；IN_PROGRESS：有具名Task/Change执行；CODE_WRITTEN：
代码已写；COMPILE_PASS：实际编译；FOCUSED_TEST_PASS：具名聚焦检查；
RUNTIME_PENDING：尚缺原Scene/完整0GC等门；VERIFIED：该项所有验收已闭合。
USER_HOLD或WAITING_DECISION/DEVICE不是其他独立小批的全局阻塞。
每次更新写日期、实际产物、证据、剩余门；不用主观百分比冒充实测。
未测获益保持未知。一个子批PASS不得把父项全部关闭。

## 总览

34项：高12/中14/低8；父项关闭0。H-11预热/容量封口子批已聚焦通过，其余33项未实施本轮优化。
阶段0当前版本基线正在清点，完整M0未完成；原Editor运行窗口/API/workload
需明确后才做完整压测。现有聚焦测试可确认两处缓存遗漏，不替代1000实体验收。
禁止项/专项门：EXT-1继续PROPOSED / MODIFY_REQUIRED，无专项M0/instancing；
MONO B0-B9独立实施门、ATLAS资源/bank/预算/格式、Scene/Input Actions、发布凭据仍按原合同。
用户本次总体启动授权不取消这些明确单列的决策。

## 所有条目

| ID | 级别 | 简述 | 本轮状态 | 已有基线（非本轮成果） | 下一验收门 / 方案 |
|---|---|---|---|---|---|
| H-08 | 高 | 全量视觉预热与源纹理回退仍可能造成高驻留及启动峰值。 | QUEUED | OPEN / SOURCE_FALLBACK_EXISTS / PREBAKE_PENDING | [方案](android-mobile-readiness/h-08-prebaked-visual-content-memory.md) |
| H-10 | 高 | 全量音频预热与双声道 PCM 副本增加整局内存和启动成本。 | QUEUED | OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED | [方案](android-mobile-readiness/h-10-battle-audio-pcm-memory.md) |
| H-11 | 高 | 预热与slot/mesh/motion/marker/bar硬限已补；其余缓存/全路径0GC仍待闭合。 | RUNTIME_PENDING（子批01/02/03聚焦通过） | OPEN / STATIC_ALLOCATION_PATH_CONFIRMED（改前） | [方案](android-mobile-readiness/h-11-presentation-capacity-zero-gc.md)；子批03新15/15+旧61/61→其余物化缓存/完整0GC/退出重进 |
| M-03 | 高 | 插值显示取样变化会重复物化并上传活动顶点，需分层缓存与脏区优化。 | QUEUED | OPEN / INTERPOLATION_EXISTS / PROFILING_REQUIRED | [方案](android-mobile-readiness/m-03-dynamic-mesh-upload.md) |
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
| 03 辅助缓存封口 | H-11（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH03-AUX-CAPACITY-SEAL-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H11-AUX-CAPACITY-SEAL-003.md) | 原Editor编译；新15/15、旧61/61；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH03-AUX-CAPACITY-SEAL-20261006/REPORT.md) | marker/bar eligible数量硬限、整帧拒绝/旧有效提交保持及辅助双backend局部0B已聚焦；完整0GC/Scene/设备门保留 |
| 02 容量封口 | H-11（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H11-CAPACITY-SEAL-002.md) | 原Editor编译；新16/16、旧30/30；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006/REPORT.md) | snapshot copy前/mesh mutation前/motion generation前硬限与整帧拒绝已聚焦；其余缓存、全路径0GC、Battle关闭重进与设备门保留 |
| 01 缓存预热 | H-11（部分） | [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H11-CACHE-PREWARM-001.md) | 原Editor编译；新检查7/7、网格回归8/8；RUNTIME_PENDING；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006/REPORT.md) | 当批硬限/seal/overflow缺口现由子批02部分闭合；父项其余物化缓存、完整0GC、原Battle退出重进和设备门保留 |

后续顺序沿用[共同启动门](battle-optimization-rebaseline-and-start-gates-20261006.md)：
基线→容量小批→资源闭包/内存决策→实测热点→单独MONO→Android认证。
没有资源/设备/产品选择时保持相应项等待，不无证据改格式或设置。

## 追加进度日志

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
