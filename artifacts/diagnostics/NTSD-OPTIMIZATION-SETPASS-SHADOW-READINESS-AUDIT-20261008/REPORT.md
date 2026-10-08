# SetPass 优先修复：阴影共用绑定前置核对
状态：STATIC_PRECONDITION_CONFIRMED / IMPLEMENTATION_NOT_AUTHORIZED / PERFORMANCE_UNCHANGED。
本轮为78同一 SetPass 修复方向的必要补证，不新增优化实施子批；57已执行、六项阶段4/6、H07/H11 OPEN、父项关闭0不变。没有 Unity 控制、测试、Profiler、GPU capture、M0、生产脚本或资产修改。

## 1. 本轮新闭合的静态条件
- Assets/NTSD/Config/GameConfig/GameConfig.asset:38 的 ShadowPrefab GUID 为 a74e0aa2ef455834393473f33ee52faf，与 Assets/NTSD/Prefabs/Common/Shadow.prefab.meta:2 一致。
- Shadow.prefab:63 指向 Sprite GUID 04bffa57fa05fe44e8b259e1ba07141a；Assets/NTSD/Sprite/XueYuan/s.png.meta:2 身份一致、:24 isReadable=1。该链与75 readback 的 s 源资产路径相符。不是另一个同名阴影，也不需要因猜测不可读而修改 importer。
- CharacterAnimtorManager.cs:1619–1620 从该 ShadowPrefab 建 common catalog；:1642–1648 调 TryAppendCommonAtlasSources，:1654–1665 将角色 stagedCatalog 和 commonCatalog 交给同一次 unified publication。
- 同文件:2123–2142 从 common shadow Texture2D.GetPixels32 取整纹理像素并加入共同 sources；:2144–2156 失败时才有显式 SourceTexture2D 安全例外。isReadable=1 只证明当前序列化配置，不冒充本轮真实读取成功。
- Assets/NTSD/Scripts/Animation/Runtime/BattleAtlasResources.cs:321–360 已有 TryBindCommonCatalog，并包含 Shadow；:443–514 同一 resources 按 placement 产生绑定，array identity 由 resources.ResolveTexture 确定，slice 独立为 placement.PageIndex；只有明确 excluded source 才在:461–468保留源绑定，其他缺 placement fail-closed。
- Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteCatalog.cs:1116–1157 的 WithCentralBindings 已重绑 Shadow/Spark/Words/COM；:342–358 保持源 Sprite/Texture、尺寸、pivot 和 RenderState，只替换 CentralBinding。因此后继必须比较实际中央绑定身份，不能把保留的源 descriptor.Texture 当作 atlas 绘制身份。

结论：现有代码已经具备阴影进入共同 atlas 输入及中央重绑的冷准备能力；没有静态证据要求另建阴影 GPU 读回系统、改 PNG/meta 或删除阴影。被批准的候选应优先复用此能力，让高频身体/阴影真正取得兼容的实际绑定，而不是只给身体换图集。

## 2. 对下一实施动作的约束
- 共同 atlas 输入不等于最终共享绑定已成功：75/77生产窗因全集预算超限仍走 SourceTexture2D，不能宣称本轮修复或下降。
- 不保证身体/阴影必在同一 Texture2D page；OrderedPages 若落在不同页，原透明交替仍会分段。array 在当前设备能力和既有预算允许时可跨 slice 保持相同实际 texture identity；不得强制 array 或改变 planner/segment/bank 合同。
- BattleAtlasResources.cs:70–90 已检查 array 支持、2048尺寸、slice 数、采样格式和预算；CopyTexture仅决定潜在上传优化，CPU assembly 是既有正式 first path。静态可用不是实际设备验证。
- 完整本局视觉依赖仍须生产语义捕获，上一包 raw oid13/22摘录不能直接拿来过滤逻辑/视觉全集。逻辑配置与菜单数据完整保留；超大/不可读安全例外不删除、预算不抬高。
- 最小后继仍是待人类确认的“本局完整视觉依赖＋预算内共享绑定”脚本准备/取得/容量/生命周期接线；不修改原 PNG、Scene、Prefab、importer、ProjectSettings、33ms/3ms/max2、publication、透明顺序或 Q06 方法体。只读补证不是实施授权。

## 3. 验证层级与复用
已有 BattleCommonAtlasBindingEditorTests.cs:147 ArrayPublication_BindsShadowSparkAndWordsWithoutChangingDescriptorIdentity、:346 oversized-shadow fallback、:393 over-budget source retention 的断言可作为后继受影响测试候选；本轮仅读测试源码，不执行、不报告 PASS。
SetPass 仍引用77 Combat平均1995，真实 GPU batch 组成仍 UNKNOWN；CPU segment/DrawMesh 和全帧 SetPass 不互等。本轮没有新帧率、收益、完整0GC或 Android 证书。
EXT1仍 PROPOSED / MODIFY_REQUIRED；PERF/ATLAS正文及bank/预算/纹理格式不改，Mono USER_HOLD、当前权威和11阶段关闭保持。

## 4. 进度与操作留痕
[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-SETPASS-SHADOW-READINESS-AUDIT-20261008/RECORD.md)记录六个现有 dirty 文档准确 before-01 副本、16新鲜输入和77既有递归 guard；只有文档追加与本报告创建。
79 / NTSD-OPT-H11-LATE-CAMERA-POSITIVE-CONTROL-079 已有准备 Record；本轮仅补登 Ledger/STATE/handoff 为 PLANNED / NOT_STARTED / DEFERRED_SETPASS_PRIORITY，三个 Editor C# 原SHA保持，不开启新 GC 诊断或测试，不计已执行批次。
最终原字节保留、保护、HEAD/staged、diff和validator结果见 final-audit-01.json 与 Operation 终态。阶段/Goal未完成，不调用 Goal complete、paused 或 blocked。

