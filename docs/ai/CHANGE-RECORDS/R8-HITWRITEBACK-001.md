# R8-HITWRITEBACK-001 — actual hit producer to RenderDispatch writeback Play witness

<!-- CHANGE-RECORD
id: R8-HITWRITEBACK-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitRecordWritebackPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:2061-2083 and src\render\renderer.cpp:687-758,1300-1438 / USER-APPROVED-R8-WP01G-R07A
evidence: CPP-RELEASE-SOURCE-CROSSWALK / UNITY-FRESH-COMPILE-0-ERROR / PRODUCTION-WORKER-PLAY-4-TICK-PASS / WORKER-18-OF-18 / HIT-178-OF-178 / CENTRAL-13-OF-13 / FULL-SELFCHECK-PASS / FINAL-CONSOLE-0 / LEDGER-PASS
-->

> 创建日期：2026-08-23  
> 当前状态：`VERIFIED`  
> 所属：`R8-WP01G-R07A`

## Requirement / authority

C++ Release在PreFrame/Stage后进入render，在`draw_hit_records`中使用本次render前的age选择spark，再对可画
record推进一次，invalid tail每cycle最多移除一个；FramePostProcess与LateEntityUpdate随后执行。下一tick
kind0 hit-record容量门和成功append的两次RNG因此依赖render-pass内writeback，不是任意Late视觉副作用。

Unity现有source把publication/no-publication writeback放在`NTSDBattleTickSystem.RenderDispatch`，existing
self-check覆盖exact矩阵，但没有“actual collision/hit producer→完整production tick→frozen sample→same-tick
live writeback→Late幂等→next-tick append/RNG”的结构化Play证据。

## Planned file / symbols

- `Assets/NTSD/Scripts/Test/Editor/BattleHitRecordWritebackPlayModeProbeEditor.cs`：Editor-only联合Play探针；
  只在安全fixture初始边界注册一对带正式kind0 itr/body的数据对象，由`SimulationTickDriver.StepOneTick`完整
  推进，不直接调用candidate/hit writer/finalizer/advance制造结果。

## Protected boundaries

- C++ authority只读；
- production gameplay、pass order、RNG、worker、checksum、FrameInputSet、30Hz不改；
- CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5× scale、fixed camera、扩展容量、pool/0GC不改；
- 不进入R07B/R07C、AI、P1/P2、T8、IL2CPP、Android或服务器；
- probe不得直接写HitRecordCount/age、candidate、伤害结果或调用finalizer/advance制造PASS。

## Expected evidence

- 实际碰撞在完整production tick产生一个kind0 hit record并消费exact两次RNG；
- published cycle和central command保留advance前age，live owner在tick返回时为age+1；
- 随后的Unity Late fallback不重复推进；
- 下一完整tick可追加新record，旧/新record均按各自frozen age推进且RNG再次exact+2；
- warmed presentation tick 0 managed allocation；
- cleanup恢复world/slot/pool/stat/sound/RNG/backend/driver pause，不保留probe entity或stale presentation owner；
- worker/no-publication/invalid/full-capacity矩阵由现有exact tests和self-check共同闭合。

## Existing baseline

focused job `7ec88f1aa50f4f93af44990ad9a08dd6`为2/2 PASS：

- `WorkerPresentationUsesSealedPureHitRecordLifecycleCatalog`；
- `FullKind0HitRecordCapacity_DoesNotAdvanceBattleRng`。

MCP stdio退出时的`anyio.BrokenResourceError`发生在Unity job已返回`succeeded`之后，属于工具关闭噪声。

## Actual implementation / current verification

- 已新增`Assets/NTSD/Scripts/Test/Editor/BattleHitRecordWritebackPlayModeProbeEditor.cs`；
- probe只通过正式production tick让authored kind0 itr命中authored body，不直接调用candidate/hit writer、
  hit-record finalizer或no-publication advance；
- 覆盖三个published tick和一个no-publication tick，记录frozen command/sample、live writeback、owner
  stable id/slot/generation、RNG exact call delta、Late幂等与cleanup恢复；
- 第一次scripts-only refresh没有生成新文件`.meta`，其Console 0 error不覆盖本probe，已撤销该无效证据；
- full asset refresh首次实际导入后得到`CS0102`：静态样本数组和嵌套报告类型同名`TickEvidence`；
- 将样本数组改名`TickSamples`后，下一轮实际编译发现`CS0165`：短路条件中的`out victimHandle`
  未满足C# definite-assignment；
