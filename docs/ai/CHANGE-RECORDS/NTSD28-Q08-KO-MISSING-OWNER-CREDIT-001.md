<!-- CHANGE-RECORD
id: NTSD28-Q08-KO-MISSING-OWNER-CREDIT-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08KoMissingOwnerCreditEditorTests.cs
authority: formal Logan playable battle_world.cpp resolve_native_standard_hit_attribution at 1439-1475 and ordinary/selected-armor KO callers
evidence: docs/ai/TASKS/NTSD28-Q08-KO-MISSING-OWNER-CREDIT-001.md
-->

# NTSD28-Q08-KO-MISSING-OWNER-CREDIT-001

Created before script edits. The current Unity KO resolver delegates to the B5 resource-credit resolver, which returns null at a missing first or second owner. The current formal KO resolver instead keeps the last valid source/owner when an owner lookup fails. The first observed difference is static and isolated; natural battle frequency has not been measured.

Scope: test the exact missing-owner and valid-chain guards, then give KO its source-matched traversal without changing the deliberate B5 resource fail-closed contract. Expected side effect is a KO event/counter credited to the last live entity for an orphan owner, while missing redirected source still suppresses KO. Existing event record, result, mode and presentation owners are untouched. No DAT, image, Scene, project Asset, nonbattle or GAS framework edit.

Validation and rollback: follow the Task; preserve all pre-existing dirty work. This Record will append actual files, RED/GREEN commands and evidence after execution. No compile, focused test or Play result is claimed at this checkpoint.

2026-09-29 test-first RED: added only `NTSD28Q08KoMissingOwnerCreditEditorTests.cs` and its unique meta GUID. Original Editor PID11944 was idle and non-Play; MCP refresh compiled the test with Tundra success/0 errors, then the exact five-test job `d6fdcc3500814f6aa149f36bb8c42e8b` completed 5 cases. Three missing-owner KO expectations failed because actual credit was null; missing redirected source and valid two-hop controls passed. Raw job status is `artifacts/diagnostics/NTSD28-Q08-KO-MISSING-OWNER-CREDIT-001/original-editor-red.json`. This proves the Unity branch difference in the controlled resolver; it does not prove a naturally occurring orphan KO. Production code remains untouched at this checkpoint.

2026-09-29 implementation and scoped verification: changed only `BattleDamageWriter.ResolveNativeStandardHitCredit` to resolve the initial physical/redirected source and then take up to two valid owner hops, breaking and keeping the last valid entity if the next owner lookup fails. `ResolveNativeHitResourceAttacker` and all other resource paths remain unchanged. Original Editor PID11944 recompiled with Tundra 0 C# errors; job `2274993d9fcb481787777b42dacd379e` passed 12/12 KO resolver and B5 resource tests, including the deliberate resource fail-closed guard. Adjacent standard/reduced KO producer job `6a6572524ee746b38613dcd7a8c86365` passed 4/4. `Tools/Validate-ChangeLedger.ps1` and `git -c core.safecrlf=false diff --check` exited 0; four protected Scene/config hashes were unchanged. [Acceptance](../../../artifacts/diagnostics/NTSD28-Q08-KO-MISSING-OWNER-CREDIT-001/ACCEPTANCE.md). Remaining: no natural orphan-owner hit, root-EXE same-world trace, new Play or Q08 aggregate exit. Status `FOCUSED_TEST_PASS`, not whole-stage verified.
