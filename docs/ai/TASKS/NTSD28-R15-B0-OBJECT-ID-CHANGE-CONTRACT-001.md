# NTSD28-R15-B0-OBJECT-ID-CHANGE-CONTRACT-001

Status: FOCUSED_TEST_PASS. Parent: R15 and BATCH-04/Q07. Scope: diagnostic B0 contract and comparator only.

Authority and trigger: the formal playable `BattleWorld28` kind transform can replace a type-3 target's object ID in the same physical slot without increasing its allocation epoch. The formal-source three-tick OID213/action176 versus OID206/action0 preflight reproduced OID206 -> OID213 at tick 1. The current B0 v1 snapshot validator rejects this state as `occupant-changed-without-allocation-epoch`, so that capture cannot be compared. This is an observation-format defect, not evidence of a Unity gameplay difference.

Declared code paths: `Tools/NTSD28Parity/B0DomainRawContract.cs`, `Tools/NTSD28Parity/B0DomainRawContractSelfTest.cs`, `Tools/NTSD28Parity/B0DomainRawComparator.cs`, and `Tools/NTSD28Parity/B0DomainRawComparatorSelfTest.cs`. Documentation and Temp diagnostic outputs are supporting evidence. No producer, Unity production, authority source, resource, Scene, Prefab, or nonbattle changes in this package.

Behavior contract: keep v1 schema and its same-epoch object-ID rejection exactly intact. Add opt-in v2 raw schema that derives one ordered `object-id-change` lifecycle event when a slot remains occupied at the same positive epoch but its object ID changes. The event retains the existing six fields and both epoch fields equal the unchanged epoch. Birth, death, reuse, epoch monotonicity, strict field sets, and all unrelated rules remain unchanged. Reject a missing/wrong/extra event, an invalid epoch, and a cross-version comparison; never normalize v1 to v2. The descriptor must state the additional v2 event and provenance. A v2 comparison may retain the existing report format if the input versions match; no certificate may become eligible.

Acceptance: first add focused synthetic RED cases for v1 rejection, valid v2 object-ID change, malformed v2 lifecycle event, and v1/v2 comparison rejection. Then implement the narrow parser/comparator contract; run the tool build and B0 contract/comparator self-tests. Rerun the previous valid v1 capture to prove compatibility. Do not claim the native/Unity kind scenario is aligned until both producers opt into v2 and the same scenario is captured on both sides. Run the Change Ledger validator and review the exact diff.

Rollback: revert only the four declared diagnostic-tool files after checking their current user-owned diffs and explicit repository deletion/overwrite rules; preserve all historical capture artifacts.
