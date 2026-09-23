<!-- CHANGE-RECORD
id: NTSD28-USER-CAMERA-VIEWPORT-SCALE-001
status: ROLLED_BACK
change-kind: USER_APPROVED_BATTLE_CAMERA_VIEWPORT_SCALE
code-path: Assets/NTSD/Scripts/App/BattleBackgroundPlatformPresentation.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleBackgroundPlatformPresentationEditorTests.cs
authority: formal NTSD 2.8-Logan playable RenderCamera28 1333x730 and user 2026-09-23 proportional-camera request
evidence: artifacts/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/REPORT.md
-->

# NTSD28-USER-CAMERA-VIEWPORT-SCALE-001

Status: `ROLLED_BACK`. The user's next message clarified that the camera size must remain unchanged and the requested proportional change concerns character run distance. The prior interpretation of camera authorization was wrong.

History: a battle-camera viewport cap and focused test expectations were written, compiled in the original Editor and the revised camera test class passed 21/21. The existing real-Scene probe was mistakenly run outside Play and returned `FAIL / Play Mode is not active`; Play was then entered, but before a valid real-Scene measurement the user clarified the request. Play was exited and the two script files were restored with an inverse patch; `git diff --numstat` for both is empty. Battle Scene SHA remains `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`. The rolled-back test is not evidence for the final camera or movement behavior. No camera change remains.

Post-rollback verification: the original Editor refreshed and recompiled the restored source; the original `BattleBackgroundPlatformPresentationEditorTests` class passed 21/21 (job `5d0a417431814aa7b1bc9f9cb0735c91`). This confirms the camera path is back to its prior tested behavior, not that the user's requested longer on-screen run is implemented.
