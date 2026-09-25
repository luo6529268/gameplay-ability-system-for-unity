<!-- CHANGE-RECORD
id: NTSD28-Q07-NARUTO-HELD-AIR-PUBLICATION-001
status: VERIFIED
change-kind: EDITOR_BATTLE_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NarutoHeldAirActionProbeEditor.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable render snapshot plus formal OID2 frame30 pic97
evidence: original Editor compile and formal-content focused Play PASS at tick20 with catalog pic97 and one current CentralOnly Entity command; cleanup and Scene hashes stable
-->

# NTSD28-Q07-NARUTO-HELD-AIR-PUBLICATION-001

Before edit: the prior original-Editor fixture reached held Naruto action30/state15/pic97, but every probe tick set `buildPresentation:false`. Formal sprite resolver selects top-origin `(560,720,79,79)`; Unity's read-only formula selects the same bottom-origin `(560,801,79,79)` from the identical PNG. No current evidence binds that reached tick to a frozen central entity command.

Declared change: add an opt-in publication witness to the existing Editor-only probe's request and report. On the attack tick that first reaches action30, request production presentation; verify the catalog entry and published CentralOnly command identity and source binding. Preserve the original default action-only behavior, input packets, temporary roster/entities and cleanup. No new runtime manager, external dependency or content/Scene edit. Expected transient side effect is one presentation build in the original Battle Scene. Rollback only this scoped script edit after protected-worktree review; preserve unrelated dirty work.

Acceptance/validation planned: compile in the original Editor, one focused opt-in Play and post-exit count/Scene audit, `Tools/Validate-ChangeLedger.ps1`, `git diff --check`. Do not claim camera pixel or formal EXE parity from a command-only witness. Actual files/results/risks will be appended after implementation.

Actual edit: extended only the declared existing Editor probe. `Request.capturePresentation` defaults false; the attack tick opts into `StepOneTick(... buildPresentation:true)` when requested. At the reached action30/pic97 it records the production sprite catalog source rect and central binding, prepares the current CentralOnly frame, acquires its submission, and counts the exact fixture Entity command. Existing fixture setup and cleanup are unchanged. Compile/Play pending; no production or content edit.

Validation: original Editor refreshed/recompiled `Assembly-CSharp-Editor.dll` after the source edit, with zero current C# Console errors before Play. The opt-in `naruto-held-air-publication-20260926-01` JSON is PASS: formal root, pickup101/-1, action30/state15/pic97 on tick20, catalog rect `(560,801,79,79)` and valid central binding, current CentralOnly submission with one matching fixture stable ID102/OID2/pic97 Entity command size79×79 among 10 commands. World/slot/pool4/2/2 unchanged, fixture/roster restored, request false, same original Editor non-Play after transient MCP disconnect; both saved Scene hashes unchanged. Two MCP disposed-client Console errors occurred on exit; they are not C# compile errors. Detailed result and SHA in `artifacts/diagnostics/NTSD28-Q07-NARUTO-HELD-AIR-PUBLICATION-001/ACCEPTANCE-20260926.md`. Camera pixels, natural collision pickup, physical keys and formal EXE same-world visual comparison remain unverified. No focused NUnit or full SelfCheck was run for this diagnostic-only edit.

Governance exit: `Tools/Validate-ChangeLedger.ps1` exited 0 with 838 Records/32 governed code files in the current diff; `git diff --check` exited 0. The three live progress documents remain NUL-free, and the original Editor assembly timestamp is later than the edited source. Existing unrelated worktree changes were preserved.
