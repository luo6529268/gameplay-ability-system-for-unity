# NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001

状态：`COMPILE_PASS / UNITY_RUNTIME_PENDING`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07/C053。新请求分支已写，生成Editor工程第二次编译0错；原Editor程序集与Play仍待。

权威与前置：[源/根双命中报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001/REPORT.md)已证明当前336B44正式版受控slot0 OID65/action511、slot1 OID702/action553、slot2 OID251/action0/X700/Y-60/Z400中，tick7两个不同攻击者对自然OID808同tick命中，目标HP440；根40tick五槽1136项可比字段同源码。第三对象由测试受控加入，不是玩家自然生成。

本Task仅扩既有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs` 的**新请求式辅助案例**，保留旧双安科及Q10音频请求的runId、结果文件和逻辑。生产、DAT、图片、Scene、Prefab、背景、模式Asset、非战斗均不改。新请求和结果使用独立路径、拒绝覆盖；只在原Editor单一干净Battle Scene、非Play/无测试、当前程序集已完成编译后运行。

预期行为：两个角色在正式内容中保留自然OPoint生成；受控OID251/action0/team1只加入空闲slot2，使用统一源坐标投影X/Z和原Y，seed/mode/输入与源/根案例同。完整生产Driver逐tick运行，记录slot0/1/2/50/51源坐标动作、HP及target命中计划/逐writer；tick7核对slot2与自然slot51两个applied writer、目标action156/HP440。若Unity factory无法构建完全相同初态，记录第一处前置差而不改生产凑结果。

验收：生成工程C#编译0错、Ledger校验；待原Editor恢复后仅定向Play，核对同源/根可比字段、逐writer、原Scene退出后clean且四保护SHA稳定。原Editor持续`is_compiling=true`时，不以旧程序集Play。回滚仅审该测试脚本增量与本Task/Change文档，任何文件删除按文件操作审计另行授权。
