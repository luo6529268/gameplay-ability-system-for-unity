# NTSD28-B6-LEGACY-WEAPON-STATE-BEHAVIOR-RETIREMENT-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-LEGACY-WEAPON-STATE-BEHAVIOR-RETIREMENT-PRODUCTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponFrameLogicResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyWeaponStateRetirementProductionEditorTests.cs
authority: NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001 VERIFIED; official NTSD2.8-Logan.exe SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; current playable source closure per source/README_SOURCE.md.
evidence: RED 3FAIL/2PASS; focused5/5; native normalized firstDifference/firstChecksumDifference/firstMotionDifference null; Temp/Goal20_R3_GREEN_Comparison.json; Temp/Goal20_FinalSelfCheck.result; Temp/Goal20_SharedRegressionReconciliation.json; final evidence Temp/Goal20_FinalSummary.json.
-->

Goal20 R3 行为退休包，依赖 `NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001 / VERIFIED`。Authority 的当前 frame state 来自 `definition->frame(frame.action)`；Unity 独立 `NTSDEntityRuntime.WeaponState` 只驱动旧的 1002→2000、2000 Vx halving→3000 prelude，且 held/drop/throw 路径有五处动态 writer。state1000 fast-to-40 若由当前 frame 的实际 state 触发则属于现有 frame 行为，必须保留并改为 `CurrentFrameState()` 读取。该 Record 只覆盖并行 state machine 的旧 production behavior/reader/writer 退休，保留 reserved carrier=0 及 canonical copy、snapshot、ECS fingerprint、checksum、parity 结构。

范围严格限定为三个生产脚本的 `WeaponState` 相关符号、新 focused test(+meta)、`Temp/Goal20_R3_*` witness/RED/green artifacts 与本 Task/Record。不得修改 `NTSDEntityRuntime`、schema、`NTSDSpec.cs`、ECS/checksum/parity/snapshot 结构、`Gen/`、`Plugins/`、content、Prefab、Scene、`+0x2F8` 或 relation 本体。不得 git add/commit/push。

Test-first 顺序：

1. 已完成 source/callgraph 只读 preflight：`ntsd28_core` exact `weapon_state` grep 为 0；Unity dynamic chain 与 allowed reserved/schema positions 已枚举。编辑器当前由主代理占用，未执行 refresh/test/Play。
2. 生产改动前以自建当前 playable-core C++ harness 固定 OID124 action40..55 actual state witness，并保存旧行为 tick1 checksum 首差/tick2 motion 首差 RED；不得复用 Goal18 impact output 作为本包裁决。
3. 新 focused test 先建立可达 fixture/source guard，再修改三个生产脚本；不使用 NUnit `Assert.Multiple`。
4. 生产改动后验证 actual frame state、carrier reserved zero、无额外 Vx halving、held relation/motion/RNG/action 不变，并重跑同窗口 Unity/C++ witness。

验收需记录：

- OID124 action40..55 每帧 `Frame.D.state == 1002`，hit_Fa=12 实际 current frame consumer 保持有效；
- reset、held follow、throw、drop、damaged-drop 与连续 frame logic 后 `Runtime.WeaponState == 0`，不再出现 1001/1002/2000/3000 carrier transition 或 Vx×0.5；
- tick1 checksum 与 tick2 motion 首差在退休后同 seed/input/tick 窗口为 `firstDifference=null`；canonical copy/fingerprint/checksum/parity/snapshot carrier 位置与 schema 不变；
- package focused、必要编译/Play/C++ witness 证据完成后，Record 才能从 `IN_PROGRESS` 推进。批末共享 regression、full SelfCheck、双 build、validator、Console/MinMaxAABB/Overlap、Scene SHA 由主代理统一执行。

