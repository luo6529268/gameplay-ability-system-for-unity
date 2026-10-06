<!-- CHANGE-RECORD
id: NTSD28-336B44-BATTLE-AUDIO-EVENT-RULES-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleAudioAlignmentEditorTests.cs
code-path: Tools/NTSD28Q10Diagnostics/battle_audio_20261006_probe.cpp
code-path: Tools/NTSD28Q10Diagnostics/battle_audio_formal_kind9.py
authority: user 2026-10-06 battle audio request; formal DAT/sound assets; current Core comparison limited; formal event correspondence pending
evidence: docs/ai/TASKS/NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006.md
-->

# NTSD28-336B44-BATTLE-AUDIO-EVENT-RULES-001
修改前建立。状态 PLANNED。
需求及权威：用户要求先修复角色与战斗全部音效，不改DAT。正式根336B44、runtime sound.dat/角色DAT及当前稳定音频live分支；源树候选非战斗开发不能自动晋升，正式EXE逐事件音频导出限制如实记录。
Unity 原状与预期：按已确认普通命中/防御/护甲/特攻命中与非角色反弹落地公共分支修声音选择、事件顺序和 source X；同步 ECS shadow projection。动作、伤害、运动、RNG、碰撞结果均不改；kind9/dash候选只在闭合来源证明后处理，不凭猜测改。
受影响路径：metadata 精确清单；新增测试仅验证共享规则/资源差异，不建立全角色操作矩阵。
不变项：原框架、非战斗场景、33ms/3ms、固定相机/统一比例、原资源 GUID、DAT字节、项目模式Asset、Scene/Input、旧目标关闭状态。
验证：先聚焦首差；原Editor自动Refresh/编译；具名共享分支测试、实际音频加载/voice见证、必要代表Play；文件SHA/ChangeLedger/diff审查。
风险：帧多声列表预热后的加载时长/内存；旧测试可能固定错误音效；双路径声音预测必须一致。主代理负责集成，禁止改变伤害/物理/状态逻辑。
回滚：PREPARE操作before实际原字节或对应hunk；任何回滚必须保护并发改动并另记操作，不恢复整仓库HEAD。
不可回退边界：不得回退用户/其他任务修改、DAT及Scene；无外部发布/删除。

2026-10-06 CODE_WRITTEN：实际脚本已按声明路径编辑；资源966新增WAV+966meta+53foldermeta已逐SHA验证，原12保持。cue新两项RED均预期首差；原Editor Refresh已提交，正在导入资源，初次请求30秒超时不等于失败/编译验收。事件规则worker三个指定文件已静态检查，ECS投影和运行仍待主代理集成。无DAT/Scene/非战斗修改。

诊断实施前追加准确路径 Tools/NTSD28Q10Diagnostics/battle_audio_20261006_probe.cpp：仅当前Core受控音频trace，复用28Core编译参数但使用新输出，绝不覆盖旧证据/正式EXE。测试用字符串DAT只在内存，禁止写生产DAT。此证据不能独立宣称逐字节对应336B44；静态精确hunk跨两个早期快照连续性仅补来源，不恢复旧物理门。

补充实施前首差定位：C++ ordinary type0 final audio块7199-7207 在状态后置与OID100 channel13之后发base/effect1，再 append_native_kind0_post_audio(effect3/30 post_action200 -> channel14；effect2/20/21/22 post_action203 ->16；effect23->16)。Unity普通角色writer缺后效音，且base在OID100前排队。仅既有声明三个脚本/测试范围补公共prefix/base/post helper，将角色base移至普通transaction声音尾部、保留type3前缀和OID100顺序，不改变动作决定/数值/随机。ECS相同移动/共享post选音；不修改Reduced尾部。以新post分支实际writer测试/native小矩阵验证，kind9上游转换/其他逻辑不扩。

2026-10-06 FOCUSED_TEST_PASS：原Editor实际compile0 error；audio-focused-green-final-result 25/25（含978原WAV UnityWebRequest samples/channels/frequency逐个一致，代表8声音voice，128稳定播放GC0）；post-audio-focused-final-result16/16；post-audio-ecs-final-result6/6。去重为39具名case，不称47个不同case。native当前Core22/22应用成功/0anomalies，普通effect0/1/type3/prefix，effect2/3/23后效，reduced0/7/70/75 DAT回退/type3目标X，type2反弹。正式EXE音频事件未捕获，当前source身份与正式根精确对应仍受历史来源限制；当前Core trace不晋升formal EXE。681保护哈希、978三端WAV一致、原12 WAV/meta24项与开始Git基线完全同。
原Scene run01只在正式内容预热startup180秒超时，没有战斗tick/声音测量，报告FAIL；正常有序关闭/退出后voice0/hashclean保留，不包装PASS。run02用既有078十分钟启动先例、12分钟总限，30秒CreateNew状态快照，正在运行；collision门已要求同tick实际assigned+playing+playdelta，有序关闭失败停止不卸载。无DAT、Scene/Input或非战斗改动。

冲刺声音修正事前登记：native run-dash-attempt-05 完整Driver7tick，实际running与crouch跳跃分支仅声明帧a7/012，无channel7；输入源码音频hunk两早期快照一致，正式Naruto213/215帧引用核实。移除LF2Entity三个输入分支额外SFX_017，保留DAT FrameSounds及一切动作/运动写入。现有声明测试补running、crouch左右三项，先RED再GREEN；不得由本例删DAT frame210/其他原有017。

