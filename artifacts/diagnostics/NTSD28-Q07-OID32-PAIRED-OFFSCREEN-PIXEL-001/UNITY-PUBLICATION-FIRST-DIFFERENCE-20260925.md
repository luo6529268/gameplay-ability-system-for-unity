# Q07 image-boundary publication first difference

Status: `VERIFIED_STATIC_SEAM / PRODUCTION_FIX_PENDING`. This note follows the paired-source WARP pixel witness and the original Unity Battle Scene SpriteCatalog witness. It changes no script, DAT, image, Scene, Prefab or importer.

The paired playable renderer submits `sprite.frame.source_x`, `source_y`, `width` and `height` directly to `draw_quad` (`source/ntsd28_playable/src/d3d11_renderer.cpp:1615-1635`), using a point-filtered CLAMP sampler (`:405-409`). Its OID32/action95 offscreen readback has a 79×79 all-white rectangle exactly at the actor projection. In Unity, `CharacterAnimtorManager.ProcessAndCreateSpritesForCandidateAsync` builds rectangles against the *actual* PNG dimensions (`:2372-2399`); `BuildIndexedSpriteRects` clips and leaves a null rectangle when the source starts beyond the image (`:3258-3301`). The sprite creation loop skips null (`:2423-2452`), and `BuildBattleSpriteCatalog` skips any missing legacy sprite and null rectangle (`:2509-2546`). The original saved Battle Scene witness therefore finds authored OID32 pic0 but no pic64. This is the specific publication first difference; it is not a DAT value error.

The prior all-frame census identifies six base-pic rectangles outside the source image. Read-only inspection of the current formal-equivalent PNG bytes shows the sampled bottom-edge strip for each requested 79-pixel horizontal interval:

| Formal DAT/frame/pic | Source image and requested top-origin rect | Clamped bottom-edge strip |
| --- | --- | --- |
| `c/saso/pup.dat` 105/44 | `pup.png` 799×319, `(320,320,79,79)` | 79 transparent pixels |
| `m/nin/hun.dat` 95/64 | `hun.png` 799×480, `(320,480,79,79)` | 79 opaque white pixels |
| `m/nin/nin.dat` 31/81 and 41/91 | `nin.png` 799×560, `(80,640,79,79)` and `(80,720,79,79)` | each 79 opaque white pixels |
| `m/nin/nin2.dat` 31/81 and 41/91 | `nin2.png` 799×560, same rectangles | each 79 opaque white pixels |

This predicts all-white or transparent cells for these static source coordinates under the confirmed point/CLAMP sampler, but only OID32/action95 has paired-source GPU and root-EXE command evidence here. It does not establish natural gameplay reachability, runtime visual-offset coverage, direct root-EXE GPU output or Unity same-state entity pixels for every row.

The tempting remedy of enlarging every sheet to its full DAT-declared grid is broad: a read-only scratch scan parsed 769 of the 773 sheet declarations in the earlier census and found 239 rows whose declared vertical grid extends past image height (35 by at least one full cell). The scratch parser does not cover the four unmatched declarations and is a sizing signal, not a complete formal inventory. For example, `hun.png` is 799×480 while one declared 10×14 grid at 79×79 plus separators would make it roughly 800×1120. Blind full-grid padding would alter atlas size/memory and potentially placements across many sheets, even when no battle frame uses the extra cells.

The implementation seam is the formal-content sprite prewarm/publication transaction, with a generic out-of-image point/CLAMP sampling rule for **requested drawable cells** and consistent Legacy Sprite, `BattleSpriteEntry`, central source/atlas binding and ownership cleanup. A change limited to `BuildIndexedSpriteRects` cannot create a valid Unity Sprite or atlas binding for a rectangle completely outside its texture. Before production edits, make a new Task/Change with the exact prewarm, catalog, atlas and shutdown paths, define memory bounds and a full/partial/negative-rect test matrix, then compare the original Editor's publication and pixels with the paired-source witness. Preserve raw PNG/DAT bytes and all excluded visual boundaries. Q07 and Q09/R17 remain open.
