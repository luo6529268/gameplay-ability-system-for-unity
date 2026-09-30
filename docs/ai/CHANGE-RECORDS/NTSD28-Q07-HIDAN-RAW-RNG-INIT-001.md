<!-- CHANGE-RECORD
id: NTSD28-Q07-HIDAN-RAW-RNG-INIT-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: formal root NTSD2.8-Logan EXE Hidan X580 LFR and paired GameSession28 explicit-BGM branch
evidence: docs/ai/TASKS/NTSD28-Q07-HIDAN-RAW-RNG-INIT-001.md
-->

# NTSD28-Q07-HIDAN-RAW-RNG-INIT-001

Before: the Hidan natural raw fixture initializes synchronized RNG with an extra random-BGM draw although the formal root fixture explicitly selects BGM2. Existing Hidan selected-field agreement cannot prove equal RNG initial state. The already corrected Lee diagnostic branch demonstrates the intended explicit-BGM setup, but its schema guard excludes Hidan.

The Task Contract declares exact path/symbol, expected side effect, preservation boundaries, acceptance, risk and rollback. PLANNED registration preceded the script edit.

Actual diff: `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs::ConfigureWorldAndRoster` adds only `Q07HidanNaturalCatchScenarioSchema` to the existing `ResetFromSeed` explicit-BGM branch. Before, this test schema consumed one random-BGM synchronized draw through `ResetForDirectBattle`; after, its diagnostic RNG starts from the seed with no BGM draw, matching the selected formal fixture precondition. No production caller, other schema, DAT or Scene was changed. Generated build and original Editor import/run pending.

Validation: generated Editor project `dotnet build --no-restore` exit0 / 0 errors / 201 warnings (`generated-editor-build.log`); original Editor PID11944 Unity MCP refresh and Tundra build success; one EditMode X580 request `PASS`; initial sync RNG equal, 440/440 selected battle fields, 200/200 sync RNG fields and 40/40 input phases equal; protected two Scene/GameConfig/project-mode SHA stable. `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exit0 (`ledger-validator.log`, historical non-current-diff warnings); `git -c core.safecrlf=false diff --check` exit0. Full evidence: `artifacts/diagnostics/NTSD28-Q07-HIDAN-RAW-RNG-INIT-001/ACCEPTANCE.md`. The root playback's CRT scalar uses default seed0 while Unity fixture uses scenario seed; no full CRT parity claim. The distinct physical Play action60 remains unexplained by same-state evidence.

Scope/rollback: `VERIFIED` applies only to this test fixture initialization and selected diagnostic comparison. Remove the one added schema condition to restore the historical diagnostic behavior if required; production code and assets have no change to roll back.
