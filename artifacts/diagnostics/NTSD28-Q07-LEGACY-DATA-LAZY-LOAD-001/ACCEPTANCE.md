# Q07 implicit legacy data load retirement — 2026-09-22

Status: `VERIFIED_LAZY_LOADING_ONLY`. This is a battle-content initialization change, not Q07 aggregate exit or Q08 stage/result acceptance.

`GameDataManager` no longer overrides `InitializeSingleton` to read old `Assets/NTSD/Config/data.txt`. The formal candidate publication still supplies object definitions; empty-root `CharacterAnimtorManager.ParseCharacterFrameConfigs` still calls `LoadDataFile(fullDataPath)` explicitly. Direct Battle bootstrap may configure `GameConfig` in `Start`, so an `Awake`-time root predicate would not reliably prevent the old read. Existing old files are retained.

Final-source Editor compile updated `Assembly-CSharp-Editor.dll` with no `error CS` or `Compilation failed` in the recent Editor log. Focused EditMode job `030ecccb9a00407fb3bd481cc5612159` passed 1/1: singleton creation left the data manager unloaded; explicit legacy load then parsed 137 objects. After removing the redundant base-only override, final-source job `8eb24dc8a39647aa8d55f0115f6f4140` also passed 1/1.

Formal staged publication job `ccab44720a734051bc4ca2dbbc58b29f` passed 1/1 after 129 seconds, including 330 object definitions, 906 effective images, three-owner publication and resource recycling. Its provisional `stuck_suspected` flag did not represent its terminal status, which was `succeeded`.

Actual serialized-root menu Play `q07-lazy-menu-1.json` passed: production asset root `Assets/NTSD/Content/LoganRuntime`, formal source key/fingerprint, menu prewarm, coherent three-owner publication, four World objects, 29,400 tracked resources, zero old-owner/new-owner survivors and pool borrowers, ordered close to `RuntimeMapCleared`, and two frames Stopped. This Play ran before the final removal of the now empty `InitializeSingleton` override; that removal changes no initialization action. Final code received a fresh compile and focused lazy-load test, but Play was not repeated after the no-op removal. The separate Menu Scene callback still cannot load Battle while Build Settings has an empty scene table.

`Tools/Validate-ChangeLedger.ps1` passed with 652 records/15 governed code files after the Record listed both edited scripts. `git diff --check` passed for scoped files. Protected Battle and Menu Scene SHA-256 remained `BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6` and `6CF124A17F692325CED5774C3C10C3324AA047439BC6C3EB08856188A78AB4B1`; active Battle Scene was clean/root14 after Play.

Q08 remains open: formal `data/data.txt` has 24 background rows, whereas the retired old file has zero; this package does not decide the native result-stage count, publish formal background definitions, or claim result parity. Old DAT/images/data.txt have no deletion authorization and remain present. Formal natural skills, display, audio and whole-session acceptance retain their Q07–Q12 owners.
