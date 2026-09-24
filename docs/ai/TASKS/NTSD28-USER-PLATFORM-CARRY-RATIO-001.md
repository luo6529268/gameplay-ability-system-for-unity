# NTSD28-USER-PLATFORM-CARRY-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Parent D-024 all-entity motion ratio.

Authority: user D-024 fixed full-background view screen-fraction exception, with unchanged DAT and camera. Formal playable `battle_world.cpp` applies linked-platform frame DV as raw passenger displacement; Unity `LF2Entity.ApplyLinkedPlatformMotion` does the same. After D-024 the platform entity's own common X/Z physics travel is scaled, but the passenger's linked carry is still raw, so their relative position can drift under the configured view.

Declared scripts: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` at the linked-platform X/Z final position writes and `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FrameMotionTailEditorTests.cs` for one real caller configured/default test matrix. These files may contain other authorized dirty changes; preserve them exactly. No Scene, DAT, Prefab, Y-height, platform candidate-selection, nonbattle or camera changes.

Test-first acceptance: build an actual World with registered platform slot20 and rider slot21, matching rider `PlatformSourceSlotF4`, ground collision reference and synthetic platform frame DV, then run `rider.ApplyNativeFrameMotionForWorldPass`. Demonstrate configured-view RED for right/left X and Z while default-factor cases stay source-equal. Multiply only the existing decoded linked-platform X/Z displacement at its final passenger position write by the World factors; keep raw frame DV, Y and `CollisionYReference`, branch order, velocity and rounding. Original Editor compile, exact GREEN, narrow default adjacent frame-tail regression, Scene/DAT and Ledger checks. Full Driver/Play, Y/floor/platform geometry and formal EXE visual proof remain open.

Rollback: reviewed inverse patch on the two declared paths only; do not restore/reset/clean other work.

Evidence: original Editor RED `cdc0c47e3b4b4db59c94c48381c41399` (configured-view X 206.1455/193.8545 expected vs 204/196 raw), then production X/Z exit change, GREEN `eee3d49ce931424d8f9427fb48ce28ad` 4/4, adjacent source linked-motion `425047edee8544faa792393a2dcaee60` 21/21. Y/floor semantics and real Play remain open.
