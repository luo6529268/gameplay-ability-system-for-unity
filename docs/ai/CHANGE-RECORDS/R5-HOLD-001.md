# R5-HOLD-001 — type-2 held throw preserves copied frame delay

<!-- CHANGE-RECORD
id: R5-HOLD-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1527-1535,1621-1630,1924-1932,1999-2006
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-DUAL-WRITER-MISMATCH / UNITY-COMPILE-PASS / FULL-SELFCHECK-PASS / CPP-TRACE-AND-PLAYMODE-PENDING
-->

> 创建日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — 已完成合同内two-writer removal与existing fixture更新，并取得Unity compile和full self-check证据；C++ trace与真实Play Mode仍未关闭。

## 差异

C++在两轮held pass中先复制`holder.frame_delay`，type2 throw不覆盖它。Unity generic与real weapon writer都
在复制后额外写`FrameDelay=1`。

## 允许代码路径

| 文件 | 符号 | 允许内容 |
|---|---|---|
| `BattleHeldObjectWriter.cs` | `RunStep12` type2 branch | 删除`held.FrameDelay=1`。 |
| `LF2WeaponHeldStateResolver.cs` | `Act` type2 branch | 删除`weapon.FrameDelay=1`。 |
| `BattleRuntimeSelfCheck.cs` | generic / real type2 held fixtures | 将错误的固定1断言替换为holder delay保持，同时保留frame/velocity/link/state断言。 |

## 禁止扩大

不改type2 `SpawnerEntityIndex`、ReleaseTick、PN/wait、random call、held link release、其它weapon type、
valid held relation、CPoint/WeaponSync、scheduler、slot/generation、input、AI、collision、render、DAT/scene/resource或C++ authority。

## 验收门槛

代码写入后必须同步Ledger/STATE/full diff/main plan/handoff，并实际运行ledger validator、R5范围diff check、
Unity compile与full self-check。最高状态为`RUNTIME_PENDING`。

## 本次代码写入

- generic `BattleHeldObjectWriter` type2 branch不再覆盖已从holder同步的`FrameDelay`；
- real `LF2WeaponHeldStateResolver` type2 branch不再覆盖已从holder同步的`FrameDelay`；
- 现有generic type2 fixture用holder `FrameDelay=-3`锁定负值保持；现有real weapon type2 fixture用holder
  `FrameDelay=7`锁定正值保持，二者继续验证随机frame范围、速度、link清理与throwing state。

## 实际验证

| 层级 | 实际操作 | 结果 |
|---|---|---|
| 留痕 | `Tools/Validate-ChangeLedger.ps1` | PASS；25个governed diff均被Record覆盖，R5三条路径归`R5-HOLD-001`。 |
| 文本差异 | R5范围的`git diff --check -- <R5 paths>` | exit 0；仅LF→CRLF提示。全工作区仍仅被用户已有场景trailing whitespace阻塞，未触碰场景。 |
| Unity编译 | 当前已打开Unity 2022.3.62f3经UnityMCP `refresh_unity(mode=scripts)`，随后`read_console(filter=error CS)` | 0条C# compiler error。 |
| 既有聚焦fixture / 完整自检 | 菜单`NTSD/验证/运行战斗运行时自检` | `Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 08:01:15为`PASS`；generic type2 negative-delay与real weapon type2 positive-delay fixture均在该self-check中执行。Console仅含两个既有rest-binding negative control。 |

本包没有新建独立EditMode class；它复用了并扩展已有、已被full self-check调用的generic/real type2 held fixture。C++ runtime trace、同场景first-difference和真实Play Mode尚未取得，本记录不得写为“已对齐”。
