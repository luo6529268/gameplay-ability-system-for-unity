<!-- CHANGE-RECORD
id: NTSD28-Q07-WINDOWS-PORTABLE-CONTENT-PACKAGING-001
status: VERIFIED
change-kind: BUILD_BATTLE_CONTENT
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsContentBuildProcessor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsBuildProbeEditor.cs
authority: D-023 formal Logan content and Q01 frozen 1343-file SHA manifest
evidence: Windows Mono BuildPipeline q07-package-build-4 PASS; 1343 files 46594829 bytes exact SHA, SPARK equal
-->

# NTSD28-Q07-WINDOWS-PORTABLE-CONTENT-PACKAGING-001

Status: `VERIFIED` for Windows Player build output only; actual Player startup and Q07 batch remain open.

Before: project-local staged content passed Editor candidate/publication and real App/menu Play, but ordinary Windows Player builds have no formal raw DAT/PNG packaging. The runtime resolver uses the EXE sibling for a relative root, and its spark prewarm separately reads a raw BMP inside `<exe>_Data`. The Q07 production GameConfig root remains empty. Existing R8 stress build manually copies legacy raw files only for that one build entry point.

Exact planned path/symbol: add one Editor-only `IPostprocessBuildWithReport` callback in `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsContentBuildProcessor.cs`. For successful Windows Standalone builds, validate the frozen CSV against the staged source, copy the 1,343 precise files into the EXE-sibling tree, validate destination hashes and absence of extra formal files, then place the existing SPARK source at its current raw Player path. No other script, serialized asset or runtime root edit in this package.

Expected side effects: approximately 46.6 MB additional Windows Player output and SPARK's existing battle visual dependency. No non-Windows change. Fail instead of overwriting conflicting output; do not delete existing files. Acceptance, risk and rollback are in the Task Contract. First run compile and focused manifest test, then one clean actual Windows build and exact output audit. Record actual command, report and any failure before status promotion. Player runtime content selection remains a distinct Q07 gate.

Actual script change: added only the declared `NTSD28Q07WindowsContentBuildProcessor.cs`. Its Windows post-build callback validates the exact 1,343-row manifest, source non-meta set, source lengths/SHA, copies missing destination files without overwrite, verifies destination lengths/SHA and no extra formal files, and copies the existing SPARK raw file to the Player data path with the same no-overwrite rule. No GameConfig, Scene, loader or nonbattle source was changed. Compile and actual build are pending.

Actual first build after the separate 28-error Player compile fix: R8 Windows Mono Development build reported PASS and produced an EXE, but Q07 formal output directory was absent and independent manifest audit found 1,343 missing files. SPARK existed only because the R8 entry copies it separately. No Q07 callback log appeared. Do not promote packaging. Hypotheses: callback visibility/registration or `BuildReport.summary.result` not yet Succeeded during postprocess. Next change makes the callback public, logs entry/platform/result and does not prematurely return on summary.result; Windows output/EXE checks remain. Re-run using the newly declared request-driven harness in a fresh output directory, preserving the completed R8 output.

Actual second script: added declared `NTSD28Q07WindowsBuildProbeEditor.cs` with a single-use Temp JSON request, unique output directory, Windows Mono Development `BuildPipeline.BuildPlayer`, BuildReport JSON and backend restoration. It does not run the Player, change GameConfig/Scene or copy content itself. The build processor now logs its callback entry, is public, and validates Windows output without relying on `BuildReport.summary.result` during postprocessing. Fresh Unity compile and build are pending.

First build-probe request `q07-package-build-1` was consumed (`requested=false`) but produced neither report nor output directory; Editor stayed idle and no probe log was present. Treat as a test-entry scheduling/early-exception failure, not a Player build result. The probe now logs queue and entry and captures `PlayerSettings` inside its exception-reporting scope. Recompile and use a distinct run ID; do not reuse or overwrite the R8 build.

Second request `q07-package-build-2` logged `Build probe queued` but never `Build probe entered`, confirming that the Editor's `delayCall` continuation was not delivered in this context; no output/report was created. The test-only poll now invokes its synchronous build directly while its `running` guard blocks re-entry. Recompile and use a third unique output ID; this is not a packaging callback result.

Third request `q07-package-build-3` entered BuildPipeline and wrote a terminal FAIL report with `errors=2`: Burst 1.8.21 AOT assembly-reference scan threw an `ArgumentOutOfRangeException` for a DateTime timestamp. The new EXE is partial and not a valid Player. This is before Q07 postprocessing, so it does not disprove the callback. The existing successful R8 build uses a narrow temporary Burst-AOT disable helper; the Q07 probe will reuse that exact private helper by reflection and restore the original settings bytes in `finally`, with a fourth unique output directory and pre/post settings hash check. No prior build output will be removed or overwritten.

Final fresh `q07-package-build-4` BuildPipeline report Succeeded/errors0/size130671528. The public callback logged entry with `BuildReport.summary.result=Unknown` and packaged 1343 exact files/46594829 bytes plus SPARK. Independent source/manifest/output SHA audit found 1343 present, 0 missing/extra/bad, and byte-identical SPARK. Burst settings SHA before/after remained `72601656F7F4B74E53D13C65A01CF8A26A450257D75A3F145A0D5D6EBF5D1296`; Scene SHA unchanged and GameConfig asset clean. Focused AI EditMode job57ed5ec6 11/11 PASS supports the separate compile guard. Detailed evidence in `WINDOWS-PLAYER-PACKAGING-ACCEPTANCE.md`. This closes only packaging; Player runtime and production root switch are separate Q07 gates. Four build-generated Odin asset/meta files remain untracked and preserved.
