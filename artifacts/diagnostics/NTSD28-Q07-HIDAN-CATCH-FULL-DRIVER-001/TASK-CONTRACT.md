# NTSD28-Q07-HIDAN-CATCH-FULL-DRIVER-001

Status: `IN_PROGRESS / CONTROLLED_FULL_DRIVER_DIAGNOSTIC`. Parent BATCH-04/Q07. The current CPoint resource fix has original Editor focused tests but lacks a whole-session formal EXE catch observation.

Authority: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, paired playable `GameSession28::step`, `SimulationTickDriver28::step`, `BattleWorld28` kind-3 catch/advance/settlement, and formal indexed `c/hid/hid.dat` OID24/type0. Hidan frame236 declares kind-3 ITR with catching action120 and caught action130; frame120 advances to239; frame239 has positive held injury30. This is a controlled initial-action candidate. It is not a natural player-input claim until the action is reached from normal inputs.

Exact script path before edit: new `Tools/NTSD28Q07Diagnostics/hidan_catch_full_driver_lfr_probe.cpp` only. It may create source-authored LFR and CSV in a fresh diagnostics directory for X520 overlapping and X1200 nonoverlap controls. It must not write formal EXE/source/DAT, Unity production/test, Scene, config, image, audio or nonbattle code. Preserve all prior artifacts and unrelated dirty work.

Final scoped result: `VERIFIED`; see `ACCEPTANCE.md`. Ordinary input, Unity complete driver and Battle Play remain separate gates.

Acceptance: paired source full GameSession captures initial and completed tick action, relation slots, PP/HP and local gate; either reaches frame239 with reciprocal catch and a measured resource transfer or reports exact earlier failure. Generate LFR without overwriting old output, independently replay through the root formal EXE with explicit resource/VFS roots, preserve report/trace, and compare common tick fields. A root EXE LFR PASS alone proves packet replay checks, not complete native same-phase parity. Original Unity Editor full-driver parity and natural physical input remain later tasks. Build scripts/command must compile paired playable closure read-only, log actual output, verify formal EXE/content hashes, and run Ledger/diff/protected hash gates. Rollback only this new diagnostic script and evidence, retaining user work.
