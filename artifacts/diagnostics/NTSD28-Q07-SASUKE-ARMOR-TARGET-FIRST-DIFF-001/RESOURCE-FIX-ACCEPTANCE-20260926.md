# Q07 reduced-hit resource transaction: scoped acceptance

Change `NTSD28-Q07-REDUCED-HIT-RESOURCE-TRANSACTION-001` is `FOCUSED_TEST_PASS / SCOPED_MP_PARITY_POSITION_PENDING`; BATCH-04/Q07 and D-024 remain open. The formal root EXE SHA-256 is `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The same strict OID11 Sasuke versus OID87 armored target, seed `0x28A55A5A`, LFR packets and 26 completed ticks were used before and after the fix. X550 causes four child hits at tick16; X1200 is the miss control. The native source-authored packets and formal root EXE traces were not regenerated for the post-fix comparison.

The original Unity Editor compiled with zero Console errors. New direct reduced-hit writer test `Type3ChildToType0ArmorTarget_ReducedHitTransfersCurrentMp` first failed with expected owner current PP426, actual400 (job `a22e93c37a9e46f18ac1d4e2195a38d0`). After `BattleDamageWriter.ApplyAlternateDamage` called the existing shared resource transaction after reduced-hit credit, focused job `f6cd457447074d058d09eb8d062835cc` passed 2/2, including the neighboring unarmored test. Both post-fix original Editor raw-capture result files report PASS. This package touched only the declared battle writer and focused Editor test scripts; the resource helper and current-PP fix were introduced by the preceding standard-hit package.

The mapped entity comparison is in `entity-comparison-post.json` (21 common fields on every occupied entity row):

| Scenario | Occupied rows | Field comparisons | Remaining differences | Resource result |
|---|---:|---:|---:|---|
| X550 hit | 68 | 1428 | 12 | Both actors' current MP equals formal values from tick16 onward: attacker500, target404. |
| X1200 miss | 100 | 2100 | 0 | Unchanged from formal and from pre-fix Unity. |

Pre/post Unity raw rows differ only in the two actors' `vitals.currentMp` at X550 ticks16–26: attacker452→500 and target352→404. The header assembly hashes changed as expected after recompilation. The X550 and X1200 `domain.jsonl` and `input-rng.jsonl` rows are bytewise-equivalent after JSON decoding. HP, action, child occupancy/lifecycle and other mapped entity fields remain unchanged by the resource fix.

The 12 remaining X550 differences are all victim horizontal position fields. First difference is completed tick20 `preciseX`: formal558.4 versus Unity558. Integer X diverges from tick22. This discrepancy existed before the resource change, is unrelated to the MP transaction, and requires its own source-coordinate/motion audit under Q07/D-024 before any movement edit. The earlier first-difference diagnosis and raw files are preserved.

The original Menu Scene SHA-256 is `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`; Battle Scene `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`; ProjectBattleModeConfig `88E10D43B047952FD3053A87B7E5F60D0F87A6CD1A23313F37EA1503C686F55C`. The three recovered v3 documents (alignment, handoff, STATE) remain NUL-free after their subsequent addenda. Natural physical Battle Play, remaining cpoint/resource callers and formal EXE GPU same-world presentation were not run by this package. Full Q07 alignment is not claimed.
