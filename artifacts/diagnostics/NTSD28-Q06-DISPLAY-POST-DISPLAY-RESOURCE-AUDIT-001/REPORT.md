# Q06 显示与post-display资源审计

VERIFIED_AUDIT_ONLY / SOURCE_WITNESS_AVAILABLE / UNITY_IMPLEMENTATION_PENDING，2026-09-14。上一目标轮属于progress：HP事务完成验证并改变下一执行项；本轮完成当前调用链与原函数边界证据，不重做HP/MP。总目标/Q06保持ACTIVE。

## 身份与调用链

正式EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；README_SOURCE声明对应源码，playable scripts/build.ps1引用battle_world.cpp、simulation_tick_driver.cpp、game_session.cpp。新见证构建复核75源码/header清单SHA 07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F，与HP包一致。SOURCE_MODEL_DIAGNOSTIC_ONLY不等于正式EXE交互验收。

原版SimulationTickDriver28::step（simulation_tick_driver.cpp:948-993）逐slot：definition transition→special clones→resource pre-display→display→resource post-display→computer state→frame及后续尾部。BattleWorld28::entity(slot)只查slot存在，不过滤lifecycle pending。资源pre/post各自排除pending/non-type0/missing frame，display仅排除slot不存在，不可共用HP资源资格。

Unity NTSDBattleTickSystem调用现有BattleLateEntityLifecycleModule.Run；该模块:78-155按runtime slot查询active entity，transition/clones→recovery后直接RefreshNativeComputerState/帧推进。无display/post-display生产调用。枚举NTSD28BattlePassOrder里的两个名字不是实际writer证据。既有LF2Entity recovery/ECS恢复仅负责已验pre-display HP→负environment→MP，不能把post直接塞进去让display时点丢失。

## 当前差异及字段合同

| 编号 | 正式行为和依据 | Unity当前事实 / 后继要求 |
|---|---|---|
| DP-01 | battle_world.cpp:2038-2064：score/damage显示值小于累计真值则加各自step，否则赋真值；HP/max显示值大于真值则减各自step，否则赋真值。不在加减当次clamp，越过后下一次纠正。 | 四值/四step已有runtime、Reset/copy/checksum/parity载体，BattleDamageWriter已写step；没有四值推进writer。必须逐tick写四显示值，不改HP/score真值或UI。 |
| DP-02 | spawn_at:1277-1278将display HP/max初始化为request.hp；累计显示和step为默认0。所有type如此。 | Runtime Reset四值全0；LF2Character.Initialize与LF2WeaponBase.InitializeHealth只写Health，没有显示初始值。BattleLogicEntityFactory有Initialize和OID5/52后写HP。完整出生链必须确认最终request hp与post-init覆盖语义后接线，不能在每次Health.HP setter同步或靠首tick掩盖出生差异。 |
| DP-03 | display适用于所有仍占slot对象，即使type!=0、frame缺失或lifecycle pending；native entity(slot)不作active过滤。 | Unity晚循环FindEntityByRuntimeSlotCurrentForLateModule经IsActiveForCurrentPass排除pendingUnregister、OidMergeDormant、PendingFlushDestroy。须核对各Unity状态与native slot存在关系，只局部补display可见边界，不能解除整个C25活动门或把其他tail重新作用于已停对象。 |
| DP-04 | post入口保存current frame指针；有效type0/非pending/current frame存在才进入。bmp.frame_0mp>0、HP>0、effectiveMax<=0且当前action不落112-114/130-144/180-192/200-206/220-231时只写action，不立即执行entry reset。 | Q05 metadata/frame载体已可读，但无对应producer。后继需查Runtime.Frame/Frame.N/action latch当前适配，不能调用带额外entry副作用的通用转帧。 |
| DP-05 | 用previous_action_078对应frame state，62→HP1；63→HP/max0；64→MP0；65→HP/max=base；66→MP500；405→stage整数中心、保持整数Y、精确xyz同步并motion={}；4000..4999→lives=state-4000；3640..3645→group=state-3640。 | Frame.Prev为078候选且C25尾已有commit；HP/HPBound/PP、HP2Orig(lives)、RelationTeam已有独立语义载体。RespawnCount是reviveNextHp，严禁错接。未找到本post事务；stage405需复用现有场地合同并遵守多边形边界例外，不新增默认stage.dat或借机改相机。 |
| DP-06 | ordinary_credit_gate==-1、保存的current frame.state!=63、timer1B0>0或mode18=2/3：HP/max=base、累计自伤及KO=0。注意frame_0mp改action后仍用原frame.state。 | timer/gate/累计/KO载体已在；GameSession:4165把config.selected_mode_hit_group_gate_18传给资源规则，game_session.h:327默认0。后继用不可变raw mode参数，正式选中记录投影仍归Q08；不得把helper mode0视为所有模式。 |
| DP-07 | 完整恢复后依次：HP>base则HP和max一起=base；max>base再max=base；stats.max_mp>0且MP>=max_mp则MP=max_mp。没有HP/MP下限clamp，也不把HP强制压到effectiveMax。 | 没有共同post限幅owner；Health属性本身直接写值。HP段故意不限幅与本尾部是两个顺序明确的事务，不能把限幅塞进HP setter。 |
| DP-08 | display先读取pre-resource结果，再执行previous-state/fullRestore/clamp，随后frame。 | 新源组合向量保留display与实际值不同，后继需同slot边界和真实Play断言，不能只比最终HP。已有HP/MP测试保持，不能因新增post限幅而删其独立段不限幅断言。 |

