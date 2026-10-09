> 2026-10-07 完成边界更正：优化执行以[有限首阶段合同第0—8节](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BOUNDED-AUTOGOAL-20261007.md)为准。次数/批次只触发复盘，不自动停止目标；候选不采用不等于全部Goal无路可做。阶段、产物、父项关闭分别判断，旧停点按历史阅读；[文档操作](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-PHASE-COMPLETION-BOUNDARY-CORRECTION-20261007/RECORD.md)。

> 2026-10-07 第22批限定通过：NTSD-OPTIMIZATION-BATCH22-RUNTIME-FOOT-CONFIG-20261007 / SCOPED_RUNTIME_FOOT_CONFIG_PASS；NTSD-OPT-M03-RUNTIME-FOOT-CONFIG-022 / VERIFIED（本批）。GameConfig fallback接线，显式authoring/禁用保持；原Editor32/32，Battle1800实际camera每帧Foot2/Health2、两slot、CPU DrawMesh11815录制=执行、growth0；三档100/500/1000真实catalog重复命令重放均局部0B/growth0，非1000AI。11阶段关闭三残留0/Scene同/Menu恢复，30保护/11备份保持。有限首阶段1/8新子批完成，34项父项关闭仍0；完整选定链与后续5项继续，EXT-1/Mono/ATLAS门不解冻。下方旧快照按时间阅读。

> 2026-10-07 第21批限定诊断通过：NTSD-OPTIMIZATION-BATCH21-FOOT-AUTHORING-20261007 / SCOPED_FOOT_AUTHORING_DIAGNOSIS_PASS；NTSD-OPT-M03-FOOT-AUTHORING-021 / VERIFIED（仅诊断）。原Editor9 RED→新9＋旧61=70/70；原Battle64 distinct camera/tick8→63，每帧Self2/Health2/Foot0；六帧GameConfig有效，loadedPreview0→authoring false→runtime Foot禁用/Sprite空，NO_LOADED_AUTHORING已确认。只Editor观察，无生产修复/Scene变更，第20批严格FAIL/replay NOT_RUN保留；两slot/CPUlease0、关闭三残留0/Scene同/Menu恢复，733保护/8备份保持。34项高12中14低8关闭0、父M03/H11 OPEN，专项门不解冻。执行期外部HEAD提交变化已记录，本轮无Git提交；下方事前历史保留。 [报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH21-FOOT-AUTHORING-20261007/REPORT.md) / [Record](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-FOOT-AUTHORING-021.md)。

> 2026-10-07 第21批事前：NTSD-OPTIMIZATION-BATCH21-FOOT-AUTHORING-20261007 / IN_PROGRESS；NTSD-OPT-M03-FOOT-AUTHORING-021 / PLANNED。仅原savedBattle 64实际camera只读Foot配置/Self前置诊断，不改production/Scene/Q06 body，不降第20批严格门；8准确备份/733保护完成。34项高12中14低8、父M03/H11 OPEN，专项门保持，尚未实施或测试。 [Record](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-FOOT-AUTHORING-021.md)。

> 2026-10-07 第20批PARTIAL / FOOT_COVERAGE_UNMET：NTSD-OPTIMIZATION-BATCH20-PRODUCTION-CATALOG-20261007；NTSD-OPT-M03-PRODUCTION-CATALOG-020 / RUNTIME_PENDING。7新＋54相关=61/61 Passed；原Battle1800 distinct camera/tick8→1563、1800Build/0growth、9700实际CPU DrawMesh、catalog/Health活动、camera/observer各0B、两slot/CPUlease0；Foot每帧0导致严格window FAIL，原件保留，三档replay NOT_RUN。Editor两fullGC硬门false/global三代collection各3，非全链0GC/1000AI/Android。关闭三残留0、Scene同/原Menu恢复，8备份/708保护；34项高12中14低8关闭0，父OPEN/专项门保持。下一先只读定位Foot接线，不改Scene/降门槛；下方事前历史保留。 [报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH20-PRODUCTION-CATALOG-20261007/REPORT.md) / [留痕](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-PRODUCTION-CATALOG-020.md)。

> 2026-10-07 第20批事前：NTSD-OPTIMIZATION-BATCH20-PRODUCTION-CATALOG-20261007 / IN_PROGRESS；NTSD-OPT-M03-PRODUCTION-CATALOG-020 / PLANNED。原生产1800 distinct camera观察＋实际catalog的100/500/1000重复显示命令重放分证据；只Editor诊断，不改production/World真值。准确8文件备份完成，708既有保护；34项高12/中14/低8、父M03/H11 OPEN/RUNTIME_PENDING，EXT1/Mono/ATLAS专项门保持。尚未实施或测试。 [留痕](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-PRODUCTION-CATALOG-020.md)。

