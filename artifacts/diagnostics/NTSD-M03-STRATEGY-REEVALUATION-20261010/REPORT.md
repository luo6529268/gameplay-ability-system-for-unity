# M03 策略调整与重新评估

Task：NTSD-M03-STRATEGY-REEVALUATION-20261010。

结论：SCOPED_REASSESSMENT_COMPLETE / DIAGNOSTIC_DESIGN_READY / PRODUCTION_REPAIR_NOT_READY。

本轮已完成源码与既有原始数据的重新评估。停止以纹理尺寸 getter 为主线；改为按显示样本变化评估整个“命令物化—插值—Mesh 构建—上传”链。优先验证同一 publication 仅 alpha 变化时，是否值得复用未插值的命令准备结果；新 publication 必须独立评估，不能继承该 skip。此为有代码和历史频率支撑的诊断方向，不是收益已证明或生产修复可派发。

## 一、纠正上一策略依据

1. 旧千人报告的 4.1559677778 ms 对应 `Materialize/BuildCommands`。`BattleCentralRenderSystem.PrepareFrameForMaterialization` 中该包围范围包含 `MaterializeCommands`、容量检查、`DisplayMotion.Prepare` 和 `ApplyToCapturedCommands`；它不是整个 materialization 或完整 Mesh Build。之前聊天及旧报告的“Materialize 4.156 ms”简称容易造成误读，以此明确更正，旧数据原件不改。
2. 同一报告 `Mesh/ResolveAndWriteCommands` 为 2.1159566667 ms，内部 `ResolveCommands` 0.754210、`WriteQuads` 1.080572；`UploadChunks` 0.413000。`WriteQuads` 包含几何、UV、颜色、vertex 写入及 bounds，不能当宽高 getter 成本。父子指标不可相加。`BuildCommands/Core` 为 3.023381，deferred sprite resolve 为 0.340934；与 4.156 的剩余差额也不能直接称独占插值成本。
3. 上述旧窗 `detailPhaseTimingEnabled=false`，但 `presentationTimingEnabled=true`。源码表明非空 presentation diagnostics 仍使 BuildCommands 每实体/类别、Backend 每 command resolve/write 执行 timestamp。关闭 input/detail 并不等于整个表现链没有细分观测。现有报告提供热点线索，不能充当无扰动基线；本轮没有测出这些计时的实际开销，不将其宣布为根因。
4. 最新候选唯一成本组是完整 Backend Build、4000 个已准备的合成 Entity 命令、32×32 测试纹理/array、同一固定 frame，每布局 8 warm + 32 sample、OFF/ON/ON/OFF。真实 prepared resolver 和原生 Mesh upload 均执行，但不含 publication 捕获、BuildCommands、DisplayMotion、Configure；不能代表完整 camera 物化或千人战斗收益。
5. 新计算：连续 Source2D 的两 OFF 均值 4.9635265625 ms，两 ON 4.7867296875，表观下降 3.5619%；不走候选的 Strict 对照从 6.051221875 到 5.84671875，表观也下降 3.3795%。不同布局不能互减形成“净收益”，但负控的变化使因果归因更弱。保留旧 Source2D 事前局部门 PASS，不将它升格为已证明 3.56% 生产收益。

## 二、实际路径核对

- 不能用 Batch20 的 SourceTexture2D 历史替代当前生产路径。Batch80 的 Sakura1000 实际取证明确 Texture2DArray/AtlasTextureArray，源 fla.png 保留 Source2D 回退；Batch86 两次直接 Battle 的 1800 camera 窗均实际出现 array segment 1—2、Source 最多 1、page 0。当前 GameConfig 的 atlas policy 为空（Auto），它不是本机当前绑定分布证明，但与源码冷准备路线一致。
- 新旧成果不重做：相同 Unity frame / 相同 publication+alpha 已在 `MaterializeLatestPublishedFrame` 提前返回；resolver 的 trusted identity、per-type hot resource、Configure no-op 已存在；metadata 批量上传、索引模板和容量预备也已存在。不能把这些重新列成新优化。
- 当前每次实际 materialization 会把 publication 复制到 staging submission，在捕获副本上物化命令，再施加显示插值，然后解析资源、写顶点及上传。`BattlePresentationFrame.CopyFrom` 复制 source 的 CommandsMaterialized；仅在当前 frame 已物化时 `MaterializeCommands` 返回。已施加 alpha 的 staging 命令不是可直接重复使用的未插值模板。
- `BuildCommands` 读取 viewport/camera、catalog、实体显示快照、overlay 和公共资源；同 publication 不自动意味着所有输出依赖不变。`DisplayMotion.ApplyToCapturedCommands` 会替换命令坐标/锚点，累积应用或反向减去旧 float offset 均不能作为等价方案。

## 三、新的工作量证据与其限制

从 Batch86 两个原始 materialization-window 文件的 **counts 差分范围**重新提取：

| 旧实际场景窗口 | 新 publication Build | 仅 alpha 变化 Build | 完全重复 Build |
|---|---:|---:|---:|
| camera-01 | 1024 | 776 / 1800（43.1111%） | 0 |
| camera-02 | 1025 | 775 / 1800（43.0556%） | 0 |

这证明原正常小规模 Battle 确实存在同一 publication 的重复物化工作，且已有完全重复样本消重生效。它不证明 43% 工作都可以消除：插值、几何和必要上传仍须执行；也不能把小规模频率乘千人旧 4.156 ms，或用于声称千人收益。两窗口只约 4—5 个平均 quad，与千人约 1992 条命令不同。当前千人报告没有同口径 alpha 分类成本及 actual binding 分布，不能由 segment=1 猜测。

## 四、调整后的策略与优先顺序

