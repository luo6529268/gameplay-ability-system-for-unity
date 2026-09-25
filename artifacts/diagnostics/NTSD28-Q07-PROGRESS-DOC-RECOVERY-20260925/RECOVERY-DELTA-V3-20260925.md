# Recovery candidate v3 active-record index (2026-09-25)

This is an **index correction in review candidates only**. The current Change Record metadata classifies 272 records as active. All 272 IDs occur in the v2 STATE candidate and all 797 current Record IDs occur in `docs/ai/CHANGE-LEDGER.md`; nine active IDs were absent from the v2 handoff candidate. Include these nine with their existing Record statuses and exact Record paths so the handoff can recover governance references without silently changing any task's scope or status:

| Active Change ID | Current Record status | Record path |
| --- | --- | --- |
| `BUILD-LOCKSTEP-TEST-COMPILE-001` | `CODE_WRITTEN` | `docs/ai/CHANGE-RECORDS/BUILD-LOCKSTEP-TEST-COMPILE-001.md` |
| `CAMERA-PLATFORM-BACKGROUND-001` | `FOCUSED_TEST_PASS` | `docs/ai/CHANGE-RECORDS/CAMERA-PLATFORM-BACKGROUND-001.md` |
| `GOVERNANCE-S0-UNITY-CONTENT-AUTHORITY-DIRECTION-B-001` | `BLOCKED` | `docs/ai/CHANGE-RECORDS/GOVERNANCE-S0-UNITY-CONTENT-AUTHORITY-DIRECTION-B-001.md` |
| `MAPCFG-001` | `FOCUSED_TEST_PASS` | `docs/ai/CHANGE-RECORDS/MAPCFG-001-map-assets-catalog.md` |
| `MAPCFG-002` | `RUNTIME_PENDING` | `docs/ai/CHANGE-RECORDS/MAPCFG-002-boundary-runtime-source.md` |
| `MAPCFG-003` | `FOCUSED_TEST_PASS` | `docs/ai/CHANGE-RECORDS/MAPCFG-003-explicit-boundary-authoring.md` |
| `MAPCFG-004` | `RUNTIME_PENDING` | `docs/ai/CHANGE-RECORDS/MAPCFG-004-map-startup-integration.md` |
| `MAPCFG-005` | `CODE_WRITTEN` | `docs/ai/CHANGE-RECORDS/MAPCFG-005-boundary-asset-consolidation.md` |
| `NTSD28-USER-STATE9996-CHILD-VIEW-RATIO-WITNESS-001` | `FOCUSED_TEST_PASS` | `docs/ai/CHANGE-RECORDS/NTSD28-USER-STATE9996-CHILD-VIEW-RATIO-WITNESS-001.md` |

This index comes from metadata in the current Record files and does not claim these packages are current battle-alignment priorities. The three live NUL-corrupt documents remain unchanged. Replacing them with any candidate still needs explicit approval under `AGENTS.md`, followed by the real `Tools/Validate-ChangeLedger.ps1` and a content review of the restored documents.
