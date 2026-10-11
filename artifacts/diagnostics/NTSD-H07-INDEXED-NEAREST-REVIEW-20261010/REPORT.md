# H07 输入阶段 Indexed 最近目标查询评审

结论：**SCOPED_REVIEW_COMPLETE / BOUNDED_CANDIDATE_READY**。找到实际生效查询中的重复距离计算，形成仅两个脚本的候选实施与局部成本验证范围。没有实际 C# 候选、Unity 验证或 CPU 收益结果；整个输入约 12ms、H07 和 Android 均未完成。

Task：NTSD-H07-INDEXED-NEAREST-REVIEW-20261010。用户直接要求本讨论会话先处理评审，再交既有“处理优化方案”会话实施。

## 1. 依据与实际路径

复用 NTSD-H07-INPUT-SPIKE-CPU-DIAG-20261010/cpu-analysis.json 的同一 CPU provider OFF 窗口：CharacterInput 11.996463ms，RemainingAiDecision 4.961281ms，内层 IndexedCanonicalKernel 3.115163ms，再内层 NearestSearch 1.540116ms。后者为本次选定成本块，不能相加或当作全部可消除收益。这些数含原 elapsed 细分计时，不是无计时生产成本。

实际路径：SimulationAiDecisionModule.TryPrepareIndexedCanonical（3139—3148 的 DataOrientedCanonical + CanonicalStoreDirect 分支）→ BattleCharacterInputWriter.TryEvaluateCanonicalDecision → BattleCharacterInputStore.TryEvaluateCanonicalDecision → AiDecisionKernel.TryEvaluateCanonicalInput/TryEvaluateCore → AiSensingKernel.TryFindNearest(policy=Indexed)。不是 Legacy FindNearestGround 旁路。

现有 Ground/Air 索引在 SimulationAiSensingModule.BuildRoleIndexes（1474）按 team、X、slot 构建。FindIndexedGround（376）/FindIndexedAir（446）在各 team span 通过 LowerBound 找两侧候选，按 X 距离推进与剪枝。查询已有索引；本提案不添加新索引、改变索引构建/更新或跳过 eligibility。

## 2. 已确认的重复工作

两段 while 每次都重新执行左右两侧的 `Abs(rows.X[indexSlot] - selfX)`。一次只移动一侧 cursor，另一侧 cursor 与坐标未变，下一次仍重算该侧。选定 slot 后 Ground 又在 Distance 中重算其 X 差，Air 又在 481 行重算 X 差。

候选只在当前函数调用、当前 team span 的栈局部变量中保存 left/right 的 slot、dx 及 cursor 有效性；cursor 改变才重算该侧，并将选定侧 dx 用于原距离表达式。函数返回即失效，新调用重新读取快照。没有跨角色/跨 tick 缓存、额外数组、manager、World 状态或新的关闭资源。

不得移除 IsGroundTarget/IsAirTarget、TeamAllowed、included/hp/state/hitstop/弹道方向检查。不得因 cached target 常保留而跳过 nearest：后续还消费 ground bestDist、sameZLane、initial selection 等事实，不能只保持最终 target id。

## 3. 等价要求

- span 顺序、LowerBound 和 `leftDx <= rightDx` 选择左侧的顺序保持。
- Ground 剪枝仍为两侧 dx 都严格大于 bestDist；初始 bestDist=10000；等距时仍选择较小 slot，不能用 >= 改动边界。
- Air 最大相关 X 差仍为 min(bestDist,249)，X<250/Z<40；state9 不进行 Air 查询。
- Air 可覆盖 SelectedSlot，但 BestDist/SameZLane 仍是 Ground 阶段事实；epoch/generation/identity 捕获保持。
- 原候选 visits、拒绝与 no-target 行为保持。只减少相同的算术/数组读取，不减少查询、访问候选或随机调用。
- 现有 Abs 是手写 int 绝对值；不能改为 Math.Abs、long、float 或改变 unchecked 溢出语义。
- 查询依赖既有同步只读快照合同。不能把此局部复用扩大为跨查询缓存；两次查询之间位置/状态/index 变化必须重新计算。

