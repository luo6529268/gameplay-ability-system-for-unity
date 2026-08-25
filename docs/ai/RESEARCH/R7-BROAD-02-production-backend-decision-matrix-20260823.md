# R7-BROAD-02 production backend decision matrix

> 日期：2026-08-23
> 状态：`DECISION COMPLETE / RETAIN BRUTEFORCE`

## Authority and adapter boundary

C++ `collision_collect.cpp`的slot-pair/direction/ITR-BDY顺序仍是行为权威。Unity LooseQuadtree只可作为
pair discovery adapter；最终pair必须恢复authority ordinal顺序，双方向进入同一exact collector，任何
identity/geometry/epoch异常必须恢复RNG和candidate后完整fallback BruteForce。

## Deployment

`Assets/NTSD/Config/GameConfig/GameConfig.asset` 当前broadphase为空；
`CollisionBroadphaseBackendResolver`为空/invalid时返回BruteForce。普通production scene未默认启用Loose。

## Evidence gap

synthetic parity/pair reduction充分，但历史真实1000-AI harness强制Loose，不能构成current-build backend A/B。
因此当前不能把“pair数下降”扩大成“production FPS将达标”或“可安全切默认”。fresh focused结果待填。

## Fresh evidence

- `623e91b88792432a87bccd0969b08ba9`：80/80 PASS；
- `01bea6cea2e340e088683226c81cf713`：8/8 PASS；
- 同域03:13:57 full self-check PASS；
- production config仍为空，resolver仍默认BruteForce。

## Decision

当前保留BruteForce。LooseQuadtree继续作为显式stress/诊断和future production候选；它的source/parity证据
允许继续评估，但没有real current-build A/B与R8 scene certificate前，不修改default。
