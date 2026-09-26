# Q09/P-08 low-HP bpoint bleed mark: scoped source and consumer audit

2026-09-26. Status: `CONFIRMED_CURRENT_CONTENT_PRESENTATION_GAP / IMPLEMENTATION_PENDING`.

## Authority and current content

- The root formal `NTSD2.8-Logan.exe` SHA-256 is `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. Its paired playable rendering source `source/ntsd28_core/src/rendering/render_snapshot.cpp` lines 1777-1817 emits a `RenderBleedMark28` for each frame `bpoint` when `current_hp <= threshold`, after the body sprite and only for `action < 1000`. Threshold priority is valid `bpoint.respond` (1..100), then valid `stats.bleed_hp`, else integer `base_max_hp / 3`. Zero `w`/`h` default to 1/3; `rect` maps through `native_bpoint_color`, with absent/zero yielding red `0x00FF0000`. X mirrors by facing using sprite width/center and body shake; Y uses position Z+Y, sprite center and bpoint Y. The command is ordered after its entity body (`render_snapshot.cpp` lines 2137-2149). `source/ntsd28_playable/src/d3d11_renderer.cpp` lines 1639-1659 draws the solid colored rectangle in the presented camera with entity interpolation.
- A fresh scan of staged formal `Assets/NTSD/Content/LoganRuntime/decoded_dat/**/*.dat` found 318 `bpoint` declarations in exactly two DATs: `c/ita/ita.dat` 231 and `c/sasu/sasu.dat` 87. All 318 declarations specify `x` and `y` only; none specifies `w`, `h`, `rect`, or `respond`. Neither DAT declares `bleed_hp`. For this **current content**, the formal defaults are therefore a red 1×3 mark at `base_max_hp / 3`; this corpus observation must not replace the generic source priority rule. No DAT values were changed.

## Unity consumer gap

- `Lf2DatConverter` already seals source-ordered bpoint X/Y into `LF2FrameData.BloodPoints` via `BattleBloodPointCatalog`. The value and canonical scalar contract currently retain only X/Y. The runtime capture in `BattlePresentationShadowBuild.cs` has `currentFrame`, `runtime.HP`, and `runtime.HP3` (base max), but `BattlePresentationEntitySnapshot` does not carry bpoints. `BuildCommands` emits Shadow, Entity, OverlayGlyph and HitRecord, with no bleed-mark command. This is a positive downstream consumption gap, not an inference from type names alone.
- The central `BattleCatalogCentralResourceResolver` accepts only the four existing command/resource classes. A solid bleed rectangle needs an ordered rendering outlet with an actual 1×1 solid/white resource or equivalent ordered solid-quad path. Treating the shadow ellipse as the solid resource or drawing in a detached after-pass would give incorrect pixels/order.
- The current `BattleBloodPointValue` X/Y-only contract is enough to render the **current** 318 records using formal defaults. Before claiming a generic future-DAT contract, account for optional `respond/w/h/rect` and definition-level `bleed_hp` in the content identity and parser; do not silently hardcode an OID-specific branch or edit any DAT.

## Next bounded implementation and exit

1. Declare a separate Task/Change before script edits. Add a current-frame, source-ordered bleed input to the immutable presentation snapshot and propagate it through both snapshot copy paths. Respect formal body visibility, `action < 1000`, HP threshold and frame facing/center/shake rules. Maintain user-approved physical presentation scaling without writing back to battle coordinates.
2. Add an ordered solid-quad resource/render outlet to the existing central command stream and the necessary legacy outlet, preserving per-entity body→bleed→tail order. Avoid a free-standing late overlay or a special case for the two character OIDs.
3. Run narrow original-Editor tests for HP above/equal/below threshold, frame change, facing, body suppression, multiple bpoints/order, solid resource resolution and pixel color/geometry. Then obtain original Battle Scene Play and formal EXE same-state visual evidence. Static source/current-content evidence alone does not close P-08, Q09 or the full goal.

Recovery-document note: the three v3 candidate replacements in `NTSD28-Q07-PROGRESS-DOC-RECOVERY-20260925/RECOVERY-V3-QA.md` were already installed previously. The current live alignment, handoff and STATE documents are NUL-free and include later addenda; re-copying the frozen candidates would discard those updates. This audit does not replace any of those three documents.
