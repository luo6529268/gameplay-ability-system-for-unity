<!-- CHANGE-RECORD
id: NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-001
status: PLANNED
code-path: Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H10ResidentClosureDiagnostics.cs
authority: Direct user finish H10; verified discussion authorization and exact four-script handoff
evidence: docs/ai/TASKS/NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010.md
evidence: artifacts/diagnostics/NTSD-H10-RESIDENT-CLOSURE-20261010/execution-handoff.txt
-->
# NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-001

Human authorization: direct human discussion thread01a11fe2-bfaa-72f3-a9ed-c8260f63b193 turn01a1265b-425f-7b63-9a21-f2b5634cf553 "那就先把H10完成了啊"; exact delegated handoff retained below. Scope only four authored scripts, no assets/loader/cache/Q06. New finite task, no Goal.

Precise production symbols: NTSDSoundPlayer.PreparedSoundCue (generation/path identity/OwnedSource), owner fields and readonly diagnostics; PrepareBattleCuesAsync (serialized request invalidation/drain), GetOrPrepareCue/EnsurePreparedCueLoadedAsync/LoadPreparedClipsAsync/LoadAndPlayPreparedCueAsync (local result qualification), PrepareBattlePlaybackClips (only post-publication source retirement and failure ownership release; PCM math/helper unchanged), ReleaseBattlePlaybackClips/OnDestroy plus BeginBattleSoundShutdown/CompleteBattleSoundShutdown; PlaySfx/PlayPreparedCue (closed intake guard). SimulationTickDriver.EnterPreparingState captures exact owner; ShutdownBattleRuntime stage1 stop intake/stage5 release; SetSoundPresentationSinkForDiagnostics owner binding; CompleteBattleRuntimeShutdownAfterMapCleanup clears captured owner. AppManager.TryShutdownBattleRuntimeBeforeSceneDestroy no-driver branch releases existing soundPlayer. Test H10ResidentClosureDiagnostics adds bounded production cases only.

Ownership: each NTSDSoundPlayer owns its formal cue generation, serialized cold prepare state, pending formal load counter, and list of attached exclusive sources. Max one prepare/local formal load at a time; attached list grows only cold bounded by unique prewarmed cues, mono immediately removed after complete output publication. Existing playback list bounded by prewarmed clips. No new hot capacity growth/cache/pool. Request generation invalidates on shutdown/new prepare/controller switch; stage1 intake closed without waiting, stage5 releases all attached sources/playback after voice detachment and clears formal aliases; pending I/O remains local, is released statically if stale, next cold prepare waits old owner+pending drain. OnDestroy marks disposed and uses same idempotent stages. Borrowed global/config remain untouched. All counters owned per player and reset state only on new cold prepare.

Risk: late await resurrects old generation; nonmono ownership; fallback borrowed mistaken owned; voice retains destroyed clip; concurrent prepare overlaps; failed conversion publishes partial result. Minimum validation: compile; real production aliases/PCM/borrowed/failure/closed intake; three lifecycle cycles and cancel/destroy/controller/concurrent prepare; existing narrow ordered-shutdown/sound behavior. Only after correctness use one fixed four-WAV plus nonmono group vs isolated frozen baseline, no cost ABBA. One safely isolated minimal Play permitted for deferred Destroy if necessary. Red failures and source coverage boundaries retained. Cold allocations allowed, no Driver/whole-H10/device claim.
Rollback: exact task increments from true before/ intermediate preimages only after audited authorization, preserve user changes, no HEAD restore or autonomous rollback.

Pre-edit call-chain correction: BattleTestBootstrap EnsureAppManager can run after Driver.Awake; EnsureRuntimeProfileFromSources may return without EnterPreparingState. Therefore declared AppManager scope additionally includes EnsureRuntimeModules immediately after creating soundPlayer, calling new Driver.BindBattleSoundPreparationOwner (Preparing-only, reject replacing captured different owner). This proves both creation orders before audio await. Same four-script contract, no bootstrap/profile/tick changes.
