<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C045-RECOVER-COVER-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldInjuryAccountingCoverProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs
authority: selected 336B44 playable catch settlement and formal OID65/OID2 natural source-root LFR witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C045-RECOVER-COVER-001.md
-->

# C045 抓取伤害停顿 recover/cover 分读

脚本前创建。当前正式OID65自然tick18/23伤害100后的抓取者hold2与受害者-3已由源码和根同LFR声明字段核实；当前Unity `ApplyHeldInjury` 用cover1跳过抓取者hold，字段合约已有Recover而旧夹具把cover误作停顿。只更新声明的战斗测试与共用writer、必要时既有Play探针；预期副作用是抓取正伤害后停顿按recover控制，cover仍只控制层级/朝向。先测试RED，后最小修复，聚焦/Play及审计；自然Unity同初态整链需独立出口，Q07不因本包关闭。回滚仅审阅并精确区分本包与同文件C042/C044差量。状态PLANNED。

2026-10-01 测试先行：只改声明的HeldInjury Editor测试，将旧 cover→停顿矩阵改为recover主控/cover独立的九例（含正式cover1/recover0），保留资源/伤害/定位其它断言；CreatePair只增可选recover并传入正式CatchPoint数据合同。BattleCatchPointValueAdapter现有映射已核，无需增字段。原Editor Refresh已请求，RED尚待，生产writer未改。

2026-10-01 原Editor新编译后job f8624bcb8a1c4323b21204ce55297546 的HeldInjury相关40例中5例RED、其余35例PASS；正式cover1/recover0的首差抓取者hold期望2、旧实现7，recover2/3反例亦按预期RED。之后仅将BattleCpointWriter.ApplyHeldInjury正伤害后的三处Cover判断改读Recover，不动SyncHeldPosition中cover定位或其他战斗写者。修后原Editor刷新/编译/聚焦仍待；自然Unity Scene尚未运行。

2026-10-01 修后原Editor Tundra build success/0 error；job 9770a3ccb0a546b4990537e53aad4dd8 HeldInjury及邻近CaughtAct类40/40 PASS，九例recover/cover矩阵皆过。已在声明既有Play探针把cover11改正式差异cover1、明确recover0，并改独立Temp结果路径以保留旧C044 Play证据；该文件的新编译/Play仍待。自然Unity同初态首差另留待验，状态RUNTIME_PENDING / FOCUSED_TEST_PASS。

2026-10-01 原Editor再次编译Tundra成功/0 error并进入原Battle Scene Play，R8抓取探针cover1/recover0 PASS：停顿2/-3与cover持有位置116/19/201断言通过，object/slot/两池回基线、cleanupCompleted=true。原始JSON SHA A0D0674F5FBBB13AA21E18A43C066B97A184BED460920F32A1ACFE078A1501A9；退出后Editor idle/非Play/非编译、四保护SHA稳定。实际脚本范围仅上述三个声明路径；DAT/Scene/配置资产/GAS/非战斗未改。状态VERIFIED仅指Unity受控机制，正式OID65自然Unity同初态首差、父C045/Q07及总目标仍开放。详[限定验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C045-RECOVER-COVER-001/ACCEPTANCE.md)。
