# NTSD28-B6-WPOINT-KIND3-RELEASE-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-WPOINT-KIND3-RELEASE-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_WPOINT_KIND3_PRODUCTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointKind3ReleaseProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: 用户Goal11明确授权，NTSD2.8-Logan settle_held_refill_objects:8020-8077，owner correction及Goal10只读甄别；NativeRandom exact callsites与耗尽不追加tail。
evidence: RED completed92 failed capped25; finalfocused92/92 B6=195/195 refill=9/9 RNG-relations=41/41; fullSelfCheckPASS; bothbuilds0error; RockLee255 scopedPlayPASS tick12 draws0,5,1,0 final100,-1,-2; cleanup4to4 rosterRestored Console0 SceneSHAunchanged; validator439/378; Goal12USER_HOLD.
-->

[事前Task](../TASKS/NTSD28-B6-WPOINT-KIND3-RELEASE-PRODUCTION-001.md)定义精确范围、RNG流、outcome、矩阵、Play现场、关闭/回滚。
原状：real/generic Thrown提前return；DropRandomly使用旧LCG、无authored覆盖且Z*0.2；refill ForceDrop回到RunStep12仍可能进kind3。
计划只迁移kind3和其type2 prefix到NativeRandom；耗尽kick/其他ForceDrop legacy prefix保持。
Step0已读当前Rock Lee normalized projection：action255 Kind3 WeaponAct40 Dvx100 Dvy-1 Dvz0，standing hit_Da255；9项held-refill回归明确要求旧kick+1，不能改。
production/runtime用户已有改动按本轮SHA基线保护，不认领整个文件历史。

RED前新增focused：80个real/generic×type1/2/4/6×朝向×DV变体，4个slot0/399、2个damaged ForceDrop、2个nonkind3 type2旧RNG、4个refill组合，共92个用例。只新增测试，production仍未改。检查同步callsite/bounds/value/count、第二held pass不重复、历史slot/catch/owner/generation保持；不要求后置DVX weaponHP/+2F8行为改变。

新测试初次编译缺GenericHeld两个抽象方法Init/Reset，实现为空的测试stub后Editor build0 error/104 warnings（9.10s）。未运行旧程序集或将编译错误计作behavioral RED；production仍未改。

实际RED：Unity job 44e519abe9fe4752835e82849ed35c1b completed92 / failed，返回25条失败明细且failures_capped=true；明细为期望NativeRandom draw4/5实际0。不推算总失败数，完整返回Temp/Goal11_Kind3_RED_Result.json。
production已写：RunStep12只在RefillExhausted或非kind3 Thrown提前返回，real/generic DVX continuation保留；DropRandomly接收WPoint，按四个精确callsite无条件抽样后逐轴覆盖，X不翻向、Z整数；real/generic type2仅kind3 prefix改同步流。WeaponActResult新增瞬态RefillExhausted，仅耗尽设置；耗尽kick与damaged family保持。SelfCheck仅具名Vz断言改[-2,2]。outcome无World/queue/pool生命周期和持久化职责，不改变十一阶段关闭合同。编译/绿色/回归/Play待运行。

实际GREEN92/92，B6分类195/195，held-refill9/9，NativeRandom/relations/slot41/41全部通过；full SelfCheck实际PASS，结果保存Temp/Goal11_Kind3_SelfCheck.result。Editor首轮build0 error/129 warnings。新测试文件同文件新增scoped Play probe：生产current Lee ITRkind2 pickup→controller Defend/Down/Attack packet→真实driver tick，observer记录release瞬间motion、每tickdraw、owned cleanup后正常退出。probe初次compile仅两个FuncKeyMask.none大小写错，改为既有None；production无变更。Play/build最终复核待运行。

Play attempt1在current OID123地面帧选择失败：该DAT state3005没有WeaponOnGround1004，tick5未pickup/未抽样，owned cleanup4→4通过、退出Console0。这是新probe的资源前置选择错；保留attempt1 JSON，改用当前可地面拾取的OID120 kunai type1（0..5及40均存在），不改production、DAT或pickup规则。

Play attempt2 current OID120真实pickup accepted，115→0，tick5..22无release/无NativeRandom，cleanup4→4。只读确认SimulationWorld.HumanInputPollAll通过IsBoundActiveHumanRosterInputEntity过滤，新scoped holder未绑定roster故跳过。修probe：临时替换inactive roster row并保存原引用，以显式FrameInputSet经driver注入D/Down/A，finally恢复原row再unregister并记录rosterRestored；不修改Scene/InputAction或生产input。

Play attempt3已绑定roster2并恢复原引用，当前FrameInputSet.Defend实际进入210/211/212（jump），未到255。只读闭环：FreezeProducerState bank[4/5/6]=legacy KeyJump/KeyDefend/KeyAttack，而NativeComboStateMachine Attack/Jump/Defend=4/5/6；ProjectExactStateToLegacy也固定这一对应。probe按当前carrier投递native D/Down/A为FrameInputSet.Attack/Down/Jump，逐tick同时记semantic与raw carrier，不宣称物理L/Down/J或修复全局输入；scoped新角色同时调用现有battle-entry input clear/history initializer。无production更改，attempt3 cleanup4→4、roster原引用恢复。

