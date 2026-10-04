<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs
authority: selected 336B44 root EXE and playable source controlled OID251 plus natural OID875 same-tick C053 hit witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001.md
-->

2026-10-04 GREEN 复验增量，脚本修改前登记：在既有 `NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor` 中增加第四个独立默认命中请求/runId，复用已证 RED 的受控初态和 12tick World/第7tick 断言，输出到同一诊断目录的新文件且拒绝覆盖。只为验证独立生产修复 `NTSD28-336B44-Q07-C053-LATCH-CANDIDATE-CONTINUE-001`，不改变前三轮请求或结果；原 Editor 需先完成 Unity 编译且 Battle Scene clean。成功仍仅能关闭该 C053 限定案例，Q07 其他出口开放。回滚仅审本文件与探针局部增量。

2026-10-04 实际增量与验证：唯一 Editor 探针新增独立 `AuxGreenRequestPath/AuxGreenRunId`，复用默认命中模式、同初态和第7tick断言；前三份 RED 结果未覆盖。生成 Editor 工程编译0错/299警告，原 Editor 生产及测试程序集晚于源文件且未编译中，单一 clean Battle Scene 原项目 Play 12tick `SCOPED_PASS/DONE`；正式源码逐tick 261/261 可比数值同，第7tick辅助对象action20/HP368、目标action156/HP440。运行内 Scene clean、磁盘SHA前后相同；退出后的独立查询Scene再次dirty而磁盘SHA仍同，来源未证并保留内存内容。状态仍 `RUNTIME_PENDING`，因为纯角色相邻回归、自然物理键和其它C053出口未验；详情见本包报告。

# C053 正式双命中原 Battle Scene 定向探针

脚本修改前登记。当前既有测试脚本只覆盖双安科OID875和Q10同案音频；新正式源/根阳性是两个初始角色自然生成OID875/808，外加受控第三OID251/action0，tick7对OID808两次applied。原状无同条件Unity逐writer证据，不能用既有双安科HP450证明HP440案例。

唯一脚本路径为现有 `NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs`；精确符号计划：请求/结果路由、`OnSceneLoaded`与`WaitForRoster`的opt-in初态、`TickRow/MeasureOneTick`选定字段与writer、`CompleteMeasurement`独立期望、`Finish`清状态。不改变旧runId、旧结果、生产战斗逻辑、资源或场景。新入口仅在请求文件且原Battle Scene clean时执行。

风险：Unity的factory生成OID251与正式LFR初始对象可能在owner/基础字段上不同；碰撞/HP若在tick7前分叉，应报告首差，不补测试专用生产逻辑。退出Play后必须通过已有Scene hash/clean检查；若当前Editor编译仍停滞，只做生成工程编译与静态核对，Play保持待验证。回滚仅按本记录审脚本增量，删除任何新增或旧文件须独立用户授权及审计。

2026-10-04 已在唯一脚本中增加独立 `AuxRequestPath/AuxRunId/AuxResultRoot`，旧Q10与双安科请求及结果不改；`OnSceneLoaded/WaitForRoster` 对新案保留两名自然OPoint角色并以现有 `BattleLogicEntityFactory.Create` 加入slot2/OID251/action0，使用共用源X/Z投影及Y=-60、mode0/seed同正式源。`MeasureOneTick` 对新案用两玩家离散空输入，记录辅助体/自然生成体/目标逐tick字段与目标命中计划、逐writer；`CompleteMeasurement` 新案单独校验tick7先slot2后slot51写者及目标HP440，旧双安科/Q10断言仍在原分支。`Finish`清辅助引用。未改Unity生产、DAT、图片、Scene、Prefab或非战斗。

生成Editor工程首轮 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` 失败1个CS0246：新`ObjectPoint`缺少`NTSD.Animation` using；原日志保留。仅补该命名空间后同命令第二轮exit0、0 error、299 warning，原件在 `artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001/`。此编译不代替原Unity Editor程序集重载或Play；当前状态`COMPILE_PASS / UNITY_RUNTIME_PENDING`，需复核Editor编译恢复后才请求该新案例。风险仍是factory初态或自然hit首差，届时按每tick和逐writer定位，不能把旧案例PASS当新案PASS。

2026-10-04 续验登记（再次改脚本前）：首轮ShadowCompare原Battle Scene运行`FIRST_DIFFERENCE/DONE`、退出Scene clean、SHA于本次运行内相同。正式源与Unity前6tick×23字段全同，tick7正式slot50→slot2 applied/90伤、OID251 action20/HP368，Unity action10/HP458；诊断首错`ObservationPreprocessMissing`、随机数调用少2。不能排除ShadowCompare诊断模式本身影响默认路径。仍只修改元数据唯一Editor探针：增加独立默认命中模式请求/runId、保留两原请求及结果，跳过新案的ShadowCompare配置，另校验tick7 World状态与正式值。编译后仅一次原Editor定向Play；若差异仍在，另立生产修复Change，本记录不得直接改生产。回滚只审该分支，不删旧证据。

2026-10-04 第三定向诊断登记（改脚本前）：默认模式第二轮完整12tick仍在tick7缺少正式slot50→slot2的90伤，OID251 action10/HP458，对照正式action20/HP368；Scene clean、Shadow配置次数0。需区分候选没建成与候选有但消费丢失。只在同一Editor探针加独立第三请求和`attacker50PlanEntries`列表，按捕获的攻击者槽50逐候选记录ordinal/target/ITR/预处理与处分，无生产文件改动；保存旧两份结果，不将诊断模式观察错误本身当生产原因。只跑第三定向Play和编译，然后以正式根tick7 candidate0 rejected、candidate1 applied作为定位依据。若需生产修复，另建Change及聚焦回归。

2026-10-04 三次运行结果：实际修改仍仅为本Record唯一Editor探针路径；独立默认模式和计划请求均保留原件。生成Editor工程两次增量build均exit0/0 error（各299 warning），原Editor新程序集已重载。原Battle Scene三次Play都`FIRST_DIFFERENCE/DONE`、12tick、退出clean、各次Scene SHA前后相同。正式源码/Unity首轮所选前6tick×23字段同，第7tick正式slot50候选1→slot2 applied/90伤，Unity slot2动作10/HP458而正式20/HP368，CRT调用3004对3006；默认模式`shadowConfigCount=0`仍同差，不能归因仅ShadowCompare。第三轮计划确有候选0→slot0、候选1→slot2，但二者未记录预处理/处分。状态`RUNTIME_PENDING / FIRST_DIFFERENCE_CONFIRMED`，不能写VERIFIED；下步只读定位共用候选消费守卫，若需修生产另立Change。未改生产、DAT、图片、Scene或非战斗。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001/REPORT.md)。
