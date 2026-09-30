<!-- CHANGE-RECORD
id: NTSD28-Q07-HIDAN-PHYSICAL-FIRST-ACTION-RNG-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HidanPhysicalBattlePlayProbeEditor.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable input_routing.cpp route_native_standing_attack
evidence: docs/ai/TASKS/NTSD28-Q07-HIDAN-PHYSICAL-FIRST-ACTION-RNG-001.md
-->

# NTSD28-Q07-HIDAN-PHYSICAL-FIRST-ACTION-RNG-001

Pre-script state: the existing original-Battle Hidan physical-key probe records the first action60 and canonical input, but not its pre-step synchronized RNG state or the actual `0x82` branch. The paired playable uses `(synchronized_next(0x82, 2) + 12) * 5`; a separate same-initial-state Unity raw fixture reached65. The saved physical Play differs in stage, Z and input phase, so the rule mismatch is unproven. This Change only adds non-consuming RNG observation to the existing Editor-only probe. No production, DAT, Scene, saved Asset, collision, camera or nonbattle change. Acceptance and rollback are in the Task Contract.

Post-script before compile: only the declared Hidan Editor probe changed. Each completed full Driver tick now captures native synchronized RNG calls/counter/index/last-site before and after; when precisely one call occurred and its last site is `0x82`, it derives the two-way result from the captured post-state table and counter. No RNG call is made by observation, and the existing physical input, 40-tick capture and shutdown path remain in place. Generated compile, original Editor import/Play and classification are pending. Rollback remains test-probe-only.

2026-09-28 scoped runtime closure: generated Editor build 0 errors/243 warnings, original Editor refreshed a newer script assembly, and one original saved Battle Scene X580 physical J/J/K/K request produced `PASS_SCOPED_PHYSICAL_CHAIN` over 40 complete ticks. First attack tick6 had synchronized calls/counter/index `0→1`, last callsite `0x82`, reconstructed result0 and action60; formal root tick2 has the same cursor step and action65, implying result1 under the paired playable formula. These are different stage/Z/input-phase initial worlds, so this classifies the measured difference as an RNG branch difference, not a production rule defect or a same-world parity failure. Ordered shutdown Completed/zero residue, original Editor back idle/non-Play and Scene clean, Battle/Menu/GameConfig/project-mode hashes unchanged. No production, DAT, Scene, saved Asset, collision, camera or nonbattle change. [Evidence](../../../artifacts/diagnostics/NTSD28-Q07-HIDAN-PHYSICAL-FIRST-ACTION-RNG-001/ACCEPTANCE-20260928.md). Q07/D-024 and the total goal remain open.
