# NTSD28-R15-B0-UNITY-V2-PRODUCER-001

Status: FOCUSED_TEST_PASS. Parent: R15 and BATCH-04/Q07. Unity Editor diagnostic capture producer only.

Authority and trigger: `NTSD28-R15-B0-OBJECT-ID-CHANGE-CONTRACT-001` freezes opt-in B0 v2. The formal-source producer has passed the actual kind scenario with a same-slot/same-epoch OID206->213 event. Unity's diagnostic writer still declares v1 and throws on that state, preventing a strict cross-producer B0 comparison. This package updates only the diagnostic capture format; it does not create the type3/action scenario carrier or change battle gameplay.

Declared paths: `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs` and `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`. Task/Change/Ledger/STATE/handoff/alignment and Temp test output are evidence. No Unity production, formal source, Scene, Prefab, content, package or nonbattle edit.

Behavior: preserve the current default B0 v1 header, event derivation and historical capture/request behavior. Add an explicit diagnostic request/test entry for B0 v2, only valid when `domainOutputPath` is present. Strictly reject unknown values. In v2, same slot and positive epoch with changed object ID emits the existing six-field `object-id-change` event, previous/current epoch equal. V1 keeps its prior rejection; birth/death/reuse, epoch ordering, `aiAcceptedTrace`, player input, RNG stream availability and main/B2 output remain unchanged. Do not increase an epoch, relabel the event as reuse, or change production lifecycle.

Acceptance: focused test-first cases prove default v1 remains, explicit v2 neutral scenario has the v2 header, and a synthetic same-epoch ID transition emits exactly one event while v1 rejects. Verify invalid version/missing domain output rejection and unchanged existing capture test. Compile in the original project Editor if available, run only focused tests, validate v1/v2 B0 output with the strict parity tool and run Change Ledger. The kind-dependent Unity scenario remains a separate Task; this package cannot claim cross-engine parity.

Rollback: preserve pre-existing diagnostic work in both files; review and revert only this package's changes under repository approval rules. No file deletion or cleanup.
