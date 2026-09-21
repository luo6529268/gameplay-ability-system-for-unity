# Current fusion authority gap audit

READ_ONLY_FINDINGS / UNITY_TRANSACTION_NOT_ALIGNED.

Formal GameSession loads actual decoded fusion.dat before locked fallback, passes catalog+two feature gates to tick. C12 geometry→fusion→hits and C25h timer placement already VERIFIED; do not redo. Formal advance_native_fusions loops records then primary slots0..19; source ids primary<10, partner<20, type0/alive/no pending/group/HP/record-state/timer/proximity gates. respond1/2/3 use external first/second/both gates; cover1 bypasses timer requirement; extra-slot partner grounds against collision_y_reference, not absoluteY0. Native descriptor validity replaces857 limit.

Actual two records:7/8→51,hp177,mp500,respond1,decrease4500,wait900,state2,action290,frame112,chp0,hit_ja0,cover0;10/11→52,hp375,mp500,respond0,decrease200,wait100,state2,action310,frame112,chp1,hit_ja1,cover1. Both hurt-range9..260 inclusive.

Unity BattleOid5152RuntimeModule hardcodes first record and GameMode1 bypass; lacks second record/catalog. Merge writes state before unchecked TryApplyRuntimeIdentity; source validates fused definition/action first. Source action commit setsaction/latch/snapshot/counter0 and resets opoint/sound latches. Definition publication updates objecttype,AIprofile,drop,maxMP,weaponHP,incoming damage scale with mode multiplication; Unity helper only wrapper/id/WeaponCount and oldgetter. Additional gate194/displaytimer/revivevisual fields require mapping.

Source defuse validates suspended partner and BOTH original definitions/actions before writes. Unity mutates primary before partner check and invokes partner.Reset plus extra resets; source restores suspended entity without genericReset, preserves unspecified fields including some speed/history/state. Source copies complete position+collision reference, does not universally setY0/Vz0. chp1 does nothalveHP. These need complete transaction migration, not a getter-only patch.

Next source witness starts with both actual records and two failure controls, then derive exact Unity data/carrier/consumer plan. No Unity production or resources changed. Q07 migration remains separate; new fusion catalog/runtime data interface dependencies must be explicit before implementing. Old B3 C12/C25h positions remain verified; internal behavior not closed by them. No source-derived output may be described as formal EXE recordings.
