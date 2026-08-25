# R8-WP01G-R07B — central liveness / identity / visibility runtime evidence

> 日期：2026-08-23  
> 结论：`UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`  
> Change：`R8-RENDERLIVE-001 / VERIFIED / TEST-ONLY`

## 1. 范围与权威

- C++ Release只读依据：`src/entity/game_tick.cpp:1008-1154,2061-2083`、
  `src/render/renderer.cpp:517-685,1300-1438`；
- 本证据只关闭`D-RENDER-003`的pending/generation/T+1子集、`D-RENDER-004`和`D-RENDER-005`；
- OID7/8→51 dormant/split仍只归R08，R08完成前不得整体关闭`D-RENDER-003`；
- production gameplay、renderer、DAT、scene、URP、CentralOnly和protected adapter均为0改动；
- C++ executable/full trace仍受R1-WP02 blocker限制，因此不写成C++ runtime trace VERIFIED。

## 2. 联合Play报告

报告：`Temp/NTSD_R8_WP01G_R07B_CentralLivenessIdentityVisibility.result.json`

- 状态：`PASS`；tick `202 -> 203`；cleanup=`true`；
- 本轮场景中的dedicated worker已被先前诊断入口停止，因此联合probe走正式同步完整tick入口，
  `workerPath=false`；worker publication边界另由本次fresh 18/18 focused覆盖；
- pending正式fixture：OID225/frame51；old handle=`slot51/gen1`；
- FrameLogic产生`pendingFreeDelta=1`和`generationReleaseDelta=1`；
- Late opoint OID999复用同一`slot51`且generation推进到`gen2`；
- T帧：旧gen为`GenerationMismatch`，新gen为`MissingSnapshotEntity`，证明late child没有污染已冻结T；
- T+1：新gen具有snapshot、body command、shadow command和central submission；旧gen仍为
  `GenerationMismatch`；pool reuse body/shadow visibility恢复；
- world/object/logic pool/slot/driver pause均恢复到baseline。

## 3. current-DAT OID223/224与visibility

正式factory生成的OID223和OID224都进入不可变presentation snapshot：

| 对象 | current DAT | body | shadow snapshot | shadow command/submission | 判定 |
|---|---:|---|---|---|---|
| OID223 | 223 | snapshot/command/resource/submission均true | true，`ShadowVisible=true` | false / false，reason=`CommandSuppressed` | current-DAT gate正确，无shadow geometry/pixel owner |
| OID224 | 224 | snapshot/command/resource/submission均true | true，`ShadowVisible=true` | false / false，reason=`CommandSuppressed` | current-DAT gate正确，无shadow geometry/pixel owner |
| baseline正式角色 | 2 | snapshot/command/resource/submission均true | true | command/resource/submission均true | ordinary shadow control通过 |

该结果同时证明两点：

1. 223/224不是通过额外写`ShadowVisible=false`隐藏，而是在C++对应的current-DAT shadow gate处不生成命令；
2. 同一帧普通角色仍保留body和common-shadow，未观察到production visibility cache额外隐藏。

中央body/shadow命令已进入有效submission；既有WP01D-06/07已经证明同一CentralOnly command/segment
管线的Game/SceneView像素所有权。本包不重复创建新的隔离pixel reader；无shadow command即没有该对象的
shadow geometry/pixel owner。

## 4. 排序与不可变帧边界

- 正式223/224 DAT在tick内得到不同Z：223=`376`，224=`375`；实际base order分别`16`和`12`，符合
  `ZInt -> runtimeSlot -> stableId`比较器；
- focused test `CentralOnly_StableSlotRadixPreservesSignedZAndSlotTieBreakExtremes`通过，补足同Z时slot tie；
- 探针早期失败证明不能在同tick immutable plan发布后注入对象再要求旧快照纳入；最终实现改为独立
  identity tick，再执行pending/late tick，没有放宽production immutable-frame合同。

## 5. Fresh验证

| 验证 | 结果 |
|---|---|
| Unity fresh asset compile | 0 C# error；`Assembly-CSharp-Editor.dll`更新 |
| central materialization + begin-frame focused | job `f049ee1e4d65445f8a9075bc2ccc0e57`，24/24 PASS |
| W05 opoint + late slot capacity focused | job `1f7e4b5432af422a967489b9e40fa5f3`，9/9 PASS |
| dedicated worker boundary focused | job `60e2742eb1b7477486cf66d9cbb71bd1`，18/18 PASS |
| `BattleRuntimeSelfCheck` | 2026-08-23 21:47:26 `PASS` |
| final Console | clear后error 0；self-check负向夹具产生的预期日志不计入final Console |
| Change Ledger validator | 83 records / 98 governed code files，PASS |

## 6. 诚实结论

- `R8-WP01G-R07B`完成；
- `D-RENDER-003`仅pending/generation/T+1子集达到Unity联合S4，dormant/split仍待R08；
- `D-RENDER-004`与`D-RENDER-005`达到Unity联合S4；
- 没有发现需要production repair的first-difference；
- R07C、R08、R1-WP02、AI、T8、IL2CPP、Android、服务器均未由本包执行。
