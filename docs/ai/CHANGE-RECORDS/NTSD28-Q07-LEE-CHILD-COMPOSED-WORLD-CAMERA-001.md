<!-- CHANGE-RECORD
id: NTSD28-Q07-LEE-CHILD-COMPOSED-WORLD-CAMERA-001
status: VERIFIED
change-kind: Q07_LEE_NATURAL_CHILD_COMPOSED_WORLD_CAMERA_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07LeeJlBattlePlayProbeEditor.cs
authority: root formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable Lee J-L OID204 path; user-retained Unity full-background camera exception
evidence: artifacts/diagnostics/NTSD28-Q07-LEE-CHILD-COMPOSED-WORLD-CAMERA-001/COMPOSED-CAMERA-ANALYSIS.md; original Editor composed world-camera Play PASS
-->

# NTSD28-Q07-LEE-CHILD-COMPOSED-WORLD-CAMERA-001

Before edit: original Editor saved Battle Scene Lee J→L run naturally produced five OID204/pic28 commands and 574 changed nonblack pixels in an isolated camera ROI. That test disabled all scene layers and used a temporary black clear. It does not establish visibility over the retained project background. The existing probe's other modes and all user worktree modifications are protected.

Planned behavior: add a new request flag and artifact root to the existing Editor-only probe; allow exactly one camera mode per run; in composed mode preserve the saved camera's culling/clear/HDR/MSAA settings, set only a temporary render target, capture before/after birth, reuse the same child-command ROI and restoration checks. Risk: `Camera.Render` may not include all URP/Game view layers, non-child pixels may change in the ROI, and capture timing may differ from normal presentation. These limitations must remain explicit. No production/Scene/DAT/image or nonbattle edit. Acceptance and rollback are in the Task. Actual writes, compile/run and failures will be appended before delivery.

Actual edit: only `NTSD28Q07LeeJlBattlePlayProbeEditor.cs` acquired `captureComposedCamera`, a distinct result root, mutual-exclusion guard and report flag. The existing J→L/default/isolated paths remain; composed capture retains scene culling/clear/HDR/MSAA and changes only the temporary targetTexture. Original Editor `refresh_unity` completed and `read_console` returned zero errors. Unique Play `lee-child-composed-01` passed: five natural children/commands, composed ROI600 changed pixels, of which 574 overlap the isolated-change mask; visible background outside ROI. Camera restoration, ordered shutdown/borrowers0, Editor exit Play, saved Scene SHAs and no Scene/Build Settings Git diff passed. Independent PNG analysis and artifact hashes are in `COMPOSED-CAMERA-ANALYSIS.md`. This verifies only the world-camera pass, not full Game view, physical keyboard or formal EXE GPU parity. Validator result will be appended after the final documentation update.

Final governance check: `Tools/Validate-ChangeLedger.ps1` exited 0 with 849 Records and all five current governed code diffs covered, including this diagnostic. `git diff --check` exited 0. The three previously v3-recovered progress documents remain NUL-free. No Scene or Build Settings Git diff appeared.
