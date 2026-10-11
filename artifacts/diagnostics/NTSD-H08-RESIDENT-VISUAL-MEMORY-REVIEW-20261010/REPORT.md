# H08 resident visual memory review
Status: SCOPED_REVIEW_COMPLETE / READY_FOR_BOUNDED_IMPLEMENTATION
Task: NTSD-H08-RESIDENT-VISUAL-MEMORY-REVIEW-20261010
Change: NTSD-H08-RESIDENT-VISUAL-MEMORY-REVIEW-001 VERIFIED (Editor diagnostic only)

## Conclusion
The current native body-sheet loader creates duplicate whole-sheet textures and processed pixel arrays for repeated file references. Sharing those resources within one publication is a concrete H08 implementation candidate. Each declaration must retain its own Sprite/crop/first-declared effective-pic ownership. No production code changed or memory optimization applied by this review.

## Fresh evidence
Current serialized GameConfig selects Assets/NTSD/Content/LoganRuntime. Actual C# LoganVisualContentCandidate and BuildCharacterFrameConfigsFromCatalog/BuildFirstDeclaredSpriteOwnership yield 330 definitions,773 live sheet references,703 unique full paths,56 duplicated paths/70 extra references.19 duplicate groups differ in crop size or pic ranges.

inventory.json contains every path and declaration. Header-based RGBA32 source payload is2188710784B for all references,2003918132B for unique paths,184792652B (176.232MiB;8.443% of reference payload) repeated. This is a calculated source payload opportunity, not current whole-battle native/managed/GPU memory, measured savings, or allocation peak. It excludes clamped-cell textures, atlas,UI,audio,temporary decode and object overhead.

Real existing private ProcessAndCreateSpritesForCandidateAsync executed against exact captured SHA for ID19/713 ten.png,130Sprites each. Two different TextureIDs and two separate processed pixel arrays with all pixels identical. Each Texture has RGBA32 payload3328000B; Unity Profiler.GetRuntimeMemorySizeLong reports6656528B separately. Do not add these metrics, interpret them as isolated GPU memory, or multiply the fixture's overhead factor across the inventory. Texture.isReadable is false; Apply(false,true) already exists.

focused-result-02.json job943a987ca53345258b466a25024faa2a:2/2 passed. First status query timed out while real parser/converter occupied Editor; terminal result succeeded, not a failed test. Parsing existing configs emitted substantial existing diagnostic logs; no production logger setting changed.

owner-result.json job23185ee03aa94011b96f6e6b3a49ed55:1/1 passed after additive diagnostic compilation. Two distinct rect/pivot Sprite objects share one source texture; original HashSet ownership deduplicates it; actual DestroyStagedPresentation destroys twoSprites+oneTexture, second cleanup0. This isolated helper test does NOT validate candidate cancellation/atomic publication/last lease; those are explicit implementation acceptance conditions.

## Source and ownership
CharacterAnimtorManager.LoadCharacterSpritesCoreAsync builds one work item per surviving file declaration. ProcessAndCreateSpritesForCandidateAsync verified-decodes/prepares/uploads a new whole sheet each time; only native clamped cells currently have content sharing. BuildBattleSpriteCatalog preserves each declaration's effective pics and legacy Sprite. Existing planner/resource index path dedup happens later, after source textures already exist.

NativePrewarmOperation owns Sprites/Textures/Resources HashSets. TryCommitNativePublication hands resource ownership to publishedOwnedResources; RecycleNativeStaging handles cancellation. QueueRetiredSpritePublication/TryRetireCatalogIfUnbound retain old publication until existing catalog bindings release. Reuse these boundaries; no per-declaration lease/refcount or new texture manager. Atlas-only resources must remain distinct from shared source ownership. MatchCentralAtlasPublication already replaces match atlases; do not recreate that optimization.

Independent read-only sidecar reviewed Q08 historic changes and neighboring tests; integration/rejected over-expansion in independent-review.md. Old Q08 OOM/private-byte numbers are historical and not this review's current measurements.

## Exact next action
execution-handoff.txt authorizes a defaultOFF native publication candidate in CharacterAnimtorManager plus the current diagnostic and existing atomic-publication test file. Group same normalized path/expectedSHA/source semantics, decode/upload/sourcePixels once per group, preserve distinct Sprite semantics, cancellation and publication ownership. Validate bounded real duplicate groups plus unique negative control, actual output/pixels/cleanup and original publication tests. Memory/resource counts are the benefit criterion; loading legitimately allocates and is not subjected to unrelated fullDriver0GC gates. No full2GB dual-preload, AB/bake/newatlasformat/device work or next optimization.

## Scope and closure
Original Menu clean8 observed;324 protected source/Scene/config/settings files unchanged, original dirty backups preserved. Source addition is Editor-only; no production/resource modifications. Validator/diff results appended below after completion. M03 candidatesOFF,H07/producer deferred,95OFF,H06not resumed; H08 parent fullprebake/budget/device acceptance remainsOPEN. Dispatch receipt is authoritative for whether the implementation instruction was sent.

Final review: validator PASSED/exit0; scoped diff-check exit0.324 protected files unchanged,6 initial dirty backups verified,staged diff unchanged. Original Menu clean8. Actual3/3 diagnostic tests passed; exact handoff ready. No production optimization yet.

Dispatch completed: exact handoff sent to existing execution thread01a1201b-7930-7c93-8c84-5bc7baada43f. New turn01a125c7-8de5-73c3-b561-1a8e419dccfc confirmed active/inProgress/noerror and acknowledged three-file/defaultOFF scope. Heartbeat resumed ACTIVE, cursor92. Receipt and exact instruction hash recorded.
