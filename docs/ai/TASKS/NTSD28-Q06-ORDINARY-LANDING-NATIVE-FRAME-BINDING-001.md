# NTSD28-Q06-ORDINARY-LANDING-NATIVE-FRAME-BINDING-001

PLANNED / SOURCE_WITNESS_FIRST。当前source SimulationTickDriver28→BattleWorld28::step_physics(1798)→PhysicsIntegrator28普通type0落地：strict crossing先clamp floor/Vy0/post-friction Vx除3，state100优先94，action212或state6优先215，否则非0hit_g或219；动作写入后counter0。Unity已闭ApplyCurrentDatType0OrdinaryLanding行为仍调用旧raw binder。只补native descriptor/cache读帧，不重做B4 ordinary landing或Q06 canonical physics-tail接线。

formal330 type0静态hit_g候选（排除12/18/100/6和212）2343显式/7隐式，例OID870/802/721/508/88/89 frame701→1、OID413 frame420→422。统计不证明实际落地可达；原草稿误排state5已纠正且保留，计数恰好不变。

第一写域仅新Tools/NTSD28AuthorityTrace/ordinary_landing_native_frame_witness.cpp。实际world.step_physics或完整driver源捕获，覆盖上述1/422、隐式0..998/高位/无效、94/215/219优先级及声明/隐式、接触与非接触控制；完整raw/descriptor/counter/latch/Prev/snapshot/速度/RNG/后继tick。先source双跑与独立模型后新增准确Unityfixture和单生产符号清单，当前不改production。保护已闭state12/18、非角色物理、边界、音效与schema。

验收compile/focused/SelfCheck、sameWorldreplay、真实Play两factory/关闭及authority全调用链。不得为通过测试改变source、物理规则或既有例外；不改Scene/resources/非战斗/GAS/Server。回滚仅同ID精确diff并保留失败及其它工作。Q06仍active，Q07不得提前部署。

IN_PROGRESS / RED_CONFIRMED。186×四组before0/即时162/后继132；允许按同ID Record扩充的LF2Entity.ApplyCurrentDatType0OrdinaryLanding单caller native绑定修正，后继collision reference差异另追踪；不关闭父任务。

最终VERIFIED_SCOPED：同ID Record与普通落地ACCEPTANCE.md为出口证据；不关闭Q06/平台域。
