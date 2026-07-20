# C# 权威战斗台账：输入、帧推进、物理与实体 Runtime

日期：2026-07-18
唯一来源：`J:\QQFile\NTSD2.4\ntsd_release_C#\src`
审计边界：只读上述权威 `src`；未读取、未引用任何其他工程；未修改权威源码。

## 0. 结论与使用规则

### 勘误说明（2026-07-18）

本 ledger 曾将 `InputRuntime.cs:926-934 DoFrameJump` 误读为清零八个输入 Cd。按权威真实源码复核后，成功路径只清 `CdRight/CdLeft/CdUp/CdDown/CdAttack/CdJump/CdDefend` 七个普通 Cd，不写 `CdDefendLock`；下文 `IN.JUMP.03` 与字段拥有者说明已据此纠正。此勘误只修正 ledger 的权威建模，不改变 237-ID 集合。

本台账冻结以下权威事实：

1. 输入在 `SimulationTickDriver.StepOneTick` 进入，先调用 provider 并执行 `PollHumanInput`，随后才进入 `GameTick.Run`。
2. `GameTick.Run` 的相关顺序是：全局 tick/phase -> cooldown/step gate -> 角色输入 -> 非角色 `hit_fa` frame logic -> 清除所有实体当前按键 -> frame advance/Physics -> 交互 -> late per-entity `FrameTick` -> opoint -> `PrevFrame`。
3. 人类输入在 `PollHumanInput` 内执行 `RollFromCurrent -> 写 Key -> TickCooldowns -> ApplyEdges`；AI 路径执行 `RollAndClearAiKeys -> 决策 -> ApplyEdges`，没有独立调用 `TickCooldowns`。
4. 输入边沿字段名是交叉语义：Attack 边沿写 `CdDefend`，Defend 边沿写 `CdJump`，Jump 边沿写 `CdAttack`。这是权威行为，不能按名称“纠正”。
5. frame logic 与 frame tick 不是同一阶段：`FrameAdvance.FrameLogic` 在 frame advance 之前；`FrameTick.Tick` 在交互后 late pass 执行。
6. `NtsdEntityRuntime.CopyFrom(NtsdEntityRuntime)` / `Clone()` 故意不复制 `Transient`；runtime 匹配与 checksum 也排除 `Transient`。实体 `CopyFrom(Entity)` / `ApplyTo(Entity)` 则包含 transient carrier。
7. 本文 ID 是唯一核销键。`A.B.C` 的后缀是同一方法内互斥或有序分支，不表示可交换顺序。

## 1. 权威调用框架

| ID | 文件/方法/行 | 权威顺序、条件与写入 |
|---|---|---|
| FLOW.01 | `BattleCore/Simulation/SimulationTickDriver.cs:25 StepOneTick` | `tickIndex=GameTick+1`; gate 不满足即返回；provider `BeforeSimTick -> GetFrameInput`; tick 不匹配时替换为 `Empty`; `ApplyFrameInput`; scheduler；可选 checksum；provider `AfterSimTick`。 |
| FLOW.02 | `SimulationTickDriver.cs:82 CanAdvanceTick` | 非 `LockstepBuffered` 且不要求 ready 时直接 true；否则询问 provider。 |
| FLOW.03 | `SimulationTickDriver.cs:93 ApplyFrameInput` | 按 Players 顺序；非法 battle player slot、空/非 active/AI 实体跳过；buttons -> `NtsdInputState` -> `PollHumanInput`。重复 player slot 会按列表顺序重复轮转输入。 |
| FLOW.04 | `BattleCore/Entity/CharacterLogic.cs:9 ApplyInput` | 先 `SyncRuntimeFromLegacy(entity)`；AI 调 `AiInputRuntime.PrepareBasic`；非 AI 且未外部 poll 时 poll 全 false；再 `ApplyCharacterInput`; 最后 `ApplyLegacyFromRuntime`。 |
| FLOW.05 | `BattleCore/Simulation/GameTick.cs:18 Run` | `GameTick++`, `InputPhase=(+1)&1`, `FrameMod12`, `FrameToggle`; 清瞬时 world 标志；results active 时只 results tick 并返回。 |
| FLOW.06 | `GameTick.cs:48-69` | `RunCooldownsTick`（ARest/AttackExempt，不是 input cooldown）；step gate；可选 postCooldown 回调；OID51/52 maintenance；NeedClearInput 时 reset 角色输入并整 tick 返回；`GameTick>1` 才 ApplyCharacterInputPass。 |
| FLOW.07 | `GameTick.cs:72-93` | early state passes；仅 active、非角色 DAT、当前 frame `HitFa>0` 的实体执行 FrameLogic；随后对所有 active 实体先清 action/direction Key，再 DispatchFrameAdvance。 |
| FLOW.08 | `GameTick.cs:95-112` | post-frame state、Z clamp、cpoint、held sync/link、snapshot `PrevFrame2`、collect、character/object hit resolve。 |
| FLOW.09 | `GameTick.cs:127-134,1513 RunLatePerEntityUpdatePass` | post process -> late per entity；late 内 `RegeneratePreCollisionStats -> FrameTick.Tick -> death/frame range -> ProcessOpointSpawn -> broken weapon -> N30 history -> transition effects -> PrevFrame=Frame`。 |
| FLOW.10 | `BattleCore/Frame/FrameRuntimePasses.cs:12-29` | DispatchFrameLogic/Advance 经 `EntityDispatch`/category；所有 category 默认最终调用 `FrameAdvance.FrameLogic/Advance`。Character 没有额外 frame 分支。 |

### 1.1 输入载体

| ID | 文件/方法 | 契约 |
|---|---|---|
| CARRIER.01 | `Lockstep/SimulationPlayerInput.cs:5-20` | byte flags：Right=1, Left=2, Up=4, Down=8, Attack=16, Jump=32, Defend=64。 |
| CARRIER.02 | `Lockstep/SimulationFrameInput.cs:5-13` | `TickIndex`; Players 默认为空；`Empty(tick)` 只写 tick。 |
| CARRIER.03 | `Input/NtsdInputState.cs:16` | 对七个 flag 分别 `HasFlag`，无优先级或互斥化。 |
| CARRIER.04 | `Input/SimInputBuffer.cs:11-29` | dictionary 以 tick 覆盖写；TryGet；ClearBefore 删除严格小于 tick；Reset 清空。当前 `src` 无生产构造/调用者，属预留未接线。 |
| CARRIER.05 | `Lockstep/ISimulationFrameInputProvider.cs:5-18` | local provider 永远 ready，返回 Empty。 |
| CARRIER.06 | `Host/HostSimulationFrameInputProvider.cs:29-53` | 每 tick 读取 pressed set，固定生成 player 0/1 两项；每项独立 OR 七个 flag。 |

## 2. InputRuntime 逐方法/逐分支台账

### 2.1 人类输入、边沿、冷却与历史

| ID | 文件/方法/行 | 分支、顺序、字段与常量 |
|---|---|---|
| IN.HUMAN.01 | `Input/InputRuntime.cs:609 PollHumanInput` | 无 early return。`RollFromCurrent`; 七个 bool 写七个 `Key*` byte；`TickInputCooldowns`; `ApplyInputEdges`。 |
| IN.CD.01 | `InputRuntime.cs:624/629 TickInputCooldowns` | Entity overload 只转发 Input；Input overload 调 `TickCooldowns`。 |
| IN.CD.02 | `Runtime/NtsdEntityRuntime.cs:619 TickCooldowns` | 依次 Right,Left,Up,Down,Jump,Attack,Defend,DefendLock；每个 `>0` 减 1，无下溢。 |
| IN.EDGE.01 | `InputRuntime.cs:2563/2568 ApplyInputEdges` | 两 overload 只转发 `Input.ApplyEdges`。 |
| IN.EDGE.02R | `NtsdEntityRuntime.cs:576` | `PrevRight=0 && KeyRight=1`: `CdRight=5`, history code 6。 |
| IN.EDGE.02L | `NtsdEntityRuntime.cs:582` | Left: `CdLeft=5`, code 4。 |
| IN.EDGE.02U | `NtsdEntityRuntime.cs:588` | Up: `CdUp=5`, code 8。 |
| IN.EDGE.02D | `NtsdEntityRuntime.cs:594` | Down: `CdDown=5`, code 2。 |
| IN.EDGE.02A | `NtsdEntityRuntime.cs:600` | Attack: **`CdDefend=5`**, code 9。 |
| IN.EDGE.02DEF | `NtsdEntityRuntime.cs:606` | Defend: **`CdJump=5`**, code 0。 |
| IN.EDGE.02J | `NtsdEntityRuntime.cs:612` | Jump: **`CdAttack=5`**, code 5。 |
| IN.HIST.01 | `NtsdEntityRuntime.cs:550 PushInputHistory` | `[1]=[2], [2]=[3], [3]=[4], [4]=[5], [5]=new`; `[0]` 永不滚动。 |
| IN.HIST.02 | `NtsdEntityRuntime.cs:559/564/569` | `[0]` 是 history gate；Clear tail 仅清 1..5；Has gate 只读 `[0]!=0`。 |
| IN.HIST.03 | `GameTick.cs:1676 RunN30InputTrigger` | 仅 slot<10 active living character；读取 history[2..5]：9,0,9,0=>100；9x4=>102；9,5,9,5=>104；命中后先清 tail，再生成 OID998；100 写同队 `Unk3FC/Unk400` 随机坐标；102 gate=true；104 gate=false。 |

