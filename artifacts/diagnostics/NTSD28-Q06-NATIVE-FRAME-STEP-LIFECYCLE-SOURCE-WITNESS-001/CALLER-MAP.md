# 当前帧事务真实调用链与修复边界

2026-09-14，源码观察与2676原函数输入见证。SOURCE_MODEL_DIAGNOSTIC_ONLY；Unity生产未改，未执行Unity同输入逐字段对比，不能把以下静态差异当作已修或完整场景first-difference。

## 正式执行顺序

GameSession28::step配置options.frame.next_999_policy=return_to_zero（game_session.cpp:4126）。SimulationTickDriver28::step在升序slot C25尾部执行：resources pre → display → resources post → computer → step_frame_slot → reaction/armor/rest → frame-zero OPoint → state18 particles → previous_action_078=current action → weapon pieces → resolve_pending_lifecycle → 存活后的healing。对应simulation_tick_driver.cpp:983-1071，均进入playable构建闭包。

BattleWorld28::step_frames_range（8543+）入口：不存在/pending跳过；interaction_state<0保持；motion_hold_timer非0且type!=3保持；slot<20、type0、HP<=0、state14、lives<=1、queuedHp<=0保持。之后type3 drain → 读之前action_latch的state → 帧声音 → FrameMachine28::step → action212 side effect → 正raw999的实体resolver → state14 blink → 目标声音 → 自动负成本 → action<0或>=999标terminal → defend cooldown。

FrameMachine只读真实DAT wait/next，wait<0返回error；action!=latch才清counter并提交latch；++counter后<=wait保持，否则counter=0；next0原地保持；负next翻面后取绝对值；abs999基础解析0；>=1000写action但不改latch；合法目标写action/latch。实体resolver仅raw_next==+999再判断type0且integerY非0且与collision_y_reference不同→212，否则0。故-999没有airborne212二次转换。212跳跃初始化仅advanced且raw!=999；212→212 stayed不能再次初始化跳跃速度。

自动成本：local gate、raw next非0、advanced/stayed、destination0..998准入。waiver优先，stats.recmp正值优先于selected-mode+30，再signed整数百分比和double。仅负MP/HP参与；MP先、HP后；不足用原destination.next原样写action及latch，HP仍读原destination；HP/MP相等可支付，HP支付还减effectiveMax(cost/3)，各自真实consumed totals累加。成本fallback的999/-999不再规范化，统一terminal后继处理。

resolve_pending_lifecycles_range（8299+）先处理1100..1299：runtime_state=1100-code、action=0、collision tick_action_snapshot=0、pending/code清零，保留action_latch/counter。其余code<0或>=999 despawn。driver中previous078已先提交，不能用本runner直调两个函数时保留的078=45推广到完整driver。

## Unity实际所有权

| 真实入口/方法 | 当前已读行为 | 必须处理的差异或约束 |
| --- | --- | --- |
| BattleLateEntityLifecycleModule.Execute slot循环（约150-220） | BeginNativeC25FrameTick包住optimized/virtual两入口；随后reaction/armor/rest；当前HandleFrameTickExit早于OPoint/particles/Prev mirror | 生命周期分支排序应对照完整driver，终止门不能仅改857数字；terminal跳过OPoint但mirror/encoded reset碰撞帧合同不能丢 |
| BattleEcsCharacterFrameTickPass.ExecuteExactCharacter | exact LF2Character调用RunNativeC25FrameBodyForWorldPass | 与legacy路径共享一个事务，不能优化路径修了兼容路径漏掉 |
| LF2Character/WeaponBase/SpecialAttack.SimFrameTick；Other frameModule | 均走RunCommonFrameTick→同一native body；FrameTransistor.Trans也委托该body | 未知derived虚入口保留所有权，不能根据类名跳过 |
| LF2Entity.RunNativeC25FrameBodyForWorldPass（6427+） | whole-body cpoint-kind2返回；旧state0空中强写212；正999先分支、负值else；next后857门；trans current/latch相关 | native原函数cpoint orphan仍推进（kind2仅抑制type3 drain）；状态与入口可达性须完整driver补证。负999、YReference、212 stayed和wait/latch必须原子匹配 |
| LF2Entity.ApplyCommonFrameTickPpDisplayPostAdvance（5573+） | 用PP<负mp比较，fallback=hit_d，mp>=0直接返回，没有负HP部分，旧转向hit_d分支 | 与原版负成本事务不一致；不能局部修不等式而保留hit_d与遗漏HP。已有Runtime Input*Cost/Consumed、CollisionYReference，先复用，不加重复字段 |
| BattleCharacterActionWriter.AdjustNativeMpCost（1650+） | 既有输入成本倍率/helper为private；读取recmp/mode/double/waiver | 自动负成本与输入正成本资格不同，可复用纯调整部分但不可重用输入fallback/可支付HP判定；不要复制第二份倍率逻辑 |
| LF2Entity.SetFrameTickDirect/RawDirect/DirectWriteRaw/Held（4520+/6378+/6590+） | 使用legacy cache；trans wait/next写入保留或重置counter的用途不同 | 精确分辨currentaction、action_latch、真实counter，迁移Native查询不等于统一重置。snapshot Native绑定已完成，不重做 |
| FrameTransistor.SetWait/SyncDirectFrameData（Character目录） | 将负wait夹到0，持有请求wait/next覆盖 | source负wait返回error；现有catch/damage/input writers应按生产可达性审计，不能无条件每tick覆写或全部删除 |
| LF2Entity.OnFrameTickFrameChangedFromWaitCounter | 旧857限制，仅单sound，在当前action!=latch时播放 | source独立sound_action_latch且每帧最多20条，在head与destination/cost前取样；搜索未找到对应runtime sound latch载体，必要新字段须独立数据合同及copy/reset/checksum/replay审计，不能偷塞无快照字段 |
| HandleFrameTickExit/MirrorLatePrevFrame | encoded reset只写HitStun/Frame0；通用857释放；负link保护 | native编码重置同时collision snapshot=0，latch保留（普通next与成本fallback值不同），且previous078按driver先提交；不能用单个current帧字段代替所有镜像 |

## 已测关键对照

详见validation.json。缺失998维持action998且存活；声明999/next0仍terminal；+999/type0/Y=-10/Yref0→212保留速度，-999→0并翻面；+999/Yref=-10→0。原MP8/HP6、目标MP-9/HP-6/next999→MP8、HP0、max198、HP消费6、action999删除，声音数2。MP9/HP5/同成本/next1101→MP0、HP5、编码重置action0但latch1101、collisionFrame0。MP9/HP6恰好够→action7存活、HP/MP0、max198。孤立cpoint-kind2/type0/next1000仍删除；已在212再次next212维持原速度。

## 下一执行决策

原函数见证前置已满足，下一NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001先把上述真实入口组成准确修改Record，并用2676矩阵中相同输入做Unity RED。不能只改旧857边界，不能先把自动成本fallback999留在存活实体上，也不能把next语义与terminal拆成临时不一致的生产状态。

该Task需要同时明确声音latch缺失载体和完整driver structural tail的出口；若声音需单独数据迁移，先建立其数据合同，不以“本包不含声音”宣称frame事务完整。没有新增mutable字段则不默认改13/21/24/2/2；需要字段时按联合snapshot/checksum/replay规则确定版本，禁止无证据冻结旧schema或机械升版本。

保留Unity/GAS/非战斗/Scene/资源及用户例外；当前角色内容仍Unity旧138；Q07未部署。所有非live旧reader迁移保持独立，不为清grep扩大本包。frames驱动修复后回display其余出生/post；完整Q06/总目标未完。
