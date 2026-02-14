## 2026-02-12

- Kept implementation scope minimal: only skeleton flow + placeholder per-kind handlers for Character/Weapon/SpecialAttack.
- Kept timing contract unchanged: Character via `post_combo`, Weapon/SpecialAttack via existing TU `Interaction()`.
- Applied conservative gating before dispatch: self/null/invalid/dead/team/vrest + kind-service `ShouldHitTarget`.
- Updated arest/vrest only when dispatch returned accepted (`true`), matching planned acceptance semantics.
- Introduced `SimulationWorld.ItrKindService` and wired Character/Weapon/SpecialAttack interaction checks to prefer world-level service instance, with static handler kept as compatibility fallback.
- Left SceneQuery geometry-only boundary unchanged in this step (no behavior/rule logic reintroduced).
