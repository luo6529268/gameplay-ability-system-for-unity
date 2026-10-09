# 优化依赖与阶段顺序

状态：DOCUMENTED / IMPLEMENTATION_STOPPED。依赖依据为34单项方案、共同启动门、PERF/ATLAS/MONO相关合同及2026-10-09交接；具体代码依赖进入具名实施前再次核对。以下为方案层依赖，不冒充已运行证明。

## 为何单独建文档

已发现资源共享前置、Android构建/运行/最终认证不同阶段、worker与暂停架构专项依赖。直接按父项“全部完成”排序会出现表面循环：H01↔H08、H02↔H04、H05↔M07/M08、M09↔M15。按所需产物拆阶段即可解释，不需新增运行框架。

规则：硬前置必须满足；验证前置只阻塞相应验收；共享接口先协商不要求两父项都完成；共享文件是写冲突不是自动先后；可选增益不设硬门。未知依赖标UNKNOWN，不能假定不存在。暂缓/未授权依赖不能自动解冻，也不阻塞无关已授权项目。

## 阶段节点与必须交付的前置

| 节点 | 最小可交付条件 |
|---|---|
| CONTENT-CONTRACT | H01/H08/H10共享key、闭包、Manifest、Lease与失效合同经批准；不是资源全量实施 |
| H01-PROVIDER | 选定平台内容可加载且身份/取消可验证 |
| H08-BAKE / H10-PREPARE / M12-BACKPRESSURE | 各自视觉/音频/队列范围实施，互不要求整个父项全通过 |
| H04-CONFIG | ARM64/AOT/native闭包配置已准备，不等待设备全部跑完 |
| M15-IDENTITY | 本地Kernel实际来源与内容身份已采集，不要求更换包源 |
| M09-FINGERPRINT | 选定平台必需字段齐备；Windows无APK时明确平台N/A |
| H02-BUILD | 可审计Development产物，正式签名不是debug包前置 |
| H02-RUNTIME / H04-RUNTIME | 可安装包+部署内容具备后再做冷启动/平台回归 |
| API-INITIAL-MATRIX | 候选API/能力/设备组合明确，不是最终API策略 |
| DEVICE-FUNCTION | 设备基础功能/渲染/生命周期通过，不是H05最终认证 |
| M07-FINAL-POLICY | 依据初始矩阵结果决定实际API策略 |
| H07-ANDROID / M08-THERMAL | 对同指纹包分别认证吞吐与热稳态 |
| H05-FINAL | 汇总规定设备功能/性能/热态；不反过来阻塞其前置测量 |
| L04-DECISION / M10-LAYOUT / H03-TOUCH | 产品方向→共享布局合同→触控接入，不能互相等待整体验收 |
| M11-B0-B6-L1 | 原架构专项明确阶段成果，USER_HOLD未解除 |
| M01-WORKER-ADMISSION | 只有worker资格深化依赖该架构阶段，现有局部成本分析不依赖 |
| IMMUTABLE-MANIFEST-LEASE | M14安全复用必要的不可变身份与读持有语义 |
| M14-REUSE | 校验结论复用，仍保留必要校验和可变Editor守卫 |
| L01-IDENTIFIER / L02-SIGNING / L03-SDK / L08-RELEASE | 产品值/安全配置/发布要求→Release收口；不阻塞当前Windows优化 |

硬阶段边如下，箭头表示“前置 → 消费者”；本图仅用于方案顺序检查，不授权执行：

- `CONTENT-CONTRACT -> H01-PROVIDER`
- `CONTENT-CONTRACT -> H08-BAKE`
- `CONTENT-CONTRACT -> H10-PREPARE`
- `CONTENT-CONTRACT -> M12-BACKPRESSURE`
- `H01-PROVIDER -> H02-RUNTIME`
- `H04-CONFIG -> H02-BUILD`
- `M15-IDENTITY -> M09-FINGERPRINT`
- `M09-FINGERPRINT -> H02-BUILD`
- `H02-BUILD -> H02-RUNTIME`
- `H02-RUNTIME -> H04-RUNTIME`
- `H02-RUNTIME -> DEVICE-FUNCTION`
- `API-INITIAL-MATRIX -> DEVICE-FUNCTION`
- `DEVICE-FUNCTION -> M07-FINAL-POLICY`
- `DEVICE-FUNCTION -> M08-THERMAL`
- `DEVICE-FUNCTION -> H07-ANDROID`
- `H07-ANDROID -> H05-FINAL`
- `M08-THERMAL -> H05-FINAL`
- `M07-FINAL-POLICY -> H05-FINAL`
- `L04-DECISION -> M10-LAYOUT`
- `M10-LAYOUT -> H03-TOUCH`
- `M11-B0-B6-L1 -> M01-WORKER-ADMISSION`
- `IMMUTABLE-MANIFEST-LEASE -> M14-REUSE`
- `L01-IDENTIFIER -> L08-RELEASE`
- `L02-SIGNING -> L08-RELEASE`
- `L03-SDK -> L08-RELEASE`
- `H02-BUILD -> L08-RELEASE`

