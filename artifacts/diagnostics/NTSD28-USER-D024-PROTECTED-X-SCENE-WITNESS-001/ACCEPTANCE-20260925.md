# D-024 protected-object X boundary: original Battle Scene witness

Status: `VERIFIED` for the controlled scene witness; parent D-024/Q07 remains open.

The original Unity Editor imported `NTSD28D024ProtectedXSceneWitnessEditor.cs` through a forced asset refresh and reported zero console errors. Request `d024-protected-x-play-1` entered the saved `NTSD_Battle` scene, found the formal OID122/123 type6 definitions, registered two transient positive-participant objects, and advanced the production `SimulationTickDriver` exactly one tick (0 -> 1). Both objects survived at formal frame 0.

| Coordinate | Before | After |
| --- | ---: | ---: |
| OID122 physical X / source-rule X | 50 / 50 | 100 / 100 |
| OID123 physical X / source-rule X | 2030 / 2030 | 1940 / 1940 |

The stage width was 2040 and both source-rule integer mirrors were 100 and 1940. This matches the formally sourced 100-pixel protected margin in this Unity full-tick path. After cleanup, World objects were 4 -> 4, claimed runtime slots 2 -> 2, renderer borrowers 2 -> 2, and `cleanupPassed=true`. The Editor exited Play. Battle Scene SHA-256 remained `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; Menu Scene remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`.

Raw result: `original-editor-d024-protected-x-play-1.json`, SHA-256 `80E8D34553B416256CDBAAC45A3C635492B96F6F44EB19697F17BEE79584BD23`.

Scope limit: this is controlled registration in the original scene, not a natural drop/consumption sequence or same-condition formal EXE playback. It closes this diagnostic Task only; Q07, D-024's broader reachability and Q08 mode1 selected-stage gate remain open. No DAT, production script, Scene, camera or nonbattle file was changed by this witness.
