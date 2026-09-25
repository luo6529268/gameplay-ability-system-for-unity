# Q07 direct Battle empty-root change: isolated Unity C# compile

Status: `ISOLATED_COMPILE_PASS / ORIGINAL_EDITOR_IMPORT_PENDING / FOCUSED_TEST_PENDING`. This is supporting evidence for `NTSD28-Q07-EMPTY-ROOT-BATTLE-ENTRY-001`, not a Q07 phase exit.

On 2026-09-25 the original Unity Editor PID 11944 remained responsive in the `NTSD_Battle` scene, but its `Editor.log` had no asset reimport or script compile after the 00:43 Editor test-runner edit. The queued `Temp/NTSD28-Q07-EmptyRootBattleEntry-20260925-004534.request` had no `.started` or `.result`. A second Unity Editor was not launched and no computer-use was used.

To check the *current source* without writing to the active Editor's `Library`, the existing Unity-generated `Library/Bee/artifacts/1900b0aEDbg.dag/Assembly-CSharp.rsp` and `Assembly-CSharp-Editor.rsp` were copied to a task-specific `Temp` directory. Only `-out` and `-refout` were redirected to `Temp/NTSD28-Q07-StandaloneCsc-20260925`; the Editor response file's reference to `Assembly-CSharp.ref.dll` was redirected to the newly compiled temporary runtime reference assembly. The source, defines, analyzers and remaining references were not changed. The archived response files here preserve those exact inputs. The runtime response includes `Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs` at line 871; the Editor response includes `Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCacheCallerEditorTests.cs` at line 679.

Commands, run sequentially from the repository root:

```powershell
& 'C:/Program Files/dotnet/dotnet.exe' 'D:/Unity/HubEditor/2022.3.62f3/Editor/Data/DotNetSdkRoslyn/csc.dll' '@Temp/NTSD28-Q07-StandaloneCsc-20260925/Assembly-CSharp.rsp'
& 'C:/Program Files/dotnet/dotnet.exe' 'D:/Unity/HubEditor/2022.3.62f3/Editor/Data/DotNetSdkRoslyn/csc.dll' '@Temp/NTSD28-Q07-StandaloneCsc-20260925/Assembly-CSharp-Editor.rsp'
```

| Assembly | Process exit | C# errors | C# warnings | Temporary output |
| --- | ---: | ---: | ---: | --- |
| `Assembly-CSharp` | 0 | 0 | 24 | `Assembly-CSharp.dll` and `.ref.dll` |
| `Assembly-CSharp-Editor` | 0 | 0 | 6 | `Assembly-CSharp-Editor.dll` and `.ref.dll` |

The exact compiler output is in `runtime-compile.log` and `editor-compile.log`; the warnings are not zero and are retained, not described as a warning-free build. The two changed source SHA-256 values were `01BE4E17A96DDE5543B07608E4FD38362BB9BEE927A7C0ED89C9EA5CB58BC853` (`BattleTestBootstrap.cs`) and `4BA182BB3227B9A3D72B5E4830C308B54FF3F9139F0711FF6B602C0250E0A213` (Editor test source). Scoped `git diff --check` exited 0. Menu and Battle Scene disk SHA-256 stayed `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` and `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`.

Limit: this compiles against the cached Unity-generated reference graph; it does not load the new assemblies into the original Editor, run NUnit or `BattleRuntimeSelfCheck`, prove Play behavior, inspect the current Console, or fix the unrelated Change Ledger validation failure. Keep the Change Record at `CODE_WRITTEN` until the original Editor imports the change and the focused request actually returns a result. No DAT, resource, Scene, Prefab, ProjectSettings or production battle rule was modified for this diagnostic.
