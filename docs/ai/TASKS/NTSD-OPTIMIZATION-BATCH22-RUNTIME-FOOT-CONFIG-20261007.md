# 第22批：生产 Foot 配置入口（有限首阶段 1/8）

状态：SCOPED_RUNTIME_FOOT_CONFIG_PASS。本批固定矩阵已通过（32/32、严格1800camera及三档catalog重放、关闭/Scene/Menu保护），父项仍OPEN。用户“开始执行吧”启动六项有限首阶段；本批只实施M03/H11的Foot前置，不重开第21批诊断。结果：artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH22-RUNTIME-FOOT-CONFIG-20261007/REPORT.md。

## 改动合同

需求：无显式 Preview authoring 时，中央 runtime 从已绑定 GameConfig 读取 FootMarkerSprite、六帧数组、帧间隔；显式 authoring（含禁用）保持优先。无 feature 或无有效 Sprite 仍禁用。仅复用既有 ResolveReferenceSprite/ResolveSprite、默认 style、unscaled 表现时间、Self 人类过滤、地面锚点和 batch backend。不复制配置数组，不创建生产 Sprite/Texture，不增加 per-frame 资源查询。

准确脚本路径/符号：
- Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs：RefreshRuntimeFootMarkerAuthoringSettings 的无 authoring else；保持零参数签名与已有 ResolveRuntimeFootMarkerTexture。
- Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralRuntimeFootMarkerEditorTests.cs：新增 RuntimeConfig 聚焦测试与隔离 fixture，只恢复测试借用的 global GameConfig/registration。
- Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs：新增独立 Batch22 菜单入口及只读 pipeline 标签，复用严格1800相机窗、catalog replay、关闭流程；旧入口/证据不改。

原状：GameConfig 已在 App/测试 bootstrap 绑定，但中央 fallback 直接 disabled；第21批已确认 loadedPreview0、六帧有效、Self2/Health2/Foot0。第20批严格 FAIL 原件保持。本批不读取活跃 Q06 方法体，不改模拟、publication、插值、segment、材质、像素采样或 ATLAS/EXT-1 合同。

## 冻结验收

测试请求（原 Editor EditMode）：BattleCentralRuntimeFootMarkerEditorTests 整类，新增 RuntimeConfig_UsesGameConfigWhenNoAuthoring、RuntimeConfig_FramesOnlyResolvesReference、RuntimeConfig_MissingConfigDisables、RuntimeConfig_EmptyConfigDisables、RuntimeConfig_InvalidDurationUsesDefault、RuntimeConfig_ExplicitDisabledAuthoringWins、RuntimeConfig_ExplicitAuthoringWins、RuntimeConfig_TextureSamplingAllocatesZeroAfterWarmup。测试首先在原生产分支运行 RED；随后最小修复并运行本类及既有 BattleFootCoverageDiagnosticEditorTests、BattleProductionCatalogReplayPolicyEditorTests，实际所选数与每项结果留存。不运行全 discovered suite。

生产请求：原 saved Assets/NTSD/Scene/NTSD_Battle.unity，保留其现有 roster/input/AI/renderer/config，不写 Scene。Batch22 Production Foot Config 1800 Cameras，1次功能窗口，既有 frame-by-frame cameraExecutedDraws=recordedDraws、Foot>0、Health>0、catalog bound、lease0 条件全部保留，随后原 production catalog replay 100/500/1000 受控命令（不是1000 AI）。camera envelope/observer 必须局部0B，明确不能代替完整热路径0GC门。已有动态/容量/溢出/slot-reuse证据只按其依赖域复用，本批不制造新的全面矩阵。
退出：复用十一阶段 owner shutdown，objects/slots/borrowers0，Scene clean及字节SHA不变，原 Menu恢复；不得以 CPU lease 0 证明 GPU consumer 已结束。
若新增失败只在本候选内修复，父条目累计最多3轮；不得放宽 Foot 门/追加同构诊断。本次功能验收不是 H07/Android/120fps证书。

## 生命周期/风险/回滚

不新增 manager/queue/buffer/pool 或 shutdown stage。GameConfig 资源所有权不变；中央只借用已有数组/Sprite，feature移除后按既有 registration refresh 关闭。Startup authoring 搜索已有分配不纳入热路径0GC；新增热路径仍仅现有 texture selection。
风险：显式禁用优先、frames-only、空配置、duration fallback、global测试恢复、feature lifecycle和 stale DLL。运行前后验证当前 Editor 编译/重载，保存精确测试结果与程序集身份。
操作记录：NTSD-OPTIMIZATION-BATCH22-RUNTIME-FOOT-CONFIG-20261007，11文件当前字节备份+before.json。仅准确声明脚本与治理文档增量；不覆盖并发改动、不删除/move、不Git弃改/commit/push。回滚必须先获用户准确恢复操作批准，只用本批 before 字节与最小反向补丁，不恢复HEAD。
本批限定通过不关闭34项的任何父项；其余5项按总 Goal 合同和独立既有方案继续，准确 request 在各批实施前冻结，最多8新子批。

