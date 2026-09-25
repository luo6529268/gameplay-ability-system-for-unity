<!-- CHANGE-RECORD
id: NTSD28-USER-D024-PROTECTED-X-SCENE-WITNESS-001
status: VERIFIED
change-kind: DIAGNOSTIC_PROTECTED_OBJECT_FULL_DRIVER_PLAY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28D024ProtectedXSceneWitnessEditor.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable BattleWorld28::settle_ordinary_stage_bounds
evidence: docs/ai/TASKS/NTSD28-USER-D024-PROTECTED-X-SCENE-WITNESS-001.md
-->

# NTSD28-USER-D024-PROTECTED-X-SCENE-WITNESS-001

Before: protected OID122/123 clamp has focused RED/GREEN and SelfCheck, but no original Battle Scene production Driver tick. Formal type6 frame0 is `pic999/state3005/wait0/next1000`, so lifecycle timing may prevent a simplistic survivor assertion. Existing D-025 TTL probe belongs to a closed independent contract and is untouched.

Intended after: one diagnostic-only request captures two formal type6 objects across a full tick, recording physical/source clamp and possible terminal recycling, with world/pool/Scene protection. Precise scope, invariants, validation and rollback are in the Task. No production or DAT change is authorized. Current state `IN_PROGRESS` before script creation.

Code written: new single Editor-only request probe in the exact declared script path. It requires a clean saved Battle Scene, waits for the existing formal World, registers OID122/123 type6 at transient slots, records both coordinate domains across one real Driver tick, then unregisters and compares renderer borrowers before leaving Play. Terminal-frame recycling is reported as a lifecycle limit rather than an alignment pass. Original Editor compile and fresh Play result are pending. The preceding IN_PROGRESS sentence is the pre-code snapshot.

Original Editor validation: `refresh_unity` force/all imported the script and generated its `.meta`; editor console returned zero errors. Request `d024-protected-x-play-1` ran in the saved `NTSD_Battle` scene through one production Driver tick (0 -> 1), result `PASS`. Formal OID122/123 remained at frame 0 across that tick; physical and source-rule X moved 50 -> 100 and 2030 -> 1940 respectively, with integer source-rule mirrors 100/1940 and stage width 2040. Object count 4 -> 4, claimed slots 2 -> 2 and renderer borrowers 2 -> 2 after explicit cleanup; result `cleanupPassed=true`. Battle Scene disk SHA-256 remained `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; Menu remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`. Evidence: `artifacts/diagnostics/NTSD28-USER-D024-PROTECTED-X-SCENE-WITNESS-001/original-editor-d024-protected-x-play-1.json` (SHA-256 `80E8D34553B416256CDBAAC45A3C635492B96F6F44EB19697F17BEE79584BD23`) and sibling `ACCEPTANCE-20260925.md`. Status `VERIFIED` applies only to this controlled scene witness; natural drop/consumption, formal EXE same-condition playback and parent Q07 remain open. Rollback remains removal of this new diagnostic script/meta and package docs only, subject to repository approval rules.

Governance validation after the result and status update: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` exit 0 (`Change ledger validation PASSED`, 815 Records, 18 governed code files in the then-current diff); `git diff --check` exit 0. Historical Record path warnings were emitted by the validator but no errors. No wider regression suite was rerun for this diagnostic-only script.