### 2.2 组合技与角色输入总入口

| ID | 方法/行 | 权威分支 |
|---|---|---|
| IN.APPLY.00 | `InputRuntime.cs:634 ApplyCharacterInput` | CharData/null frame 早退；取进入时 state；先 RunComboWrappers；frame null 早退。 |
| IN.APPLY.01A | `:649` | `HitA!=0` 且 CdAttack 严格大于 Defend/Jump：DoFrameJump(HitA)，无论 jump 成败都 `CdAttack=0`。 |
| IN.APPLY.01D | `:654` | 否则 HitD + CdDefend 严格最大：jump，`CdDefend=0`。 |
| IN.APPLY.01J | `:659` | 否则 HitJ + CdJump 严格最大：jump，`CdJump=0`。平局不触发。 |
| IN.APPLY.02 | `:665` | frame 110：Right 置 Facing=0；随后 Left 可覆盖为 1。 |
| IN.APPLY.03 | `:678` | state 301/19 且地面：Up-only/Down-only 直接写 RunningSpeedZ。 |
| IN.APPLY.04 | `:691` | LinkState=2 且 state 0/1：HeavyWalkRun -> velocity tail -> return。 |
| IN.APPLY.05 | `:698` | frame 215：landing input -> velocity tail -> return。 |
| IN.APPLY.06 | `:705` | frame 182/188 + WeaponCount>=0 + Defend held + CdJump>0 + alive：RecoveryJump -> tail -> return。 |
| IN.APPLY.07 | `:716` | state 0/1 WalkRun 后 StandingActions；2 Running；4 仅 YInt<0 时 Jumping；5 Dash；最后 velocity tail。 |
| IN.COMBO.01 | `:740 ComboInterrupt` | wrapper 未 advance：任一 Cd==5 中断；已 advance 时排除当前 mode 对应键。mode d 排除 defend；j 排除 jump；a 排除 attack。 |
| IN.COMBO.02 | `:768 AdvanceCombo` | state0 + CdDefend==5 ->1；state1 + step2Cd==5 ->2，否则按 d 中断；state2 + step3Cd==5 ->3，否则按 step2 mode 中断。同一次调用可连续跨多步。 |
| IN.COMBO.03 | `:804 RunCombo` | state==3 且 targetFrame!=0 且 LinkState!=2：jump；可写 facing；combo=0；返回 true。否则 finalMode 中断可清零。 |
| IN.COMBO.04 | `:824-849` | 顺序固定：DRA,DLA,DUA,DDA,DRJ,DLJ,DUJ,DDJ；每个成功后重取 frame。映射数据：HitFa,HitFa,HitUa,HitDa,HitFj,HitFj,HitUj,HitDj；左右 final 写 facing 0/1。 |
| IN.COMBO.05 | `:851-881` | DJA 单独 advance；frame null 或 state!=3 直接 return，导致前八个局部 combo **尚未写回实体**；OID6+HitJa300+Hp>177+global guard0 再 return，同样不写回；HitJa!=0、Unk324==-1、LinkState!=2 时 jump/清 DJA/return；Unk328==1 写 Unk338=0 后 return；否则按 attack interrupt。 |
| IN.COMBO.06 | `:883-891` | 只有走到尾部才把九个局部 combo 全量写回。这是早退敏感契约。 |
| IN.JUMP.01 | `:894 DoFrameJump` | target<0 取反并标 flip；999=>0；CharData/HasFrame/frame null 失败。 |
| IN.JUMP.02 | `:912` | ppMode：cost=`Mp%1000`; Pp不足失败；hpCost=`Mp/1000*10`; `Hp<=hpCost` 失败；成功扣 Hp/Pp，加 ComboCountVic，Spend display。非 ppMode 不检查/扣费。 |
| IN.JUMP.03 | `:926` | 成功写 Frame；仅 `flip && ppMode` 翻面；清零 `CdRight/CdLeft/CdUp/CdDown/CdAttack/CdJump/CdDefend` 七个普通 Cd，`CdDefendLock` 不变；true。 |

### 2.3 角色移动/动作分支

| ID | 方法/行 | 权威分支与常量 |
|---|---|---|
| MOVE.STAND.01 | `:935 ApplyStandingActions` | CharData null 退。Jump held + CdAttack>0：AnimSub/Attacking=0；Link 0: HitConfirm>0=>70，否则 60/65 随机；ppMode 用目标 frame.Mp 扣 Pp，负则钳 0；Link101 无方向=>20/25 随机，有方向45；`%100==1`=>20/25；4=>45；6=>55。 |
| MOVE.STAND.02 | `:984` | Defend held + CdJump>0 => frame210, clear attack/anim。 |
| MOVE.STAND.03 | `:991` | Attack held + CdDefendLock==0 + CdDefend>0 => frame110, clear attack/anim。 |
| MOVE.WALK.01 | `:999 ApplyWalkRun` | rate=max(1,int WalkingFrameRate); AnimSub 每 tick 向0；Right-only/Left-only地面：翻面、6 phase walk frame 5..8..、Vx=±ws；对应 Prev==0 时 AnimSub ±10，累计越过 ±11 进入 frame9 并清计数。 |
| MOVE.WALK.02 | `:1056` | Up-only/Down-only地面：若未横移同样推进 walk anim；Vz=±wsz；Vx*=5/7。上下是两个独立 if，但互斥条件。 |
| MOVE.RUN.01 | `:1081 ApplyRunning` | LinkState2：heavy speed/rate，frame16..18/17；逆向键 frame19；深度移动 Vx*=5/6；Jump+CdAttack>0=>50；return。 |
| MOVE.RUN.02 | `:1123` | 普通 run frame9..11/10；Vx=±RunningSpeed；逆向键 frame218；深度 Vx*=5/6。 |
| MOVE.RUN.03 | `:1155` | Jump+CdAttack>0：Link0=>85（ppMode 仅 Pp足够才扣费/进入）；link `%100==1`=>方向?45:35；4=>45；6=>方向?45:55。 |
| MOVE.RUN.04 | `:1190` | Attack+CdDefend>0=>102。Defend+CdJump>0=>sound17/frame213/AnimSub0；Vx=朝向*DashDistance, Vy=DashHeight, Vz按上下。 |
| MOVE.JUMP.01 | `:1208 ApplyJumping` | YInt>=0 或无 CharData 退；左右 only 翻面；未按 Jump 退。Link0=>clear attack/frame80，ppMode 无“足够”门，直接扣，负钳0；link `%100==1`=>方向?52:30；4/6=>52。 |
| MOVE.DASH.01 | `:1248 ApplyDash` | 左右 only 翻面；按 facing/Vx/frame 选择213/214，216/217是保护帧。 |
| MOVE.DASH.02 | `:1276` | 仅朝前 dash 且 Jump：Link0=>frame90（ppMode 要 Pp足够）；`%100==1`=>40, Vy-=1, clear attack；4/6 且有方向=>52, Vy-=1, clear attack。 |
| MOVE.HEAVY.01 | `:1316 ApplyHeavyWalkRun` | heavy walk speed；AnimSub向0；Frame<12=>12；左右 only 为 12..15.. frame 并可双击到16；上下 only 使用 Vz=±heavyZ、Vx*=5/7；Jump+CdAttack>0=>50。 |
| MOVE.LAND215.01 | `:1400` | Attack+CdDefend>0=>102。Defend held 且右/正Vx或左/负Vx、CdJump>0：sound17，帧由 facing 算，写 dash V；函数尾再次按上下写 Vz。注意右分支写正 DashDistance，左分支写负。 |
| MOVE.RECOVER.01 | `:1442` | backward 由 facing/Vx 判定；frame100/108；clear attack；Vy 若大于 RowingHeight 则钳为该负值；abs(Vx)<1 时按 facing 写 `1?-rd:-rd`，否则保留速度符号写 ±rd。 |
| MOVE.VTAIL.01 | `:1461` | 当前 frame null 退。Dvx>500=>`Dvx-550`; 正/负 Dvx 按 facing 只在越过目标速度时覆盖。Dvy>500=>`-550`，否则累加。Dvz>500=>`-550`；否则 Up 且 CdUp>=CdDown 写负，Down 且 CdDown>=CdUp 后执行可覆盖写正。 |
| MOVE.HASDIR.01 | `:1515` | 任一方向 Key 非0。 |

