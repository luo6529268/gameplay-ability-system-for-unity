<!-- CHANGE-RECORD
id: NTSD28-Q08-ATLAS-ALLOCATION-TRACE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleAtlasResources.cs
authority: Q08 formal-content second-result Scene validation blocked by two matching atlas Texture2D allocation crashes
evidence: ATLAS-ALLOCATION-TRACE-RESULT.md; plan145, array budget512MiB rejects 2.43GB payload, ordered page88 OOM, no XML
-->

# NTSD28-Q08-ATLAS-ALLOCATION-TRACE-001

Created before script edit. Before: atlas resource builder chooses array/ordered pages and allocates textures without pre-allocation plan or page progress in the crash log. Planned after: command-line opt-in trace only, recording plan count, array decision, and each ordered-page allocation boundary. No behavior change when flag absent; no semantic change when present. Failure handling, resource cleanup and atlas binding remain identical. Acceptance and rollback are in the Task. Formal EXE/content authority and Q08 result behavior are unchanged. Real Scene validation remains pending until a full-content run reaches its result assertions.

Actual change: the declared `BattleAtlasResources.cs` now checks `-ntsdAtlasAllocationTrace` once and logs plan/array decision and allocation progress only when that flag is present. No atlas branch, allocation argument, upload, cleanup or binding was changed. The same script was copied to the isolated validation clone after a diff confirmed the clone differed only by the new trace lines. Isolated Editor compiled and reached the real formal-content atlas builder. The single flagged Scene test crashed at ordered page 88 of 145; no XML or battle verdict. External process sampling observed peak private bytes 18,015,555,584; array use was rejected by the pre-existing 512 MiB budget. Exact logs, sample file, hashes and limits are in `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/ATLAS-ALLOCATION-TRACE-RESULT.md`. Status remains `RUNTIME_PENDING`; a resource-lifetime/environment remedy is a separate change.

Focused validation after the diagnostic edit: the first atlas test invocation used `-quit` and exited without XML, so no pass was credited. The corrected isolated EditMode invocation of `BattleCommonAtlasBindingEditorTests` produced `ATLAS-ALLOCATION-FOCUSED-ACTUAL.xml` with 2/2 PASS, process exit 0, and no opt-in trace lines. `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <current>` passed 675 records; `git diff --check` passed. This verifies compilation and the small default-path fixture only. Full formal-content Scene still crashes at the measured atlas allocation boundary.
