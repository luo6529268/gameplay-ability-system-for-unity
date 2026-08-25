# Handoff — R5-HOLD-01 type-2 held throw frame delay

> 日期：2026-08-22  
> Change ID：`R5-HOLD-001`  
> 当前状态：`RUNTIME_PENDING`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1527-1535,1621-1630,1924-1932,1999-2006`  
> 保护边界：不修改C++ authority；不改type2 spawner、ReleaseTick、PN/wait/random、valid held relation、CPoint/WeaponSync、pass order、slot/generation、input、AI、collision、render、DAT、scene或资源。

## Source contract

C++第一、第二negative-held pass均先同步 `child.frame_delay = holder.frame_delay`，随后type2 throw只写random frame、
velocity和link release，不再次写delay。`frame_delay`是可观察字段：C++ frame postprocess在它非零时跳过对应分支。

Unity有两条实际type2 writer：

1. `BattleHeldObjectWriter.RunStep12`（generic current-DAT type2）；
2. `LF2WeaponHeldStateResolver.Act`（real `LF2WeaponBase.WeaponType==2`）。

二者原本都先复制holder delay，随后额外强制为1。

## 已写入的最小变更

- 删除generic writer type2 branch的`held.FrameDelay = 1`；
- 删除real weapon resolver type2 branch的`weapon.FrameDelay = 1`；
- 既有generic type2 fixture现在输入holder delay `-3`并断言child保留`-3`；
- 既有real weapon type2 fixture本来输入holder delay `7`，现改为断言weapon保留`7`；
- 两个fixture继续锁定random frame范围、authoring velocity、link clear和weapon throwing state。

## 已实际完成的验证

- `Tools/Validate-ChangeLedger.ps1`：PASS，R5三条脚本路径均被`R5-HOLD-001`覆盖；
- R5范围的`git diff --check -- <R5 paths>`：exit 0（仅LF→CRLF提示）；全工作区仍只报告用户已有场景trailing whitespace；
- 当前已打开Unity 2022.3.62f3 Editor scripts refresh后，`error CS`筛选为0条；
- full `BattleRuntimeSelfCheck`：`Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 08:01:15为`PASS`；
  existing generic negative-delay与real weapon positive-delay type2 fixture均在其中执行。

## 仍未关闭

- C++ release runtime trace / first-difference；
- 对应真实战斗场景Play Mode；
- `D-HOLD-002` type2 spawner、ReleaseTick及其它R5链路。

因此该记录为`RUNTIME_PENDING`，不是“R5已完成”、更不是C++ runtime完全对齐。

## 下一步

保持`R5-HOLD-001`作为等待trace/Play Mode的证据包；按D-009可继续处理下一个具有独立source合同的R5最小包。
如后续验证失败，只在本Record列出的三份脚本中处理最小原因；不得将type2 spawner、release/throw其它字段或
CPoint/held调度混入本包。
