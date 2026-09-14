> 当前VERIFIED，准确六脚本，最终限定出口见末节；下文五脚本为扩展SelfCheck前的历史计划。

<!-- CHANGE-RECORD
id: NTSD28-Q06-KIND3-CATCH-NATIVE-FRAME-LOOKUP-001
status: VERIFIED
change-kind: KIND3_NATIVE_FRAME_ADMISSION_AND_PAIRED_BINDING
code-path: Tools/NTSD28AuthorityTrace/kind3_frame_lookup_witness.cpp
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06Kind3FrameLookupEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchRelationExactFieldsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Current BattleWorld28.resolve_kind3_catch_relation preflights both definition.frame actions before mutation; native frame accessor implicit0..998/declared999/null out of range.
evidence: Actual BattleInteractionWriter.TryApplyKind3Grab and HitPlan.ProjectGrab still use legacy HasFrame/GetFrameDataById, rejecting high/implicit native frames.
-->

# kind3成对抓取的Native帧准入和绑定

准确五脚本。原actual writer经CharacterInteractionResolver/CharacterDatInteractionResolver/SpecialAttack/WeaponInteractionResolver进入；shadow投影位于BattleEcsHitExecutionPlan.ProjectGrab的strictKind3分支。原SimulationTickDriver候选消费调用resolve_kind3_catch_relation，battle_world.cpp:5205同时取得两个definition.frame，任一null在任何写入前unsupported。原frame0..998含implicit零帧，声明999可在此阶段绑定（后续C25另裁决生命周期），1000及缺失999不可用。

先单CPP原几何候选+resolve方法800个配对向量：10个正负/低/高/终止action×两侧declared布尔×左右位置，输出before/after raw与catch关系/timeout、不可用无修改和RNG0。只为生成测试初值，不修改正式source/EXE。Unity先800原向量direct/实际candidate+ShadowCompare和两profile定向RED，再将actual preflight取得的Native descriptor传入已有SetCpointRawFramePreserveWait(frame, descriptor)，避免改共享helper影响其它未审计调用者；HitPlan strictKind3同步Native preflight及geometry descriptor，kind1保持原路径。

旧B6测试把未声明99当作缺失，按原证据改为真实越界1000或未声明999；保留无修改/成对预检/负action翻面/缓存几何与0分配断言，原FAIL留证。不修改抓取坐标算法、counter/latch/持有关系语义，不加入其它frame writer或缓存全局开关。完整kind3 Step7关系写入是本包原子出口；下一tick CPoint/throw/pickup及其它legacy reader仍另回访，不把本包标为全部抓取流程已对齐。

验收compile、source重复/正负边界、raw47+关系字段、actual/ShadowCompare差异0、旧B6相关、SelfCheck和真实Scene关系建立/恢复/关闭。无新持久字段/schema/服务/队列，沿用既有World关闭；回滚仅本差量且按规则获批。非战斗/框架/Scene/资源/Server/Gen/Plugins不改，禁止computer-use。

Source实际800：512 applied、288双实体不变的unsupported；原frame可用性公式全部吻合，RNG0，重复SHA d8263c342390f32c7f38393291229bb6b57f0d13595d356a205a0584e3a94dbb。最终fixture group1/2，便于同一输入运行实际Step7+ShadowCompare；initial group0输出仅保留前期诊断。第一次runner误用不存在的fall_244编译失败，已按源实际hit_reaction_timer修正，失败log保留，正式source未改。新Unity测试脚本已写，source.before/after raw47加catch target/source/timeout、mode直接和真实candidate+shadow两路径；生产尚未修改，先RED。

实际RED 7df4a1b6918f457a9e9a532689d22a75：Authority400 direct/shadow两组800均FAIL，7126/6664条，initial无差异；case8未声明10已被旧HasFrame错误拒绝。RNG与shadow内部差异0（两边一起错误拒绝），证明必须同步actual与projection而不能以shadow0判原版一致。原XML/JSON留red，开始两生产修改。

