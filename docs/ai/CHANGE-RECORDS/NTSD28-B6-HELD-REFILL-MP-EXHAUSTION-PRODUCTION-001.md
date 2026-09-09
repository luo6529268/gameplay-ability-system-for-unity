# NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldRefillMpExhaustionProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldRefillMpExhaustionPlayModeProbeEditor.cs
authority: NTSD 2.8-Logan BattleWorld28::settle_held_refill_objects OID122/123 refill and exhaustion branch; EXE B1E13AE1, closure 39DDDA15.
evidence: Goal2 validation-only, production unchanged. Fresh focused7/C09related2=9/9; NTSD28 category229/229; B5 name-group759/759. Real NTSD_Battle Play6/6 via current Sakura/Naruto DAT and production OPoint relation attachment plus full driver ticks5-20:26 refill samples,6 exhaustion events,exact child cap and one RNG per exhaustion,zero Vy,preserved nonzero Vz,PS.zz reset,action/counter/relation/weaponHP reset. Owned cleanup passes; one ambient OID150 slot52 allocation is proven by RunNormalDrop call stack. Exit Console0/Scene dirtyfalse/root13/SHA D18E75F7...A2F11 unchanged. Runtime22 warnings/0 errors; Editor104 warnings/0 errors. Fresh SelfCheck11:36:59Z remains blocked at unrelated GT06. HP baseMax/full OPoint materializer remain outside scope; Goal3 remains on hold. Final Ledger validation appended below.
-->

> 状态：`VERIFIED / FOCUSED_7_OF_7 / C09_RELATED_2_OF_2 / NTSD28_CATEGORY_229_OF_229 / B5_NAME_GROUP_759_OF_759 / TARGETED_PLAY_6_OF_6 / REFILL_SAMPLES_26 / EXHAUSTION_EVENTS_6 / BUILDS_0_ERROR / CONSOLE_0_ERROR / SCENE_UNCHANGED / SELFCHECK_BLOCKED_UNRELATED_GT06 / HP_BASEMAX_DEFERRED / GOAL3_USER_HOLD`

## 改前事实

- OID123 在 child HP<=0 时提前 return，与 Authority 无条件 MP-refill 分支不同。
- OID123 cap 误读 `KillCount`并把 holder MP 写成150；Authority 读 child
  `OrdinaryCreditGate2F4`并 cap child MP。
- OID122/123 exhaustion 已有正确的单次 random Vx，但误写 Vy=-8并清 Vz=0。
- OID122 HP refill 中与 authoritative baseMax 有关的 clamp 属 B11/H，本包不修改或宣称已闭合。

## 预期修改

- 新增 focused test，在不改 production 前固定上述差异。
- 只修 `LF2WeaponHeldStateResolver.ProcessDrinkConsumption()` 的 OID123 前置/cap 与
  共享 exhaustion motion。
- 必要时只更新 SelfCheck/Play probe 中直接冲突的旧期望。

## 验证状态

### 已写

- `LF2WeaponHeldStateResolver.ProcessDrinkConsumption()` 已取消OID123的HP<=0早退；holder MP仍按
  +3/500处理，child只在`OrdinaryCreditGate2F4 >= 0 && child PP > 150`时截到150，不再读
  `KillCount`或误写holder。
- OID122/123共享exhaustion现只draw一次`BattleRandInt(0, 7)`写Vx，Vy写0且不写Vz；显式清零
  双方`AttackingCounter`，既有consume release继续清action/relation与weapon flight/HP载体。
- 新增7-case focused Editor test；既有SelfCheck consume路径增加RNG delta、HP、Vx/Vy/Vz、双方
  action/counter与weapon flight断言。

### 已运行

- `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly`：
  47 warnings，0 errors。
- 将新test临时加入生成的`Assembly-CSharp-Editor.csproj`后运行同类build：104 warnings，0 errors；
  随后已移除临时Include，生成csproj无任务diff。
- 10项源码合同检查：10/10；scoped `git diff --check`：通过；
  `Tools/Validate-ChangeLedger.ps1`：PASS，361 records / 309 governed code files。

### 未运行 / 风险

Unity Test Runner、fresh SelfCheck、C09/C20 Play probe、Console与Scene dirty/hash均未运行。
既有Hub token/AppData sandbox许可阻塞没有外部变化，本轮未重复无效启动。因此7-case test只证明
编译，不证明断言已执行；状态保持`RUNTIME_PENDING`。OID122 authoritative baseMax clamp仍明确归
B11/H，不在本包完成声明内。

### 2026-09-09 恢复验证与暂停

