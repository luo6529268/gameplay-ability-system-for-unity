# Android 移动端就绪度与 1000 AI 风险登记表

> 文档 ID：`ANDROID-MOBILE-READINESS-RISK-REGISTER-001`  
> 建立日期：2026-09-05  
> 最后更新：2026-09-06  
> 当前状态：`REGISTER_ONLY / ANDROID_NOT_CERTIFIED / IMPLEMENTATION_NOT_STARTED`  
> 适用项目：`gameplay-ability-system-for-unity`  
> 本文只维护问题说明、状态、进度、详情入口和证据留痕；解决方案、验收条件与测试矩阵统一维护在每个问题的独立文档中。  
> 本次文档整理不授权修改脚本、Scene、Prefab、DAT、纹理、ProjectSettings 或构建配置。

## 1. 当前结论与总进度

当前项目不能被判定为“已能在 Android 手机上正常运行”，也不能承诺中端 Android 持续稳定运行 1000 个完整 AI。

| 总体能力 | 当前进度 | 最近留痕 |
|---|---|---|
| Android 工具链 | 已安装 | Unity 2022.3.62f3 对应 Android Player、SDK、NDK、OpenJDK 已观察到 |
| Android 正式构建 | 未就绪 | Build Settings 没有正式 Scene 闭包，未找到可复核 APK/AAB |
| Android 内容部署 | 阻塞 | 生产路径仍读取项目 DAT/BMP/SPARK 文件路径 |
| Android 触屏输入 | 未开始 | 正式 Input Actions 只观察到 Keyboard bindings |
| Android ARM64 | 未就绪 | 当前序列化目标为 ARMv7，未取得 ARM64 IL2CPP 成品证据 |
| 中央渲染 | 已实现，真机未认证 | CentralOnly、URP BattleRenderFeature、动态 Mesh 已接线 |
| 1000 active 容量 | 已实现 | MobileExtended 定义 1050 runtime slots / 1000 max active |
| 1000 AI 当前性能 | 未认证 | 只有较早 Windows/Editor 历史数据，没有当前 APK 真机证书 |
| NTSD 2.8-Logan 正确性 | 对齐进行中 | 当前权威状态仍包含 `B3_ALIGNMENT_IN_PROGRESS` |
| Mono/Core/Presentation 分层 | 方案已记录，实施暂停 | 当前仍有 Core→Renderer/MountRegistry/Mono Driver 反向依赖；专项计划为 `USER_HOLD` |

问题总数与文档进度：

```text
高优先级：9 项；9 项已建立独立方案文档；0 项实现关闭
中优先级：11 项；11 项已建立独立方案文档；0 项实现关闭
低优先级：8 项；8 项已建立独立方案文档；0 项实现关闭
总计：28 项；28 项方案文档齐全；ANDROID_NOT_CERTIFIED
```

## 2. 状态与留痕规则

| 状态 | 主登记表含义 |
|---|---|
| `OPEN` | 问题存在，尚未达到关闭条件。 |
| `SOLUTION_DOCUMENTED` | 独立文档已经记录解决方案、验收和测试条件；不表示代码已写。 |
| `IMPLEMENTATION_NOT_STARTED` | 没有获得或执行对应实现工作。 |
| `CODE_WRITTEN` | 实现存在，但没有完成目标构建/运行。 |
| `BUILD_PASS` | Android BuildReport 成功；不能扩大为真机运行通过。 |
| `DEVICE_RUNTIME_PASS` | 指定设备和步骤实际通过。 |
| `PERFORMANCE_PASS` | 指定 workload、采样长度和门槛全部通过。 |
| `VERIFIED` | 对应独立文档的实现、构建、真机、性能、生命周期和必要 parity 证据完整。 |
| `STALE` | 曾有结果，但代码、内容、APK、设备或权威指纹已不再匹配。 |
| `USER_HOLD` | 用户明确暂停实施；风险仍存在。 |

主登记表的“最近留痕”只记录最新、足以识别状态的证据摘要；完整解决方案、测试步骤、报告路径和历史更新必须写入对应独立文档。任何状态变化都要同步更新本表和独立文档，禁止只在聊天中口头关闭。

## 3. 高优先级进度

