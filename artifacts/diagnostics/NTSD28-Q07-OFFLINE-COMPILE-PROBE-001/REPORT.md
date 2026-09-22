# NTSD28-Q07-OFFLINE-COMPILE-PROBE-001

Status: `OFFLINE_CSHARP_COMPILE_PASS / UNITY_IMPORT_AND_RUNTIME_PENDING` (2026-09-22). The original Unity project and its live Editor were not restarted or duplicated. No project source, Scene, Prefab, asset, generated `.csproj`, or Unity package was edited by this probe.

The live original Editor (PID 33236) still has `Library/ScriptAssemblies/Assembly-CSharp.dll` dated 2026-09-22 04:23:43 UTC and `Assembly-CSharp-Editor.dll` dated 04:23:44 UTC. Both predate the Q07 mode input/activation and R15 code. Existing request-file pollers execute only those loaded assemblies and cannot prove new code compiled or ran.

The generated `Assembly-CSharp.csproj` predates the newly added `LoganModeComboInput.cs` and does not list it. A direct `dotnet msbuild Assembly-CSharp.csproj` therefore failed with CS0246 in `LoganObjectCatalog` (missing `LoganModeComboInput`); this is a stale generated project-file input, not a proved source-code defect. The generated Editor project likewise omits the new `NTSD28Q07ModeComboPublishedActivationEditorTests.cs`.

To inspect the current source without launching a second Unity Editor, `current-sources.targets` in this diagnostic `Temp` directory adds exactly those two missing files to their respective MSBuild `Compile` items. Evaluated project inputs were saved as `runtime-evaluated.json` and `editor-evaluated.json`; both files appear in the right project, and the Editor defines `UNITY_INCLUDE_TESTS`. The check then ran:

```powershell
dotnet msbuild Assembly-CSharp-Editor.csproj -t:Build -p:CustomAfterMicrosoftCommonTargets=<Temp diagnostic current-sources.targets> -p:OutputPath=<Temp diagnostic Build> -clp:ErrorsOnly -nologo
```

Exit code: **0**. `build.log` is empty under `ErrorsOnly`; the resulting offline `Assembly-CSharp.dll` and `Assembly-CSharp-Editor.dll` were written at 07:13:06/07:13:08 UTC under this diagnostic `Temp` directory. This build includes current tracked Q07/R15 source paths plus the two new files. It does not update `Library/ScriptAssemblies`, import Unity assets, execute NUnit, run Play Mode, or establish formal EXE parity. The Q07 and R15 Change Records therefore retain their Unity pending status.

Next required evidence is a fresh compile/reload **in the original Editor**, then the focused Q07 mode publication/seal/reset/restore test and same-seed Unity V2 capture against the current native diagnostic trace. Do not run a second Unity instance against the occupied project or count this offline build as that gate.
