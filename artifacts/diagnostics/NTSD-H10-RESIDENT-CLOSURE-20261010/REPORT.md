# H10 resident ownership and cue scope review

Status: DIAGNOSTIC_VERIFIED / H10_CONTINUATION_IN_PROGRESS. The user asked to finish current-stage H10 before another optimization. This report does not close H10 or device gates.

## Actual evidence
- New imported Editor diagnostics: actual job 8b662ac5add6460fa203666cfb0b323b, 2/2 passed. Earlier job 4ce9019467ee4b1caf2e911f3d05e1e5 had total 0 and is NOT a pass. Explicit ImportAsset command exceeded Windows command-line limit; successful force-all refresh then real tests supersede discovery only.
- Current production CollectBattleSoundIds/normalization inspected with actual formal catalog: 330 definitions, 1000 cue IDs, 978 unique existing formal file paths, 72 existing generic paths. Built-ins and configured mode sounds retained.
- WAV header inventory: 965 mono formal files. Float source payload 116169868 B and all-channel final playback payload 226264216 B are estimates, not runtime object sizes or process peak. Previous 220188696 B describes mono-created stereo outputs only, excluding 13 non-mono borrowed outputs; no contradiction or fabricated measured saving.
- Actual existing uncached loader plus actual production PCM converter, four formal WAV files over three cycles (12 rows): all sample bits equal, source retired and output released, isolated cache empty, existing global cache unchanged. Source object bytes per cycle total 848237 B, retained playback object bytes 6706088 B. These are separate object counters, not full session/native/RSS peak. Each source was retired only after a valid playback copy existed.
- Prototype retirement was performed by Editor test; production formal loads still use global cache at this review point. Do not claim the resident fix is already applied.
- Current visual dependency closure is NOT an admitted audio dependency closure. It omits stage/reserve, random weapons and runtime producers. Full existing cue collection remains the conservative correctness fallback; no player-only filtering or hot-path lazy decoding/conversion is authorized by this finding.
- 87 protected files unchanged; existing Menu scene clean, 8 roots, Editor idle, no Play. No global cache clearing, forced GC, all-library decode or user resource deletion.

## Next implementation obligations
1. Formal file loads may use existing empty-cache-key loader route to establish explicit per-generation ownership. Generic files and configured asset fallbacks remain borrowed and unchanged.
2. Retire only newly-owned mono source after successful exact conversion; preserve non-mono owned output until session retirement. Failure, stale load and same-controller re-entry must not attach dead or prior-generation objects.
3. Stop and detach battle voices before destroying owned clips; close preparation/event intake at existing shutdown start, release at existing presentation stage. Do not change eleven-stage order or create global cache/lease manager.
4. Prove cancel/late-result and three-cycle retirement, exact PCM/events/gain/retrigger and unchanged borrowed resources. Measure cold loading stages and object ownership without claiming RSS or device budget.
5. Audio dependency subset and approved whole-session/device budget remain separately explicit H10 obligations; do not relabel this review or first production repair as all-H10 complete.

## Retained limits
No production change by this review. No actual audible waveform/device validation, full-session peak, or optimized producer closure certification. The incidental read of a file carrying a Q06 alignment marker is not used to authorize Q06 changes; further Q06 body inspection is excluded. Older bounded PCM cost spike is retained as timing uncertainty on the old path; no rerun or guessed cause.

Validation: Tools/Validate-ChangeLedger.ps1 exit 0, PASSED 1419 records. Scoped git diff --check exit 0. Raw validator warnings retained; no global cleanup. Exact reviewed source/assembly hashes saved in review-fingerprints.json.

## Dispatch and remaining H10 work
Exact four-script production ownership handoff sent to existing execution chat 01a1201b-7930-7c93-8c84-5bc7baada43f; turn01a1266b-e278-73c0-9812-7de94d7933ae active, latest cursor154. Monitoring resumed. This review remains the parent H10 continuation, not a declaration that all H10 is complete. Conservative closure candidate JSON is an unadmitted estimate; no filtering has been authorized. Source retirement/lifecycle implementation proceeds first; producer closure and measured loading/steady budget remain on this same H10 worklist.
