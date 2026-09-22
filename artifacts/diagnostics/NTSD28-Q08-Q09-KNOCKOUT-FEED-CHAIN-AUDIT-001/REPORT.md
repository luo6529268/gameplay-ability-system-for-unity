# Q08 to Q09 native knockout-feed chain audit

Date: 2026-09-22. Status: `STATIC_PRODUCER_AND_CONSUMER_GAP_CONFIRMED`. Read-only audit; no runtime or pixel acceptance.

## Formal playable path

- `battle_world.cpp::record_native_knockout` emits a persistent `WorldKnockoutEvent28` just before eligible lethal HP subtraction in both unarmored and selected-armor standard-hit branches. It increments the credited entity's `knockout_count_358` and records tick, victim/source/credit slots, source object type, and the source's four-owner slot. The credit owner and four-owner display slot are distinct contracts. `prune_native_knockout_tail` removes only expired newest records.
- `GameSession28::initialize_resources` loads the selected `mode.dat` child into `NativeKnockoutFeedConfig28`. The active formal `data/mode/ntsd.dat` `#killtext` block enables modes 0, 1, and 4, with `times:70`, `loop:40`, screen positions, team/name flags, and `pic_type0..6`. Its exact three selected PNGs have already been staged by `NTSD28-Q09-NATIVE-KNOCKOUT-FEED-ICONS-STAGING-001`.
- After each native core tick, `GameSession28::step` emits audio for newly appended events and prunes the tail, then advances earthquake and camera. `GameSession28::snapshot` passes the feed config to `render_snapshot.cpp`. The snapshot draws only records in the hard-coded new-event window (`tick < event_time+30`), with mode/bound/display/ID and live attacker/victim gates, while row spacing and record expiry follow separate rules. `d3d11_renderer.cpp` draws the whole type PNG and two WORDS/fallback name strings after mode HUD layers. Sound belongs to Q10, independent of these staged PNGs.

## Current Unity boundary

- `BattleDamageWriter.ApplyNativeStandardHitKnockout` already applies the eligible lethal pre-HP `KnockoutCount358` credit; existing Q06/B5 tests cover that count. This does **not** create the native persistent record. The searched `Assets/NTSD/Scripts` paths contain no `WorldKnockoutEvent` carrier or `#killtext`/`sprite/kill` reader. `SimulationWorld.KillStats` and `KnockoutCount358` are aggregate counters, insufficient to reconstruct event order, source object type, separate four-owner slot, tick, or the stack-tail expiry rule.
- `NTSD28BattlePassOrder` names `SessionKnockoutFeedAndPrune`, but the searched C# tree has no executing consumer of that pass ID. Current `BattleRenderCommandType` contains Shadow, Entity, OverlayGlyph, and HitRecord; `BuildCommands` overlays are per-entity WORDS glyphs and contain no knockout-feed row. The three PNGs are exact content preconditions, not proof of a Unity reader or on-screen display.

## Ownership and next implementation boundary

Q08 owns the native knockout event producer, immutable per-tick event/record carrier, post-core prune order, and separate audio-event handoff; it must reuse the existing lethal credit instead of incrementing `KnockoutCount358` twice. Q09 owns the `#killtext` config/content identity, snapshot projection, icon/name command order and visible pixel comparison. Q10 owns the associated sound resource and playback. Q08 must establish the event data contract before Q09 can faithfully render it. Each code package needs its own pre-change Task Contract and Change Record and must preserve the current Unity presentation framework, nonbattle UI, and eleven-stage shutdown order.

Focused acceptance should use one eligible lethal standard hit plus an armor case and an ineligible gate, compare pre-HP event timing/source-vs-credit-vs-four-owner fields with formal same-seed traces, then test 30-tick draw cutoff, 70-tick tail expiry with older/newer records, modes 0/1/4 versus disallowed mode, type image selection and text order. Finally verify original-project pixels and exit/re-entry zero residual. Do not use aggregate knockout count or successful PNG loading as a proxy for that full chain.
