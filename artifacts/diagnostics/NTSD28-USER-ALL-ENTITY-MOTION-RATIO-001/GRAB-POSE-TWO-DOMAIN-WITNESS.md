# D-024 kind-3 grab pose after scaled target motion

Status: `FOCUSED_TEST_PASS / DEPENDENCY_WITNESS / NO_PRODUCTION_FIX`, 2026-09-24. Original-project Unity Editor job `6fe9428e5d1f45ea9bd5e4049b3eb22e` ran only the two newly declared EditMode cases; summary 2 passed, 0 failed, 0 skipped. The Editor was on `Assets/NTSD/Scene/NTSD_Battle.unity` and idle before the test; `Assets/Refresh` completed and a domain reload preceded the run. No second Unity project, computer-use or full test suite was used.

| Case | Target X integer before grab | Attacker precise X after | Target precise X after | Target minus attacker |
| --- | ---: | ---: | ---: | ---: |
| Factor 1, raw Vx48 from target X140 | 188 | 148 | 140 | -8 |
| Fixed full view, Sx=2048/1333, same raw Vx48 | 213 | 160.5 | 152.5 | -8 |

The paired playable `BattleWorld28::apply_native_relation_hit` uses raw frame-local `centerx/cpoint.x` to form the target anchor, then blends half of the target-to-anchor integer gap into both precise X positions (`battle_world.cpp` around lines 5212-5254). The existing Unity `BattleInteractionWriter.AlignGrabPair` performed that transaction in both cases. The local -8 pose gap stayed unchanged; blindly multiplying these DAT-local anchors would alter the grab pose. Conversely, the source-rule post-grab precise positions are 148/140 for this raw history, whereas the configured battle positions are 160.5/152.5. The future reference carrier needs the attacker's and victim's separate raw coordinate histories at the relation write, followed by independent truncation. The current battle coordinates alone do not provide them.

The test uses synthetic but source-formula-consistent frame-local values from the existing `NTSD28B6CatchRelationExactFieldsProductionEditorTests` fixture, actual registered World entities, production `CharacterMechanics.StepBattleLogic`, and production `InteractionWriter.TryApplyGrab`; it is not a formal EXE visual/contact test. It proves the writer dependency, not that collision selection, real sprite pixels, CPoint settle, WPoint follower, OID219 or fusion are aligned. No production script, DAT, Scene, camera or nonbattle file was changed for this witness. Q07 remains behind the confirmed D-024 differences.
