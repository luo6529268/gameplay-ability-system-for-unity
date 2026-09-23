# Target-present `hit_Fa=7` full-tick witness

Status: `SOURCE_MODEL_CAPTURE_PASS / UNITY_PENDING`. This is a repository diagnostic model built from the paired playable source, not a formal EXE screen observation. Q07/R15 and the full alignment goal remain open.

The dedicated [runner](../../../Tools/NTSD28AuthorityTrace/hitfa7_target_fulltick_main.cpp) and [fixture](scenario.json) use formal runtime OID99/action0 as target in slot0 and OID875/type3/action55 as subject in slot1. The fixture uses Stage23 Z620/Z600 and three no-input ticks. After `GameSession28::initialize`, the runner verifies both authored entities, then sets only the subject's preassigned target slot to 0 and initial Vy to 3.8 before calling real `GameSession28::step()` for each tick. It does not modify formal J: sources, the generic scenario schema or the existing shared capture runner. The fixture's `referenceExeSha256`/`dataSha256` values are the loader's inherited locked scenario metadata; the current formal EXE identity is verified independently by the build script.

[The corrected build manifest](source-stage23-build-manifest.json) records formal EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, paired-source manifest `07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F`, runner SHA `E09082407F562DA711B61DE5C6FCAF1B8A29A6C58E35B62E637C150C332364D1` and binary SHA `DA8297D82DBF792E4866B56EC630C93448131F1851BA4C58FD88D7B8BFFA18FE`. The [source witness](source-stage23-witness.jsonl) is:

| Completed tick | Action | Vy | Integer Y | Precise Y | Previous Y | Entities |
|---:|---:|---:|---:|---:|---:|---:|
| 0 | 55 | 3.8 | -30 | -30 | 0 | 2 |
| 1 | 55 | 5.2 | -20 | -20.600000000000001 | -30 | 2 |
| 2 | 60 | 0 | -15 | -15.400000000000002 | -20 | 2 |
| 3 | 61 | 0 | -15 | -15.400000000000002 | -15 | 3 |

Action55 has `dvy:1` in the formal DAT. The full tick includes native AI, frame motion and physics in that order, so the earlier local AI→physics seam's `-21.6` is not this full-tick expectation. Tick3's third entity is an observed source-model birth, not yet classified or matched in Unity.

The first Editor attempt used a two-tick Z100/Z120 fixture and failed *before battle setup* because the existing Unity replay wrapper requires three ticks and Stage23 Z542..712. Its [earlier source output](source-witness.jsonl) and [build manifest](source-build-manifest.json) are retained solely as superseded fixture history. The fixture and Unity test now target the corrected three-tick Stage23 witness. The original Editor was subsequently observed in another Play-mode transition, so the corrected Unity test has not yet compiled or run. No cross-end equality is claimed.
