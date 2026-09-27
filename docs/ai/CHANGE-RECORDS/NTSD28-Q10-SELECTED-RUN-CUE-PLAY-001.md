<!-- CHANGE-RECORD
id: NTSD28-Q10-SELECTED-RUN-CUE-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10SelectedRunCuePlayProbeEditor.cs
authority: formal NTSD2.8-Logan root EXE and paired playable per-tick audio submission; formal Naruto running frame sound declarations
evidence: docs/ai/TASKS/NTSD28-Q10-SELECTED-RUN-CUE-PLAY-001.md
-->

# NTSD28-Q10-SELECTED-RUN-CUE-PLAY-001

Initial `PLANNED` record before any script edit. Exact Task above declares source identity, existing Unity state, single test-only code path, expected temporary sink-forwarding side effect, cleanup, validation and rollback. The implementation must not change battle runtime, DAT/WAV/image/Scene or nonbattle code. Capture original Editor compile, real Menu→Battle Play result, cue and pooled voice evidence, and Scene hashes; distinguish logical sink timing from actual wave parity. Preserve any failed attempt in the diagnostic artifact rather than relaxing entry guards to force a pass.

2026-09-27 `CODE_WRITTEN`: added only the declared Editor-only probe script. It loads saved Menu→Additive Battle with formal `LoganRuntime`, takes physical D tap-release-hold through the existing Input System, records each matching Naruto running cue at the battle sound sink, forwards the original event batch unchanged to the actual `NTSDSoundPlayer`, and checks event/callback tick, World event presence, prepared clip assignment and pooled voice count. Cleanup releases keyboard, removes temporary sink, restores stress suppression, and unloads only the loaded Battle. No production/Scene/DAT/WAV/nonbattle edit under this ID. Original Editor compile and Play verification remain pending.

2026-09-27 `VERIFIED` for this test-only Play scope: original Editor compiled the new script after refresh (`Assembly-CSharp-Editor.dll` newer than script) with no named Console error. Saved Menu→selected formal content Battle Play result `PASS`: physical Naruto running sound 003 event/callback tick8 and 004 event/callback tick13, World events present, main-thread callbacks, player pooled voice count 0→1→2, prepared clips assigned and playing. Raw result SHA-256 `09A7E8D27F42C027314C339C7AB5CD84BF65A81345AEFA400F95F1EAD37ED3BF`; exact conditions, WAV SHA difference and limits in `artifacts/diagnostics/NTSD28-Q10-SELECTED-RUN-CUE-PLAY-001/ACCEPTANCE.md`. Probe unloaded Battle and MCP stopped Play; same Editor idle Menu; saved Menu/Battle Scene hashes unchanged and neither Scene has Git diff. `git diff --check` and `Tools/Validate-ChangeLedger.ps1` both exited 0. This status closes only this probe's accepted behavior, not O-01/O-02/O-03/Q10, formal audible parity or BGM/WMA.

2026-09-27 read-only audio-content correction: Python standard `wave` decoded the actual selected 003/004 Unity and formal WAV files; each pair has identical mono/16-bit/22050 Hz parameters, frame count and complete PCM SHA, despite the 10-byte container size and whole-file SHA difference. Details in `artifacts/diagnostics/NTSD28-Q10-SELECTED-RUN-CUE-PLAY-001/PCM-AUDIT.md`. No sound resource changed. This narrows O-02 for these two cues to playback scheduling/gain/pitch/spatialization/mixing rather than sample-content mismatch; other WAVs and BGM/WMA remain open.
