# R8-WP01G-R02 — D-MOV-005 exact state2000 facing closure

> 日期：2026-08-23  
> 状态：`RUNTIME_PENDING / CODE + UNITY AUTOMATED EVIDENCE PASS`

## Result

C++ Release `frame_tick`对所有`state == 2000`实体无条件按最终`Vx`写facing。Unity fallback已有同一规则，
但exact current-type0 character ECS pass缺失。`R8-MOV-005-001`已在C++对应时点补齐通用规则，不包含
角色、OID或技能特判。

当前正式Unity DAT inventory仍只有type2/type4 weapon使用literal state2000，正常场景继续走原有fallback；
新增exact规则关闭未来type0/mod DAT与eligibility扩张时的代码差异，不改变当前武器路径。

## Authority / Unity crosswalk

- C++ Makefile包含`src/entity/frame_advance.cpp`；
- C++ `frame_advance.cpp:884-887`：`state == 2000`时`facing=(vx>0)?0:1`；
- Unity fallback `LF2Entity.RunCommonFrameTick`：同一规则；
- Unity exact `BattleEcsCharacterFrameTickPass.ExecuteExactCharacter`：本Change前无writer，本Change后在lying
  后、wait/transition前写`right/left`，保持C++时点。

## Verification

- UnityMCP force scripts refresh触发domain reload并恢复ready；
- `Assembly-CSharp.dll`与`Assembly-CSharp-Editor.dll`更新时间为16:12:45/46；Console无`error CS`；
- focused EditMode job `e5e283e740cc49e597c99b7ef994c419`：1/1 PASS，覆盖Vx正/零/负与exact ownership；
- full `BattleRuntimeSelfCheck`：2026-08-23 16:14:46 `PASS`；
- Change Ledger validator：74 records / 74 governed files PASS；
- scoped diff-check exit0，仅既有LF→CRLF warning。

## Evidence boundary

真实正式type0 state2000 Play不可构造，因为当前authored inventory无该DAT；C++ runtime trace继续BLOCKED。
因此最高为`RUNTIME_PENDING`，不是C++ runtime VERIFIED。

