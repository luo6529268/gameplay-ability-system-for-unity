# Q07 current formal DAT reachability and ownership split

Status: `CURRENT_BYTE_INVENTORY_VERIFIED / CONSUMER_REACHABILITY_PARTIAL` (2026-09-23). This report updates the older 74- and 70-missing snapshots. It inventories current files; it is not a copy or deletion authorization. Source is the formal release runtime `resources/runtime/decoded_dat` under the root EXE with SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. Target is `Assets/NTSD/Content/LoganRuntime/decoded_dat`.

Current byte result after the exact `data/kind.dat` package: **405 formal DAT, 338 staged DAT, 338/338 common paths byte-identical by SHA-256, zero extra or differing staged DAT, 67 formal DAT absent**. The enumerated missing paths, size, formal SHA and provisional owner are in `current-formal-dat-not-staged.csv` (SHA-256 `CC34598FD062544979F78EE3743E60453B30C97A7BC76F4EB51F1DAED0A372A3`). The list partitions as follows:

| Provisional owner | Count | Immediate handling |
|---|---:|---|
| Background catalog `b/*` | 24 | Q07/Q08 background resource and stage-boundary caller audit; associated PNG dependencies remain distinct. |
| Background mode children `data/bg/*` | 25 | Q07/Q08 menu/background-mode selection audit; no blind staging. |
| Background mode index `data/bg_mode.dat` | 1 | Formal playable `GameSession28` parses this for the post-roster background cursor; menu/scene boundary owner first. |
| Battle HUD child `data/frame/INKHUD2.dat` | 1 | Q09/R17 frame HUD content and renderer owner; source-to-Unity pixel verification required. |
| Minibar UI `data/minibar*` | 5 | Presentation/menu owner unresolved; preserve nonbattle boundary. |
| Audio `data/bgm.dat`, `data/sound.dat` | 2 | Q10 audio route, packaging and playback; D-023 does not authorize blanket WAV migration. |
| Menu `data/menu.dat` | 1 | Protected nonbattle flow; do not migrate under battle-only scope. |
| Story/stage `s/*` | 7 | Q08 stage/mode caller and default-stage exception audit. |
| Default `data/stage.dat` | 1 | Deployment remains explicitly paused by user; no copy. |

`data/kind.dat` is now staged separately because it is directly consumed by the formal playable battle path. `GameSession28` loads it and passes the parsed `KindCatalog28` into the World. The record is `effect=209`, `frame=40`, `bound={8,209,213}`, `respond={200,203,205,206,207,215,216}`; `HitCandidateBuilder28::classify_kind_table_candidate` rejects a type-3 OID209 target/respond attacker pair for all ITR kinds except 9 before geometry, while `BattleWorld28` uses the bound/respond/frame fields in the type-3 transform. The relevant source files are in `build.ps1 -Target playable`'s compile closure.

Unity currently implements the locked values in `BruteForceSceneQuery.IsBlockedReleaseOidInteraction` and `BattleDamageWriter.IsNativeLockedKindTransformCandidate`, with existing `NTSD28B5Type3KindCatalogTransformEditorTests`. The candidate helper does not accept target object type as a parameter, whereas the formal helper gates on type 3; this is a **contract candidate**, not a measured current-roster difference because the current formal OID209 type must be checked at the live call site. The Unity catalog/identity currently includes object, fusion and selected mode components but no `kind.dat` fingerprint, and no production kind DAT reader was found. Therefore the content copy must not be treated as a data-driven rule migration. A new pre-change Task/Change Record should first trace both Unity consumers and authority fields, then specify immutable parse/validation, candidate and type-3 transform publication, content identity/version/replay implications and focused formal-vs-Unity tests. Existing locked-value behavior should not be removed until the parsed catalog is proven equivalent.
