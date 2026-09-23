# NTSD28-R15-B0-SOURCE-V2-PRODUCER-001

Status: FOCUSED_TEST_PASS. Parent: R15 and BATCH-04/Q07. Diagnostic source-model producer only.

Authority and trigger: the formal playable kind transform updates a target's object ID without replacing its physical slot or allocation epoch. The frozen three-tick formal-source candidate OID213/action176 versus OID206/action0 reproduced this state but the v1 B0 writer throws before completing the bundle. `NTSD28-R15-B0-OBJECT-ID-CHANGE-CONTRACT-001` added an explicit strict B0 v2 validator that accepts a six-field `object-id-change` lifecycle event; this package makes the source-model producer emit it. It does not change formal C++ gameplay or claim a formal EXE runtime trace.

Declared code path: `Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp` only. New diagnostic outputs under `artifacts/diagnostics/NTSD28-R15-B0-SOURCE-V2-PRODUCER-001/` and Temp, plus Task/Change/Ledger/STATE/handoff/alignment documentation. No Unity script, scene, prefab, resource, ProjectSettings, formal J: source or nonbattle edit.

Behavior: keep default B0 v1 output and historical capture semantics. Add an explicit invocation option selecting B0 v2 only when a domain output is requested; reject unknown schema/invalid option. In v2, if a slot stays occupied with unchanged positive allocation epoch but object ID changes, emit one `object-id-change` event with existing six fields and equal previous/current epochs. Epoch growth remains reuse, decrease fails, and slot ordering stays ascending. Header raw schema must match the selected version. Main/B2 content and certificate=false stay unchanged; no artificial epoch increment, definition, frame40, or gameplay adjustment.

Acceptance: compile from the current formal playable/core source with the repository build tool; v2 kind scenario full three-tick main/B0/B2 bundle passes strict validators twice with byte-identical corresponding outputs, event206->213 and equal epochs; default v1 neutral scenario remains valid; requesting v1 for the kind scenario still rejects and marks bundle invalid. Compare completed main/B2 v2-run bytes against the previously validated same scenario where applicable, accounting for runner provenance header changes. Review actual diff and run Change Ledger. No Unity or broad battle tests for this diagnostic-only producer.

Rollback: inspect current user-owned diff then revert only this package's runner changes under repository approval rules; retain all earlier V3 content-identity changes and diagnostic artifacts.
