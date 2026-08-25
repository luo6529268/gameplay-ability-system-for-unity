# Handoff — R5-LINK-01 positive-link invalidation

> 日期：2026-08-22  
> Change ID：`R5-LINK-001`  
> 当前状态：`RUNTIME_PENDING`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1828-1845`  
> 保护边界：不修改C++ authority；不改negative link、CPoint/WeaponSync、held/release、opoint、slot/generation、scheduler、input、AI、render、DAT、scene或资源。

## 已闭合的源码合同

C++ `game_tick(...)` T11按升序检查每个active、`link_state > 0`的holder：target index越界、target inactive或
target `holder_idx != ci`时，只写holder `link_state = 0`。该分支不写target index、held weapon slot或target的
reverse holder字段。`Makefile`列入`src/entity/game_tick.cpp`，但本包没有运行、修改、构建、复制或向C++ authority写入。

## 已写入的Unity最小改动

1. `SimulationQueryAndLinkModule.RunLegacyPositiveLinkValidation` 的invalid branch只清`LinkState`。
2. `BattleEcsPositiveLinkValidationPass`：
   - ShadowCompare expected在invalid case保留captured `TargetSlotIndex`和`HeldWeaponStableId`；
   - DataOriented writer在invalid case只清`LinkState`。
3. `BattleRuntimeSelfCheck.CheckValidatePositiveLinksMatrix`改为覆盖：invalid target index、inactive target与
   reciprocal mismatch后前向字段保持。
4. `BattleEcsPositiveLinkValidationPassEditorTests`改为锁定Legacy/ShadowCompare/DataOriented一致、live-link路径、
   structural witness和forward-field保持。

## 本轮工具事件

一次包含四个文件的大补丁在读取`BattleRuntimeSelfCheck.cs`时收到sandbox helper编码错误。该工具调用不是原子操作：
前两份writer文件已经成功写入，self-check与Editor test未写入。随后已确认这一事实，并用两个独立小补丁完成相应
测试改动；没有回退或扩大范围。以后同一Work Package将按“writer小批次 → 对应测试小批次 → diff核对”执行，避免
半组脚本停留在工作区。

## 已实际完成的验证

- `Tools/Validate-ChangeLedger.ps1`：PASS，R5四个脚本路径均被`R5-LINK-001`覆盖；
- R5范围的`git diff --check -- <R5 paths>`：exit 0（仅LF→CRLF提示）。最终全工作区重跑为exit 1，
  但只报告用户已有`Assets/NTSD/Scene/NTSD_Battle.unity`的trailing whitespace；没有R5路径差异错误，未触碰场景；
- 当前已打开Unity 2022.3.62f3 Editor scripts refresh后，`error CS`筛选为0条；
- full `BattleRuntimeSelfCheck`：`Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 07:32:40为`PASS`；
- focused EditMode `NTSD.Test.BattleEcsPositiveLinkValidationPassEditorTests`：job
  `edc22b2fd5314fb685c59d1b04f97c7a`，8/8 passed、0 failed、0 skipped、0.675s。

## 仍未关闭

- C++ release runtime trace / first-difference；
- 对应真实战斗场景Play Mode；
- `D-LINK-002` negative child invalidation及其他R5链路。

因此该记录为`RUNTIME_PENDING`，不是“R5已完成”、更不是C++ runtime完全对齐。

## 下一步

保持`R5-LINK-001`作为等待trace/Play Mode的证据包；按D-009可继续处理下一个具有独立source合同的R5最小包。
如后续验证失败，记录first failure、最小修复范围和重跑结果，不将其并入negative link或其它R5链路。
