<!-- CHANGE-RECORD
id: NTSD28-Q07-SASUKE-NATURAL-STATE15-MOTION-001
status: VERIFIED
change-kind: Q07_SASUKE_NATURAL_STATE15_MOTION_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired frame-motion/world tick; controlled release/Unity 26tick comparison
evidence: docs/ai/TASKS/NTSD28-Q07-SASUKE-NATURAL-STATE15-MOTION-001.md
-->

# NTSD28-Q07-SASUKE-NATURAL-STATE15-MOTION-001

Before script edit: production `LF2SpecialAttack` generic state15 post-physics duplicate velocity write was removed in `NTSD28-Q07-STATE15-POSTPHYSICS-VELOCITY-001`, with 26tick controlled release/Unity 3826/3826 compared fields. Existing Sasuke physical-key Battle Scene probe confirms birth/visual binding but only captures child Vx at birth; natural scene action12 Vx remains unobserved. Its file already contains prior governed diagnostic changes; preserve them.

Intended after: opt-in Sasuke request captures four OID440 action12 motion rows from the real Driver while preserving all existing probe behavior. Require Vx0 for each stable ID in the opt-in result, report missing action12 samples separately from a measured nonzero Vx. Only the exact Editor diagnostic script path is authorized; no production or content edit. Expected side effect is new diagnostic JSON fields for the opt-in request only. Task contains authority, prerequisites, acceptance, limits and rollback. Current status `IN_PROGRESS` before code modification.

Pre-code scope refinement: the existing Sasuke request JSON remains on disk from a prior run; do not overwrite it. The script will accept a separate uniquely named motion request and write into this Change's diagnostic directory, while preserving the existing Sasuke request route and result files.

Code written: the existing Editor probe now recognizes `Temp/NTSD28_Q07_SasukeState15Motion.request.json` independently from the prior Sasuke request and writes a unique result under this Change's diagnostic directory. Only this opt-in samples OID440 frame12-14 full-tick child motion, counts distinct frame12 stable IDs, checks their Vx against zero, records missed Editor-observation ticks, and extends the Sasuke-only tail by four ticks. All other probe routes retain their prior tail and predicates. Original Editor compile and Play are pending; no production or content file was edited by this Change.

Verified 2026-09-25: original Unity Editor imported the updated script and ran the unique natural physical L/D/J request `q07-sasuke-natural-state15-motion-20260925-1` in the saved Battle Scene. Result PASS: PP 500→400, frame264, four OID440 births, formal chi.png pic0 binding; stable IDs 101–104 each had Vx0 at actions12, 13 and 14 (12 motion rows). The Editor observer missed two unrelated tick samples, but all 12 target rows were present. Editor exited Play; no C# compiler errors were observed. Battle/Menu Scene and GameConfig disk SHA-256 values were unchanged. This is natural Unity scene evidence plus the prior controlled formal 26-tick comparison, not a full formal EXE natural same-world, visual, collision, lifetime or audio certificate. Evidence: `artifacts/diagnostics/NTSD28-Q07-SASUKE-NATURAL-STATE15-MOTION-001/ACCEPTANCE-20260925.md` and the linked JSON. No production, DAT, image, Scene, Menu or nonbattle file was edited by this Change. Rollback is the opt-in diagnostic hunk only, subject to repository approval rules.

Post-edit checks: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repository>` exited 0 (`Change ledger validation PASSED`, 18 governed code files in the current worktree diff; historical path warnings only). `git diff --check` exited 0. The focused one-Play target was run; broad EditMode/self-check suites were not rerun for this diagnostic-only extension.