# Android 移动端就绪度与 1000 AI 优化风险登记表

> 2026-10-07 子批19限定通过：NTSD-OPTIMIZATION-BATCH19-REAL-TEXTURE-SUBMISSION-20261007 / SCOPED_REAL_TEXTURE_CPU_BRIDGE_PASS；NTSD-OPT-M03-REAL-TEXTURE-SUBMISSION-019 / FOCUSED_TEST_PASS。六唯一真实项目Texture2D受控CPU桥窗10800samples，当前线程0B/零扩容/零CPU read lease，两slot/stride44；1000连续2segment、交错/Strict各1000，4097为2chunk/3segment。初7completed作业一项MCP超时连接日志failed保留；仅受影响case＋54回归补跑55/55 Passed。只新Editor fixture，无production改动/收益A-B，7备份/679保护；原Menu clean非Play，父M03/H11 OPEN/RUNTIME_PENDING、34项高12中14低8不关闭；实际生产RenderPass/catalog/活动辅助/高负载全链及设备待验，专项门不解冻。下方事前与中间状态保留。 [报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH19-REAL-TEXTURE-SUBMISSION-20261007/REPORT.md)。

> 2026-10-07 子批19事前：NTSD-OPTIMIZATION-BATCH19-REAL-TEXTURE-SUBMISSION-20261007 / IN_PROGRESS；NTSD-OPT-M03-REAL-TEXTURE-SUBMISSION-019 / PLANNED。仅新Editor真实项目纹理/高command受控CPU提交桥，7当前字节备份/679保护；100/500/1000/4097 command、64warm/1800samples，两slot snapshot/motion/Mesh/DrawMesh/Graphics.Execute/lease。非生产RenderPass/自然AI/完整PlayerLoop/Android；production/Q06/Scene资源与专项门不变，尚未测试。 [留痕](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-REAL-TEXTURE-SUBMISSION-019.md)。

> 2026-10-07 子批18限定通过：NTSD-OPTIMIZATION-BATCH18-DISPLAY-SAMPLE-TIMING-20261007 / SCOPED_DISPLAY_SAMPLE_TIMING_PASS；NTSD-OPT-M03-DISPLAY-SAMPLE-TIMING-018 / RUNTIME_PENDING。54/54回归；原Battle自然240tick/293camera、53同publication对/378命令差值核验（100运动）、误差最大0.000048px、293alpha CPU区间通过。queue→首次CPUcamera-end均5.72/max10.05ms不是screen/GPU latency；observer/camera各0B、关闭三残留0/Scene同/原Menu恢复。只Editor扩展，8备份/653保护，父OPEN/设备与专项门保持。 [报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH18-DISPLAY-SAMPLE-TIMING-20261007/REPORT.md)。


> 2026-10-07 子批18事前：NTSD-OPTIMIZATION-BATCH18-DISPLAY-SAMPLE-TIMING-20261007 / IN_PROGRESS；NTSD-OPT-M03-DISPLAY-SAMPLE-TIMING-018 / PLANNED；仅Editor原Battle alpha/CPUcamera时序只读核验，8当前备份/653保护；production/Q06/Scene/资源及专项门不变，尚未测试/Play。 [Record](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-DISPLAY-SAMPLE-TIMING-018.md)。


> 2026-10-07 子批17限定通过：NTSD-OPTIMIZATION-BATCH17-SAME-SAMPLE-PIXELS-20261007 / SCOPED_SAME_SAMPLE_PIXEL_PASS；NTSD-OPT-M03-SAME-SAMPLE-PIXELS-017 / FOCUSED_TEST_PASS。新6/6＋相关54/54，90同显示样本/829440像素最大通道差0、反序負控制每组819差异；只新Editor fixture，无production改动。原Menu clean/8roots/非Play，7当前备份/611保护；自然排序/first-visible/latency、完整0GC/高负载/设备及父OPEN不晋升。 [报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH17-SAME-SAMPLE-PIXELS-20261007/REPORT.md)。


> 2026-10-07 子批17事前：NTSD-OPTIMIZATION-BATCH17-SAME-SAMPLE-PIXELS-20261007 / IN_PROGRESS；NTSD-OPT-M03-SAME-SAMPLE-PIXELS-017 / PLANNED；同显示样本离屏像素/插值契约验证，只新Editor测试，不改production/Q06/Scene/资源，不启动专项M0。7准确当前备份/611保护；父OPEN及专项门保持。 [Record](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-SAME-SAMPLE-PIXELS-017.md)。



