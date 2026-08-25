# R6-PRES-003 — Central shadow visibility cache current-DAT identity

<!-- CHANGE-RECORD
id: R6-PRES-003
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:/QQFile/NTSD2.4/ntsd_release/Makefile:11-35;src/render/renderer.cpp:517-556
evidence: SOURCE-CURRENT-CHAR-DATA-OID-VERIFIED / UNITY-PRODUCTION-SHADOW-CACHE-SHELL-IDENTITY-DIFFERENCE-FIXED / FIRST-SELF-CHECK-FIXTURE-PRECONDITION-FAILURE-RECORDED / FIXTURE-BINDING-WRITTEN / FRESH-RECOMPILE-PASS / FULL-SELF-CHECK-RERUN-PASS / PLAYMODE-AND-CPP-TRACE-PENDING
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 状态：`RUNTIME_PENDING`

## 1. Authority / requirement

C++ `Renderer::draw_shadow`用当前 `char_data->oid` 裁决223/224。Unity BuildCommands direct gate已在
R6-PRES-002修正，但 production `UpdateShadow/UpdateShadowManagedState` 仍用shell `ObjectId`写
`ShadowVisible`，可提前覆盖direct gate。本Record只闭合这条cache writer。

## 2. Unity before

- Legacy与Central managed shadow writer均向helper传shell `ObjectId`；
- snapshot冻结该 `ShadowVisible`，BuildCommands先用它作为额外gate；
- P7 inverse identity fixture没有先调用production managed writer，因此漏检。

## 3. Planned changes

| 文件 | 符号 | before | after |
|---|---|---|---|
| `LF2Entity.cs` | `UpdateShadow` / `UpdateShadowManagedState` | shell ObjectId | current DAT ObjectId |
| `LF2Entity.cs` | `ShouldHideShadowForPresentation`参数名 | objectId | currentDataObjectId |
| `BattleRuntimeSelfCheck.cs` | P7 identity matrix | default visibility直接capture | 先运行managed writer并断言cache+command |

## 4. Protected boundaries

- 不删除visibility schema，不改EntityVisible、body、snapshot layout、sort/order、mesh/shader/catalog/camera；
- 不改gameplay、pass ordering、C++ authority、scene、DAT、T8或容量；
- 不回退已批准Unity rendering/scale/performance边界。

## 5. Acceptance

- shell223/current7300与shell224/current7300 writer后cache可见且draw；
- shell7300/current223 writer后cache不可见且hide；
- P7其它gate/order/checksum继续通过；
- fresh compile/full self-check/validator/scoped diff PASS；
- PlayMode/C++ trace未取得时最高 `RUNTIME_PENDING`。

## 6. Actual changes / verification

| 文件 | 实际改动 | 当前状态 |
|---|---|---|
| `LF2Entity.cs` | Legacy/managed shadow writer均传入`ResolveCurrentDataObjectId(this)`；helper参数和223/224 gate明确为current DAT identity。 | fresh compile PASS |
| `BattleRuntimeSelfCheck.cs` | P7三条inverse identity绑定rendererless catalog sprite，在capture前执行production managed writer，并断言snapshot cache与command结果。 | fresh compile/full self-check PASS |

Fresh evidence：source `18:27:59/18:28:01` < Assembly-CSharp `18:29:31`；Tundra build
success 5.38s，9 items updated，Editor.log latest 3000 lines中 `error CS/Compilation failed`=0。
首次full self-check于18:31:32运行并失败：P7 inverse snapshot为
`object=7300,currentDat=223,shadowVisible=true`。诊断确认三条synthetic identity entity没有绑定
`LF2Sprite`，`UpdateShadowManagedState()`无法写cache，capture按null-sprite fallback返回true；该失败是
fixture前置缺失，不推翻production writer修复。下一步只为三条identity case绑定existing rendererless
catalog sprite后重编译/重跑；该binding现已写入，失败证据永久保留。此前18:29:31 DLL不能证明
18:32之后的fixture改动。现已fresh重编译：test source `18:32:36` < DLL `18:33:37`，
Tundra build success 2.66s、6 items updated、filtered errors=0；full self-check重跑待执行。

最终自动证据：`Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `18:35:48.011` 写入 `PASS`，晚于
fresh DLL `18:33:37.270`；P7 production-cache inverse identity、existing common shadow/order/checksum及
整套self-check均实际执行。第一次ledger validator失败于既有active `R6-PRES-002`没有继续出现在
STATE当前摘要；该文档登记缺失已补回，并在最终validator中重验。PlayMode/C++ trace仍未执行，故最高
`RUNTIME_PENDING`。

最终 `Tools/Validate-ChangeLedger.ps1`：PASS（41 Records / 30 governed code files）；task-scoped
`git diff --check`：PASS（仅现有LF→CRLF warning）。

## 7. Risks / pending

- `EntityVisible` production writer inventory当前未发现独立于C++ descriptor gate的可达false状态，
  本Record不删除它；新增production Hide writer必须重开D-RENDER-005；
- C++ trace与真实PlayMode/GPU可见仍待。

## 8. Rollback

只回滚本Record列出的两个脚本文件内本包diff及关联文档，不触碰用户工作树其它修改。
