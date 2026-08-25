# R6-PRES-02 — shadow current DAT identity 预检

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（difference fixed；fresh compile/full self-check PASS）  
> 对应：`D-RENDER-004`  
> Change ID：`R6-PRES-002`

## 1. C++ authority

- `Makefile:11-35` 将 `src/render/renderer.cpp` 和相关entity source纳入release `SRCS`；
- `renderer.cpp:517-531` 的 `draw_shadow` 读取 `entity_core_runtime(e).char_data`，并用当前
  `char_data->oid` 判断 `{223,224}` 不画shadow；
- C++ dynamic identity writer通过替换`char_data`改变当前DAT，shadow不读取shell历史identity或初始spawn oid。

结论：shadow special OID gate的authority字段是“当前DAT oid”。

## 2. Unity current state

`BattlePresentationEntitySnapshot` 已同时冻结：

- `ObjectId`：Unity entity shell/runtime identity；
- `CurrentDatObjectId` / `VisualDataId`：`FrameCache.Wrapper.characterId`，没有wrapper时才fallback ObjectId。

body sprite resource正确使用`VisualDataId`。但`BattlePresentationCoordinator.BuildCommands`的shadow gate
当前使用`entity.ObjectId != 223 && entity.ObjectId != 224`，与C++ current DAT字段不一致。

## 3. Reachability / writer inventory

- `TryApplyRuntimeIdentity`、state transform、CPoint transform及owned-object propagation通常同时更新
  `ObjectId`与FrameCache wrapper；这些路径中两个字段相等；
- `LF2SpecialAttack` Karasu特殊分支存在只加载wrapper 209而不改ObjectId的production writer，证明
  shell identity与current DAT在架构上不是同一字段；209本身不触发223/224 gate，但不能据此把ObjectId
  提升为current DAT authority；
- existing P7 self-check已经构造两种反向identity：ObjectId=223/currentDAT=7300，以及
  ObjectId=7300/currentDAT=223。旧断言明确编码了Unity ObjectId gate，因此正好是可复用的first-difference fixture。

## 4. 最小修复

只改：

1. `BattlePresentationShadowBuild.cs`：shadow `{223,224}` gate从`entity.ObjectId`切到
   `entity.CurrentDatObjectId`；
2. `BattleRuntimeSelfCheck.cs`：反转两条identity fixture的shadow预期，并把消息改为current DAT语义。

不改snapshot字段、body resource、sorting、position、visibility、shader、mesh、catalog、camera或gameplay。

## 5. Acceptance

- ObjectId=223/currentDAT=7300：必须画shadow；
- ObjectId=7300/currentDAT=223：必须不画shadow；
- ObjectId=224/currentDAT=7300：必须画shadow；
- normal、state3005/state9997、negative link、hit-stop blink既有结果不变；
- render handoff前后battle checksum不变；
- fresh compile、full self-check与ledger validator通过。

## 6. 状态边界

该修复只能关闭D-RENDER-004到Unity自动证据层；C++ runtime trace、真实Play Mode/GPU像素仍待，
所以最高`RUNTIME_PENDING`。D-RENDER-001/002/005与R6其它子流程保持独立。

## 7. Actual evidence

- source `BattlePresentationShadowBuild.cs 18:15:26`、`BattleRuntimeSelfCheck.cs 18:15:29`；
- `Assembly-CSharp.dll 18:16:56`，Tundra build success 6.02s，filtered compile errors=0；
- `Temp/NTSD_BattleRuntimeSelfCheck.result 18:18:10`=`PASS`；
- P7 checksum isolation与normal/state/link/hit-stop矩阵均包含在本次full run；
- 17:49旧PASS未用于本改动。
