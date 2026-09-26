<!-- CHANGE-RECORD
id: NTSD28-Q07-REVIVAL-PEER-FULL-DRIVER-001
status: FOCUSED_TEST_PASS
change-kind: Q07_D024_REVIVAL_PEER_COMPLETE_DRIVER_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/revival_peer_full_session_source_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07RevivalPeerFullDriverEditorTests.cs
authority: formal root NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 step / BattleWorld28 advance_native_revivals; user D-024
evidence: docs/ai/TASKS/NTSD28-Q07-REVIVAL-PEER-FULL-DRIVER-001.md
-->

# NTSD28-Q07-REVIVAL-PEER-FULL-DRIVER-001

Pre-change: `NTSD28-USER-SOURCE-REVIVAL-AVERAGE-001` established focused production behavior and a temporary-World Play gate, but neither formal nor Unity complete Driver has shown source peer coordinates, RNG gate and post-revival integer timing for the same current DAT participants. Formal action230/state14 is present in indexed Lee DAT. Full Driver can alter the initial state before the revival pass; the candidate must be measured, not assumed.

Declared change is diagnostic only in the three exact code paths above and raw artifact folder named in Task. The shared capture helper has unrelated existing Q07 edits; preserve every pre-existing hunk. Runtime, DAT, Scene, project framework, mode Asset, camera and nonbattle behavior remain outside this Record.

Expected side effects: none on production; the two explicit fixtures must be distinguished from natural Play. The test helper must use the original Editor and existing ordered shutdown. Compare formal source-domain outcomes and Unity physical-domain ratio as separately declared; never mask a first difference by changing production or manipulating DAT. Rollback and acceptance are in the Task. Actual code, build, raw traces, failures and final status will be appended after execution.

First actual script: `Tools/NTSD28Q07Diagnostics/revival_peer_full_session_source_probe.cpp` uses the indexed formal Lee OID7 action230/state14, controlled dead slot0 lives2/render phase2 and same-group living peer slot1. It records 0–3 complete `GameSession28::step` results, coordinates, HP/MP/lives, render phase, frame count and native RNG state/calls for nonzero/zero source peer X. Formal compile is running; Unity test and strict schema have not yet been edited. No production/DAT/Scene code changed by this diagnostic.

Formal result: `formal/compile-01.log` exit0 with the paired playable build closure. Root formal EXE SHA reconfirmed `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. Indexed catalog has 330 entries and OID7 Lee registry71 action230/state14; `formal/run-01.log` exit0. Both complete-Session cases contain tick0–3 × 2 slots, 32 columns. At tick1 positive peer source X100 gives action212, revived HP500/lives1/source precise X113/Z603 and two synchronized calls ending site0x91; negative peer X0 also revives but retains source precise X50/Z600 with zero synchronized calls.

Unity diagnostic code now added in the declared shared helper and new Editor test `.cs/.meta`, plus exact positive/negative scenario JSON. Its original-project Editor assembly rebuilt after MCP `Assets/Refresh`; exact two-case EditMode job `4bef04361cd84c31aa5a5e202575a88f` is running. No Unity parity conclusion yet.

First Unity job `4bef04361cd84c31aa5a5e202575a88f` ran exactly two cases and wrote complete raw tick0–3 rows. Offline first-difference audit found only `mp` at revived slot0 ticks1–3: formal `current_mp=500`, raw test reader `Runtime.MP=77`; all other declared fields in both cases matched. Current formal `current_mp` maps to Unity battle vital `Health.PP`, and production revival writes `entity.Health.PP=500` while `Runtime.MP` is a separate carrier. The diagnostic reader is corrected to `Health.PP` and retains `Runtime.MP` as an un-compared extra column. This is test-field mapping, not a production repair; the exact two cases must be rerun.

Final original-Editor job `6b4a9aea35914d77a37ae387720d3458` ran two complete-Driver cases, 2 passed/0 failed. Formal and Unity source/lifecycle/RNG traces agree on 29 fields × 8 rows in each case; 2048x1152 physical positive and source-zero/physical-nonzero negative ratio assertions passed. Ordered-shutdown helper asserted zero World objects, slots and borrowers. Full raw names, first failed reader mapping, formal identities and proof limits are in `artifacts/diagnostics/NTSD28-Q07-REVIVAL-PEER-FULL-DRIVER-001/ACCEPTANCE.md`. This is a controlled gate only; natural Play/root-EXE/Q07/D-024 remain open.
