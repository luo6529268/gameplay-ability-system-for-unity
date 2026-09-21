<!-- CHANGE-RECORD
id: NTSD28-Q06-STATE1218-AIRBORNE-NATIVE-FRAME-BINDING-001
status: VERIFIED
change-kind: STATE1218_AIRBORNE_NATIVE_FRAME_SOURCE_WITNESS_FIRST
code-path: Tools/NTSD28AuthorityTrace/state1218_airborne_native_frame_witness.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State1218AirborneNativeFrameEditorTests.cs
code-path: Tools/NTSD28AuthorityTrace/validate_state1218_airborne_native_frame_witness.py
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
authority: Formal playable PhysicsIntegrator28 airborne selection and BattleWorld28::step_physics upcoming phase.
evidence: Contact480 source matrix airborne controls measured18 immediate differences in each Unity matrix.
-->

# NTSD28-Q06-STATE1218-AIRBORNE-NATIVE-FRAME-BINDING-001

IN_PROGRESS / SOURCE_WITNESS_FIRST。父contact480的24个airborne control中有18项即时descriptor差异（before0），独立于soft/hard contact两个caller。当前正式PhysicsIntegrator28 state12 airborne按照postgravity Vy<-8/<1/<8选择180..183/186..189；action185/191不选；负environment仅前侧family按Vy<12与upcomingPhase>=6选182/181；state18 action<205且postVy>1选205。源BattleWorld28::step_physics覆盖context phase为(resourcePhase+1)%12。

第一写域仅Tools/NTSD28AuthorityTrace/state1218_airborne_native_frame_witness.cpp，实际world.step_physics+following driver，保留contact/ordinary源runner。按阈值上下与等号、action family边界、负environment/phase5→6及11→0、目标声明/隐式、无action控制和floor0/-10组合有界采样，避免冗余笛卡尔爆炸；DAT真实当前frame与目标相同需避免重复覆盖。输出before/after/following raw/descriptor/B2/nativeRNG/extra environment与phase、source-only previousXYZ。phase只能通过已有正式World接口或同源state恢复构造，不改权威源码。

确认source后新增精确Unityfixture及唯一LF2Entity.ApplyCurrentDatType0AirborneAction单binder路径，当前生产未授权本Record改写。不能改已闭selector/重力/phase顺序，只核对descriptor；不能修改共有raw setter或前包contact/ordinary行为。既有raw3映射缺口与previousXYZ边界不掩盖，environment额外字段比较须显式恢复。

验收：source双跑+独立oracle、Unity RED→最小修复→compile/回归/SelfCheck/replay/Play/有序关闭，父contact保留全部控制后联合验收。正式330动态可达尚未证明，synthetic witness不能宣称真实玩家已触发。资源/Scene/非战斗/GAS/schema保持；回滚仅本ID声明差异且保留其他工作。Q06未完成/Q07未迁移。

按用户收敛验证要求：新增单Editor fixture NTSD28Q06State1218AirborneNativeFrameEditorTests.cs，复用ordinary helper结构但主配置Authority400/DataOriented跑source55代表，MobileExtended/Legacy只取8个分支代表（0,4,8,15,19,32,36,44），不构造55×所有配置/工厂乘积。阶段phase12/3按源begin_native_resource_tick前置恢复，environment额外字段单独capture。先RED后单caller；不每次小改跑完整SelfCheck/Play，联合出口集中执行。生产当前未改。

源runner归还写权后构建session64770运行；准确新增独立validator validate_state1218_airborne_native_frame_witness.py，用params重建初态和即时实体变更/phase选择，比较完整投影、RNG不变与无即时call；不模拟following。55代表覆盖多个分支，requestedPostVy=-8实际double积分为-7.999999999999999，不能声称精确-8等号，使用实际post计算，保留±邻值两侧覆盖。Unity新fixture已编译，尚未执行。

源55双跑一致SHAa4d7c8fd…52e691，独立24696检查PASS；validator初稿key targetDeclared拼写错误已修为实际targetDeclarationRequested。Unityjob2f394a51385f4953983c6645047dbb7c两测试RED已归档production-red，初态必须零差异；差异为implicit wait。生产修改前精确扩域：仅LF2Entity.ApplyCurrentDatType0AirborneAction最后单binder改DirectWriteNativeRawFramePreserveWaitCounter，保持Resolve/phase/环境/速度/负action返回/计数器不变。已有contact/ordinary同文件变更不可回退。复验55+8，联合出口再跑相关旧回归/一次SelfCheck/代表Play，不重复全场景乘积。

独立只读复核三个caller通过：soft/hard/airborne只有native binder替换，selector/phase/速度/pending/counter/latch/snapshot保持；不称四配置全矩阵重跑。当前联合job e6d47ce73ce2473180ce73ea5b4eca50包含airborne55+8、contact仅Authority400/DataOriented480一次及两组旧state12/18回归；等终态，不重复另三完整组合。

按用户分支等价类验证：airborne源55双跑一致SHAa4d7c8fd…52e691/独立24696检查PASS；Unity RED55即时30与smoke8即时7，before/following0，后仅改LF2Entity.ApplyCurrentDatType0AirborneAction单binder。联合job e6d47ce73ce2473180ce73ea5b4eca50已29/29PASS（airborne17旧+contact9旧+新55/8两测试+contact主路径480一个测试），实际8.03秒。新55/8与contact480 before/即时/following均0，证据两包joint-pass。不要重复另外三contact矩阵；已过分支复用证据。完整SelfCheck请求已消费，PID62860当前运行，等新结果勿并行C#改动/Unitytests。下一用代表案例补sameWorld replay与一次Play：contact选覆盖soft/pending/invalid/explicit999/hardmotion的约12例，两条配置路径各验证必要代表；airborne8代表；不要跑旧3840全乘积Probe。代表filter/计数变更前扩Record。两包仍IN_PROGRESS，Q06未完/Q07未迁移，总目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

最新完整SelfCheck 2026-09-21T00:45:27.002427+00:00 PASS已归档joint-pass，之前运行中描述被本条覆盖。当前无运行job，下一仅代表replay/Play，勿重复SelfCheck除非新生产修改或失败。

代表验收实现前登记：contact现有单Editor fixture新增RepresentativeIndices12={0,78,144,216,288,294,300,301,410,416,444,456}，保留原480直接矩阵默认不变；replay改为这12例×Authority/DataOriented与Mobile/Legacy两入口。airborne既有单fixture新增8代表sameWorld replay（0,4,8,15,19,32,36,44）。contact既有PlayProbe集中调用两fixture：Authority/DataOriented/renderer各12+8、Mobile/Legacy/logic各12+8，共40，不再3840笛卡尔乘积；Scene checksum/borrowers和Q05关闭保护不变。各自Record声明测试路径已涵盖，无新生产修改，不重复已过SelfCheck。代表覆盖与未重跑范围写明，不把代表计数宣称全矩阵。

最终VERIFIED_SCOPED，详见同ID artifacts/diagnostics/ACCEPTANCE.md：联合29/29、SelfCheck、代表回放40/80tick与Play40/Q05关闭PASS。源与矩阵证据分开，未重复运行配置范围明确；不关闭Q06/平台/图像域。
