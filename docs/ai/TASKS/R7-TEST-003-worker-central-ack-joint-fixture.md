# R7-TEST-003 — worker / CentralOnly / acknowledgement joint fixture

> 日期：2026-08-23
> 状态：`VERIFIED / TEST-ONLY`

## Goal

补齐一条正式 driver 联合夹具，在同一个测试中证明：dedicated worker 以
`buildPresentation=true` 发布 frozen frame，host 消费后由 CentralOnly 物化相同 tick，
acknowledgement/finalization 释放 single-flight gate，随后下一 tick 可以提交且不会复用旧 frame 或 generation。

## Scope

- 使用 `SimulationTickDriver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics` 走正式 worker submission；
- 使用现有 publication consume、CentralOnly editor materialization、acknowledgement/finalization 边界；
- 断言 ack 前下一 tick 被拒绝、ack 后下一 tick可提交；
- 断言 tick2 publication、captured frame、materialized plan和generation均不复用tick1；
- 只修改 Editor test 与本 Task/Record/Ledger/STATE/diff register/handoff/主计划。

## Authority / Evidence

- C++ `src/entity/game_tick.cpp:945-948,2023-2087`：render observation point位于PreFrame/Stage之后、
  FramePostProcess/Late/tail之前；该文件参与release build；
- Unity production合同：`SimulationTickDriver` worker publication/ack single-flight、
  `BattlePresentationCoordinator.BeginSimulationWorkerFrame` frozen publication、
  `BattleCentralRenderSystem` latest publication/materialization gate；
- `D-TEST-003` 已确认现有测试只分别覆盖这些边界，缺少联合夹具。

## Files likely involved

- `Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs`
- `docs/ai/CHANGE-RECORDS/R7-TEST-003.md`
- `docs/ai/CHANGE-LEDGER.md`
- `docs/ai/STATE.md`
- `docs/ai/RESEARCH/R1-SOURCE-ALL-DIFF-REGISTER.md`
- `Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md`
- `docs/ai/HANDOFFS/HANDOFF-R7-TEST-003-worker-central-ack-joint-fixture.md`

## Deliverables

1. 一条 formal driver → frozen publication → CentralOnly materialization → ack → next tick fixture；
2. exact test、worker boundary class、central latest-frame class、Unity compile与fresh self-check证据；
3. `D-TEST-003` 更新为 automated joint coverage closed。

## Verification

- tick1 `buildPresentation=true`，driver消费后`PublishedFrame.TickIndex == 1`；
- tick1原始publication仍未materialize commands，而central captured clone已materialize且plan tick=1；
- ack前tick2 submission明确被single-flight gate拒绝；
- ack/finalization后tick2 submission成功；
- tick2 publication/frame/plan tick均为2，frame引用与plan generation不复用tick1；
- exact/class/central regression、Unity compile、fresh-domain full self-check和ledger validator通过。

## Stop conditions

- 联合链必须修改production API或改变worker/central/ack顺序；
- 需要放宽single-flight、`maxCatchUpTicksPerFrame`或改变presentation ownership；
- 发现publication tick/generation差异指向production bug，必须另建独立Change Record。

## Out of scope

- D-TEST-001 static pollution；
- worker pipeline、多outstanding publication或catch-up设计；
- render feature/材质/相机/像素验收；
- C++ runtime trace、Play Mode、R8与服务器代码。

## Result

- 新增formal driver双tick联合fixture，production脚本未修改；
- exact job `8f7e88df654449e38a6ac8df97bb6faa`：1/1 PASS；
- worker boundary + central latest-frame job `acfb083ac4fc458e999a9715b4f45dca`：31/31 PASS；
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：0 error；
- Unity force scripts compile：Console 0 error；
- focused tests后重新加载脚本域，2026-08-23 02:27:37 fresh `BattleRuntimeSelfCheck=PASS`；
- `D-TEST-003` 的自动联合覆盖已关闭；真实URP Play Mode、C++ trace与R8仍独立待验。
