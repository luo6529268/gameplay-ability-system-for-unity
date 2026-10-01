# NTSD28-336B44-Q07-C048-BDEFEND-EARLY-RETURN-001

状态：`CONTROLLED_PLAY_PASS / FORMAL_NATURAL_PENDING`。父目标 NTSD28-UNITY-BATTLE-REALIGNMENT-001，BATCH-04/Q07/C048。

当前 336B44 playable 源 `BattleWorld28::resolve_confirmed_unarmored_standard_hit` 在 victim-rest 门通过后、首个当前 BDY 特殊响应前将目标 `bdefend_accumulator` 设为 45；首 BDY 响应若提前返回也保留此值。现有 Unity `BattleHitCandidateSequenceRunner.TryConsumeCandidate` 在同一门之后调用 `BattleFirstBodyResponseWriter.TryApply`，响应提前返回时尚未写 `Runtime.Bdefend`。正式 DAT 的 OID301/action29 提供 kind1033 入口；现有 Unity 战斗探针用 OID300/frame30 覆盖相同共用机制。本任务不推断两种角色的完整实战等价。

仅修改 `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs` 和 `Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs`：先增加现有战斗探针中 BDY 早退目标的 Bdefend=45 断言，再在共用消费路径的 victim-rest 门后、特殊响应前写 45。不得碰 DAT、Scene、模式、GAS 或非战斗代码。保留当前工作树其它更改。

验收：生成 C# 工程编译 0 错；原 Unity Editor 导入后运行既有定向战斗探针，核对提前返回的动作、HP、vrest 和 Bdefend；若可用，再做正式 OID301 同初态源/根/原 Battle Scene 对照。Editor 未导入时只能报告 `CODE_WRITTEN / RUNTIME_PENDING`，不能关闭 C048/Q07。回滚只审阅本包两处行级差量，不执行整文件恢复。

2026-10-01 代码已写：共用 runner 在首 BDY 响应前设置45，既有战斗 Play 探针加防御累计断言。生成 C# 工程编译0错；原 Editor 新程序集/Play 与正式 OID301 同态待验。

2026-10-01 探针夹具修正计划（修改脚本前）：原 Editor 定向 Play 在建夹具处失败，原因是旧 OID300/frame30 在当前正式DAT已无kind1033；战斗写入尚未执行，不能判C048失败或通过。当前正式 `data/data.txt` 的OID301→`s/1/1.dat`，正式与暂存该DAT SHA均为 `FB2517651A071550E23B53D667321F4273175DC56CC9CC8E909ECA3540C8FF6D`，frame29首BDY kind1033/respond默认0，响应action33。只在已声明的 Play 探针中将夹具配置/对象ID/起始帧及报告字段从300/30更正为301/29，保留原首BDY结果断言和生产逻辑；重跑同一个原Scene Play探针，随后退出并核四保护SHA。若后续出现其它真实首差，另行定位，不扩改DAT或非战斗。

2026-10-01 夹具已按脚本前范围更正：`BattleCollisionHitDamagePlayModeProbeEditor` 的正式配置、`ProbeCriminal.ObjectId`、初始动作帧与报告字段统一使用OID301/frame29，结果动作33、HP/vrest/Bdefend45断言不变；生产runner未再编辑。`dotnet build .\Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 成功，232警告/0错误；ChangeLedger validator PASS、相关脚本 `git diff --check` 0。原Editor DLL 已更新但域重载中，重跑Play和正式OID301同态仍待。

2026-10-01 第二次Play在已修OID301夹具后进入矩阵，但被独立“角色击倒统计归属”旧断言于C048断言前中止；`cleanupCompleted=true`且已退出Play，故仍未测Bdefend45。修改脚本前追加范围：在同一已声明的探针脚本内增一个C048专用菜单入口和独立结果路径，复用现有原World等待、受控实体注册/清理与JSON载体，只创建首BDY攻击者+正式OID301/frame29目标一对，经生产 `PostInteractionTickAll` 仅断言动作33、team1、hold3/-3、wait77、HP100、vrest0、Bdefend45。保留原R8全矩阵入口和代码，不修/绕过其统计失败，不改生产、DAT、Scene或非战斗。生成编译→原Editor导入→同Scene一次Play定向入口→退出/四SHA验收；若结果不符另做首差定位。

2026-10-01 C048专用探针已写：同一现有脚本增加`RunC048OnlyFromMenu`、独立`Temp/NTSD28_Q07_C048_FirstBody.result.json`与`ExecuteC048Only`；只物化一对OID301/frame29首BDY夹具，共用原生产World暂停、碰撞候选、`PostInteractionTickAll`、清理与结果载体。旧R8全矩阵入口保留。生成Editor工程0错/232警告；原Editor导入、专用Play及退出验收待。

2026-10-01 专用原Battle Scene Play实际通过：正式暂存OID301/frame29单对、生产候选1、`PostInteractionTickAll`后Bdefend45、action33/group1/hold3,-3/wait77/HP100/vrest0，结果`PASS`；cleanupCompleted=true、对象/槽/池和统计/RNG/sound/rest/plan均恢复。已退出Play，Editor idle/非Play、Scene clean，四保护SHA稳定。正式OID301自然链与正式根同初态逐tick仍待，C048/Q07不关闭。[限定报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C048-BDEFEND-EARLY-RETURN-001/REPORT.md)。

2026-10-01 只读可达性门更正：正式 decoded DAT 仅 `s/1/1.dat` 含 kind1033；同DAT未见转入frame29的 next/hit 字段，全decoded DAT未见OID301/action29 OPoint。普通OPoint/帧转移自然到达尚无证据；不等于证明不可达，先查正式战斗启动/stage等动态入口。受控原Battle Scene Play验收保持有效，不重复无鉴别力的全矩阵。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C048-BDEFEND-EARLY-RETURN-001/REPORT.md)。

2026-10-01 上段“同DAT未见转入frame29”只覆盖正值搜索，现已更正：OID301/frame30的`next:-29`由正式FrameMachine取绝对值、翻向后进入frame29。正式`data/stage.dat` mission1的child3第三phase还以OID301/action30为生成行，playable `GameSession28`按行传入初始动作。这是**剧情phase条件下的内容与源码前驱**，根EXE实际phase、后继命中和Unity剧情均未验；Unity stage父/子DAT依用户决定暂缓部署。C048保持`CONTROLLED_PLAY_PASS / STORY_CONTENT_DEFINED_PREDECESSOR / STORY_RUNTIME_PENDING / USER_STAGE_ASSET_HOLD`；不借此改DAT或非战斗流程，下一先推进不依赖stage的Q07首差。[完整更正](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C048-BDEFEND-EARLY-RETURN-001/REPORT.md)。