## 3. AI 输入逐方法/逐分支台账

### 3.1 PrepareAiInputBasic 主流程

| ID | 行 | 分支与副作用 |
|---|---|---|
| AI.PREP.00 | `InputRuntime.cs:14` | CharData null 或 Hp<=0 直接返回，**不 roll、不 edges**。 |
| AI.PREP.01 | `:19` | `Unk3FC>-1000`：roll/clear，MoveTowardCoordinate，ApplyEdges，return。 |
| AI.PREP.02 | `:27-45` | difficulty；AiPhaseGate==1 强制0；否则 InputPhase1 且 team!=5 时 slot<20 或 oid<30 强制0；负值也钳0。写 world AiDifficulty/Rand3/5/15/20、MoveMode0，scan，StageTargetX=override或bg width。 |
| AI.PREP.03 | `:47-68` | 找最近目标；缓存 Unk360 合法 active living character 且 Rand%30>0 时沿用，否则更新为新目标。该缓存随机调用顺序不可挪。 |
| AI.PREP.04 | `:70` | 无目标：roll/clear；用旧 saved slot 构造 fallback；ApplyEdges；return。 |
| AI.PREP.05 | `:94-141` | phase1/4 且 team!=5 计算 7A ground/guard；按自身 Hp、同队最低 Hp、同队数修正。KillCount>-1 同时启用7A/7B；Pp>250、特定 phase/team/slot 启7B。 |
| AI.PREP.06 | `:143-248` | 扫描 slots 20+：C8 frame group 6/5 threat；D3 state18 或 D4 frame150..170 写避让方向；符合 oid/state/距离/history gate 的特殊候选可替换 selected；C8 group5低血压和 7A ground 可后选。 |
| AI.PREP.07 | `:250-254` | 只要见过 C8 threat，恢复 special scan 前目标；写 Unk360；roll/clear；解析 self/target state/OID。 |
| AI.PREP.08 | `:260-278` | 朝向不对时写转向键；self state2 强制同 facing 方向键；阻挡且 Rand%(AiRand5+8)==0 时 `PrevJump=0,KeyJump=1`。 |
| AI.PREP.09 | `:280` | target state3000 helper 返回 true 时 edges+return。 |
| AI.PREP.10 | `:286` | history gate + positive link；TargetIdx 指向 OID7A/7B 时用实体 alias 写 PrevJump0/KeyJump1，edges+return。 |
| AI.PREP.11 | `:303-351` | gate 时检查 coordinate；target state1004/2004：远且非7A/7B则只 edges return；否则以 X阈值6/250/100、Z阈值3、AiMoveMode 控制接近；完全近距触发 jump；edges return。 |
| AI.PREP.12 | `:354-410` | target state14 或 abs(YInt)>2：临近右边界30向左、左边界30向右并清 Prev；若 Z<=45 **或** X<=350 执行远离 X 和 Z/边界动作；所有路径 edges return。 |
| AI.PREP.13 | `:412-437` | history gate 且远，或 target 非14且近地，允许 C8 分支；C8 用 X阈值7、Z阈值2跟随；edges return。 |
| AI.PREP.14 | `:439-476` | Rand%(AiRand5+1)==0 才按固定顺序尝试 First、TeammateGuard、Oid1Combo、CloseOid1、Oid4、Oid5；任一 true 都 edges+return。 |
| AI.PREP.15 | `:478-494` | 无外层随机门，顺序尝试 Oid33/19/16、Oid52/1/2/21、Oid51/2/18/7；true 即 edges+return。 |
| AI.PREP.16 | `:496-567` | closeOrFree = 无 gate 或距离在 Z150/X240；widePath 为 OID18/5/31，或弱势 stage AI 条件。用 60/170、0/150 阈值和 special flags 写追踪方向；state19 禁止主体移动写入。 |
| AI.PREP.17 | `:569` | LinkState>0 且 `AiProcessHelper` 返回 false：edges+return。注意 helper 的 false 常表示已消费/继续拦截。 |
| AI.PREP.18 | `:576-600` | 背对/状态攻击随机；双随机防御；预测 X/Z+随机 jump。 |
| AI.PREP.19 | `:602-606` | 固定顺序 CallerPrewrite -> Label435PressurePrewrite -> SubHelper -> ApplyEdges。后调用可覆盖前写入。 |

### 3.2 AI 辅助方法

