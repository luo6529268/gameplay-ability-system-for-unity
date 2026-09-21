<!-- CHANGE-RECORD
id: NTSD28-Q06-STATE1218-CONTACT-NATIVE-FRAME-BINDING-001
status: VERIFIED
change-kind: STATE1218_CONTACT_NATIVE_FRAME_SOURCE_WITNESS_FIRST
code-path: Tools/NTSD28AuthorityTrace/state1218_contact_native_frame_witness.cpp
code-path: Tools/NTSD28AuthorityTrace/validate_state1218_contact_native_frame_witness.py
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State1218ContactNativeFrameEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
authority: Formal playable BattleWorld28::step_physics and PhysicsIntegrator28 state12/18 contact native action selection.
evidence: Remaining live caller audit; no fresh Unity RED for this scope yet.
-->

# NTSD28-Q06-STATE1218-CONTACT-NATIVE-FRAME-BINDING-001

IN_PROGRESS / SOURCE_WITNESS_FIRST。Q06剩余reader审计确认正式SimulationTickDriver28→BattleWorld28::step_physics→state12/18 contact action路径；soft230/231/counter0、hard185/191或pending动作/counter保持，environment先于action；当前Unity LF2Entity.ApplyCurrentDatType0State1218ContactAction两个旧raw binder仍活跃于exact/canonical/shared type0物理。已闭B4规则/前包ordinary landing与candidate reset不得重做。

第一写域仅Tools/NTSD28AuthorityTrace/state1218_contact_native_frame_witness.cpp。复用既有source capture include和真实world.step_physics(0,{})及后继driver，保留source186原文件。矩阵覆盖soft边界11与±9、hard越界/state18、action185/186分界、pending1/非1与signed/implicit/high/invalid选帧，显式/隐式descriptor、counter/latch/Prev/snapshot和pendingmotion；floor0/-10、非接触控制。完整before/after/following、raw、RNG序列及独立extra字段。先源码双跑和独立oracle，再准确声明Unityfixture/生产符号，不提前改Unity。

源derived witness不是正式EXE行为录制；与当前playable闭包绑定。正式原EXE只读，源previousXYZ无Unitycarrier必须沿用明确source-only边界，既有raw3缺口保持。环境伤害/KO不以本包重新定义；主矩阵先固定environment0以隔离descriptor，必要非0控制明确额外字段恢复与比较，不放宽断言。

后继门槛：源独立验证、Unity RED及最小caller修复、compile/聚焦回归/SelfCheck/replay/Play/有序关闭。禁止共享getter批改、kernel/schema/资源/Scene/非战斗修改。回滚只同ID精确diff且遵守用户授权删除规则；未知交叉差异独立Task追踪，不改expected掩盖。没有新增runtime模块，不涉及shutdown顺序变化。Q06仍ACTIVE/Q07未迁移。

源runner build与双跑exit0、480行完全一致SHAd898b84df517ebc3b5c21b1113c1a390baaedadffa961f35d8e385608dffc6c1。新增准确独立验证器Tools/NTSD28AuthorityTrace/validate_state1218_contact_native_frame_witness.py：复用既有initial_entity定义核对初态，独立params计算物理/选帧/pending消费/descriptor/counter/snapshot/RNG保持并比较完整after；following仅保留源capture不独立模拟。正式静态默认目标全显式、19461 ITR自定义pending动作均0，详见mapping audit；不因synthetic源通过自动推进生产。

源480独立228481检查PASS，初版模型8处误认implicit999已按DatDocument::frame纠正，首次失败存source/independent-initial-model-failure.json；following/stepflags无独立模型。准确新增Unityfixture路径NTSD28Q06State1218ContactNativeFrameEditorTests.cs：复用ordinary真实physics+following及sameWorld/Play模式；新增pending七字段restore/capture，不改旧fixture。四矩阵初态校验先行，再决定两个生产caller是否有测量差异。正式域静态零隐式不等于动态身份全域已证明，也不构成已经有玩家可见Bug。当前生产未改。

Unityfixture初稿pending dx/dy/dz错误float赋值触发编译，核对NTSDEntityRuntime三carrier为int后纠正，源矩阵亦使用int。未改carrier或生产。新fixture同时沿用回放/Play方法但当前只计划跑直接before/physics/following四矩阵；正式Play尚未执行。

