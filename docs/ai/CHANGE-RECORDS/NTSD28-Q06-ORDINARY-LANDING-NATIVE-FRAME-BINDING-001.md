<!-- CHANGE-RECORD
id: NTSD28-Q06-ORDINARY-LANDING-NATIVE-FRAME-BINDING-001
status: VERIFIED
change-kind: ORDINARY_LANDING_NATIVE_FRAME_SOURCE_WITNESS_FIRST
code-path: Tools/NTSD28AuthorityTrace/ordinary_landing_native_frame_witness.cpp
code-path: Tools/NTSD28AuthorityTrace/validate_ordinary_landing_native_frame_witness.py
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06OrdinaryLandingNativeFrameEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
authority: Current formal playable PhysicsIntegrator28 ordinary type0 landing, BattleWorld28::step_physics and SimulationTickDriver28.
evidence: Read-only live raw caller matrix and formal330 static7 implicit hit_g references; no runtime first-difference claim yet.
-->

# NTSD28-Q06-ORDINARY-LANDING-NATIVE-FRAME-BINDING-001

PLANNED / SOURCE_WITNESS_FIRST。当前source SimulationTickDriver28→BattleWorld28::step_physics(1798)→PhysicsIntegrator28普通type0落地：strict crossing先clamp floor/Vy0/post-friction Vx除3，state100优先94，action212或state6优先215，否则非0hit_g或219；动作写入后counter0。Unity已闭ApplyCurrentDatType0OrdinaryLanding行为仍调用旧raw binder。只补native descriptor/cache读帧，不重做B4 ordinary landing或Q06 canonical physics-tail接线。

formal330 type0静态hit_g候选（排除12/18/100/6和212）2343显式/7隐式，例OID870/802/721/508/88/89 frame701→1、OID413 frame420→422。统计不证明实际落地可达；原草稿误排state5已纠正且保留，计数恰好不变。

第一写域仅新Tools/NTSD28AuthorityTrace/ordinary_landing_native_frame_witness.cpp。实际world.step_physics或完整driver源捕获，覆盖上述1/422、隐式0..998/高位/无效、94/215/219优先级及声明/隐式、接触与非接触控制；完整raw/descriptor/counter/latch/Prev/snapshot/速度/RNG/后继tick。先source双跑与独立模型后新增准确Unityfixture和单生产符号清单，当前不改production。保护已闭state12/18、非角色物理、边界、音效与schema。

验收compile/focused/SelfCheck、sameWorldreplay、真实Play两factory/关闭及authority全调用链。不得为通过测试改变source、物理规则或既有例外；不改Scene/resources/非战斗/GAS/Server。回滚仅同ID精确diff并保留失败及其它工作。Q06仍active，Q07不得提前部署。

源186 build/双跑exit0，SHAed75a284cc6a23423d4aee09241bf6bdafa8e516ca732fbab81131a85a773f2e。独立模型准确新增路径validate_ordinary_landing_native_frame_witness.py：从params/固定初值核算即时物理与binding（X/Z积分后摩擦、floor跨线、gravity/94/215/219/hit_g优先级、counter/descriptor/历史坐标/RNG及stepflags）。following仅源捕获不独立模拟。保留首次失败证据；source previousXYZ额外字段尚无Unity直接carrier，不能伪造Unity值或因本任务扩kernel，映射边界须在后继fixture明确。

独立模型186/97845检查0失败。准确Unityfixture预声明：仅新NTSD28Q06OrdinaryLandingNativeFrameEditorTests.cs，使用真实World.NativePhysicsAndDeadCharacterResourceNormalizeAll(0)及其Legacy/DataOriented两路径×两profile，之后完整RunReleaseTick1；normal factory slot0，恢复source before，分别报告before/即时/following。raw沿用既有47绑定/3缺口；previousXYZ是source-only额外字段，保留原capture但比较明确移除，不伪造Unitycarrier，回链平台字段后继，不能据本包称平台对齐。即时检查模式diags及Clock不推进；生产未改，先跑RED。

2026-09-21 RED job b89e3f3a6a194dcaa527b23ba9c86904 terminal FAILED 4/4：两profile×Legacy/DataOriented各186，before0/immediate162/following132，XML及四JSON存production-red。CS error查询0。首次即时wait=1 expected0来自旧raw descriptor；following包含collisionYReference=-10 expected0，尚未裁决，不隐去或改期望。

生产修改前扩充准确范围：Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs，仅ApplyCurrentDatType0OrdinaryLanding内单次landingAction绑定调用改为既有DirectWriteNativeRawFramePreserveWaitCounter。保留分支、位置/速度/counter和state12/18处理；使用既有native 0..999 descriptor合同，不改共享binder，不改kernel/schema。可能改变隐式/高位帧的wait/next与后继动作，这是源证据要求；后继完整tick其余差异须独立追踪，不能以即时通过关闭。回滚仅此单caller差异且保留其他工作。待编译/四组重测/回放/SelfCheck/Play与独立审阅。