> 2026-10-07 M-03子批16限定通过：[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH16-DYNAMIC-DISPLAY-20261007.md) / [Record](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-DYNAMIC-DISPLAY-016.md)。50/50、自然240tick/308camera、1183snapshot、18新增/16离开publication；只Editor探针，production未改，三关闭残留0/Scene保持。34项/高12中14低8不变；透明像素/first-visible/完整链/设备仍开放。

> 2026-10-07 M-03子批15限定通过：[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007.md) / [Record](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-CAMERA-MATERIALIZATION-015.md)。149去重Passed、原Battle前后窗口Build/camera2→1、entity vertex bytes/camera1408→704；非整场/GPU/FPS50%。两次重进关闭/Scene通过，production RUNTIME_PENDING、34项/高12中14低8/父OPEN保持。

> 当前需处理：整局视觉/音频内存、表现层 0GC、插值物化与上传、1000 AI/碰撞/worker、
> Android 内容/构建/输入/设备认证，以及架构、证据和发布配置。
>
> 文档 ID：`ANDROID-MOBILE-READINESS-RISK-REGISTER-001`
> 建立日期：2026-09-05；最后更新：2026-10-07
> 当前状态：`DOCUMENTS_REBASELINED / IMPLEMENTATION_STARTED / ANDROID_NOT_CERTIFIED`
> 本文只记录说明、优先级、状态、进度、详情入口与留痕；方案、验收和测试条件在独立文档中。
> 2026-10-06 后续授权：用户已批准按优化文档开始。统一实施进度见
> [独立进度总表](battle-optimization-progress-tracker.md)。当前H-11缓存预热/容量封口已聚焦验证，
> Scene/资源/配置及EXT-1/MONO专项仍按原决策门，不由总体启动授权自动变更。

## 1. 当前结论与总进度

不能据现有静态证据承诺 Android 普通战斗或 1000 AI 持续达标。
文档整理阶段仅做静态重扫；后续用户批准启动后，H-11子批01已有7/7+8/8，
子批02新增16/16与旧回归30/30；子批03新增15/15与旧61/61、原Editor编译通过。
子批04解析器Prepare封口新15/15、适配旧1/1及相关旧105/105、原Editor编译通过；
strict/Prepared冷未命中/满缓存及Configure no-op局部0B，不把cache skip当丢draw。
子批05已失效native Mesh恢复前移启动预热，新12/12、旧120/120；首次Build及尾部Upload局部0B，
实际Battle无Domain Reload重进、突发native丢失策略与完整链/GPU/设备门仍未闭合。
M-03子批06补中央实体Mesh每Build的API上传次数/顶点数/字节数，新18/18、旧132/132；
两模式各48 Build/Upload局部0B；未改变上传算法，分组导出/完整链/性能收益仍待验收。
子批07将上传计数接入可选完成帧快照/报告，新14/14+旧168/168；仅接受样本，v5必测判据未改。
新增helper局部0B不是完整benchmark/central链0GC，尚未减少上传或执行完整M0/设备测量。
子批08显示request三分类/Prepare入口及原去重原因已补，新20/20+旧182/182（去重202）。
仅观察有效CentralOnly request，非实际Build/上传分类或成功像素复用；原条件不变，helper局部0B。
真实生产分组/基线、完整链0GC及收益仍待验证，不把未授权M0/A4变成当前执行步骤。
子批09实际Build admission/outcome与成功API payload按三类request归属已补，新23/23+旧202/202（去重225）。
准入拒绝不重复旧值、失败在Clear前冻结，completed不等于像素成功；局部wrapper+Build0B。
尚未减少上传，生产分组报告/真实基线/完整链0GC/1000AI/Android继续待验收。
子批10显式生产冻结快照/JSON及同source同epoch窗口已补，新58/58+旧225/225（去重283）。
跨Reset/来源/非累计端点或counter倒退整份拒绝；导出非热路径有分配，未自动开启采样。
真实生产窗口/基线、完整链0GC/dirty优化/收益/1000AI/Android继续待验收。
局部插值Prepare/lookup及辅助双backend交错帧0B，详进度总表。
2026-10-07子批14原Battle自然两窗口已通过：101回归，camera128+136/Build256+272，
局部记录分配0B/growth0，关闭三残留0/Scene不变；发现同显示帧LateUpdate与camera两物化。
本优化任务仍无新的Player/GPU/完整渲染链0GC/1000AI或真机性能证据；统一结果见进度总表。

子批15 warm中央host只Queue与原clock取样，geometry留原camera；14新+135旧Passed。
before132camera/264Build，after143/143及118/118；每camera实体vertex1408→704bytes。
两次重进零扩容/失败/拒绝、记录分配0B/关闭三残留0/Scene同SHA；动态/像素/完整链和设备仍开放。

