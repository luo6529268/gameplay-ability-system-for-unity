# Q07 held type1 weapon complete-tick dual-domain gate

Status: `IN_PROGRESS / NO_RUNTIME_RESULT`. Task and Change Record were registered before any authored script edit. Source and original-Editor traces are pending; Q07/BATCH-04 remain open.

This is the pre-run snapshot. The final scoped result supersedes the pending state in `ACCEPTANCE.md`; the original first-test failure and source compile correction remain below as historical evidence.

The paired playable source diagnostic compiled with g++ C++17 against 28 core and 2 playable translation units, exit 0, after correcting the probe-only depth key enum from `down` to `depth_down`. Its 24-tick `native-24-v2.csv` uses the formal root SHA from the Task and captures a natural OID120 pickup on tick2 followed by 9 moving held-X and 7 moving held-Z ticks. The earlier X-only `native-24.csv` and one failed X/Z compile log are retained as intermediate evidence. Original Unity Editor compilation and comparison are still pending; no source result alone closes the gate.

Original Editor imported/compiled the new test and strict replay schema. First exact filtered job `30631c5d810a46b89561da4b1dc81738` reached tick1 and failed its test-only raw relation-slot assertion: native unlinked `linked_child_slot` defaults to 0, while Unity's unlinked `TargetSlotIndex` is -1. The tick1 CSV confirms both link states are 0, OID120 remains ground action64, and source/view X/Z match. The assertion now compares slot identity only while the reciprocal held relation is active, preserving the different unlinked sentinels; the original failure remains archived. Recompile and rerun only this test; no production change is inferred from this fixture-projection difference.