| ID | 方法/行 | 分支摘要、字段写入、RNG |
|---|---|---|
| AI.TARGET.01 | `:1518 FindNearestAiTargetSlot` | 第一遍排除 self/inactive/null；非角色仅接受 state3000 且 Vx 朝 self；TeamCandidateAllowed；alive、非14、abs(Y)<=2；Manhattan X+Z 最小。sameZLane 在此时按 <15 固定。self state!=9 时第二遍可用空中/14目标替换：最近、Z<40、X<250；**不会重算 sameZLane/bestDist**。 |
| AI.COORD.01 | `:1607 MoveTowardCoordinate` | 任一坐标<=-1000 退；X阈值6/250/100，Z阈值3；左右阻挡触发 jump；进入 X/Z各90 内清两坐标为-1000。 |
| AI.ROLL.01 | `:1648/1653 RollAndClearAiKeys` | RollFromCurrent；清方向；显式清 Jump/Defend/Attack。 |
| AI.STATE.01 | `:1667 AiFrameState` | 当前 DAT frame state；null=>0。 |
| AI.DIST.01 | `:1670 AiDistance` | Manhattan X+Z。 |
| AI.BETWEEN.01 | `:1673 AiBetweenX` | 严格位于两端之间，不含端点。 |
| AI.COORD.02 | `:1676 AiPostCacheCoordinateAllowsSpecial` | X coord未设=>true；任一轴距>90=>false；两轴<=90 清坐标；true。 |
| AI.S3000.01 | `:1693 AiPreUpdateTarget3000SideEffect` | 非3000=>false；AiRand3<=0 或 Rand%AiRand3==0，self state!=7，目标在前后200且朝来时 Attack edge；再按朝向写转向键；true。 |
| AI.OID331916.01 | `:1718` | self OID 33/19/16；Rand%5==0 或 target state16/8；预测 dx<60,dz<7,Pp>150,面向目标=>ComboDua=3,true。 |
| AI.OID521221.01 | `:1740` | self 52/1/2/21；依序：state3,Pp125,Rand10,dx120,dz10=>Dja；Pp125,Rand5,dx100,dz30=>若目标右 Duj、但无论左右都true；Pp125,Rand14,dx700,dz150=>Dra/Dla；Pp125,Rand5,dz20=>Drj/Dlj；最后随机/target-state预测近距且 Pp<100=>Dua。 |
| AI.OID512187.01 | `:1796` | self51/2/18/7；frame266..279 且 dz>13或target非角色=>Attack edge；Pp300随机近距=>Duj；Pp300随机 dx950=>Dua；Pp250,Rand5,40<dx<1200,dz13=>Drj/Dlj。 |
| AI.FIRST.01 | `:1837` | self1/2/4/5/21；低血+Rand10+Pp85=>Ddj；有目标+Rand30+Pp250=>Dua；按 target OID 选择 dx/Pp 阈值，Rand15、100<dx<250/500,dz30,selfPp100,targetPp170/220、无特殊物体=>Dlj/Drj。 |
| AI.GUARD.01 | `:1881` | 同 self group；link/frame gate；Hp window且非sameZ；扫 slots<20 同队、距离、Pp>350、低血 teammate、其距离<nearest/3；需要先转向则写左右并 true，否则 Duj=3。 |
| AI.OID1.01 | `:1943` | self1/21/17；排除自身frame260..289近距；第一随机分支 Pp150、dx150,dz8、target state随机门=>Drj/Dlj；第二 Pp75、dx100,dz7,Rand7：低Pp/状态门=>Dda，否则 Drj/Dlj。 |
| AI.OID1CLOSE.01 | `:1986` | 仅 self1/21/17 且 frame260..289、dx<100,dz<7；同为空/空中随机可 Jump edge；否则大多数情况直接 true；余下仅面向目标时 KeyDefend=1，但无条件 PrevDefend=0；true。 |
| AI.OID4.01 | `:2016` | self4/10/19；Pp360、dx100,dz70、Rand%(Hp/5+10)==0=>Duj；Rand45,100<dx<550,dz20,Pp170=>Dlj/Drj；Rand30,Pp200,100<dx<160,dz55且面向=>Dja。 |
| AI.OID5.01 | `:2053` | self5/19；Pp450,dx>100,dz>50,Rand3=>Ddj/Duj随机；Pp70,100<dx<160,dz8,Rand10=>Drj/Dlj；Rand30,Pp200,100<dx<160,dz55且面向=>Dra/Dla。 |
| AI.SUBOID.01 | `:2097/2100` | Sub group `oid<=29 ||33||34`; special gate `18||5||31||36`。 |
| AI.SUB.01 | `:2103 AiProcessSubHelper` | Pp<150 无条件 Dja=3；预测 throw 距离80/Z5/Rand(AiRand3+3)触发 jump；special左右逆目标时 return；Rand(AiRand3+1)门；随后普通 OID 预测 attack/jump/defend，OID34二选一，OID1有额外 attack/jump。 |
| AI.PREWRITE.01 | `:2179 AiProcessSubCallerPrewrite` | special OID、Link0；target state16 时预测350/Z5/随机/面向=>jump；非16时 close trigger，非close且self state!=7走预测300 jump；close且state!=7在 gate距离内按舞台边界写左右+Prev0，Rand17 defend。 |
| AI.PRESSURE.01 | `:2242 AiProcessSubLabel435PressurePrewrite` | target非16+specialOid+Link0 直接 return；要求 target Hp>2*self Hp 或 self低血、InputPhase1、target ObjType0、self slot>=20/team!=5；close trigger且self state!=7；近窗按边界移动，Rand17 defend。 |
| AI.HELD.01 | `:2288 AiProcessHelper` | 首门 Rand%(AiRand3+1)>0=>false；held slot非法=>true。扫 slots<20 求 lineCover（代码条件读取 `target.Unk364 != self.Unk364`，不是 cand team）；state2随机 jump/defend。 |
| AI.HELD.02 | `:2333` | held OID100/101/120/121/124：宽预测 jump；124额外随机；随机近窗追目标。OID150/151且无 lineCover 走另一 jump。非122/123=>true。 |
| AI.HELD.03 | `:2381` | held122/123：先清全部 keys；state17+sameZ+无特殊+HitStop!=0=>Attack,true? 实际 return false；gate远=>false；按 Z边界反向、舞台左右、近距躲避、state2逆 facing，均多为 false；最后 Rand5，条件失败 jump false，否则 Drj/Dlj true。 |
| AI.TEAM.01 | `:2488 TeamCandidateAllowed` | candidate team不同：phase!=1 或 self team5 可 true；之后 candidate team!=5=>false；phase!=1=>false；最后仅 candidate team5且不同于self true。 |
| AI.MOVEMODE.01 | `:2505` | 仅 InputPhase1 且 self team!=5；扫前10 active living characters 取最右 X/Z；self 超过加半Z差>200=>mode1；X>right+400=>mode2（覆盖1）。 |
| AI.NOTARGET.01 | `:2543` | saved target有效且 gate近窗且 moveMode1=>KeyLeft；self OID7 frame255..261、OID9 280..290、OID32 240..245=>Attack。 |
| AI.SOUND.01 | `:2583` | 无空 cue 过滤；直接 PendingSound `{Cue,WorldX,Tick=GameTick}`。 |

AI RNG 调用计数：`InputRuntime.cs` 中 `NtsdRng.Rand()` 共 72 个文本调用点；短路条件决定实际消费数，任何条件重排都会改变后续 RNG 序列。

## 4. FrameTick 台账

| ID | 方法/行 | 分支、字段、数据入口 |
|---|---|---|
| FT.TICK.00 | `Frame/FrameTick.cs:13 Tick` | CharData null；ThrowFrameGuard==Frame；FrameDelay!=0且非 type3；LinkState<0；frame null；首 cpoint kind2，均早退。AttackExempt decrement 位于 FrameDelay gate 后、Link gate 前。 |
| FT.TICK.01 | `:38` | type3 且 frame.HitA>0：Hp-=HitA；<=0钳0并立即跳 `HitD`，重取 frame/state。 |
| FT.TICK.02 | `:52-57` | HitStop 向0；Fall>0--；HitStateCount>0--；Residual.HitConfirm>0--。 |
| FT.TICK.03 | `:59-65` | Frame!=WaitCounter 时 queue frame sound 并 Attacking=0；随后 Attacking++。 |
| FT.TICK.04 | `:67` | state0且YInt<0：frame212、SuppressJumpInit=true、JumpInitPending=false，重取 frame/state。 |
| FT.TICK.05 | `:78` | IronBall state2000、地面、abs(Vx)<0.1：清 jump flags 并 return。 |
| FT.TICK.06 | `:88` | state14且dead：KillCount>=0/team5 或 slot>=20 时 HitStop<=0=>30；Attacking=0。 |
| FT.TICK.07 | `:103` | state2000 facing 按 Vx>0；零速度得到 facing1。 |
| FT.TICK.08 | `:106` | `Attacking>frame.Wait` 才推进（严格大于）；取 Next，Attacking=0。Next0 不切帧。 |
| FT.NEXT.01 | `:111` | Next999：非地面角色=>212，否则0；SuppressJumpInit=to212，Pending=false。 |
| FT.NEXT.02 | `:122` | Next<0 先翻面再取绝对值；SetFrameImmediate；Suppress=false；Pending=(Frame==212)。Frame越界[0,400) return。 |
| FT.NEXT.03 | `:141` | previous frame 用 `WaitCounter` 查；previous state14离开且新state!=13，按 team/Unk344/difficulty/mode/OID规则写 HitStop15。 |
| FT.NEXT.04 | `:162` | frame212且Pending且未Suppress：Vy=JumpHeight；左右 only 写 Vx ±JumpDistance；上下 only写 Vz ±JumpDistanceZ。 |
| FT.NEXT.05 | `:176` | 切帧后 queue sound；Frame<400且 frame.Mp<0且 ppMode：代码比较 `Pp < mpDelta`（mpDelta为负，通常 false）；false 分支 Pp+=负数并 Refund display。之后 HitD>0 可按地面反向输入转帧。 |
| FT.TAIL.01 | `:207-215` | current frame110/114=>CdDefendLock=3；202=>HitStop20；清 jump flags；`WaitCounter=Frame`。 |
| FT.SOUND.01 | `:218 QueueFrameSound` | Frame越界或 sound空白退；否则 pending sound。 |
| FT.OP.00 | `:233 ProcessOpointSpawn` | CharData/frame null退；首 opoint 无效、Oid<=0或Attacking!=0退；角色且FrameDelay!=0退。 |
| FT.OP.01 | `:248` | 为每个有效 op；Facing>10 表示 count=`/10`, facingMode=`%10`；每次 Spawn；成功 ObjectCount++。多发按 -5..5 spread 写 Vz，并按 Vx符号减速/偏移。 |
| FT.OP.02 | `:279` | 子体 AttackExempt先0；spawner DAT type3 + frame state3003 时用 spawner.AnimCounter 作为 linked slot，双向 VRest=10。 |
| FT.OP.03 | `:294` | 多子体按中心距离设置 AttackExempt=2*n；偶数中间两体为0；每对先前子体双向 VRest=40。 |
| FT.SPAWN.00 | `:333 SpawnFromOpoint` | GetChar失败/null；从 slot50 起找 free，失败null；Entity.Reset 后初始化。 |
| FT.SPAWN.01 | `:367` | facingMode0=spawner facing；1=反向；其他=0。X位置仍按 **spawner facing** 算，非 spawnFacing；Y中心差；Z=spawner.Z+1。 |
| FT.SPAWN.02 | `:396` | op.Dvx 按 spawnFacing 取反；Vy=op.Dvy,Vz=0；runtime identity/type/WeaponHp。DAT ObjType0：KillCount=spawner KillCount或slot；继承HitStop；AI=true。OID5/52 stats=10/10/10/Pp5。 |
| FT.SPAWN.03 | `:422` | op.Kind2：spawner LinkState1/Target/Held=child；child LinkState=-1, HolderIdx=spawner slot, Team继承。 |
| FT.SPAWN.04 | `:432` | child state3000/1002/3006且OID非223/224：按 spawner 上下写 Vz±2.5；OID211再*0.25。 |
| FT.SPAWN.05 | `:446` | world.ResetCooldowns 只清 ARest/VRest；随后显式清八个 input Cd；返回 child。 |

