# C25L state18/19粒子出生与组合顺序原函数证据

VERIFIED_SOURCE_MODEL_ONLY。一个workspace runner链接未修改的正式playable源；1550行=775直接C25L出生端点+775独立完整SimulationTickDriver，重复stdout逐字节一致，12759条检查通过。frame/lifecycle错误0；18条motion/physics诊断仅来自显式pending或无定义动作样例，原JSON完整保留。未运行Unity新代码/新测试/新Play，不把源见证称已修复。

源入口：battle_world.cpp materialize_state18_broken_weapon_particles约3029-3170→spawn_at；simulation_tick_driver.cpp逐slot frame→reaction/armor/rest→OPoint→state18→078提交→weapon pieces→lifecycle。正式EXE与75source身份由build-manifest确认，输出仅Temp/artifacts。

结果：离开18/19请求7粒；持续态先synchronized(0x416A40,4)，非0不生成；positive delay持续态不消费此roll，离开态不受该delay门。缺999/无槽位不消费粒子tuple，持续态选择roll仍遵循前置。每粒四次调用严格为0x4212DE/29、0x4212FE/59、0x421339/11、0x421367/1（bound1仍消费），首次空槽从50开始。

出生：generic HP/MP/bound/base500，max_mp来自metadata700，stats ohp25/omp50没有替代generic初值；owner=-1/group0/facingright，不继承源owner17/group9/left。动作140与latch/previous/collision140，sound latch-1。整数位置保留源整数100/-20/200，精确xyz用源100.25/-20.5/200.75及随机偏移，vx继承源3.25再加随机，vy=-1/vz0；不把两套位置提前合并。真实Unity已有initialRuntimeIntPosition/directPosition字段应复用。

同tick：高于源slot的粒子在当tick经过frame，counter1；已扫过低slot保持counter0。全七type均有见证。此fixture没有hit_a，apply_native_type3_frame_hp_drain明确hit_a<=0不扣血，所有粒子HP保持500；首次验证脚本误假设type3自动扣1，按实际函数纠正验证假设，源/生产均未改。

完整组合：source20时ordinary OPoint OID777占50；state18占51..57；内置15片占58..72；DAT片777占73，然后源删除，108同步调用。source70时低槽分配同前，但内置跳过70并延伸到73，DAT片占74。保存全部子raw及frame参与，不只看对象数量。catalog缺失/无槽/一槽/三槽和父pending/声明与缺失999/1000均有原始分支。

覆盖修正：最初三固定seed只覆盖持续态未选中，原782行保留native-initial-no-selected-seed.jsonl；追加seed0..31后出现144个单粒请求样例，选中seed2/7/12/17/22/24，包含catalog与容量失败。不能以后只复用原三个seed宣称完整7/1覆盖。

Unity当前差异（静态、待RED及修复）：LF2Entity.RunNativeC25State18BrokenWeaponParticles仍BattleRandInt→Match.Rng；SpawnTransitionEffectBranch2使用旧普通OPoint出生/继承身份；SpawnTransitionEffect未显式给task.targetWorld（依赖factory归属），render-backed隔离World需注意。原C25L位置已正确，保留原owner和state13/200其他tail，勿混改。原C25L四测试中的legacy world.Rng计数应随真实RNG修复改为Native observer，不能简单保留旧随机或只改期望。

下一NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001：准确Record后Unity RED，复用现有native generic碎片出生能力和目标World绑定，OPoint→state18→weapon fragments整体同tick验收；补两factory/pool失败/正式OID999以及oldScene恢复关闭。native state18分配用entity(slot)==nullptr，而builtin/DAT fragment用slot_available_for_spawn；融合保留槽的首次候选失败语义不能直接套另一个writer，须先对应Unity实际保留槽。动态global-delay生产者仍按原B8/Q08边界追踪，不因此把delay-clear路径的缺失拖到资源迁移之后。

依赖：父帧事务仍IN_PROGRESS。本次F08具体缺口先闭合；资源post-display缺失已由既有Task记录，不在本次重复源审计，也不能用无post覆盖的fixture为完整C25发证。后继同seed/input/tick联合比较再返回reader/display-post/Q07。Scene/HUDBg x30、非战斗/Unity/GAS/正式资源及schema15/23/26/2/2、raw47/3保持。

正式内容补充：runner可选runtime根，实际加载330正式catalog；仅父fixture888/151与组合777明确覆写，OID999使用正式definition/type5。48出生+48完整driver共96，重复stdout一致、frame/lifecycle/诊断0。formal.jsonl/formal-validation.json/formal-build-manifest独立于原1550文件，正式内容未部署到Unity项目。
