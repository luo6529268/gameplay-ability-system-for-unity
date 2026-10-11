# NTSD-H07-OWNED-INPUT-EVALUATION-20261010
Status: RUNNING
本次主Task子阶段：只评估已存在SnapshotCopy/CanonicalStoreDirect输入读取模式。用户要求继续检验直到完整方案；不改生产默认/Kernel/Store/Writer，不切编译模式。
写域仅新增Assets/NTSD/Scripts/Test/Editor/H07OwnedInputEvaluationEditorTests.cs及.meta；必要真实fixture接线须在实施前具名补充，不预授权任意文件。
先完整41输入字段、native/legacy RNG、完整RuntimeChecksum、实际profile/mode、注册/释放/跨tick等价focused；再一次合成1000角色完整CharacterInputAll对照，两布局spacing120/12，各OFF->ON->ON->OFF，每独立同seed初态8warm+32sample，计时/GC仅整个pass，校验日志在外。硬正确性/容量/异常失败即停止；GC如非零如实记录，不能称0GC资格。收益预定两个ON mean均低于两个OFF mean且ON最大P95/P99不高于OFF最大值；否则本候选不采用，不重复找PASS。局部通过才补真实fixture资格与成本；不外推12ms。World.Clear须finally关闭并确认objects/slots0，不改Scene/设置。
预期副作用仅Editor构造测试World/运行既有路径及新文件导入；生产路径原样。复用已有3测试证据，但不可冒称全覆盖。
回滚：仅本次新增文件可具名撤回需保留证据，旧文件逐份真实dirty备份，不reset/clean。无新生产handoff直到证据齐全。
开始UTC 2026-10-10T08:03:17.792630+00:00
执行: 主讨论会话和具名luna_worker；先manifest和备份，后apply_patch新增Editor测试，原单Editor编译/focused/有限成本；不删除。

## 测前入口精确修订
实际NTSDBattleTickSystem.CharacterInput阶段调用NativeProducerSampleAndInputRouteAll(tick)，includeNonCharacterHitFa=true/applyFrameMotionTail=false。有限成本组改用该原入口，纯1000角色合成fixture；不是Driver实景，也不拿CharacterInputAll的额外tail混入目标成本。focused仍可覆盖两公共入口。尚未运行任何成本组，非失败后换口径重测。关闭使用实际World已有API，最终核objects/slots0，不假定存在World.Clear。

Subagent wrote only new Editor file; main takes ownership. Pre-review SHA f258be2f22b5548c5a0ffb756b5d8c02b6a221d529c4f805a9f770d77f5cadeb. Review fixes before compilation/measurement: absent property refs, invalid StableId-generation comparison, explicit opt-in cost, fixed seed/phase, cold preallocated vectors/typed capture (avoid boxing observer), actual gain criterion and capacity/GC metadata. No cost ran.
`nFocused job fa5639253f7f437787dcc1fe05ef1b41: 3 executed, one fixture assertion failure (input pass RunCount is per-character, observed4 expected1). Corrected assertion to expectedCharacterCount after reading Execute body; production unchanged; no cost group ran. Original receipt retained.

Final finite execution: reviewed/repaired test-only code, latest focused3/3; cost8windows done no gain, no production handoff. Evidence and exact APIs in REPORT.md/receipts. No deletion; original files backup protected. Source audits passed; validator pending.

Final documentation scope amendment before writes: exact three paths in manifest-final-doc-amendment.json, 3 additional dirty backups verified. Required parent report/unique tracker/H07 state synchronization, no scripts/resource writes.

Status: VERIFIED (finite diagnostic and audit only). Last source/assembly checks stable, 2659 protected files unchanged, 9 dirty document backups; main review fixed test assertions before finite group. Validator PASSED exit0; original failures kept. No production handoff or automation update.
