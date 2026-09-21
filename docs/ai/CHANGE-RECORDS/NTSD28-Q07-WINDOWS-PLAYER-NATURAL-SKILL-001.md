<!-- CHANGE-RECORD
id: NTSD28-Q07-WINDOWS-PLAYER-NATURAL-SKILL-001
status: VERIFIED
change-kind: FORMAL_CONTENT_PLAYER_NATURAL_SKILL_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/NTSD28Q07WindowsNaturalSkillProbe.cs
authority: D-023 formal Logan Naruto DAT and ncl.png; playable source OID33 spawn and existing Q07 Editor natural-skill witnesses
evidence: build-3 0 errors; physical Naruto Player exit0/PASS; formal fingerprint, frame285, OID33 pic999-to-pic1 ncl.png, Stopped and pool0; see ACCEPTANCE
-->

# NTSD28-Q07-WINDOWS-PLAYER-NATURAL-SKILL-001

Before: formal DAT/image root and all 330 object definitions publish in a built Windows Player, but existing Player probe does not inject a natural skill or assert OID33/pic/sprite output. Editor natural-input OID33 and ncl.png representative evidence cannot by itself prove built Player runtime behavior.

Planned code: add a command-line-only Development Player probe. It reads the saved Battle Scene's first Naruto, verifies the formal root/fingerprint, queues physical L/D/J with bounded hold/release pulses across logic ticks, observes canonical combo/target/spawn/sprite, then uses existing ordered shutdown and writes a terminal JSON report. It must never run in release builds or Editor by default.

Invariants: no production code, Scene, GameConfig, Build Settings, loader, input binding, old resource, nonbattle or GAS edit. Formal source and protected Scene hashes stay unchanged. No extra service, queue or production lifecycle owner. Runtime failure is recorded rather than hidden by an asserted PASS.

Validation: Unity compile; one unique Windows Mono Development Player build through existing Q07 build probe, one built Player physical-key run with report/process exit, exact Scene hashes and Change Ledger. Do not rerun full character suite or Q06. The result can close only Player representative skill portability; EXE same-condition tick, all skills, full visible presentation and Q07 aggregate remain open.

Rollback: remove only this diagnostic script/meta under the repository's delete-approval rule after recording why; never reset unrelated work.

Actual code: only the new Development Player test probe and generated meta. It injects queued physical device states, checks canonical combo/target and formal clone sprite, writes JSON, and calls the existing ordered shutdown owner. Build-1 found a C# definite-assignment error in the probe; declaring the report before the conditional fixed it. Build-2 found reversed expected left/right combo values in the probe; matching the existing Editor probe fixed it. Neither failure changed production logic.

Validation: Editor refresh compiled with no Console errors; unique build-3 Windows Mono Development build succeeded with 0 errors; built Player process exited 0 and JSON `PASS`, L at tick4, direction at tick6, Naruto frame285 at tick8, OID33 hidden pic999 tick13 and formal `ncl.png` pic1 tick14. Formal fingerprint matched, ordered shutdown reached `Stopped`, active pool borrowers 0. Both Scene SHA-256 values stayed at the pre-task values. Earlier failed build/run evidence is retained. Whole-skill/EXE-tick/visual-sort/other-platform validation remains Q07/Q09/Q12.
