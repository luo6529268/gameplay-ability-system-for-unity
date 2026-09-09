# NTSD28 B2 F1～F12 功能键路由交叉表

> Change ID：`NTSD28-B2-FUNCTION-KEY-ROUTE-CROSSWALK-001`  
> Authority：NTSD 2.8-Logan formal playable source closure  
> 口径：区分 Win32/Host route、GameSession queue、session acceptance、simulation tick 与 post-tick effect；
> 不把“按键已识别”误写成“下游效果已执行”。

## 1. Authority live closure

| 层 | Authority 入口 | 正式职责 |
|---|---|---|
| pure route/state | `native_function_keys.h:11-434` | F1～F12映射、disposition/reject、Host/Session/maintenance command、Session state 与 Host transition |
| physical edge | `main.cpp:1924-1984` | `WM_KEYDOWN`保存held state；仅非repeat消息进入one-shot route；Host立即应用，Session只OR入event byte |
| continuous hold | `main.cpp:2401-2417` | 每Host loop读取F11/F12 held state；F12在两者同时按住时覆盖F11；每loop音量±1并刷新overlay 100 ticks |
| session queue | `game_session.cpp:2444-2471` | bit mask聚合F3/F6/F7/F8/F9；尚不在physical callback中改变world |
| tick dispatch | `game_session.cpp:2632-2664` | tick开头快照并清空queued byte；固定顺序F3→F6→F7→F8→F9重新做Session gate，记录accepted event byte |
| post-tick tail | `game_session.cpp:3200-3227` | simulation tick之后消费shared F8/F9 command与独立F7 flag；F7只写active entity `current_mp=500` |
| reset | `game_session.cpp:2197-2204` | battle reset恢复lock0、初始hit-resource、计数0、pending false/none、queued/last-event byte0 |

`native_function_keys.h` 被正式 playable host与GameSession直接包含并调用；这不是未接入实验 helper。正式 tests
`native_function_keys_tests.cpp` 覆盖F1～F12表、repeat、maintenance、context、lock/count/pending/reset与Host transition。

## 2. 严格路由优先级

`route_native_function_key28` 的判定顺序固定为：

1. 非F1～F12：`not_function_key`。
2. F11/F12：直接成为 `continuous_host_command`；pure route层绕过repeat和battle/context拒绝。
3. F1～F10 repeat：`rejected_auto_repeat / auto_repeat`。
4. 非repeat `Ctrl+F9`/`Ctrl+F10`：recording maintenance；在battle/context gate之前。
5. 非battle：`rejected_by_context / not_in_battle`。
6. main state不允许：`rejected_by_context / main_state_disallows`。
7. F6～F9且F3 lock active：`rejected_by_context / f3_locked`。
8. F8/F9且global delay未清：`rejected_by_context / global_delay_active`。
9. 按键表分发；普通F10为 `native_no_action`。

正式 `main.cpp` 有两个适配细节：

- `WM_KEYDOWN`外层已经过滤repeat，所以one-shot route实参恒为`false`；repeat规则仍是pure contract和tests的一部分。
- physical route只提供真实battle-scene与当前F3 lock；`main_state_allows`、`global_delay_clear`当前传`true`。Session在
  tick开头按BattleFlow active/timer0/mode0重新检查main state；当前正式调用仍把global-delay参数传`true`。因此
  global-delay rejection是已定义但当前live caller尚无动态producer的seam，不得伪造“正式EXE已观察到delay拒绝”。

## 3. 逐键矩阵

