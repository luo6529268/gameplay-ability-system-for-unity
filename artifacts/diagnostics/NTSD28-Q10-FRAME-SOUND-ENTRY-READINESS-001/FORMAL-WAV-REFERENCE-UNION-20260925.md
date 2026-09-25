# Q10 formal WAV reference union (2026-09-25)

Status: `READ_ONLY_CONTENT_OWNER_CENSUS / NO_DEPLOYMENT / PLAYBACK_PENDING`.

Current formal `resources/runtime/vfs` contains 981 distinct WAV paths (33,571,573 bytes). The 976 rows of `formal-indexed-cue-paths.csv` collapse to 970 physical paths after slash and Windows case normalization. Unioning those 970 frame-declared paths with the 18 ordered paths in the byte-identical formal/staged `data/sound.dat` and the object-level weapon cue `data/0.wav` yields **974/981** formal WAV paths (33,244,409 bytes), with no path absent from the formal VFS. Only three sound-table paths are outside the frame set: `data/011.wav`, `data/065.wav`, and `data/085.wav`.

The remaining seven paths (327,164 bytes) are:

| Path | Formal reference and current Unity boundary |
| --- | --- |
| `data/037.wav` | Formal `w/l.dat` declares `weapon_drop_sound`; Unity `CollectBattleSoundIds` reads weapon fields. Old `Assets/NTSD/Sound` path exists but whole-file SHA differs from formal. |
| `data/040.wav` | Formal `w/6.dat` declares `weapon_broken_sound`; same Unity prewarm field path and old-file byte difference. |
| `data/079.wav` | Formal `w/5.dat` declares weapon hit/drop/broken sound; same Unity prewarm field path and old-file byte difference. |
| `data/m_join.wav` | Formal excluded `data/mode/ntsd.dat` mentions it, while project-owned `ProjectBattleModeConfig` currently configures it for stage-team-5 death audio. Old `Sound` file is whole-file SHA-identical to formal; event/timing and actual playback remain separate Q08/Q10 gates. |
| `data/m_cancel.wav`, `data/m_end.wav`, `data/m_pass.wav` | No literal declaration was found by the scoped search of formal decoded DAT, Unity NTSD scripts or playable C++ source. This negative text search does **not** prove these three are unreachable or nonbattle; their consumer/EXE-visible role is unresolved. Old `Sound` has `m_end` and `m_pass` with different bytes; `m_cancel` is absent. |

The first three are **battle weapon-level declarations**, so treating a frame-cue CSV or failed-prewarm warning set as the complete battle WAV closure would miss them. Their old Unity copies do not raise a 404 warning, but their bytes differ from the selected formal content. The Unity `CharacterAnimtorManager.CollectBattleSoundIds` source explicitly adds `weapon_hit_sound`, `weapon_drop_sound`, and `weapon_broken_sound` before frame sounds; `NTSDSoundPlayer.PrepareBattleCuesAsync` then attempts prewarm through the old `Sound` root. This also explains why a success from that root cannot certify formal WAV content parity.

Adding the three weapon-level paths and `m_join` to the 974-path reference union accounts for **978/981** formal WAV paths; the three `m_cancel`/`m_end`/`m_pass` paths remain unresolved. This is an ownership census, not a proposed copy set or evidence that 978 cues fire in battle. No audio, DAT, script, Scene, importer, or Menu behavior was changed.

2026-09-25 negative-search bound: the root formal EXE SHA-256 was rechecked as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. A literal ASCII/UTF-16 search of that binary found none of the `m_cancel.wav`, `m_end.wav`, `m_pass.wav`, `m_join.wav`, or `m_ok.wav` names. A scoped playable/core C++ `.cpp/.h` search found no first-three names either; `m_join` appears in tests and project config, while the formal excluded mode DAT names it. These negative searches cannot prove runtime non-use because paths may be data-driven or encoded. Keep all three unmatched paths unclassified pending a reachable formal behavior or a stronger source/consumer trace.
