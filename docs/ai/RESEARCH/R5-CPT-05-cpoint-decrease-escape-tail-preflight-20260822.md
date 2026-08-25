# R5-CPT-05 — CPoint valid decrease-negative escape tail source preflight

> 日期：2026-08-22  
> 状态：PLANNED — source / Unity mapping 已闭合；尚未改脚本。  
> 对应差异：D-CPT-005  
> Change ID：R5-CPT-005  
> C++ authority：J:/QQFile/NTSD2.4/ntsd_release 的 src/entity/cpoint.cpp:60-172、
> src/entity/game_tick.cpp:659-684、Makefile:20-21。

## 结论

在 valid relation 的 cpoint.decrease 小于零且 caught_duration 变为负数时，C++：

1. 写 attacker frame0、victim frame181、双方 hit_count、victim knockback；
2. 仅设置 skip_actions=true；
3. 不 return；
4. 若 cp.throwvx 非零，继续使用 attacker current fallback frame0执行 throw tail；
5. 若无 throw且 attacking 仍为2，继续执行 dircontrol；
6. step14 FramePostProcess 后仍会依据保留的 hit_count将 velocity覆写为 knockback并清 hit_count。

Unity RunKind1 当前在 escape write后直接 return，因此阻断了 C++ required tail。该差异与
R5-CPT-003 的 active mismatch不同，必须单独实现。

## C++ release contract

| 顺序 | source | 已确认行为 |
|---|---|---|
| 1 | cpoint.cpp:60-67 | valid relation才递减 caught_duration。 |
| 2 | cpoint.cpp:68-75 | negative duration写 raw attacker0 / victim181、hit_count=1、knockback，且仅 skip_actions=true。 |
| 3 | cpoint.cpp:77-121 | skip_actions阻止 aaction/taction/jaction。 |
| 4 | cpoint.cpp:123-163 | throw tail不以 skip_actions 为条件；frame0 已成为 current geometry/next source。 |
| 5 | cpoint.cpp:165-172 | throw未清attacking时，dircontrol继续有效。 |
| 6 | game_tick.cpp:666-684 | FramePostProcess 对 hit_count>0 用 knockback写 velocity，再清 hit_count。 |

## Unity current mapping

| C++ | Unity current | 差异 |
|---|---|---|
| negative duration sets skip_actions then tail | RunKind1 writes escape state then return | throw / dircontrol 被截断。 |
| fallback current frame0 for tail | ApplyThrow can accept a frame argument | 当前 escape path未调用，需在本包为其选择 Frame.D。 |
| postprocess consumes hit_count | existing FramePostProcessAll | 已有 consumer；本包只确保 escape tail不提前丢失 hit_count。 |

## Minimal implementation direction

在 RunKind1 当前 negative-duration branch：

- 保留 existing frame0/181、hit count、knockback；
- 替换 direct return 为 skipActions=true 与 useFallbackFrameForThrow=true；
- existing action gate尊重 skipActions；
- existing throw tail用 current Frame.D；
- existing ApplyDirControl 保持在 tail 后；
- 不改变 mismatch handling、valid decrease arithmetic、throw body、kind2 validation、postprocess或任何其它模块。

## Focused acceptance matrix

1. valid escape + throwvx：immediate tail锁定 frame0/132、prev2、fallback geometry、throw velocity、
   hit count/knockback与FWC；随后 step14锁定 velocity回落为knockback和hit count清零；
2. valid escape + no throw + dircontrol + attacking=2：锁定 frame0/181、hit count、knockback与转向；
3. valid escape + no throw/no dircontrol：existing escape output保持；
4. must prove aaction/taction/jaction仍不执行。

## Explicit exclusions

- D-CPT-002 global stats、D-CPT-003 mismatch、D-CPT-004 injury phase owner；
- CPoint relation validation、kind2 validation、throw transform body、held/link、opoint、input、collision、
  render、DAT/scene、pass order、array capacity、C++ authority；
- C++ runtime trace/build/executable与 real Play Mode。

## Evidence status

- source branch order / Unity early return / postprocess consumer：VERIFIED；
- C++ runtime trace：BLOCKED by R1-WP02；
- real Unity Play Mode：PENDING；
- source preflight only；尚未写 script。

