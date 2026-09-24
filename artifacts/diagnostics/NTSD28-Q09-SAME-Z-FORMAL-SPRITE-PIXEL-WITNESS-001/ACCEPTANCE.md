# Q09/P-04 formal-sprite same-Z controlled GPU witness

Change: `NTSD28-Q09-SAME-Z-FORMAL-SPRITE-PIXEL-WITNESS-001`  
Status: `FOCUSED_TEST_PASS`; this closes the controlled original-Scene central GPU witness only. Parent P-04/Q09 remains `RUNTIME_PENDING` pending natural full-tick, Legacy, and formal EXE same-condition comparison.

## Authority and fixture

The formal playable `render_snapshot.cpp` sorts final `entity_commands` by ascending Z, equal-Z descending physical runtime slot, then phase. The original Battle Scene `CentralOnly` production World spawned OID120/frame0/pic0 (`w/4.dat`, `w/4.png`) and OID121/frame0/pic0 (`w/5.dat`, `w/5.png`) through the production OPoint factory. The staged PNGs and formal `resources/runtime` PNGs match SHA-256 individually: OID120 `1657FF0084079D1565CD1D303931A5D6F0B68EB51C03AE3D847205`, OID121 `8F8CE022421865DD6998D6580C69629AE25E3C32CF9AD0335912535DB3819C20`. No DAT data was changed.

The probe paused the real driver at tick 5 after the dedicated worker became idle. To bypass the central plan's same-tick cache without advancing battle logic or changing formal frame0, it published four controlled presentation frames with distinct diagnostic tick labels: baseline, A only, B only, both. The original enabled world camera rendered each central submission to a temporary 1920-pixel-wide target; all temporary camera properties were restored. This is controlled GPU outlet evidence, not a natural full battle tick or original EXE screenshot.

## Original Editor result

After fixing test-only fixture issues, the original Editor imported the `.meta`, reported zero Console compile errors, and the focused original Battle Scene request returned `PASS` in [controlled-pixel-pass.json](controlled-pixel-pass.json). The final JSON was independently checked against the four PNG readbacks:

| Witness | Observed |
|---|---|
| Formal handles | OID120 slot51 generation1; OID121 slot52 generation1 |
| Depth/command order | both Z240; slot52 body command index1, slot51 index3; greater slot painted first |
| GPU overlap region | projected formal body-command intersection x1000..1068, y551..606 in 1920x1080 readback coordinates |
| Pixel oracle | 11 discriminating pixels inside that intersection; 11 match later OID120, 0 match earlier OID121, 0 mixed |
| Representative pixel | x1033/y578 (bottom-origin): baseline 255,255,255; OID120 only 165,165,147; OID121 only 201,201,182; composite 165,165,147 |
| Cleanup | objects 4→4, claimed slots 2→2, renderer pool 2→2, logic pool 2→2, prior pause restored; probe result says RNG/sound restoration performed |
| Scene | Editor exited Play; `NTSD_Battle.unity` disk SHA-256 before/after exit `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39`; Git Scene diff empty |

The sample is inside the two actual submitted body-command rectangles. A separate Python/Pillow read of the saved images reproduced the pixel values and the full 11/11/0/0 count. The readback files are [baseline](capture-0.png), [OID120 only](capture-1.png), [OID121 only](capture-2.png), and [composite](capture-3.png).

Earlier attempts are retained as diagnosis, not acceptance: the first producer inherited the wrong base and failed before spawn; the second lacked a discriminating sample; the third changed precise positions without syncing integer render coordinates; the fourth reported a full-screen match near an unrelated character and was invalidated by independent review. The final probe now restricts comparison to the two body-command intersection and separately counts matches/opposition/mixed pixels. The archive contains their JSON and superseded images.

## Remaining exits

This probe deliberately does not step the full battle simulation while both weapons are visible. It does not verify Legacy rendering, natural camera/display frames, formal EXE same-condition pixels, or the rest of Q09. Those remain parent P-04/Q09 work. There was no broad test-suite run; the scoped test affected only an Editor diagnostic script and generated outputs.