STATE1218-CONTACT-NATIVE-FRAME-BINDING-001已测RED：job87f3c88664724d3b80bf2501db16de3b终态FAILED4/4，两profile×两路径各480 before0/immediate410/following120；即时392 contact+18 airborne-control，后继120全部contact。production-red四JSON/XML已存；生产未改。下一先精确扩Record仅LF2Entity.ApplyCurrentDatType0State1218ContactAction两个raw binder，修后保持完整480控制不删；airborne18另建独立Task/source见证，不在contact包顺手改第三caller。源480/228481模型PASS，但stepflags/following未独立模型（review泛称flags由模型承担不适用当前脚本，按实际scope）。静态正式330默认目标全声明及19461 ITR无自定义pending动作保持，不把synthetic RED称正式玩家Bug；动态身份域仍待。前ordinary/candidate已VERIFIED不重做，Q06未完/Q07未迁移/总目标ACTIVE。无运行job/build/agent/Play，复用Editor62860；禁computer-use/非战斗/Scene/资源修改。
独立只读fixture review通过：完整DAT目标同源声明/999/原始snapshot/pending7字段/输出与probe隔离正确。当前未跑replay/Play，未改生产。

生产修改前准确扩域：LF2Entity.ApplyCurrentDatType0State1218ContactAction内soft/hard两次DirectWriteRawFramePreserveWaitCounter改用既有DirectWriteNativeRawFramePreserveWaitCounter；保护soft counter0、hard counter保持、pending消费顺序及环境伤害原事务。仅改变目标descriptor语义，implicit0..998零帧、显式999有效/缺失999无效及高位/负值raw保持；已有source480/Unity RED构成依据，不能声明正式内容动态缺陷。airborne控制仍完整保留且预期剩余差异须独立解决。回滚仅本Task两个caller，不能回退同文件ordinary已验改动。

contact两binder已修，job45daa31087ba4319ac0fa19b0f9e36fb终态13项：旧contact9PASS/四矩阵仍FAIL；四组各480 before0/即时18(全airborne)/following0。完整产物after-contact-binding。contact生产仅两caller，父任务等待独立airborne出口后一次联合验收，当前不重复旧矩阵。
用户要求收敛重复验证（2026-09-21）：按相关分支/数据等价类选代表案例，不能只按角色名扩矩阵；局部修改先编译+原差异最小案例+受影响边界。未变化已通过证据复用，只有新修改/失败/未决风险才扩测。完整SelfCheck与相关联合回归在一个闭合执行包出口集中一次；Play按实际受影响路径/生命周期选代表，不每改一行都重跑所有配置/角色。Q07内容迁移与Q12完整集成出口不缩减，原失败不得删除/排除来变绿。

按用户分支等价类验证：airborne源55双跑一致SHAa4d7c8fd…52e691/独立24696检查PASS；Unity RED55即时30与smoke8即时7，before/following0，后仅改LF2Entity.ApplyCurrentDatType0AirborneAction单binder。联合job e6d47ce73ce2473180ce73ea5b4eca50已29/29PASS（airborne17旧+contact9旧+新55/8两测试+contact主路径480一个测试），实际8.03秒。新55/8与contact480 before/即时/following均0，证据两包joint-pass。不要重复另外三contact矩阵；已过分支复用证据。完整SelfCheck请求已消费，PID62860当前运行，等新结果勿并行C#改动/Unitytests。下一用代表案例补sameWorld replay与一次Play：contact选覆盖soft/pending/invalid/explicit999/hardmotion的约12例，两条配置路径各验证必要代表；airborne8代表；不要跑旧3840全乘积Probe。代表filter/计数变更前扩Record。两包仍IN_PROGRESS，Q06未完/Q07未迁移，总目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

最新完整SelfCheck 2026-09-21T00:45:27.002427+00:00 PASS已归档joint-pass，之前运行中描述被本条覆盖。当前无运行job，下一仅代表replay/Play，勿重复SelfCheck除非新生产修改或失败。

代表验收实现前登记：contact现有单Editor fixture新增RepresentativeIndices12={0,78,144,216,288,294,300,301,410,416,444,456}，保留原480直接矩阵默认不变；replay改为这12例×Authority/DataOriented与Mobile/Legacy两入口。airborne既有单fixture新增8代表sameWorld replay（0,4,8,15,19,32,36,44）。contact既有PlayProbe集中调用两fixture：Authority/DataOriented/renderer各12+8、Mobile/Legacy/logic各12+8，共40，不再3840笛卡尔乘积；Scene checksum/borrowers和Q05关闭保护不变。各自Record声明测试路径已涵盖，无新生产修改，不重复已过SelfCheck。代表覆盖与未重跑范围写明，不把代表计数宣称全矩阵。

最终VERIFIED_SCOPED，详见同ID artifacts/diagnostics/ACCEPTANCE.md：联合29/29、SelfCheck、代表回放40/80tick与Play40/Q05关闭PASS。源与矩阵证据分开，未重复运行配置范围明确；不关闭Q06/平台/图像域。