## 5. FrameAdvance / FrameLogic 台账

### 5.1 Advance 与非角色 frame velocity

| ID | 方法/行 | 分支 |
|---|---|---|
| FA.ADV.00 | `FrameAdvance.cs:13 Advance` | CharData/ThrowFrameGuard 早退；FrameDelay<0 先++并退；>0先--并退；LinkState<0、frame null、首 cpoint kind2 退；非 Character runtime ObjType 才 ApplyNonCharacterFrameVelocity；最后 Physics.Update。 |
| FA.VEL.01 | `:992 ApplyNonCharacterFrameVelocity` | Dvx 经 ApplyFrameVelocity(facing sign)；Dvy>500=>-550，非0累加；Dvz>500=>-550并return，0 return；否则 Up/Cd优先写负，Down/Cd优先随后可覆盖正。 |
| FA.VEL.02 | `:1019 ApplyFrameVelocity` | value>500=>value-550（因此550先命中并得0，后面的 `value==550` 实际不可达）；value>0按 direction 只提高对应方向最低速度；value<0只压低对应方向最高速度。 |

### 5.2 hit_fa FrameLogic

| ID | 方法/行 | 分支 |
|---|---|---|
| FL.ROOT.00 | `:49 FrameLogic` | CharData/frame null退；weapon FlyingA/B + WeaponState1000 + abs(Vx)>Boomerang threshold=>frame40。WeaponState1002=>2000；2000每次 Vx*=0.5，abs(Vx)<0.5时Vx0/state3000。HitFa0退。 |
| FL.TARGET.01 | `:87-162` | hitFa 非4/5/6/7/8/9/10/11/13时验证/扫描 target；现 target 要 alive、非state14、abs(HitStop)<=2、敌队；holder team 可阻止重扫。扫描 active character DAT、敌 team、非holder team、alive；已有 Picker!=-1 时排除 state14/HitStop；距离 X+逻辑Z。无 Picker=>entity.Hp=0 return。 |
| FL.CASE10 | `:166` | Vx按符号±1.1，钳±30；Y>3钳3；facing按Vx；YInt同步。 |
| FL.CASE1 | `:218` | target有效 alive；X加速度0.85；Z阈值7加速度0.3；Vy*=5/7；角色 target 以 Y+10 追踪±1.2，否则 Y>0 时 Y+=1；Vx±13,Vz±2,Y<=1；facing/YInt。 |
| FL.CASE5 | `:247` | 对每个 active living same-team character，slot50+生成 OID219；位置 self、Vx=(teammateX-selfX)/50、Picker=teammate；最后销毁 self、ObjectCount--。 |
| FL.CASE8 | `:315` | 收集敌 living characters；spawn数 candidates<=4?3:(count-3)/2+3；缺 OID225 时也销毁 self；每 child Vx Rand21-11, Vy/Vz `3-Rand24*0.25`, Picker随机 candidate或self；最后销毁 self。 |
| FL.CASE2_4_12_14.A | `:392` | case4 target null时可直接取 targetSlot；entity dead/inactive 或 target null走 no-target drift。case4 target alive且 dx(-30,30),dy(0,80),dz(-10,10)：停速、frame60、target.CatchTimer100 return。 |
| FL.CASE2_4_12_14.B | `:425` | X accel0.7；Z阈5 accel0.4；Vy*=5/7；Y+40追踪；Vx±14,Y<=1.4；case14 Vz±1.5否则±2.2；facing/YInt。case2按 abs(Vx)>14=>frame5, >7=>3, else1，保护成对帧。case14按 abs(Vx)>=8 对 frame>40减50，否则 frame<10加50。 |
| FL.NOTARGET.CATCH | `:488` | Vx按符号加速2并钳17；Y<=1.4；facing/YInt；仅 hitFa2按速度组选 frame5/3/1。 |
| FL.CASE11 | `:519` | 固定14项 spawn 表（OID211/221/212、具体 frame/offset/vz/facing）；逐项 char存在且有free才生，free失败break；随后销毁 self。再按 holder team 验证/扫描 Picker；无 Picker=>Hp0；最后 Vx按符号±2、钳17、facing。 |
| FL.CASE6_9 | `:667` | hitFa6 zOffMax7、单轮；hitFa9 zOffMax10、later max4。扫敌 living character；生成 OID220 或随机221/222。6：Vx追踪/50,Vy=-4-Rand4；9：Vx Rand21-11,Vy=-2-Rand40/6；Picker=target。循环条件按源码；最后销毁 self。 |
| FL.CASE13 | `:774` | 收集敌 living；无free/char228均销毁 self；target随机或self；生成228，YInt随机±3但 `Y` 复制 self.Y（整数/浮点初始不一致）；Vy0.1,Vz随机+原Vz；销毁 self。 |
| FL.CASE3 | `:854` | entity alive/active+target active：X accel0.7,Z阈10 accel0.17，钳16/2.4；否则 no-target drift。 |
| FL.CASE7 | `:871` | 先尝试 clone self DAT到free slot，frame40/stats；之后重取 Picker。有效时 X加速度0.7 **重复两次**，Z阈5 accel0.4，Vy<4加0.4并直接Y+=Vy；`YInt>-25` 判定使用旧整数，满足则frame60/停速；钳速。无效时 Vx加速2、Vy/Y同类；YInt>-25则frame60,YInt=-25后停速；尾部 facing，随后 `YInt=(int)Y` 会覆盖前面的 -25。 |
| FL.NOTARGET.DRIFT | `:961` | Vx按当前符号±2（0走正），钳17，facing。 |
| FL.Z.01 | `:978 LogicZInt` | type3 返回 `int(Z-Type3VisualZOffset)`；其他ZInt。 |
| FL.Z.02 | `:985 FrameLogicTargetZInt` | hitFa 1/3/7/12/14 强制ZInt；其他走 LogicZInt。 |

FrameAdvance RNG 调用点共 11；集中在 case8/6/9/13 的生成参数与随机目标。

## 6. Physics 逐方法/逐分支台账

