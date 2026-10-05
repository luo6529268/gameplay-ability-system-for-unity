<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponFrameLogicResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs
authority: formal 336B44 NativeAi28 step_non_character_hit_fa source X/Z threshold consumers and user D-024 shared ratio entry requirement
evidence: artifacts/diagnostics/NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-20261005/REPORT.md
-->

# 共用追踪规则坐标入口

2026-10-05完整Driver限定出口已通过：job242dd61ff0514d7d9eee2746486fb120正式summary1/1 Passed、0失败、24.1515287秒。正式518/1无dvz/OPoint，唯一生产tick后Vz0、精确sourceZ600/target604/viewDeltaZ0、两对象及目标槽保持，正常wrapper scope三计数归零并恢复发布；旧875注销/新518注册与Roster新StableId均有直接断言。最后原Editor idle/非Play/无测试、原Battle clean/root11、Console0error，四保护SHA/四authority SHA/七定义两端SHA保持。七DAT有六份原字节同、一份仅CRLF/LF同，不改DAT；只读最终审阅无新增阻断发现。Report保存原件与限制，本Record维持RUNTIME_PENDING（仅自然原Scene/正式根同初态/GPU等未覆盖范围），必要ONE完成按REUSE，220份同名Record/59未关闭为REUSE45/TRIGGER14/P0=DEP=ONE=0；不重跑18项或完整tick。

2026-10-05唯一完整Driver方法已写/生成0错：IndexedTrackingSourcePairSurvivesOneFullDriverTick按Task事前范围加载正式518/1，断言无dvz/OPoint，注销旧875并逐项确认旧槽/绑定已清，再同slot1注册新518、更新诊断Roster身份并检查计数/引用。源600/604、Y-100/0、统一view/零速度，单次生产StepOneTick，既有scope正常归零与发布恢复。生成工程退出0/301warnings/0errors/8.58秒；原Editor唯一单方法待终态，不重跑18项。

2026-10-05原Editor GREEN已取得：jobbb07a0d205e84ffaa0a3d4a656ce985f正式summary18/18通过、0失败、124.5701521秒；新12项及旧7四边界/active/raw两邻例均通过。生成build退出0/334warnings/0errors/15.42秒。两次get_test_job includeDetails=true观察超时保留原件，没有重启测试；读Editor日志确认RunFinished后同job includeDetails=false取得终态汇总。独立只读审阅精确before diff确认共同门、raw/incomplete/type0/武器4特殊fallback、旧7/阴影和Y等边界保持。必要完整Driver按Task已声明518/1同槽诊断替换，代码尚未写；不会重跑18项。

2026-10-05生产增量已写：新增LF2Entity.ResolveFrameLogicPositionPair一处共同入口，active target/双方source初始化/当前非type0才成对读源X/Z，否则保留整对view/raw。通用1/3/2/4/12/14和非角色7调用；4提前回收和后续追踪共用同一读数。LF2WeaponFrameLogicResolver专门4/12调用，4的source不完整时明确保留旧GetRenderZInt fallback。未改任何Y/速度常量、目标扫描、HP门、DAT、资源、Scene、非战斗或关闭阶段；既有7与平台阴影增量保留。生成与原Editor GREEN进行中，未取得终态前不称通过。

2026-10-05原Editor RED已确认：job804fecf984b24ae1b42c89be989fad61终态failed/completed12，六项失败均为预期规则首差：固定视野875/gap±10误Vz±0.17、700/gap6误0.3、518及124/gap4误0.4、219源X29/Z9应action60而实为0；另外六项包括恒等、正向追踪、不完整source fallback及严格回收阴性通过。MCPresult为null，计数由completed12/六条failures_so_far证明；8858是发现数，不能称执行总量。无夹具异常。独立只读审阅认为测试构造合理；它另确认875/frame50的dvz550会在完整tick后继frameMotion归零，不能用其tick末零速度作死区证书。完整Driver必须选Task已声明518/frame1或记录证据边界，不改DAT/pass保留首差。下一只在三个已声明脚本实现共同入口。

2026-10-05测试先行已写：既有测试档新增IndexedTrackingDeadZoneUsesOneRulePositionPair八参数和IndexedRecoveryUsesStrictSourceDistanceBounds四参数。仅正式DAT字段和source/view一致构造，检查精确积分、三死区、124/type4武器12、目标source不完整及4严格回收边界；finally注销，没有trace/请求写出。生产尚未修改。实际生成Editor命令dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly退出0/301warnings/0errors/7.67秒，原Editor具名RED待运行。

2026-10-05脚本前登记：source排序已修而追踪/回收仍读view的实际消费者候选。正式三个死区与回收X/Z边界、type4武器124/frame40源/对象工厂入口已只读闭合；原Editor当前idle、非Play、原Battle clean/root11。尚未改脚本或运行新测试，不把静态首差候选称自然Play证书。

[Task](../TASKS/NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-001.md)记录完整前置、三个脚本/准确符号、raw/不完整/type0与Y保留边界、生命周期副作用、窄验证及回滚。NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-001-EDIT-20261005保留实际操作前字节和范围；已有7与平台阴影修改原样保留，生产先等RED。

最终治理检查：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity 实际exit0/PASSED，1273份Record覆盖当前24个dirty governed脚本；本包仍仅拥有声明的三个脚本增量。首次git diff --check发现四份本轮状态文档新增EOF空行，已仅去掉这四个尾空行，未重排其它内容；最终git diff --check实际exit0。
