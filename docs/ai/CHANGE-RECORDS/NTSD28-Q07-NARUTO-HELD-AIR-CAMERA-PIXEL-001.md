<!-- CHANGE-RECORD
id: NTSD28-Q07-NARUTO-HELD-AIR-CAMERA-PIXEL-001
status: VERIFIED
change-kind: EDITOR_BATTLE_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NarutoHeldAirActionProbeEditor.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable rendering plus formal Naruto pic97
evidence: original Editor compile and focused formal-content Play PASS; target pic97 ROI/camera PNG/source color match plus cleanup and Scene SHA stable
-->

# NTSD28-Q07-NARUTO-HELD-AIR-CAMERA-PIXEL-001

Before edit: previous full-Driver original-Editor Play reached Naruto held attack action30/pic97 and published one exact OID2/pic97 CentralOnly command with valid catalog binding. It did not read camera pixels. A separate controlled OID30/frame31 camera probe does not cover this naturally selected Naruto action.

Declared change: opt-in camera readback in the existing Editor-only probe, after current command validation and before fixture cleanup. Save/restore world-camera target texture, culling mask, clear flags, background, HDR/MSAA and RenderTexture active state; write one diagnostic PNG and ROI counts for the actual reached pic97. Preserve old request behavior and all current fixture/cleanup invariants. No production logic/resource/Scene/nonbattle changes; no new manager, worker or persistent state. Rollback only the new code in the existing diagnostic after review, preserving unrelated dirty work.

Planned validation: original Editor compile and focused Play, image/ROI inspection with source-cell color comparison, post-exit World/slot/pool/roster and saved Scene checks, Change Ledger validator and `git diff --check`. Do not claim formal EXE visual parity from Unity camera evidence alone. Actual results and residual risks will be appended.

Actual edit: added only `Request.captureCamera` and report fields plus a camera readback helper in the declared existing Editor probe. The flag implies the already-verified presentation tick; after the exact pic97 command, the helper projects its ROI, reads a 960-wide world-camera RenderTexture, writes a diagnostic PNG and counts nonclear ROI pixels. Camera target/culling/clear/background/HDR/MSAA and RenderTexture active state are restored in `finally`. Default action-only and publication-only requests retain their behavior. Compile/Play pending.

First focused Play `naruto-held-air-camera-20260926-01` PASS and wrote a camera PNG with 56×57 projected ROI/486 nonclear pixels; same original Editor returned non-Play, temporary bindings/counts and saved Scene hashes restored. Visual inspection shows an airborne Naruto at the left of the black-background image, but the report omitted ROI origin, so that screenshot alone does not independently tie the left cluster to the target command. A same-scope diagnostic-only follow-up adds ROI x/y fields before the final pixel attribution run; the first result and PNG are preserved.

Final validation: original Editor rebuilt Editor assembly after the x/y source edit, pre-Play Console zero C# errors. Follow-up `naruto-held-air-camera-20260926-02` PASS: same tick20 action30/pic97 and exact central command, camera ROI `(66,276,56,57)` bottom-origin with 486 nonblack pixels, camera state restored. Independently read the 960×540 PNG and formal source pic97 crop: 474/486 ROI nonblack pixels have an exact source-cell RGB value; all 19 distinct nonblack source-cell colors appear in the ROI. The PNG was visually inspected. World/slot/pool/roster restored, request false, same Editor non-Play, saved Battle/Menu Scene hashes unchanged. Two MCP disposed-client errors on exit do not indicate gameplay compile failure. Exact JSON/PNG hashes and method are in `artifacts/diagnostics/NTSD28-Q07-NARUTO-HELD-AIR-CAMERA-PIXEL-001/ACCEPTANCE-20260926.md`. No focused NUnit/full SelfCheck because this package only extends an opt-in visual probe. Formal EXE same-world pixels, natural collision pickup and physical keyboard remain unverified.

Governance exit: `Tools/Validate-ChangeLedger.ps1` exited 0 with 839 Records and 32 governed code files in the current diff; `git diff --check` exited 0. All three live progress documents remain NUL-free. No old asset was deleted and no background or mode DAT was consumed by this test.