**第一优先：按新 publication / alpha-only / 完全重复三个队列类别核对成本和工作量。** 复用现有 `BattleCentralDisplaySampleKind` 与计数，不再发明分类器。完全重复路径已消重，只作回归负控。先获取准确现实权重再选择改法。

**首个待检验生产方向：在同 publication 且所有命令生成依赖不变时，复用未插值的命令准备结果。** 消除目标是重复的 catalog/显示资格/overlay/基础 command 准备，而不是两个 getter；插值仍按原 alpha 从固定基准计算，publication 仍只读，最终命令顺序、Mesh、透明像素、aux anchor 和 fail-closed 必须相同。

该方向须先补的合同：

- owner 应沿现有 central submission/presentation 生命周期；不得新增 manager 或跨 World 静态缓存。容量必须冷预备/封口，stage5 清理，不能覆盖有读 lease 的两槽，也不能为了复用占住全部槽。
- key 至少包括 World/publication 身份、catalog/common binding identity、viewport/camera/投影及 command-generation 实际依赖。具体字段须沿调用链闭合，不能仅用 tick 或 source 引用。
- 换 publication、World、catalog、视口/相机依赖、mode、资源失效、异常/容量拒绝及 Clear/Dispose 时，必须原路重建/安全失效。所有角色显示、shadow、血条/脚标锚点、overlay/hit record、生命周期和 slot generation 边界保持。
- 不省略真实移动显示、不改 alpha、不减少显示刷新、不改变资源格式和 atlas 策略。模板内存/复制成本必须计入收益；不能从旧4.156 ms直接承诺可得时间。

**第二优先：新 publication 的命令准备与插值数据流。** 如果当前高负载几乎每个 camera 都是新 publication，那么 alpha-only 的权重可能太小；应优先核对实际 command 生成与插值的重复遍历/大结构写入。只有定位具体可消除工作后才能提出合并或模板方案，不以“3 ms core”代替语义和频率证据。

**暂不采用：** 旧尺寸方案 source-only 拆分、继续调整 cache 命中算法、第四次孤立 UV 微测、未经取证的 dirty-chunk/部分上传、改 shader/vertex stride、Burst/Jobs/GPU instancing/atlas 重建。这些不因本次重评自动获得实施许可。

## 五、下一轮最小诊断设计和有限出口

本节为诊断设计，不是已运行测试，也不是直接生产修复指令。

1. 使用当前既有 CentralOnly/Role/DataOrientedCanonical/正常共享绑定的代表性 Battle；既有小规模窗口直接复用，补缺只针对当前千人工作负载。保留 actual command/category/binding、publication/alpha 类别、容量/GC/关闭与源码指纹。输入和随机数不得为提高命中率而改动。
2. 将 **外层阶段计时** 与 **每实体/每 command 细分计时**明确区分。基准中后者必须完全关闭，包括 presentation recorder 隐含开启的 section timestamps；完整外层计时保留。不得通过仅令 Enabled=false 假设消除外层已执行的 GetTimestamp。需要改诊断能力时准确声明路径与符号，另建 Change，保持正常生产行为。
3. 在一个预先固定的有限采样组中观察新 publication 与 alpha-only 的实际频率及完整物化成本；低频类别不足就如实报告不足，不调整运行直到凑够理想结果。冻结采样数量、cold/warm/steady、owner和保护范围后才启动，不重复旧UV组。取证输出/校验/序列化在计时及GC外，可靠 recorder 正/空前后校准。不得把完整 Driver 的旧失败抹除，也不把无关 Driver 门阻断本M03独立scope。
4. 如确有足量 alpha-only，先在捕获 publication/catalog 的受控回放中验证完整 **基础命令模板→原 alpha→完整 Mesh** 与原路逐位/像素一致，且声明全部失效键。回放不改 live World，不产生另一套玩法。没有足量样本或合同不闭合，则不实现模板。
5. 只有观测权重、准确可消除工作、复制/内存代价、owner/失效/关闭、同 scope 成本对照同时成立，才形成生产修复 handoff。生产准入仍看完整链均值/尾部、可靠0GC、容量及所有正确性门；可按实际主路径/合法fallback分别定门，但必须事前确定，不能再从失败组事后挑通过子集。

本轮评价结果：旧尺寸方案不继续；alpha-only 命令准备复用有源码和历史发生频率线索，值得一次有界诊断，尚未达到可直接修复的成熟度。若新工作负载不支持该方向，具名结束该方向，再评估本M03的新 publication成本，不无限补窗或恢复其它优化点。

## 六、证据与执行事实

- 本轮实际工作：读取当前源码、原报告与JSON；独立重算16窗汇总/32原始ticks统计、512GCscope状态、历史两个1800camera的分类；保存机器摘要和源码/文档SHA。
- 没有新增Unity/Editor测试、Play、Profiler、性能测量、生产修改、资源/Scene/Settings修改、Git操作或向执行会话发送消息。自动监测保持暂停。
- 源码引用：BattleCentralRenderSystem.cs:607/790/873；BattlePresentationShadowBuild.cs:1132/2647/2897/3485；BattlePresentationDisplayMotion.cs:72/162；BattleCentralRenderTypes.cs:382/440；BattleDynamicMeshBackend.cs:171/694/761。行号随后续改动需重核。
- 原件：NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-20261010/windows/00-combat1000-off/report.json；NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010/full-build-cost.json；NTSD-OPTIMIZATION-BATCH80-SETPASS-MATCH-BINDINGS-20261008/REPORT.md；NTSD-OPTIMIZATION-BATCH86-DIRECT-BATTLE-SHARED-BINDINGS-20261009/camera-01及camera-02/materialization-window-01.json。
- 本次只追加文档，Operation保存六文档真实dirty备份；不改变旧ABANDONED事实。父M03、H07精确33ms/drop/backlog/Android仍开放。
