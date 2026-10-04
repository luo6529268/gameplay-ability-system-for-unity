# NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001

状态：`RUNTIME_PENDING / GREEN_SCOPED_PASS / OTHER_GATES_OPEN`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07/C053。前三次原Editor定向Play确认第7tick缺slot50→slot2的90伤；共用消费修复后第四次默认模式原Scene12tick为`SCOPED_PASS`，正式源/Unity 261/261 可比数值同。当前独立Editor查询显示Scene又为dirty，磁盘SHA未变；保留其内存状态，尚不关闭其它C053出口。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001/REPORT.md)。下方编译待Play段为历史快照。

2026-10-04 GREEN 复验范围（探针脚本修改前登记）：共享候选消费守卫已在独立 `NTSD28-336B44-Q07-C053-LATCH-CANDIDATE-CONTINUE-001` 包中作最小修改，生成工程编译通过；原 Editor 尚待导入。仅为本探针增加新的独立请求/runId，沿用第二轮默认命中模式的同一受控12tick初态、字段及第7tick断言，拒绝覆盖三份 RED 原件。原 Editor 编译后只跑这一次定向 Play，并与当前336B44正式源/根逐 tick 配对；若后续 tick 或退出清理仍有首差，如实保持 Q07/C053 开放。不改生产、DAT、Scene、Prefab 或非战斗逻辑；本次脚本增量的回滚仅审新请求分支。

2026-10-04 默认命中模式复验范围（脚本二次修改前登记）：首轮原Battle Scene `ShadowCompare` 诊断运行已退出、Scene clean，前6tick所选23字段同正式源码，tick7正式slot50→slot2造成OID251 HP458→368/action20，Unity记录HP458/action10；同时诊断首错 `ObservationPreprocessMissing`，因此尚不能把此差归咎于默认生产命中路径。只在同一Editor探针新增独立请求/runId，跳过显式ShadowCompare配置并以默认生产模式复演同一12tick初态；仅断言tick7的World动作/HP、OID808结果和Scene保护，旧结果不覆盖。若默认模式亦分叉，再追候选/消费/写者；若默认模式相同，则隔离ShadowCompare诊断路径。生产、DAT、Scene、Prefab、非战斗不改；回滚仅审新诊断分支。

2026-10-04 候选顺序细化范围（脚本再次修改前登记）：默认命中模式第二轮也在tick7对slot2缺90伤，故不是仅ShadowCompare配置造成。下一仅扩同一探针独立第三runId，复用原ShadowCompare受控初态，在TickRow补slot50全部计划候选的顺序、目标和观察状态；不改生产、不修改既有两份结果，也不把诊断首错直接等同规则原因。仅一次原Scene定向Play；按正式根tick7 slot50 candidate0→slot0 rejected、candidate1→slot2 applied 的顺序核对，先判断候选是否存在，再决定下游调查。回滚仅审新增探针字段和runId。

权威与前置：[源/根双命中报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001/REPORT.md)已证明当前336B44正式版受控slot0 OID65/action511、slot1 OID702/action553、slot2 OID251/action0/X700/Y-60/Z400中，tick7两个不同攻击者对自然OID808同tick命中，目标HP440；根40tick五槽1136项可比字段同源码。第三对象由测试受控加入，不是玩家自然生成。

本Task仅扩既有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs` 的**新请求式辅助案例**，保留旧双安科及Q10音频请求的runId、结果文件和逻辑。生产、DAT、图片、Scene、Prefab、背景、模式Asset、非战斗均不改。新请求和结果使用独立路径、拒绝覆盖；只在原Editor单一干净Battle Scene、非Play/无测试、当前程序集已完成编译后运行。

预期行为：两个角色在正式内容中保留自然OPoint生成；受控OID251/action0/team1只加入空闲slot2，使用统一源坐标投影X/Z和原Y，seed/mode/输入与源/根案例同。完整生产Driver逐tick运行，记录slot0/1/2/50/51源坐标动作、HP及target命中计划/逐writer；tick7核对slot2与自然slot51两个applied writer、目标action156/HP440。若Unity factory无法构建完全相同初态，记录第一处前置差而不改生产凑结果。

验收：生成工程C#编译0错、Ledger校验；待原Editor恢复后仅定向Play，核对同源/根可比字段、逐writer、原Scene退出后clean且四保护SHA稳定。原Editor持续`is_compiling=true`时，不以旧程序集Play。回滚仅审该测试脚本增量与本Task/Change文档，任何文件删除按文件操作审计另行授权。
