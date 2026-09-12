# NTSD28-B6-LEGACY-RELEASE-TICK-PRODUCER-RETIREMENT-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-LEGACY-RELEASE-TICK-PRODUCER-RETIREMENT-PRODUCTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyReleaseTickRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointDvxWeaponHpPreservationProductionEditorTests.cs
authority: NTSD28-B6-LEGACY-RELEASE-TICK-OWNER-AUDIT-001 VERIFIED; official NTSD2.8-Logan.exe SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; current playable source closure per source/README_SOURCE.md.
evidence: RED 9FAIL/4PASS; focused13/13; current Play4/4 only retired carrier differs; Temp/Goal20_R4_CurrentPlayComparison.json; Temp/Goal20_FinalSelfCheck.result; Temp/Goal20_SharedRegressionReconciliation.json; final evidence Temp/Goal20_FinalSummary.json.
-->

Goal20 R4 producer retirement包。Authority audit 已确认正式 NTSD 2.8-Logan release live path 没有 `ReleaseTick` 字段、writer 或 semantic reader；Unity 当前 tick stamp 只制造 checksum/parity/canonical-state 首差。因此本 Record 只删除两个获准生产文件中的实际动态赋值，保留 `ReleaseTick=-1` carrier、copy/reset、snapshot、ECS fingerprint、checksum/parity 结构和所有关系/动作/运动/RNG 语义。

Test-first 约束：先用 current DVX/kind3/consume witness 证明 legacy dynamic writer 可达并保存 `Temp/Goal20_R4_RED_*`；再新增 focused test 与 source guard；最后才改两个生产文件并重跑 focused。`stampReleaseTick` 在 `LF2WeaponHeldStateResolver`/`LF2WeaponBase` 的范围外兼容 plumbing 不在本包改动，删除允许文件内的赋值后不再具备写入效果。

本包禁止修改 `NTSDEntityRuntime`、schema/snapshot/checksum/parity/ECS fingerprint 结构、`NTSDSpec.cs`、`Gen/`、`Plugins/`、content、Prefab、Scene、`+0x2F8`、其他生产脚本，以及 Ledger/STATE/handoff/对齐总表；不执行 git add/commit/push。Unity refresh/compile/Play、共享回归、SelfCheck、双 build、validator、Console 和 Scene 验收由主代理串行执行。

当前状态：`IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED`。实际改动、验证命令、focused 结果、未验证项、风险和依赖在生产/测试完成后追加；若超出授权清单或出现无法由 Authority 解释的 current gameplay 变化，状态转 `BLOCKED` 并停止。

Main review: production-path fixtures now start at reserved default -1 (prior draft97 was insufficient for reserved acceptance); arbitrary23 is used only for fingerprint sensitivity. Current script fixtures are synthetic, not current DAT/Play evidence; current witness remains pending. Main agent takes over Assets test/production integration and Unity RED; worker only prepares current-data evidence under Temp.

Synthetic focused RED13: 9FAIL/4PASS, eight dynamic stamp failures (-1->0) plus sourceguard; carrier copy/fingerprint structure and no-stamp controls pass. Temp/Goal20_R4_REDResult.json. Added compiled current Play probe in same declared test file for current Gaara16/254, RockLee7/255, Sasori51/396,399 from RuntimeCharacterConfigs, production held setup/held pass, seed424242 per case, RED before production edit. No natural input/full skill claim.

Current Play RED4/4 reached legacy stamps: Gaara16/254->120, RockLee7/255->120, Sasori51/396,399->213 all -1->5 while relation released, actions/motion/RNG captured and objects4->4. Temp/Goal20_R4_CurrentPlay_RED.json. Only after this evidence, main deleted all3 actual ReleaseTick assignment sites plus their stamp-only if guards in the two declared production files. Compatibility bool parameters/signatures retained inert, no relation/action/RNG statement modified. GREEN/current comparison/shared pending. First mutation script matched no CRLF block and stopped without production changes; normalized match applied successfully next.

