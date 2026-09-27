# Q10 formal battle-WAV deployment candidate (read-only)

Status: `EXACT_CANDIDATE_PREPARED / USER_AUDIO_SCOPE_CONFIRMATION_PENDING / NO_COPY / NO_CODE_CHANGE` on 2026-09-27. This belongs to the master alignment document's Q10/O-05; it does not alter the D-023 DAT and character-image scope or close Q10.

The formal root EXE was rehashed to SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The candidate is an exact normalized, Windows-case-folded union of the already audited formal frame cue CSV, the byte-identical formal/staged `decoded_dat/data/sound.dat` 18-entry ordered table, the object-level `data/0.wav`, three weapon-level `data/037.wav` / `data/040.wav` / `data/079.wav` declarations, and `data/m_join.wav` used by the project mode asset. All paths resolve to existing formal `resources/runtime/vfs` files.

| Measured item | Result |
| --- | ---: |
| Unique candidate WAV paths | 978 |
| Formal source missing | 0 |
| Already present under staged `Assets/NTSD/Content/LoganRuntime/vfs` | 0 |
| Formal source bytes | 33,300,049 (about 31.76 MiB) |
| Candidate paths currently present under old `Assets/NTSD/Sound` | 75; these are **not** a formal-content substitute |
| Formal WAVs deliberately outside the candidate | `data/m_cancel.wav`, `data/m_end.wav`, `data/m_pass.wav`; present in formal VFS but no scoped playable/decoded-DAT battle consumer yet confirmed |
| Formal WMA/BGM in this candidate | 0 |

The exact manifest is `FORMAL-BATTLE-WAV-DEPLOYMENT-CANDIDATE-20260927.csv`, SHA-256 `A335C40971C9885AF53856EEBB1EAE265662DB2344E7E968941BEB51B566FA25`. It contains each relative formal path, reference owner, source byte length/SHA-256, staged status/SHA-256, and old-Sound presence. The measured union covers 978 of the 981 formal WAV paths; it does not prove every candidate plays in the shipped battle, and its three exclusions remain unresolved rather than declared unused.

If approved as one scoped Q10 package, copy only those 978 exact bytes from the formal VFS into the matching staged `LoganRuntime/vfs` paths, **never overwrite** an existing file or `.meta`, and let the original Unity Editor import the additions. In the same declared production package, change only the battle `NTSDSoundPlayer` path selection and necessary `BattleContentSource` resolver/tests so a configured formal battle root reads those staged VFS files; an empty root retains the old path. Do not change Menu audio, UI, DAT values, Scene, excluded background/mode DAT, BGM/WMA or nonbattle framework. A WAV copy alone would be inert because the player currently builds paths under `Assets/NTSD/Sound`.

Validation plan: recheck the manifest against fresh source and target hashes before copy; prove 978/978 source-to-staged hashes afterward and no prior target/meta overwrite; run original-Editor import/0-error compile; use focused tests for formal-root and empty-root path resolution, escape rejection, and one selected formal WAV decode; run one selected natural Battle Play that reports the resolved source and real voice opening; keep Menu/Battle Scene hashes stable; run Change Ledger validator and diff check. A Windows Player sidecar/decode probe is a separate required exit before claiming deployment complete. This package would not close O-02 voice position, WMA/BGM, stop-all or Q10 overall.

Risk and recovery: about 978 new Unity audio imports can add Editor import time, disk and Player sidecar size. Since the current tree has zero candidate files at those staged paths, the proposed copy has no overwrite. The new-file list and source hashes are immutable in the manifest; any removal afterward still requires the project's explicit file-deletion authorization and must be reviewed against current user work. Production route changes must have their own pre-edit Task/Change Record and can be reverted through a later authorized patch without touching old `Sound` resources.
