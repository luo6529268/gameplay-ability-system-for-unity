# NTSD28-R15-VERSIONED-TRACE-HEADER-001

Status: CODE_WRITTEN / TOOL_SELF_TEST_PASS / UNITY_COMPILE_PENDING. Parent: BATCH-04/Q07 and R15. The formal Logan playable loads selected mode input before constructing battle options; the Q07 production catalog now uses a five-component V2 content identity when mode exists. The original Unity Editor has not compiled that change. The existing Unity trace emitter currently combines a V2 digest with a V1-only property set; the parity tool accepts only V1 and old aggregate/checksum schema 25/28, while current Unity emits 26/29. See `../NTSD28-Q07-MODE-COMBO-R15-IDENTITY-IMPACT-001/REPORT.md`.

## Exact code ownership

- `Assets/NTSD/Scripts/Test/Editor/NTSD28TraceContentIdentity.cs`: emit an exact V1 or V2 content header from the catalog identity, and reject incomplete mode components.
- `Tools/NTSD28Parity/TraceContentIdentity.cs`: strictly validate V1/V2 component sets, recomputed digests, scope and supported exact schema tuples; include contract/schema in the comparison identity key.
- `Tools/NTSD28Parity/TraceContractSelfTest.cs`: preserve historical V1 cases and add V2 vectors, malformed header and cross-contract/schema rejection cases.
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TraceIdentityEditorTests.cs` and `NTSD28Q06FusionCompositeContentIdentityEditorTests.cs`: update only the formal-root expectations to the independently verified V2 vector; retain synthetic absent-mode V1 checks.

Governance: matching Change Record, Ledger, STATE, current handoff and alignment R15. No native formal-source edits, Scene, Prefab, ProjectSettings, gameplay producer, old resource, nonbattle code or unrelated fixed-vector probe belongs to this package. The native diagnostic runner remains V1/25/28 until a separate exact Task/Change captures selected mode bytes and supported schema. No cross-version compatibility bypass is allowed.

## Acceptance and rollback

The old V1 header and historical captures must remain valid. A V2 header must carry five component hashes and the explicit V2 input contract, use a scope that names mode, and recompute its raw/semantic/projection. Missing, half-present, extra or wrong-version fields must fail closed. Accepted schema tuples are exact historical 17/25/28/2/2 and current 17/26/29/2/2; their identity keys must differ. V2 is valid only with the current tuple. V1/V2 and old/current schema comparison must stop at the header; same-version V2 may enter tick first-difference only after each side validates. Run the parity tool's focused self-tests and build; run the two affected Unity EditMode tests only in the original project after it recompiles. If that Editor remains stale, report Unity compile/runtime pending. Rollback is review and reversal of only the five declared code-file diffs plus this package's governance changes; preserve the Q07 input/published code and user work.
