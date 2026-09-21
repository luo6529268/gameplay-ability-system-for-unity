# Partial acceptance — replay remains failed

Status: IN_PROGRESS / PLAY_PASS_LOCAL_IDENTITY_REPLAY_PENDING.

Source2 SHA83c2bc4e018e9d9f557086283598f98dd45709d09326bcddb8881b0aa5a5bffe; independent67 PASS. Existing focused9/9, captured2/2 and SelfCheck2026-09-21T03:32:42Z evidence retained without reruns.

Archived Play4 PASS (source result mtime03:38:11Z): sceneChecksumUnchanged true, renderer borrowers2→2. Q05 close PASS (mtime03:38:12Z): restored4→4, World objects/slots/logic borrowers/render borrowers all0 and remainedStoppedAfterTwoFrames true. Manifest records exact source mtimes and hashes. These are existing measured results archived now, not newly executed tests. Current on-disk Scene SHA remains BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6. No new claim about live Editor state is made by this archive.

Same-world retained-local-shell replay job eb4986b07a5d4f4abc247d7f6e345405 remains FAIL1/1, EntityIdentityMismatch, preserved in ../replay-identity-mismatch/results.xml. Snapshot target DAT200 legitimately changes to213; preflight rejects changed current data identity. Declared40 case already reproduces it, so this is not evidence that the new implicit40 admission introduced the restore defect.

Dependency: NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001. Do not delete guards, clear local shell references, rebuild pure-value shells, or move capture after transform to replace the failed retained-shell exit. StableId, entity-kind, corruption rejection and failed-preflight nonmutation must remain covered.

Scope exclusions: formal EXE dynamic player reproduction, physical keys, image parity, implicit40 writer projection, complete Q06 and Q07 content migration. Current source-derived diagnostics are not formal EXE recordings.
