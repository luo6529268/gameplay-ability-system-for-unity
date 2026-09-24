# Character AI source-position implementation gate

2026-09-24 read-only production-path map after two-profile focused witness. This is an implementation contract and evidence inventory, not a parity claim or script-change authorization by itself.

## Authority and confirmed first difference

- Formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; playable `SimulationTickDriver28::step` calls `NativeAi28::step_main`; `NativeAi28::select_local_character_target` compares `entity.position.x/z` in `native_ai.cpp` under strict Manhattan `<` and slot order.
- Unity `Assets/NTSD/Config/GameConfig/GameConfig.asset` sets `BattleAiExecutionProfileName: DataOrientedCanonical`; `SimulationTickDriver` resolves that profile before World setup. The alternative LegacyCanonical remains supported and must be equivalent.
- Original-project Editor formal-expected job `30d46b8a54c44c1a9de0ed8f33f873ff`: DataOriented and Legacy each selected physical-nearest slot2, while source-nearest is slot1. The source X gaps were 20/40, physical X gaps 100/30, all Z equal. The final 2/2 passing test `a2ba788ec3744f09a2be57da8175b8f7` is deliberately a current-defect characterization, not a fix.

## Unity position flow that one patch must close

1. `SimulationAiSensingModule` captures `rows.X/Z` from `runtime.XInt/ZInt` at `1650-1652`, then sorts role indexes on `rows.X` near `1794`; candidate broadphase consumes those indexes. A single distance-comparator change cannot repair indexed ordering/pruning.
2. `SimulationAiDecisionModule` captures owned rows from runtime X/Z near `2558-2560`, and unified rows from `BattleFrameMotionAiProjection.X/Z` near `1480-1482`. `BattleFrameMotionStore` sources that projection from canonical physical frame-motion fields. The unified publisher also stages physical X/Z mutations. AI source projection must not alter canonical physical store or render position.
3. The same decision module refreshes/compares row X/Z after input and has legacy/remainder fallback `X/Z` methods near `4222-4235` that return physical runtime values. It also uses `UnifiedMoveModeFirst10X/Z` and teammate/threshold consumers. Every path must consume a coherent domain for the same AI pass.
4. `SimulationAiInputModule` reads `world.X/Z` in brute, spatial/best-first nearest, lane, special and move-mode branches. Its spatial broadphase must index and query the same domain. Refresh after in-pass position mutation must use the chosen domain, not stale initial rows.

## Required behavior and evidence before production promotion

- Keep physical `Runtime.XInt/ZInt`, `BattleFrameMotionStore` and presentation unchanged under D-024. AI reads source integer X/Z when the decision has complete authoritative source history for its relevant subject/candidates. An incomplete-history fallback must choose one coherent physical domain for the whole affected decision; never compare mixed source/physical values or build an index in one domain and query it in the other.
- Define when the AI domain is latched within the current tick, how it is refreshed after mutation, and how legacy and DataOriented paths share the rule. Source coordinate initial writes, reset/reuse and snapshot/restore must be checked before activating the reader. Do not add DAT values or rescale AI thresholds to hide the difference.
- RED/GREEN: original-project Editor two-profile nearest-target witness must change from slot2 to slot1 with complete source history, while missing-source fallback retains the current physical result. Add a threshold case, same-tick refresh, broadphase versus brute parity, replay/checksum, allocation check, full Driver and natural Battle Scene representative. Formal EXE same-seed/input/tick comparison remains the final authority gate. Existing AI class-wide tests need to pass after focused cases, not before the source-domain path is correct.
- A production Task/Change must name every touched script before edits, specify invariant and rollback, and keep this test-only witness record separate. Q07 remains paused while this confirmed difference is open.
