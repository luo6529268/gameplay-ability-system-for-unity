# Task Contract — NTSD28-B4-F04-DERIVED-WEAPON-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / MIGRATION_SEAM_DEFINED`

## 目标

只读闭合正常 pooled `LF2Weapon` 为什么仍绕过 shared reference-aware physics、其 gate、
in-flight/landing virtual seam 与 snapshot 边界，并定义不丢失 weapon specialization 的最小迁移包。

## 范围与不变量

- 读取正式 `PhysicsIntegrator28`、Unity `LF2WeaponBase/LF2Weapon` 及 snapshot/test caller。
- 不改脚本、Scene、Prefab、DAT、资源、Authority。
- 不把 type3/OID999、collision-Y producer、Audio 或内容迁移混入 derived 包。

## 结论

- pooled weapon 的 current DAT type 等于 `_poolWeaponType` 时固定走 `RunFrameAdvancePhysics`；只有不等时回落 shared owner。
- derived owner 在 `WeaponFlightPhysics` 后仍调用 legacy `WeaponDynamics`，只返回 y=0 bool；因此 negative collision-Y、exact-contact gravity 与 type-family predicate 均未对齐。
- `Runtime.Y < -0.0001` 作为 in-flight gate 同样错误地忽略 effective floor。
- `LF2Weapon.OnLanded()` 通过四参数 wrapper 固定使用 landingY=0；但 weapon specialization、virtual dispatch 与 `_lastLandingVyBeforeClamp` snapshot 必须保留。
- 最小 seam：保留 `WeaponFlightPhysics`，改用现有 result core；以带 landingY 的 virtual overload 传递同调用栈 reference，避免新增跨 tick 字段和 snapshot schema；兼容旧无参 virtual fallback。

## 下一步

建立 `NTSD28-B4-F04-DERIVED-WEAPON-REFERENCE-001`，先写 pooled derived production 红灯，再实施。
