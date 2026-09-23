# NTSD28-Q07-MENU-FIRST-WINDOWS-PLAYER-001

Status: VERIFIED_SCOPED_PLAYER_CALLBACK. Parent: BATCH-04/Q07 Player cold-start acceptance. Original Unity repository only.

Authority: user-confirmed Menu index 0 and Battle index 1 in `ProjectSettings/EditorBuildSettings.asset`; D-023 formal LoganRuntime bytes; current formal EXE identity and Q07 additive Scene callback contract. This is a Player diagnostic, not a new battle-rule implementation.

Pre-change: the existing `NTSD28Q07WindowsBuildProbeEditor` builds Battle alone, so its prior PASS does not prove a normal Menu-first Player. The original Editor Menu callback probe did enter and leave Battle, but EditMode Play does not prove installed Player cold-start. The old content postbuild manifest gate has been updated separately under `NTSD28-Q07-WINDOWS-PLAYER-MANIFEST-V2-001` and compiled; its Player postprocess is still unverified.

Exact script scope: extend only `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsBuildProbeEditor.cs` with an opt-in Menu-first request branch, preserving existing Battle-only request behavior; add `Assets/NTSD/Scripts/Test/NTSD28Q07MenuFirstWindowsPlayerProbe.cs` as a Development Player-only command-line probe. Task/Change/Ledger/STATE/handoff/alignment and a diagnostic artifact may be updated. No production gameplay, nonbattle Menu callback implementation, Scene, Prefab, ProjectSettings, DAT, image, or formal J: file may be edited.

Acceptance: original Editor compiles both scripts, a new Windows Mono Development Player build uses exactly enabled Menu/Battle order and passes the 1371-file postprocessor, and the built Player cold-starts in Menu and drives actual Menu component callbacks through formal prewarm, Naruto OID2 selection, Battle Additive with a running World and three matching content keys, then ordered unload back to Menu with zero pool borrowers. Probe may invoke callbacks directly; it does not claim physical pointer/keyboard or rendered-pixel parity. Record exit code, probe JSON, build result, output source/destination content comparison, narrow ledger validation, and Scene hash invariance. Keep Q07 open for remaining real input/presentation acceptance.

Rollback: reverse only the opt-in diagnostic branch after reviewing then-current diff and observing repository approval requirements; preserve existing Battle-only behavior and all user work. The new probe/artifact can be retired only through the same governed process.
