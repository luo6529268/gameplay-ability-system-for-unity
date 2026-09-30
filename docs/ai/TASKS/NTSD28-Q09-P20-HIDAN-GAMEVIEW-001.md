# NTSD28-Q09-P20-HIDAN-GAMEVIEW-001

Status: `VERIFIED_SCOPED_ORIGINAL_GAMEVIEW_VISIBLE`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q09 / P-20`; independent while BATCH-04/Q07 D-024 collision policy is pending. [Acceptance](../../../artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-GAMEVIEW-001/ACCEPTANCE.md).

Authority: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` and matching playable natural Hidan frame430 render path. Existing formal paired WARP actor A/B and original Battle World Camera actor visibility are scoped evidence, not a complete Game View witness.

Objective: use the original project's existing physical J1–2/K9–10/L+D+J13–14 Play probe to capture one actual, composed Game View at the naturally reached frame430. Record screen dimensions, saved PNG identity, actor action/pic, published command tick, World tick/checksum before and after the screenshot, and confirm visible actor pixels without mistaking background/HUD or an unrelated frame for the actor. Keep the user-approved fixed camera and existing Unity HUD. No computer-use or second Unity project.

Allowed code: only `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HidanPhysicalBattlePlayProbeEditor.cs`. Add a new opt-in request flag and unique result directory. Preserve the existing Q07, frame430-command, and isolated-World-Camera branches. `ScreenCapture.CaptureScreenshot` may be asynchronous: pause the complete Driver on frame430, wait for a valid PNG before ordered exit, and fail safely if the expected frame or image is unavailable. Do not alter the production World or serialized Camera/Scene. No DAT/image values, formal source/EXE, Prefab, ProjectSettings, or nonbattle changes.

Acceptance: script compile0; original Editor Edit/idle before request; one original Battle Scene physical-input Play at natural frame430; actual full Game View PNG with dimensions and actor-visible pixel evidence; stable tick/checksum while awaiting screenshot; ordered shutdown/zero borrowers and clean unchanged Scene; Change Ledger validator and diff check. Report that this does not prove formal root GUI GPU equivalence, exact same-view pixel parity, or Q09 completion. Reuse previously passed formal logic/sheet/isolated-camera evidence; do not rerun unrelated cases.

Rollback: review exact new opt-in hunks and evidence outputs; preserve existing dirty work. Do not delete or reset without applicable approval.