已知外部依赖：`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 的旧 `FL-WEAPON-STATE` 迁移断言（约 23013–23041）不在本 Record 的明确生产脚本范围。若共享 SelfCheck 需要按退休语义类内修订，须由主代理确认并另行登记；本包不擅自修改该文件。

当前状态：`IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED`。当前实际代码、测试与 witness 尚未写入；fresh callgraph 结论与测试方案已报告给主代理，等待主代理登记后写 focused test。

Progress correction: focused test and actual C++ native-core harness ARE now written and executed (earlier planned-only paragraph is historical). Temp/Goal20_R3_AuthorityWitness.cpp/.exe/.json and RED_FocusedWitness.json show native vx14/14 versus legacy Unity14/7.699999988079071, carrier2000 both ticks. Native/Unity normalized hash comparison pending fresh same-seed enriched RED. One new carrier fixture baseline-order issue is being corrected; not an existing-test regression. Main has approved only FL-WEAPON-STATE old expectation updates in existing SelfCheck within user class-in retirement authorization; exact path added before edit. Production still unchanged.

Native provenance correction (2026-09-12): `powershell -NoProfile -ExecutionPolicy Bypass -File .\Temp\Goal20_R3_AuthorityBuild.ps1 2>&1 | Tee-Object -FilePath .\Temp\Goal20_R3_AuthorityBuild.log` completed with exit code 0. The build compiled `Temp/Goal20_R3_AuthorityWitness.cpp` against the current `ntsd28_core` source list and executed the resulting binary against `Assets/NTSD/Config/chars/weapon9.dat`; `Temp/Goal20_R3_AuthorityWitness.json` reports PASS with OID124/type4, slots 0→1, seed 424242, empty input, action40, actual state1002/hit_Fa12, Vx14 on both isolated pre-frame-advance ticks, and synchronized RNG calls 0→0. `python Temp/Goal20_R3_Compare.py RED` completed with exit code 0 and wrote `Temp/Goal20_R3_RED_Comparison.json`: the identical normalized FNV-1a-64 projection records tick1 `reservedWeaponState 2000 != 0` and tick2 `vxBits 4620355447696654336 != 4624070917402656768`; Unity full checksums remain diagnostic only. This native witness is a direct current-core hit_Fa subpass window and makes no full-tick/physics claim.

Main integration review: removed parallel resolver/GetRuntimeWeaponState, redirected only the existing frame-logic state read to GetResolvedWeaponStateForExternalUse (existing actual-frame helper), retired two prelude assignments/Vx-halving branch and all five held-state assignments, retained Reset0 and actual-state fast-to40/hitFa behavior. SelfCheck legacy carrier assertions now use actual frame states and reserved0; no other rule changes. An intermediate worker patch syntax error was observed in Unity compile (Temp/Goal20_R4_GREENCompileConsole.json); final files no longer contain patch markers and await fresh compile/GREEN. Main owns final code now.

Fresh Unity GREEN5/5 (Temp/Goal20_R3_GREENResult.json). Same-seed424242/empty-input two-subpass normalized comparison measured firstDifference=null, firstChecksumDifference=null, firstMotionDifference=null in Temp/Goal20_R3_GREEN_Comparison.json. RED tick1 FNV difference and tick2 vx7.699999988079071 vs14 eliminated; current16frame state1002/hitFa12 loop unchanged, carrier0 copy/snapshot/checksum sensitivity test passes. This is an isolated live-core pre-frame-advance window, not whole-world/full-physics checksum parity. Final compile/Play/shared gates pending.

Added RunCurrentPlayWitness in existing declared newR3 test file: current runtime OID124/action40 and OID7/action0 registered in actual paused NTSD_Battle world, two explicit pre-frame-advance calls, reserved0/state1002/hitFa12/Vx14 assertions and unregister cleanup. Called only by newR4 GREEN Play probe; this remains a subpass witness, not full physics. Evidence Temp/Goal20_R3_PlayWitness.json pending execution.

Fresh focused 5/5 PASS; R3 actual Play and normalized-native comparison PASS, R4 current Play4/4 and before/after only ReleaseTick differs PASS, R5 new8 plus kind5/type3/G16 prerequisite149 PASS. Exact package evidence in Temp/Goal20_R3_*; batch shared/SelfCheck/build/final validator gates remain pending. Earlier partial/draft state statements are superseded by this measured progress.

Full SelfCheck attempt1 reached another legacy WeaponState assertion in CheckWorldLevelRealWeaponStep12Contracts (expected1002 after throw). Same explicit user-authorized state-machine expectation class; updated its two1002 assertions and two residual mismatch/missing-definition1000 assertions to reserved0. All action/motion/spawner/picker/link assertions preserved. Arbitrary sentinel no-write tests unchanged. Failure retained Temp/Goal20_SelfCheck_Attempt1.result. No production change; fresh SelfCheck rerun pending.

RED 3FAIL/2PASS; focused5/5; native normalized firstDifference/firstChecksumDifference/firstMotionDifference null

Goal20 final scoped closure, 2026-09-12: VERIFIED for the authorized R1-R5 behavior retirement only. Full SelfCheck fresh PASS: Temp/Goal20_FinalSelfCheck.result (attempt4; earlier failures retained). Actual commands: dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly and dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly; both exit0/0errors, 47/104 warnings, Temp/Goal20_AcceptanceRuntimeBuild.txt and AcceptanceEditorBuild.txt. One shared1824 job completed with20 old ReleaseTick expectation failures; only authorized assertion rebaseline followed by affected24/24 PASS. Original broad FAILED receipt is retained, no second broad run or standalone all-green1824 claim; Temp/Goal20_SharedRegressionReconciliation.json. B6 coverage1735+37=1772, refill9 included.

Targeted runtime evidence: Temp/Goal20_FinalReservedPlayResult.json, Goal20_R3_PlayWitness.json, Goal20_R4_CurrentPlay_GREEN.json. Current OPoint seam, G16 pickup witnesses, weapon prepass and release pass are covered; no physical-key/full-skill or full native-world checksum parity claim. R3 native comparator covers two isolated hit_Fa pre-frame-advance calls, seed424242/empty input, identical normalized schema; tick1 checksum/tick2 motion differences disappear. No full tick/physics equivalence claim.

Disk Scene SHA remains D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Final editor scene isDirty=true/root13; source of dirty flag UNKNOWN, no save/clear performed, so scene-dirty-unchanged is NOT claimed. External UIPanels deletions/new images appeared during work and were not performed or modified by this batch; preserve them. Final Console snapshot: MinMaxAABB0/Overlap0; 7 expected fault-injection errors plus1 MCP disposed-connection error, warnings0; do not claim Console0errors. Temp/Goal20_FinalScene.json, FinalErrors.json, FinalWarnings.json, FinalScopeAudit.json.

Reserved contract: GrabbedBy0, TrackerFlag0/TrackerParentnull, WeaponState0, ReleaseTick-1. HolderCopy retains actual type/lifecycle defaults (runtime/Character/SpecialAttack99; Weapon/Other-1; task-1), not a new uniform default. Existing synthetic sentinels remain for no-write/fingerprint tests. Schema/snapshot/checksum/parity/ECS fingerprint structures, +2F8, NTSDSpec, Gen, Plugins and task content/Scene remain untouched; no staged files or git add/commit/push. Broader battle alignment and joint schema migration remain incomplete. Earlier progress statements are superseded by this closure; failures and correction history are retained. Final evidence index: Temp/Goal20_FinalSummary.json. Final validator receipt is appended after execution.

Final validator executed: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity; exit0 PASS,459 records/28 governed code files, Temp/Goal20_Validator.txt. Process-only Git config environment avoided unavailable user global ignore; no Git config files changed. Final git diff --check exit0. All changed scripts also explicitly covered by these five Goal20 Records.
