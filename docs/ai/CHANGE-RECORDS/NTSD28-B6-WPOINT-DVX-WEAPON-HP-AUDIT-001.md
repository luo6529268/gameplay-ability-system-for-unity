# NTSD28-B6-WPOINT-DVX-WEAPON-HP-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-WPOINT-DVX-WEAPON-HP-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::settle_held_refill_objects WPoint DVX release and EntityState28::weapon_hp_31c writer set; EXE B1E13AE1, closure 39DDDA15.
evidence: Authority DVX branches do not write weapon_hp_31c; Unity held ThrowHeldWeapon calls the sole OnThrown override which resets WeaponFlightCounter to definition weapon_hp; production creates LF2Weapon for all 1/2/4/6 types; current Unity has 3 and release has 599 non-kind3 DVX WPoints; subsequent preservation package defined, no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / SUBSEQUENT_WEAPON_HP_PRESERVATION_ROUTED / PRODUCTION_HELD`

Authority held DVX release不重置`weapon_hp_31c`；Unity的唯一`OnThrownInternal()`调用却通过
`LF2Weapon.OnThrown()`把`WeaponFlightCounter`恢复成definition `weapon_hp`。生产type1/2/4/6
全部走`LF2Weapon`，当前content有3条non-kind3 DVX WPoint，因此该差异正式可达。

下一独立actual为
`NTSD28-B6-WPOINT-DVX-WEAPON-HP-PRESERVATION-PRODUCTION-001`，但排在kind-3 production之后，
且当前runtime stack未清，所以保持held。完整矩阵见
`docs/ai/TASKS/NTSD28-B6-WPOINT-DVX-WEAPON-HP-AUDIT-001.md`。

## CORPUS CORRECTION（2026-09-08）

current holder non-kind3 DVX由3纠正为184；weaponHP preservation owner/package保留，current矩阵扩大。

## HELD RELATION DOMAIN CORRECTION（2026-09-08）

完整ITR+OPoint held union把current non-kind3 DVX修正为186；原184是pickup-only。owner不变。