- 两个错误均属于本test-only probe，尚未修改production；修复并fresh compile前状态保持`CODE_WRITTEN`；
- 已把数组改名`TickSamples`，并在短路条件前显式初始化`victimHandle`；随后fresh scripts compile
  Console为`0 error`，当前状态推进为`COMPILE_PASS`；
- 尚未执行本probe的Play报告，不能标为运行时通过。

## First Play findings and correction boundary

- 第一次菜单调用发生在`tick 0`，生产HitRecord lifecycle/Spark尚未就绪；等待场景加载后原样重跑，确认
  这是调用过早，不是production first-difference；
- 第二次于`startTick=1510`进入正式worker路径，tick1511实际产生HitRecord，cleanup完整恢复world/slot/pool、
  RNG、stats、sounds、presentation owner与pause；
- 报告失败在probe要求`GetHitRecordLastAdvanceTickForSnapshot(index)==expectedTick`；只读复核
  `FinalizePublishedHitRecordCycle`确认正式表现writeback调用`AdvanceHitRecordFromPresentation`，C++合同要求
  age/tail推进，但不要求Unity诊断字段`LastAdvanceTick`变化；该诊断字段只由另一条`AdvanceHitRecord`
  API维护；
- 因此该断言属于test-only probe越界假设。修正只删除这条非权威字段断言，仍保留live age、frozen age、
  cycle、Late幂等、RNG exact与cleanup的强验收；production不修改。修正并重跑前状态为`RUNTIME_PENDING`。
- 删除越界字段断言后的下一轮worker Play于`startTick=1049`通过actual hit、age writeback、owner/cycle与RNG
  前置检查，但probe随后错误要求worker的`PublishedFrame.CommandsMaterialized=true`；
- existing worker boundary contract明确worker publication保持纯冻结且`CommandsMaterialized=false`，中央宿主稍后
  复制到`CurrentPixelFramePlan.CapturedFrame`并物化commands。下一修正将等待正式中央宿主完成后检查该captured
  frame，不调用self-check专用materializer，不修改production。
- 改为等待正式central captured frame后的下一轮Play已取得tick1018完整PASS样本：actual hit、RNG+2、
  frozen age`[0]`、live age`[1]`、central hit command与Late幂等全部成立；
- tick1019同一pair只保留1条record而没有追加，符合正式hit-rest可能抑制连续同pair命中的边界。probe不能为
  追求第二条record而清理rest或直接写hit；下一修正仅在fixture加载阶段预建4个攻击者，每tick把一个新pair
  放到命中位置，仍由正式collision/hit链产生record，production不改。
- 4个攻击者轮换但共享同一victim的下一轮Play仍在第二tick保持1条record，说明受击者侧状态/交互资格也会
  抑制立即重复命中；继续共享victim无法作为跨tick append夹具；
- 下一修正使用4组预建attacker/victim pair，每tick只把一组置于碰撞位置。每个record仍由正式链产生，旧
  victim保留在world并继续由每次render/no-publication推进；不清rest、不重置受击状态、不直接写record。
- 4-pair fixture首次编译暴露test-only `VictimGenerations`误声明为`int[]`，而runtime handle generation与
  report字段为`uint`，产生两处CS0266；修正为`uint[]`后重编，production未改。

## Final verification

- fresh compile：0 error；
- production worker Play：`Temp/NTSD_R8_WP01G_R07A_HitRecordWriteback.result.json` PASS；tick843～846
  的published/no-publication、RNG exact、frozen/live age、central command、Late幂等和cleanup全部通过；
- focused：worker 18/18、hit execution 178/178、central materialization 13/13 PASS；
- full self-check：2026-08-23 20:25:11 PASS；
- Play结束前Console error0；self-check两条预期negative-path error清理后final Console error0；
- direct-call禁用扫描0；ledger validator 82 records/97 governed files PASS；scoped diff-check PASS；
- 完整证据见`RESEARCH/R8-WP01G-R07A-render-writeback-joint-runtime-evidence-20260823.md`。

本Record的`VERIFIED`只指test-only witness已验证；D-ID最高结论仍是
`UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。

## Acceptance / rollback

以R07A Task Contract为准。回滚只删除本probe及其`.meta`并把本Record标为`ROLLED_BACK`；不得回退既有
R6 presentation实现、R4/R8 hit实现或用户工作树。发现production first difference时本Change停止在
`BLOCKED`或`RUNTIME_PENDING`，另建最小production修复Task/Change，不能在probe中顺手修改。