| ID | 方法/行 | 权威顺序与分支 |
|---|---|---|
| PH.ROOT.01 | `Frame/Physics.cs:12 Update` | CharData null退；严格顺序 Horizontal -> Depth -> Type3VisualZ -> GroundFriction -> BoomerangFrame -> Vertical -> SyncIntegers -> ResetWeaponCount。 |
| PH.X.01 | `:27 UpdateHorizontal` | Vx>0且BlockRight或Vx<0且BlockLeft时不吃基础Vx；之后 DAT type4或OID120仍加0.2Vx，OID101再减0.2Vx；特殊偏移不受 block gate。 |
| PH.Z.01 | `:42 UpdateDepth` | 按BlockFwdZ/BackZ决定是否 Z+=Vz；无论是否阻挡，随后清四个 block flags。 |
| PH.TYPE3.01 | `:52 ApplyType3VisualZ` | 仅 DAT type3、frame存在、HitJ>0；visualZ=HitJ-50；同时累加 Z 与 Type3VisualZOffset。 |
| PH.FRIC.01 | `:66 ApplyGroundFriction` | YInt<0不摩擦；否则 Vx/Vz 各 ApplyUnitFriction。 |
| PH.FRIC.02 | `:75 ApplyUnitFriction` | >0.0001减1，穿过0.0001钳0；<-0.0001加1，穿过0.0001钳0；其余原值。 |
| PH.BOOM.01 | `:95 ApplyBoomerangFrameSelection` | DAT type4/6，当前 frame state1000，abs(Vx)>9：立即frame40。 |
| PH.Y.01 | `:110 UpdateVertical` | 先 `newY=Y+Vy`, 写Y；newY<-0.0001：gravity -> character air frame，return；否则 ground resolve。 |
| PH.GRAV.01 | `:125 ApplyAirGravity` | type3无重力；type6=1.1333333；type4=0.85；其他若 frame state1002，OID124=.17,120=.425,101=type6,其余=.5666667；否则1.7；Vy+=gravity。 |
| PH.AIR.01 | `:162 ApplyCharacterAirFrameSelection` | 仅 character DAT。state12 frame<185按Vy<-8/<1/<8/else选180..183；WeaponCount<0时用 `(GameTick-1)%12` 与Vy<12在181/182覆盖。frame186..190按同阈选186..189。重取 frame；state18、frame<205、Vy>1=>immediate205。 |
| PH.GROUND.00 | `:203 ApplyGroundResolve` | frame null、首 cpoint kind2、newY<=0.0001 均退。 |
| PH.GROUND.CHAR13 | `:216-241` | character且下降着地；state13：Vy<=17且abs(Vx)<=9则 Vx/3,Y0,Vy0；否则按 FallDamageDiv 扣10或`-1000/div`，Y0,Vy=-3.5,Vx钳7,frame185。其他走 generic landing。 |
| PH.GROUND.SHURIKEN | `:255` | 仅 newY>0且Vy>0；Unk31C-=WeaponDropHurt,Y0；oldVy<=9.9=>Vy0, state1002?70:60,Vx/2,Attacking0；否则 state1002=>Vy-8,frame7,flip,Vx/2,sound；否则 frame60停Y/Vx/attack。 |
| PH.GROUND.FLY | `:289` | type4/6且下降；Unk31C减drop hurt；type6且Hp<=0再设-1；high=oldVy>8.5或abs(Vx)>10，且 state1002/1000 时弹：Vy=-.7 old、最小-10,Vx*.7,frame0,sound；否则停止并 frame70/60、Attacking0。 |
| PH.GROUND.BALL | `:321` | type2且newY>0；先Unk31C--；oldVy>9：sound,Vy-5,flip,Vx/2；否则再减drop hurt并钳>=0,Vy0,frame20,Vx/2,Attacking0。 |
| PH.GROUND.999 | `:349` | OID999：Y/Vy/Vx=0，frame101，Attacking0。 |
| PH.LAND.GENERIC | `:359` | state12/18：sound6；WeaponCount!=0 按绝对值扣Hp/HpMax，div>0缩放，清WeaponCount；Vy<=11且abs(Vx)<=9且state!=18：Vx/3,Y/Vy0,frame230/231,Attacking0；否则Y0,Vy=-3.5,Vx钳7,frame185/191。其他 state：Vx/3,Y/Vy0；state100=>94，frame212或state6=>215，否则219；Attacking0。 |
| PH.SYNC.01 | `:406` | XInt/YInt/ZInt = `(int)` double，C# 向0截断。 |
| PH.WCOUNT.01 | `:413` | 当前 frame null 或 state!=12 时 WeaponCount=0。 |
| PH.SOUND.01 | `:420` | cue 空白退，否则 pending sound with GameTick。 |

## 7. DAT / 常量数据驱动入口

| ID | 来源 | 使用契约 |
|---|---|---|
| DATA.FRAME.01 | `Data/DatModels.cs:99 FrameData` | 默认 `Wait=1`，其余 int 0，Sound empty，lists empty。FrameId/Pic/State/Wait/Next/Dvx/y/z/Center/HitA/D/J/Fa/Fj/Ua/Uj/Da/Dj/Ja/Mp/Vaction 全是直接数据入口。 |
| DATA.FRAME.02 | `DatModels.cs:185 GetFrameOrNull` | id越界=>null；id合法但 FrameIndex=-1 时返回静态 `EmptyFrame`，不是 null。`HasFrame` 才区分缺帧。 |
| DATA.OP.01 | `DatModels.cs:45 OPointData` | Kind/X/Y/Action/Dvx/Dvy/Oid/Facing，默认0。Facing十位承载数量，个位承载模式。 |
| DATA.CP.01 | `DatModels.cs:76 CPointData` | 本分区读取首 cpoint Kind==2 作为 Tick/Advance/ground gate；完整 cpoint 行为属 Interaction 跨分区。 |
| DATA.CHAR.01 | `DatModels.cs:156-177` | 默认 movement：walk rate3/speed4/z2；run rate3/speed8/z3.3；heavy walk3/z1.5；heavy run5/z0.8；jump -16.3/8/z3；dash -13/15/z3.75；rowing -2/20。 |
| DATA.CONST.01 | `Common/NtsdConstants.cs:7-14` | MaxObjects400, HitCandidateMax20, Dvx threshold500, zero550。 |
| DATA.CONST.02 | `NtsdConstants.cs:50-60 PhysConst` | gravity default1.7,type4 .85,type6 1.1333333；boomerang threshold9；其他声明常量在本 Physics 实现未全部直接引用。 |

## 8. NtsdEntityRuntime 全字段、默认值与 reset 契约

字段通过 `Entity.cs:11-145` 代理到 runtime；因此 `entity.X` 和 `entity.Runtime.Transform.X` 是同一存储，不存在第二份 legacy backing field。

### 8.1 Identity / Transform / Motion / Frame

| 分组（声明行） | 全字段（声明默认 / Reset） |
|---|---|
| Identity `NtsdEntityRuntime.cs:8-47` | `Active false/false`; `Slot -1/-1`; `CharId -1/-1`; `Team 0/0`; `AiControlled false/false`; `ObjType 0/0`; `EntityType 0/0`; `Category 0/0`; `OwnerId -1/-1`; `Unk364 0/0`。 |
| Transform `:50-82` | `X,Y,Z 0/0`; `XInt,YInt,ZInt 0/0`; `RenderOffsetX 0/0`; `Type3VisualZOffset 0/0`; `Facing 0/0`。 |
| Motion `:85-114` | `Vx,Vy,Vz 0/0`; `KnockbackVx,KnockbackVy,KnockbackVz 0.1/0.1`; `HitCount 0/0`; `Fall 0/0`。 |
| Frame `:117-157` | `Frame,WaitCounter,FrameWaitCounter,PrevFrame,PrevFrame2 0/0`; `SuppressJumpInit,JumpInitPending false/false`; `Attacking,HitStop,FrameDelay,HitStateCount,AnimCounter,AnimSub 0/0`。 |

### 8.2 Links / Transient / Stats / Residual

| 分组 | 全字段（声明默认 / Reset） |
|---|---|
| Links `:160-214` | `LinkState 0`; `TargetIdx -1`; `HeldWeaponSlot -1`; `ThrowFrameGuard -1`; `ReleaseTick -1`; `GrabbedTimer 0`; `StuckVictimSlot -1`; `CaughtIdx -1`; `CatcherIdx -1`; `CaughtDuration 0`; `EscapeCounter 0`; `HolderIdx -1`; `HolderCopy 99`; `PickerIdx -1`; `PickupCount 0`；声明与 Reset 相同。 |
| Transient `:217-274` | `HitCandidateSlots int[20]`全0；`HitCandidateItrIndices sbyte[20]`全0；`Mp 0`; `Mp2,Mp3,Mp4 1000`。`ResetScratchState`只重置Mp组；`ClearHitCandidates`清数组；`Reset`两者都做。 |
| Stats `:277-323` | `Hp,HpMax,Hp3 500`; `Pp 500`; `RespawnCount 0`; `KillCount -1`; `SpawnerSlot -1`; `WeaponCount 0`; `FallDamageDiv 0`; `Unk344 0`; `ComboCountVic,ComboCountAtk 0`; `KillStat 0`。 |
| Residual `:326-477` | `AbortRemainingHitPairs false`; `Unk360 -1`; `Unk318 0`; `Unk31C 0`; `Unk324,Unk328,Unk32C -1`; `Unk330,Unk334,Unk338 0`; `Unk33C -1`; `Unk400,Unk3FC -1000`; `HitConfirm,HitConfirm2 0`; `AttackExempt 0`; `HealTimer,CatchTimer 0`; `BlockBackZ,BlockFwdZ,BlockLeft,BlockRight 0`；`WeaponState 0`。 |

### 8.3 Input / Presentation