| Key | route结果 / command | route与Session gate | 接受后的状态或Host副作用 | 最终效果时点 | Unity现状 | Owner / 处理结论 |
|---|---|---|---|---|---|---|
| F1 | Host / `toggle_pause` | one-shot；battle/main context | `paused=!paused` | message时立即 | B1已有physical edge latch与shared paused state | `B1_DONE`；B2统一route只转接既有owner，不重写 |
| F2 | Host / `single_step` | one-shot；battle/main context | 仅当前已paused时request exactly one step；running不积压 | message后由paused Host推进 | B1已有paused-only one-step | `B1_DONE`；只转接 |
| F3 | Session / `toggle_lock` | tick时若lock已2则拒绝 | 首次接受单向写`lock_state=2`；无解锁切换；event bit `0x04` | tick开头 | 无正式carrier/route | B2 route+carrier+integration |
| F4 | Host / `leave_battle` | one-shot；battle/main context | request guarded close；formal复用与Esc/窗口关闭同一recording保护 | message时请求关闭/离开 | 无统一route；Unity已有其他battle exit机制但非F4接线 | B2产出handoff，实际leave/scene flow归B8 |
| F5 | Host / `toggle_fast_mode` | one-shot；battle/main context | toggles fast；interval reset；33ms↔3ms | message时立即 | B1已有F5 cadence toggle | `B1_DONE`；只转接 |
| F6 | Session / `toggle_hit_resource` | F3 lock拒绝；不受global delay gate | count F6++；toggle hit-resource；立刻project rules/entities | tick开头Session dispatch | 无正式route/state；现有hit resource字段来自配置 | B2 carrier/integration；rule consumer验证归B5，表现/资源归B8/B11 |
| F7 | Session / `fill_active_mp` | F3 lock拒绝；不受global delay gate | count F7++；`pending_full_mp=true` | simulation tick后遍历active slots，仅`current_mp=500`，再clear | 旧latch toggle `InitStatsRequest`；Entity postframe写HP3/HPBound/HP/PP全500并清exit countdown | 这是明确语义差异；B2 carrier/integration，精确tail placement归B3，旧效果替换/验收归B8 |
| F8 | Session / `drop_mode_objects` | F3 lock；定义global-delay gate，当前live caller传clear | count F8++；shared pending=`drop_objects` | simulation tick后消费D1～D4 RNG并生成mode objects | 旧Mode2Request=1，legacy随机武器链；受Config allow-list | B2 carrier/integration；pass/effect归B3+B8，catalog归B11，D1～D4见NONAI RNG manifest |
| F9 | Session / `terminate_mode_objects` | F3 lock；定义global-delay gate，当前live caller传clear | count F9++；shared pending=`terminate_objects` | simulation tick后令eligible object `weapon_hp_31c=-1` | 旧Mode2Request=2，清weapon picker；受Config allow-list | B2 carrier/integration；effect归B8/B11 |
| F10 | `native_no_action` | 仍受repeat/battle/main context；plain F10不是command | 无 | 无 | 无统一route | B2 route contract必须保留no-op，不能映射调试功能 |
| F11 | continuous Host / `volume_down` | pure route绕过repeat/context；真实Host读取held state | 每Host loop volume -1；overlay=100 | simulation之外的Host/audio loop | 未接 | B2只分类/held handoff，音频与overlay归B10 |
| F12 | continuous Host / `volume_up` | 同F11；F11+F12同时held时F12赢 | 每Host loop volume +1；overlay=100 | Host/audio loop | 未接 | B2只分类/held handoff，音频与overlay归B10 |

## 4. Maintenance、event byte 与同窗口顺序

- `Ctrl+F9`=`retry_pending_recording`，`Ctrl+F10`=`discard_pending_recording`；它们不是battle Session事件，并且
  maintenance判定早于battle/main context。当前Unity没有formal LFR `save_pending` recording adapter；按B2 I09结论，
  route可以保留分类，但production effect暂不接。若B12要求直接导入/恢复LFR，必须另立recording adapter包。
- Session bit为F3 `0x04`、F6 `0x10`、F7 `0x20`、F8 `0x40`、F9 `0x80`，known mask=`0xF4`。
- 同一tick前相同按键多次只保留一个bit，不累加次数；计数只在tick开头accepted时递增。
- bit消费顺序固定F3→F6→F7→F8→F9。若F3与F6～F9同窗到达，F3先锁，后四项被Session gate拒绝。
- runtime state primitive的F8/F9是“最后accepted command赢”；但正式Win32→byte路径会先固定分发F8再F9，故同一
  pre-tick窗口两bit同时存在时总是F9 pending胜出，与物理消息先后无关。跨tick则最后accepted且尚未tail消费者胜出。