| 总体能力 | 当前进度 | 留痕与限制 |
|---|---|---|
| Unity/Android 工具链 | 版本已核对；工具链安装沿用历史观察 | Unity 2022.3.62f3；本轮未启动构建核验 SDK/NDK |
| Android 正式构建 | 场景列表已接线；成品/冷启动待验收 | Menu 第一、Battle 第二；既有 Editor 回调通过不能晋升 Android |
| Android 内容/触屏/ARM64 | 相应落地或认证门仍开放 | LoganRuntime File 路径、键盘 bindings、ARMv7 配置仍在 |
| 中央渲染 | 动态 Mesh 与插值代码已存在；设备未认证 | 同 publication 取样变化可重新物化；UV 采样边界须保留 |
| 1000 active 容量与预热 | runtime 档位与容量预热已存在 | 1050 slots/1000 max active 不等于 AI/GPU 吞吐通过 |
| 当前 1000 AI 性能 | 新鲜统一证书缺失 | 已有 DataOriented/SoA、碰撞候选优化路径不能代替当前 Player/设备实测 |
| 视觉内存 | 全量预热仍在；图集超预算可回源纹理 | 回退不代表全局内存或低端机 G1/G2 达标 |
| 音频内存 | 正式音频/PCM 适配已存在；优化待批 | 约 210 MiB 为已有副本 payload 估算，不是实测 RSS/峰值 |
| 当前战斗正确性 | 用户接受限定收尾；优化回归仍强制 | 当前336B44及已声明例外保持，不自动重开旧 campaign |
| Mono/Core/Presentation | 方案及正文修正已记录；代码实施仍 USER_HOLD | 从 B0 重新 inventory，不能把目录整理当分层完成 |
| 本轮优化 | 34项高12/中14/低8；父项关闭0；限定产物5/6、阶段条件通过4/6；已执行19批/38仅PLANNED | 40普通Brute接入限定通过，41生成准入分配源聚焦修复；最新千人logic P95仍107.065/113.849ms、显示约3.7—3.9FPS；H11完整camera2事件未定位、严格FAIL。阶段未完成，继续范围内必要工作；次数不构成停止。本次Goal实返active，旧blocked仅历史；实际进度只见统一总表，本次文档对账无运行时收益 |

条目共 **34 项：高 12、中 14、低 8**。原 28 项保留 ID/路径，新增 H-10/H-11、
M-12～M-15 六项；M-03 从中调高但不改旧 ID。高优先级表示安全性/潜在成本/前置重要性，
不表示已测得最大瓶颈；表内顺序是建议关注顺序，不是未经批准的实施排程。
实现进展逐项记录，不再用“全部 IMPLEMENTATION_NOT_STARTED”掩盖既有代码进展。
`EXT-1` 是 M-02/M-03 的候选表示方案，不重复计为第35个风险。

## 2. 状态与留痕规则

- `OPEN`：未满足该项关闭条件；`SOLUTION_DOCUMENTED`：有方案而非实施通过。
- `*_EXISTS` / `*_PASS`：仅表述相应代码或具名既有证据；不能扩大验证层级。
- `PROFILING_REQUIRED` / `CERTIFICATION_PENDING`：缺测量/设备证据，不把推演写成事实。
- `IMPLEMENTATION_NOT_STARTED`：本项新优化尚未执行；`USER_HOLD`：原专项暂停继续保持。
- `BUILD_PASS`、`DEVICE_RUNTIME_PASS`、`PERFORMANCE_PASS`、`VERIFIED`：
  只有对应实际证据和独立文档关闭条件齐备后才可使用。
- `STALE`：代码、权威、内容、kernel、APK、workload 或设备指纹不匹配的旧结果。
- 文档整理时的`WAITING_USER_APPROVAL`已由用户后续启动授权替代；
  当前只启动总表中具名批次，未选条目保持排队/原专项门，不启动EXT-1专项M0。
- 必须实施/通过的阶段按验收判断完成，不按批次数、候选数、修复轮数或报告数量。
  未通过继续范围内必要定位/修复；无效候选停止采用，已完成评估不重复执行。
  单项等待新权限/设备不阻塞其它READY项；真实外部阻塞与技术未知项分别记载。

共同合同、证据分级、依赖、下一批建议及启动批准字段见
[本轮复核与启动门](battle-optimization-rebaseline-and-start-gates-20261006.md)。
主表不复制各项实施步骤和测试矩阵；进度变动必须同步独立方案并保留 supersede 留痕。

## 3. 高优先级