## 当前Windows主线

- H06 Role候选准入：D1决定→准确写域/受影响门冻结→候选验证→满足批准门后默认切换。83—86到约44ms是历史比较，不是本轮收益承诺。
- H07-INPUT：已有91归因/字段合同→具名热点方案→获批实施；Role默认不是它的硬前置，但A/B必须固定同一种collector。
- H07-GC95只阻塞使用该失败fixture或相关分配生产路径的资格门，不全局阻塞Input只读分析或已有实景证据。
- H07-LATE先细分成本与读范围授权，不能靠publication只读推断缓存安全。
- H06-SPATIAL是Role比较基线下的条件候选；不依赖必须把Input优化完才允许评估。
- H07-AI-MODE需独立行为合同，不依赖普通模式必须先失败到“无路可走”，也不替代原模式验收。
- H09/H11/M09/M15是按影响域伴随门，不要求其Android父项全部关闭才做Windows局部工作。

## 34项依赖覆盖

各项精确摘要在下表；没有列“无依赖”的项不代表已证明零依赖。进入实施前若出现新硬前置，更新本表及受影响方案/进度，先交付所需最小前置阶段。

| ID | 所需最小前置或验证条件 |
|---|---|
| [H-01](../../../Assets/NTSD/Docs/android-mobile-readiness/h-01-content-deployment.md) | 盘点项目路径读取，确定内容key与失效协议，先shadow compare后换运行入口 |
| [H-02](../../../Assets/NTSD/Docs/android-mobile-readiness/h-02-android-build-scene-closure.md) | 沿用Menu/Battle顺序；Build Profile指记录化构建配置/脚本，不默认要求Unity6专有UI |
| [H-03](../../../Assets/NTSD/Docs/android-mobile-readiness/h-03-touch-input-fixed-tick.md) | 复用现有Input Actions和FrameInputSet，处理多指/失焦释放；Prefab/Scene/Input Actions另批 |
| [H-04](../../../Assets/NTSD/Docs/android-mobile-readiness/h-04-il2cpp-arm64.md) | 盘点AOT/native/stripping并冻结Development配置；不改第三方或包源 |
| [H-05](../../../Assets/NTSD/Docs/android-mobile-readiness/h-05-device-gpu-certification.md) | 先功能与故障门，再分API/设备性能；目标设备和预算需用户确认 |
| [H-06](../../../Assets/NTSD/Docs/android-mobile-readiness/h-06-collision-broadphase-1000.md) | 先现有Role生产准入设计，复用81/82四形状/Driver证据，只补实际覆盖缺口；空间分区为独立后续候选 |
| [H-07](../../../Assets/NTSD/Docs/android-mobile-readiness/h-07-fresh-1000-ai-certificate.md) | 已有窗口按父子计时整理；按需补缺，不把诊断当优化，不恢复旧自动目标 |
| [H-08](../../../Assets/NTSD/Docs/android-mobile-readiness/h-08-prebaked-visual-content-memory.md) | 先保守生产者闭包和生命周期，再预印/按局装载；已有共享绑定不是完整ATLAS实现 |
| [H-09](../../../Assets/NTSD/Docs/android-mobile-readiness/h-09-ntsd28-correctness-alignment.md) | 逐候选声明输入/RNG/候选顺序/事件/状态比较域；不重启旧全量对齐 |
| [H-10](../../../Assets/NTSD/Docs/android-mobile-readiness/h-10-battle-audio-pcm-memory.md) | 统计cue/原clip/播放副本/暂存生命周期，评估按局准备与构建期等价适配 |
| [H-11](../../../Assets/NTSD/Docs/android-mobile-readiness/h-11-presentation-capacity-zero-gc.md) | 复用86两轮1800camera和容量证据；只处理新影响或父项未覆盖域 |
| [M-01](../../../Assets/NTSD/Docs/android-mobile-readiness/m-01-dedicated-worker-throughput.md) | 先核eligible/active/fallback/等待；不将worker默认true当实际并行 |
| [M-02](../../../Assets/NTSD/Docs/android-mobile-readiness/m-02-central-render-draw-segmentation.md) | 保留80同Sakura SetPass1995→5、CPU DrawMesh约2；多资源场景有证再优化 |
| [M-03](../../../Assets/NTSD/Docs/android-mobile-readiness/m-03-dynamic-mesh-upload.md) | 保留13 metadata和15每camera消重；脏区优化先区分publication/alpha/完全重复显示 |
| [M-04](../../../Assets/NTSD/Docs/android-mobile-readiness/m-04-centralonly-fail-closed-diagnostics.md) | 复用reason/首错字段，缺哪个补哪个；故障注入用受控夹具不删除生产资产 |
| [M-05](../../../Assets/NTSD/Docs/android-mobile-readiness/m-05-gameobject-shell-cost.md) | 分别数logic-only/active/visible/inactive shell；不假设1000AI=1000Mono |
| [M-06](../../../Assets/NTSD/Docs/android-mobile-readiness/m-06-pool-capacity-prewarm.md) | 按slot/command/shell分别清点峰值，不能用GameConfig旧10/200判断现容量 |
| [M-07](../../../Assets/NTSD/Docs/android-mobile-readiness/m-07-android-graphics-api-policy.md) | 先候选Vulkan/GLES矩阵后定策略；Automatic并非已证错误 |
| [M-08](../../../Assets/NTSD/Docs/android-mobile-readiness/m-08-sustained-thermal-performance.md) | 固定温度/功耗等环境，按原10—20分钟门评价 |
| [M-09](../../../Assets/NTSD/Docs/android-mobile-readiness/m-09-performance-evidence-fingerprint.md) | 沿用现有请求和报告；平台不适用字段显式N/A，不造第二套harness |
| [M-10](../../../Assets/NTSD/Docs/android-mobile-readiness/m-10-mobile-viewport-safe-area.md) | 先viewport/Safe Area合同后触控适配；保留用户底部黑区和平台取景 |
| [M-11](../../../Assets/NTSD/Docs/android-mobile-readiness/m-11-mono-core-presentation-layering.md) | 沿用原B0-B9方案；目录整理不是分层完成；本轮不改正文架构 |
| [M-12](../../../Assets/NTSD/Docs/android-mobile-readiness/m-12-decode-upload-byte-backpressure.md) | 区分待上传和仍被atlas持有的像素；保留UniTask，取消归还reservation |
| [M-13](../../../Assets/NTSD/Docs/android-mobile-readiness/m-13-sound-event-aggregation.md) | 现有评估未建立真实热点，冻结原算法；不因二次复杂度描述直接改 |
| [M-14](../../../Assets/NTSD/Docs/android-mobile-readiness/m-14-content-verification-io.md) | 保留可变Editor stale守卫；前置成立后才评估同epoch复用 |
| [M-15](../../../Assets/NTSD/Docs/android-mobile-readiness/m-15-kernel-build-reproducibility.md) | 记录相邻本地package精确文件身份，复用现有工具；不写Server/更换包源 |
| [L-01](../../../Assets/NTSD/Docs/android-mobile-readiness/l-01-application-identifier.md) | 冻结dev/release命名与Manifest一致性 |
| [L-02](../../../Assets/NTSD/Docs/android-mobile-readiness/l-02-signing-keystore.md) | 用户安全环境配置；文档只公开证书指纹，不读取私钥/口令 |
| [L-03](../../../Assets/NTSD/Docs/android-mobile-readiness/l-03-target-api.md) | 实施时查询官方要求并冻结min/target；本轮不写未经核实的政策值 |
| [L-04](../../../Assets/NTSD/Docs/android-mobile-readiness/l-04-orientation-policy.md) | 比较横屏左右与允许旋转的输入/布局成本，产品决定后再改设置 |
| [L-05](../../../Assets/NTSD/Docs/android-mobile-readiness/l-05-graphics-jobs-frame-timing.md) | 采集开销与Jobs性能分开；不能盲开开关 |
| [L-06](../../../Assets/NTSD/Docs/android-mobile-readiness/l-06-urp-hdr.md) | 保持其他配置做HDR对照；先有GPU成本再定 |
| [L-07](../../../Assets/NTSD/Docs/android-mobile-readiness/l-07-ai-profile-description-drift.md) | 只同步tooltip/说明，不改resolver或asset；既有测试优先复用 |
| [L-08](../../../Assets/NTSD/Docs/android-mobile-readiness/l-08-release-packaging.md) | 版本/图标/AAB/包体预算统一收口；不自动上传商店 |


## 写冲突与证据失效

BruteForceSceneQuery由Role准入/空间裁剪共享，不能并行改；AI/World/publisher由INPUT/GC95/AI-MODE可能共享，先准确划写域再并行；CharacterAnimtorManager由H08/M12/M02共享；App/生命周期由资源和运行入口共享。
只读分析可并行，代码任务独立且写域不重叠才委派；架构/合同决定与最终集成留主代理。
字段、候选顺序、发布时点或Kernel变化可能使相关正确性证据过期；资源身份/模式变更可能使性能基线不再可比；不触及相应依赖的修改不要求全量重跑。取哈希不等于已核行为，复用理由要写入具名Task。
