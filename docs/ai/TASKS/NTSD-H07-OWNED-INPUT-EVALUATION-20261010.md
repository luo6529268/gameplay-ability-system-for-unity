> **2026-10-10 H07 owned-input有限评估完成：COMPLETED_NO_DISCERNIBLE_GAIN / 本版不采用。** NTSD-H07-OWNED-INPUT-EVALUATION-001 VERIFIED仅Editor诊断：最新3/3 focused、唯一千人合成native输入两布局8窗完成，256稳态pass0B、逐tick完整checksum/双RNG/41字段一致；两个布局均无可辨收益，不跑后继实景或推广，生产SnapshotCopy不变。不是GC门阻止测量，也不是已解决旧Sakura约12ms。主H07仍OPEN/未形成新的生产实施方案；未派发、监测暂停。报告artifacts/diagnostics/NTSD-H07-AI-DECISION-WORK-REVIEW-20261010/owned-input-evaluation/REPORT.md。以下保留此前时点。

# NTSD-H07-OWNED-INPUT-EVALUATION-20261010
Status: IN_PROGRESS
Parent: NTSD-H07-AI-DECISION-WORK-REVIEW-20261010
Change: NTSD-H07-OWNED-INPUT-EVALUATION-001
本次主Task子阶段：只评估已存在SnapshotCopy/CanonicalStoreDirect输入读取模式。用户要求继续检验直到完整方案；不改生产默认/Kernel/Store/Writer，不切编译模式。
写域仅新增Assets/NTSD/Scripts/Test/Editor/H07OwnedInputEvaluationEditorTests.cs及.meta；必要真实fixture接线须在实施前具名补充，不预授权任意文件。
先完整41输入字段、native/legacy RNG、完整RuntimeChecksum、实际profile/mode、注册/释放/跨tick等价focused；再一次合成1000角色完整CharacterInputAll对照，两布局spacing120/12，各OFF->ON->ON->OFF，每独立同seed初态8warm+32sample，计时/GC仅整个pass，校验日志在外。硬正确性/容量/异常失败即停止；GC如非零如实记录，不能称0GC资格。收益预定两个ON mean均低于两个OFF mean且ON最大P95/P99不高于OFF最大值；否则本候选不采用，不重复找PASS。局部通过才补真实fixture资格与成本；不外推12ms。World.Clear须finally关闭并确认objects/slots0，不改Scene/设置。
预期副作用仅Editor构造测试World/运行既有路径及新文件导入；生产路径原样。复用已有3测试证据，但不可冒称全覆盖。
回滚：仅本次新增文件可具名撤回需保留证据，旧文件逐份真实dirty备份，不reset/clean。无新生产handoff直到证据齐全。

## 测前入口精确修订
实际NTSDBattleTickSystem.CharacterInput阶段调用NativeProducerSampleAndInputRouteAll(tick)，includeNonCharacterHitFa=true/applyFrameMotionTail=false。有限成本组改用该原入口，纯1000角色合成fixture；不是Driver实景，也不拿CharacterInputAll的额外tail混入目标成本。focused仍可覆盖两公共入口。尚未运行任何成本组，非失败后换口径重测。关闭使用实际World已有API，最终核objects/slots0，不假定存在World.Clear。
