# Task Contract — NTSD28-B3-C25G-FRAME-BODY-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`
> 依赖：`NTSD28-B3-AI-RENDER-PHASE-CONSUMERS-001 / VERIFIED`

## 目标

闭合 current Authority C25g `BattleWorld28::step_frame_slot()` 的完整内部顺序、gate与副作用，并逐项映射Unity当前per-slot late frame owner、exact-character ECS fast path和virtual compatibility path；本审计只决定后续实施边界，不把B4/B7/B10/B11行为改写塞进B3。

## 权威

- `source/ntsd28_core/src/simulation/battle_world.cpp`：`step_frame_slot`、`step_frames_range`及5个native helper。
- `source/ntsd28_core/src/simulation/frame_machine.cpp`。
- 上述文件进入82-file playable closure `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。

## 允许路径

- 本Task、Change Record、manifest、Ledger、STATE、handoff和总表。
- Authority与Unity只读源码。

## 不做

- 不修改C#、Scene、Config、DAT、资源或Authority。
- 不提前实现B4 frame semantics、B7 lifecycle、B10 audio或B11 content/resource cost。

## 验收

- C25g每个gate/helper都有Authority位置、Unity owner、差异状态与唯一后续阶段。
- 明确B3结构是否已放置、哪些是confirmed behavior difference。
- C25i不得与C25g混写；下一包独立审计/放置。

## 结果

详见`docs/ai/MANIFESTS/NTSD28-B3-C25G-FRAME-BODY-OWNER-AUDIT.md`。B3的C25g per-slot结构owner已存在，但完整行为不等价；差异已按B4/B7/B10/B11拆分，下一先处理C25i placement/content dependency。
