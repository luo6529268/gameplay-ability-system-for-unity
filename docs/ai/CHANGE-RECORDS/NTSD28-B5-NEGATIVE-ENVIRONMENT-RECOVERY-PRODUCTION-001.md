# NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_SHARED_ATOMIC_PRODUCTION_TRANSACTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNegativeEnvironmentRecoveryWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NegativeEnvironmentRecoveryProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/FrameAdvanceRuntimeSnapshotEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan playable type0 pre-display negative EnvironmentState320 resource transaction, corrected post-accounting clamp; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-11-OF-12 / FOCUSED-12-OF-12 / RELATED-154-OF-154 / RELATED-B5-981-OF-981 / TARGETED-PLAY-12-CASES / BUILDS-0-ERROR / SELFCHECK-BLOCKED-UNRELATED / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / RED_11_OF_12 / FOCUSED_12_OF_12 / RELATED_154_OF_154 / RELATED_B5_981_OF_981 / TARGETED_PLAY_12_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SINGLE_EXACT_TRANSACTION / FLUTE_FALSE_POSITIVE_RETIRED`

## 计划

新增一个world-aware exact writer，legacy virtual与data-oriented recovery只负责在既有HP/PP顺序中调用。
writer按native phase、negative EnvironmentState、world rule、900/scale、decoded root + two owner hops完成
KO/HP/HPBound/+34C/+348及post-accounting clamp，且不写legacy stats或重置source carriers。

test-first证据、实际文件/符号、验证结果与未关闭项将在实施后追加。B6 producer、B8 event、schema/content/
Scene均排除。

## Test-first RED

旧生产代码上focused job `a649def072834bfe8347dd1e6354c5c9` 实际执行12项、11项失败：两profile
phase0/tick1不伤害、derived不伤害、raw rule fallback两项不伤害、slot-reuse lethal不伤害、scale-zero不写
+34C、flute两profile仍被误伤为64、shared writer文件缺失、warm transaction最终仍为100。唯一已通过项是
positive Environment / nonzero phase inert control。失败面与事前差异完全一致，无范围外失败。

## 实际修改

- 新增`BattleNegativeEnvironmentRecoveryWriter.IsEligible/Apply`，集中实现registered current-DAT type0、
  negative `EnvironmentState320`、native phase0、rule/default9、`900/scale`、CatchSource decode、最多两次
  current-owner hop、pre-mutation KO、victim/credit counters以及post-accounting clamp0。
- `LF2Entity.RunPreCollisionRecoveryPhase`保留既有HP→negative environment→PP顺序，但只调用共享writer；
  移除旧WeaponCount/tick/step-wait/FallDamageDiv/ComboVic transaction。
- `BattleEcsCharacterRecoveryPass.TryRunCharacterRecovery`的exact no-op gate纳入共享eligibility；
  `ApplyAuthorityRecovery`只在既有顺序点调用同一writer，因此非HP/PP周期tick也不会漏掉native phase事件。
- 更新`FrameAdvanceRuntimeSnapshotEditorTests`与`BattleRuntimeSelfCheck`中的旧WeaponCount recovery期望；新增
  focused Editor tests、request runner与真实Play runner。没有改变其他WeaponCount语义。

## 验证证据

- RED job `a649def072834bfe8347dd1e6354c5c9`：`11 failed / 1 passed`（12项实际执行）。
- focused GREEN job `f3878759aa014f0397ee550fdbb3f93c`：`12/12`。
- related recovery job `046d5223665e4176ab35171b67656849`：`154/154`。
- 73-class B5/HitPlan/FrameAdvance/LateTail job `89df898c7f4146299a8c857c781b2aa6`：`981/981`。
- runtime build：`0 error / 47 warnings`；Editor build：`0 error / 104 warnings`。
- `NTSD_Battle` targeted Play：`12 cases / Passed`，覆盖DataOriented、Legacy与derived；清理后Console
  `0` error，退出Play后Scene dirty=false。
- Scene前后SHA-256均为
  `32451F1A311476C036AC7D28514FDE34AE53E06B3CC28B7263835A65569AA928`，长度`214471`，mtime
  `2026-09-09T01:26:42.8284084Z`。
- full SelfCheck到达并停在独立既有CPoint throw Vz断言
  `BattleRuntimeSelfCheck.cs:10939`；这不是本包回归，故不将本包状态扩大为full SelfCheck pass。
- `Tools/Validate-ChangeLedger.ps1`在记录闭合后通过：`421 records / 352 files`。

## 未关闭边界与风险

- negative EnvironmentState producer、B8 knockout event/feed与联合schema仍由后继包负责。
- 顶层`NTSDBattleTickSystem`的step-wait分支会跳过整个LateEntityUpdate；本包只证明共享transaction本身不读取
  step-wait，不能据此宣称完整host step-wait/C25行为已对齐。
- full SelfCheck的CPoint throw Vz阻塞必须独立审计/修复，不能通过修改本transaction规避。

## 回滚

删除shared writer/test并恢复两个入口与旧fixtures；无数据迁移。回滚会恢复已证伪的WeaponCount false-positive。
