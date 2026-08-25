# Handoff — R5-LINK-02 negative-link invalidation

> 日期：2026-08-22  
> Change ID：`R5-LINK-002`  
> 当前状态：`RUNTIME_PENDING`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1441-1457, 1860-1872`  
> 保护边界：不修改C++ authority；不改pass order、valid held/release/throw、positive link、CPoint/WeaponSync、opoint、slot/generation、input、AI、collision、render、DAT、scene或资源。

## Source contract

C++ `game_tick(...)`有两次升序negative-held scan。两次的无效关系条件都是：holder index越界、holder inactive或
holder target不是child当前slot。两次都只将child `link_state`写为0，保留child `holder_idx`。C++特殊held release
分支会写更多字段，但只在valid relation之后，不属于本记录。

Unity `SimulationQueryAndLinkModule.HeldObjectProcessAll`是已调度的两次shared pass共用实现；它原本在同一invalid
branch额外写`HolderStableId=-1`。`HolderStableId`是Unity中对应C++ `holder_idx`的当前relation字段。

## 已写入的最小变更

1. 删除`HeldObjectProcessAll` invalid branch的`HolderStableId=-1`，保留child `LinkState=0`与snapshot refresh。
2. full self-check新增out-of-range holder、active holder/target mismatch和second shared pass字段保持断言。
3. 新增`SimulationQueryAndLinkModuleEditorTests`，用两个EditMode test验证：
   - out-of-range holder跨两次pass后保留holder slot；
   - active holder target mismatch后保留child holder与holder target字段。

## 已实际完成的验证

- `Tools/Validate-ChangeLedger.ps1`：PASS，R5三条脚本路径均被`R5-LINK-002`覆盖；
- R5范围的`git diff --check -- <R5 paths>`：exit 0（仅LF→CRLF提示）；全工作区仍只报告用户已有场景trailing whitespace；
- 当前已打开Unity 2022.3.62f3 Editor scripts refresh后，`error CS`筛选为0条；
- full `BattleRuntimeSelfCheck`：`Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 07:46:36为`PASS`；
- focused EditMode `NTSD.Test.SimulationQueryAndLinkModuleEditorTests`：job
  `161af4674f524a388233e9e89865065c`，2/2 passed、0 failed、0 skipped、0.499s。

## 仍未关闭

- C++ release runtime trace / first-difference；
- 对应真实战斗场景Play Mode；
- valid held/release/throw与`D-HOLD-001/002`等其它R5链路。

因此该记录为`RUNTIME_PENDING`，不是“R5已完成”、更不是C++ runtime完全对齐。

## 下一步

保持`R5-LINK-002`作为等待trace/Play Mode的证据包；按D-009可继续处理下一个具有独立source合同的R5最小包。
如后续验证失败，只记录first failure并做合同内最小修复；不得借此改动valid held/release/throw或其它R5子链路。