| ID | 问题说明 | 当前进度 | 方案与验收文档 | 最近留痕 | 更新 |
|---|---|---|---|---|---|
| `H-01` | DAT、BMP、SPARK 仍按编辑器/开发机文件路径读取，Android Player 无法可靠加载正式内容。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED` | [内容部署方案](android-mobile-readiness/h-01-content-deployment.md) | `File.*`、`Assets/...` 与绝对路径调用仍在生产加载链 | 2026-09-06 |
| `H-02` | Build Settings 没有正式 Scene 闭包，也没有可复核 APK/AAB。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED` | [Build 与 Scene 闭包方案](android-mobile-readiness/h-02-android-build-scene-closure.md) | `EditorBuildSettings.asset` 为 `m_Scenes: []` | 2026-09-06 |
| `H-03` | 手机触屏尚未接入固定 tick 的正式输入链。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED` | [触屏输入方案](android-mobile-readiness/h-03-touch-input-fixed-tick.md) | Input Actions 只观察到 Keyboard bindings | 2026-09-06 |
| `H-04` | Android 当前只目标 ARMv7，缺少 ARM64 IL2CPP 成品和真机验证。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED` | [IL2CPP 与 ARM64 方案](android-mobile-readiness/h-04-il2cpp-arm64.md) | `AndroidTargetArchitectures: 1`，当前枚举对应 ARMv7 | 2026-09-06 |
| `H-05` | Adreno、Mali、Vulkan、GLES3、内存和持续性能均无 Android 真机证书。 | `OPEN / SOLUTION_DOCUMENTED / CERTIFICATION_NOT_STARTED` | [真机与 GPU 认证方案](android-mobile-readiness/h-05-device-gpu-certification.md) | 没有可复核 Android Player 设备报告 | 2026-09-06 |
| `H-06` | 生产碰撞默认仍为 BruteForce，1000 实体聚集时可能达到 499,500 对。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED` | [Broadphase 方案](android-mobile-readiness/h-06-collision-broadphase-1000.md) | 空配置解析为 BruteForce；LooseQuadtree 尚无当前强一致 A/B | 2026-09-06 |
| `H-07` | 1000 AI 最好性能数据属于较早工作树，不能证明当前代码和 Android 达标。 | `OPEN / SOLUTION_DOCUMENTED / CURRENT_CERTIFICATE_MISSING` | [1000 AI 新证书方案](android-mobile-readiness/h-07-fresh-1000-ai-certificate.md) | 2026-08-23 结果仅作历史参考，当前原始 JSON 不可复核 | 2026-09-06 |
| `H-08` | 全量 BMP 运行时解码、逐帧 Sprite 和中央图集预热可能造成严重启动内存峰值。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED` | [视觉预烘焙与内存方案](android-mobile-readiness/h-08-prebaked-visual-content-memory.md) | 约 408 MB 源图；加载期多类 staging 与最终 atlas 共存 | 2026-09-06 |
| `H-09` | NTSD 2.8-Logan 重新对齐尚未完成，性能通过不能替代战斗正确性。 | `OPEN / SOLUTION_DOCUMENTED / ALIGNMENT_IN_PROGRESS` | [正确性与移动证书绑定方案](android-mobile-readiness/h-09-ntsd28-correctness-alignment.md) | 当前权威改变 pass 基线，旧 2.4/旧证书无裁决权 | 2026-09-06 |

## 4. 中优先级进度

