# H07 input facing causal check — NOT ADOPTED / MEASUREMENT_NOT_EXECUTED

Parent Task remains IN_PROGRESS. This is a rejected causal clue, not an optimization failure or production change.

## Reused actual cost evidence
The existing native-owned compatibility OFF report, fine timing disabled, already records CharacterInput mean12.0374633333ms, P95 18.949035ms, P99 27.224985ms over180steady. Overall tick mean51.481118ms. Its GCqualification failed2events and no ON ran, so this is an observed cost baseline, not formal qualification or a gain comparison. No repeat scene was needed.

## Source chain and limitation
- LF2Entity.SwitchDir939 writes Runtime/PS then main-thread Sprite.SwitchLR; Runtime equal-facing guard does not skip following calls.
- LF2Sprite.SwitchLR261 writes native flipX whenever renderer is non-null, even when legacy suppressed. SetLegacyRendererSuppressed347 restores visibility only; skipping the setter would require facing restoration before showing legacy. Worker/central managed-only, pool normalization and state9997 presentation overrides invalidate a generic Dir-equality skip.
- The actual GameConfig asset references EntityObject.prefab GUID9c107ede7c04d83469572b66618c3680. That prefab has no body SpriteRenderer class212. LF2ObjectPool.CreateNewObject150 adds a body SpriteRenderer only for non-CentralOnly. Current saved thousand baseline says CentralOnly, initialRosterLogicOnlyMaterialization=false. No current runtime count of retained legacy pool components was captured; do not claim zero observed setters merely from source. However no demonstrated positive body-setter frequency supports attributing this baseline to flipX.
- Production flipX reads found in stage/presentation shadow construction; no demonstrated battle-state authority, but presentation correctness still required.

## Diagnostic disposition
A fresh finite Editor test was drafted while source review ran, production source untouched. First exact test job fc9cbd4d75754718ace822aaea0ae4f1 was marked succeeded by runner but actual summary total0; Explicit filtering meant NO TEST EXECUTED. It is not a PASS. Temporary Explicit removal was compiled, but further source findings invalidated relevance to current fixture before any second dispatch. The original Explicit file bytes were restored exactly. No result.json, no local cost group, no preview scene or new simulation ran. No speculative improvement is inferred.
The unused Explicit diagnostic is retained with ABANDONED Change for audit; no deletion of files or user work. It must not be revived absent positive actual-runtime frequency evidence.

## Continuation criterion
Prior micro cursor/raw-state candidates remain unproven/not adopted. H07 needs a real workload-reduction proposal across the input causal chain, backed by actual fixture coverage. Passing an isolated synthetic API loop cannot prove this. Production handoff is NOT_READY; do not dispatch this facing candidate or new unrelated optimization. Original source/Scene protection and ledger validation are recorded separately.
