> 最新出口：VERIFIED / WEAPON_PIECE_ADMISSION_SCOPE_ONLY。最终12回归/完整SelfCheck/173 Play/旧Scene4向量及有序关闭已通过，CS0、Scene dirty false/root14/hash保持、Ledger与diff-check通过。以下按形成过程保留失败与修正记录。

# 武器碎片准入、生成失败与同tick槽位验证

当前FOCUSED_TEST_PASS / PLAY_REVALIDATING。准确三生产脚本+一个Editor验证脚本，普通OPoint准入保持，Unity/GAS/非战斗/Scene/正式资源/schema不改。

生产改动：BattleNativeWeaponPieceWriter.IsInitialActionAdmitted复用原0..998隐式zero/声明999及非法范围规则；两工厂对nativeWeaponPieceSpawn允许OID0且借用前校验动作。Renderer工厂该专用路径把目标World传给现有referencePool.Get resetWorld，在武器/特效Init读取配置前绑定正确目录；普通及多生成caller保持原参数。

原依据：WEAPON-PIECE-ADMISSION-SOURCE-WITNESS原1800行（900出生/900完整driver）、17286断言、重复字节相同。原157 synthetic/762出生及3正式Logan/50出生作为两阶段回归。source raw epoch投影与global generation已纠正为独立字段，旧输出保留。

测试：初13中2PASS/11FAIL（six矩阵OID0以及直接非法动作nullref、池失败门限），文件red/focused.xml。修复后20/20 PASS，199.1982352秒，实际执行新15+原5；八组648/900/648/900/648/900/254/254共5152向量差异0，其中最后两组为完整NTSDBattleTickSystem。source998仅在容量1000 profile测试，保留400容量例外。直接端点、physics+Late、完整tick报告各自phase0/1/2，不混称。原raw三MISSING仍未绑定。

第二次Play暴露render factory未绑定目标World，随后同文件修复；最终12/12回归（原5+准入5+池2）37.9965179秒通过。20项证据在该binding修复前取得；新改动由fresh12和后继真实Renderer路径验证，不隐瞒时序。

Play失败记录：第一次probe读FrameWaitCounter误作原frame_counter，按既有raw绑定和93vs3测试纠正为AttackingCounter，生产不变；第二次98向量后render type1的Init取得全局目录导致FrameCache null，生产已修复并待重新运行。两失败均Renderer借用2→2、Scene checksum保持，随后现有Q05有序关闭全部零/两帧Stopped，失败原文保留，不改成通过。

完整SelfCheck首PASS为render binding之前；最终生产版本再次请求中。验证Scene使用旧Unity内容，Play新矩阵为隔离DAT fixture worlds借用实际两个factory/Renderer pool；不声称正式330资源/图片一致或物理按键技能验收。

最终生产绑定修复后：完整SelfCheck新鲜PASS（UTC2026-09-14 00:34:41），真实Play 172生成向量+1Renderer池耗尽PASS。两factory/type0..6/OID0/动作0/998/999/高低槽位/8次直接非法或普通OID0拒绝，以及logic/task/Renderer池失败全有证据；Renderer2→2、Scene checksum保持。现有真实旧内容Scene两route健康/破碎四向量0/5/0/5通过，逐例恢复4→4。最终Q05有序关闭结果待最后归档。

VERIFIED / WEAPON_PIECE_ADMISSION_SCOPE_ONLY：最终原20/20+renderer binding后12/12、5152端点无差异、完整SelfCheck PASS，真实Play 172向量+1池耗尽/两factory/全部七type与OID0/高低slot通过；普通OID0及非法动作直接拒绝，reference-task/logic/Renderer耗尽回收通过。旧内容Scene四向量0/5/0/5与checksum4→4通过，最终有序关闭World/slot/logic/Renderer全0且两帧Stopped，已退出Play。证据见artifacts同名REPORT/validation/各XML与Play、Shutdown文件。初次计数carrier错误、第二次真实targetWorld缺失及各自关闭记录全部保留。生产准确三脚本/测试一脚本，使用原pool API；Scene/HUDBg x30、非战斗/框架/资源/schema不变。

正式范围限制：当前正式catalog的OID0及已有正OID生成，保留普通OPoint合同；source upsert可构造任意signed ID的人工catalog，本矩阵未将额外负ID catalog作为正式内容准入要求。高低slot/full tick是指定fixtures，不是所有角色完整技能图。未来实际图像/声音播放按Q07/Q10回访。
