# R7-TEST-003 — worker / CentralOnly / acknowledgement joint fixture

<!-- CHANGE-RECORD
id: R7-TEST-003
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp::game_tick render observation point + Unity production worker/central/ack adapter contract
evidence: SOURCE-CONTRACT + EXACT 1/1 + WORKER/CENTRAL 31/31 + COMPILE 0 ERROR + FRESH SELF-CHECK PASS
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / render / worker acceptance

## 1. 状态与范围

- 当前状态：`VERIFIED / TEST-ONLY`
- 所属 Work Package：R7 repair order 8 / D-TEST-003
- 不属于本次范围：production worker、driver、CentralOnly、renderer、pass顺序、catch-up、gameplay
- 关联 Change ID：R7-PRES-WORK-01、R7-TEST-002、R6-PRES-004、R6-PRES-005

## 2. Authority / 需求依据

- C++ release `src/entity/game_tick.cpp:945-948,2023-2087` 定义render observation point，且参与Makefile release build；
- Unity worker/central/ack是已批准适配边界，本Record只验证它们联合作业，不用Unity反定义C++ gameplay；
- Evidence等级：C++ source `VERIFIED`；Unity联合自动覆盖当前为`UNKNOWN/PENDING`。

## 3. Unity 原状与已确认差异

- worker boundary tests已分别覆盖submission/publication/ack；central tests已分别覆盖latest materialization；
- 没有单条formal driver fixture跨越worker frozen publication、CentralOnly exact-tick物化、ack和next-tick unblock；
- `D-TEST-003` 是验收覆盖缺口，不是已确认production行为差异。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs` | new joint Editor test | worker与central分段覆盖 | 一条链覆盖tick1 publication/materialize/ack和tick2 unblock/new generation |

## 5. 不可回退边界

- 不改CentralOnly、Texture2DArray、dynamic Mesh、URP或1.5×表现适配；
- 不改Authority400/MobileExtended/DesktopExtended容量合同；
- 不改30 Hz、FrameInputSet、slot/generation、SoA/ECS、对象池、worker single-flight或0 GC production边界；
- 不修改C++ authority或任何production脚本。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs` | `FormalLocalDriverPublishesCentralFramesAcknowledgesAndAdvancesNextTick` | 新增formal driver双tick联合夹具；反射只调用现有private host consume/ack边界；用现有CentralOnly editor self-check物化同tick | 仅增加Editor测试时间；production无行为变化 |

夹具先在tick1 ack前证明tick2 submission被single-flight拒绝；随后分别物化tick1/tick2，断言
原worker frozen publication不被materialized command反写，且tick2 publication引用和central plan generation均更新。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | `dotnet build Assembly-CSharp-Editor.csproj --no-restore` + Unity force scripts refresh | dotnet 0 error；Unity Console 0 error | `PASS` |
| focused exact | job `8f7e88df654449e38a6ac8df97bb6faa` | 1/1 PASS | `PASS` |
| worker + central regression | job `acfb083ac4fc458e999a9715b4f45dca` | 31/31 PASS | `PASS` |
| full self-check | focused后force scripts domain reload；2026-08-23 02:27:37 | `BattleRuntimeSelfCheck=PASS` | `PASS` |
| Play Mode / C++ trace | 不属于本test-only包 | 未运行 | `PENDING` |

## 8. 风险、回滚与未关闭项

- 已知风险：同域focused suites可能触发既有D-TEST-001静态污染，full self-check前必须scripts domain reload；
- 未关闭项：真实URP Play Mode、C++ runtime trace、R8；
- 回滚方式：仅删除新增测试方法与本Record的后续状态，production无需回滚。

## 9. Git / 交接

- 修改前工作树基线：大量既有用户/项目改动；目标测试文件已有R7-TEST-002的两条断言修正；不覆盖无关diff；
- 实际 diff 范围：一个新增Editor test方法与本包治理文档；production脚本无diff；
- 提交 hash：未提交；
- validator：`Change ledger validation PASSED`（53 Records / 51 governed code files）；
- 交接：`docs/ai/HANDOFFS/HANDOFF-R7-TEST-003-worker-central-ack-joint-fixture.md`。
