<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-IDLE-TICK14-GAMEVIEW-001
status: COMPILE_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09P08SameStateBattlePlayProbeEditor.cs
authority: Official root 336B44 tick14 D3D11 capture and current Q09 one-sample exit
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-IDLE-TICK14-GAMEVIEW-001.md
-->

# NTSD28-336B44-Q09-IDLE-TICK14-GAMEVIEW-001

Before: the existing original-Scene P-08 probe fixes X500/540, Z650, target HP30, early Jump input and a single-camera tick22 blood-mark screenshot; it cannot compare with the formal root tick14 no-input OID2/OID9 capture. The active Editor and Scene may contain other work, so only a request-gated, clean-Scene entry is allowed.

Planned change: extend that existing Editor-only probe with an independent, expiring idle-tick14 request and separate result directory. Set the specified actor tuple in the Play clone, step exactly 14 no-input production ticks, capture the composite Game View with stable publication/logic tick, then exit Play and verify original scene bytes remain intact. Preserve every existing P-08 request/result behavior. No production, DAT, PNG, Scene, Prefab, ProjectSettings, nonbattle UI, or framework edit.

Risks: inappropriate Editor scene switch, partial current user edits, an asynchronous screenshot from another tick, formal/Unity RNG or map-state mismatch, and accidental old P-08 regression. Gate scene cleanliness and request expiry, use unique non-overwriting output paths, record initial tuple and each tick, separate logic comparability from image verdict, and abort without a visual alignment claim on mismatch. Rollback by exact forward edit of this branch only, preserving concurrent work.

Acceptance: generated Editor compile0; one original-Editor opt-in run only if the Scene preconditions hold; report all 14 production ticks, screenshot dimensions and stable tick, current authority/content identity, Scene/file hashes and lifecycle cleanup. A successful capture is not by itself a Q09 or total alignment certificate; any first difference gets a separate evidence-backed production task. Actual edits and command results will be appended here.

2026-10-05 code written: only `NTSD28Q09P08SameStateBattlePlayProbeEditor.cs` changed. A separate expiring request selects the idle branch, including clean Menu→Battle→Menu handoff or direct clean Battle entry; the old P-08 request prefix, X500/540 Z650 HP30 setup, first-two-tick Jump, 22-tick mark and original result directory remain the default branch. The new branch sets OID2/OID9 to X500/620 Z400 HP500/MP200, restores seed0 and the formal direct-battle BGM synchronized callsite, runs 14 no-input complete Driver ticks, checks publication, captures composite Game View, records title-visible fields and screenshot tick, then exits with Scene hashes. No production or resource file changed.

`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` passed with 0 errors/301 warnings. Scoped `git diff --check` passed with Git's LF/CRLF notice. The original Editor import, actual one-shot request, 14-tick runtime report, Game View and clean return remain unverified; do not promote beyond `COMPILE_PASS`.

Follow-up source check: the formal playable `BattleConfig28` defaults to seed0/mode0/HP500/MP200 and facing P1 right/P2 left; its random BGM selection consumes callsite `0x004021E0` before gameplay. The idle branch reproduces that one synchronized call without changing the old P-08 fixed-BGM branch. A guard now ignores the expiring idle request while the Editor is already entering or running Play; the generated Editor build reran with 0 errors/301 warnings and scoped diff check still passed. `unity status` reported no Pipeline-connected instance, while `unity pipeline list` identified the real original Editor PID19040 as running with `hasPipelinePackage:false`; no package installation, second Editor, direct socket route or runtime test was attempted.
