<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-C032-NATURAL-VOICE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07F03NaturalMarkerBattlePlayProbeEditor.cs
authority: 336B44 natural landing channel6 source and original Battle Scene event comparison
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-C032-NATURAL-VOICE-001.md
-->

# NTSD28-336B44-Q10-C032-NATURAL-VOICE-001

PLANNED before script edits. Current formal source and original Scene already match the natural six-event audio sequence, including builtin channel6 at relative ticks60/66; actual pooled battle voice was not inspected. Add a second exact request variant to the existing natural 128-tick Scene probe, preserve its F03 behavior and unique original result, and record only production sound-player observations at those two events. Expected side effects are a new diagnostic JSON and temporary Play, with no production/Scene/resource change. The exact path, acceptance, limitations, preservation and forward-correction rollback are in the Task. A playing Unity voice is scoped evidence, not device or formal speaker equivalence.

Actual test-only edit: one existing request-driven `NTSD28Q07F03NaturalMarkerBattlePlayProbeEditor.cs` accepts one additional exact C032 voice run ID and routes that variant to a separate Q10 result directory. Its original F03 run ID, report root, 128-tick roster, initial state, input, Driver, entity/RNG capture and shutdown remain in place. Only the new variant checks the sealed production battle player and records at the two natural pending `data/016.wav` events the actual pooled play count, resolved clip channels/frequency/samples, matching assigned AudioSource and `isPlaying`; PASS additionally requires tick60/X373 and tick66/X360. No production, DAT, WAV, Scene, audio routing or non-battle script changed. Generated compile and original Editor Play are pending; preserve any failure JSON for first-difference classification.

First generated Editor build failed with two CS0266 diagnostics: the existing sound player's `PooledOneShotPlayCountForDiagnostics` is `long`, while the new probe carrier and local counter were declared `int`. This is a test-only type mismatch, not a production audio first difference. Before the correction, change only the new two carrier fields and local value to `long`, then rebuild; do not cast away counter range or alter sound-player code.

The three new probe declarations now use `long` for the sound-player play-count values; the player and production audio route are unchanged. Generated compilation and original Editor runtime remain pending after this correction.

Final scoped evidence: generated Editor build exit0, 0 errors/253 warnings; the original Editor imported and ran unique `tay36-a243-x550-c032-voice-01.json` to `PASS/DONE`, 128 complete Driver ticks. At relative ticks60/66 the formal `data/016.wav` pending event X373/360 increased production pooled play count 3→4 and 4→5, and a matching assigned mono 22,100 Hz/8,158-sample AudioSource was playing at each event. All 128 captured entity/RNG/pending-sound `samples` objects exactly equal the pre-existing F03 same-state Scene report; no fixture drift. The report saw Battle Scene clean at completion; a later live query found clean Menu, then the original saved clean Battle Scene was reopened without saving. Final Editor idle/non-Play/Battle clean, protected Battle/Menu/mode asset SHA unchanged. The unique request file remains with `requested:false`. Details and limits are in `artifacts/diagnostics/NTSD28-336B44-Q10-C032-NATURAL-VOICE-001/REPORT.md`. Only the declared test-only script changed; formal root speaker PCM, device output, pan/volume and full Q10/Q12 remain pending. Governance check results will be appended after running them.

Final governance checks: `Tools/Validate-ChangeLedger.ps1` exit0/PASSED with 1163 records and 9 governed code files in the current dirty worktree diff; `git diff --check` exit0 with only Git line-ending notices. Exact outputs are retained in this package's diagnostic directory. The full test suite was not run because this is a bounded natural audio Play probe.
