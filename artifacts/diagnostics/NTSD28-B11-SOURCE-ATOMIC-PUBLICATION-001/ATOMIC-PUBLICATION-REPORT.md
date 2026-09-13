# Q02 E2 原子发布：当前验证与未关闭项

2026-09-13，`NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001 / RUNTIME_PENDING`。
总目标仍ACTIVE/FULL_ALIGNMENT_INCOMPLETE。E2不能因下面的局部成功而标为完整VERIFIED。

## 当前实现

新增Logan候选绑定catalog、完整对象配置和sheet/head/small图片身份。加载期验证实际解码bytes的SHA，body/common/atlas/UI均先暂存，所有准备及身份检查通过后，无await交换object/type/index/config/sprite/UI视图与source key；UI引用更新后才按catalog lease退休旧owned资源。registry_index保留；不把对象目录成功冒充背景、stage或WAV也完成切换。

正在战斗、Preparing已有实体、已seal或Stopping时拒绝切源；取消后未发布资源在原关闭stage5回收，stage1只撤销许可和世代，不重排worker Join或其他关闭阶段。旧公开入口保持；原UI loader创建的动态图片补上明确所有权，以便native交接及manager销毁回收。借用Sprite/material不转为owned。SelectRoleItem仅新增资源引用重绑方法，没有改既有选择、输入、状态切换或布局流程。

## 新鲜证据

| 检查 | 实际结果 | 证据 |
|---|---|---|
| Unity EditMode focused，生命周期夹具纠正后的最新运行 | 27/27，0 failed/skipped；atomic14、candidate7、PNG alpha6 | atomic-result-v5.json，job d088c453b2184143a8b00aad32a94e45 |
| 真实Play第一轮 | Running真实World4，public切源拒绝；App关闭Completed/RuntimeMapCleared，World/slot/pool borrower0；四项资源事务4/4，登记113资源全部销毁 | play-running-cycle-3.json |
| 退出后重新进入Play | 同上，四项4/4，113资源全部销毁 | play-running-cycle-4.json |
| 编译错误查询 | 0条error CS；上述新程序集实际运行 | atomic-compile-console.json |
| 编辑器Scene | NTSD_Battle，dirtyfalse、root14 | atomic-scene-after.json |
| 原3059文件保护 | 3052原hash相等，7个已声明既有脚本变化，零缺失 | atomic-workspace-protection.json |
| Change Ledger | PASS，469 Record，31 governed code diff；含历史路径不在当前diff的warning | atomic-change-ledger.log |

7个变化的既有脚本为BMPLoader、Lf2DatParserV2、CharacterAnimtorManager、GameDataManager、CharacterUIResourceManager、SelectRoleItem、SimulationTickDriver；前三个也包含本轮之前Q02子包改动。其余受保护配置、DAT、图片、Scene、Prefab等hash保持。新增脚本不在原3059集合中，另由Ledger覆盖。没有删除、提交或推送资源/代码。

实际验证使用已连接Unity2022.3.62f3 Editor及Temp/Goal13_bridge.py的refresh_unity/run_tests/get_test_job/read_console/manage_scene入口，没有另开Library写入实例。run_tests的testNames为NTSD.Test.NTSD28B11AtomicPublicationEditorTests、NTSD.Test.NTSD28B11VisualContentCandidateEditorTests、NTSD.Test.NTSD28B11PngSheetAlphaEditorTests。Play由本包Editor probe消费Temp/NTSD28_B11_SourceAtomicPublication.request.json，waitForRunning=true，runId分别cycle3/cycle4。Ledger实际命令：`& Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path`。保护检查按Q01 manifest逐文件重新算SHA256。read_console首次并行桥连接重置，随后顺序重试成功；未将空输出当作0错误证据。

## 失败记录与修正

atomic-result-v4已经27/27；随后为真实Play修改测试夹具，再运行v5。早期CS0117是Driver基类Accessor误用，已纠正为无创建的Instance；旧程序集测试不能算新代码通过。UI preview Scene夹具和SelfCheck common shadow前置缺失也已纠正。Play cycle1/Preparing诊断暴露真实owner缺口；Running cycle1因脱离NUnit上下文的TestContext.WriteLine失败，改用Unity诊断；Running cycle2因测试manager从未激活导致OnDestroy不执行而留24资源。夹具现真实激活/停用一次且保存恢复单例，再由销毁GameObject触发生产OnDestroy，未手工代替生产回收。全部失败artifact保留。

## 必须继续的R16回访

`play-preparing-diagnostic.json`仍为未修复真实失败：Preparing、World4/runtime slots2，关闭停在PendingObjectPointTasksDiscarded，原因unity-renderers-remained-without-an-object-pool-owner。报告cached-null pool borrower0不能证明真实池为空。

新Task/Change `NTSD28-PREPARING-SHUTDOWN-OWNER-CAPTURE-001 / PLANNED`已经写前冻结：首次出生前登记实际factory/pool owner，守住Preparing和已有borrower条件；准备世代/同World检查阻止过期async启动续体在关闭后重新启用战斗；保留十一阶段顺序。生产尚未改，focused测试脚本尚未创建。下一先执行该回访的focused RED/最小修复/Preparing与Running真实关闭后两帧检查及重进，再返回本E2决定出口。

正式6个DAT Converter拒绝仍归Q03；E3 cache/caller、Q07正式内容部署、后续B域战斗规则和Q12整场对照均未完成。本报告不是全正式内容加载、完整BattleRuntimeSelfCheck、角色技能物理按键、整场音视表现或最终native/Unity逐tick一致证书。

## 后继回访已返回（2026-09-13）

NTSD28-PREPARING-SHUTDOWN-OWNER-CAPTURE-001现已VERIFIED；Preparing2/Running2/App1五次真实Play，每次本E2四项4/4及113资源零残留，实际pool/factory关闭0残留/两帧仍Stopped。完整SelfCheck PASS、关联focused26/26、CS0/Scene clean/Ledger469/34PASS。此返回关闭上面的Preparing未关闭项：本E2现VERIFIED_SOURCE_ATOMIC_TRANSACTION_GATES_ONLY。原失败和pending事实全部保留；正式资源/E3/Q02/总目标不据此完成。下一E3 source/cache/caller合同审计Task已READY。
