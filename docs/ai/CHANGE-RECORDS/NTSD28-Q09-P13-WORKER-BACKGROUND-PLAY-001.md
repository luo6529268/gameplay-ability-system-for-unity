<!-- CHANGE-RECORD
id: NTSD28-Q09-P13-WORKER-BACKGROUND-PLAY-001
status: BLOCKED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09EarthquakeWorkerBattlePlayProbeEditor.cs
authority: formal NTSD2.8-Logan root EXE and paired playable frame150 151 background snapshot plus Unity project map exception
evidence: artifacts/diagnostics/NTSD28-Q09-P13-WORKER-BACKGROUND-PLAY-001/ACCEPTANCE.md
-->

# NTSD28-Q09-P13-WORKER-BACKGROUND-PLAY-001

`BLOCKED / KNOWN_INACTIVE_BRANCH`. The new opt-in Editor probe compiled with zero errors in the generated Editor project and the original Unity Editor, then ran once in the original saved Battle Scene. It stopped before Han setup because the production dedicated worker is ineligible: `unity-presentation-bindings-are-still-attached`. The original Editor completed ordered shutdown, returned zero borrowers, exited Play and retained the Battle Scene SHA-256. This agrees with the earlier B1 worker-pacing audit, so the failed precondition is not evidence of an earthquake producer or Map consumer defect. No worker tick or Map PNG was produced. The probe remains request-gated; no production, DAT, Scene, Prefab, ProjectSettings or nonbattle path changed.

Actual code: `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09EarthquakeWorkerBattlePlayProbeEditor.cs` and its Unity-generated `.meta`. The probe rejects unsafe run IDs, records the next tick before worker submit, prevents report/PNG overwrite, and requests ordered shutdown on completion or failure. Validation: `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` returned 0 errors; original Editor domain reload generated `Assembly-CSharp-Editor.dll` after the new source; run `q09-worker-scene-20260927-01` retained the exact ineligibility reason, `stopped=true`, `borrowersAfter=0`, matching before/after Scene SHA. Worker publication/pixels and formal-root same-frame pixels remain unverified. Do not enable/detach the worker in Q09; B1/B9 requires its separate cadence and presentation-binding contract before that path becomes live. Q09/P-13 continues through the currently live inline tick path and the Q07/D-024 natural-catch gate. Rollback remains the Task's reviewed exact-file deletion boundary.
