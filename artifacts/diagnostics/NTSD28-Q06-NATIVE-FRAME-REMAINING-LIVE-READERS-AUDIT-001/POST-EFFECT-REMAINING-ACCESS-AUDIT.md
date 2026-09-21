# Remaining post-effect access audit — 2026-09-21

Read-only review /root/effect_fall_review; root inspected current Kind0 caller. No new implementation or source witness yet. Do not call these formal player-visible bugs.

## Next priority: Kind0 direct post-effect

BattleDamageWriter.ResolveNativeKind0PostEffectAction previous reader at187 and ApplyNativeKind0PostEffectAction raw binder at333 remain legacy. HitPlan.ProjectNativeKind0PostEffectAction shares resolver at4921.
Formal battle_world.cpp apply_native_kind0_post_effect_action at251, called after effect override at6981: previous state !=13 gates effect3/30→200; effect2/21/22 or effect20 with previous !=18→203; counter0;203 facing uses final pendingX sign.
Prior NTSD28-B5-KIND0-DIRECT-POST-EFFECT-ACTION-001 and16 tests already verify action table, previous selection, counter, facing and ordering. Preserve, do not redo entire B5.
Remaining access differences: previous900 declaredstate13/18 rejected by old857 upper bound; implicit200/203 get legacy defaultwait1 instead of nativezero descriptor with correct frameId/native marker.
Suggested six minimal source representatives: previous900state13/effect3 suppressed; previous900state18/effect20 suppressed; previous900state0/effect3 allowed; implicit200/effect3; implicit203/effect20; one explicit200/203 control. Include full hit before/after/following, latch/previous/snapshot/counter/descriptor. Existing negative/zero/positive facing tests reused. Source comparison required because shared resolver can make actual and projection identically wrong.

## Following priority: type3 attacker post-hit selected access

BattleDamageWriter.ResolveNativeType3AttackerPostHitAction selected getter at150 and ApplyNativeType3AttackerPostHitAction binder at166 remain legacy; HitPlan5447 shares resolver. Formal battle_world.cpp apply_native_type3_post_hit_action at395: state3000 or3007 cover2/3; only hit_Fj0→10; raw action, counter0, motionX0; native selected descriptor dvx→motionZ, absent descriptor preservesZ.
B5 already covers gate, cover, zero-only fallback, dvx vsdvz, counter/X and missingdescriptorZ. New access classes are explicit900/dvx nonzero, implicit900/Zzero, lowimplicit/descriptor, declared999 vsmissing999, invalid1000/-1 raw action with missingdescriptor, zero→implicit10,3007cover2 andcover1 controls.
Important correction: ApplyNativeUnarmoredAttackerPostHit at1386 already uses native current/selected/binder; noncharacter reduced1658 consumes this native helper. Do not apply obsolete four-route B5 descriptions to current code. Remaining old shared consumers are standardtype0 at1188, alternate type0 at1789, specialobjecthurttail2033; last live dispatch must be traced before deciding representative coverage. Review suggestion of reduced representative must be adapted to the actual remaining alternate route, not retest already-native helper.

Overall Q06 remains open. Identity/cpoint/lateeffects/display/postdisplay/platformop30/raw3 and Q07 migration are separate. Formal content reachability and exact implementation Task/Change still required before editing these remaining production paths.
