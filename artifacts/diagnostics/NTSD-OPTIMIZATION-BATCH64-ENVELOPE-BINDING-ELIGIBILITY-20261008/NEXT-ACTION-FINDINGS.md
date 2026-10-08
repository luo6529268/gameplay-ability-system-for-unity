# 第64批收口后的下一必要动作：保持当前包络路径的分支证据

状态：CURRENT_SOURCE_GAP_IDENTIFIED / NO_NEW_CODE_OR_MEASUREMENT。本次只读重新扫描；本结论不是新优化收益，也不增加已执行批次数。原第45批诊断出口不重开。

## 新信息与场内证据

- 当前四窗原件及组内比较：PairExactLoop平均OFF/ON分别39.314933/37.519451ms、43.465384/38.887202ms；CandidateCollect平均39.887177/38.213626ms、44.096203/39.561456ms。嵌套PairExactLoop包含cache build和整个遍历，不能全部归为某个pair判定；占collector主要部分且候选ON自身仍超过33ms。CharacterInput/EntityInputPass约10.975/12.561/11.561/12.086ms不是零，单独修collector也不能凭本表保证整tick达标。
- BruteForceSceneQuery.cs:2554—2556，TryBuildBruteExactCache仅在!EnableBruteBranchTimingForDiagnostics时构建包络；:2594—2596，CollectCandidatesForBruteExactDirection仅在相同条件下进入包络提前拒绝。两处都会让旧分支计时模式离开当前包络路径。
- 同文件:5958—5979，CollectCandidatesForPairCached只在geometryFirst＋branch timing＋有效recorder时记录eligible/timed及四细分。当前64分支开关OFF，因此BruteCoarse/RejectedBinding/PairAllowed/ExactWork零值是未采集，不是零成本。
- RoleAwareCollisionShadowSelfCheckTests.cs:595—610，BruteCoarseEnvelope_InactiveGateKeepsOriginalPath的timing case明确要求包络direction计数0。这是已存在的旧诊断兼容合同，不能直接删!timing条件并修改旧断言来刷绿。
- BattleOptimizationWindowsAiSuiteEditor.cs:1044—1065，既有44/45 owner只保存配置branch bool/stride；旧45菜单:310—314不配置包络＋绑定复用。因此直接再跑旧45不能回答64残余热点，也不能将其旧分支访问比例外推成本。
- BruteForceSceneQuery.cs:2616—2620及:6339—6356，拒绝仍PreserveBruteRejectedBinding，包括ItrRest.IsBound可能清过期绑定的副作用；无证跳过拒绝/提早绑定/改变第一次访问时点不可接受。
- 旧45 REPORT区分coverage含warm+sample、steady phase180和顺序计时干扰，不乘64外推纯成本；其结论与原件完整保留。本次不是重测该旧路径。
- 上述路径均非Q06方法体；Q06仅随116guard核SHA。初次猜测的BattleBruteBranchTimingEditorTests.cs不存在，未读取到方法、未写入；随后真实定位为RoleAwareCollisionShadowSelfCheckTests与已声明Suite，不能以猜测路径作依据。

## 下一最小动作

[第65批Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH65-ENVELOPE-PATH-BRANCH-TIMING-20261008.md)仅READY，先保持旧诊断默认/旧timing gate，新增明确opt-in去测当前包络＋绑定路径；共享既有stride64/coverage/phase，无新collector/空间索引/规则或缓存算法。首先聚焦证明采样开关不改变candidate顺序、RNG、first binding、包络真实应用，旧58 timing case仍PASS；接线、冷owner/所有退出恢复和完整logic GC scope随后验证。

通过必要资格后才一次固定四120+180，保持包络＋绑定ON、eligibility/kind5/dispatchOFF，仅新诊断OFF/stride64ON变化。记录instrument tax及真实coverage/分支，不使用新读数当FPS收益或乘64冒充纯成本；若观察证据无效，按具体缺口修工具，不盲堆微优化或重复本64找PASS。

这是一项新路径证据缺口，不是同一旧45计时器再细化：58新增包络入口导致旧开关不再代表当前路径。原63/64窗口、71case、61无效byte量、H11旧FAIL不重复；下一未实施、不计第44已执行。H07/H11 OPEN、Goal active、范围/完成门不变。
