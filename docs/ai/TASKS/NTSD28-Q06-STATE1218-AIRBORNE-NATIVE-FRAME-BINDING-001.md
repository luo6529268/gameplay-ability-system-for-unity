# NTSD28-Q06-STATE1218-AIRBORNE-NATIVE-FRAME-BINDING-001

IN_PROGRESS / SOURCE_WITNESS_FIRST。父contact480的24个airborne control中有18项即时descriptor差异（before0），独立于soft/hard contact两个caller。当前正式PhysicsIntegrator28 state12 airborne按照postgravity Vy<-8/<1/<8选择180..183/186..189；action185/191不选；负environment仅前侧family按Vy<12与upcomingPhase>=6选182/181；state18 action<205且postVy>1选205。源BattleWorld28::step_physics覆盖context phase为(resourcePhase+1)%12。

第一写域仅Tools/NTSD28AuthorityTrace/state1218_airborne_native_frame_witness.cpp，实际world.step_physics+following driver，保留contact/ordinary源runner。按阈值上下与等号、action family边界、负environment/phase5→6及11→0、目标声明/隐式、无action控制和floor0/-10组合有界采样，避免冗余笛卡尔爆炸；DAT真实当前frame与目标相同需避免重复覆盖。输出before/after/following raw/descriptor/B2/nativeRNG/extra environment与phase、source-only previousXYZ。phase只能通过已有正式World接口或同源state恢复构造，不改权威源码。

确认source后新增精确Unityfixture及唯一LF2Entity.ApplyCurrentDatType0AirborneAction单binder路径，当前生产未授权本Record改写。不能改已闭selector/重力/phase顺序，只核对descriptor；不能修改共有raw setter或前包contact/ordinary行为。既有raw3映射缺口与previousXYZ边界不掩盖，environment额外字段比较须显式恢复。

验收：source双跑+独立oracle、Unity RED→最小修复→compile/回归/SelfCheck/replay/Play/有序关闭，父contact保留全部控制后联合验收。正式330动态可达尚未证明，synthetic witness不能宣称真实玩家已触发。资源/Scene/非战斗/GAS/schema保持；回滚仅本ID声明差异且保留其他工作。Q06未完成/Q07未迁移。

最终VERIFIED_SCOPED：出口见同ID Record及ACCEPTANCE.md。按用户等价类收敛验收，代表回放/Play40；原计划全乘积已由后继明确修订替代。
