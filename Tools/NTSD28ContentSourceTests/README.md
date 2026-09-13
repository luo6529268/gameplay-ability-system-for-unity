# Content source path focused tests

Change: NTSD28-B11-CONTENT-SOURCE-PATH-CONTRACT-001. This runner compiles the actual production BattleContentSource and the same pure NUnit fixture used in Unity. It does not emulate Unity or load its cached gameplay DLL.

Use the installed Unity-compatible NUnit and Mono runtime. The Unity NUnit DLL targets .NET Framework; .NET 10 execution is not compatible with its CallContext usage. No new dependency is installed.

```powershell
dotnet build Tools/NTSD28ContentSourceTests/ContentSourceTests.csproj -v:minimal
& 'D:/Unity/HubEditor/2022.3.62f3/Editor/Data/MonoBleedingEdge/bin/mono.exe' Tools/NTSD28ContentSourceTests/bin/Debug/net472/ContentSourceTests.exe
```

The narrow runner supports only the Test/TestCase attributes used in this fixture. It is not a general NUnit runner. For actual Unity validation, filter EditMode tests to `NTSD.Test.Editor.NTSD28B11ContentSourceEditorTests` in the existing Editor; do not open a second instance on the same project.

The initial 20 cases failed because the production class was absent; after implementation and review, all 24 cases passed in both the source-linked process and the actual Unity Test Runner. These validate path semantics only. File existence, catalog ordering/validation, content fingerprints, PNG pixels and production cache/publication are later gates.
