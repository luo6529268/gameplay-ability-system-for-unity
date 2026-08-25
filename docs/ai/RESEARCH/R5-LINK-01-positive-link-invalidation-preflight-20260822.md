# R5-LINK-01 — positive-link invalidation field-preservation preflight

> 日期：2026-08-22  
> 状态：`SOURCE_CONTRACT_VERIFIED / PLANNED`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1828-1845`。

## C++ release contract

`game_tick(...)` T11 在 CPoint/weapon sync 后、second negative-held pass 前，按slot升序遍历 active entity：

1. 只处理 `link_state > 0` 的 holder；
2. `target_idx` 越界、target inactive、或 `target.holder_idx != holderSlot` 时，只写
   `holder.link_state = 0`；
3. 该三个 invalid branch 都不写 `target_idx`、`held_weapon_slot`、target reverse holder 字段或其他link字段；
4. 下一轮 held loop仍按negative child的`holder_idx`和holder的`target_idx`读取关系，所以不能用“清理更彻底”
   代替原始字段保持。

`Makefile:32` 列入 `game_tick.cpp`，这是release live-source事实。

## Unity first difference

- `SimulationQueryAndLinkModule.RunLegacyPositiveLinkValidation:140-146`：invalid时写
  `LinkState=0` **并额外清** `TargetSlotIndex=-1`、`HeldWeaponStableId=-1`；
- `BattleEcsPositiveLinkValidationPass.ExecuteDataOriented:235-241` 与 `CaptureExpected:170-174` 复制了同一extra clear；
- default canonical mode是 `DataOriented`，但 `Legacy` / `ShadowCompare` 是诊断等价链，因此三者必须共享C++字段合同；
- 当前 `BattleRuntimeSelfCheck.CheckValidatePositiveLinksMatrix` 和 Editor pass test把extra clear当作预期，需随writer一起更正。

## Minimal repair boundary

仅允许：

1. invalid positive-link branch只写`LinkState=0`并刷新snapshot；
2. DataOriented expected/shadow assertion改为保留调用前的target/held字段；
3. 更新 existing self-check 和 `BattleEcsPositiveLinkValidationPassEditorTests` 的 invalid-target/out-of-range/mismatch/inactive
   witness。

禁止：

- 改negative child的`LinkState`/`HolderStableId`清理（`D-LINK-002`）；
- 改 CPoint、weapon sync、held first/second pass、release/drop、pool、slot generation、scheduler、render、DAT或C++；
- 改 `TargetSlotIndex` / `HeldWeaponStableId` 的创建语义或默认值；
- 因异常关系而引入额外自愈、扫描或allocation。

## Verification contract

- `BattleRuntimeSelfCheck`：valid link不变；越界/inactive/reciprocal mismatch仅LinkState归零且holder target/held字段保留；
- Editor tests：Legacy、ShadowCompare、DataOriented在kept/cleared的三个字段上严格相同，event witness显示preserved fields；
- Unity compile=0 error、full self-check PASS、focused EditMode test PASS、ledger/diff check PASS；
- 最多 `RUNTIME_PENDING`；C++ trace / target Play Mode未关闭。
