# NTSD28-USER-SOURCE-GRAB-RELATION-POSITION-001

Status 2026-09-24: `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE / PLAY_PENDING`. Original-project Editor final catch-relation class 23/23 PASS; source-domain facing/selection remains gated. Parent D-024; Q07 remains paused.

Authority: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, playable `BattleWorld28::resolve_kind3_catch_relation` in `source/ntsd28_core/src/simulation/battle_world.cpp:5198-5259`, and its comment confirming kind1 shares the paired relation branch. The DAT-local CPoint and frame-center values are raw pose offsets, not view-scale travel.

Unity pre-change: `BattleInteractionWriter.TryApplyGrab` handles kind1/kind3 and `AlignGrabPair` updates only physical X and integer mirrors. Initialized source-rule X history remains stale after both participants move. Existing paired witness proves physical pose gap stays raw while source-rule precise positions differ under D-024.

Declared script scope: `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs` and `Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchRelationExactFieldsProductionEditorTests.cs`. Reproduce the formal source-int anchor, half-gap blend and immediate integer mirror for both participants only when both source carriers are initialized. Preserve physical pose and DAT-local offsets. Source-rule facing/selection is a separate decision-reader gate and must not be implicitly changed in this writer package. No DAT/Scene/ProjectSettings/nonbattle edits.

Acceptance: divergent physical/source source-history witness at factor1 and configured view, source-incomplete no-write gate, existing kind1/kind3 relation regressions, original-project Editor compile and focused NUnit. Full Driver/Play/formal EXE comparison remains open.

Rollback: review and reverse only the two declared script hunks, retaining all pre-existing dirty work.

Result: shared `AlignGrabPair` now writes both initialized source-rule X histories from their own integer anchors with raw DAT-local center/CPoint offsets and native half-gap blend, then immediately truncates both source integer mirrors. It leaves physical placement untouched and declines source writes if either carrier is absent. Existing factor1/configured-view witness asserts source 148/140 against physical 160.5/152.5 at configured scale; additional kind1 and incomplete-carrier cases passed. Unity original Editor job `aabd274d553d46bda0c9b354e41642aa` ran 23/23 PASS after both new cases, replacing first 21/21 pass job `62841368f5164c83ab8c4ad198e983b9` as the final focused evidence. No full Driver/Play/EXE or source-facing decision-reader proof.
