<!-- CHANGE-RECORD
id: NTSD28-Q07-N30-LEGACY-TAIL-RETIREMENT-001
status: VERIFIED
change-kind: Q07_N30_LEGACY_LATE_INPUT_PRODUCTION_RETIREMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsLateTailNoOpEditorTests.cs
authority: formal root NTSD2.8-Logan.exe and paired playable simulation_tick_driver.cpp, original Editor complete Driver tick8 first difference
evidence: NTSD28-Q07-N30-UNITY-DRIVER-WITNESS-001 ACCEPTANCE.md
-->

# NTSD28-Q07-N30-LEGACY-TAIL-RETIREMENT-001

Pre-change: Unity's ordinary low-slot late tail invokes legacy `RunLateCharacterDatInputTrigger`, which detects `9,0,9,0`, clears input history, creates OID998 immediately and broadcasts code100. The same action and input history in the exact formal root EXE fixture do not create OID998 and retain history. J-L-J-L is a negative control. The paired playable production source's OID998 references are tied to revival continuation, with no corresponding N30 input-birth route in the inspected full simulation caller chain. The exact per-tick observation and its limited claim are in the diagnostic ACCEPTANCE.

Declared changes and invariants: one production call removal in `LF2Entity.RunLateTailBeforePrevFrame`, preserving virtual dispatch, LF2Character cleanup, historical helper names and direct historical SelfCheck; one focused Editor test for matched-history low-slot ordinary late pass. No special physical-key mapping or DAT value patch. No wider InputModule, factory, resource, Scene or nonbattle edit. Old per-helper test may remain for migration history but does not define current release production authority. The Task Contract contains expected side effects, validation and rollback. Status `PLANNED` before either script edit.

Test-first RED: new `MatchedLegacyInputHistory_RemainsIntactAfterProductionLateTail` entered the original Editor after the in-place MCP refresh. Original Editor focused job `146d081a81fe48968b71a781b82eb307` ran one test and failed at history index1 (expected -1, actual 0), proving the old late call clears the matched history before production edit. One transient MCP connection refusal during domain reload preceded the successful idle-state query; no second Editor was started. Production edit may now proceed within the declared call site only.

Actual production edit: `RunLateTailBeforePrevFrame()` no longer invokes `RunLateCharacterDatInputTrigger()`; the latter stays as a private historical diagnostic helper, with a corrected summary. The virtual tail and Q06 state13 contract remain. The single new Editor test is the RED fixture above. No DAT value, image, Scene, InputModule, factory, revive path or nonbattle script was edited under this ID. Status `CODE_WRITTEN`, pending GREEN and release comparison.

Final scoped validation: generated Editor-project build exit0, 0 errors/215 warnings (`production-generated-editor-build.log`); original Editor refreshed in place, focused GREEN job `e8ffd626278842d0a182d53e96085289` 1/1 and final late-tail adjacent job `a021de96fdfa49dfb693b922d8a0bea1` 4/4. The existing misleading low-slot test name was corrected to describe actual virtual-tail execution; no assertion changed. Two post-fix original-Editor complete-Driver 20-tick captures returned PASS. Independent formal-root versus Unity comparison of actor action, five input-history scalars and OID998 count gives 140/140 each, 280/280 together, with no OID998 birth in the L-K-L-K trace. Exact result paths and limits are in `PRODUCTION-RETIREMENT-ACCEPTANCE.md`. Editor Console had no C# compile errors, only unrelated MCP client-exit error-category entries. Menu/Battle SHA and Git Scene diff stayed unchanged. Q07/D-024/old 521 resource gate remain open; neither full-state nor GPU parity is claimed. Status `VERIFIED` for this scoped production retirement only.

Final audit: `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Resolve-Path -LiteralPath '.').Path` exited0 with `Change ledger validation PASSED`; its numerous historical declared-path warnings are retained in `change-ledger-validation.log`. `git -c core.safecrlf=false diff --check` exited0. Neither the user-owned dirty UI/Input/CPoint hunks nor serialized Scene files were changed by this production ID.
