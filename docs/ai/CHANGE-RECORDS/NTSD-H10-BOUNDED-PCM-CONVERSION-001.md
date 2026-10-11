<!-- CHANGE-RECORD
id: NTSD-H10-BOUNDED-PCM-CONVERSION-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H10AudioMemoryDiagnostics.cs
authority: Direct user H10 review and accurate implementation delegation; turn01a12638-f06e-72c3-a1b8-79f0d1799fff
evidence: docs/ai/TASKS/NTSD-H10-BOUNDED-PCM-CONVERSION-20261010.md
evidence: artifacts/diagnostics/NTSD-H10-BOUNDED-PCM-CONVERSION-20261010/REPORT.md
-->
# NTSD-H10-BOUNDED-PCM-CONVERSION-001

Human authorization verified: discussion thread01a11fe2-bfaa-72f3-a9ed-c8260f63b193 turn01a12638-f06e-72c3-a1b8-79f0d1799fff direct user "可以，开始吧", authorizing independent H10 investigation then accurate implementation delegation to this thread. Exact execution-handoff.txt and received instruction limit this task.

Only two script write scopes:
- Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs PrepareBattlePlaybackClips mono branch plus private bounded PCM helper/local const.
- Assets/NTSD/Scripts/Test/Editor/H10AudioMemoryDiagnostics.cs production-vs-frozen-original PCM/boundary/owner/failure and once-only cost evidence.
No loader/AppManager/CharacterAnimtorManager/Q06/other tests/Scene/assets/settings/Server/Gen/Plugins edits.

Preserve short mono <8192 exact whole-array path; >=8192 uses local4096/8192float buffers and exact tail remaining/2remaining. First GetData before target creation, original checked(totalSamples*2) preserved after first successfulread; create same name/samples/2channels/frequency/nonstream; add to original owner before any SetData; full cue publication once at end, unchanged firstOwned batch catch destroys only this cue new copies. Existing entry earlyreturn/borrowed nonmono/null/voice/gain/Release/generation/OnDestroy unchanged. No new runtime state/cache/pool/flag/manager. Buffers are local to one clip conversion, at most4 arrays, data payload<98304B; naturally invalidated at return/unwind, no shutdown registration. Native target remains existing ownedBattlePlaybackClips owner; original shutdown/OnDestroy Release unchanged.

Minimal validation: safe originalEditor idle cleanMenu singleinstance, compile, actual production vs frozen prechange Prepare wrapper/algorithm for nine specified mono boundaries plus16384 exactmultiple/stereo, four existing realWAVs; samples/channels/frequency/name/bitwiseL-R/source unchanged/final object memory. Actual streaming/non-readable GetData qualification then first-read and later-clip failure batch rollback/no partialpublication/earliercue retained; null cue/notloaded/ready idempotent/null/stereo borrowing/mixed order/destroyed owner guard. Cannot inject production failures; later-block/SetData failures untriggerable under valid Unity clips must be precisely source-proved and explicitly reported, not claimed executed. Existing OriginalCommonAudio gain/retrigger/destroy tests unchanged. All own clips explicitly tracked; only own cleanup, borrowed remain until their source-owner cleanup; explicit Release is not natural callback evidence.

Only after hard correctness/owner gates pass: unique production-vs-frozen original two-size65537/329244, orderoriginal/new/new/original each4warm16sample. Same44100/content. Delegate/reflection/input/assert/readback/cleanup outside Stopwatch; actual conversion/AudioClip create inside. Save create-new marker before run; no retries/repeatwindows/JITextra/profiling search. Per-size each newmean <= max(two original means), spikes report; 16sample P95/P99=max only observed. Exact float capacities/count/payload analytic from actual source, Profiler finalclip measured; no hotpath0GC requirement/certificate and no RSS/peak/wholelibrary saving claim.

Risk: tail wrap corrupts beginning; ownership added too late; empty/null semantics; changed error/overflow; bench mirror substitutes actual production; unrelated dirty overwritten. Preserve full old method text frozen Editor control and trace SHA, same wrapper/owner cleanup for fair comparison. Every patch exact preimage before edit; failedraw retained. Any real correctness/resource gate failure stops cost; any cost failure ends this version, report; no expansion/new fault-injection/routes. Rollback would be exact task delta against current preimage after operation audit and applicable authorization, neverHEAD/reset/checkout. No autonomous rollback of unrelated work.

Success SCOPED_PCM_TEMPORARY_MEMORY_REDUCTION_VERIFIED, applied to existing production conversion with noflag. H10 parent resident copies/rosterclosure/loadpeak/device remainOPEN; H08defaultON,M03OFF,H07/producerhold,95OFF,H06notcontinued preserved. NoGoal/34queue/nexttask.


Actual implementation written: long mono branch delegates to CreateBoundedBattlePlaybackClip only; original short conversion and original cue catch/release code retained byte-normalized exact. Test frozen original method copied from true preimage with only Editor owner/type/Destroy qualification adaptations (frozen-original-method.txt). Added production PCM/guards/stream failure/batch rollback and once-only cost tests; old diagnostic outputs unchanged. Compile and tests pending.


Production compile/live reload passed; qualification job b32bf761e6f6429aa9b5a8eb1b4ba078 1/1, focused job459e3e8d2d954cafb0634cd70506f335 10/10. Nine required boundaries plus16384/stereo and4WAV bits/metadata/objectmemory equal,17owner audits31records released. Real first/later-clip GetData failure rollback passed; intra-clip later-read/write/huge overflow only source-proof coverage, exact failure-coverage.md. Cost notyetstarted; code/assembly SHA stable.


> **2026-10-10 H10有界PCM转换：CORRECTNESS_PASS_COST_SPIKE_REVIEW_REQUIRED / FINITE_TASK_STOPPED。** Task NTSD-H10-BOUNDED-PCM-CONVERSION-20261010 / Change NTSD-H10-BOUNDED-PCM-CONVERSION-001 BLOCKED。两脚本生产分块已写入并接原调用、无新flag/未回滚；真实失败资格1/1、正确性及原声音规则10/10、唯一成本1/1程序断言PASS，临时float payload长源<98304B、最终clip/逐样本不变。但大尺寸首个旧算法窗有46.8822ms尖峰，按“异常尖峰明显则交回”条款停止，不重测/归因/称准入成功。新均值1.5964/1.5734ms低于另一个旧均值2.1918ms，此事实保留。19owner/191资源记录释放，Menu clean8。报告 artifacts/diagnostics/NTSD-H10-BOUNDED-PCM-CONVERSION-20261010/REPORT.md；下一仅审阅尖峰与接入状态，不自动新实验/后继；H10/H07/Android仍OPEN、H08默认ON保持。

Compile/live reload pass; qualification1/1, focused10/10, once-only productioncost1/1 automated PASS. Oldlargefirstwindow46.8822ms triggers qualitative explicit contract handback. No rerun, newJIT/warm/Profiler or rollback. 15818protections/10backups/codeassembly stable; final validator/closure in report. New buffers local percall, no runtime cache or changed lifecycle owner.


2026-10-10 independent discussion review after human request to finish H10: SCOPED_PCM_TEMPORARY_MEMORY_REDUCTION_VERIFIED. Actual production and precise temporary-array objective accepted. Old-only 46.8822ms spike retained, not dropped or explained; both new means below both old means independently, so limited no-observed-cost-increase evidence does not require relying on inflated old window. This resolves handback for this subchange only; no stable-tail/full-load/whole-H10 certification, no new measurement. All prior BLOCKED notes are historical.
