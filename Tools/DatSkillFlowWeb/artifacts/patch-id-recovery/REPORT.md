# NTSD patch-package ID recovery audit

## Scope and safety

- Source library: `J:\QQFile\NTSD2.4大量人物补丁（2）`
- Authoritative base registry used for cross-checking: `J:\QQFile\NTSD 2.4.1\data\data.txt`
- The source library was read only. No file under `J:` was created, edited, renamed, or deleted.
- Recovered/corrected manifests are editor-local artifacts under `supplemental/`.
- No DatSkillFlowWeb runtime, API, client, or Native source file was modified during this audit.

## Discovery result

The scan searched TXT contents instead of requiring an exact `ID.txt` filename. This matters because valid packages use names such as `第四尾ID.txt`, `id档.txt`, `新建 文本文档.txt`, and `新建文本2文档.txt`.

- TXT files scanned: 224
- Files containing registration records: 195
- Registration records recognized (including `tupe`/`tpye` diagnostics): 638
- Type-0 records: 272
- Type-3 records: 357
- Other-type records: 9
- DAT files observed in the patch library: 772
- Structural package candidates examined in detail: 17
- Editor-local supplemental/corrected manifests produced: 9
- Supplemental entries whose DAT basename resolves exactly once in its source package: 31/31

The number of DAT files not directly named by a nearby registration line is not the number of missing packages. Many are dependency copies, replacement files, backups, nested duplicate folders, or objects already registered by an ancestor manifest.

## Generated manifests

| Package | Artifact | Status | Evidence |
|---|---|---|---|
| `鸣人系列-11\仙人鸣人—完全修炼2` | `ID.editor-recovered.txt` | recovered | Four type-0 OIDs are inherited from the sibling package. `Nf1` retains all 60 old `imm_Na_Kyubi` frames and adds 24; `Nf2` retains all 49 old `Kyubi_Na_imm` frames unchanged and adds 8. Therefore OIDs 466/464 are preserved. |
| `自来也仙人\自来也77` | `ID.editor-corrected.txt` | corrected | Source OID 81 accidentally repeats `immNaru.dat`; sibling manifests identify `immNaru_c.dat` as OID 81. |
| `绝\新绝\绝` | `ID.editor-corrected.txt` | corrected | Source manifest declares missing `zetsu.dat`; the package's actual type-0 Zetsu DAT is `jie.dat`, retaining source OID 26. |
| `鼬系列-5\R-鼬1` | `ID.editor-corrected.txt` | syntax corrected, collision retained | Corrects `tupe` to `type`. The authored duplicate OID 259 remains unresolved and is not silently rewritten. |
| `鼬系列-5\秽土鼬1\秽土鼬1` | `ID.editor-corrected.txt` | corrected/completed | Corrects `tupe`; adds `amaterasudefend.dat` as OID 333 because the matching R-Itachi registry has that mapping and the Z-Itachi main DAT references OID 333. |
| `鼬系列-5\鼬+须佐` | `ID.editor-completed.txt` | completed | Existing OID 499 is retained; NTSD 2.4.1 base registry plus DAT metadata identify Itachi/Kisame as type-0 OIDs 9/17. |
| `佩恩系列-4\晓—新佩恩` | `ID.editor-recovered.txt` | recovered replacement | `pein2.dat` is a full type-0 Pein DAT and internally references source OID 50; the NTSD 2.4.1 Pein registry also uses OID 50. |
| `迪达拉\移土迪达拉` | `ID.editor-partial.txt` | root recovered, auxiliaries unresolved | Main DAT is a type-0 Deidara replacement and internally references source OID 10. Three `deidaraballs*.dat` files have no provable original registration. |
| `其他忍者系列-7\通草野饵人` | `ID.editor-recovered.txt` | synthetic package-local identity | No registration was found. The full type-0 DAT uses package-local source OID 0 for Native preview; its globally unique editor identity is `(packageId, sourceOid)`. This is not claimed to be its historical Native OID. |

## Candidates that do not actually lack an ID registry

- `鸣人系列-11\鸣人6尾\鸣人4` uses `第四尾ID.txt`; its three DAT basenames match the entries. It was never a missing-ID package.
- `土影师傅` registers `data\wu.dat`. The many DATs beside it are a bundled data/dependency dump, not dozens of unregistered characters belonging to the package.
- `蛇兜\Kabutomaru` is registered by the parent `蛇兜档.txt`.
- `赤砂之蝎人傀儡2k\≯Sasori` is registered by the parent `id.txt`; `OLD.dat` is an extra/backup candidate, not a second proven character entry.
- `NTSD II人物包\迪达拉\deidara2` is registered by the parent `安装说明及id.txt`.
- `xsasuke` uses `新建 文本文档.txt` and contains six valid records.
- `小时候的佐助` uses `新建文本2文档.txt` and contains four valid records; the nested `cs1sasuke` folder is a duplicate layout.
- `自来也仙人` already registers `x-jiraiya.dat` and `hamoball.dat`; `template.dat` is not evidence of another package character.

## Remaining unresolved source problems

These are deliberately not disguised with invented OIDs or paths:

1. `R-鼬1` assigns OID 259 to both `amaterasu_katon.dat` and `R-itachi-katon_big.dat`. The main DAT references 259, but available evidence does not prove which file should win or what a replacement OID would be.
2. `移土迪达拉` contains `deidaraballs.dat`, `deidaraballs2.dat`, and `deidaraballs3.dat` without an ID registry. Their original OIDs cannot be derived reliably from filename, frame IDs, or the main DAT's opoint actions.
3. `新绝\绝\jieX0.dat` is a 26-frame non-character DAT, but neither a registry entry nor an opoint reference identifies its OID.
4. Existing manifests elsewhere refer to files absent from their package trees, including `water_2.dat`, `davis_ball7.dat`, `NarutoLS_2.dat`, `NarutoLS_3.dat`, and `tonton.dat`. These are missing dependencies, not missing-ID packages.
5. `minato尸鬼版\readme.txt` contains several registration records merged onto one malformed/conflicting line, including duplicate character OIDs. It requires package-specific review rather than automatic repair.

## Important OID conclusion for the editor

An OID is not always a freely replaceable selector ID. DAT `opoint` records and some Native rules refer to source OIDs. For example, the recovered Pein and Deidara roots reference OIDs 50 and 10 respectively. Replacing those IDs globally without translating references can spawn the wrong object or break a skill chain.

To load all alternative patch packages simultaneously, the runtime should later separate:

- source OID: the Native/DAT identity used inside one package;
- package identity: the package/version that owns the DAT;
- effective editor entity ID: a collision-free key used by the UI and API;
- resolved opoint target: `(package scope, source OID)` with explicit fallback rules.

That runtime design has not been implemented yet. This audit stops before project-code changes as requested.

## Validation performed

- Every generated registration line was parsed with the normal `id/type/file` shape.
- All 31 generated entries resolve to exactly one DAT basename beneath the corresponding source package.
- The only duplicate OID in generated files is the explicitly preserved and reported R-Itachi OID 259 collision.
- The J: source tree remains unchanged.
