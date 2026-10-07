<!-- CHANGE-RECORD
id: NTSD-OPT-H11-OBSERVER-STAGES-033
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs
authority: approved six-item H11 full selected zero-GC and section13 continuation; preserve formal336 and production
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH33-OBSERVER-STAGES-20261007/REPORT.md
-->
# H11 EndObserver 六子范围分配归属
本批最新终态 PARTIAL / PROVENANCE_PASS / ZERO_GC_FAIL。RUNTIME_PENDING指完整0GC与具体调用点未闭合，不是1800窗口尚未执行。本批只改声明的Editor probe；旧27/28入口的新增flag默认false，完整camera/root observer严格门、生产Runtime与shutdown顺序不变。所有新recorder沿原DisposeAllocationRecorder在退出/重载释放，不新增Runtime所有者。

实际测试：原Editor job259f0f6fa1034f9e8039d7c82f167004具名10个，3有效汇总stub RED、其他7通过；job9518b84e9ad04094818e623daad4b46a GREEN 10/10、1.3274319s，含nested空/已知1MiB正反。9391只是测试catalog，不是执行量；旧27正例和两CPU桥未重跑。

实际一次33菜单/自然1800camera，tick8→1256，camera scope2事件（ordinal1/5），observer13全部ordinal0，六块snapshot/gates11、segmentBindings0、commandsAndDraws0、alphaAndTiming0、sampleWrite0、completion2；children=root、unattributed0、invalid0，两recorder前后校准PASS。GC marker unit TimeNanoseconds，仅事件数可归属，不是字节；未证具体getter/Require/初始化调用，也不作冷启动豁免。完整root严格FAIL保留：The full camera/observer scope did not prove calibrated zero GC.Alloc events; evidence retained.

两slot、活动Foot/Health每帧至少各2、11560中央CPU draw录制=执行、growth0、CPU read lease0。原11阶段orderedShutdown=true，objects/slots/borrowers=[0,0,0]；Battle SHA253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010及Menu SHA6B5BAD6DB12E2C5FA3324F275FE87E788C27B0F0B562E2637B92BDDD6DD204AE保持。只退出后load原Menu、不保存Scene；最终Menu8roots/isDirty=false。PID19040未重启；EnterPlay/reload暂时6401拒绝后同实例恢复，没有重复菜单或第二Editor。

2026-10-07T09:11:28.7220661+08:00 post-measurement audit：247非写域SHA mismatch0、8当前字节备份不变、HEAD8107196b1f17ee0f7ce9e7fcbb7ffb9fc260956c保持，source冻结95692A5EB0493A59251D3B795DE8C90ABF306D0B1C9B851BF5DD95691291440D同；Tools/Validate-ChangeLedger.ps1 PASS/1329 records/11 governed code files，4248历史warning如实保留；git diff --check exit0无诊断。新证据仅本33目录；32与Q06等既有保护路径未改。最终文件审计另见Operation after.json/final-audit-01。

未验证：准确分配调用栈/CPU camera另2来源、完整战斗0GC、默认路径额外诊断分支成本、1000AI新帧率、真实GPU/Android/native逐tick一致性。本批无FPS收益主张，H11仍FAIL/H07未达/Goal active；不再重跑同构窗口、盲改生产、切默认或解冻专项门。下文为按发生顺序保留的历史阶段，不代表当前尚在Play。

CAMERA_RUNNING：原6401 manage_scene(load,name=NTSD_Battle,path=Assets/NTSD/Scene)成功，33菜单仅一次；get_editor_state已证Battle isPlaying=true/isChanging=true、PID19040仍在、source冻结SHA同。terminal尚未出现，启动/1800窗口待完成。EnterPlay重载6401暂拒后同实例恢复，不重启/重复菜单；运行期不改C#/Assets/refresh/测试，原完整scope拒证仍保持。
pre-run-audit-01：247保护/8当前副本/HEAD同、validator1329Records/11files PASS、4248历史warning、diff-check无诊断，new output absent；source SHA95692A5EB0493A59251D3B795DE8C90ABF306D0B1C9B851BF5DD95691291440D已冻结。原Menu8roots clean/非Play，下一仅load原savedBattle→一次33菜单，不改Scene/运行期间不写C#/Assets/refresh/额外测试，原截止不重启。此段CAMERA_READY不是已采样通过。
GREEN原Editor job9518b84e9ad04094818e623daad4b46a实际10/10 PASS，1.3274319s，包含新nested空/已知1MiB正反；原27四校准/两CPU桥不重跑。新增程序集/原PID恢复、没有新编译错误。下一冻结source/247保护/8副本/validator后一次33原1800camera；尚无真实归属/完整0GC通过，runtime不修改。
CODE_WRITTEN：独立33菜单/原1800camera，6固定子scope与第二预校准recorder、preallocated帧值字段、完整root差额保留；startup/complete校准，Exit/重载原Dispose释放，旧27/28flag默认false。原camera/root严格FAIL门未改。下一同10 GREEN/一次camera，尚无真实新归属或0GC通过。
有效RED job259f0f6fa1034f9e8039d7c82f167004实际10用例，3有效汇总stub失败、5拒证及2nested已校准工具正反通过；不是catalog9391实际执行量。原PID短暂reload6401拒绝后同PID恢复，未重启/换实例。下一只实现声明的6子范围、GREEN同10，再一次1800camera；nested空范围未增加root GC事件，正例child/root都记录，旧27四项/CPU桥未重跑。
test-first已写：6固定值scope字段/Get/Set、汇总stub false；10本类cases，3有效汇总应RED，5拒证/2nested工具正反应通过。原生产observer/Renderer未变，尚未新编译或执行；下一原Editor仅此类真实10 RED/GREEN，不重跑27旧已过矩阵。
PLANNED，脚本尚未改。准确scope/10 test-first/一次1800camera及8写域在[Task](../TASKS/NTSD-OPTIMIZATION-BATCH33-OBSERVER-STAGES-20261007.md)。既有28 EndObserver13/camera2真实FAIL保留；新子Recorder完整root不缩小、不跳早期帧，不把工具/归属进步叫FPS收益。
修改前当前byte备份与保护manifest将保存本Operation before.json。字段、生命周期、溢出拒证、关闭/回滚与未确认项同Task；实际代码/验证/错误/原命令后续追加。无native/Android/GPU/1000AI新证书，H11/H07/Goal仍开放。
