<!-- CHANGE-RECORD
id: NTSD28-Q09-TRUSTED-COMMAND-TEST-CONSTRUCTOR-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_PRESENTATION_TEST_FIXTURE
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCatalogCentralResourceResolverEditorTests.cs
authority: current BattleRenderCommand internal constructor signature and original Editor focused adjacent failure
evidence: ORIGINAL_EDITOR_28_OF_32_FOUR_REFLECTION_FAIL / EXACT_FOUR_PARAMETER_TEST_FIXTURE_UPDATE / ORIGINAL_EDITOR_COMPILE_0_ERRORS / ADJACENT_32_OF_32_PASS / LEDGER_860_20_PASS / FOUR_PROTECTED_SHA_STABLE
-->

# NTSD28-Q09-TRUSTED-COMMAND-TEST-CONSTRUCTOR-001

Created before modifying the old test helper. Current resolver test fixture requests a 23-parameter nonpublic constructor; production has 27 parameters. Four adjacent tests fail at `Assert.That(constructor, Is.Not.Null)` before reaching resolver behavior. This record owns only appending the four existing constructor parameter types and source values to the reflection helper; it does not change production. The P-08 test added in this same file belongs to `NTSD28-Q09-BPOINT-BLEED-CENTRAL-001` and must remain untouched. Task declares acceptance, risk, rollback and protected scope.

Actual test-only edit: `CreateTrustedCommandWithIdentity` now reflects the existing final `Vector2/bool/bool/float` parameters and passes the corresponding `source.StableFootAnchorWorld`, `HasStableFootAnchor`, `ShowSelfFootMarker`, `FootMarkerScale`. No production constructor or resolver logic changed. Original Editor compile/focused rerun and governance validation remain pending at this point.

Fresh result: original Editor compiled with zero current Console errors, then exactly selected resolver/command-writer classes passed 32/32 (`13260ab2c3614e79a4107c92dc026c78`), including the four previously failing trusted-command cases. The original 28/32 job is preserved. Ledger 860/20, diff check and four protected asset hashes passed. This closes only the test fixture gate; P-08 Play/Legacy/EXE and Q09 remain open.