| ID | 一句话说明 | 当前进度 | 独立方案与验收 | 最新留痕 | 更新 |
|---|---|---|---|---|---|
| `H-08` | 全量视觉预热与源纹理回退仍可能造成高驻留及启动峰值。 | `OPEN / SOURCE_FALLBACK_EXISTS / PREBAKE_PENDING` | [H-08 方案](android-mobile-readiness/h-08-prebaked-visual-content-memory.md) | 全量 sprites/textures/atlas sources；Auto 超 atlas 预算保留 SourceTexture2D | 2026-10-06 |
| `H-10` | 全量音频预热与双声道 PCM 副本增加整局内存和启动成本。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED` | [H-10 方案](android-mobile-readiness/h-10-battle-audio-pcm-memory.md) | 已有报告：965 副本约 209.99 MiB 静态估算，非 RSS/峰值实测 | 2026-10-06 |
| `H-11` | 多类预热/硬限/resolver封口与native Mesh再预热已聚焦通过，完整 0GC 尚未闭合。 | `OPEN / RUNTIME_PENDING / CACHE_PREWARM_AND_CAPACITY_GUARD_FOCUSED_PASS` | [H-11 方案](android-mobile-readiness/h-11-presentation-capacity-zero-gc.md) | [批次进度](battle-optimization-progress-tracker.md)；子批05新12/12、旧120/120；父项未关闭 | 2026-10-06 |
| `M-03` | metadata/warm消重限定通过；22已补无authoring时GameConfig Foot生产入口，显式覆盖/禁用保持。 | `OPEN / RUNTIME_PENDING / FOOT_CONFIG_SCOPED_PASS` | [M-03 方案](android-mobile-readiness/m-03-dynamic-mesh-upload.md) | [批次进度](battle-optimization-progress-tracker.md)；22 32/32、1800camera每帧Foot2/Health2、三档catalog重放局部0B。20 FAIL/21诊断不改；有限选定链/A-B/关闭重进继续，全域0GC/1000AI/GPU/FPS/设备待验 | 2026-10-07 |
| `H-07` | 缺少能覆盖当前代码、内容、插值及音频链的 1000 AI 性能证书。 | `OPEN / CURRENT_CERTIFICATE_MISSING` | [H-07 方案](android-mobile-readiness/h-07-fresh-1000-ai-certificate.md) | 历史 Windows/Editor 压测不构成当前 Android 证书 | 2026-10-06 |
| `H-01` | 正式 DAT、角色图片和公共资源仍依赖项目文件路径，Android 部署链待闭合。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED` | [H-01 方案](android-mobile-readiness/h-01-content-deployment.md) | LoganRuntime 项目根 File 路径；D-023 背景/模式排除保持 | 2026-10-06 |
| `H-02` | Menu/Battle 场景列表已接线，但 Android 构建和冷启动闭包仍待验收。 | `OPEN / UNITY_SCENE_LIST_AND_EDITOR_CALLBACK_PASS / ANDROID_BUILD_PENDING` | [H-02 方案](android-mobile-readiness/h-02-android-build-scene-closure.md) | EditorBuildSettings 两场景；复用既有 Editor 限定报告 | 2026-10-06 |
| `H-04` | Android ARM64/IL2CPP 成品及平台确定性证据尚未齐备。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED` | [H-04 方案](android-mobile-readiness/h-04-il2cpp-arm64.md) | AndroidTargetArchitectures=1，不能代替 ARM64 成品 | 2026-10-06 |
| `H-03` | 触屏输入尚未进入正式固定 tick 输入链。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED` | [H-03 方案](android-mobile-readiness/h-03-touch-input-fixed-tick.md) | 正式 Input Actions 观察为 Keyboard bindings | 2026-10-06 |
| `H-06` | 碰撞默认仍走 BruteForce，现有 Role-aware/Sweep/树路径需新鲜准入与成本评估。 | `OPEN / OPTIMIZED_PATHS_EXIST / PARITY_AND_PROFILING_REQUIRED` | [H-06 方案](android-mobile-readiness/h-06-collision-broadphase-1000.md) | ResolveFormalCollectorMode；499,500 非实测固定 pair 数 | 2026-10-06 |
| `H-05` | 目标 Android GPU/API 的功能、内存及持续性能未取得当前认证。 | `OPEN / CERTIFICATION_PENDING` | [H-05 方案](android-mobile-readiness/h-05-device-gpu-certification.md) | 未找到可关闭该项的本轮 Android 证据；本轮未实测 | 2026-10-06 |
| `H-09` | 优化仍需绑定当前权威和限定正确性状态，不能以性能代替回归。 | `OPEN / SCOPED_CLOSURE_ACCEPTED / OPTIMIZATION_REGRESSION_GATE_PENDING` | [H-09 方案](android-mobile-readiness/h-09-ntsd28-correctness-alignment.md) | 336B44；用户接受限定收尾，不重启旧全量 campaign | 2026-10-06 |

## 4. 中优先级

