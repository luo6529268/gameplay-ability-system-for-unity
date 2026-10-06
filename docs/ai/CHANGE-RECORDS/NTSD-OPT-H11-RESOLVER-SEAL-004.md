<!-- CHANGE-RECORD
id: NTSD-OPT-H11-RESOLVER-SEAL-004
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderTypes.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleResolverCapacitySealEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCatalogCentralResourceResolverEditorTests.cs
authority: user approval to execute next documented H-11 presentation-only capacity batch; formal336B44 rules unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH04-RESOLVER-SEAL-20261006/REPORT.md
-->

# H-11资源解析器准备入口封口

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH04-RESOLVER-SEAL-20261006.md)；[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH04-RESOLVER-SEAL-20261006/RECORD.md)。
原状：已有cached insertion seal，但PrepareCapacity可绕过seal显式Ensure/Resize并提高逻辑limit。
预计唯一生产hunk：有效参数校验后、两个容器或limit写入前sealed抛InvalidOperationException。
既有Unseal/Configure/ResolvePrepared/Resolve缓存满skip/颜色/绑定失效与no-op不改；
这是缓存准入合同修补，没有native gameplay新字段，非新算法或资源/预算变化。
owner/释放沿现有Central静态resolver及End/Reset/Configure，未增queue/GPU buffer/关闭阶段。
验收范围和回滚见Task；新增test先RED再最小修补、原EditorGREEN及既有具名回归。
静态可达增长不等于当前已测每帧分配或唯一热点，完整链/Scene/设备仍pending。
八文件原状备份含前三批dirty；无删除/移动/破坏性Git/Q06 body读取。
当前未修改脚本或运行本批测试，结果按实际追加。

2026-10-06 test-first：独立test已写，生产仍精确before原状；CODE_WRITTEN仅测试。
15具名case覆盖sealed Prepare/negative/Unseal/两resolver owner/0,1,17缓存满strict与Prepared解析/
Configure no-op、材质切换、存储/limit/字段及局部GC断言；RED/编译尚待原Editor执行。

2026-10-06 第一RED请求934bbcd0d51c433d898709bd68a716b2完成15项：
2参数控制通过，central Prepare缺guard负例失败；其余12项因fixture反射选到另一个27参数bool重载失败。
这12项不算生产缺口证据，原件red-initial-result.json保留。只改新fixture的构造器类型筛选，
要求14/15/16参数为RenderState/ValueDescriptor/object；生产仍before原状，重编译后重新RED。

2026-10-06 第二夹具选择请求d8b25d5bf9eb47778456a3752bf1e746仍为12个fixture失败，
中央缺guard负例1失败/参数控制2通过，原件保留，不能当有效15项RED。
Roslyn只读metadata查询因环境不可用未执行，未安装或换CodeDOM。
改用既有Unity.Cecil只读当前程序集构造器签名（不读取IL/body）：RenderState+trusted实际28参数，
末尾MotionAnchor；27参数trusted重载实际flipX bool。新fixture改精确28类型并追加default anchor。
已有resolver旧test helper也使用过时27类型，列为待核测试适配，不把它当生产规则缺口。

实施前准确范围补充：既有BattleCatalogCentralResourceResolverEditorTests.CreateTrustedCommandWithIdentity
仅反射构造器类型末尾和source.MotionAnchor实参适配，预期断言/其它helper不改；
before-test-addendum原件与新备份已登记，不读写Q06方法体。

2026-10-06 有效RED jobf11432bc9b3f498ead19bcd495e786fc：新15项10控制PASS/5预期guard失败；
旧具名trusted-cache测试1项反射签名失败。16项结果原件red-result.json保留。
生产本批before SHA保持到该RED后。实际已写唯一生产PrepareCapacity sealed检查（参数检查后、
Ensure/Prepare/limit前），既有测试helper补28末尾MotionAnchor类型/实参。
不改解析/缓存满skip/失效/颜色或材质绑定，编译/GREEN/旧完整具名回归尚待执行。

2026-10-06 原Editor PID19040/TCP6401实际编译程序集19:50:48/50，idle/error CS0。
GREEN job30da70bca4934400a76c075037a0e7e3：新15/15与适配旧1/1全部PASS，无跳过。
capacity0/1/17 × strict/Prepared，32个不同资源64轮Configure no-op+Resolve局部0B，
全部output/颜色/UV/size/pivot保持，逻辑缓存满skip而非丢draw、存储身份和limits不增长。
guard4正负容量、两resolver owner、Unseal及配置换材质存储复用通过。
旧resolver/三批/mesh/LatestFrame/motion/辅助/common绑定具名回归请求已发，结果待收。
父H-11完整链0GC/退出重进/预算/1000AI/Android未验收；无Scene/资源/配置/专项门变化。

2026-10-06 相关回归job22280521ddd54125b48d14c25546ae1a实际105/105通过，0失败/跳过；
resolver23、前三批38、mesh8、LatestFrame13、motion2、Foot7/Health8、common绑定6。
新15与该105去重为120；GREEN的适配旧1已包含在回归105中，不重复计算。
两existing代码diff为生产3插入行、旧test helper2插入行，无其它hunk；Q06方法体未读写。
原Editor post Menu clean/idle/nonPlay/error CS0。完整链/Scene/native/GPU/设备验收仍RUNTIME_PENDING。
