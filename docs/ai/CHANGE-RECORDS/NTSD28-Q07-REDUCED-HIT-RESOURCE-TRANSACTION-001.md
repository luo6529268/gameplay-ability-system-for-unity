<!-- CHANGE-RECORD
id: NTSD28-Q07-REDUCED-HIT-RESOURCE-TRANSACTION-001
status: FOCUSED_TEST_PASS
change-kind: SHARED_REDUCED_CHARACTER_HIT_RESOURCE_TRANSACTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5UnarmoredHpConsumptionProductionEditorTests.cs
authority: formal root NTSD2.8-Logan.exe and paired battle_world.cpp selected-armor reduced hit resource call
evidence: OID87 X550 source/rootEXE/Unity 26-tick first difference at tick16 MP, formal hit damages 3/3/35/35, Unity and formal target HP424/action186 match
-->

# NTSD28-Q07-REDUCED-HIT-RESOURCE-TRANSACTION-001

2026-09-26 focused RED before production edit: original Editor recompiled with zero Console errors. New direct type-3-child→type-0-armor-target writer test job `a22e93c37a9e46f18ac1d4e2195a38d0` failed at root current PP expected426/actual400 after confirming reduced HP was applied. This validates the missing shared reduced resource transaction independently of the 26-tick raw first difference. At that point `BattleDamageWriter.cs` had not yet been edited by this package.

Pre-change: `ApplyAlternateDamage` writes reduced HP/MP, counters and credit but lacks native shared resource transfer. The previously fixed unarmored path uses `ApplyNativeStandardHitResourceTransfer` and writes current PP; the selected-armor formal branch calls the same native transaction after reduced damage. X550 attacker/target Unity PP452/352 versus formal500/404 at tick16; X1200 no-hit 2100 mapped fields equal. Later target horizontal position divergence starts tick20 and must not be attributed to this resource change.

Declared edit only to the two `code-path` scripts: focused reduced-hit writer test before production edit, then one shared helper invocation after reduced hit credit in `ApplyAlternateDamage`. No DAT, images, Scene, camera, ProjectSettings, content config, Menu or other nonbattle change. Expected side effects are native injury MP rewards and any configured drain/gain on accepted reduced type-0 character hits; input consumption counters and checksum may change only through those formal resource writes. No new manager/pool/worker or shutdown ownership.

Validation: original Editor compile/RED/GREEN; source-rootEXE/Unity X550/X1200 post-fix parity with separate classification for position; Ledger/diff and protected hashes. Rollback exact hunks only after review; preserve pre/post diagnostics and user work. Status cannot advance beyond evidence; Q07 remains open.

Actual change: added one call to the existing `ApplyNativeStandardHitResourceTransfer` immediately after reduced-hit credit in `BattleDamageWriter.ApplyAlternateDamage`, and added `Type3ChildToType0ArmorTarget_ReducedHitTransfersCurrentMp` in the declared Editor test file. No DAT, image, Scene, camera, mode/config, Menu or other nonbattle implementation changed. The existing helper itself and its current-PP correction belong to the preceding standard-hit package, not this change.

Original Editor recompiled with zero Console errors. After the RED, focused job `f6cd457447074d058d09eb8d062835cc` passed 2/2 (new reduced-hit case and neighboring standard-hit case). Post-fix original Editor raw X550 and X1200 requests both returned PASS. `entity-comparison-post.json` versus the unchanged formal root EXE reports X550 68 occupied rows × 21 fields = 1428 comparisons with 12 remaining position differences and zero MP differences; X1200 100 rows × 21 = 2100 comparisons with zero differences. At X550 tick16, attacker/target current MP now equal formal 500/404. Pre/post Unity raw entity rows change only those two current-MP fields at ticks16–26; both scenarios' domain and input/RNG JSONL rows remain identical. The position first difference persists at tick20 (formal preciseX558.4, Unity558) and is explicitly outside this resource package. See `artifacts/diagnostics/NTSD28-Q07-SASUKE-ARMOR-TARGET-FIRST-DIFF-001/RESOURCE-FIX-ACCEPTANCE-20260926.md`.

Result: `FOCUSED_TEST_PASS / SCOPED_MP_PARITY_POSITION_PENDING`. Natural physical Battle Play, remaining shared resource callers, the independent position discrepancy and complete Q07 exit are unverified. No full-battle equivalence claim.
