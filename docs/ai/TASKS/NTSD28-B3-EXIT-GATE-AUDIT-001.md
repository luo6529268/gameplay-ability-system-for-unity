# Task Contract — NTSD28-B3-EXIT-GATE-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / B3_PLACEMENT_EXIT_READY / FULL_CLOSE_DEFERRED`

## 目标

核验B3 pass/order阶段能否进入B4，并逐项识别仍阻止“B3完全关闭”的生产残留；不删除仍有行为的legacy caller。

## 结论

- C00～C24位置、C25 dynamic live-slot skeleton、completed-tick presentation边界与已实现C25 owner足以支持进入B4。
- normal production仍在C25后调用临时`SerialTickAll`；当前body只剩special-attack state-entry/state15/death、per-entity snapshot和state9998 cleanup，不能在B3无证据删除。
- `EntityPostFrameTailAll`已不再healing，但仍有F7 init、hit/transient carrier cleanup与snapshot；`Mode2RandomWeaponDropTailAll`为用户保留例外。
- C25c-e算法、C25g细节、C25k/n/o与C25l资源runtime分别依赖B4/B5/B7/B8/B10/B11/H。
- 裁决：B3取得`PLACEMENT_EXIT_READY`，下一进入B4；B3的`FULL_CLOSE`必须等这些下游owner接管后删除临时serial/global残留并复跑完整pass trace。

## 不做

不修改脚本/Scene/资源/Authority，不删除SerialTickAll或global post-tail，不改变random-drop例外。
