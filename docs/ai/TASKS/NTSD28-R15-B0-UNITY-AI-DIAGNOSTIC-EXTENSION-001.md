# NTSD28-R15-B0-UNITY-AI-DIAGNOSTIC-EXTENSION-001

Status: FOCUSED_TEST_PASS. Parent: R15 and BATCH-04/Q07. Tool-only strict trace-consumer repair. Scoped validation and remaining gates are in the Change Record and diagnostic REPORT.

Authority and trigger: current formal source-model/Unity V3 same-seed three-tick B0 captures from NTSD28-R15-NATIVE-KIND-V3-CAPTURE-001. Formal source B0 tick has kind/completedTick/input/rng/slots/lifecycleDelta. Unity exporter adds aiAcceptedTrace, a Unity-only diagnostic counter object, while B0DomainRawContract.TickProperties is an exact shared six-field set. Unity validator currently returns invalid/tick-properties before any domain comparison.

Declared code paths: Tools/NTSD28Parity/B0DomainRawContract.cs and Tools/NTSD28Parity/B0DomainRawContractSelfTest.cs. Task/Change/Ledger/STATE/handoff/alignment and diagnostic outputs are documentation/evidence. No Unity production or exporter change, no formal source, resource, Scene or nonbattle behavior edit.

Behavior contract: retain the strict shared B0 tick property set and the existing v1 source/Unity captures. Accept the optional aiAcceptedTrace only on Unity producer, only with its exact five named properties and typed nonnegative counters plus nonempty reason string. Authority producer must still reject the field; unknown extra fields and malformed extension must still reject. The extension is diagnostic only: B0 shared-domain comparator must not infer an authority counterpart or include it in equality. Certificate remains false.

Acceptance: add a focused RED self-test with Unity extension, then implement minimal producer-specific validation. Rerun B0 self-test and the same current source/Unity domain captures, record actual comparison including any shared differences/RNG topology. Run Change Ledger and scoped diff checks. Do not rerun unrelated battle cases. Rollback only the two declared tool files after reviewing current diff and respecting file deletion/overwrite approval rules.
