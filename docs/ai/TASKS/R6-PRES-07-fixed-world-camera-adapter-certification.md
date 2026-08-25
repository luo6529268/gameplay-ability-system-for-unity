# R6-PRES-07 — fixed-world camera adapter certification

## Goal

在不修改脚本的前提下，证明`A-RENDER-003`把C++ display camera与Unity fixed-world logic truth分离，
并且safe-area只作用于presentation camera。

## Scope

- 只读C++ camera/perspective consumer；
- 只读Unity camera/RenderOffset writer、central snapshot与safe-area；
- 复用fresh full self-check；
- 记录Play Mode及snapshot-restore残余风险。

## Authority / evidence

- `J:/QQFile/NTSD2.4/ntsd_release/src/entity/game_tick.cpp:2026-2059`；
- `J:/QQFile/NTSD2.4/ntsd_release/src/render/renderer.cpp:460-505,517-575,687-716`；
- 用户批准的fixed-world logic camera / presentation camera separation；
- `RESEARCH/R6-PRES-07-fixed-world-camera-adapter-certification-20260822.md`。

## Deliverables

- no-code source certification；
- 全量差异登记、STATE、主计划和handoff更新。

## Verification

- tick边界camera/RenderOffset清零；
- stationary entity/shadow不因另一character移动而偏移；
- safe-area不写runtime position/camera scalar；
- fresh Unity compile/full self-check通过。

## Stop conditions

需要改script、scene、C++、camera architecture、snapshot schema或logic position时停止并另建Change Record。

## Out of scope

Play Mode视觉、R8场景认证、C++ runtime trace、server/network和R7优化。

