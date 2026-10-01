# 336B44 G1 / Q01-C052 content recheck

Status: VERIFIED_SCOPED_CONTENT_ONLY (2026-10-01). This is a read-only disk audit; no DAT, PNG, scene, or production code was changed.

The formal root EXE was rehashed as `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`. The serialized `Assets/NTSD/Config/GameConfig/GameConfig.asset` production root is `Assets/NTSD/Content/LoganRuntime`; `CharacterAnimtorManager.ConfiguredContentRoot` resolves this root via `BattleContentSource.ForLoganRuntime`. This audit compares that staged root with the formal `resources/runtime` root. It does not itself prove every Play tick read the expected bytes; the C052 Play evidence is a separate runtime layer.

| Scope | Observed result |
| --- | --- |
| Full `catalog.csv` and `decoded_dat/data/data.txt` | SHA-256 pairwise equal |
| Object catalog rows for OID 2 Naruto, 73 Hayate, 211 Fir, 417 Sla | 4/4 rows equal |
| Decoded DAT bytes | 4/4 source/staged SHA-256 equal; 4/4 formal SHA-256 equal each catalog `plain_dat_sha256` |
| DAT PNG references | 23/23 distinct object-path references; 29 text occurrences; all four per-object distinct counts match catalog `sprite_ref_count` |
| Distinct referenced VFS PNG files | 22/22 source/staged SHA-256 equal; 22/22 formal PNG headers valid |

The shared `c/0/M.png` layer reference is present in Naruto and Hayate DAT and is counted once among 22 distinct files. Face/small/smallb references were checked for file identity only; user-excluded native character HUD remains excluded from production alignment. No original background or mode DAT enters this four-object sample.

This closes only the content-identity prerequisite for the C052 physical-input case. Q01 overall remains `336B44_CONTENT_RECHECK_PENDING`; Q07/C052 still need per-hit internal-field/full-World and natural-keyboard coverage. The exact file hashes, source paths, PNG roles, line references, and dimensions are in `MANIFEST.json`.
