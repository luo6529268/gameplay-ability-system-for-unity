# Handoff — R5-CPT-03 CPoint reciprocal mismatch tail

> 日期：2026-08-22  
> Change ID：R5-CPT-003  
> 当前状态：RUNTIME_PENDING — 最小 writer、focused fixture、Unity compile与full self-check已通过；C++ trace / Play Mode待验。  
> Authority：J:/QQFile/NTSD2.4/ntsd_release/src/entity/cpoint.cpp:20-172。

## 已确认 source contract

- missing/inactive victim仍是 frame0 后立即结束；
- active reciprocal mismatch或 invalid previous victim CPoint是 frame0 + skip decrease/actions，
  不是 complete return；
- throw tail不受 skip flags约束；
- mismatch throw的 geometry/next来自fallback current frame0；
- dircontrol仍执行，除非 throw将attacking清为0。

## 已写内容

- RunKind1 用 local skip flags替换 active mismatch direct return；
- mismatch throw显式使用 attacker fallback current frame；
- reciprocal throw、invalid previous throw、dircontrol-only、negative-decrease skip及FWC matrix已写；
- missing-victim immediate return保持；
- valid-relation escape未改，仍为单独 D-CPT-005。

## 实际验证

- UnityMCP scripts refresh后 filtered C# compiler error为0；
- full BattleRuntimeSelfCheck结果文件于2026-08-22 09:59:58为PASS；
- focused mismatch matrix在同次self-check通过；
- 最终 ledger / scoped diff 已在本次文档收口后重跑并通过：36 条 Record、26 个 governed code file均被覆盖。

## 下一动作

按连续队列先为 D-CPT-005 valid decrease-negative escape tail 建立独立 source contract；
不得把本包PASS扩大为完整 CPoint/R5/C++ runtime对齐。

## 未关闭项

C++ runtime trace仍由 R1-WP02 阻塞；real Play Mode待验。若source / fixture显示需要越过两文件、
改变 pass order或改 C++ authority，立即停止并另建合同。
