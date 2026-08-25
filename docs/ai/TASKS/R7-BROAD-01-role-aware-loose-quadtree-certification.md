# R7-BROAD-01 — role-aware / Loose Quadtree conditional certification

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING / NO-CODE CERTIFICATION`

## Goal

证明role-aware/LooseQuadtree只优化candidate discovery，保留C++ pair/direction/ITR/candidate/RNG语义，
并区分测试显式backend与production默认配置。

## Authority / Evidence

- C++ `collision_collect.cpp:242-372`；
- Unity `BruteForceSceneQuery` formal collectors；
- focused jobs 9/9、58/58、16/16；
- fresh-domain full self-check 2026-08-22 22:13:06 PASS；
- `RESEARCH/R7-BROAD-01-role-aware-loose-quadtree-recertification-20260822.md`。

## Result

- 未发现新candidate behavior difference；
- `D-PERF-002`：production default仍为BruteForce；
- `D-TEST-001`：broadphase EditMode suite之后存在静态状态污染，domain reload后self-check恢复。

## Out of scope / stop

不授权修改production backend、candidate gameplay、C++、pass/RNG/capacity。Play Mode与C++ trace仍待。