- Unity job `91cebcc8fe354f1f888e5ed33b529ebd`中本包7-case focused全部通过；
  同job的唯一失败是positive-link phase退休后C09旧索引未同步，不是refill行为失败。
- 索引由其所属positive-link退休包修正后，job `edb6022909c5414499e9a5c2364c4c76`
  为`9/9`，其中本包focused `7/7`、C09 placement `2/2`。
- 当前两套Assembly build均0 error；full SelfCheck停在独立R2 exact-catch旧夹具。
- 尚未取得专门覆盖OID122/123 refill/exhaustion的稳定Play证据，用户随后要求暂停总体目标，
  因此本包保持`RUNTIME_PENDING / USER_HOLD`，不得提升为VERIFIED。

### 2026-09-09 Goal 2 validation-only恢复（探针修改前）

用户只解除本包Play验收的hold；production、SelfCheck与其他任务继续暂停。
本轮只新增`NTSD28B6HeldRefillMpExhaustionPlayModeProbeEditor.cs`及`.meta`，并更新本Task/Record、
Ledger、STATE、对齐总表与Temp结果；不修改本Record原有code-path代表的既有生产成果。
精确探针、生命周期/清理、验证与回滚合同已事前追加到Task。
开始时Scene实测SHA与用户要求的`D18E75F7...A2F11`完全一致。
旧C09探针只证明pose发布位置，不覆盖refill/exhaustion；新增探针以真实catalog、kind2拾取、
实际driver tick与`base.Act`前后采样补齐该证据，代码尚未写入，状态仍RUNTIME_PENDING。

探针已写：新增独立Editor request runner与观察型LF2Weapon子类，声明的六个case通过生产kind2
拾取建立关系，并使用正式DAT state17与实际driver tick推进；不覆盖physics或生产resolver。
逐pass记录与检查非正HP、cap/gate/KillCount、RNG/motion及exhaustion reset；finally回收自身实体。
尚未编译或Play，当前只记录CODE_WRITTEN证据，不提升包状态。

首轮编译通过(runtime22/editor104 warnings，均0 error)，focused+C09 job
`f93986ff88264b1ba811fe493461bef7`为9/9。首轮真实Play在tick5夹具ground-frame前置失败，
0个refill samples、无实体泄漏；退出Console0、Scene dirty=false/root13、指定SHA一致。
结果保留`Temp/NTSD28_B6_HeldRefillMpExhaustion.preflight-v1.json`。
Task已追加正式内容OPoint kind2入口更正；接下来只改新增探针的fixture setup，production不变。

v2真实Play：milk12、juice负gate5、juice零gate5、zeroHP1、negativeHP1共24次消费全部通过；
tick19第五组cleanup全场计数4/2→5/3，保守失败并停止第六组。退出后Console0、Scene SHA一致。
结果保存`Temp/NTSD28_B6_HeldRefillMpExhaustion.v2.json`。未观察到refill断言冲突，也未改production。
Task追加纯测试structural来源观察，下一次必须证明额外对象来自独立normal random-drop，或继续失败；
不得清理未知对象或无条件放宽计数。当前仍RUNTIME_PENDING。

v3专项Play于2026-09-09T11:30:34Z写出PASS：tick5→20，Sakura OID1/action242与Naruto
OID2/action291的正式DAT，六组26次消费/6次exhaustion全部通过。
额外对象由测试allocate调用栈确认是`BattleRandomWeaponDropModule.RunNormalDrop`在tick19
生成OID150/slot52；未清理该环境对象。探针自身角色/饮料的RegisteredWorldForSimulation均已清空，
对象/slot计数只增加这个已记录的环境对象，六组cleanup全部通过；退出Play执行正常runtime shutdown。
退出后Console error0、Scene dirty=false/root13、SHA与用户指定值一致，未恢复或保存Scene。
结果：`Temp/NTSD28_B6_HeldRefillMpExhaustion.result.json`，SHA-256
`4383004EDDFEBF70DA117733EAF545F969C493F84551EB695B774D96DCA86B5E`。
NTSD28分类回归job `b5a39e50c26c47e49fe2d3402dbbfc97`为229/229（0 failed/skipped）。
B5广回归与fresh SelfCheck仍在收尾，尚不提前推进VERIFIED。

B5名称组广回归job `e0d5600b1a354998a02525a327cf00d9`完成759/759，0 failed/skipped。
本轮没有运行stress或重新甄别其独立失败；fresh SelfCheck已通过既有request机制发起。

## Goal 2 最终验收（2026-09-09）

### 逐项Play结果

