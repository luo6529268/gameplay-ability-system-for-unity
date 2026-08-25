# R5-CPT-03 — CPoint reciprocal mismatch control-flow source preflight

> 日期：2026-08-22  
> 状态：PLANNED — source / Unity control-flow mapping 已闭合；尚未改脚本。  
> 对应差异：D-CPT-003  
> Change ID：R5-CPT-003  
> C++ authority：J:/QQFile/NTSD2.4/ntsd_release 的 src/entity/cpoint.cpp:20-168、
> src/entity/game_tick.cpp:659-664、Makefile:20-21。

## 结论

Unity BattleCpointWriter.RunKind1 将 active victim 的 reciprocal mismatch 或 victim prev-frame
non-kind2 CPoint 统一为 attacker raw frame 0 后直接 return。C++ cpoint_check_internal 在相同条件下
只设置 skip_actions 和 skip_decrease；其后的 throw tail 仍无条件按 cp.throwvx 执行，dircontrol 也仍执行。

此外，C++ mismatch 已把 attacker current frame 写为 0，随后 throw tail 的 geometry 和 next 都从
current frame 0 读取；不是从原 attacker prev_frame2 的 CPoint frame读取。Unity 若只移除 return 而继续
将 catcherFrame snapshot 传给 ApplyThrow，仍会产生错误的位置/next path。

## C++ release control-flow contract

| 条件 | C++ source | 必须结果 |
|---|---|---|
| caught slot 越界、victim inactive或无 char_data | cpoint.cpp:32-42 | attacker frame=0，立即 continue；没有 victim tail。 |
| active victim，但 victim.catcher_idx 不等于 attacker slot | cpoint.cpp:49-53 | attacker frame=0，skip_actions=true，skip_decrease=true；继续后续 tail。 |
| active victim，但 victim prev_frame2 无 CPoint或 kind 非2 | cpoint.cpp:54-58 | 同上；继续后续 tail。 |
| skip_decrease=true | cpoint.cpp:60-75 | 不运行 decrease / escape；不得写 hit count、knockback或 victim 181。 |
| skip_actions=true | cpoint.cpp:77-121 | 不运行 aaction/taction/jaction，也不得清 attacker/victim attacking。 |
| cp.throwvx 不为零 | cpoint.cpp:123-163 | 不受 skip_actions/skip_decrease 影响；在 fallback current frame 0 上执行 throwinjury、position、next、prev_frame2、attacking=0、victim velocity/frame/prev_frame2。 |
| cp.dircontrol | cpoint.cpp:165-172 | 不受 mismatch skip flag影响；若 throw 未将 attacking清零且 attacking=2，仍按 runtime direction转向。 |

## 新登记但不属于本包的相邻差异

C++ cpoint.cpp:65-75 的 valid relation decrease-negative escape 在 caught_duration 小于零时，
只设置 skip_actions=true；后续 throw tail 仍不受该 flag约束。Unity当前 escape分支在写
frame0/181、hit count和knockback后直接 return。该差异已登记为 D-CPT-005，不得借
R5-CPT-003 修改 valid-relation escape control flow。

## Unity current mapping

| Source branch | Unity current behavior | 差异 |
|---|---|---|
| missing victim | RunKind1 raw frame0 then return | 一致，保留。 |
| reciprocal mismatch / victim prev CPoint invalid | RunKind1 raw frame0 then return | 错误地抑制 throw tail和dircontrol。 |
| valid relation | normal decrease/action/throw/dircontrol | 不在本包改动范围。 |
| throw source frame after mismatch | Unity ApplyThrow 接收 old catcherFrame snapshot | 若仅解除 early return，会与 C++ fallback frame0 geometry/next不一致。 |

## 最小实现方向

仅在 RunKind1 内区分：

1. missing/inactive victim：保留 existing early return；
2. active-but-reciprocal-invalid：raw write attacker frame0，设置 local skip flags；
3. skip flags仅门控 decrease/action；
4. throw tail使用 attacker fallback current frame data，而非 old collision snapshot；
5. dircontrol保留在 tail 后；
6. 不改 kind2 validation、valid relation、R5-CPT-004 injury owner或其它 CPoint branch。

## 验收夹具

1. reciprocal mismatch + throwvx：验证 action/decrease不发生；fallback frame0 geometry/next、victim
   vaction/prev2/velocity发生；
2. invalid victim prev CPoint + throwvx：同上；
3. reciprocal mismatch + no throw + dircontrol + attacking=2：验证 frame0后仍正确转向；
4. mismatch + decrease negative：验证 skip decrease，不得 escape/hit count/knockback；
5. missing victim：验证仍立即返回且不跑 tail；
6. all cases锁定 FrameWaitCounter，防止回退 raw-frame writer重新引入 R5-CPT-001 问题。

## 明确排除

- D-CPT-002 global stats 与 R5-CPT-004 phase ownership；
- D-CPT-005 valid-relation decrease-negative escape tail；
- valid CPoint relation、kind2 validation、throw transform semantics本身、held/link、opoint、input、
  collision、render、DAT/scene、pass order、array capacity、C++ authority；
- C++ runtime trace / instrument / build / executable与 real Play Mode。

## 证据与状态

- source build participation、branch order、Unity direct-return mapping：VERIFIED；
- source runtime trace：BLOCKED by R1-WP02；
- real Unity Play Mode：PENDING；
- 仅 source preflight，尚未写 script。
