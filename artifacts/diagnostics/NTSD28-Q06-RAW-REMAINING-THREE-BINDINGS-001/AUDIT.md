# Remaining raw bindings audit

2026-09-21 / CONFIRMED_CAPTURE_GAP. Read-only current code evidence:
- NTSD28UnityEntityRawCapture declares verified47 and MissingBindings for combat.platformSourceSlot/environmentState/environmentSourceSlot; emitted values at lines96/98/101 remain null.
- EntityFieldContract maps the corresponding native fields platform_source_slot_f4/environment_state_320/environment_source_slot_160 to MissingBinding.
- NTSDEntityRuntime has independent PlatformSourceSlotF4, EnvironmentState320, EnvironmentSourceSlot160; canonical copy/reset are present. Existing platform/environment source and production packages must be reused, not recreated.
- Thus diagnostic missing does NOT mean production carrier missing. But existing raw47 comparisons cannot establish equality for these three fields. No claim of raw50 equality is currently justified.

Exact next amendment scope: diagnostic capture and tool descriptor, plus two existing raw-capture Editor test files and tool TraceContractSelfTest if its fixed missing-field fixture needs revision. Confirm latter test body before edits; do not mass replace47 throughout historical artifacts or unrelated gameplay matrices. Entity/raw object shape already has all50 fields. Inspect binding-status and identity contract compatibility before deciding if raw/trace schema can remain unchanged; no new persistent carrier/checksum payload is authorized here.

Tests must use distinct nonzero and negative sentinel values, retain Unk328=-3 non-alias assertion, verify each proper field rather than defaults, and cover missing/extra/type rejection. Fresh matching source/Unity capture must label actual source/content/schema identity and report every difference; binding completion is not proof of behavior equality. Only affected diagnostic tests/tool self-test and one targeted capture, not full character/Scene matrices or repeated stable SelfCheck.

Q06 total-exit audit remains open: finish this gap and reconcile live-reader/R02/R04-R13/R16 obligations. Platform shadow stays Q09; existing populated-target cross-World epoch deficiency stays explicit, not waived by this diagnostic change. Q07 has not started.
