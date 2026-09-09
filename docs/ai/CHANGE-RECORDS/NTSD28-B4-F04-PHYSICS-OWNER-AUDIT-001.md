# NTSD28-B4-F04-PHYSICS-OWNER-AUDIT-001 — F-04 physics owner audit

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-PHYSICS-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan battle_world.cpp BattleWorld28::step_physics and physics_integrator.cpp PhysicsIntegrator28::step; EXE B1E13AE1, closure 39DDDA15.
evidence: GATE-INTEGRATION-FRICTION-GRAVITY-LANDING-MATRIX / FIRST-INDEPENDENT-DIFFERENCE-TYPE1-STATE1002-THRESHOLD / IMPLEMENTATION-SPLIT-DEFINED / NO-CODE-WRITE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / PHYSICS_SPLIT_DEFINED`

## 已观察事实

- Authority 的 physics gate 顺序为 motion hold 倒计时、negative interaction return、frame lookup、
  type 0 state 10 suppression，再进入统一 integrator；Unity 的 `FrameDelay`/`LinkState` 主体同形，
  state 10 与额外 cpoint gate 仍需独立证明。
- Authority 以 `collision_y_reference` 同时决定摩擦平面、negative effective floor 和所有 landing
  strict predicate；Unity runtime 目前没有该独立 carrier，主要按零地面判断。
- Authority type 1/state 1002 的 action 7 threshold 是正式锁定运行时读取的巨大 double，
  不是邻近但无机器码引用的 `9.9`。Unity 的 `landingVy <= 9.9` 会让正常高速轻武器错误反弹。
- type 3/OID999 只在 `contact_y < -9` 的特殊 reference 条件发射 `(x,y) motion=(9,9)`；
  Unity 当前 `crossedGround` 后清零 X/Y motion 的分支不能在缺 carrier 时单独修补。
- derived `LF2Weapon.WeaponFlightPhysics` 对 type4/OID120 与 OID101 使用 `else if`，而 Authority
  是两个独立 X extra；需和 identity alias/profile 一并闭合。

## 裁决

先实施不依赖 carrier 的 type 1 threshold 小包。其余按 carrier/core/type-family/owner-retirement
分包，Audio 事件归 B10，正式内容身份与资源归 H/B11。

