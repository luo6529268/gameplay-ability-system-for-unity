# 任务：选择下一批等价性能优化

项目：Unity 2022.3 NTSD 战斗 runtime。

唯一战斗逻辑权威：
`J:\QQFile\NTSD2.4\ntsd_release_C#`

当前 Unity 1000 全 AI 最新详细报告：
`Temp/NTSD_ProductionEntityStress.dispersed-full-ai-occupancy-epoch-detail-20260726.json`

已知热点（详细诊断开启）：

- `CharacterInput = 17.919 ms/tick`
- `RemainingAiDecision = 10.086 ms/tick`
- `LateEntityUpdate = 10.130 ms/tick`
- Late `FrameTick = 4.126 ms/tick`
- Late `OpointProcess = 2.655 ms/tick`
- `CandidateCollect = 4.680 ms/tick`
- `RenderDispatch = 4.251 ms/tick`

现在另有一次 `enableDetailPhaseTiming=false` 的生产基线正在运行。

请只读分析，不要修改任何文件。重点回答：

1. 在 `SimulationWorld.AiInput.partial.cs::PrepareAiInputBasic` 中，将
   `RemainingAiDecision` 继续拆成哪些粗粒度阶段，才能低扰动定位热点？
2. 找出 1～3 个可证明等价、预计收益最大的第一批实现候选。必须保持：
   - 权威 C# 的 runtime slot 升序；
   - RNG 调用次数与调用顺序；
   - 同 tick 可观察 mutation；
   - 早退顺序；
   - 输入边沿与 `ApplyInputEdges` 时机。
3. 评估按 OID 分发角色专属决策、缓存重复 runtime 字段、维护紧凑活动 slot 表的语义风险。
4. 分析 Late `FrameTick` 和 `OpointProcess`。本次报告 `observedOpointCreates=0`，
   但 `OpointProcess` 仍为 2.655 ms。判断这是诊断成本、无效检查还是正式逻辑成本，
   并给出安全优化边界。
5. 给出聚焦测试矩阵和 old/new A/B oracle。不要建议跳 tick、降低 AI 数量或改变玩法。

输出必须给出精确文件、方法和建议实现顺序，并明确哪些方案暂时不要做。
