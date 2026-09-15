<!-- CHANGE-RECORD
id: NTSD28-Q06-CANONICAL-CHARACTER-PHYSICS-TAIL-001
status: VERIFIED
change-kind: CANONICAL_CHARACTER_PHYSICS_TAIL_INTEGRATION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterFrameAdvancePass.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CanonicalCharacterPhysicsTailEditorTests.cs
authority: Formal Logan playable SimulationTickDriver28::step -> BattleWorld28::step_physics -> physics_integrator; environment damage precedes state12/18 ground contact and selected-action/counter updates. Build closure manifest 07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F.
evidence: Cost witness151 bounded stage800/-10000/10000 source double-run SHA3278f3bec1314c42ae5f5eb70dc4f7ca866ccda4ffb0f768a294eef63c875056. Unity job7f39dba873144d8f9526a3df3340107b: two Legacy profiles before0/diff0; two DataOriented before0/diff326. Independent read-only reviewer confirmed omitted existing native tail helpers.
-->

# Canonical character physics tail

IN_PROGRESS / TEST_FIRST_RED_CONFIRMED. Parent Q06 native-reader/cost fulltick acceptance dependency; does not close the complete domain.

Original production: exact-character fast pass executes mechanics -> legacy landing resolver only on crossed-floor Landed -> integer position, followed by obsolete state12/state18 airborne promotions. Ordinary LF2Character.ApplyDynamics already has current-authority helpers.

Declared implementation: single production file TryExecute/ExecuteCharacterDynamics, pass tickIndex; after BoundaryWriter consumed flags call existing environment damage, contact action, conditional ordinary landing (legacy handler fallback only if unhandled), current airborne action, then integer sync. Read current Frame.D at each corresponding ordinary-path point. Remove only the two obsolete airborne call sites, retain method definitions. Preserve delay/link/cpoint kind2 gates, exact-type ownership, native lifecycle guards, runtime slot iteration, resource normalization order and weapon-count tail. No source rule, global helper, schema or framework changes.

New focused fixture owns actual world DataOriented/Legacy pass comparisons and explicit representative contact/ordinary/airborne assertions, including nonzero floor and resource phase. Existing source151 fulltick remains under its cost Task owner and is reused as authority trace evidence.

Validation: bounded source151 four groups zero diffs; explicit actual-pass tests; relevant existing B4 contact/environment/ordinary/airborne regressions; compile CS0, full SelfCheck, required real Play/replay/shutdown and independent review. Pure-helper or Legacy-only PASS cannot substitute default fast-path acceptance.

Side effects: intended action/counter/velocity/environment HP-credit and airborne phase now consumed in native order. No new persistent fields, queue, manager or allocation; existing eleven-stage shutdown unchanged. Risk: broader previously bypassed helpers may reveal other mismatches; preserve failures and track independent causes. Rollback only this reviewed diff with required authorization, never revert user work. No Scene/Prefab/resource/InputAction/GAS/nonbattle/Server changes, computer-use, commit/push/delete.

生产已写：仅声明pass的TryExecute/ExecuteCharacterDynamics，按现有普通路径接四helper并移除两旧promotion调用；等待新测试文件完成后统一刷新，尚未宣称编译或运行通过。

新增测试由独立worker完成，root已读：两profile各14case×两mode，共56实际world physics调用；明确exact dispatch计数和native结果，包含非零floor、soft/hard/pending、ordinary/action212/state100、world phase与环境credit/lethal normalize。旧typed夹具为已有B4 source合同回归，不冒充正式330内容；当前正在统一编译，未宣称通过。

联合joba456cae2273e42cbb592ebf3828d2c96实际49项47PASS/2新fixture FAIL；费用151端点+四fulltick0差异，151×两profile replay通过。新physics在lethal断言期待HP0但实际-5，原失败已归费用following-after-physics-fix。核对source normalize_native_dead_character_resources1978及Unity同方法：不归零current HP，只清effectiveMaxHP与PP。修订新fixture两断言为HP保留-5、lethal HPBound0，生产保持；这是本任务新增oracle纠正而非改变已有B4权威。

修正新fixture后jobaa2e0629548f4776bfe6cdef1b9ae8f5 2/2 PASS，56实际physics pass全部完成。CS0；源151完整tick四组before0/diff0及302 replay/604重放tick通过。最终独立review生产diff未发现确定顺序/回写错误。完整SelfCheck已请求，Play/关闭仍待；不标VERIFIED。

新增同测试文件真实Play请求入口，运行已通过的14×2profile×2path共56次实际physics，稳定暂停Scene，检查checksum与borrower保持并单独输出；不重复1208费用矩阵，不宣称真实按键。19:06:33Z关闭已PASS且Editor idle。


最终 VERIFIED / DECLARED_CANONICAL_PHYSICS_TAIL。此前IN_PROGRESS与失败记录保留历史。Unity最终刷新CS0；完整SelfCheck19:09:57Z PASS。费用原151两profile端点+四完整tick零差异，302同World replay/604tick；新physics56pass通过；旧B4四类39PASS。真实Scene1208 source/factory/mode对照PASS、独立56physics Play PASS，两次borrowers2→2/checksum保持；最终19:11:02Z关闭恢复4→4、World/slots/logic/render0、两帧Stopped、Scene dirtyfalse/root14/hash BCD1047B…0E9FB6保持。生产fast diff独立review无确定问题；oracle仅修新合同下无效目标。实际范围/原失败/合成内容边界见artifact与父报告；未完成整个Q06/Q07正式迁移/全部按键或视听。
