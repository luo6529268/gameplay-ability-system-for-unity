# Current Type3 attacker selected-frame access scope

Read-only independent review /root/effect_fall_review plus root source/Unity inspection, 2026-09-21.

Three current old-shared-helper consumers:
1. Type0 unarmored ApplyStandardCharacterDamage -> ApplyNativeType3AttackerPostHitAction.
2. Type0 reduced route selected by ApplyStandardCharacterDamage -> ApplyAlternateDamage -> shared helper.
3. Normal type3 kind0 target through LF2SpecialAttack.Hit/generic typed dispatch -> ApplySpecialAttackDamage -> ApplySpecialObjectHurtTail -> shared helper. This is live, not merely an unused method.

Already-native weapon/type5 unarmored and noncharacter reduced via ApplyNativeUnarmoredAttackerPostHit are excluded. Old B5 four-route descriptions must not be used to redo those paths.

Type3 target conditions: avoid kind9, non-kind0, matched3005/3006 early pairs and locked identity IDs. Target3005 and attacker3000 avoid early match; selected attacker900state0 avoids later pair reset and state1002 tail. Effect0/catching0 excludes override. Source runtime candidate/result must still confirm eligibility.

Public formal APIs are resolve_ordinary_unarmored_standard_hit and resolve_ordinary_type1_armor_standard_hit; there is no public resolve_ordinary_reduced_hit. Type1 source case must prove selected type1 and actual reduced result, not merely carry a reduced label.

Production access mismatch: selected old GetFrameDataById rejects declared857..999 and returns legacy wait1 shared empty descriptor for missinglow; native supports zero frames0..998 and declared999. SetFrameTickDirect writes raw action even negative but binds old descriptor; scoped native raw binder preserves current latch/history and no entry events. Shared projection uses resolver, so actual/projection equality alone can miss shared incorrect Z.

Formal330 raw frame-header inventory has no duplicateframe documents:893 declaredlow destinations,6 implicitlow allOID221frames0..5; those six have neitherITR norBDY. No dynamic player-visible discrepancy claimed. Source diagnostic boundary coverage remains necessary for declared runtime contract, not proof of a specific formal character bug.

## Captured consumer and projection boundary correction

Type3 attacker belongs to ObjectInteractionTickAll, not character PostInteractionTickAll (SupportsObjectInteractionPhase, LF2SpecialAttack.TryGetBattleHitCandidateConsumer(Object)). Initial test used wrong pass; original failure archived, production unchanged.

Independent reviewer confirmed CanProjectStandardType3DamageWriterEffect explicitly excludes target ObjectFlying3005; mixed attacker3000/target3005 also fails matched-state sync and OID77/78 identity gates. ShouldObserveWriterEffect is false. Captured case12 actual source comparison remains mandatory in both modes, but observedwriter0 is not projection PASS. Case1/11 must still observe writer>0. This existing unsupported projection domain remains explicit under Q06.
