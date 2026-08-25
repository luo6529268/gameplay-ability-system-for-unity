# Handoff — R5-HOLD-02 type-2 held throw spawner preflight

> 日期：2026-08-22  
> Change ID：`R5-HOLD-002`  
> 当前状态：`RUNTIME_PENDING`（source preflight / Task Contract、最小writer/fixture、Unity compile与full self-check均已完成；C++ trace / Play Mode待验）  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1597-1630,1977-2006`。

## 已完成的只读工作

- 确认 `Makefile:32` 包含 `src/entity/game_tick.cpp`；未运行、构建、修改、复制或向 C++ authority 写任何内容；
- 确认第一、第二 held scan：type `1/4/6` 写 `spawner_slot=holder slot`，type `2` 不写；
- 确认 C++ `clear_released_held_slot` 只清 holder held slot / throw guard，不能解释或覆盖 type-2 的 spawner字段；
- 确认 Unity 只有 real `LF2WeaponHeldStateResolver.ThrowHeldWeapon` 在 held throw路径无条件写
  `SpawnerEntityIndex`；并记录两个Unity target-selection consumer；
- 建立 preflight、Task Contract、Change Record、ledger、STATE、总差异登记与主计划同步记录。

## 已写入的最小改动

- `LF2WeaponHeldStateResolver.Act`：type `1/4/6` 调用当前throw helper时传入`stampSpawnerSlot: true`，type `2`
  传入`false`；
- private throw helper只在stamp为真时写`SpawnerEntityIndex`；type2不清、不重置且不写该字段；
- existing real held self-check fixture新增type1/4/6 stamp断言，type2输入preseed sentinel `77`并断言保持；
- 未改`PickerStableId`、ReleaseTick、FrameDelay、PN/wait、random、OnThrown或任何target-selection reader。

## 已完成的 Unity 验证

- `Tools/Validate-ChangeLedger.ps1`：PASS（31个Change Record、25个已有governed脚本改动均有归属）；
- 本包范围 `git diff --check -- ...`：exit 0（仅LF→CRLF提示）；
- 当前已打开 Unity 2022.3.62f3：scripts refresh 后 `error CS`筛选为0；
- 菜单 `NTSD/验证/运行战斗运行时自检`：`Temp/NTSD_BattleRuntimeSelfCheck.result` 于
  2026-08-22 08:25:40 写入 `PASS`，existing real-held fixture已执行type1/4/6 stamp和type2 sentinel保持。

## 下一步（保持独立的后续工作）

在 `R5-HOLD-002` record 已存在的前提下：

1. 保持`R5-HOLD-002`作为等待C++ trace / Play Mode的证据包；不得再扩大它；
2. 按D-009可继续下一个独立source合同包；优先前必须重新评估`D-HOLD-003`或另一个R5差异的范围；
3. 本包最高状态仍是`RUNTIME_PENDING`。C++ trace和Play Mode仍不能声称完成。

## 已发现的独立项

`D-HOLD-003`：Unity shared helper还写 `PickerStableId=holder slot`，而当前 C++ type2 branch没有
`picker_idx` writer。它尚未完成pickup origin / reader合同，未建立 Change Record，也不在`R5-HOLD-002`允许路径内。

## 禁止事项

不得修改 `PickerStableId`、release tick、FrameDelay、PN/wait、random、OnThrown、held relation、CPoint/WeaponSync、
pass order、slot/generation、input、AI、collision、render、DAT、scene、resource或C++ authority。不得恢复、运行或实现
R1-WP02 trace；它仍为`BLOCKED`。
