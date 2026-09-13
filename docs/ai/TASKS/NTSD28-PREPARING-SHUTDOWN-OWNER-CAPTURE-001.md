# Preparing关闭缺少对象池owner：R16即时回访

当前状态 VERIFIED_PREPARING_SERVICE_OWNERS_AND_CONTINUATION_GUARDS（2026-09-13）；以下保留最初问题与计划。发现于Q02 E2真实Play验证，必须闭合后才能完成相关退出重进验收；不是六个DAT Converter失败，也不是停止整个总目标的理由。

## 新鲜证据

`artifacts/diagnostics/NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001/play-preparing-diagnostic.json`：真实NTSD_Battle，Lifecycle=Preparing，World对象4；public native入口正确拒绝切源。App.TryShutdownBattleRuntimeBeforeSceneDestroy在PendingObjectPointTasksDiscarded之后失败，原因`unity-renderers-remained-without-an-object-pool-owner`，剩余World对象4/runtime slots2。报告pool borrowers=0来自driver缺失cached owner，不能据此认定实际全局池没有borrower。

`play-running-cycle-1.json`：等待Running后，同一正常App关闭已到RuntimeMapCleared/Completed、World/slots/pool borrower均0；后续probe因脱离NUnit上下文的WriteLine测试辅助问题停止。该正常路径证据不覆盖Preparing失败。

源码边界：SimulationTickDriver.ApplyMatchConfig只缓存当时已存在的factory/pool；BeginBattleAllocationSeal再取Instance。BattleTestBootstrap.SetupTestCharacters已创建角色，随后先await UniTask.Yield，才BeginBattleAllocationSeal/SetPaused(false)。ShutdownBattleRuntime在cached pool为空且World仍有Unity presentation bindings时明确fail。这构成可复现的出生后/封印前owner缺口。

## 要核验并实施

先沿App/直接BattleTestBootstrap的出生调用链确认实际factory/pool所有权、创建时点与所有renderer borrower的RegisteredWorld，不能根据单例名推断owner。优先在首次materialize之前显式建立已有runtime service owner关系；不要靠Shutdown中随意抓取可能属于其他World的全局池。

检查Preparing中取消后已有bootstrap async continuation是否还能重新enable presentation/进入Preparing；旧启动任务不得在关闭完成后继续生效。保持现有十一阶段顺序、dedicated worker Join硬门槛、不创建替代池、不把未清状态置null伪造成功；不能把allocation seal提前到角色冷构建之前来掩盖缺口。

开始脚本前另建同ID Change Record，冻结准确Driver/启动caller或factory路径与symbol、owner证据、异常/取消/回滚和最窄测试。此Task不是任意改框架的授权；只处理已证实的战斗准备/关闭适配。

验收：先复现Preparing失败RED，修后真实同一窗口的App关闭到RuntimeMapCleared，World/slot/实际池borrower/新source未发布资源零残留；运行Running退出、正常启动和E2取消回归，并退出重进。需明确actual pool诊断，避免cached-null计数0误导。Q02 E2本身27/27只证明其已覆盖的事务/owner路径，不替代本项。

## 写前合同已冻结

同ID Change Record已PLANNED并登记Ledger/STATE/handoff；准确范围为Driver service owner/准备世代、App.InitializeBattleAsync、BattleTestBootstrap.Start、focused editor tests和E2 Play probe实际pool/关闭后续体检查。尚未修改这些新symbols、尚未创建新focused脚本。原始Preparing真实RED保留；E2现fresh focused27/27，cycle3和cycle4各4/4且113资源零残留、正常Running关闭0残留。下一从本Record进行focused RED和生产修复，不因正常路径通过跳过本项。

最终出口：focused26/26、完整SelfCheck PASS、Preparing2/Running2/App1真实Play均0残留/两帧仍Stopped；CS0/Scene clean/Ledger469/34PASS。准确报告见同ID artifacts/diagnostics/REPORT.md及Change Record。本回访返回E2，E2同源原子事务限定关闭；下一E3合同审计READY，总目标仍ACTIVE。
