# NTSD28 B5 status producer split manifest

| 包 | Authority | Unity owner/缺口 | 状态 |
|---|---|---|---|
| ITR fields | `InteractionRecord28` 13 status fields | `InteractionArea`/Converter/Copy/ECS projection/fingerprint | VERIFIED |
| type0 encoded | `apply_confirmed_input_statuses` ordered sync RNG | `BattleDamageWriter.ApplyStandardCharacterDamage` | VERIFIED |
| type0 join/mimic | `apply_native_join_and_mimic_side_effects` | type0 same-hit immediate activation | VERIFIED |
| type0 arm | `arm_native_unarmored_hit_motion` | standard character damage status/pending-Z arm | VERIFIED |
| other-target audit | non-type6 encoded+join；all-unarmored arm | weapon/special/other owner split | VERIFIED |
| non-type0 encoded+join | type1/2/3/4/5；type6 skip | weapon/special/other production insertion | VERIFIED |
| non-type0 arm | type1/2/3/4/5/6 | weapon/special/other reaction→horizontal seam | VERIFIED |
| content | runtime decoded DAT 92 status-field ITR rows | frozen Unity Config 0 rows | USER_CONTENT_STRATEGY_PENDING |
| KO event | `record_native_knockout` event push | event sink缺失 | B8_PENDING |

Encoded field order:

```text
poison -> weak -> bound -> facing -> manacle -> delay -> join -> mimic
       -> dx -> dy -> dz -> gain -> confus -> optional 7 remap calls
```
