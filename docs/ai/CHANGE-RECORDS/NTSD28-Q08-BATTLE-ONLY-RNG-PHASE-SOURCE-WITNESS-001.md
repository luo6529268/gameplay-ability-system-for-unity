<!-- CHANGE-RECORD
id: NTSD28-Q08-BATTLE-ONLY-RNG-PHASE-SOURCE-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: formal Logan playable start_selected_battle random and input-phase restoration; default BGM selection0
evidence: isolated native build exit0; two identical formal-content fixture runs; RNG-PHASE-SOURCE-WITNESS.md
-->

# NTSD28-Q08-BATTLE-ONLY-RNG-PHASE-SOURCE-WITNESS-001

Created before diagnostic C++ edit. Task: `docs/ai/TASKS/NTSD28-Q08-BATTLE-ONLY-RNG-PHASE-SOURCE-WITNESS-001.md`. Before: first and second result host branches have source-model witnesses, but neither records the random/input-phase transaction of the first recreation. The formal code captures prior native RNG and input phase, reinitializes, restores them and may draw random BGM. Planned separate fixture will record exact before/after fields and assert the phase/call sequence; build in isolated native tree and double-run. No governed Unity project code path (`NONE`), J: authority write or original Scene/content change. Validation and rollback are in Task.

After: only the declared diagnostic CPP and isolated native test copy changed. Focused build and two executions exited0; output SHA-256 `4B6AEAE85CF1E459A73607CEA07087B15076ED67FE34F7D10A9ABF3AB058EE2D` both times. CRT state/calls and input phase 1 persisted; synchronized calls increased by one at BGM site `0x004021E0`. Unity rematch host, real Play and formal EXE presentation remain unverified. Detailed evidence: `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/RNG-PHASE-SOURCE-WITNESS.md`.