| 分组 | 全字段（声明默认 / Reset） |
|---|---|
| Input `:480-745` | 八 Cd：`CdAttack,CdJump,CdDefend,CdDefendLock,CdRight,CdLeft,CdUp,CdDown=0`；九 Combo：`ComboDra,ComboDla,ComboDua,ComboDda,ComboDrj,ComboDlj,ComboDuj,ComboDdj,ComboDja=0`；`InputHistory int[6]`全0；七 Prev：`PrevUp,PrevDown,PrevLeft,PrevRight,PrevJump,PrevDefend,PrevAttack=0`；七 Key：`KeyUp,KeyDown,KeyLeft,KeyRight,KeyAttack,KeyJump,KeyDefend=0`。 |
| Presentation `NtsdEntityPresentationRuntime.cs:5-61` | `Hp2Orig,HpOrig,PpDisplay,HitRecordCount,Blink=0`; 三个长度10数组 `HitRecordDamage,HitRecordX,HitRecordZ` 全0。 |

### 8.4 总 reset / copy / snapshot

| ID | 方法 | 契约 |
|---|---|---|
| RT.RESET.01 | `NtsdEntityRuntime.cs:761 Reset` | 顺序 Reset Identity,Transform,Motion,Frame,Links,Transient,Stats,Input,Presentation,Residual。`Entity.Reset` 调它后把 runtime ObjType 设为 Character。 |
| RT.COPY.01 | `:775 CopyFrom(runtime)` | 复制除 Transient 外全部九组；注释明确 snapshot/runtime clone 排除 scratch。目标新建时 transient 保持其构造默认。 |
| RT.CLONE.01 | `:789 Clone` | new runtime -> CopyFrom(this)，因此不带 transient。 |
| RT.COPYENTITY.01 | `:796 CopyFrom(Entity)` | 包含 Transient/Presentation/Residual 全部字段；Category 只在 Identity.CopyFrom Entity 时写。 |
| RT.APPLY.01 | `:879 ApplyTo(Entity)` | 包含 Transient；**未把 `Identity.Category` 应用回 Entity**（Entity.Category 是 resolver property）。 |
| RT.SYNC.01 | `Entity/CharacterSync.cs:12-20` | SyncRuntimeFromLegacy 调 runtime.CopyFrom(entity)；ApplyLegacyFromRuntime 调 runtime.ApplyTo(entity)。因 Entity 属性本已代理到 runtime，这两者主要是数组/全字段镜像动作。 |
| RT.CHECK.01 | `CharacterSync.cs:89-161,164-317` | RuntimesMatch/HashEntityRuntime 包含 Identity/Transform/Motion/Frame/Links/Stats/Input/Presentation/Residual；排除 Transient。 |

### 8.5 runtime helper 逐方法核销

| ID | 方法/行 | 契约 |
|---|---|---|
| RT.IDENTITY.01 | `NtsdEntityRuntime.cs:21/35` | Identity Reset / CopyFrom，字段集合与8.1一致。 |
| RT.TRANSFORM.01 | `:62/71` | Transform Reset / CopyFrom。 |
| RT.MOTION.01 | `:96/104` | Motion Reset / CopyFrom。 |
| RT.FRAME.01 | `:133/142` | Frame Reset / CopyFrom。 |
| RT.LINKS.01 | `:178/197` | Links Reset / CopyFrom。 |
| RT.TRANSIENT.01 | `:226 ResetScratchState` | 只写 Mp=0、Mp2/3/4=1000。 |
| RT.TRANSIENT.02 | `:234 ClearHitCandidates` | Array.Clear 两个 candidate array。 |
| RT.TRANSIENT.03 | `:240/246/256/266` | Reset=01+02；CopyFrom(runtime/entity)与ApplyTo(Entity)均含4个Mp与两数组。 |
| RT.STATS.01 | `:293/308` | Stats Reset / CopyFrom。 |
| RT.RESIDUAL.01 | `:352 ClearBlockFlags` | 四个 block 全0。 |
| RT.RESIDUAL.02 | `:360 ClearHitConfirm` | HitConfirm/HitConfirm2=0。 |
| RT.RESIDUAL.03 | `:366 ClearAttackExempt` | AttackExempt=0。 |
| RT.RESIDUAL.04 | `:371/398/425/452` | Residual Reset / CopyFrom(runtime/entity) / ApplyTo。 |
| RT.INPUT.01 | `:515 Reset` | 清八Cd、九combo、history、七Prev、七Key。 |
| RT.INPUT.02 | `:524 RollFromCurrent` | 七 Prev 按同名 Key 快照。 |
| RT.INPUT.03 | `:535/543` | ClearDirectionalKeys只清四方向；ClearActionKeys只清Attack/Jump/Defend。 |
| RT.INPUT.04 | `:550/559/564/569` | history push/gate/clear/query，见IN.HIST。 |
| RT.INPUT.05 | `:574/619` | ApplyEdges/TickCooldowns，见IN.EDGE/IN.CD。 |
| RT.INPUT.06 | `:639/675/711` | Input CopyFrom(Entity/runtime) / ApplyTo；数组用 Array.Copy。 |
| RT.PRESENT.01 | `NtsdEntityPresentationRuntime.cs:16/28/40/52` | Reset / CopyFrom(runtime/entity) / ApplyTo，包含三个长度10数组。 |
| RT.ENTITY.01 | `NtsdEntityRuntime.cs:761/775/789/796/879` | 总 Reset / Copy / Clone / Entity bridge，见8.4。 |

## 9. 全字段生产读写方总表

以下是对 `src/**/*.cs` 的生产引用闭包。`Entity.cs` 代理、runtime 自身 Reset/Copy/Apply、`CharacterSync` snapshot/hash 是所有字段的共同读写/观察方，不在每行重复。`Host` renderer 只读位置/帧/表现字段，不定义战斗写入。

| runtime 分组 | 生产写入方（除共同镜像） | 生产读取方（除共同镜像） |
|---|---|---|
| Identity 全10字段 | `App/DirectBattleBootstrap`; `SimulationWorld.Registry`; `GameTick` 的 respawn/state transform/stage spawn/weapon drop/merge-split；`FrameTick.SpawnFromOpoint`; `FrameAdvance` 各 spawn case；`CPointRuntime.SwapAttackerCharData`; `HitResolve.ReplaceWithActiveOid`。 | `SimulationTickDriver.ApplyFrameInput`; `CharacterLogic`; Input AI 全体；FrameTick/Advance/Physics；Collision/CPoint/Hit/Weapon；GameTick pass gates；Host renderer/audio。 |
| Transform 全9字段 | Input movement/AI facing；FrameAdvance/FrameTick spawn与追踪；Physics；Collision/CPoint/Hit/Weapon 对齐/抓取/击退；GameTick bounds/camera/respawn/stage/effect；Host 仅 `RenderOffsetX` 表现写。 | Input AI/动作；Frame/Physics；Collision/Interaction；GameTick；Host render。`Type3VisualZOffset` 的逻辑读者为 FrameAdvance.LogicZInt、CollisionCollect.CollisionZ、GameTick bounds。 |
| Motion 全8字段 | Input movement/frame velocity；FrameAdvance/Physics；CPoint throw；HitResolve knockback；Weapon held sync；GameTick postprocess/death/respawn/spawn。 | 同上；FrameTick state2000；AI prediction；GameTick postprocess。 |
| Frame 全13字段 | Input frame/action；FrameRuntime.SetFrameImmediate；FrameTick；FrameAdvance/Physics；CPoint/Hit/Weapon；GameTick state passes/late/respawn/spawn。 | 所有帧/输入/交互 passes。`PrevFrame2` 唯一周期性生产写为 GameTick.SnapshotPrevFrame2；`PrevFrame` 在 late tail 写；WaitCounter 在 FrameTick 用作“上一帧”并在尾写。 |
| Links 全16字段 | FrameTick op kind2；FrameAdvance Picker；CPoint/Hit/Weapon 为主要拥有者；GameTick positive-link validation/merge-split/spawn。 | Input link/held AI；FrameTick/Advance gates；Interaction全链；GameTick late/death。 |
| Transient 全6载体 | `CollisionCollect.RecordCandidate` 写 Mp/Mp2/Mp3/Mp4 与 candidate arrays；`Entity.ResetTransientScratchState/ClearHitCandidateCarriers`; GameTick postframe清 carrier。 | `HitResolve.ResolveCandidates` 及 collision recorder。无 Host 规则读者；snapshot/checksum排除。 |
| Stats 全13字段 | bootstrap/spawn/respawn；Input Pp/Hp cost；FrameTick type3；FrameAdvance spawn/kill self；Physics fall/drop；CPoint/HitResolve damage/stats；GameTick regen/results/respawn/heal。 | AI target/decision；Frame/Physics；Interaction；GameTick results/late；Host HUD/results。 |
| Input 全32载体 | `SimulationTickDriver -> PollHumanInput`; AI Prepare/helpers；GameTick clear/history gate；FrameTick CdDefendLock与spawn清Cd；runtime edge/cooldown/combo。 | Input combo/movement；FrameTick jump init/turn；FrameAdvance Dvz；AI；GameTick results/N30；opoint child Vz。Interaction 对当前 keys/Cd有少量条件读取。 |
| Presentation 全8载体 | `CharacterPresentation`; Input/FrameTick Pp display；HitResolve hit record；GameTick respawn overlay/可能Blink。 | GameTick results/respawn；Host HUD/render；checksum。 |
| Residual 全23字段 | AI写 Unk360/坐标、HitConfirm等；FrameTick AttackExempt decrement；FrameAdvance weapon state/catch timer；Physics block clear/weapon hp；Collision/Hit/CPoint/Weapon 为主要写入方；GameTick state transforms/maintenance/heal/cleanup。 | Input AI/combo；Frame/Physics；完整 interaction；GameTick state/late。 |

