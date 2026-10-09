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

- H06 Role候选准入：D1=A已批准→准确写域/受影响门冻结→候选验证→满足批准门后默认切换。83—86到约44ms是历史比较，不是本轮收益承诺。
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
| [H-01](../../../Assets/NTSD/Docs/android-mobile-readiness/h-01-content-deployment.md) | Manifest/Provider接口与资源载体先确认；H-08/H-10共享闭包，不等其父项全验收 |
| [H-02](../../../Assets/NTSD/Docs/android-mobile-readiness/h-02-android-build-scene-closure.md) | H-04构建配置；H-01可部署内容用于运行验收；M-09/M-15指纹 |
| [H-03](../../../Assets/NTSD/Docs/android-mobile-readiness/h-03-touch-input-fixed-tick.md) | L-04方向策略、M-10布局合同；设备验收需H-02可运行包 |
| [H-04](../../../Assets/NTSD/Docs/android-mobile-readiness/h-04-il2cpp-arm64.md) | 构建准备可独立；运行验收需要H-01内容和H-02可安装包 |
| [H-05](../../../Assets/NTSD/Docs/android-mobile-readiness/h-05-device-gpu-certification.md) | H-02/H-04可运行包、H-01内容、M-07初始API矩阵；最终汇总H-07/M-08 |
| [H-06](../../../Assets/NTSD/Docs/android-mobile-readiness/h-06-collision-broadphase-1000.md) | H-09受影响正确性、H-11适用容量、M-09/M-15基线；不依赖Mono整体 |
| [H-07](../../../Assets/NTSD/Docs/android-mobile-readiness/h-07-fresh-1000-ai-certificate.md) | 所选优化的实际成果；M-09/M-15指纹与H-09正确性，Android阶段另需H-02/H-04 |
| [H-08](../../../Assets/NTSD/Docs/android-mobile-readiness/h-08-prebaked-visual-content-memory.md) | 与H-01先冻结共同Manifest/key/Lease接口；ATLAS预算和资源格式待决 |
| [H-09](../../../Assets/NTSD/Docs/android-mobile-readiness/h-09-ntsd28-correctness-alignment.md) | 当前正式336B44、D023及用户例外；M-09/M-15身份 |
| [H-10](../../../Assets/NTSD/Docs/android-mobile-readiness/h-10-battle-audio-pcm-memory.md) | 共享H-01/H-08保守闭包接口，不依赖纹理烘焙全部完成 |
| [H-11](../../../Assets/NTSD/Docs/android-mobile-readiness/h-11-presentation-capacity-zero-gc.md) | 具体变更的容量单位、consumer生命周期；设备部分依赖H-02/H-05初始矩阵 |
| [M-01](../../../Assets/NTSD/Docs/android-mobile-readiness/m-01-dedicated-worker-throughput.md) | 生产资格深化依赖M-11 B0-B6/L1与PERF门；局部只读归因不依赖其全完成 |
| [M-02](../../../Assets/NTSD/Docs/android-mobile-readiness/m-02-central-render-draw-segmentation.md) | H-08 bank改变时需绑定合同；设备证据需H-02和M-07矩阵 |
| [M-03](../../../Assets/NTSD/Docs/android-mobile-readiness/m-03-dynamic-mesh-upload.md) | H-11容量与publication/alpha合同；EXT-1仍未批准 |
| [M-04](../../../Assets/NTSD/Docs/android-mobile-readiness/m-04-centralonly-fail-closed-diagnostics.md) | H-02可运行包、M-07初始API矩阵；不依赖H-05最终证书 |
| [M-05](../../../Assets/NTSD/Docs/android-mobile-readiness/m-05-gameobject-shell-cost.md) | 数量归因可独立；移除Core反向引用依赖M-11明确阶段 |
| [M-06](../../../Assets/NTSD/Docs/android-mobile-readiness/m-06-pool-capacity-prewarm.md) | 复用H-11 seal；按局资源准备依赖H-08/H-01接口 |
| [M-07](../../../Assets/NTSD/Docs/android-mobile-readiness/m-07-android-graphics-api-policy.md) | H-02包可生成初始API矩阵；最终策略消费H-05功能数据 |
| [M-08](../../../Assets/NTSD/Docs/android-mobile-readiness/m-08-sustained-thermal-performance.md) | H-02/H-04包、H-05基础功能、批准设备环境；不等待H-05最终证书 |
| [M-09](../../../Assets/NTSD/Docs/android-mobile-readiness/m-09-performance-evidence-fingerprint.md) | Client dirty/内容/Kernel实际身份；无需等待Android包才记录Windows证据 |
| [M-10](../../../Assets/NTSD/Docs/android-mobile-readiness/m-10-mobile-viewport-safe-area.md) | L-04方向决策；H-03使用布局接口；设备需H-02包 |
| [M-11](../../../Assets/NTSD/Docs/android-mobile-readiness/m-11-mono-core-presentation-layering.md) | 获明确批准才从B0开始；不能因M-01需要而自动解冻 |
| [M-12](../../../Assets/NTSD/Docs/android-mobile-readiness/m-12-decode-upload-byte-backpressure.md) | H-01/H-08所有权接口与容量；不要求视觉整个父项先通过 |
| [M-13](../../../Assets/NTSD/Docs/android-mobile-readiness/m-13-sound-event-aggregation.md) | 仅真实cue热点新证据才重开；若实施沿用H-10容量合同 |
| [M-14](../../../Assets/NTSD/Docs/android-mobile-readiness/m-14-content-verification-io.md) | H-01不可变manifest/read lease，H-08输入身份与M-09失效协议 |
| [M-15](../../../Assets/NTSD/Docs/android-mobile-readiness/m-15-kernel-build-reproducibility.md) | 源身份采集可独立；APK关联仅需H-02/H-04对应产物 |
| [L-01](../../../Assets/NTSD/Docs/android-mobile-readiness/l-01-application-identifier.md) | 用户正式包名决定；内部调试包不必等待Release命名 |
| [L-02](../../../Assets/NTSD/Docs/android-mobile-readiness/l-02-signing-keystore.md) | 正式密钥管理及L-01包名；不阻塞Debug包 |
| [L-03](../../../Assets/NTSD/Docs/android-mobile-readiness/l-03-target-api.md) | 发布商店和时点、H-02构建环境 |
| [L-04](../../../Assets/NTSD/Docs/android-mobile-readiness/l-04-orientation-policy.md) | 无优化硬前置；影响M-10/H-03布局 |
| [L-05](../../../Assets/NTSD/Docs/android-mobile-readiness/l-05-graphics-jobs-frame-timing.md) | H-02包、M-07候选矩阵；相关配置写入另批 |
| [L-06](../../../Assets/NTSD/Docs/android-mobile-readiness/l-06-urp-hdr.md) | H-02包、M-07矩阵、视觉验收 |
| [L-07](../../../Assets/NTSD/Docs/android-mobile-readiness/l-07-ai-profile-description-drift.md) | 核resolver实际空值语义；不依赖H07性能通过 |
| [L-08](../../../Assets/NTSD/Docs/android-mobile-readiness/l-08-release-packaging.md) | H-02/H-04产物，L-01/L-02/L-03产品发布配置 |


## 写冲突与证据失效

BruteForceSceneQuery由Role准入/空间裁剪共享，不能并行改；AI/World/publisher由INPUT/GC95/AI-MODE可能共享，先准确划写域再并行；CharacterAnimtorManager由H08/M12/M02共享；App/生命周期由资源和运行入口共享。
只读分析可并行，代码任务独立且写域不重叠才委派；架构/合同决定与最终集成留主代理。
字段、候选顺序、发布时点或Kernel变化可能使相关正确性证据过期；资源身份/模式变更可能使性能基线不再可比；不触及相应依赖的修改不要求全量重跑。取哈希不等于已核行为，复用理由要写入具名Task。