- loading active时queued byte与last-event byte清零，不执行Session effect。

## 5. Unity结构差异

| Unity路径 | 已观察事实 | 差异/风险 |
|---|---|---|
| `SimulationTickDriver.CaptureHostControlEdges` | F1/F2/F5只在LocalFreeRun采physical edge | B1 owner可复用，但目前未经过统一F1～F12 route/context result |
| `BattleFunctionKeyInputLatch` | 只在LocalFreeRun读取F7/F8/F9；Config exact-mode allow-list；F7 parity-fold、F8/F9 latest int | 没有F3 lock/F6/F10/F11/F12；无route reject reason/event counts；F7 parity并非Authority one-bit queue |
| `ApplyPendingBattleFunctionKeyCommandsForTick` | tick前直接改`InitStatsRequest`/`Mode2Request` | 没有Authority queue-byte固定dispatch与Session二次gate |
| `EntityPostFrameTailAll` | `InitStatsRequest`会把HP3/HPBound/HP/PP全部设500 | Authority F7只在整个simulation tick之后写current MP500；可观察差异明确 |
| `BattleRandomWeaponDropModule.RunMode2Tail` | `Mode2Request`驱动旧spawn/clear链 | 不是Authority D1～D4/eligible规则证明；必须随B3/B8 owner迁移，不能在B2换名冒充完成 |
| core snapshot/checksum | 已覆盖旧`InitStatsRequest`/`Mode2Request` | 新lock/hit-resource/count/pending/event-byte必须显式进入reset/snapshot/checksum；不能复用错误语义字段 |
| `GameConfig.asset` | mode0/battle1 allow F7/F8/F9 | Authority gate来自active battle/BattleFlow/mode/lock；Config allow-list是旧策略，不应继续定义正式按键规则 |
| old tests/probe | 验证旧F7全属性、旧F8 spawn、旧F9 picker clear | 需保留为历史证据后重基线/替换，不能拿旧green证明2.8对齐 |

## 6. 最小实施拆包

1. `NTSD28-B2-FUNCTION-KEY-ROUTE-CONTRACT-001`：新建纯C# logical key/modifier/context/result/enums/router；覆盖非F、
   F1～F12、repeat、maintenance-before-context、F11/F12 bypass、F6～F9 lock、F8/F9 delay、F10 no-op。只分类命令，
   不复制B1 Host transition，不读取Keyboard，不改Config/Scene/Input Actions。
2. `NTSD28-B2-FUNCTION-KEY-SESSION-STATE-CARRIER-001`：新增lock/hit-resource/count/pending flags/shared command、queued/
   accepted event byte与reset/copy/snapshot/checksum；pure apply/dispatch测试覆盖F3 first-wins、固定bit顺序、同bit折叠、
   F8/F9 same-window F9 wins。下游effect仍不执行。
3. `NTSD28-B2-FUNCTION-KEY-PRODUCTION-INTEGRATION-001`：让LocalFreeRun physical/diagnostic入口统一走router；F1/F2/F5
   适配到既有B1 Host command owner；Session只queue、在tick边界dispatch；F4/F6～F9/F11/F12发布typed handoff。
   删除旧Config allow-list对正式rule的所有权，但不在本包实现B8/B10效果。
4. B3：确定Session dispatch和post-tick tail相对native pass骨架的唯一位置；F7/F8/F9 effect不得在骨架之前运行。
5. B5/B8/B10/B11：分别完成F6 hit-resource消费、F4/F7/F8/F9效果、F11/F12 audio/overlay与content catalog；B12做
   physical Play+formal observable parity。

唯一下一包：`NTSD28-B2-FUNCTION-KEY-ROUTE-CONTRACT-001`。验收以test-first missing-type red、纯路由全矩阵green、
相关B1 Host tests不回归、compile0、full SelfCheck与Change Ledger validator为准；production physical行为保持不变。

