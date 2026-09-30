# R06 natural ordinary revival, same-state Unity diagnostic

Status: `FOCUSED_TEST_PASS`. Scope: one Sasuke OID11 selected-armor hit on OID87 with initial lives2; 70 complete Driver ticks. This closes only the natural ordinary-revival subgate. R06 queued revival, owner/birth/visual observation, Q07 and BATCH-04 remain open.

Authority: root Logan EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, paired playable source. Root EXE LFR cannot inject target lives2; therefore the same-state reference for this case is the paired playable complete `GameSession28` run recorded in [`../NTSD28-R06-NATURAL-REVIVAL-ENTRY-001/ACCEPTANCE.md`](../NTSD28-R06-NATURAL-REVIVAL-ENTRY-001/ACCEPTANCE.md), not the unequal-state root replay. Formal source rows: [`../NTSD28-R06-NATURAL-REVIVAL-ENTRY-001/source-run-02/natural_revival_source_ticks.csv`](../NTSD28-R06-NATURAL-REVIVAL-ENTRY-001/source-run-02/natural_revival_source_ticks.csv). Unity raw rows: [`unity/natural-20260928T175518081-c8fbdd8d74094c4d8e48174e52a3dd84.csv`](unity/natural-20260928T175518081-c8fbdd8d74094c4d8e48174e52a3dd84.csv). The first failed Unity diagnostic raw is retained beside the passing file: it read uninitialized `SourceRuleXInt/ZInt` as zero, a test extraction error; all nine nonposition fields were already equal.

Original project Unity Editor PID11944, MCP port6401, EditMode one-case job `2fe3ea1b55b0471ba793fd499f37c7ff`: `1 passed / 0 failed / 0 skipped`, 35.37 seconds. Original Editor Tundra C# build succeeded with no C# error before the focused test. The scenario loaded selected formal Logan runtime content and project mode Asset. Exact fields `actor_action`, `actor_ko`, `target_present`, `target_hp`, `target_action`, `target_state`, `target_lives`, `target_phase`, `target_hold` matched for all 70 rows: 630/630 comparisons. Key observations:

| Completed tick | Source and Unity target | Attacker |
|---|---|---|
| 16 | HP -73, action 186, state 12, lives 2, hold -3 | action 264, KO 1 |
| 27 | HP -73, action 231, state 14, phase 30, lives 2 | action 0, KO 1 |
| 54 | HP 3, action 212, state 4, phase 19, lives 1 | action 1, KO 1 |

The scenario does not initialize `SourceRulePosition`, so the test checks physical positions under the approved D-024 fixed full-background scaling instead of comparing uninitialized source integers. With initial (550,542), X scale 2048/1333 and Z scale 1152/730, the largest source-to-Unity scaled X error was 1.154 px at tick24; Z error was zero. The focused assertion uses a 1.5-px bound for stepwise integer rounding. This checks the declared positional exception, not exact original-world pixel equality or unresolved collision-domain policy.

The replay helper's post-shutdown invariant passed: World object count 0, claimed runtime slots 0 and logic-reference borrowers 0. Four protected file SHA-256 values after the test equal the prior baseline: Battle Scene `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`, Menu Scene `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`, GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`, ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`. No production code, DAT, Scene or mode Asset was edited by this package. Formal EXE visual/physical-key Play and queued revival are not established by this EditMode result.

`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` passed (989 records, 20 governed code files in the shared dirty diff; existing historical path warnings). `git -c core.safecrlf=false diff --check` exited 0. No broad test suite was run for this diagnostic-only package.
