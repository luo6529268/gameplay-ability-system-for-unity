# Issues

- 2026-07-22: The open Unity Editor consumed request-file self-checks but kept using `Library/ScriptAssemblies/Assembly-CSharp.dll` from 21:48:48 after `BattleRuntimeSelfCheck.cs` was updated at 21:55:58. Fresh request results therefore reproduce the old P4 failure text and cannot prove which new diagnostic predicate fails. `dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal` passed with 0 errors (42 existing warnings), but it does not replace Unity runtime acceptance. Do not make a production correction until the Editor recompiles and reports the expanded P4 message.