2026-10-06 最新更正：Scene02提前退出由用户明确确认是他或其他任务的操作；尚未进入声音预热/战斗tick，所以不是运行音频失败也不是PASS。Scene03在已保存Battle/Editor idle前置后启动，输出新03保留01/02。Dash三RED额外1个cue，删三行后同三项GREEN，动作/速度/DAT帧a7断言通过；累积42去重case，原39和978加载未重复。独立只读复审无新增阻断；原Scene声音/普通SelfCheck整体未知仍明确未晋升。

2026-10-06 最终运行验收：Scene03 PASS/DONE，原Scene/Driver tick5→69，global39自然技能data078、global69实际命中HP500→480/SFX001；两受控p3/a7帧均实际voice。所有64样本五失败/拒绝/溢出计数0；World/slot/borrower清零并正常退出，退出后voice0/hashclean。资源978实际解码、42去重case、681保护/原12WAV与meta稳定；详本Task REPORT.md/scene03/final-content-guard。
状态边界：cue/resource VERIFIED；Event RUNTIME_PENDING仅正式根逐事件对应不足，Unity运行已通过，不表示还未运行Play。当前Core25受控case非formal根见证，kind9候选来源不足未实施。旧记录中的两个早期音频hunk表述不作为kind9正式源身份证明，不扩大旧目标或自动重跑全角色。整体BattleRuntimeSelfCheck及设备听感未运行。

2026-10-06 Before implementation: user asks to confirm remaining kind9. Add only the declared Python diagnostic: launch the unchanged formal EXE with a new LFR fixture under GDB, inspect returned tick audio vectors without process state writes, use formal frame-sound positive controls and exact root trace relation results. Header-only offsetof helper output is a diagnostic layout hypothesis, validated with actual positive event bytes. No production/DAT changes until actual formal evidence. Old 42 tests and 978 decode checks are reused. Operation NTSD28-336B44-BATTLE-AUDIO-KIND9-FORMAL-CONFIRM-20261006.

2026-10-06 formal kind9 evidence closed before production change: attempt05 exact formal336B44 returned tick audio vectors; four actual DAT scenarios x4ticks, all root-report passed, actual relation kinds9/type1 rejection/type3 state3000 transfer30/type3 state3005 action40 plus far negative control. Audio only frame source1 data005, plus actual spawned OID211 frame data020 in transfer case; no builtin or weapon_broken source. Offset1816/event48/string16 current-header hypothesis validated by actual positive sounds/source/path/worldX, no process game-state edits. Only remove raw-kind9 effect/broken enqueue from writer, LF2Weapon fallback and matching ECS projections, and remove their sound-capacity prerequisites. Preserve converted-kind0 audio, frame DAT audio, state/HP/motion/RNG/relationship rules. Add LF2Weapon exact path before edit. Four production/test byte backups declared in this Operation. RED uses the three existing raw-kind9 cases plus one legacy fallback case, then GREEN plus two converted controls; no old suite.

2026-10-05T20:33:47.285545+00:00 CODE_WRITTEN kind9: actual three production paths and one test path match production-before-manifest. Removed raw-kind9 effect/broken calls only; projection sound-capacity requests now0, no state-field diff. RED original Editor four cases failed solely queued sound0 vs1/2, saved kind9-red-result01. Original Editor Refresh sent; GREEN includes these four plus converted two controls. Formal-kind9-attempt05 4 root passes /16ticks retained. Full old suites not run.

GREEN01 raw4 PASS; converted broad2 fail at existing HitStateCount expected45 got0 before sounds. Added planned narrow two converted sound controls in same declared test path, no changes to old counter assertion or unrelated production. This does not certify broad state-test behavior.

2026-10-06 scoped kind9 follow-through complete: FORMAL_EVENT_CONFIRMED / FOCUSED_FIX_PASS. Formal attempt05 four reports passed/completed4,16 audio tick observations, positive frame005/020 source1, builtin and definition broken0, formal exit0/SHA336B44 stable. Three production diffs remain raw-kind9 sound-only; four raw checks pass in green01 (six completed, failures only old broad converted2). Final OrdinaryKind0PreservesAttackerDatSound succeeded2/2 with actual LF2SpecialAttack and declared broken cue; direct ordinary writer evidence, no converter/Shadow/full-Driver promotion. Earlier new control attempts had incorrect expectations or no supported writer observation; all original failures retained. Original broad converted HitStateCount45/0 remains a validation limitation; no pre-change execution proof, no confirmed baseline-failure claim. Original counter/SFX assertions and ordinary production rules untouched.
Original Editor Refresh/compile completed; error-CS0. 681 protected SHA0diff, formal EXE stable, old42/978 reused. Independent read-only reviewer confirmed three production diffs and final direct fixture, no new blocker. No fresh Play/device audio. Parent remains RUNTIME_PENDING for overall event-correspondence/runtime boundaries; raw kind9 no longer UNKNOWN. Exact Operation NTSD28-336B44-BATTLE-AUDIO-KIND9-FORMAL-CONFIRM-20261006, Report and KIND9-FORMAL-CONFIRMATION.
