<!-- CHANGE-RECORD
id: NTSD28-Q07-LEE-CHILD-CAMERA-PIXEL-001
status: VERIFIED
change-kind: Q07_LEE_NATURAL_CHILD_SAVED_BATTLE_CAMERA_PIXEL_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07LeeJlBattlePlayProbeEditor.cs
authority: root formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable OID204 render source
evidence: artifacts/diagnostics/NTSD28-Q07-LEE-CHILD-CAMERA-PIXEL-001/PIXEL-ANALYSIS.md; original Editor isolated camera capture PASS
-->

# NTSD28-Q07-LEE-CHILD-CAMERA-PIXEL-001

Before edit: `NTSD28-Q07-LEE-JL-BATTLE-PLAY-001` is a scoped original-Editor Play PASS for ordinary Lee J→L, five source-initialized OID204 and five CentralOnly Entity commands at tick12. The saved Battle Scene Menu/Battle SHAs are `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` and `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; Editor PID11944 is idle in EditMode. The existing probe has no pixel readback and cannot prove visibility. Other Q09 worktree changes are protected.

Declared change: add only an optional pixel branch to the same Editor probe. Retain existing J/L schedule, 45 complete Driver ticks, birth/source/command checks and ordered cleanup. Read the saved world camera into temporary RenderTextures with camera state restoration, persist unique before/after PNGs, derive first child command ROI and report/catalog metadata and ROI pixel counts. Risks: URP capture render timing, camera layer/target restoration, five overlapping child sprites, another command changing in the same ROI, and black-key transparency. The Task defines exact acceptance and rollback. No production code, scene or content is changed; a camera pixel difference alone is not formal EXE visual parity.

First compile: original Editor reported one CS0165 in the new diagnostic because the short-circuited `manager != null && TryGetSpriteEntry(... out entry)` left the local entry not definitely assigned. No Play request was issued. The new local is now explicitly initialized to null before the guard; this is a diagnostic-only compile fix. Recompile and check Console before any request; preserve this failure in the record.

After fix/run: original Editor recompiled with Console error count0. Opt-in unique `lee-child-pixel-01` saved-Battle Play returned PASS. The formal source/release tick6 child pic28 matched published Unity `VisualDataId204/EffectivePic28`; staged/formal `cha.png` sheet SHA identical. Baseline tick11/Lee frame146 and post-birth tick12/Lee frame164 world-camera PNGs used temporary black clear/cullingMask0 with URP central submission. ROI58×58 projected from first child command; baseline had 0 nonblack pixels, post had 574 changed/nonblack. Independent Pillow decode found 288/574 exact formal pic28 RGB matches and 471/574 within distance5. The camera state and active RenderTexture were restored; ordered shutdown/borrowers0, Editor EditMode/Console errors0, Menu/Battle Scene SHAs stable. No production/DAT/image/Scene/ProjectSettings/nonbattle modifications. Exact data, hashes and limits: `artifacts/diagnostics/NTSD28-Q07-LEE-CHILD-CAMERA-PIXEL-001/PIXEL-ANALYSIS.md`. The visual result is scoped to isolated CentralOnly camera readback, not normal composition or root EXE GPU parity. Rollback is the opt-in diagnostic branch only after evidence review.

Final governance check: `Tools/Validate-ChangeLedger.ps1` exited 0 with 848 Records and all 5 current governed code diffs covered, including this diagnostic file. `git diff --check` exited 0. Current Menu/Battle Scene SHA-256 values still equal the pre-run values above, and neither Scene nor `ProjectSettings/EditorBuildSettings.asset` has a Git diff. The three previously approved v3-recovered progress documents remain NUL-free; their later addenda must be preserved rather than replaced with older candidates.
