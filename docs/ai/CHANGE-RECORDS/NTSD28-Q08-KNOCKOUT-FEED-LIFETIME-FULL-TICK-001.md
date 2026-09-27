<!-- CHANGE-RECORD
id: NTSD28-Q08-KNOCKOUT-FEED-LIFETIME-FULL-TICK-001
status: FOCUSED_TEST_PASS
change-kind: Q08_KNOCKOUT_FEED_LIFETIME_COMPLETE_TICK_DIAGNOSTIC
code-path: Tools/NTSD28Q08Diagnostics/negative_environment_ko_lifetime_source_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08KnockoutFeedLifetimeFullTickEditorTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; paired playable GameSession28 and BattleWorld28 newest-tail prune
evidence: artifacts/diagnostics/NTSD28-Q08-KNOCKOUT-FEED-LIFETIME-FULL-TICK-001/ACCEPTANCE-PENDING.md
-->

# NTSD28-Q08-KNOCKOUT-FEED-LIFETIME-FULL-TICK-001

Pre-change: the existing negative-environment KO complete-tick fixture proves event time12 but stops at tick13. The feed lifetime World state has original-Editor focused coverage, not same-state full-tick expiry. Formal `battle_time == 0` is protected from expiry, so the existing held-CPoint time-zero event cannot exercise the 70-tick boundary.

Planned result: extend only the already-proven time12 positive/control formal session through the native expiry boundary, then compare original-Editor full Driver tick rows using the project-owned mode Asset. A measured difference will be retained as evidence and handled under a separate production Change ID. No gameplay, DAT, Scene, mode Asset, old resource or nonbattle code is authorized for this change.

Owned symbols: new paired C++ diagnostic; new exact Editor NUnit test and `.meta`. Existing Q08 scenario/harness are read-only dependencies. Side effects are only diagnostic outputs and test-created World state, which must be disposed by the existing replay helper. Validate C++ build/run, original Editor compile/focused test, four protected SHA values, `git diff --check` and Change Ledger. Scope and rollback are in the [Task Contract](../TASKS/NTSD28-Q08-KNOCKOUT-FEED-LIFETIME-FULL-TICK-001.md). Status advances only with actual evidence.

Source diagnostic built with the 28 formal core and two playable translation units after adding the Windows `-municode` entry-point option; the first link without that option failed on missing `WinMain` and made no executable. Both independent 84-tick runs then exited 0 and produced identical positive/control CSV bytes. The first 13 rows of each match the earlier accepted negative-environment source trace. Positive event time12 is present through tick82 and pruned on tick83, while the HP50 control is only initially nonlethal: it later records a time72 event, still present at tick84. The new Unity Editor test is written but not yet compiled or run; no production first difference has been observed. No production code, DAT, Scene, mode Asset or nonbattle path changed.

2026-09-27 final scoped evidence: original Editor refreshed and compiled the new test; exact job `672fa8e62f9b448c8041acfe49f68b3a` 1/1 PASS. The formal and Unity production traces each contain 84 completed ticks for HP5 and HP50, 17 fields per row, with zero text differences across all 168 rows. Source run1/run2 CSV hashes match; old 13-tick source prefix matches both cases. The project Asset record is present with lifetime70; time12 event survives tick82 and disappears tick83 while a later time72 event remains. A first MCP observation timed out, but the same job later reported terminal success; no rerun was made. Four protected hashes and `.meta` GUID uniqueness passed. New test and C++ diagnostic are the only script edits, with no production/asset/Scene/nonbattle change. Acceptance details and remaining limits are in the linked artifact. Status is `FOCUSED_TEST_PASS`, not Q08/Q09 aggregate verification.

Final scope checks: `git diff --check` exit0; Change Ledger validator PASS (905 Records / 19 governed code files in the current dirty tree). No broad SelfCheck or extra character matrix was run for this test-only package; production did not change.