普通落地单caller native绑定已写。job01658e8a1a8241caab89c3c3fb25234e终态FAILED4/4，四组各186 before0/immediate0/following78，仅combat.collisionYReference=-10 expected0；证据after-binding-fix。源battle_world.cpp4026在pair pass前清零collision_y_reference/platform_source_slot_f4/render_shadow_offset_10c；Unity仅carrier reset已有，需审完整生产pass及平台依赖，不能在fixture清零掩盖。下一先只读确认并独立Task/Change声明必要生产修复，再完整回归/回放/SelfCheck/Play。CS查询0、Ledger596/9PASS、diffcheck无错误、Scene SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。落地Task仍IN_PROGRESS，Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use与非战斗/Scene/资源修改。

独立只读cpoint_acceptance复核：单caller修复正确；剩余reference差异属于后继pair-pass生产/重置职责。报告PAIR-PASS-REFERENCE-FIRST-DIFFERENCE.md；下一独立任务，禁止在fixture清0或修改expected掩盖。

Q06候选高度参考reset独立包已生产写入；联合job0bbead38ad244ed1adc8ac109ebdabd3 SUCCEEDED15/15（新5/载体6/源矩阵4），普通落地186×两profile×两路径before/即时/following全部0。实际未运行旧B4 fixture（初始filter漏Editor namespace），后继须NTSD.Test.Editor.NTSD28B4Type0OrdinaryLandingEditorTests补跑。完整SelfCheck请求已消费，现有Editor PID62860执行中，实际日志Logs/kind8-real-play-editor.log已有RunAllChecksStatic/CheckActivatedRuntimeProfileContracts调用；结果待新Temp/NTSD_BattleRuntimeSelfCheck.result，禁止重复启动或测试并行。之后同World回放/真实Play及关闭仍待。平台op30/previousXYZ/其他两个reference字段仍未闭合，不称整域完成。Q06 active/Q07未迁移/禁computer-use/非战斗/Scene/资源修改。

最新验收：candidate reference reset与普通落地native binder联合15/15PASS，source186×四组before/即时/following均0；旧B4普通落地已以正确Editor namespace补跑6/6PASS（job4191c162de2543eb83166574b9983c84）。完整SelfCheck 2026-09-21T00:11:47.663892Z PASS文件与时间证据已存candidate包。CS0、Ledger597/11PASS、diffcheck无错误，Scene hashBCD1047B…0E9FB6保持。下一唯一工作：同一普通落地186夹具补同World回放，再真实Play两profile/两路径/两factory及Q05关闭重入；脚本扩展前登记Record准确符号。两个包仍IN_PROGRESS（尚未完成replay/Play），不是平台op30域对齐。当前无运行test/SelfCheck/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，Q06未完/Q07未迁移/总目标ACTIVE。

验收扩展前登记：仅现有NTSD28Q06OrdinaryLandingNativeFrameEditorTests.cs新增OrdinaryLandingSurvivesLocalSnapshotReplay/ExecuteLandingAndTwoTicks与Editor-only NTSD28Q06OrdinaryLandingPlayProbe。复用既有snapshot/cursor epoch/replay/Play保护/请求文件模式，186×两profile×两路径回放；Play再乘两factory为1488场景。现有Scene保持checksum与borrowers，结束调用既有Q05关闭重入probe；不写Scene、生产或资源。源矩阵及即时/后继断言保持，不将replay自洽当authority证据。

新增验收编译CS0；job72b8838670da441bab85d7ed0e4a609b SUCCEEDED8/8，四源码矩阵及四sameWorld replay，744场景1488重放tick，证据replay-pass。独立只读复核确认before物理快照、cursor失效、逐阶段签名与Play1488计数/cleanup正确；未覆盖previousXYZ/跨World/图像及物理键。真实Scene dirtyfalse/root14，现进入Play，request为Temp/NTSD28_Q06_OrdinaryLandingPlay.request；等1488结果与独立Q05关闭结果，不可仅按请求存在声称通过。

最终限定VERIFIED：详见artifacts/diagnostics/NTSD28-Q06-ORDINARY-LANDING-NATIVE-FRAME-BINDING-001/ACCEPTANCE.md。源186矩阵0差异/联合15+旧6/回放744场景1488tick/完整SelfCheck/Play1488/真实退出重进两次关闭PASS，Scene保持。平台op30、raw3、previousXYZ及Q06后继明确保留。
