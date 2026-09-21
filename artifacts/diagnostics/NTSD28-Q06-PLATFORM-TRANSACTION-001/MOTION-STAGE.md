# Platform linked motion stage

FOCUSED_TEST_PASS / LINKED_MOTION_REPRESENTATIVES, not VERIFIED.

LF2Entity.ApplyNativeFrameMotionForWorldPass now invokes a private linked transaction after existing own velocity handling. Nonzero positive slot/current source frame, target integerY==reference and type3 state gates follow current battle_world.cpp. Current native float64 source frame velocity is used; legacy frames use existing int projection. X/Z/Y displacements each use target integer origin, >500 override bias550, facing only ordinary X, round-even integer plus precise double. Y updates collision reference; shadow remains candidate-time offset. Slot0 and missing source cause no linked coordinate write.

Results:
- Initial motion job23f2ead1:21 total,7PASS/14FAIL (13 linked position mismatches plus removed-source cleanup failure); full initial outputs archived.
- After implementation jobeaef8c44:20PASS/1FAIL. All moved-state projections agree; removed-source test failed teardown. Archived motion-first-eaef8c44.
- Corrected only removed-source fixture from direct unregister/Destroy to existing StructuralWriter.Free + flush, matching native direct despawn and preserving pool ownership. Focused retry90ba7a6a executed ONLY case19:1PASS including source absence, unchanged invalid-link motion and shutdown.
- Therefore evidence is20 priorPASS+1 focusedPASS, not a fabricated single21PASS run. No unchanged cases repeated.
- Candidate42, adjacent11 and carrier4/trace87 retained as unchanged upstream evidence. No fullSelfCheck/Play.

Adjacent lifecycle finding must not be lost: LF2Entity.DestroyEntityLikeExeCoreForStructuralWriter resolves pool after UnregisterFromWorld, which clears registeredWorld; Free captures pool beforehand. The isolated World Destroy fixture's shutdown failure is evidence of a possible wrong-owner/no-owner return. Create separate exact Task/Change and reproduction before finalQ06 closure. Do not call this resolved merely because platform fixture now uses correct direct-Free semantics.

Remaining priority:
1. Revisit the Destroy owner issue in a bounded lifecycle package, preserving already-closed Q05 shutdown order.
2. Add actual NativePreviousY104 physics producer at correct final integer sync (not generic SetPosition/SyncIntegerPosition); source20 afterPhysics remains untested.
3. Own-frame native float velocity and delay134/dxdy/dz positional tail are currently not consumed by this entry (it still invokes existing integer kernel). This matters when the platform itself advances. Requires authority witness then complete transaction before claiming fulltick fidelity.
4. Linked-motion overrides>500/zero components/invalid descriptor diagnostic success boolean not fully covered by existing21 cases; no broad claim.
5. Add mixed ordinary/platform source case, integrated fulltick/replay and shadow snapshot consumer/representativePlay. Raw47/3 remains unpromoted; schema17/25/28 unchanged.

Scope remains battle runtime only; no Scene/resources/nonbattle edits. Q06 incomplete/Q07 not migrated.
