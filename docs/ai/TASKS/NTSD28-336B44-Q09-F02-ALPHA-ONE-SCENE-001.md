# NTSD28-336B44-Q09-F02-ALPHA-ONE-SCENE-001

Status: `VERIFIED` (scoped alpha-one Unity-to-current-playable anchor witness). Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q09 / F02`.

Authority and gap: the 336B44 formal root executable and matching playable rendering path define F02 behavior. The existing formal WARP tick39 image uses the current, uninterpolated snapshot, whereas the saved Unity Battle Scene command used an inferred intermediate interpolation phase. A like-phase Unity Game View and command witness is missing; this Task does not redefine battle rules.

Only `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07F02Kind10BattlePlayProbeEditor.cs` may change. Add an opt-in request flag that waits for the already paused tick39 central frame to be built at display alpha 1 before taking its screenshot, and record the alpha used by that built frame. Keep all existing requests and tick/input logic unchanged. Do not modify production scripts, scene, DAT, images, audio, camera, menu, or nonbattle behavior.

Preflight: original project Editor must be non-Play, non-compiling, and have one clean saved Menu or Battle Scene. Preserve current dirty work. Use a unique request/run ID and never overwrite prior evidence.

Acceptance: generated Editor compile and original Editor import have zero C# errors; one bounded original Battle Scene Play produces tick39 alpha=1 command and a valid Game View PNG, with the existing source/root/Unity 45-tick selected fields and event sequence unchanged, ordered shutdown zero residuals, and four protected file hashes stable. Compare the same tick's relative entity anchors with the saved formal raw WARP geometry; keep differing background/viewport and root-EXE GPU limits explicit. A timed-out alpha, changed tick, compile error, or Scene dirt is a failed or pending result, not parity.

Rollback: a later scoped edit may remove only this opt-in diagnostic after diff review. Preserve all captured evidence and existing work; do not use destructive Git operations.

2026-10-04: the declared Editor diagnostic now waits for a paused central frame built at alpha 1 before requesting the tick39 PNG, and records the built alpha and frame generation. Generated `Assembly-CSharp-Editor.csproj` compiled with 0 errors/297 warnings; original Editor import and Play are still pending.

2026-10-04 exit: original Editor Play `f02-alpha-one-20261004-01` completed with built alpha1 and 1920×1080 tick39 PNG, 46/46 previous tick samples and 47/47 events strictly equal, ordered shutdown all zero, and four protected SHA stable within this run. Formal raw-WARP relative anchor residuals are −0.357/−0.887 output pixels. Root EXE GPU, physical keyboard and broader Q09 remain open. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q09-F02-ALPHA-ONE-SCENE-001/REPORT.md).