| ID | 问题说明 | 当前进度 | 方案与验收文档 | 最近留痕 | 更新 |
|---|---|---|---|---|---|
| `M-01` | Dedicated worker 单线程单飞，不能自动并行加速 1000 AI。 | `OPEN / SOLUTION_DOCUMENTED / PROFILING_REQUIRED` | [Worker 吞吐方案](android-mobile-readiness/m-01-dedicated-worker-throughput.md) | 单 `System.Threading.Thread`、输入容量 1、publication ack 等待 | 2026-09-06 |
| `M-02` | 中央渲染按资源 segment、FootSelf 和血条分别提交，不保证全战斗固定一个 draw。 | `OPEN / SOLUTION_DOCUMENTED / DEVICE_MEASUREMENT_REQUIRED` | [Draw Segmentation 方案](android-mobile-readiness/m-02-central-render-draw-segmentation.md) | 资源/材质/page/chunk 会拆 segment | 2026-09-06 |
| `M-03` | 中央动态 Mesh 每个可见帧仍需构建命令和上传 GPU。 | `OPEN / SOLUTION_DOCUMENTED / DEVICE_PROFILING_REQUIRED` | [动态 Mesh 上传方案](android-mobile-readiness/m-03-dynamic-mesh-upload.md) | 存在命令解析、Quad 写入、submesh 与 `SetVertexBufferData` | 2026-09-06 |
| `M-04` | CentralOnly fail-closed 可能把资源/提交故障表现为角色不可见。 | `OPEN / SOLUTION_DOCUMENTED / DIAGNOSTICS_INCOMPLETE` | [Fail-Closed 诊断方案](android-mobile-readiness/m-04-centralonly-fail-closed-diagnostics.md) | Legacy materializer 被抑制，错误 reason 闭环未认证 | 2026-09-06 |
| `M-05` | 1000 实体仍可能保留真实 GameObject、Transform、wrapper、mount 与池记录。 | `OPEN / SOLUTION_DOCUMENTED / MEASUREMENT_REQUIRED` | [GameObject Shell 成本方案](android-mobile-readiness/m-05-gameobject-shell-cost.md) | 压测 harness 创建正式角色 GameObject | 2026-09-06 |
| `M-06` | 当前池初始容量与 1000 active 目标差距较大。 | `OPEN / SOLUTION_DOCUMENTED / CAPACITY_PROFILE_REQUIRED` | [对象池容量与预热方案](android-mobile-readiness/m-06-pool-capacity-prewarm.md) | GameConfig 为 10/200，MobileExtended 为 1000 active | 2026-09-06 |
| `M-07` | Android Graphics API 为 Automatic，Vulkan/GLES3 差异尚未锁定。 | `OPEN / SOLUTION_DOCUMENTED / API_POLICY_NOT_FROZEN` | [图形 API 策略方案](android-mobile-readiness/m-07-android-graphics-api-policy.md) | 无 API/设备 allowlist、denylist 或独立证书 | 2026-09-06 |
| `M-08` | 没有 10～20 分钟持续负载和热降频证据。 | `OPEN / SOLUTION_DOCUMENTED / SUSTAINED_TEST_MISSING` | [持续负载与热降频方案](android-mobile-readiness/m-08-sustained-thermal-performance.md) | 当前仅有短时/历史性能信息 | 2026-09-06 |
| `M-09` | 性能报告缺少统一 APK、源码、内容、设备、GPU 和运行配置指纹。 | `OPEN / SOLUTION_DOCUMENTED / SCHEMA_NOT_IMPLEMENTED` | [性能证据指纹方案](android-mobile-readiness/m-09-performance-evidence-fingerprint.md) | 当前报告无法稳定关联当前 dirty worktree 与 APK | 2026-09-06 |
| `M-10` | 移动端取景和底部黑区尚未覆盖 Safe Area、超宽屏、平板和旋转矩阵。 | `OPEN / SOLUTION_DOCUMENTED / DEVICE_MATRIX_PENDING` | [Viewport 与 Safe Area 方案](android-mobile-readiness/m-10-mobile-viewport-safe-area.md) | 用户已确认保留底部黑区/平台取景，设备矩阵未执行 | 2026-09-06 |
| `M-11` | Host/Core/Presentation 只有目录划分，仍存在 Renderer、MountRegistry、具体 Mono Driver 反向依赖。 | `OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD` | [Mono/Core/Presentation 分层方案](android-mobile-readiness/m-11-mono-core-presentation-layering.md) | 当前代码复核仍命中 World/Registry/Stage/Snapshot/Lockstep 反向依赖 | 2026-09-06 |

## 5. 低优先级进度

