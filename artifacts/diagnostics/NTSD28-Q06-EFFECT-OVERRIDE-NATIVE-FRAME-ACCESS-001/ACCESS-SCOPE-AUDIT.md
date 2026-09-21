# Effect override native frame access scope

Current live paths: DamageWriter ApplyStandardCharacterDamage / ApplyWeaponDamage / ApplySpecialAttackDamage -> ResolveNativeEffectActionOverride -> ApplyNativeEffectActionOverride. Its target latch and previous readers use old GetFrameDataById, and its two final positive action writes use old raw binder. Post-effect kind0 and type3 post-hit remain separate transactions with independent timing/counter/motion.

Correction to initial peer audit: LF2Entity.GetFrameDataById delegates LF2FrameCache.GetFrameDataById, which returnsnull for id>=857 before reading its1000-slot array. Thus DECLARED900/state602 or firstBDY50 can lose suppression; DECLARED900/state12 with pickedact12 can falsely reject previous-state matching. Old implicit high null vs nativezero equivalence applies only to undeclaredstate0 in these specific gates. Initial claim that all declared frames read identically is withdrawn and must not drive scope.

Conditional HitPlan identity projection reads attacker definition with projected target latch/previous using the same old getter. Ordinary target source tests do not certify this transformed-definition branch. It requires its own small actual-transform/projection evidence; do not change/close it solely by proximity or count normal fixture tests as identity coverage.

Formal330 static801effect8 ITR declarations; other effect9..16 absent. catchingact/pickedact all0; positivecaught values and-3 exist. First frame/field policy matches source lookup inventory, but dynamic target/identity eligibility not asserted. Old B5 behavior/type/suppression/HP rules remain closed, current task onlynative frame access consistency.

Source16 ordinary kind0/effect8 should preserve entire hit and following outputs. Give normal hurt/death support frames explicit descriptors if necessary to isolate already-closed reactions from missing-frame work elsewhere; never patch source behavior or assert overrides when suppressed. Before/after/following must distinguish latch/previous/ticksnapshot and damage-before-target-HP guard. No framework/Scene/resource changes.
