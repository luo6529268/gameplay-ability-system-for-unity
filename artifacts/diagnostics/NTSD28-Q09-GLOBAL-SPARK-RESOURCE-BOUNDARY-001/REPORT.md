# NTSD28-Q09-GLOBAL-SPARK-RESOURCE-BOUNDARY-001

Status: `STATIC_FIRST_DIFFERENCE_CONFIRMED / FORMAL_CONTENT_STAGED / READER_AND_RUNTIME_PENDING` (2026-09-22). This is a battle presentation dependency, not a reopening of the Q06 hit/spark event producer or lifecycle checks.

## Formal playable path

- Formal `resources/runtime/decoded_dat/data/resource.dat:45` gives global resource index 43 as `sprite\UI\SPARK.png`. `NativeResourceCatalog28::native_spark_index` is 43; `GameSession28` resolves that resource against its sprite root when loading the formal VFS (`game_session.cpp:1241-1246`).
- Formal `resources/runtime/decoded_dat/data/system.dat:15-16` gives `spark_w: 99`, `spark_h: 79`. `NativeSystemVisualConfig28::load` reads those values, and `GameSession28::snapshot` passes them to `RenderSnapshot28` (`game_session.cpp:1251-1253,3315-3321`). `render_snapshot.cpp:1635-1662` uses the dimensions and a one-pixel cell gutter to make source rectangles. Native IDs have 10-column/10-row bounds, with every tenth ID non-drawable; this bound alone does not prove every source rectangle is present in this particular PNG.
- The formal VFS file `resources/runtime/vfs/sprite/UI/SPARK.png` is 500×320, 3,600 bytes, SHA-256 `15D8843E0CE87FF63F46DFF7170D30C23BAEA0F2799434B26717AADFD5EC881B`.

## Current Unity path and first difference

- `CharacterAnimtorManager.BuildSparkPublicationAsync` (`Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs:1649-1714`) always reads the legacy `Assets/NTSD/Sprite/UIPanels/SPARK.bmp`, requires an at-least-510×256 BMP, and builds 20 old-layout Sprite bindings.
- `BattleCommonVisualCatalog.GetSparkPixelRect` (`Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteCatalog.cs:584-624`) uses old variable 102×80 / 61×48 cells. `SparkRenderer.RenderAll` consumes those bindings. There is no `resource.dat` index-43 / `system.dat` size reader on this publication path.
- The staged formal root `Assets/NTSD/Content/LoganRuntime` does **not** contain `vfs/sprite/UI/SPARK.png` as of this audit. A different character-skill file, `vfs/c/kid/a/spark.png`, is present; its similar basename is not authority for the global spark resource.

## Exit path and boundaries

1. Scope the missing index-43 global PNG and relevant `resource.dat`/`system.dat` fields as a battle presentation content dependency; verify importer/path and exact owners before adding or replacing files. Do not delete the old BMP while its current publication path or serialized/test references remain.
2. Add a versioned formal-common-spark publication branch preserving Unity renderer/pool architecture; derive rectangles from the formal sizes and resource index, and keep legacy behavior only where explicitly needed for unmigrated content. Do not alter Q06 hit production, spark age, or battle logic to make the image look right.
3. Compare native snapshot spark command and Unity battle command for the same hit/age/source rectangle, then verify actual pixels in the **original** Unity project at a focused battle scene. The formal resource and current Unity source difference is statically proven; audible/visible runtime parity is not.

No Unity process, project, Scene, script, or resource file was changed by this audit. `Q09` remains open; `Q06` local producer evidence stays closed within its stated scope.

## Subsequent scoped content entry (2026-09-22)

`NTSD28-Q07-GLOBAL-SPARK-CONTENT-ENTRY-001` subsequently copied only `decoded_dat/data/resource.dat`, `decoded_dat/data/system.dat` and `vfs/sprite/UI/SPARK.png` into the staged formal root; all three hashes match formal runtime. Thus the missing-staged-file observation above is a pre-copy snapshot, not current state. The Unity common-spark loader still reads old BMP and still lacks the formal layout reader; Q09 remains open pending its separate code/Play package.

## Subsequent native-ID/layout correction (2026-09-22)

`NTSD28-Q09-NATIVE-SPARK-ATLAS-REACHABILITY-001` traced the actual native event-to-D3D11 path and enumerated all 100 logical IDs against the staged 500x320 PNG. Only IDs 0-4, 10-14, 20-24 and 30-34 have a drawable in-bounds 99x79 source cell; every tenth ID is explicitly suppressed. The PNG's decoded alpha is entirely 255, so exact native black color key is necessary. Unity's 20-picture age mapper collapses some distinct native IDs and is shared by legacy and central renderers. A Q09 fix therefore needs a formal-content raw-ID presentation branch, not a path-only PNG substitution or a blind 20-Sprite rect change. See `../NTSD28-Q09-NATIVE-SPARK-ATLAS-REACHABILITY-001/REPORT.md` and its 100-row CSV. Q06 producer/lifecycle and old BMP ownership remain untouched.
