<!-- CHANGE-RECORD
id: NTSD28-Q07-HIDAN-NATURAL-RELATION-001
status: FOCUSED_TEST_PASS
change-kind: Q07_HIDAN_NATURAL_CATCH_RELATION_FULL_DRIVER_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HidanNaturalCatchRelationEditorTests.cs
authority: formal root NTSD2.8-Logan.exe natural Hidan OID24 action0 attack1-2 jump3-4 X580/X1200 trace
evidence: NTSD28-Q07-HIDAN-NATURAL-UNITY-DRIVER-001 880/880 raw common fields; relation slots absent in raw schema
-->

# NTSD28-Q07-HIDAN-NATURAL-RELATION-001

Before change: source and root EXE trace expose catchTarget/catchSource slots, while existing Unity 40-tick raw capture does not. Selected action, HP/PP and position equality cannot directly certify those relation fields.

Exact script path: new `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HidanNaturalCatchRelationEditorTests.cs` plus `.meta`; use existing formal scenario world and root trace, 40 ticks X580/X1200. Assert both runtime relation slots each completed tick, preserving first mismatch. No production or broad exporter changes. Acceptance and rollback in Task Contract. Status PLANNED before script.

Actual script: only the declared new Editor test and its meta were written. Two NUnit cases read the root formal trace, run the existing `WithLoganScenarioForReplayTests` formal-content Authority400 driver for 40 natural-input ticks, and compare `CaughtSlotIndex`/`CatchSourceSlot90` for each completed tick. Compile and focused test pending.

First original-Editor focused job `e1056ffed8d44c4e98a0d523988b0769` discovered exactly two cases and completed both, but the job result is FAILED: X1200 reports an unhandled Unity Error log from the MCP bridge, `Cannot access a disposed object: System.Net.Sockets.NetworkStream`, after a status poll was made while the test job was active. No relation-field assertion mismatch was reported in that failure summary. Preserve this result; rerun only the affected case without MCP polling during execution to separate transport noise from combat behavior.

Final scoped status `FOCUSED_TEST_PASS`: the original Editor compiled the new test after a full AssetDatabase refresh (the earlier `scope:scripts` request compiled existing assemblies but did not import a new file). Direct discovery found exactly two cases. The affected X1200 case was rerun in job `783fb81f87414f5ab395c5e6d94d57d1` and passed 1/1; X580 was independently rerun in job `3cf9438ef53c49cca3b6b43ff65f8fa4` and passed 1/1. Both jobs were allowed to finish before status reads; no relation assertion mismatch. Each test compared two relation slots for all 40 completed ticks to the formal root trace, so 80/80 field comparisons per case, 160/160 total. The first combined job's bridge log failure remains as a separate failed attempt, not reclassified. The full original Battle Scene physical-key Play, pixels and all Q07 exits remain pending. See same-ID ACCEPTANCE.md.

Final repository gates: `Tools/Validate-ChangeLedger.ps1` PASS, 877 records and 11 governed code paths; `git diff --check` exit0. Protected Menu/Battle Scene SHA-256 remains `785F828C...81E13`/`2EE465D8...8B77A`; GameConfig/ProjectBattleModeConfig remains `0527D737...C8EA7`/`88E10D43...F55C`. Existing concurrent InputModule/CharacterInputModule/BattleControlsView changes were not edited by this package. These gates do not certify Battle Play.
