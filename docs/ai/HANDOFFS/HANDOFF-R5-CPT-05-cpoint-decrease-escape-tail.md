# Handoff — R5-CPT-05 CPoint valid decrease-negative escape tail

> 日期：2026-08-22  
> Change ID：R5-CPT-005  
> 当前状态：RUNTIME_PENDING — 最小 writer、focused fixture、Unity compile与full self-check已通过；C++ trace / Play Mode待验。  
> Authority：J:/QQFile/NTSD2.4/ntsd_release/src/entity/cpoint.cpp:60-172、
> src/entity/game_tick.cpp:666-684。

## 已确认

- valid escape写frame0/181/hit count/knockback后不 return；
- throw tail仍可覆盖victim frame/velocity，但hit count仍留给step14；
- step14随后以knockback覆写velocity并清hit count；
- no-throw时 dircontrol仍有机会根据 attacking=2执行；
- Unity当前direct return是唯一计划修复的差异。

## 已写内容

- RunKind1 escape branch复用already established skipActions / fallback-frame mechanics；
- escape+throw immediate与step14结果已写入focused assertions；
- escape+no-throw dircontrol assertion已改为C++结果；
- 未复制writer、未增加pass、未修改mismatch/postprocess。

## 实际验证

- Unity 2022.3.62f3 Editor完成Tundra build，Editor.log未检出error CS；
- request-file full BattleRuntimeSelfCheck结果于2026-08-22 16:09:37为PASS；
- focused escape-tail assertions在同次self-check通过；
- 最终ledger/scoped diff已在文档收口后重跑并通过：37条Record、26个governed code file均被覆盖。

## 下一动作

保持本包为RUNTIME_PENDING；按连续队列预检下一条R5 CPoint/held/link/opoint/lifecycle source差异。
不得把本包PASS扩大为完整CPoint、R5或C++ runtime对齐。

## 未关闭项

C++ trace继续受 R1-WP02阻塞；real Play Mode待验。若需要更改throw body、postprocess、pass order、
mismatch、kind2或任一 scope 外模块，停止并另建合同。
