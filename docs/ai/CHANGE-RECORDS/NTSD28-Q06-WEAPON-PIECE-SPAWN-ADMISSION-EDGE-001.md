<!-- CHANGE-RECORD
id: NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001
status: VERIFIED
change-kind: NATIVE_WEAPON_PIECE_ADMISSION
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeWeaponPieceWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06WeaponPieceAdmissionEditorTests.cs
authority: Formal battle_world.cpp spawn_at/materialize_weapon_piece_fragments; original1800 admission/slot witness.
evidence: Both Unity factories reject native weapon piece oid0; native accepts existing catalog0. Native action checks must precede acquiring pooled entities.
-->

# 武器碎片准入与失败回收

事前准确四脚本。工厂只允许nativeWeaponPieceSpawn专用OID0，普通OPoint仍拒绝非正OID；原动作0..998隐式零/999声明/<0或>=1000拒绝在writer及两个factory共用现有辅助类判定，非法任务在借用实体/Renderer前返回。InitializeBirth继续最终重绑，不迁移其他reader/重构ModuleBind。逻辑frame/生命周期/随机及structural顺序保持。

先新Editor tests RED；原900出生与900完整driver输入按profile容量筛选，Unity两frame后端实际physics+Late与原端点对照（不将子pass称完整Unity driver）。两真实factory覆盖须真实Play，失败测试含task pool耗尽、logic pool耗尽、无slot、task clear与原普通OPoint0拒绝。风险：OID0角色外壳初始化、旧frame fallback被birth最终绑定、高slot当前扫描、render借用失败。无新增module/service/queue，仍现有关闭序列阶段3停止接单/6任务归还/7Renderer回收/8逻辑清理，测试finally验证。

验证计划：Unity compile、focused RED→PASS+相关原157/3回归、完整SelfCheck及真实Scene两route场景恢复/有序关闭；native源模型不等同正式EXE画面证书。回滚只本Record新增准入和测试（删除/回退先批准），不回退其他用户改动。Scene/HUDBg x30、非战斗/Unity/GAS/正式资源/schema均保持。父frame事务未完成，不提升全局状态。

RED已实际运行job fe76f53828494d51b4148f7e831e9a8f，13项终态failed，六矩阵差异都在OID0；非法-1/999缺失/1000直接factory触发InitializeBirth空descriptor异常。原日志/矩阵/XML归artifacts同名red。开始准入修复。测试追加同文件完整NTSDBattleTickSystem代表向量与Play隔离fixture-world两实际factory，仍不称正式资源画面/物理按键证书。

CODE_WRITTEN：三生产路径准入已写：两factory仅native OID0放行，Writer.IsInitialActionAdmitted复用原边界并在借用前校验。第四测试文件增加完整tick与Play fixture probe；Play隔离World共用现有Renderer pool逐向量显式归还，SceneWorld不改。初RED13中2PASS/11FAIL，156.724秒。

首Play FAIL为新probe读错carrier：原raw frame.frameCounter正式映射Runtime.AttackingCounter（现有RawCapture及专用93vs3测试明确），probe却读FrameWaitCounter。生产/原始raw/20focused不变，纠正本测试对应属性。失败已保留，Renderer2→2及Scene checksum保持，现有Q05有序关闭完成后重入复验。

第二次Play真实生产RED：98向量已过（logic-only全86及Renderer type0共12），随后Renderer type1因FrameCache.Wrapper null失败。LF2WeaponBase.InitializeFrame在Register前ResolveRuntimeCharacterConfig，render CreateLogicObject只Get(type,oid)没有先绑定目标World；其全局CharacterAnimtorManager不含fixture oid0。logic factory现成Get(type,oid,world)先绑定再Reset可正确闭合定义。修改前追加准确符号：同已声明LF2ObjectPointFactory.cs的CreateLogicObject加入可选resetWorld，且仅nativeWeaponPieceSpawn调用传task所属world，其他既有多生成/普通OPoint保持null。不是新World框架/快照创建，复用原pool resetWorld语义；Play原失败/零借用残留保持留证。

VERIFIED / WEAPON_PIECE_ADMISSION_SCOPE_ONLY：最终原20/20+renderer binding后12/12、5152端点无差异、完整SelfCheck PASS，真实Play 172向量+1池耗尽/两factory/全部七type与OID0/高低slot通过；普通OID0及非法动作直接拒绝，reference-task/logic/Renderer耗尽回收通过。旧内容Scene四向量0/5/0/5与checksum4→4通过，最终有序关闭World/slot/logic/Renderer全0且两帧Stopped，已退出Play。证据见artifacts同名REPORT/validation/各XML与Play、Shutdown文件。初次计数carrier错误、第二次真实targetWorld缺失及各自关闭记录全部保留。生产准确三脚本/测试一脚本，使用原pool API；Scene/HUDBg x30、非战斗/框架/资源/schema不变。
