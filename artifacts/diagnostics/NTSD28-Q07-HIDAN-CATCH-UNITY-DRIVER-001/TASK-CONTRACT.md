# NTSD28-Q07-HIDAN-CATCH-UNITY-DRIVER-001

Status: `PLANNED`, BATCH-04/Q07. The source and root formal EXE controlled Hidan action236 catch cases are verified in `NTSD28-Q07-HIDAN-CATCH-FULL-DRIVER-001`; original Unity complete-driver parity remains unknown.

Final scoped status: `VERIFIED`; see `ACCEPTANCE.md`. Natural input and Battle Play remain open.

Authority and fixture: formal EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, indexed Hidan OID24/type0 `c/hid/hid.dat`, paired source `GameSession28::step` X500 versus X520/X1200, initial action236, seed `0x28A55A5A`, mode0, Stage23, difficulty0, 24 ticks without input. Use staged formal runtime content only when its catalog SHA matches the formal root. This is a controlled initial action, not normal input reachability.

Declared script before edit: `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`, only to add a strict Hidan scenario schema and fixed-state validation to the existing raw-capture diagnostic. Add two scenario JSON fixture files under this evidence directory. Do not alter production battle code, DAT values, image/audio, Scene, config, nonbattle code, or the Unity framework. Preserve all existing dirty work and the concurrently edited BattleControlsView.

Acceptance: original Editor compiles with zero errors; its existing request-file raw capture runs both controlled scenarios for 24 ticks, with output and result files preserved. Compare selected action, HP/PP, catch relation and timing to the root formal trace, recording the first difference if any. Run focused validation and Change Ledger; check protected Scene/config hashes. No blanket test suite. Rollback is limited to this new schema/validation and scenario artifacts; keep historical evidence. Do not claim Q07 complete or natural Battle Play from this diagnostic.
