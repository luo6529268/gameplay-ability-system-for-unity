# Q07 alternate half-dvx SelfCheck gate

Formal source `HitResponseResolver28::accumulate_ordinary_horizontal` uses `dvx / 2.0` for ordinary ground reduced hits. The previously accepted Unity production writer uses the same precision. The original Editor's preceding full `BattleRuntimeSelfCheck` failed at the historical integer-half `dvx=5` expectation in `CheckAlternateDamageCoreSideEffects`.

This test-only package changed that expectation and the same fixture expectation in `CheckAlternateDamageCharacterEntry` and `CheckAlternateDamageSharedDatEntry` from 2 to 2.5, including their messages. No production rule or DAT value changed. Original Editor assembly timestamp followed the test script edit; recent Editor log tail had zero C# compile errors.

After an original Editor refresh, a single full SelfCheck request was consumed. The fresh result was `FAIL` at the later `CheckAlternateDamageHeavyWeaponEntries`: `state1002 alternate tail must update frame and reflected velocity on a real weapon`. The prior odd-dvx assertion was crossed; the full SelfCheck remains failed. Exact fresh result is preserved as `original-editor-selfcheck-after-half-dvx.result` in this directory. The new heavy-weapon failure is an independent first obstruction and is not corrected or classified as a production defect in this test-only package.

Protected Menu/Battle Scenes, GameConfig and ProjectBattleModeConfig retain the recorded SHA-256 values. Parent Q07 and total goal remain open; no natural Play or formal EXE same-world claim follows from this result.