| ID | 问题说明 | 当前进度 | 方案与验收文档 | 最近留痕 | 更新 |
|---|---|---|---|---|---|
| `L-01` | Android 正式 application identifier 尚未配置。 | `OPEN / SOLUTION_DOCUMENTED / PRODUCT_VALUE_REQUIRED` | [Application Identifier 方案](android-mobile-readiness/l-01-application-identifier.md) | 未观察到正式 Android 包名 | 2026-09-06 |
| `L-02` | 正式 keystore 和可升级发布签名链尚未配置。 | `OPEN / SOLUTION_DOCUMENTED / SECURE_CREDENTIAL_REQUIRED` | [签名与 Keystore 方案](android-mobile-readiness/l-02-signing-keystore.md) | `androidUseCustomKeystore=0`，alias 为空 | 2026-09-06 |
| `L-03` | Target API 为 Automatic，实际发布目标尚未冻结。 | `OPEN / SOLUTION_DOCUMENTED / RELEASE_TARGET_NOT_FROZEN` | [Target API 方案](android-mobile-readiness/l-03-target-api.md) | `AndroidTargetSdkVersion=0` | 2026-09-06 |
| `L-04` | 当前允许所有方向自动旋转，可能影响横屏布局和输入状态。 | `OPEN / SOLUTION_DOCUMENTED / PRODUCT_DECISION_REQUIRED` | [屏幕方向策略](android-mobile-readiness/l-04-orientation-policy.md) | 四方向 autorotation 均允许 | 2026-09-06 |
| `L-05` | Graphics Jobs 与 FrameTiming 尚未形成 Android A/B。 | `OPEN / SOLUTION_DOCUMENTED / A_B_REQUIRED` | [Graphics Jobs 与 FrameTiming 方案](android-mobile-readiness/l-05-graphics-jobs-frame-timing.md) | Graphics Jobs=0、FrameTimingStats=0 | 2026-09-06 |
| `L-06` | URP HDR 已开启，但移动端画质收益和带宽成本未测量。 | `OPEN / SOLUTION_DOCUMENTED / A_B_REQUIRED` | [URP HDR 评估方案](android-mobile-readiness/l-06-urp-hdr.md) | URP `m_SupportsHDR=1` | 2026-09-06 |
| `L-07` | AI profile tooltip 与 resolver 空值默认语义不一致。 | `OPEN / SOLUTION_DOCUMENTED / SCRIPT_CHANGE_NOT_STARTED` | [AI Profile 说明修正方案](android-mobile-readiness/l-07-ai-profile-description-drift.md) | 生产 asset 显式为 DataOrientedCanonical，当前行为未受 tooltip 影响 | 2026-09-06 |
| `L-08` | Android 图标、版本、AAB、包体预算和发布检查未完成。 | `OPEN / SOLUTION_DOCUMENTED / RELEASE_PREPARATION_NOT_STARTED` | [发布打包方案](android-mobile-readiness/l-08-release-packaging.md) | icons 为空、version code=1、无 AAB 证据 | 2026-09-06 |

## 6. 进度更新记录

| 日期 | 更新 |
|---|---|
| 2026-09-05 | 建立 Android 移动端就绪度只读审计，初始登记 9 个高、10 个中、8 个低优先级问题。 |
| 2026-09-06 | 在开头增加全部问题的一句话总览。 |
| 2026-09-06 | 新增 `M-11` Mono Host / Simulation Core / Presentation L1 分层风险，总数更新为 28。 |
| 2026-09-06 | 为 28 个问题建立独立方案文档，迁移解决方案、验收条件与测试矩阵；主文档收敛为进度和证据登记表。 |

## 7. 当前对外状态

```text
ANDROID_BUILD_READY               = NO
ANDROID_CONTENT_READY             = NO
ANDROID_TOUCH_INPUT_READY         = NO
ANDROID_ARM64_READY               = NO
ANDROID_CENTRAL_RENDER_CERTIFIED  = NO
ANDROID_ORDINARY_BATTLE_CERTIFIED = NO
ANDROID_1000_AI_CERTIFIED         = NO
CURRENT_EDITOR_SELFCHECK          = PASS (existing result: 2026-09-05 23:07:48)
CENTRAL_RENDER_IMPLEMENTED        = YES
MOBILE_1000_ACTIVE_CAPACITY       = YES
DATA_ORIENTED_AI_DEFAULT          = YES
PRODUCTION_BROADPHASE             = BRUTE_FORCE
```

在对应高优先级构建、内容、输入、ARM64、真机、1000 AI 与正确性证据完成前，对外状态保持：

`ANDROID_NOT_CERTIFIED / CONTENT_DEPLOYMENT_BLOCKED / TOUCH_INPUT_MISSING / 1000_AI_DEVICE_GATE_OPEN`。