生产已写：actual两Native descriptor成对预检后传给原共享setter；strictKind3影子预测同步Native descriptor几何，kind1分支原样。40323df9acd545f9bc6f078b78857bc2实际23项17PASS/6FAIL；新4组3200向量、两profile/direct/actual+shadow、before/after raw47和relation fields全部0差异，六个失败均为旧B6将未声明99/98当不可用。原23 XML保留，现仅将这些不可用fixture改为1000/1001并同步旧Play调用，不改断言和生产规则。

新增同测试文件真实Play probe：80边界配对（正负998/999、1000、0/998、99/857及单侧越界）×两actual factory×direct/候选Shadow=320关系建立场景。世界构造允许Renderer分支，默认logic-only不变；源before/after raw47/关系字段/随机0、配对拒绝无修改、Shadow0和场景checksum/borrower恢复/关闭验收。不执行后继CPoint阶段或宣称整套抓取流程完成。

事前扩展准确第六路径BattleRuntimeSelfCheck.cs：Check原B6 missing relation self-check也明确使用caughtact99作为不可用前置，与原800见证冲突。先实际完整SelfCheck留FAIL，再仅将该捕获目标改1000，保留无修改/随机数/关系断言；不扩大生产修改。

最终定向a66ba45274614f17a8ec89a8e03c21ff 25/25 PASS（含kind1/3影子回归）。完整SelfCheck04:31:46Z实际FAIL于CheckKind3CatchRelationExactFieldsContracts旧99不可用假设；已归SelfCheck-first-fail.result，仅把该caughtact改1000，整对状态及RNG不变断言保持。

## 最终限定出口（2026-09-14 04:36Z）

VERIFIED / KIND3_NATIVE_FRAME_ADMISSION_AND_PAIRED_BINDING_ONLY。实际六脚本：原runner、actual writer、HitPlan projection、新测试、旧B6测试、SelfCheck夹具。两个生产方法改为同一Native descriptor成对预检，实际绑定通过已有sourceFrame参数传入；kind1和其它共享raw/CPoint setter未改，后继CPoint/throw/pickup仍待迁移。

原800向量512允许/288拒绝，拒绝两实体完整before/after完全相同，native/CRT0；最终异组1/2重复输出一致 SHA d8263c342390f32c7f38393291229bb6b57f0d13595d356a205a0584e3a94dbb。验证含0/未声明低帧99/856/857/998、声明或未声明999、1000、负998/999、两方向及双方独立可用性。before/after raw50输出另加catch三字段；Unity仍按47绑定+3MISSING比较，不发布全50完成。runner最初误用字段编译失败的log和旧group0诊断保持；最终build通过，正式EXE/75-source身份未改。

Unity RED：7df4a1b6918f457a9e9a532689d22a75两组FAIL，before无差异，actual与shadow同时错误拒绝原允许帧（所以shadow内部0不能证明原版对齐）。修复后40323df9acd545f9bc6f078b78857bc2实际23项17PASS/6FAIL，新四组3200向量全0差异；六FAIL仅旧99/98不可用夹具。改为真正越界1000/1001后a66ba45274614f17a8ec89a8e03c21ff 25/25 PASS，包含kind1/3旧HitPlan和零分配断言。原XML/JSON均保存。

完整SelfCheck首04:31:46Z FAIL于同一旧99不可用前置，扩展准确第六路径后只改该caughtact为1000；最终请求04:33:40.8466554Z，04:34:40Z实际PASS。不能把旧断言改动说成新增生产行为。

真实Play320：80边界配对×两actual factory×direct/真实candidate+Shadow，全部before/after raw47/关系字段/随机0/预测一致，Scene checksum不变、Renderer2→2。Play-320-pass.json。Shutdown-pass.json：恢复4→4、World/slots/logic/render全0，两帧Stopped，已退出Play。Scene dirtyfalse/root14/SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6，CS error0。Ledger/diff-check交付前验证；无computer-use、资源/Scene/非战斗/Unity-GAS/Server/Gen/Plugins更改，无commit/push。

返回原Native reader umbrella；下一精确审计是kind2拾取目标frame的actual/HitPlan读取及其holder原始写帧消费者，尚不预判所有getter都是bug。CPoint action selection/throw原旧定义快照、direct/raw/held setter、其它input/hit/出生消费者继续。已关C25/frame和本kind3不重做。原已使用World的canonical epoch恢复缺口仍开放，正式DAT/角色图迁移与总目标未完成。
