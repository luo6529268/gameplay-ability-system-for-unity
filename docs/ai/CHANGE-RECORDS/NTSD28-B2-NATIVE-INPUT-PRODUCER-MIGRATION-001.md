# NTSD28-B2-NATIVE-INPUT-PRODUCER-MIGRATION-001 — production native input producer boundary

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-INPUT-PRODUCER-MIGRATION-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeComboStateMachine.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeInputProducerMigrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs
authority: NTSD 2.8-Logan simulation_tick_driver.cpp producer/sample first scan and input_routing.cpp proxy-before-step_sampled second scan.
evidence: SOURCE-CROSSWALK-CLOSED / RED-4-OF-5 / FIRST-5-OF-5 / BROAD-239-OF-239 / AI-367-OF-367 / FINAL-8-OF-8 / ZERO-ALLOCATION / DEAD-TYPE0-SUPPRESSION / FULL-SELFCHECK-2026-09-03T12:11:48-PASS / REQUEST-BATCH-POLLER-READINESS-EXACT-OBSERVATION-PER-KIND-AUTHORED-FIELD-GATES-STALE-RESULT-INVALIDATION-AUTO-EXIT / INTERMEDIATE-FRESH-PLAY-RED-PRESERVED / FINAL-HELPER-COMPILE-0-131220 / FINAL-DDJ-PASS-131519-EXACT-1-2-3-FRAME271-TICKS-2-4-15-ATTEMPTS-1-1-1 / FINAL-DRA-PASS-131520-EXACT-1-2-4-FRAME263-TICKS-40-42-53-ATTEMPTS-1-1-1 / AUTO-EXIT-IDLE-VERIFIED / FINAL-FULL-SELFCHECK-PASS-2026-09-03T13:19:28 / FINAL-FOCUSED-JOB-E0C1-8-OF-8 / ZERO-TEST-JOB-5F5C-EXCLUDED / CONSOLE-ERROR-0 / DIFF-CHECK-PASS / LEDGER-130-77-PASS / ACTION-FIELD-ROUTING-OUT-OF-SCOPE / JOINT-TRACE-PENDING
-->

> 状态：`FOCUSED_TEST_PASS / PRODUCTION_TWO_PASS_READY / REAL_PLAY_DDJ_DRA_PASS / JOINT_TRACE_PENDING`

## 改前事实

- human poll已在第一遍前完成legacy cooldown decrement/rising edge/history。
- synchronized AI witness内部多处分支调用legacy `ApplyInputEdges`。
- freeze将legacy edge/combo写回exact，第二遍非proxy角色不运行native state machine。
- production配置为DataOrientedCanonical；已有native state machine仍只有测试caller。
- 第一轮实现后full SelfCheck在`R3-AI-LIFE-01`捕获死亡type-0 `PrevJump=1`残留；authority
  `step_sampled`要求在任何edge/combo前完整清理死亡type-0输入派生态。
- 实现dead suppression后同一自检显示DataOriented已全清，但旧断言仍统一要求Legacy的
  `PrevJump=1/CdAttack=5`；需改为profile-specific authority expectation。
- 当前Unity bridge在Play域不监听；组合键probe没有其他Play probes已有的request-file poller，
  因而补Editor-only触发入口以完成真实场景验收，不触碰production。

## 预期改后职责

- DataOriented producer只写raw sampled state，第二遍统一且唯一推进native edge/history/combo。
- proxy仍在native processing前复制完整0x21 block。
- exact结果投影给现有action resolver；LegacyCanonical保持原路径。
- native combo field action reader与B2 joint trace另包。

## 验证记录

- red `ba6be18ff43246c6bd1dbfb0a2768950`：5 total、4 expected failed；命中human早edge、
  nonproxy无native process、history初始0、sync AI早edge，Legacy兼容项通过。
- 实施后 `48a32b06652149468449668f80066625` 5/5；NTSD28/B2 broad
  `6c68108fb962432b902a9bd6ee42af05` 239/239；AI相关
  `8197f240369b42728c3a889e38127614` 367/367。
- 扩充AI exact combo request、dead type-0与128 tick zero-allocation后，dead test先按预期1/8红；
  final `3c5670e1fd8248a0ac12bdd6f913bf97` 8/8。
- full SelfCheck 12:03:41暴露dead derived state未清；实现后12:08:47确认字段已清但旧断言仍要求
  Legacy结果；profile-specific修正后12:11:48 PASS。
- `git diff --check`通过；Change Ledger 129 records / 77 governed code files PASS。
- Play进入成功且battle bootstrap完成；bridge在Play域无listener。已给combo probe添加与既有probe
  一致的request poller、live-battle readiness gate、消费请求时旧结果作废、队列完成后自动退出Play，
  并排队DDJ/DRA；当前已退出旧Play，首轮helper编译Error 0，readiness gate新增后需再次reload。状态
  保持`CODE_WRITTEN`，新鲜Play证据取得后再推进。
- 交叉审计记录加入后，Change Ledger复验为130 records / 77 governed code files PASS；旧129计数保留
  为当时证据。
- 用户退出旧Play后，helper于13:00:25完成编译，活动场景正确且Console Error0。新鲜DDJ/DRA分别
  于13:03:46/13:03:47 FAIL；trace证明第一键exact state1和第二键edge已进入production，但probe仍
  从只投影terminal的legacy `ComboD*`等中间态2，故从未发送第三键。probe现直接读exact combo[5]/[0]
  并支持DLA中间态3；production code没有因该FAIL回退或改写，等待compile/rerun。
- exact helper于13:06:41编译Error0；13:10:02 DDJ PASS，exact state `1→2→3`、Frame271、三步
  physical input各一次。随后DRA在DDJ后续Frame274且`hit_Fa=0`时启动，属于零trace前置条件FAIL，
  不分类为production input失败。poller现按probe kind等待当前帧拥有对应authored field后才消费；
  production未改，整批复跑待完成。
- per-kind gate于13:12:20编译Error0；最终batch DDJ于13:15:19 PASS（exact1→2→3、Frame271、
  ticks2/4/15、attempts1/1/1），DRA于13:15:20 PASS（exact1→2→4、Frame263、ticks40/42/53、
  attempts1/1/1）。请求均消费，自动退出Play后Editor回到idle。
- 最终full SelfCheck于13:19:28写入PASS并记录完成；清除负向夹具后Console Error0。最终focused
  `e0c1c10607fd48c28e13b8c2f5e83e87` 8/8。错误namespace的`5f5c...`为0-test job，不计证据。
- 本包闭合producer两遍边界与真实human DDJ/DRA输入；exact action transaction、hold/direction、
  built-ins和B2双端joint trace仍由后续包负责，因此不把本状态扩大为B2整体已对齐。
