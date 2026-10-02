<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-SAKURA-Z481-SOURCE-CONTROL-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q10Diagnostics/sakura_stereo_natural_reach_probe.cpp
authority: 336B44 playable Sakura natural stereo source and original Unity map-clamped source Z481
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-SAKURA-Z481-SOURCE-CONTROL-001.md
-->

# NTSD28-336B44-Q10-SAKURA-Z481-SOURCE-CONTROL-001

PLANNED before script edit. The existing diagnostic hardcodes both combatants at source Z650, while the protected Unity map clamps them to Z481 in the original Scene run. Extend only the diagnostic CLI with an optional common initial Z; default650 preserves prior output. Expected effect: a formal-source bounded positive/negative at the actual Unity map coordinate, with deterministic CSV/LFR and no production change. Risk is incorrectly promoting source-only evidence to Unity or root EXE parity; Task acceptance separates layers. Rollback is a forward correction of this additive argument under a new record, preserving prior outputs. No DAT, Scene, map, image or Unity production file is in scope.

2026-10-02 actual script: `run_case` takes `source_z`; the only new CLI argument is an optional integer 0..1000, defaulting to 650. Both fighters use it. No other script, DAT, Scene, map, image or Unity production behavior changed under this ID. Current formal playable closure compile exit0; the first PowerShell wrapper subsequently failed when trying to read a nonexistent empty compiler-output file, which does not invalidate the executable compile. The earlier wrong-VFS source attempt and wrong-resource-root EXE attempt remain in unique failure files.

Validation: Z481 source runs b/c each exit0 and grid/CSV/LFR bytes match; omitted-Z650 output grid/CSV/LFR matches the prior formal Z650 source run. Both source Z values yield action240 tick6, 172 tick23, 340 and `c/saku/w/tra.wav` tick24. Root official EXE SHA remains `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`; with both runtime roots specified, new Z481 LFR replay exit0/report PASS, 55 declared ticks, action/MP/camera 165/165 source/root equal. Root trace has no audio-event column and report sets `nativeParityClaim=false`. This is a source/root coordinate-control certificate only; original Unity Scene corrected physical-key Play, actual voice and stereo output remain pending. Full files and checks are in [REPORT](../../../artifacts/diagnostics/NTSD28-336B44-Q10-SAKURA-Z481-SOURCE-CONTROL-001/REPORT.md).

Final audit: `Tools/Validate-ChangeLedger.ps1` exit0/PASSED and new probe path COVERED under this ID; `git diff --check` exit0. Four protected Battle/Menu Scene, mode Asset and WAV hashes match the pre-run baseline. No file deletion, move, DAT numerical edit, Unity production edit or Scene modification in this package.