### 9.1 字段级例外与唯一拥有者

| 字段 | 精确生产契约 |
|---|---|
| `InputHistory[0]` | 只由 `GameTick.RunN30InputTrigger` 经 SetHistoryGate 写 true/false；Reset清；AI多处读。 |
| `InputHistory[1..5]` | 只由 ApplyEdges/PushInputHistory滚动，N30读取[2..5]并清tail；Reset清。 |
| `CdDefendLock` | TickCooldowns递减；FrameTick current frame110/114写3；Spawn清0；DoFrameJump不写；StandingActions读取。 |
| `SuppressJumpInit/JumpInitPending` | FrameTick独占战斗读写；reset/copy外无其他生产拥有者。 |
| `Type3VisualZOffset` | Physics累加；FrameAdvance/Collision/GameTick读取逻辑Z；spawn/reset初始化0；Host只读表现。 |
| `AttackExempt` | FrameTick Tick递减；opoint spread写延迟；GameTick cooldown pass可按 itr/wpoint清0；collision/hit读取。 |
| 四 `Block*` | collision/hit产生；Input AI读取；Physics X/Z阻挡读取，Depth 后一次性全清。 |
| `WeaponState` | FrameAdvance.FrameLogic 1002->2000->3000；其他 interaction 可写；reset0。 |
| `Mp/Mp2/Mp3/Mp4` | collision scratch：Mp默认0，其余1000；不是角色 PP。 |
| `RenderOffsetX` | 战斗 core不写；Host render 计算写并只用于表现。它仍进入 checksum。 |
| `Category` | CopyFrom(Entity) 取 resolver 值，clone/hash保留；ApplyTo 不写回。 |

## 10. Wrapper/适配方法覆盖

以下方法无额外规则，仍给唯一ID以防误判为第二实现：

| ID | 文件/行 | 转发 |
|---|---|---|
| WRAP.AI.01 | `Input/AiInputRuntime.cs:10` | -> InputRuntime.PrepareAiInputBasic。 |
| WRAP.AI.02 | `Input/InputRuntime.cs:1662 TickAiInputEdges` | -> ApplyInputEdges(Entity)。 |
| WRAP.INPUT.01 | `InputRuntime.cs:2573/2578 PushInputHistory` | Entity overload -> Input overload -> `input.PushInputHistory`; 当前 `src` 无外部调用，生产边沿直接调用 runtime method。 |
| WRAP.FRAME.01 | `Frame/FrameTransistor.cs:9` | -> FrameRuntime.SetFrameImmediate。 |
| WRAP.FRAME.02 | `Frame/FrameRuntime.cs:10` | 写 Frame 与 FrameWaitCounter=0；不写 Attacking/WaitCounter。 |
| WRAP.FRAME.03 | `Frame/FrameTickRuntime.cs:10/15` | -> FrameTick.Tick/ProcessOpointSpawn。 |
| WRAP.FRAME.04 | `Frame/FrameAdvanceRuntime.cs:10/15` | -> FrameAdvance.FrameLogic/Advance。 |
| WRAP.PHYS.01 | `Frame/PhysicsRuntime.cs:10` | -> Physics.Update。 |
| WRAP.DISPATCH.01 | `Entity/EntityDispatch.cs:12/34` | category switch；null CharData退。四 category 都有显式 case。 |
| WRAP.CATEGORY.01 | `Entity/EntityCategoryLogic.cs:9-47` | Character/Weapon/Special/Effect 的 logic/advance 各自转发；非Character实体基类未覆盖默认实现。 |

## 11. 未解析符号与跨分区依赖

### 11.1 未解析（不能从本分区自行赋义）

| ID | 符号 | 状态 |
|---|---|---|
| UNRES.01 | `Unk318/31C/324/328/32C/330/334/338/33C/360/364/3FC/400` 的业务命名 | 已完整记录默认、reset和本分区读写；源码未提供稳定语义名，保持原名，不能猜译。 |
| UNRES.02 | AI helper 名中的 `Label591/Label435` | 仅是移植标签痕迹；行为已逐分支冻结，原标签语义无符号定义。 |
| UNRES.03 | `FrameTick` mpDelta 分支的 `Pp < negative mpDelta` | 源码明确如此，通常不可满足；不得自行改成 `Pp < -mpDelta`。列为权威可疑点而非未读代码。 |
| UNRES.04 | `FrameAdvance.ApplyFrameVelocity` 的 `value==550` 分支 | 被前置 `value>500` 包含，实际不可达；按权威保留。 |
| UNRES.05 | AI `AiProcessHelper` line-cover 中检查 `target.Unk364 != self.Unk364` 而非 candidate | 行为已冻结；意图不明，不擅自纠正。 |

### 11.2 跨分区依赖（符号已定位，细节由对应总账负责）

| ID | 依赖 | 本分区所需契约 |
|---|---|---|
| DEP.INT.01 | `CollisionCollect` | 产生 hit candidates、Mp scratch、block flags；Physics/late前后顺序由 GameTick 固定。 |
| DEP.INT.02 | `HitResolve` | 消费 candidates，写 damage/knockback/link/stats/presentation；在 late FrameTick 前完成。 |
| DEP.INT.03 | `CPointRuntime` | 首 cpoint kind2 是 FrameTick/Advance/ground early gate；实际抓取同步属 Interaction。 |
| DEP.INT.04 | `WeaponRuntime` | held sync、drop、Link/Holder/Target与 AttackExempt；FrameAdvance 前后调用次序已在 FLOW 记录。 |
| DEP.WORLD.01 | `SimulationWorld.Registry/FreeEntity` | spawn从slot50找free；Reset/Free对 runtime 与 ObjectCount 的契约需与 world 总账交叉核销。 |
| DEP.RNG.01 | `NtsdRng.Rand` | 本范围共83个文本调用点（Input72、FrameAdvance11）；全局 seed/state定义不在本文件组。 |
| DEP.DATA.01 | `DatLoader` | parser 如何填充 FrameData/OPoint/CPoint 不在本台账；运行时读取字段已列全。 |

## 12. 覆盖计数与审计声明

- 直接审计文件：25 个，6,251 行（Input/Frame/Physics/runtime/dispatch/lockstep/host input provider）。
- 补充调用链文件：`GameTick.cs`、`CharacterSync.cs`、`CharacterPresentation.cs`、`SimulationWorld.Passes.cs`、`SimulationWorld.QueryAndLinks.cs`、`DatModels.cs`、`NtsdConstants.cs`。
- 顶层/局部声明扫描：184 个方法形态；本台账为所有与 Input/Frame/Physics/runtime reset/copy/dispatch 有关的方法分配了 ID。
- 分支扫描基线：836 个 `if/else if` 文本形态、29 个 `case`、29 个循环、237 个 return（含 runtime copy/guard 与 wrapper）；密集 AI 条件在 `AI.PREP` 和每个 helper ID 内按源码顺序保留，不把复合布尔子句拆成可交换规则。
- `NtsdEntityRuntime`：10 个分组，137 个字段/数组，声明默认、Reset、Copy/Apply/snapshot边界全部覆盖。
- 未解析调用：0 个本范围生产调用。剩余 5 项是无稳定业务命名或权威源码自身的可疑/不可达表达式，已列 `UNRES`；7 项跨分区依赖已定位。
- 未进行编译/运行测试：本任务是唯一权威的只读静态建模，不改变源码；本台账不能单独证明任何目标工程已对齐。
