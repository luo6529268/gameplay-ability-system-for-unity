<!-- CHANGE-RECORD
id: NTSD28-Q08-TRANSIENT-X-SELFCHECK-ORACLE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal root NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable battle_world.cpp settle_ordinary_stage_bounds
evidence: docs/ai/TASKS/NTSD28-Q08-TRANSIENT-X-SELFCHECK-ORACLE-001.md
-->

# NTSD28-Q08-TRANSIENT-X-SELFCHECK-ORACLE-001

Before script edit: original Editor fresh full SelfCheck FAILs on the slot>=20 type-0 X bound assertion. The current test expects width800+100=900 at input X901, but the formal playable rule and existing Unity shared production formula use width+10=810. This is a test-oracle difference; no production first difference has been shown. The preexisting `BattleRuntimeSelfCheck.cs` dirty hunk concerns an unrelated overlay test and must be preserved. Exact path, expected side effect, acceptance and rollback are in the Task.

Planned edit: change only this bound assertion and its message. Do not change formal source, production, DAT, scene, old resources or other self-check cases. Run the original Editor menu once after compilation, preserve the fresh result, then classify any next failure without claiming aggregate PASS prematurely.

2026-09-29 actual edit and verification: changed only the selected slot>=20 assertion X900→X810 and its message width+100→width+10, preserving the pre-existing overlay hunk. Original Editor refreshed and rebuilt `Assembly-CSharp.dll` after source modification. Fresh full SelfCheck wrote `PASS` at 03:29:39, SHA-256 `2F9ACB02FAA121BB2A3621951F57B4C690655337EDEE2E5AC350BE2B3BE88EA8`; the MCP menu call itself timed out at 30 seconds while execution continued, so its transport status was not treated as test status. Editor returned idle/non-Play; Battle/Menu/GameConfig/ProjectMode hashes stayed at protected baselines. Detailed result and limits: [acceptance](../../../artifacts/diagnostics/NTSD28-Q08-NATIVE-KNOCKOUT-EVENT-STATE-001/SELFCHECK-ORACLE-ACCEPTANCE-20260929.md). Production KO behavior, same-state formal trace, integrated Play and Q08 aggregate remain pending; this test-only correction is `VERIFIED`.

Governance: `Tools/Validate-ChangeLedger.ps1` passed (994 records / 25 governed diff code files), and `git -c core.safecrlf=false diff --check` exited 0. No scene Play was needed for the single assertion correction.
