<!-- CHANGE-RECORD
id: NTSD-M03-UV-DIMENSION-REVIEW-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/M03MeshUvDimensionReuseDiagnosticsEditorTests.cs
authority: User requests M03 verification until a bounded repair handoff is ready
evidence: artifacts/diagnostics/NTSD-M03-MATERIALIZATION-REUSE-REVIEW-20261010/REPORT.md
-->
# M03 UV尺寸重复读取局部验证

原状：BattleMeshChunk.WriteQuad每个有效quad两次Texture.width/height getter，半像素边界计算不依赖运动位置。
诊断：仅一个新Explicit Editor测试文件；原尺寸读取与局部两槽尺寸复用对照，未改生产backend。每轮复用状态清空，不跨Build/帧缓存。实际生产候选仅sealed BattleCatalogCentralResourceResolver路径考虑，任意custom resolver保持原样。
有限验证：精确UV Vector4值与Texture2D/Texture2DArray/边界/销毁/两轮间resize；正分配校准前后有效；3布局各OFF/ON/ON/OFF，每窗8warm+32sample，每轮4000quad，计时外校验与日志、无内部计时器/计数，完整测量轮线程allocated0。shared与two-texture各两个ON均值小于两个OFF，尾部不恶化；thrash如明显退化则必须报告，不能无条件准入。局部时间不外推全部WriteQuad或整个帧。
失败停止该组，不刷PASS；只修工具错误，原失败保留。创建纹理仅诊断且finally销毁，不创建Scene物体、不改资源或设置。执行后检Scene/源码指纹及validator/diff。
回滚：仅新工具可在用户批准删除时删除并保留审计；生产无需回滚，原文只前置变更。

## 最终证据与范围更正
原两槽计划为V1历史；V1三纹理退化，不推广。V2锁首array不通用，V3已切为局部segment状态。三版各一次组，384+512+512可靠GC scopes通过、所有41语义比较通过，纯数学cost的整体门V2/V3均未通过。VERIFIED仅工具正确执行和诚实保留原件，不是候选收益/生产验收。
首次新文件未导入job total0保留；线程bytes正校准失败发生在0成本窗，已改用未修改的BattleScopedGcAllocationRecorder；脚本最新job ae369f1ad0364fbca7ebe775537a1e06实际1/1。完整命令/输出/边界见REPORT与analysis-summary，生产未改。
新的有界生产接法不在本Change实现，另Task/Change按execution-handoff声明三域，默认OFF并先真实Backend验证。
