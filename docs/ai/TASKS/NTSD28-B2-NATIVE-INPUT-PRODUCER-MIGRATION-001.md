# Task Contract — NTSD28-B2-NATIVE-INPUT-PRODUCER-MIGRATION-001

> 状态：`FOCUSED_TEST_PASS / PRODUCTION_TWO_PASS_READY / REAL_PLAY_DDJ_DRA_PASS / JOINT_TRACE_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

把DataOriented production输入从“human/AI producer提前推进旧edge/history/combo”迁移为2.8两遍合同：
第一遍只采样/决策并冻结current/previous，第二遍先完成0x21 proxy copy，再对每个character DAT统一
执行一次native edge/history/combo10，最后投影兼容镜像供现有动作resolver消费。

## Authority 与当前事实

- authority `simulation_tick_driver.cpp:351-508`：全slot producer/sample/record完成后，第二遍先proxy
  copy，再调用`InputRouter28::step_sampled`。
- authority `input_routing.cpp:1504-1574`：sample与edge/combo分离；edge decay、rising history、combo10
  都在proxy之后且每slot一次。
- Unity `NTSDInputStateModule.UpdateFromBuffer`当前在human poll内执行cooldown decay+new edges；
  synchronized `AiDecisionKernel`也在candidate内部执行旧`ApplyInputEdges`。
- `NTSD28InputTwoPassModule.FreezeProducerState`当前把旧edge/combo覆盖回exact block，第二遍只在proxy
  成功时投影exact→legacy；非proxy角色没有production native processing。
- production `Assets/NTSD/Config/GameConfig/GameConfig.asset`选择`DataOrientedCanonical`；
  `LegacyCanonical`保留为显式兼容/回退路径，不得被本包无意改变。

## 允许修改

- `Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs`
- `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs`
- `Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeComboStateMachine.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeInputProducerMigrationEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅重基线本包改变的dead type-0合同）
- `Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs`（只补Editor request触发、live-battle readiness gate、旧结果作废与队列完成后退出Play）
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改authority、Config/DAT、Scene/Prefab/ProjectSettings、RNG call-sites、主pass顺序、动作字段语义、
hit/collision、proxy control生命周期或内容策略。

## 不变量

- DataOriented human poll只更新previous/current；在第二遍前edge window/history/combo10不得推进。
- synchronized AI candidate只写producer buttons/直接combo请求；不得提前推进legacy edge/history。
- 第二遍顺序必须是proxy copy→native process once→exact-to-legacy projection→现有routing。
- 第二遍对HP≤0的type-0先清current/previous/edge/combo/history，禁止死亡输入残留进入动作层。
- held input下一tick只衰减一次，不能重复生成history。
- 初次进入DataOriented的runtime native history tail为`-1`；battle-entry clear后必须恢复同一初始态。
- LegacyCanonical与unregistered兼容入口维持旧行为。
- 不把exact combo action field应用、`hit_ja`特殊gate或B2 joint trace伪报为本包完成。

## 验收

- test-first覆盖human producer前/第二遍后边界、held next tick单次decay、synchronized AI defer、
  LegacyCanonical不变、初始/clear history、dead type-0 suppression和proxy-before-process；
- compile0、focused、B2/AI相关回归、full SelfCheck、Console0、zero-allocation热路径与Ledger PASS；
- 真实Play至少复验一个human方向/动作输入不丢失；完整combo action留紧邻包。

## 回滚

恢复human/AI producer内旧edge推进及第二遍仅proxy投影；移除DataOriented native process接线。
不回滚已完成的exact carrier/combo core、two-pass骨架或dual RNG。

## 当前结果

- DataOriented human poll只更新sampled previous/current；synchronized AI candidate不再提前推进
  legacy edge/history。
- 第一遍freeze保留exact edge/combo bank并合并AI直接combo请求；第二遍按proxy→native process once→
  exact-to-legacy projection→routing执行。LegacyCanonical仍保持旧eager路径。
- DataOriented注册与battle-entry clear把native history tail初始化为`-1`；dead type-0在第二遍清空
  current/previous/edge/combo/tail/history，再投影到兼容镜像。
- test-first job `ba6be18ff43246c6bd1dbfb0a2768950`：5项中4个预期失败、Legacy 1项通过；
  第一轮 `48a32b06652149468449668f80066625` 5/5；NTSD28/B2 broad
  `6c68108fb962432b902a9bd6ee42af05` 239/239；AI相关
  `8197f240369b42728c3a889e38127614` 367/367；最终含dead/AI combo/zero-allocation的
  `3c5670e1fd8248a0ac12bdd6f913bf97` 8/8。
- full SelfCheck先后暴露dead suppression缺失和旧profile-independent assertion；修正后
  2026-09-03 12:11:48 PASS。`git diff --check`与Change Ledger 129/77通过。
- 真实Play已进入且`NTSD_Battle` bootstrap完成，但bridge在Play域停止监听；为既有combo probe新增
  Editor-only request poller、live-battle readiness gate、消费时旧结果作废和request批次完成后自动
  退出Play；Unity未在当前Play中自动reload。DDJ/DRA request已排队，必须在退出当前Play并完成脚本
  reload后再进入Play，才能取得新鲜结果。
- 当时因最新request helper尚未编译、真实Play结果未取得，本包曾保持`CODE_WRITTEN`；该阻塞已由
  用户退出旧Play解除，不能继续当作当前状态。
- 用户退出旧Play后，readiness helper于13:00:25编译且Console Error0。首轮新鲜Play在13:03:46/
  13:03:47得到DDJ/DRA两个FAIL：第一键exact state1和第二键edge均存在，但probe仍读取只投影终态的
  legacy `ComboD*`，没有观察到exact中间态2，因而未发送第三键。已改为直接观察exact combo[5]/[0]
  并兼容DLA中间态3；这是probe观察层修正，production未改，等待重新编译/复跑。
- exact-observation helper于13:06:41编译Error0。第二轮13:10:02得到DDJ PASS：exact `1→2→3`、
  Frame271、三个physical step各1次。紧随其后的DRA在DDJ后续Frame274启动，该帧`hit_Fa=0`，因此
  以precondition FAIL退出，未进入输入行为。批次poller现按probe kind等待当前帧声明对应
  `hit_Dj/hit_Fa`后才消费请求；等待重新编译并整批复跑。
- per-kind gate于13:12:20编译Error0。最终整批结果：DDJ 13:15:19 PASS，exact `1→2→3`、
  Frame271、ticks `2/4/15`、attempts `1/1/1`；DRA 13:15:20 PASS，exact `1→2→4`、
  Frame263、ticks `40/42/53`、attempts `1/1/1`。请求均消费，批次自动退出Play并恢复idle。
- 最终full SelfCheck结果于13:19:28写入`PASS`，日志同时记录自检通过/完成；桥接30秒响应超时不等于
  自检失败。清除预期负向夹具后Console Error0。
- 最终focused job `e0c1c10607fd48c28e13b8c2f5e83e87`为8/8；错误namespace过滤job
  `5f5c61558ffc4a0c97e5f55a51812916`为0 tests，明确不计入验收。B2 action fields与joint trace仍待。
