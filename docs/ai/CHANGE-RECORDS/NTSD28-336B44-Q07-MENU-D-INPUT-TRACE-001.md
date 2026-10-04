<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-MENU-D-INPUT-TRACE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09NameplateNaturalPlayProbeEditor.cs
authority: 336B44 formal battle input chain and current original Editor Menu-to-Battle observation; user saved-idle confirmation
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-MENU-D-INPUT-TRACE-001.md
-->

# Q07 Menu-to-Battle physical D first-difference trace

Created before script edit. Unity prior state: the existing Q09 natural nameplate probe injects held D through Input System and checks P1 X, but its first failed run after 174 observed ticks lacked enough input-chain fields to distinguish synthetic-input loss from production behavior. The Q01 visible-only path passed its glyph output while X remained 620. No native or Unity production mismatch is yet established.

Planned change: add only a bounded diagnostic trace to the existing Editor probe's report. For the first twelve observed ticks, read keyboard key state, enabled MoveAction value, callback-backed CurrentMoveInput, PlayerSlot0 canonical FrameInputSet, native key/CD, frame/state, source/projected X and Vx. Do not change probe input event timing, strict Q09 gate, production or resources. Expected side effect is a larger unique JSON report. Accept when generated/original Editor compile, one safe Play run writes the trace, exits cleanly with four stable protected hashes, and a first observed handoff can be stated without overclaiming skipped ticks. Rollback is a future audited removal of these diagnostic fields only. Unverified: original Editor compile, Play, and actual source of stationary X; status `IN_PROGRESS`.

2026-10-03 scoped exit: existing Q09 Editor probe now saves at most twelve input rows; its queue timing and strict viewport gate were untouched. `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q` exited0, zero errors, 296 warnings; original Editor MCP refresh compiled a newer `Assembly-CSharp-Editor.dll`. Unique Q01 wrapper run `q01-words-20261003-03` passed its visible glyph capture, exited Play to the sole clean Menu and preserved all four hashes. At observed ticks4–7, keyboard D pressed and MoveAction enabled, but action X/callback X/canonical buttons/native KeyRight/source X movement were all zero. First **observed** break is keyboard state→MoveAction; cause remains unproven. No production, DAT, image, Scene or nonbattle edit. Status `VERIFIED` for first-difference observation only; Q07 and goal open. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-MENU-D-INPUT-TRACE-001/REPORT.md).
