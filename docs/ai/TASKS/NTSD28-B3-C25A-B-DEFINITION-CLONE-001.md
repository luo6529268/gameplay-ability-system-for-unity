# Task Contract — NTSD28-B3-C25A-B-DEFINITION-CLONE-001

> 状态：`VERIFIED / C25A-B-PRODUCTION / TARGETED-PLAY-PASS / B11-DEFINITION-STATS-PENDING`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C25a-b`
> 依赖：`NTSD28-B3-C25-NESTED-TAIL-SKELETON-001 / VERIFIED`

## 目标

在C25动态slot事务开头精确实现当前Authority的C25a definition transition和C25b special-state clone，移除旧NTSD2.4/C# transform chain对生产路径的污染，并将五分身随机数切到2.8 synchronized stream/callsite。

## Authority合同

- C25a只处理state 8000..8999；不受object type限制；state9996及8M family留给相邻C25b。
- target OID=`state-8000`；target action读取旧definition当前frame的raw `next`，`next<999`原值，否则0；target DAT/action缺失时不修改实体。
- 成功后切换OID/definition/frame cache，action/action-latch/tick-action-snapshot写target action，frame counter清0；不执行旧state9995→50、4000→OID或render-pic-offset140链。
- C25b仅type0且frame counter=1；state9996依次请求217,217,217,217,218，最低free transient slot立即发布。
- 每个成功候选按x/y/vertical/z-motion/x-motion/action/facing顺序消费synchronized RNG；前四每个7次，第五6次，总34；callsite必须与Authority一致。
- newborn HP/effective/base HP=10、current MP映射PP=10、owner/spawner=-1、group0、attacker rest6；weapon HP仍来自target definition。

## 允许代码路径

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C25DefinitionCloneEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C25DefinitionClonePlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/BattleEcsLateTailNoOpEditorTests.cs`（仅生产不再调用旧virtual断言）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅新production C25a-b contract；旧direct compatibility fixture可保留）
- 本Task/Record、Ledger、STATE、handoff、总表与C25/B3 manifests。

## 不做

- 不实现C25c-p、resource/display/frame/timer/armor/opoint/fragment/lifecycle/healing。
- 不删除旧public `RunStateSpecialPreCollision` direct compatibility入口，但生产C25不得再调用它。
- 不改DAT/资源/Scene/Prefab/ProjectSettings、shared Server package或Authority目录。
- 不改变用户C15随机掉武器例外。

## 验收

- test-first证明当前生产仍错误执行state4000链、8000 action/frame-counter字段不符或五分身仍写legacy RNG。
- focused覆盖：任意type 8000 transition、missing target/action fail-closed、9995/4000 production no-op、9996 exact 34-site sequence、missing target零RNG、capacity零RNG、高/低slot birth。
- legacy deterministic RNG call count不因C25b变化；native synchronized state/observer精确匹配。
- compile0、focused/related/SelfCheck、真实Play通过；Scene unchanged、Play exited、Console0。

## 回滚

恢复late module调用旧`RunStateSpecialPreCollision`和`world.Rng`分身路径，删除新native方法与测试；不得回退C25 skeleton或C01-C24。

## 实施结果

- production C25a改用原子`TryApplyNativeC25DefinitionTransition`：仅state8000..8999，所有DAT type，旧frame `next<999`原值、否则action0；target definition或action缺失不写任何字段。
- 旧state9995/4000链、frame0强制跳转与render offset140只保留在direct compatibility入口，production不再调用旧virtual。
- production C25b按Authority 34个精确callsite消费`NativeRandom`，legacy RNG不推进；五个newborn的HP/HPBound/HP3/PP、owner/spawner/group、attacker rest及weapon HP已闭合。
- 低于游标的新生slot下一tick处理，高于游标的新生slot同tick处理；missing definition与无可用slot均在RNG前退出。
- 当前Unity内容模型尚无target definition `stats.max_mp/defend`及mode damage scale载体；C25a这部分明确留B11，不在本包伪造常量。8M encoded clone在Authority本身标为address-derived unresolved且locked DAT无生产者，Unity保持无生成/无RNG的可观察边界。

## 验证证据

- test-first红灯：job `cf790519cf834b27a03cac385b68bd7c`，6例中5例按预期失败（旧frame0、9995/4000身份切换、missing-action非原子、native RNG 0 call）。
- compile：Unity 2022.3.62f3 fresh refresh，Console compile error 0。
- focused：job `bee51c34dc724299aa7311a0147574f1`，11/11 PASS；相关late/C25/RNG job `9fb7d4d5c17f4e689414e04df6e1ec74`，30/30 PASS。
- broad：job `ac4c392619dc416e8a2173539e53c471`，NTSD28 399/399 PASS。
- SelfCheck：2026-09-05 07:13:17 +08:00 PASS；首次失败仅暴露direct compatibility spawner旧断言，生产/native与legacy direct字段随后明确分流。
- Play：artifact `Temp/NTSD28_B3_C25A_B_DefinitionClone.result.json` PASS，OID7/action3/type3、nativeCalls34、legacyCalls0、cloneCount5、birthDefaults=true；SHA-256 `15285774A6D85A36B908A3B9FDE76648FDE420747A4B1538389320A08C98C903`。
- 关闭：Play已退出，Console error 0；Scene SHA-256仍为`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`。
