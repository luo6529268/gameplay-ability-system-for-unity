# Preparing关闭owner回访

2026-09-13，NTSD28-PREPARING-SHUTDOWN-OWNER-CAPTURE-001。当前VERIFIED_PREPARING_SERVICE_OWNERS_AND_CONTINUATION_GUARDS；本包五次实际Play矩阵已通过，总目标与Q02整体仍未完成。

## 问题和修复

实际RED来自E2/preparing诊断：直接打开NTSD_Battle，Preparing已有4个World对象/2个runtime slots，但driver尚无cached pool owner，关闭停在PendingObjectPointTasksDiscarded，报unity-renderers-remained-without-an-object-pool-owner。

确认的调用链为App.SetupBattleCharacters/直接BattleTestBootstrap.SetupTestCharacters通过LF2ObjectPool.Instance.Get借出Renderer，再ModuleBind到driver.World；旧driver直到BeginBattleAllocationSeal才必然获得pool/factory。两个出生入口之后存在await，形成可复现的出生后/封印前关闭窗口。

现在Driver.PrepareBattleRuntimeServices在Preparing、World有效、首次对象/队列之前捕获实际pool/factory，验证空borrower/queue、拒绝不同owner替换，重复调用仅同owner可通过；不在Stopping/Stopped创建服务，不提前allocation seal。两启动caller在出生前调用它。Driver准备世代与同World检查保护两个caller各await之后的续体，避免关闭后重启、跨重新准备复用旧任务；App同步ApplyMatchConfig即使先换World再抛错，也通过finally记录本次World供原catch回收，旧sound错误不回收后继World。

原十一阶段关闭顺序、dedicated worker Join、E2的stage1取消/stage5回收、Mono/GAS架构、33ms/schema12/20/23及游戏规则保持。App仅InitializeBattleAsync，直接Bootstrap仅Start；未改菜单选择/随机/输入/普通UI行为，也未替换资源。

## 当前证据

| 检查 | 实际结果 | Artifact |
|---|---|---|
| focused RED | 新8项全部因缺API失败 | focused-red.json，job819c831f2d0949779724e5b65b066e4b |
| focused GREEN | 26/26：新增8、原ordered shutdown4、E2 atomic14 | focused-v1.json，job4059ac66247d42e5b22582ed97e708ad |
| 完整BattleRuntimeSelfCheck | 本次实际PASS；结果mtime晚于请求，原结果另存历史 | selfcheck-result.txt、selfcheck-requested-at.txt |
| 真实Preparing两轮 | 每轮Preparing/World4，关闭Completed/RuntimeMapCleared，真实pool borrower0、两帧后仍Stopped/Worldnull，factory关且空；E2四项4/4、113资源零残留 | E2/preparing-owner-fixed-1.json、preparing-owner-fixed-2.json |
| 真实Running两轮 | 每轮Running/World4；其余同上，四项4/4、113资源零残留 | E2/running-owner-fixed-1.json、running-owner-fixed-2.json |
| 真实App启动 | PASS，角色50/52，Running/World4，关闭0残留、两帧仍Stopped，E2四项4/4、113资源零残留 | E2/app-owner-fixed-1.json |
| 文件保护 | 3059中3050原hash保持，9个已声明既有脚本变化，零缺失/范围外变化 | workspace-protection.json |
| Ledger | PASS，469 records/34 governed code diff | change-ledger-in-progress.log |

E2目录即相邻NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001。Play在同一个实际NTSD_Battle场景进入/退出，不创建第二Editor或修改场景资产；关闭后保留场景两帧，证明旧启动续体自身失效，而非先销毁场景掩盖。actual pool是关闭前捕获的真实引用，独立读取borrower和quiesced/accepting状态，避免cached-null计数0误报。probe在每轮关闭后复用合法隔离candidate验证E2完整发布、取消重试、stage hook和旧头像回收。

## 实际命令和限制

通过Temp/Goal13_bridge.py连接现有Unity2022.3.62f3，用refresh_unity请求编译，run_tests指定NTSD.Test.NTSD28PreparingShutdownOwnerEditorTests、NTSD.Test.BattleRuntimeOrderedShutdownEditorTests、NTSD.Test.NTSD28B11AtomicPublicationEditorTests；get_test_job保存完整结果。完整SelfCheck由Temp/NTSD_BattleRuntimeSelfCheck.request触发既有Editor入口，核对新结果和Editor.log通过信息。Play由同一E2 Editor request驱动，Preparing请求waitForRunning=false，Running为true；App附加appBootstrap=true，先使用既有stress抑制只准备旧资源，再以已加载角色调用真实App.InitializeBattleAsync。

`& Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path`与`git diff --check`通过，后者只有LF/CRLF工作树提示。保护检查按Q01的workspace-protection-start.json重新计算SHA256。Editor busy/reload时曾桥请求超时，检查同一运行的结果及日志后继续，未因观测超时重启Editor或测试。

旧音频data/001.wav等仍有404预热警告，属于后继声音接入；SelfCheck也有预期拒绝用例日志。不得因此把本包写成全Console无error、声音完整或正式资源完全可用。正式六DAT转换缺口仍Q03，Q07正式迁移未做，Q02 E3缓存/caller合同入口已准备为NTSD28-B11-SOURCE-CACHE-CALLER-CONTRACT-AUDIT-001。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，不关闭整个B域或Q02。

最终补记：editor-state-after.json为Playfalse/idle；scene-after.json为NTSD_Battle dirtyfalse/root14；compile-console.json实际0条error CS。最终Ledger见change-ledger-final.log。E2依赖回访已返回，可以限定关闭同源原子事务；下一E3已READY_AUDIT。
