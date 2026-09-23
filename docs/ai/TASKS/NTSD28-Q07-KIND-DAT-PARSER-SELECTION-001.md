# NTSD28-Q07-KIND-DAT-PARSER-SELECTION-001

Status: FOCUSED_TEST_PASS. Parent: BATCH-04/Q07. Scope is original NTSD Unity project only.

Authority: formal EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, paired playable `kind_catalog.cpp`/`game_session.cpp`, `source/README_SOURCE.md`, and formal selected `resources/runtime/decoded_dat/data/kind.dat` SHA-256 `39E30DF8D86A5FC374B26C80BE358A6503B2C3096D8681C586478D73A0900011`. The staged Unity file has the same SHA. Full call-chain and identity dependencies are in `artifacts/diagnostics/NTSD28-Q07-KIND-DAT-READER-AND-IDENTITY-CONTRACT-AUDIT-001/REPORT.md`.

Pre-change: Unity has no kind DAT parser/selected input. `BruteForceSceneQuery` and `BattleDamageWriter` use locked literals; they remain unchanged in this package. Existing unrelated dirty files and Scene are protected.

Exact code paths: new `Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs`, new `Assets/NTSD/Scripts/DatParser/Runtime/Parsing/LoganKindCatalogParser.cs`, new `Assets/NTSD/Scripts/Animation/LoganKindCatalogInput.cs`, new `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07KindCatalogInputEditorTests.cs` and matching Unity `.meta` files. No other production code, Scene, ProjectSettings or resource edit.

Exit for this independent foundation: immutable records/diagnostics with C++ grammar, strict int/list/count/100-record handling, first effect lookup, selected-file versus exact locked fallback and native path priority, separate raw/semantic fingerprints, and freshness rejection for bytes/path changes. Test formal staged file and fallback semantic equality; test malformed selected input, type/list edge cases and capture freeze. Original project Unity compile and focused EditMode tests are required before reporting the parser foundation verified. This does not publish the table to battle, change content identity, retire literals or close Q07. Those remain the next connected package, including R15 schema/identity review.

Rollback: reverse only the four declared new script files and generated metas after reviewing current work and obtaining any deletion approval required by AGENTS.md. No cleanup or reset is authorized by this Task.
