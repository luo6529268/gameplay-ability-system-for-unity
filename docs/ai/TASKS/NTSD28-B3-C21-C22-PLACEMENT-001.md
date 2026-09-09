# Task Contract — NTSD28-B3-C21-C22-PLACEMENT-001

> 状态：`VERIFIED / C21-C22-PLACEMENT / TARGETED-PLAY-PASS / ALGORITHMS-PENDING-B4-B5-B8`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C21-C22`
> 依赖：`NTSD28-B3-RESIDUAL-SERIAL-TAIL-REHOME-001 / VERIFIED`

## 目标

把现有`PreFrameBounds`候选owner与`FramePostProcess`冲量owner移到current C20后、临时serial proxy前，恢复C21→C22→C25-proxy的生产相对顺序。

## 边界

- 只移动现有caller及phase occurrence；不改stage bounds、dynamic boundary、type3/object cull、knockback公式或ECS fast path。
- `CurrentWaveStage`是剧情波次逻辑，不冒充C21，本包不移动、不重写。
- C21完整字段/规则仍归B4/B8，C22公式仍归B5；临时serial仍待拆为C25逐slot事务。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- C06～C14 placement tests（仅修正serial sentinel index）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28ResidualSerialTailRehomeEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C21C22PlacementEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C21C22PlacementPlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅phase契约必要修正）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

## 验收

- phase为current C20→PreFrameBounds→FramePostProcess→serial proxy；full/partial仍32/4。
- serial可观察到C21边界写和C22冲量提交/累计清零。
- RenderDispatch不再早于C22；step-wait不能跳过C22。
- compile、focused、相关、SelfCheck、Play、Scene/Console门通过。

## 回滚

只把两个caller移回`RunPresentationAndCleanupPhase`原位；不得回退C01～C20或修改算法。

## 完成证据

- test-first job `8cf7d4da5ba44f7292f3aa415562c0a4`为0/2：serial仍看到X=-50，phase23仍为serial。
- production只把现有`PreFrameBounds`与`FramePostProcess` caller移到current C20后/serial前；算法、ECS path与`CurrentWaveStage`未改。
- Unity compile 0；focused job `40d3edd5bf0c44be994d10ec3986b06f`为2/2；C04～C22/actual job `2dbc3cb87816474a9a4badada6189c64`为46/46；bounds/impulse related job `5578f4ce5f0b469caffebe7997fdcd24`为21/21。
- SelfCheck首次准确捕获旧F1合同仍要求C22在step-wait后；合同更正为C22完成、C25/late仍冻结后，2026-09-05 05:28:56 +08 PASS。
- 真实Play tick6：slot50 X=-200→serial -100，Vx=10，HitCount/KnockbackVx=0；phase23～27=C21/C22/serial/Stage/Render；artifact SHA-256 `2E3A6570AB02B834320B86864F8F1A207E6F3DB9D8B81F220A6901469379AF8A`。
- cleanup PASS，Scene SHA/mtime不变，Play退出，Console 0；full/partial仍32/4。下一结构首差为C23/C24 single owners及C25 nested tail。
