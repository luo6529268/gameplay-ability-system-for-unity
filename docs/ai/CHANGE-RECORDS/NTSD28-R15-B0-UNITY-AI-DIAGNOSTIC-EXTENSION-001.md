<!-- CHANGE-RECORD
id: NTSD28-R15-B0-UNITY-AI-DIAGNOSTIC-EXTENSION-001
status: FOCUSED_TEST_PASS
change-kind: TRACE_CONSUMER_UNITY_DIAGNOSTIC_EXTENSION
code-path: Tools/NTSD28Parity/B0DomainRawContract.cs
code-path: Tools/NTSD28Parity/B0DomainRawContractSelfTest.cs
authority: B0 shared-domain strict contract and current original-Editor Unity diagnostic output; formal source-model B0 producer remains unchanged
evidence: R15 V3 same-seed Unity B0 validation invalid/tick-properties caused by Unity-only aiAcceptedTrace field, so domain comparator has no result
-->

# NTSD28-R15-B0-UNITY-AI-DIAGNOSTIC-EXTENSION-001

Before/after behavior, scope, invariants, validation and rollback are fixed in the matching Task. Before edits, producer ownership of aiAcceptedTrace has been verified: only Unity exporter writes it; authority B0 writer does not. The tool must continue strict validation and preserve historical v1 captures. No data should be dropped or fabricated to make comparison green.

RED: added focused self-test for exact Unity AI diagnostic object, malformed Unity object and authority rejection before changing B0DomainRawContract. Release self-test output red-self-test.json reported 15 cases, one failure: unity-ai-diagnostic-valid expected valid-b0-domain-raw but got invalid. The two rejection tests and prior cases behaved as expected.

Implemented: B0DomainRawContract retains its six shared exact tick fields. If and only if producer is unity-diagnostic and aiAcceptedTrace is present, it admits one additional exact object with five named fields, nonnegative integer counters and a nonempty reason. Old Unity captures without the object remain valid. Authority captures containing it and extra/malformed Unity objects remain invalid. B0DomainRawComparator is unchanged and compares shared fields only; no formal AI counter is fabricated.

Final validation: Release self-test final-self-test.json passed 16/16, including old authority/Unity captures, valid Unity extension, authority rejection, malformed negative count and extra subfield rejection. The original same-seed three-tick Unity B0 file validates. The source/Unity B0 comparison returned shared-domains-equal-rng-topology-different with inputEqual, slotOccupantsEqual and lifecycleEqual true; rngTopologyEqual false, firstDifference rng.streamAvailability. The source has authorityCrt/authoritySynchronized, Unity has unityDeterministic. This is an existing topology limitation, not RNG equality or formal EXE proof. Evidence and exact boundary are archived under artifacts/diagnostics/NTSD28-R15-B0-UNITY-AI-DIAGNOSTIC-EXTENSION-001. Q07/R15 and total goal remain open.
