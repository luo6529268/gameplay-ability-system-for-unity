# Q07 Hidan controlled catch through complete GameSession

Status: `VERIFIED` for this bounded diagnostic; Q07 remains `IN_PROGRESS`.

The paired playable source and root formal EXE both used indexed Hidan OID24/type0 and formal runtime DAT, mode 0, seed `0x28A55A5A`, actor X500/action236, target X520 or X1200, and 24 complete simulation ticks without further input. The formal EXE identity was checked against SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. Action236 is a controlled initial condition; this experiment does not establish reachability from ordinary player input.

| Case | Paired source GameSession | Root formal EXE | Independent selected-field comparison |
|---|---|---|---|
| X520 overlap | Catch relation at tick1; actor action239 at tick2; tick3 actor/target PP 300→322 and target HP 500→470 | Corrected LFR replay PASS, failureCode0, tick3 values and relation match | 24 ticks × 7 fields, 168/168 equal |
| X1200 miss | No catch or held injury in 24 ticks | Corrected LFR replay PASS, failureCode0, no catch or held injury | 24 ticks × 7 fields, 168/168 equal |

Seven compared fields: actor action, target action, actor PP, target PP, target HP, actor catchTarget slot, target catchSource slot. Source CSV and root trace are retained in this folder. The EXE replay report's `nativeParityClaim:false` states that LFR PASS alone verifies encoded replay checks, so the explicit trace comparison is the evidence for these seven fields. The source/GameSession and root EXE show more than a direct World call, but Unity complete-driver parity is still untested.

Failed attempts are retained: initial source run used `resources/runtime/decoded_dat` as the catalog root and failed to open `catalog.csv`; corrected source runs use `resources/runtime`. Initial EXE replay lacked `--lfr-slot0-action 236` and failed with code 46; corrected X520/X1200 runs use that override. The g++ compiler returned 0 and produced the binary, although the PowerShell wrapper returned 1 when it tried to read an absent empty compiler log after success. These failures are setup errors, not evidence of a combat mismatch.

Only a new diagnostic C++ source and task/evidence documents were written. No DAT, Unity production/test code, Scene, config, character image or nonbattle file was changed by this diagnostic. Next gate: same controlled initial state through the original Unity Editor's full battle driver, then normal input reachability and battle Play observation where applicable. This scoped result does not close Q07 or the alignment goal.
