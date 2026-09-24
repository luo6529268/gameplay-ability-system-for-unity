# NTSD28-USER-TELEPORT-OFFSET-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Parent D-024 all-entity motion ratio; Q07 paused.

Authority: user-approved fixed full-background camera screen-fraction exception D-024 with formal DAT unchanged. Paired playable `BattleWorld28::resolve_native_teleport_state` places state400/401 entities at the selected target's absolute X plus a 120/60 pixel relative offset. Unity `LF2Entity.RunNativeTeleportState` and the legacy early-specials fallback both use the raw offset. Formal staged character DAT contains state400 frames. The absolute target position must be copied, not multiplied.

Declared scripts: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` only the two teleport X outlet writes and `Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportProductionEditorTests.cs` actual World/native caller. First show configured-view RED and default control with both facing directions and 400/401 where possible, then apply X-only relative factor and verify the original Editor. Preserve target selection, Y, Z+1, velocity, no-target branch and DAT. Stage-space candidate ranking and Z+1 layer semantics remain independent audit items.

Rollback: inverse only this Change ID's reviewed lines; preserve all prior work and current dirty state.

Evidence: Original Editor RED job `3d092d1ad1404e7c95c2f7cd27abc8dc` (four default pass, four configured-view state400/401 fail); native/legacy GREEN job `6d3bce2079014495bec7ff035cdc0ca2` 8/8; whole focused class `e7ff4770007442ffa52e5ea4fca28f88` 13/13. Full Play and target-ranking ratio audit remain open.
