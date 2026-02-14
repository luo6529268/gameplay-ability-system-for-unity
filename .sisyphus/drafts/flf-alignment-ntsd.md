# Draft: FLF alignment in NTSD interaction/collision

## Requirements (confirmed)
- Fix FLF alignment issues around `pre_interaction` / `interaction` arest timing and placement.
- Separate scene query semantics into:
  - geometry-only query
  - business kind policy filtering
- Change collision volume mirroring width source to dat-defined width (`sp.w` equivalent), not runtime sprite texture width.
- Keep using `_FrameDataWrapper` (do not switch to `Data`).
- Add a dedicated function for dat-width logic (avoid broad unrelated changes).
- Use external references for validation:
  - `J:\QQFile\NTSD 2.4.1 工具人亲测能玩\chars`
  - `G:\GoggleDownload\LF2.IDE-master`

## Research Findings (current session memory)
- Prior notes indicate FLF does not gate entire `pre_interaction` at function entry with arest.
- Prior notes mention multiple plan/spec files were reportedly created in `.sisyphus/plans/`.

## Open Questions
- I cannot currently verify those prior plan files in this repo; should I regenerate a single consolidated plan now?
- What exact execution target do you want next:
  - continue planning only,
  - reconcile existing plan files first,
  - or proceed to final single work plan generation?

## Scope Boundaries
- INCLUDE: analysis-backed planning for the 3 alignment topics above.
- EXCLUDE: direct code implementation in this phase.
