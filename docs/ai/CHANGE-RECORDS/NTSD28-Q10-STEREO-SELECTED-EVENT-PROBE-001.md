<!-- CHANGE-RECORD
id: NTSD28-Q10-STEREO-SELECTED-EVENT-PROBE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10SelectedRunCuePlayProbeEditor.cs
authority: formal NTSD2.8-Logan battle audio stereo matrix and user fixed full-background camera exception
evidence: docs/ai/TASKS/NTSD28-Q10-STEREO-SELECTED-EVENT-PROBE-001.md
-->

# NTSD28-Q10-STEREO-SELECTED-EVENT-PROBE-001

Initial `PLANNED` record before any script edit. The Task declares the exact test-only path, current Unity limitations, event/voice measurements, output isolation, acceptance and rollback. This does not change production battle audio. Target evidence is a fresh original-Editor real Play JSON at `artifacts/diagnostics/NTSD28-Q10-STEREO-SELECTED-EVENT-PROBE-001/play-result.json`, separate from the previously verified cue-play result. Formal camera/event parity and speaker output remain pending even if this probe passes.

2026-09-27 `CODE_WRITTEN`: only the declared existing Editor-only probe script changed. The original menu entry and result path remain selectable; the new menu entry writes to this Change ID's independent JSON. The same forwarding recorder now captures each cue's `PendingSoundEvent.WorldX`, actor `SourceRuleXInt`/physical `XInt` and source-initialized flag before forwarding, then records the real pooled voice's clip channels, spatialBlend, panStereo and volume. No production sound/DAT/WAV/Scene/camera/nonbattle behavior changed. Original Editor compile/Play, Scene hashes and validator are pending.

2026-09-27 `VERIFIED` for this **test-only selected Unity Play scope**: original Editor compiled the changed script, then saved Menu→formal-content Battle physical D produced `003.wav` tick8 event X664/source X649 and `004.wav` tick13 event X787/source X729; both mono real voices were spatialBlend0/panStereo0/volume1 and playing. Raw new JSON SHA `15E137B9BCCE0800B8A9E8D6BCFB8FBC85C123E906688EDF61C169985A5B70C2`; old selected-run JSON SHA remained `09A7E8D27F42C027314C339C7AB5CD84BF65A81345AEFA400F95F1EAD37ED3BF`. Probe unloaded Battle, MCP stopped Play, Editor idle Menu; two Scene SHA unchanged. Exact values and limits in the new ACCEPTANCE. Formal same-state camera, output-channel mix and O-02/Q10 remain open.