## 4. 本会话实际检验

只读源码与已有证据；执行一个离线 Python 控制流模型，seed20261010，1,400 组夹具：

- 原 Indexed 与候选完整结果、Ground/Air visits 及访问序列：23,870 次对照，0 差异。
- 正常非溢出坐标与线性参考 payload：19,125 次对照，0 差异。
- 覆盖多 team/phase、空索引、单边耗尽、相等 X、相等距离、10000/250/40 边界、state9、Hp/HitStop/非角色3000与速度含NaN、int极值，以及调用间坐标变更。
- 模型中的边界和选中候选 X 距离求值：221,199→150,936，减少70,263次。**这是生成夹具的算术求值计数，不是实景次数、CPU收益比例或节省毫秒数。**

原件：offline-model.txt、offline-equivalence.json；七个已读 Unity 源码 SHA 在 review-source-manifest.json。模型是手工转写，仍需独立实际 C# 对照；不替代编译、Unity/Player/设备或完整输入/RNG/checksum 验收。

## 5. 准确后继与有限出口

准确指令：execution-handoff.txt。后继只允许 AiSensingKernel.cs 加入默认不接生产的 internal 候选入口/私有两个循环，以及 AiSensingKernelEditorTests.cs 的语义、调用间变更、warmed0B和一次局部 ABBA 成本验证。现有公共入口、原 helper 与所有生产调用点保持原样，不新增全局开关或为了小候选串改 World/Writer/Store/AI/测试 Suite。

先判断实际 C# 候选是否正确且实测减少局部查询耗时，随后交回本讨论会话。尚无收益时不实施生产接线。此阶段不跑完整 Driver，不因旧基线2event无限重做相同失败窗；它不是忽略0GC的正式证书。真正生产接线仍需另行明确最小范围、逐tick完整输入/双RNG/checksum、完整Driver GC与成本及关闭证据。没有可辨收益即结束此候选，不扩散到其它点。

## 6. 权威、历史和范围

正式规则 authority 336B44 保持。只读交叉定位 current native_ai.cpp 的 select_local_character_target 692—737，其最近距离、较小slot首匹配和 cached RNG 顺序仅作线索；README_SOURCE.md 380—381明确当前源码持续开发、不与根发行EXE逐字节对应，因此不拿当前这段 supported_subset 裁决完整规则或覆盖旧证据。本次要求逐字段保持既有 Unity 查询行为，不引入规则变化。

旧17字段兼容候选仍 OFF，旧19/19及旧完整Driver GC2event事实保留；producer USER_DEFERRED/OFF、95 OFF、H06不续办。没有重跑91或旧83、没有读取活跃Q06方法体，没有改生产/测试脚本、资源/Scene/Settings，未恢复旧Goal或34项队列。

Operation 与真实文档备份：docs/ai/FILE-OPERATIONS/NTSD-H07-INDEXED-NEAREST-REVIEW-20261010/RECORD.md。派发回执与最终文档/源码保护审计将在同目录另存，不回填为本报告的运行时成功证据。

## 7. 派发与本会话收尾

2026-10-10T14:10:07.198155+08:00：精确指令已发送；工具回执成功，新turn已active/inProgress/error null/cursor76。原heartbeat已恢复ACTIVE，实际回执另存 dispatch-receipt.json、monitor-baseline.json。未把启动证据当作实现或测试通过。

派发前8份真实dirty文档备份验证一致；2642份tracked及25份untracked C#对本评审开始指纹0变化，见pre-dispatch-protection.json。该保护时点明确在执行会话开工前，不用后继允许的两脚本改动倒推为本会话改代码。定向8文档git diff --check exit0，仅既有LF/CRLF提示；本会话未运行Unity、C#测试、Profiler或性能测量。
