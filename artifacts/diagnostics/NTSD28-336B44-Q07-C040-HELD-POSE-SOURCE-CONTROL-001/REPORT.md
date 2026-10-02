# C040 held-pose formal pass control (2026-10-02)

Status: `FOCUSED_TEST_PASS / FORMAL_PASS_ONLY`. Parent Q07/C040 remains open.

Authority identity: root formal `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`; corresponding playable `BattleWorld28::settle_catch_relations` in `source/ntsd28_core/src/simulation/battle_world.cpp`, current formal decoded DAT `c/hin/hin.dat` SHA-256 `C12062CE234518E54CFCFD577B7AC55F2B58A2A39E5038F64ABBEDDA122CD32F` and `c/bee/bee.dat` SHA-256 `0E52DFBE044178D654C3E5D1694B9A733AC3D15AE1E8BBFE8E0E33169ACA7A4F`. Diagnostic sources are separately compiled; the resulting binary is not the root formal EXE.

Input: initialize formal playable `GameSession28` with seed 682973786, mode 0, Hinata OID41 slot0 action125 at (500,0,400), Bee OID75 slot1 action132 at (550,0,400), then set reciprocal catch links and rightward facing. The two independent sessions differ only in Bee `motion_hold_timer`: 5 or 0. Invoke the actual formal catch-settlement pass directly once. Hinata frame125 has vaction130, Bee current frame132 kind-2 CPOINT (58,47), while Bee vaction frame130 kind-2 CPOINT is (41,39); both frames have center (39,79). The probe derives the expected anchor from these formal fields and the pass cover/z rule before comparing the actual result.

| Bee hold before | Bee action after | Expected XYZ | Actual XYZ | Active/synchronized | Pass success |
| --- | ---: | --- | --- | --- | --- |
| 5 | 132 | 529,1,399 | 529,1,399 | 1/1 | yes |
| 0 | 130 | 529,1,399 | 529,1,399 | 1/1 | yes |

The retained-action row discriminates the rule: taking CPOINT from the current frame132 would place Bee at X512 instead of X529. The formal pass uses current-frame center with vaction-frame positional CPOINT, including when positive hold leaves the current action unchanged. `run-01/held-pose.csv` and `run-02/held-pose.csv` both SHA-256 `4314A9F693949B9CADFF23B8BA5D1726974A1C83EBF9DABB32452BFF0713C056`.

The initial compile failed at link with duplicate `main` because the reused argv still named the previous Q10 probe. The corrected `compile-argv.txt` removed that source, and g++ completed with exit 0. Both probe processes completed with exit 0. This is a controlled source-pass witness, not a natural full-Driver or root-EXE-LFR case; neither physical-key reachability nor Unity Play is claimed. Static Unity review of `BattleCpointWriter.SyncHeldPosition` finds that it currently takes both center and CPOINT from `victim.Frame?.D`; a separate change must test and correct both source-rule and projected view coordinates without modifying DAT values. No file deletion or production change was made.
