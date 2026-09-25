<!-- CHANGE-RECORD
id: NTSD28-Q07-NARUTO-NATURAL-PICKUP-001
status: VERIFIED
change-kind: EDITOR_BATTLE_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NarutoHeldAirActionProbeEditor.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable input routing plus kind2 candidate gate and formal OID2/OID120 DAT
evidence: original Editor compile and controlled full-Driver collision pickup plus held-air pic97 central/camera Play PASS; cleanup and Scene SHA stable
-->

# NTSD28-Q07-NARUTO-NATURAL-PICKUP-001

Before edit: existing original-Editor witness directly calls `LF2CharacterInteractionResolver.TryApplyPreInteraction` with Naruto frame60 kind2 ITR. It proves the production consumer and later held airborne action/pixel but bypasses candidate geometry and attack-edge gate. Formal `battle_world.cpp` requires an attack rising edge, unattached holder and a ground light weapon for kind2 candidate; formal `nar.dat` has the kind2 ITR on frame60, while standing native attack selects action60 or65 through synchronized RNG. Unity `BruteForceSceneQuery.AcceptReleaseKind2Candidate` names the legacy attack carrier `KeyJump`; its fresh edge corresponds to native attack, as established by the prior input-proxy audit. No production defect is inferred from naming.

Declared change: add only an opt-in `naturalPickup` branch to the existing Editor diagnostic. Drive actual standing attacks and collision candidate production through complete Driver ticks; log attempts/actions and require reciprocal holder/weapon links before the existing jump/air-attack checks. This branch must not call the pickup consumer directly or force the attack/pickup action. Preserve all previous request modes and cleanup. No production/content/Scene/nonbattle change, no new lifecycle owner. Rollback only this exact diagnostic branch after review; unrelated worktree content remains protected.

Planned validation: original Editor compile and focused Play, inspect per-tick first difference if pickup fails, verify World/slot/pool/roster and saved Scene state, Change Ledger validator and `git diff --check`. Physical keyboard and formal EXE same-world trace/pixels remain separate. Actual results will be appended.

Actual edit: added only `Request.naturalPickup` and attempt/action report fields to the existing Editor probe. When opted in, it drives up to four standing native-attack attempts through complete Driver ticks against the registered ground OID120 and waits for standing between attempts; it never directly calls `TryApplyPreInteraction` in this branch. The prior direct-consumer path remains default. The existing reciprocal-link assertion, airborne attack sequence and cleanup are unchanged. Compile/Play pending.

Validation: original Editor rebuilt Editor assembly after the source edit and pre-Play Console returned zero current C# errors. Focused `naruto-natural-pickup-20260926-01` PASS: one full-Driver native attack packet yielded pickup action115/link101/-1 at tick6, no direct pickup-consumer call in this branch, then standing0 tick11, airborne212 tick18 and held action30/pic97 tick20. Same run produced one exact central OID2/pic97 command and camera ROI57×57/481 nonblack pixels; 470/481 exact source-cell RGB memberships in independent PNG read. Camera, World/slot/pool/roster, request and saved Scene state restored; same Editor returned non-Play. Exact evidence and SHA values: `artifacts/diagnostics/NTSD28-Q07-NARUTO-NATURAL-PICKUP-001/ACCEPTANCE-20260926.md`. No focused NUnit/full SelfCheck was run for this opt-in diagnostic-only change. Controlled fixture placement, physical keyboard, root EXE same-state/pixel parity and general Q07 remain unverified.

Governance exit: `Tools/Validate-ChangeLedger.ps1` exited 0 with 840 Records and 32 governed code files in the current diff; `git diff --check` exited 0. The three live progress documents remain NUL-free. No old asset deletion or excluded original background/mode DAT use.