Pre-shared old-expectation inventory: user-authorized class-in SelfCheck revisions are needed for legacy ReleaseTick stamp values and HolderCopy release/stage/OID5152/OPoint propagation assertions. Exact path registered before edits; all relation/action/HP/RNG assertions preserved. Existing synthetic sentinel values remain to prove no behavior writes; natural new defaults remain99/-1. Backup Temp/Goal20_R45_Before_SelfCheck.cs.

Fresh focused 13/13 PASS; R3 actual Play and normalized-native comparison PASS, R4 current Play4/4 and before/after only ReleaseTick differs PASS, R5 new8 plus kind5/type3/G16 prerequisite149 PASS. Exact package evidence in Temp/Goal20_R4_*; batch shared/SelfCheck/build/final validator gates remain pending. Earlier partial/draft state statements are superseded by this measured progress.

Single batch shared job25f9da9f6b5443bc8566dbaad80ca72f completed1824 with20 reported failures, uncapped, all in old DVX HP fixture asserting ReleaseTick=current tick0 despite initializing99. This is exactly authorized legacy-tick expectation class. All motion/action/RNG assertions preceding that assertion passed. Register exact existing test path before changing only expectation to preserved pre-call carrier. Keep the original shared FAILED receipt; do not rerun broad shared batch. Re-run only the affected24-case class and reconcile reported failures. No production change from this correction.

RED 9FAIL/4PASS; focused13/13; current Play4/4 only retired carrier differs

Goal20 final scoped closure, 2026-09-12: VERIFIED for the authorized R1-R5 behavior retirement only. Full SelfCheck fresh PASS: Temp/Goal20_FinalSelfCheck.result (attempt4; earlier failures retained). Actual commands: dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly and dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly; both exit0/0errors, 47/104 warnings, Temp/Goal20_AcceptanceRuntimeBuild.txt and AcceptanceEditorBuild.txt. One shared1824 job completed with20 old ReleaseTick expectation failures; only authorized assertion rebaseline followed by affected24/24 PASS. Original broad FAILED receipt is retained, no second broad run or standalone all-green1824 claim; Temp/Goal20_SharedRegressionReconciliation.json. B6 coverage1735+37=1772, refill9 included.

Targeted runtime evidence: Temp/Goal20_FinalReservedPlayResult.json, Goal20_R3_PlayWitness.json, Goal20_R4_CurrentPlay_GREEN.json. Current OPoint seam, G16 pickup witnesses, weapon prepass and release pass are covered; no physical-key/full-skill or full native-world checksum parity claim. R3 native comparator covers two isolated hit_Fa pre-frame-advance calls, seed424242/empty input, identical normalized schema; tick1 checksum/tick2 motion differences disappear. No full tick/physics equivalence claim.

Disk Scene SHA remains D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Final editor scene isDirty=true/root13; source of dirty flag UNKNOWN, no save/clear performed, so scene-dirty-unchanged is NOT claimed. External UIPanels deletions/new images appeared during work and were not performed or modified by this batch; preserve them. Final Console snapshot: MinMaxAABB0/Overlap0; 7 expected fault-injection errors plus1 MCP disposed-connection error, warnings0; do not claim Console0errors. Temp/Goal20_FinalScene.json, FinalErrors.json, FinalWarnings.json, FinalScopeAudit.json.

Reserved contract: GrabbedBy0, TrackerFlag0/TrackerParentnull, WeaponState0, ReleaseTick-1. HolderCopy retains actual type/lifecycle defaults (runtime/Character/SpecialAttack99; Weapon/Other-1; task-1), not a new uniform default. Existing synthetic sentinels remain for no-write/fingerprint tests. Schema/snapshot/checksum/parity/ECS fingerprint structures, +2F8, NTSDSpec, Gen, Plugins and task content/Scene remain untouched; no staged files or git add/commit/push. Broader battle alignment and joint schema migration remain incomplete. Earlier progress statements are superseded by this closure; failures and correction history are retained. Final evidence index: Temp/Goal20_FinalSummary.json. Final validator receipt is appended after execution.

Final validator executed: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity; exit0 PASS,459 records/28 governed code files, Temp/Goal20_Validator.txt. Process-only Git config environment avoided unavailable user global ignore; no Git config files changed. Final git diff --check exit0. All changed scripts also explicitly covered by these five Goal20 Records.