原版display可读字段虽归nativeResourceDisplay域，但没有授权改普通HUD、头顶血条例外或通用表现框架。当前显示载体已进入13/21/24/2/2，无新增字段时不升版本。所有实际初始化、clone/copy与实体回收副作用须写入实施Record。

## 原函数新鲜见证

由独立NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001的一新C++ runner产生：display.tsv 980行、post.tsv 2379行。分别重复运行，直接stdout逐字节一致，列完整；构建和两个运行exit0。native-build-manifest.json、validation.json、scope-validation.json保留identity/hash和实例。见证只定义source函数输出，没有Unity新运行比较，也没有正式EXE物理键输入证据。

已测实例：

- value9/target10/step4，累计显示第一次13，第二次10；HP value11/target10/step4，第一次7，第二次10。
- 所有7种type出生HP137，两个HP显示初值均137。pending和missing frame不阻止原display函数。
- hp501/bound0/base500/mp101/max_mp100：display为501/0，post后HP500/max500/MP100。证明display读取在限幅前。
- current action0 state0、frame_0mp900且frame900.state63、HP1/max0/timer1：action写900，随后仍按原state0完整恢复至500，累计自伤/KO清0。若提前取新frame.state则错误跳过恢复。
- previous405、stage width101/zNear-9/zFar10：位置中心50/0，整数Y-7保持并将preciseY -7.5同步为-7。motion={}静态已读，但本夹具未注入非零motion，不能宣称非零速度清除已测。

矩阵覆盖当前/previous state的关键分支、mode/gate/timer、保护action区间端点、HP/MP限制和无下限语义、type/pending/frame资格。显示两对字段使用相同输入，不足以排除Unity未来误接同方向字段；实施focused必须另加四组不同目标和步长。未测signed int溢出，不由C++ UB推断新规则。

## 执行拆分与回访

下一唯一Task：NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001。目标是完整C25d显示owner，涵盖出生初值、四字段递推和slot可见性；准确脚本清单必须在查完生产出生/复用/clone/恢复所有owner后写Record，不能直接只加一个普通active-character helper并宣称完整。先源见证→Unity RED→实现→字段独立/边界/回放/相关HP-MP回归→SelfCheck→真实Scene注入/恢复/关闭。没有新的全局manager、pool或关闭阶段。

随后NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001接完整post事务。不可把未审定stage adapter/mode投影或行动写入时点隐藏成默认值。mode选中记录由Q08交付，基础不可变合同可先验证；生命周期、CPoint/OPoint/+2F8、revival和pieces仍在Q06。

R05（实际pass边界）、R07（资源/显示/限幅）在生产接通时触发，当前source审计只提供前置证据，不将其置VERIFIED；R07前HP/MP子条件PARTIAL_RETURN保持。出生改动还须R09/R16，Q07数据资源部署仍未做。本轮新增单一诊断脚本，不改Unity生产/资源/Scene，前HP六脚本hash和用户Scene bcd1047b…保持。未运行新Unity tests/SelfCheck/Play；上一轮134/自检/Play仅为已验基线。

最终账本验证：Tools/Validate-ChangeLedger.ps1 PASS，511 Records、11 governed code diff已覆盖；历史Record非当前diff的WARNING保留。日志 artifacts/diagnostics/NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001/ledger-final.txt。
