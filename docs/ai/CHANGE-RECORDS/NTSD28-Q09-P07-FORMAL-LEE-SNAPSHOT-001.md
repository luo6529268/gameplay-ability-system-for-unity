<!-- CHANGE-RECORD
id: NTSD28-Q09-P07-FORMAL-LEE-SNAPSHOT-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q09Diagnostics/lee_shadow_snapshot_probe.cpp
authority: formal root NTSD2.8-Logan EXE and paired playable GameSession28 step/snapshot
evidence: docs/ai/TASKS/NTSD28-Q09-P07-FORMAL-LEE-SNAPSHOT-001.md
-->

# NTSD28-Q09-P07-FORMAL-LEE-SNAPSHOT-001

Before edit: no dedicated formal Lee J,L render-snapshot shadow/body command report exists. The existing formal LFR exposes entity state, while the Unity Battle Play exposes its central command counts; this diagnostic bridges exactly that evidence seam. The single new C++ tool is constrained by the Task Contract and does not change battle behavior or authoritative source.

Expected side effects: new read-only paired-source diagnostic executable/output under its own artifact folder. No runtime mutation beyond an isolated in-process fixture. Acceptance, protection and rollback are in the Task Contract. Validation pending.

2026-09-28 implementation: added only `Tools/NTSD28Q09Diagnostics/lee_shadow_snapshot_probe.cpp`. It creates the established Lee/Opponent formal Session, submits the exact J@2/L@3–4 input, steps 13 ticks and emits public render snapshot OID204 body/shadow/control commands for tick5–13. It refuses to overwrite an existing output. No official source or Unity production/test code was changed.

Validation: formal 28-core-source + playable GameSession/selection compilation exit0 with no diagnostics; first wrong catalog-root attempt exit4 retained, corrected formal runtime-root run exit0, no-overwrite negative exit3 with unchanged TSV SHA. Formal tick6 five visible child bodies/zero child shadows/three ordinary shadows; root EXE LFR tick6–13 child slot/action/pic 40/40 same. Existing original Unity Battle Play has five child bodies, zero child shadows and ordinary-shadow control; this is command-level field-gate correspondence, not strict same-state GPU parity. Exact hashes, paths, raw output, limits and protected Scene hashes are in `artifacts/diagnostics/NTSD28-Q09-P07-FORMAL-LEE-SNAPSHOT-001/ACCEPTANCE.md`. P-07/Q09 and total goal stay open.

Final checks: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exited 0 (`Change ledger validation PASSED`, 976 records, 13 governed code files in the current tracked diff; historical non-current-path warnings remain). `git -c core.safecrlf=false diff --check` exited 0. The new untracked tool was compiled and executed directly but is not included in the validator's current tracked-diff count. Battle/Menu Scene SHA matches the Task baseline. No Unity tests were run because the package did not change Unity code.