Play attempt4 native D正确进入defend110但逐键未到255；current normalized Lee110/111的hit_Da为0、standing0为255，故该输入序列不满足witness前置。保留actual失败JSON，改为standing同tick native D+Down+A chord（production combo先于ground builtin，可读到standing255），双计raw packet与semantic，不改current DAT/组合算法。cleanup4→4与roster恢复通过。


## 最终验收（2026-09-10）
状态VERIFIED仅覆盖kind3 release精确子集。权威采用用户已确认的settle_held_refill_objects:8020-8077及四个同步callsite；不晋升全B6或full battle parity。

| 实际执行 | 结果 | 产物/命令 |
|---|---|---|
| test-first RED job44e519abe9fe4752835e82849ed35c1b | completed92，failed；返回25条失败且capped，不推算总失败数 | Temp/Goal11_Kind3_RED_Result.json |
| GREEN job0a7ccdc4e1ff4bf1b8992cdb4d917d27 | 92/92 PASS | Temp/Goal11_Kind3_GREEN_Result.json |
| 最后probe代码编译后focused jobe9d839ba84104bb3b3f0cda129512feb | 92/92 PASS | Temp/Goal11_Kind3_FinalFocused_Result.json |
| B6 category jobad5453426e26447685d29a5c14696c1a | 195/195 PASS | Temp/Goal11_Kind3_B6_Result.json |
| held-refill7 + C09 placement2 jobd9a1d30efe884256a4e867a28437bf49 | 9/9 PASS | Temp/Goal11_Kind3_Refill9_Result.json |
| NativeRandom四类 + QueryAndLink + lifecycle cleanup + slot seam job5373c55fc4ae4c779bd793de1180126b | 41/41 PASS | Temp/Goal11_Kind3_RngRelations_Result.json |
| full BattleRuntimeSelfCheck request | PASS（2026-09-09 15:57:39 UTC） | Temp/Goal11_Kind3_SelfCheck.result |
| Runtime build | 0 error / 22 warnings（最终增量） | dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly；Temp/Goal11_Kind3_RuntimeBuild.txt |
| Editor build | 0 error / 22 warnings（最终增量；新probe重新编译时104 warnings / 0 error） | dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly；Temp/Goal11_Kind3_EditorBuild.txt |
| Change Ledger validator | PASS / 439 records / 378 governed code files | Tools/Validate-ChangeLedger.ps1；Temp/Goal11_Kind3_Validator.txt |
| Scene SHA256 | D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11 | Get-FileHash Assets/NTSD/Scene/NTSD_Battle.unity -Algorithm SHA256 |

### scoped Play真实证据与前置条件
指定实例gameplay-ability-system-for-unity@b1b02287 / Unity2022.3.62f3 / NTSD_Battle。当前OID7 RockLee与OID120 kunai生产DAT，scoped逻辑实体slots50/51，临时绑定inactive playerSlot2。
通过当前Lee60的ITRkind2与LF2CharacterInteractionResolver.TryApplyPreInteraction建立真实生产held关系（ground-state检查及writer均执行），holder进入115；不是手填Link，也不是物理键盘/碰撞候选查找验收。测试实体不创建Renderer。
Tick6..10仍115、tick11回到0；均无同步抽样且links=1/-1。tick12站立同tick native Defend+Down+Attack chord经FrameInputSet和真实driver进入255；raw packet为当前legacy carrier Down|Attack|Jump。没有直接写action255或修改内容/input rules。当前110/111 hit_Da=0，不能将该通过扩大为任意逐键D/Down/A均能触发；前四次probe前置/接线失败JSON保留，不混为成功证据。

| tick12 callsite | bound | 返回原值 | release即时消费 |
|---|---|---|---|
| 0x00418726 | 6 | 0 | child action0 |
| 0x0041873A | 7 | 5 | rawX=2，被authored100覆盖；left facing仍+100 |
| 0x00418756 | 4 | 1 | rawY=-1，authored非零-1写入 |
| 0x00418772 | 5 | 0 | rawZ=-2，authored0不覆盖，无0.2缩放 |

四次draw时双方link均已0。release即时snapshot action0 / motion(100,-1,-2)，same tick最终child action1 / motion(100,-1,-2)；观察时点不同，不将末尾action1错误报告成释放选择1。整个tick仅四次同步draw，第二held pass没有追加；target/holder历史slot保持。完整每tick结果Temp/Goal11_Kind3_Play.result.json。
finally清理两scoped entities：ObjectCount4→4、ambientSlots空、owned world绑定均null、roster原引用恢复。随后普通Editor ExitPlaymode完成，manage_editor stop返回Already stopped；Console get error0，Scene dirtyfalse、root13，SHA不变。Temp/Goal11_Kind3_ExitConsoleScene.json。零残留结论限本probe所有对象/绑定；没有扩大为额外全局pool stress验收。

### 最终增量与边界
相对Goal11前置工作树指纹仅11条路径有增量：声明的4个既有script、1个新test及Unity生成meta、Task/Record、Ledger/STATE/对齐总表。生产最小diff另存Temp/Goal11_Kind3_Production.diff；既有无关未提交文件保持原状。没有提交/push、没有覆盖资源或Scene。
旧全局BattleRandInt、耗尽kick、damaged-drop prefix、non-kind3 type2保持；+0x2F8、terminal、missing-action、non-kind3 DVX weapon-HP、schema/恢复全部后置。Play scope/input carrier说明只是fixture接线事实，不能用来宣布全输入层对齐。Goal12（terminal structural）USER_HOLD，等待用户复核放行，不自动继续。
