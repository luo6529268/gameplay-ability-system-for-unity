# H07-INPUT：CharacterInput等价优化子方案

父项：H-07；与H-06、M-01共享成本背景，但不是新增第35父项。状态：ANALYSIS_REQUIRED / EXECUTION_STOPPED。建议高优先级，Role准入之后或其授权等待期间可独立只读分析。

## 事实与代码

91实际窗口CharacterInput约12.1—12.6ms；NativeSampledState八子阶段已测，Builtins约1.0—1.1ms不是全部输入成本。95 isolated输入12.5261→12.1755ms，仅约0.35ms局部信号，四块有一块回退。
已定位：Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs::ProcessNativeSampledState；Simulation/Ai/Runtime/SimulationAiDecisionModule.cs的RefreshCapture与AiDecisionKernel.TryEvaluate调用；Simulation/Ecs/Core/BattleAiUnifiedRowPublisher.cs；InputState.SyncFromRuntime消费口。现有SoA/DataOriented路径已存在，不能重建一套。
这些是候选调查范围，不是允许任意修改整个World/AI模块。

## 方法与有限出口

1. 先读91完整结果，画CharacterInput父子相位包含关系；同窗独占耗时才能合计。只补尚未覆盖的行捕获/感知/同步调用链，不能再重复八子阶段归因。
2. 逐字段列同tick读写时点、producer freeze、双state交换、State/HitStop等失效条件；查整块重复capture/同步。不能用每实体12微秒“感觉慢”代替成本证据。
3. 若同步/行发布主导，评估在原边界消除重复采样或更新，保留原fallback；若查询主导，再评估等价剪枝/数据访问布局。最近目标O(n²)尚未证实，tie-break/顺序/RNG保持。
4. 没有发现可解释且足够有价值的成本块则交付有据“不实施”结论，不开无证微缓存。95不自动成为必须续完的候选。

## 依赖、验证与结束

依赖M-09/M-15具名基线、H-09比较范围和现有数据发布合同；不依赖M-11全关闭、Role默认推广或95 GC修复全部完成。若所选fixture受95问题影响，则先修该fixture/生产调用点，不忽略0GC。
最小验证：受影响字段及边界focused/逐tick完整输入、state、数组索引、双RNG；有效完整Driver0GC校准；同指纹双布局必要A/B。复用原37/正式root及91归因中未受影响结果。必要时检验差异不是顺序环境噪声。
方案阶段完成：明确一个具名成本块、准确改法/写域/失效表/验证，或证据证明不值得继续。
实施阶段完成：候选实际减少声明成本，整tick不出现未解释回退，正确性/0GC/容量/关闭门通过。不承诺input<=6ms；H07<33ms最终门独立保留。
风险：按脏更新漏状态、旧source/physical坐标混读、published/scratch身份判断错误、RNG顺序变化。代码前新Task/Change及dirty备份，原canonical路径保留；回滚仅具名增量并遵守操作授权。
