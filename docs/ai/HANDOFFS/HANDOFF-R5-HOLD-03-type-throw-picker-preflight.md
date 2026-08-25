# Handoff — R5-HOLD-03 held throw picker preflight

> 日期：2026-08-22  
> Change ID：`R5-HOLD-003`  
> 当前状态：`RUNTIME_PENDING`（source preflight / Task Contract、唯一writer/fixture、Unity compile与full self-check均已完成；C++ trace / Play Mode待验）  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1597-1630,1977-2006`；
> `src\entity\frame_advance.cpp:215-271,695-735`。

## 已完成的只读工作

- `Makefile:17` / `:32` 确认 `frame_advance.cpp` 与 `game_tick.cpp` 均参与 release build；
- `game_world.h` reset 将 `picker_idx=-1`；`collision.cpp` normal pickup不写该字段；
- 两轮 C++ held throw 对type1/2/4/6均无`picker_idx` write；release helper也无该写；
- `frame_advance.cpp` 只在后续target-selection路径读取/写入picker；
- Unity shared `ThrowHeldWeapon` 是唯一 held-throw extra writer，`ReleaseFlow` 与 `OnThrown` 不写picker；
- 未运行、修改、构建、复制或向C++ authority写入任何内容。

## 已写入的最小改动

- 从 shared `LF2WeaponHeldStateResolver.ThrowHeldWeapon` 删除无条件的`PickerStableId=holder slot`；
- existing real held fixture为type1/2/4/6设置不同picker sentinel，并在throw后逐个断言保持；
- 未改`SpawnerEntityIndex`、FrameDelay、ReleaseTick、PN/wait、random、OnThrown、pickup或target-selection reader。

## 已完成的 Unity 验证

- `Tools/Validate-ChangeLedger.ps1`：PASS（32个Change Record、25个已有governed脚本改动均有归属）；
- 本包范围 `git diff --check -- ...`：exit 0（仅LF→CRLF提示）；
- 当前已打开 Unity 2022.3.62f3：scripts refresh 后 `error CS`筛选为0；
- 菜单 `NTSD/验证/运行战斗运行时自检`：`Temp/NTSD_BattleRuntimeSelfCheck.result` 于
  2026-08-22 08:39:22 写入 `PASS`，existing real-held fixture已执行type1/2/4/6 picker sentinel保持。

## 下一步（保持独立的后续工作）

1. 保持`R5-HOLD-003`作为等待C++ trace / Play Mode的证据包；不得再扩大它；
2. 按D-009可继续下一个独立source合同包；先按主差异台账重评估下一条R5差异的范围；
3. 本包最高状态仍是`RUNTIME_PENDING`。C++ trace和Play Mode仍不能声称完成。

## 禁止事项

不改`SpawnerEntityIndex`、FrameDelay、ReleaseTick、PN/wait、random、OnThrown、pickup、target-selection reader、
CPoint/WeaponSync、pass order、slot/generation、input、AI、collision、render、DAT、scene、resource或C++ authority。