| 场景内测试序列 | 正式角色/frame | 消费次数 | 消费边界结果 |
|---|---|---:|---|
| OID122 milk HP12→0 | Sakura OID1/action242 | 12 | HP递减、每5/6点的既有HP/MP refill按样本通过；末次HP1→0，Vx=-2，Vy=0，Vz6.5保留。baseMax权威边界不在本包。 |
| OID123 HP10→0，gate=-1/KillCount=0 | Naruto OID2/action291 | 5 | 每次HP-2/holderMP+3，childPP500保持；末次Vx=2，Vz=-4.25保留。 |
| OID123 HP10→0，gate=0/KillCount=-1 | Naruto OID2/action291 | 5 | 首次childPP500→150，holderMP200→203，cap没有写holder；末次Vx=0，Vz3.75保留。 |
| OID123入场HP0 | Naruto OID2/action291 | 1 | HP0→-2，holderMP70→73，childPP400→150；Vx=1，Vy0，Vz9保留，PS.zz17→0。 |
| OID123入场HP-4 | Naruto OID2/action291 | 1 | HP-4→-6，holderMP70→73，gate=-1时childPP123保持；Vx=-2，Vy0，Vz-7.5保留，PS.zz17→0。 |
| OID123 HP4→0，gate=1，childPP150/holderMP499 | Naruto OID2/action291 | 2 | childPP150保持；holderMP仅按500上限处理；末次Vx=-1，Vy0，Vz5.25保留。 |

26个消费样本全部通过；20个non-exhaustion样本RNG delta0，6个exhaustion样本delta1且
Vx等于入场RNG状态预测的`NextInt(0,7)-3`。六次exhaustion即时采样均满足双方action/counter=0、
双方LinkState=0、holderTarget/childHolder=0、WeaponFlightCounter=0及PS.zz=0。
Runtime.Vz断言在实际Act调用前后比较，全部为非零sentinel，不把其他pass的运动算作消费副作用。

### 验证命令与结果

- Unity EditMode `testNames`指定本包focused+C09 placement：job
  `f93986ff88264b1ba811fe493461bef7`，9/9，0 failed/skipped。
- Unity EditMode `categoryNames=[NTSD28]`：job `b5a39e50c26c47e49fe2d3402dbbfc97`，
  229/229，覆盖其中B6/B5分类检查；不外推为所有NTSD28命名测试。
- Unity EditMode `groupNames=[NTSD28B5]`：job `e0d5600b1a354998a02525a327cf00d9`，
  759/759，0 failed/skipped；不包含stress独立支线。
- `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly`：
  exit0，22 warnings/0 errors（本轮incremental build实际输出）。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly`：
  最终探针源码exit0，104 warnings/0 errors。
- fresh SelfCheck通过既有`Temp/NTSD_BattleRuntimeSelfCheck.request`执行，
  `2026-09-09T11:36:59Z`最终仍FAIL于`GT-06 real character ... recover HP/PP ...`，
  `ExpectRecoveryFixture:25520 → CheckGameTickCurrentDatDispatchMatrix:25356`。
  结果保留`Temp/NTSD28_B6_HeldRefillMpExhaustion.SelfCheck.result`；没有修此断言。
- Play开始与结束都按instance核验Unity2022.3.62f3/NTSD_Battle，退出即时Console error0，
  Scene dirty=false/rootCount13；SHA-256始终为
  `D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11`，无需恢复/重载。

### 文件边界、关闭范围与停止

本轮增量只有新增探针及meta、本Task/Record、Ledger、STATE、对齐总表和Temp产物。
旧resolver、SelfCheck及其他production文件均未改；本Record历史code-path不能被解释为本轮改动。
Part A只读甄别结论不写仓库。本包只关闭既定held-refill精确子集，不认证完整OPoint materializer、
HP/baseMax内容、完整SelfCheck或整个B6。包状态推进VERIFIED后，所有后继工作仍等待用户复核。

最终`& ./Tools/Validate-ChangeLedger.ps1`：exit0，

```text
Change ledger validation PASSED.
  Records: 431
  Governed code files in diff: 374
Warnings: 531
```

531条WARNING为既有Record声明路径不在当前code diff，未改旧记录消除warning。
全量SelfCheck FAIL已保存到本包Temp结果；Play退出即时Console0证据与该独立GT06失败分开报告。

最终Console时点补充：专项Play退出及后续回归结束时均读取到error0。之后fresh SelfCheck在
非Play状态留下5条error日志：两条runtime rest bind registration rejected、一条mismatched rest
binding release refused、两条GT06失败日志；实际中止断言仍为GT06。读取后未清理这些日志，
因此不声称“fresh SelfCheck后Console仍0”。这五条不是Play期间新error，完整SelfCheck未通过的
事实保持。最终Scene磁盘SHA仍为指定值，增量哈希核对只有七个授权源码/治理文件，另有Temp产物。
