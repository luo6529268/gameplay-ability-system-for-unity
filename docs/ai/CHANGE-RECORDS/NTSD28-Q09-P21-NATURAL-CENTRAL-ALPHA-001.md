<!-- CHANGE-RECORD
id: NTSD28-Q09-P21-NATURAL-CENTRAL-ALPHA-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KarinState9997BattlePlayProbeEditor.cs
authority: formal NTSD2.8-Logan root EXE and paired playable renderer alpha contract
evidence: docs/ai/TASKS/NTSD28-Q09-P21-NATURAL-CENTRAL-ALPHA-001.md
-->

# NTSD28-Q09-P21-NATURAL-CENTRAL-ALPHA-001

Pre-edit: formal paired Karin OID314 WARP uses the official low-alpha `cha4.png` and Unity natural Legacy A/B consumes that low alpha. The current central shader has the matching static blend formula, but the natural central command has no attributed GPU pixel witness. The existing Karin Editor probe is already modified by other Q09 tasks; preserve all of its behavior and add only the opt-in branch named in the Task.

Planned responsibility: after natural complete-Driver OID314 command publication, render the same frozen command set with and without precisely the child body using production central resolver/materials; record target-owned pixels and frozen World invariants. Use fresh outputs and restore all temporary GPU and probe state. Do not alter production, content, Scene, camera, GameConfig, background, DAT values or nonbattle code. Expected side effects are only new opt-in request fields, scratch GPU allocations fully released, and unique diagnostic artifacts.

Acceptance and risk: see the same-ID Task for generated compile, original Editor Play, independent PNG alpha calculation, protected hashes, cleanup and ordered exit. Do not promote this Record on code or compilation alone. If the Editor is unavailable, retain `RUNTIME_PENDING`. Root EXE same-view and Q09 aggregate remain separate gates. Rollback is scoped to this opt-in diagnostic branch and requires preserving all pre-existing dirty work.

Actual code: only the declared Editor probe was extended. An opt-in CentralOnly request now takes its existing naturally produced OID314 body command, builds two temporary batches from the same frozen frame with production resolver/materials, removes exactly that one body in the second batch, writes fresh white-background GPU PNGs, counts changed pixels and confirms tick/checksum equality. The existing Legacy/config lifecycle path remains unchanged. `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q /p:UseSharedCompilation=false` exited 0 with 0 errors and 202 warnings. Existing Unity Editor PID 11944 accepted MCP `refresh_unity` and logged `Tundra build success (4.82 seconds)` with no C# errors; editor assembly timestamp advanced to 2026-09-28 12:52:59 UTC. The compile-only checkpoint was `RUNTIME_PENDING`; the later runtime closure is recorded below.

Closure: original clean Battle Scene CentralOnly Play produced natural OID314/state9997/pic60/owner8 at tick8. Frozen central commands and production-resolved quads both went 7→6 when only the child body was omitted; scratch GPU PNGs changed 1,245 pixels at threshold >0 and 609 at >2. Independent Pillow/NumPy comparison found maximum RGB differences [20,22,25], below the official first-cell alpha maximum 33. World tick/checksum were unchanged; owned fixture/child were released and object/slot/borrower counts remained 4/2/2. Existing Editor returned idle, non-Play; Battle/Menu/GameConfig/project-mode SHA values remained unchanged. `Tools/Validate-ChangeLedger.ps1` and `git -c core.safecrlf=false diff --check` both exited 0. The bounded acceptance and limits are in [ACCEPTANCE](../../../artifacts/diagnostics/NTSD28-Q09-P21-NATURAL-CENTRAL-ALPHA-001/ACCEPTANCE.md). This closes only the named diagnostic contribution gate, not exact alpha formula parity, root-EXE same-view pixels or P-21/Q09 aggregate.
