# R8-INP-01 — physical combo first-difference preflight

> 日期：2026-08-23
> 状态：`FIRST DIFFERENCE FOUND / D-INP-010 / NO CODE CHANGE`

## Observed fact

用户在当前 `NTSD_Battle` Play Mode 明确报告：角色无法通过按钮/按键组合释放技能。这是当前工作树的
真实运行时失败报告；尚未取得逐tick probe，因此不能把下列静态风险直接写成唯一根因。

## Closed source facts

1. `NTSDInputConfig.inputactions` 的 Player_1 物理绑定为 W/S/A/D + J Attack + K Jump + L Defend；
2. `CharacterInputModule` 把物理 action 映射到 NTSD 的交叉内部 key 字段，这是当前既有合同；
3. `LocalSimulationFrameInputProvider` 每次准备逻辑tick时只读取
   `ILocalFrameInputSource.CaptureHeldSimulationButtons()`；
4. `PressedButtons/ReleasedButtons` 由本次held与上次held之差生成；
5. `BeforeSimTick` 随后丢弃 `SimInputBuffer` 中该tick的直接InputAction callback packet；
6. dedicated worker是single-flight，并在publication/presentation acknowledgement完成前不采集下一tick输入；
7. 现有 `LocalFrameInputProviderEditorTests` 只覆盖“按住跨过采样点”的press/hold/release，不覆盖真实
   InputActionMap、低帧/worker in-flight期间的多次边沿或完整技能组合。

## C++ authority boundary

`input_handler.cpp:1555-1609` 在每个release game tick按current held state写key/prev并生成新按下cooldown；
组合wrapper随后消费这些字段。Unity必须在逻辑tick边界提供等价current-held/previous-held序列，不能让
输入采样频率退化为低频渲染/ack频率，也不能仅凭历史C#或单元测试定义正确性。

## Current first-difference candidates

| Candidate | Evidence | Status |
|---|---|---|
| physical asset绑定错误 | 当前asset静态映射W/S/A/D/J/K/L正确 | 暂不支持 |
| action map未实际绑定/启用 | source有bind/enable，但未取当前runtime probe | UNKNOWN |
| roster→controller route失败 | Play hierarchy有2个active实体；未取逐tickroute | UNKNOWN |
| worker single-flight使输入采样随低帧/ack稀疏 | source-confirmed architecture fact；与用户现象相容 | HIGH RISK / 未裁决 |
| callback edge在canonical held packet替换时丢失 | source-confirmed coverage gap；是否命中本次操作待probe | HIGH RISK / 未裁决 |
| combo resolver / DAT hit_*错误 | self-check只覆盖synthetic组合；真实角色未probe | UNKNOWN |

## Required next evidence

对同一角色与明确组合（优先采用用户既有Naruto防+下+跳/防+前+跳案例）记录：

1. InputAction performed/canceled sequence；
2. provider为每个逻辑tick生成的held/pressed/released；
3. roster slot→runtime slot绑定；
4. `Runtime.Key* / Prev* / Cd* / Combo* / frame`；
5. worker submission、publication、ack之间的tick间隔；
6. 首次差异点和最短重现步骤。

诊断必须默认关闭、预分配或仅Editor/Development启用，不得把日志/分配带入正式battle hot path。

## Fresh runtime preflight result

- 首次在Play Mode transition尚未结束时读取到`tick=0/object=0/paused=true`；随后通过
  `get_editor_state`确认当时`is_changing=true`，该瞬时值作废，不登记为bootstrap缺陷；
- transition完成后的fresh读取为：`CurrentTickIndex=681`、`ObjectCount=8`、Roster active=2，slot0/1
  正确绑定character2、runtime slot0/1、stable100/101，`paused=false`；
- dedicated worker active、无in-flight failure；`LastAppliedFrameInput` tick681包含player0/1且均neutral；
- CentralOnly当前pixel plan为valid/UsesCentralPixels=true，Console 0 error；
- 因此本轮已排除“战斗未启动、roster为空或全局暂停”作为稳定根因，断点仍需在真实按键进入后的
  InputAction→FrameInputSet→Runtime链中捕获。

## Non-mutating probe tooling boundary

- Codex sandbox进程无法枚举/聚焦用户桌面的Unity主窗口，系统级`SendInput`不能作为可重复自动证据；
- UnityMCP `execute_code`的Roslyn未安装，CodeDom因当前工程引用命令过长失败；未为此修改项目或MCP；
- 继续自动化需要新增一个Editor/Development-only input probe/fixture，属于项目脚本改动，必须先建立
  独立Change Record并按当前总计划等待实施确认；另一方案是用户实际按键时由MCP轮询，但证据重复性较弱。

## Authority follow-up — first difference found

继续只读C++ header/source后确认，physical mapping本身正确：C++ `DEFAULT_P1`明确按internal storage把
L/J/K交叉写入attack(+D3)/jump(+D1)/defend(+D2)。随后在Unity combo consumer发现更早、确定的
`D-INP-010`：C++ combo字段按引用即时持久化，Unity local transaction在绝大多数return前丢弃进度。
现有self-check还明确把跨tickNaruto L/S/K失败作为预期。故本轮无需先新增通用input probe就能建立最小
source repair合同；physical edge/worker风险保留为D-INP-006后续验收，不与D-INP-010混改。