| ID | 一句话说明 | 当前进度 | 独立方案与验收 | 最新留痕 | 更新 |
|---|---|---|---|---|---|
| `M-01` | worker 准入、同步回退与单飞背压可能限制吞吐，不能以启用开关代替实测。 | `OPEN / PROFILING_REQUIRED` | [M-01 方案](android-mobile-readiness/m-01-dedicated-worker-throughput.md) | 当前 Unity bindings 等拒绝条件；AI 已有 DataOriented/SoA 路径 | 2026-10-06 |
| `M-11` | Host/Core/Presentation 依赖尚未单向，需按现状重新清点并分批重构。 | `OPEN / IMPLEMENTATION_NOT_STARTED / USER_HOLD` | [M-11 方案](android-mobile-readiness/m-11-mono-core-presentation-layering.md) | B0-B9 保持；P-1…P-5 文档已落地不等于代码已改 | 2026-10-06 |
| `M-02` | 资源、模式和 chunk 会拆分物理 segment，CPU 命令数不能替代 GPU batch。 | `OPEN / DEVICE_MEASUREMENT_REQUIRED` | [M-02 方案](android-mobile-readiness/m-02-central-render-draw-segmentation.md) | DrawMesh 逐有效 segment；EXT-1 保持待批 | 2026-10-06 |
| `M-12` | 解码并发数未约束累计像素字节，上传等待与 staging 会推高峰值。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED` | [M-12 方案](android-mobile-readiness/m-12-decode-upload-byte-backpressure.md) | cpuSemaphore 释放后等待 upload；atlas sources 保留像素 | 2026-10-06 |
| `M-13` | 同 tick 多 cue 声音聚合重复扫描，密集音效可能放大 CPU 成本。 | `OPEN / HOTSPOT_CANDIDATE / PROFILING_REQUIRED` | [M-13 方案](android-mobile-readiness/m-13-sound-event-aggregation.md) | PresentSounds 前序去重/后序累加，最坏近二次；未测热点 | 2026-10-06 |
| `M-14` | 加载多阶段重读 catalog/图片 SHA，启动完整性校验可能重复消耗 I/O。 | `OPEN / STARTUP_COST_CANDIDATE / PROFILING_REQUIRED` | [M-14 方案](android-mobile-readiness/m-14-content-verification-io.md) | AssertInputsCurrent 多处调用；禁止直接删除完整性守卫 | 2026-10-06 |
| `M-15` | 本地 battle-kernel 依赖未纳入统一证书会削弱构建可复现性。 | `OPEN / CERTIFICATE_INTEGRATION_PENDING` | [M-15 方案](android-mobile-readiness/m-15-kernel-build-reproducibility.md) | Packages/manifest.json 指向相邻 Server 本地 package | 2026-10-06 |
| `M-05` | logic-only 支持不代表 GameObject shell 成本归零，需区分三类 workload。 | `OPEN / LOGIC_ONLY_PATH_EXISTS / MEASUREMENT_REQUIRED` | [M-05 方案](android-mobile-readiness/m-05-gameobject-shell-cost.md) | 真实首发角色与预热池仍有 shell；实际数量待测 | 2026-10-06 |
| `M-06` | runtime 容量预热已接入，但过量 shell 预建及峰值计划仍需评估。 | `OPEN / RUNTIME_CAPACITY_PREWARM_EXISTS / CAPACITY_PROFILE_REQUIRED` | [M-06 方案](android-mobile-readiness/m-06-pool-capacity-prewarm.md) | 按 runtime reservedCapacity 预热；不再以 10/200 判容量不足 | 2026-10-06 |
| `M-04` | 已有中央失败诊断仍需设备故障注入与有界报告闭环。 | `OPEN / DIAGNOSTICS_EXIST / DEVICE_FAULT_MATRIX_PENDING` | [M-04 方案](android-mobile-readiness/m-04-centralonly-fail-closed-diagnostics.md) | CommitCentralFailurePlan/原因码存在；不放宽 fail-closed | 2026-10-06 |
| `M-07` | Automatic 图形 API 的实际能力与回退策略尚未通过设备矩阵冻结。 | `OPEN / API_POLICY_NOT_FROZEN` | [M-07 方案](android-mobile-readiness/m-07-android-graphics-api-policy.md) | 配置不是错误；需 Vulkan/GLES3 能力/设备证据 | 2026-10-06 |
| `M-08` | 缺少当前构建 10～20 分钟热稳态证据。 | `OPEN / SUSTAINED_TEST_MISSING` | [M-08 方案](android-mobile-readiness/m-08-sustained-thermal-performance.md) | 冷机或短测不能代替持续认证 | 2026-10-06 |
| `M-09` | 证书需统一代码、权威、kernel、内容、插值、音频、设备及配置指纹。 | `OPEN / SCHEMA_INTEGRATION_PENDING` | [M-09 方案](android-mobile-readiness/m-09-performance-evidence-fingerprint.md) | 本轮补 schema 要求，不声称报告生成器已实现 | 2026-10-06 |
| `M-10` | 移动取景、黑区和触控仍需 Safe Area/宽屏/平板/旋转矩阵。 | `OPEN / DEVICE_MATRIX_PENDING` | [M-10 方案](android-mobile-readiness/m-10-mobile-viewport-safe-area.md) | 既有用户取景例外保留；实际设备待验收 | 2026-10-06 |

## 5. 低优先级

| ID | 一句话说明 | 当前进度 | 独立方案与验收 | 最新留痕 | 更新 |
|---|---|---|---|---|---|
| `L-01` | 正式 Android 包名尚需产品确认。 | `OPEN / PRODUCT_VALUE_REQUIRED` | [L-01 方案](android-mobile-readiness/l-01-application-identifier.md) | 不能凭默认 Standalone identifier 宣称正式包名就绪 | 2026-10-06 |
| `L-02` | 正式签名与升级链待安全配置。 | `OPEN / SECURE_CREDENTIAL_REQUIRED` | [L-02 方案](android-mobile-readiness/l-02-signing-keystore.md) | custom keystore 未开；本轮不创建/读取密钥 | 2026-10-06 |
| `L-03` | 发布 Target API 需在发布时按官方要求冻结。 | `OPEN / RELEASE_TARGET_NOT_FROZEN` | [L-03 方案](android-mobile-readiness/l-03-target-api.md) | TargetSdk=Automatic；本轮不推断未来商店政策 | 2026-10-06 |
| `L-04` | 屏幕方向策略需明确，避免旋转时布局与输入异常。 | `OPEN / PRODUCT_DECISION_REQUIRED` | [L-04 方案](android-mobile-readiness/l-04-orientation-policy.md) | 允许四方向旋转；最终产品决定待批 | 2026-10-06 |
| `L-05` | Graphics Jobs/FrameTiming 需按设备 A/B 决定。 | `OPEN / A_B_REQUIRED` | [L-05 方案](android-mobile-readiness/l-05-graphics-jobs-frame-timing.md) | 关闭不是错误，不直接打开开关 | 2026-10-06 |
| `L-06` | HDR 的视觉收益与移动端带宽成本待 A/B。 | `OPEN / A_B_REQUIRED` | [L-06 方案](android-mobile-readiness/l-06-urp-hdr.md) | URP HDR=1；未经视觉批准不直接关闭 | 2026-10-06 |
| `L-07` | AI profile tooltip 与空值 resolver 语义仍需同步。 | `OPEN / SCRIPT_CHANGE_NOT_STARTED` | [L-07 方案](android-mobile-readiness/l-07-ai-profile-description-drift.md) | DataOrientedCanonical 默认；仅文案修正不得改 AI 行为 | 2026-10-06 |
| `L-08` | 图标、版本、AAB 和包体预算等发布流程待收口。 | `OPEN / RELEASE_PREPARATION_NOT_STARTED` | [L-08 方案](android-mobile-readiness/l-08-release-packaging.md) | 优先级是研发先后，不豁免发布前必需项 | 2026-10-06 |

## 6. 进度更新记录

| 日期 | 更新 |
|---|---|
| 2026-09-05 | 建立只读审计，初始9高/10中/8低；这是当日状态，不作当前证书。 |
| 2026-09-06 | 增加一句话总览与 M-11；建立28项独立方案，主表收敛为进度/证据索引。 |
| 2026-09-23 | H-02 独立方案已记录 Menu/Battle 列表和 Editor 回调通过；主表此前未同步，2026-10-06更正。 |
| 2026-10-06 | 依据当前代码和限定报告重评；修正插值物化、碰撞路径、池容量、诊断、内容及正确性描述。 |
| 2026-10-06 | 新增6项独立方案；M-03调高，总计34项（12高/14中/8低）；同步 PERF/ATLAS/MONO/提案索引，实施与测量待再批准。 |
| 2026-10-06 | 后续用户批准开始；独立进度总表建立，H-11预热子批实际编译及7/7+8/8聚焦通过；父项/完整M0/Android未闭合。 |
| 2026-10-06 | 用户批准下一批；H-11容量封口新16/16、旧30/30、原Editor编译通过；整帧超限拒绝/旧有效提交和CPU lease保持，完整0GC/运行时/设备门保留。 |
| 2026-10-06 | 第三批辅助marker/bar封口新15/15、旧61/61通过；64次辅助双backend交错帧局部0B，不推出整条central0GC/帧率收益；父项与专项门保持。 |
| 2026-10-06 | H-11子批04：resolver Prepare封口新15/15、适配旧1/1、旧105/105；配置/冷未命中/满缓存局部0B，不丢解析。父项/完整链/Scene/设备开放，专项门未解冻。 |
| 2026-10-06 | H-11子批05：已失效native Mesh原恢复前移Prepare预热，新12/12、旧120/120；四组首次及尾部Build/Upload局部0B，真实重进/完整链/预算/设备门仍开放。 |
| 2026-10-06 | M-03子批06：每Build成功API顶点上传计数已补，新18/18、旧132/132；局部0B，未减少上传，未测GPU/帧率/1000AI/Android，父项开放。 |
| 2026-10-06 | M-03子批07：计数接入可选报告并在Present后冻结，新14/14+旧168/168；v5必测判据不变，仅接受样本/非GPU或全程累计；局部helper0B不替代完整链或性能验收，父项开放。 |
| 2026-10-06 | M-03子批09：实际Build准入/成功/失败及完成API按request三分类归属，新23/23+旧202/202；局部wrapper+Build0B，不是像素/全链/性能通过；未减少上传，生产分组报告/基线/设备开放。 |
| 2026-10-06 | M-03子批10：显式生产快照/JSON及同来源同epoch窗口，新58/58+旧225/225；导出非热路径有分配，不自动采样；真实基线/完整链/dirty/收益/设备开放。 |
| 2026-10-06 | M-03子批12：8阶段+12控制20/20、旧34/34；36000 Build局部0B/growth0，高segment metadata含descriptor mean1.14～1.20ms，非纯API/GPU。生产未改，下一最小A/B，父项/专项门保持。 |
| 2026-10-06 | M-03子批13：稳定fully-active多submesh批量metadata，仅4新增/1删除；改前26/26、改后20/20、相关71/71，91去重。局部1000段无诊断mean降46～47%，72000 Build局部0B/growth0/payload不变；非整场/GPU/设备，父OPEN/RUNTIME_PENDING与专项门保持。 |

本次整理任务与文件操作：
[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-DOC-REBASELINE-20261006.md) /
[操作记录](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-DOC-REBASELINE-20261006/RECORD.md)。

## 7. 当前对外状态

```text
DOCUMENT_REBASELINE                = DONE (static documentation only)
OPTIMIZATION_IMPLEMENTATION        = STARTED (H-11 prewarm batch01 / capacity seal batch02-04 / native mesh prewarm batch05)
H11_CACHE_PREWARM                  = FOCUSED_TEST_PASS / RUNTIME_PENDING
H11_CAPACITY_GUARD                 = FOCUSED_TEST_PASS / RUNTIME_PENDING
H11_AUXILIARY_CAPACITY_GUARD       = FOCUSED_TEST_PASS / RUNTIME_PENDING
H11_RESOLVER_PREPARE_SEAL         = FOCUSED_TEST_PASS / RUNTIME_PENDING
H11_NATIVE_MESH_REPREWARM         = FOCUSED_TEST_PASS / RUNTIME_PENDING
CURRENT_M0_EXECUTION               = NOT_STARTED_THIS_TASK
EXT1_SPECIAL_M0                   = NOT_STARTED_THIS_TASK
EXT1                              = PROPOSED / MODIFY_REQUIRED
ANDROID_BUILD_READY               = NOT_CERTIFIED
ANDROID_CONTENT_READY             = NOT_CERTIFIED
ANDROID_TOUCH_INPUT_READY         = NO_CURRENT_EVIDENCE
ANDROID_ARM64_READY               = NO_CURRENT_CERTIFICATE
ANDROID_CENTRAL_RENDER_CERTIFIED  = NO_CURRENT_CERTIFICATE
ANDROID_ORDINARY_BATTLE_CERTIFIED = NO_CURRENT_CERTIFICATE
ANDROID_1000_AI_CERTIFIED         = NO_CURRENT_CERTIFICATE
UNITY_SCENE_LIST                  = MENU_THEN_BATTLE
CENTRAL_RENDER_IMPLEMENTED        = YES
DISPLAY_INTERPOLATION_CODE        = EXISTS_NOT_FULL_CERTIFICATE
MOBILE_1000_ACTIVE_CAPACITY       = YES
DATA_ORIENTED_AI_DEFAULT          = YES
PRODUCTION_BROADPHASE             = BRUTE_FORCE
BATTLE_ALIGNMENT                 = USER_ACCEPTED_SCOPED_CLOSURE
```

历史 `CURRENT_EDITOR_SELFCHECK=PASS(2026-09-05)` 仅保留为旧留痕，不作当前统一验收。
优化文档整理本身不等于 Android 达标。后续总体启动授权已生效，但不取消明确的
EXT-1/MONO/资源/Scene/配置专项门；未测获益和未运行的验收保持未知。
