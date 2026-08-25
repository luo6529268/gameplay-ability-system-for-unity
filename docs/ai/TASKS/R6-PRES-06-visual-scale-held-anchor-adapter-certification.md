# R6-PRES-06 — visual scale / held anchor adapter certification

## Goal

在不修改脚本的前提下，证明`A-RENDER-002`保留1.5× visual scale，同时不改变逻辑位置，并以
scale compensation保持held wpoint相对锚点。

## Scope

- 只读C++ held logical writer与body renderer；
- 只读Unity render-space、body pivot、held compensation和Central command path；
- 复用fresh full self-check证据；
- 记录Play Mode待验边界。

## Authority / evidence

- `J:/QQFile/NTSD2.4/ntsd_release/src/entity/game_tick.cpp:1527-1550,1924-1946`；
- `J:/QQFile/NTSD2.4/ntsd_release/src/render/renderer.cpp:558-638`；
- 用户批准的`BattleVisualScale=1.5`保护边界；
- `RESEARCH/R6-PRES-06-visual-scale-held-anchor-adapter-certification-20260822.md`。

## Deliverables

- no-code source certification；
- 全量差异登记、STATE、主计划和handoff更新。

## Verification

- logical pixel/world conversion不乘1.5；
- body pivot只缩放center delta；
- held补偿在right/left使visual wpoint重合；
- Central与Legacy comparison path位置一致；
- fresh Unity compile与full self-check通过。

## Stop conditions

需要改script、scene、DAT、C++、render architecture、logic position或collision时停止并另建Change Record。

## Out of scope

Play Mode像素验收、Android/GPU、C++ runtime trace、R7优化或其它gameplay。

