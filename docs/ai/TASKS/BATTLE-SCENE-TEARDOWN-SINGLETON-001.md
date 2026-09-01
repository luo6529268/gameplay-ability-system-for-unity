# BATTLE-SCENE-TEARDOWN-SINGLETON-001 — Scene close singleton recreation task

> 日期：2026-08-31  
> 状态：`VERIFIED / COMPILE_0 / FOCUSED_1_1_PASS / LIVE_TEARDOWN_PASS`  
> Change ID：`BATTLE-SCENE-TEARDOWN-SINGLETON-001`

## Goal

关闭 Scene 或退出 Play Mode 时，allocation unseal 只处理仍存在的 `LF2ObjectPointFactory`/`LF2ObjectPool`，不得通过 `.Instance` 创建新的 `*_AutoCreated` GameObject。

## Acceptance

- compile 0 error。
- teardown focused test 证明 factory/pool singleton 为空时，unseal 后仍为空且没有新增 Scene GameObject。
- 正常 battle prepare/seal 的按需创建边界不变。

## Result

- teardown `Unseal` 使用 `TryGetInstance()`；正常 prepare/seal 不变。
- focused 1/1 PASS；真实 Play enter/exit 后两个 `_AutoCreated` 均为0，目标 cleanup warning 为0。
