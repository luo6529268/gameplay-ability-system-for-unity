# Task Contract — NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001

> 状态：`VERIFIED / B0_OWNER_PRODUCER_PREREQUISITE_CLOSED / OWNER_GUARD_TRACE_EQUAL_8_RECORDS_48_FIELDS / UNITY_FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / DOUBLE_RUN_BYTE_STABLE / BUILDS_0_ERROR / EXACT_OWNER_SLOT_BINDING / LEGACY_KILLCOUNT_DETACHED_FROM_AI / SELFCHECK_BLOCKED_UNRELATED`
> 来源：`NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001`

## 目标

把Unity native-AI OID122/OID123 guard从legacy `KillCount`改绑到Authority `owner_slot`对应的
`NTSDEntityRuntime.OwnerSlotIndex`，覆盖production SoA、unified、legacy fallback与诊断比较路径。

## Authority与原状

- Authority `native_ai.cpp`在same-team summary之后、PP/input-phase/slot guards之前执行
  `if (subject->owner_slot >= 0) avoid_122 = true`，随后`avoid_123`继承同一条件；不读取`+0x2F4`。
- Unity `AiSensingKernel`和legacy decision读`KillCount > -1`；SoA/unified rows也从relation-link store的
  `KillCount`采样并订阅其mutation。这会让type0 OPoint的credit gate、non-type0旧写入和真实owner脱钩。
- `OwnerSlotIndex`在当前input pass中无writer；它只在spawn/initialization及后续hit relation阶段变化。
  本包在AI snapshot capture边界直接采样它，不为不存在的mid-input mutation新增通知系统。

## 修改范围

- `Assets/NTSD/Scripts/Simulation/Ai/Snapshots/AiSensingSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiSensingKernel.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionTypes.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiSensingModule.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleAiUnifiedRowPublisher.cs`
- `Assets/NTSD/Scripts/Test/Editor/AiSensingKernelEditorTests.cs`
- existing AI test fixtures that initialize the renamed row
- `Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs`（publisher direct-call fixture）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B2AiOwnerSlotRuntimeAcceptanceEditorTests.cs`
  - production snapshot/SoA实际采样owner；交叉验证KillCount不驱动OID122/123 guard；提供focused与Play请求入口。
- `Tools/NTSD28AuthorityTrace/ai_owner_guard_capture_main.cpp`
  - 链接当前playable source closure，输出HP/PP与其他guard均隔离后的8-row owner/KillCount-marker专项trace。

不改`NTSDEntityRuntime.KillCount`、`BattleRelationLinkStore`或其他damage/revival/lifecycle reader；它们由后继包处理。

## 实现约束

- row字段与mismatch enum改名为`OwnerSlot`，所有production capture写`runtime.OwnerSlotIndex`。
- unified row publisher不再接收或响应`RuntimeRelationLinkField.KillCount`；其他pending bit与发布顺序不变。
- legacy fallback直接读`self.Runtime.OwnerSlotIndex`。
- 不改变candidate扫描、tie、distance、RNG调用、input phase、PP/team/slot guards或fallback策略。

## 验收

- focused pure test以同一subject交叉设置owner=-1/非负，证明OID122和OID123 guard只随OwnerSlot变化。
- 所有AI fixture从`KillCount` row迁移到`OwnerSlot`；production AI目录不再含`KillCount`引用。
- isolated compile、focused NUnit（若现有Editor可用）、SelfCheck；真实AI OID122/123 Play及joint trace仍单独记录。

## 实际实施与验证（2026-09-08）

- AI snapshot row已改名为`OwnerSlot`；SoA、unified refresh、legacy capture和fallback均读
  `OwnerSlotIndex`。publisher已删除KillCount参数、pending lane和mutation case；relation store本身未改。
- 新增`Special_OwnerSlotAloneGuardsOid122And123`，交叉覆盖owner -1、slot0与high slot 37。
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`：0 error，47 warning。
- 首次Editor build准确暴露`CharacterInputLiveSlotLoopEditorTests`仍传旧publisher参数：1 error；同步fixture后
  `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo --verbosity:minimal`：0 error，104 warning。
- 隔离纯核harness实际执行：`AI_OWNER_SLOT_FOCUSED_PASS`。
- 当前Editor消费完整SelfCheck请求，但在既有CPoint throw检查失败：
  `CheckCpointThrowRawAndTransformMatrix`要求mode0 victim Vz预先归零。本包未改该路径；完整SelfCheck、
  Unity NUnit、OID122/123 Play及joint trace仍未通过，所以状态保持`RUNTIME_PENDING`。
- 后续producer复核发现formal direct/stage、ordinary OPoint与F8尚未写出Authority owner值；详见
  `NTSD28-B0-OWNER-SLOT-PRODUCTION-OWNER-AUDIT-001`。因此本包虽已完成consumer代码，仍额外依赖B0
  target deconfliction与producer闭合，不能凭synthetic owner sentinel test宣称正式AI行为已修复。
- 2026-09-09 `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001`已以15-record/135-field双端trace、
  targeted Play及route1～4 32/32关闭此前producer prerequisite；本包现恢复production runtime验收。
- 当前source-model runner链接当前Authority source-capture子闭包并输出8条OID122/123 guard记录：
  source manifest=`07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F`，
  runner source=`094EDE9DF1C9CEF8101EB8EE3246A4E03EF025769202E15FCA6C03BDFC16EDCD`，
  binary=`088219249FE43454AADD8F94A8EF2321477D9864281B4D1F2F37497C41EA8349`。
  该runner是`SOURCE_MODEL_DIAGNOSTIC_ONLY`，不冒充正式EXE runtime capture。
- Authority JSON双跑SHA-256均为
  `FCFEA3DBE5FA983E27E00240394235705FE512A6D46515C1C4251ADEFDCE497A`；Unity production
  snapshot/SoA输出在focused与Play后均为
  `63ED4B648DE04AD1DA808C8EDEA2213BFA8747941C3CE74168852D23E387149B`。
- 2026-09-09 01:39:06 +08 Unity EditMode focused实际`4/4`通过；比较结果为
  `equal-ai-owner-guard / 8 records / 48 field occurrences / firstDifference=""`。矩阵覆盖OID122/123、
  owner `-1/0/37`、legacy KillCount marker `-1/999`，并消费B0 producer的direct/OPoint/F8 owner
  `0/7/99`。
- 2026-09-09 01:37:20 +08真实`NTSD_Battle` Play内直接执行同4个验收用例并通过；退出后Console error=0，
  active Scene为`NTSD_Battle`、dirty=false、root=13，Scene SHA-256保持
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- fresh `Assembly-CSharp.csproj`与`Assembly-CSharp-Editor.csproj`构建均为0 error。01:40:28 +08 full
  SelfCheck仍在本包之后的既有`CheckCpointThrowRawAndTransformMatrix` mode0 victim Vz断言停止；这是独立
  B6 CPoint阻塞，不降低本包专项验收，也不被隐藏为full SelfCheck通过。
- 本包只关闭AI owner/KillCount guard binding family；不声明完整AI、完整B2或全战斗parity。严格下一包为
  `NTSD28-B3-LEGACY-STATE501-OWNED-CHILD-TRANSFORM-RETIREMENT-AUDIT-001`。

## 回滚

恢复上述AI row命名/采样与publisher lane及对应tests；不触碰legacy carrier其他使用者、content、Scene、Prefab或Authority。
