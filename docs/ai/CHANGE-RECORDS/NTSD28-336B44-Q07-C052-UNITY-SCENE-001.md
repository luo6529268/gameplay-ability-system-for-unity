<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C052-UNITY-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C052PositiveRestBattlePlayProbeEditor.cs
authority: selected 336B44 playable effect21 victim_rest gate and controlled OID211 double-ITR source witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C052-UNITY-SCENE-001.md
-->

# C052 原 Battle Scene 生产首差探针

脚本前登记。Unity 当前 `BattleHitCandidateSequenceRunner.TryConsumeCandidate` 在 effect21 对当前 state18/19 提前结束整次攻击，静态未读取 relation rest；正式源码仅在该 rest 为零时结束。正式 OID211/action161 双 effect21 ITR 的受控三实体完整 `GameSession28::step` 已出现首目标命中、同目标因正 rest 拒绝且不终止、第二目标命中；原 Unity 生产路径未运行同条件，故此时不宣称生产 Bug。

唯一脚本范围为本 Record 的新 Editor 探针 `.cs`，附属唯一 GUID `.meta`。前置/作用/验收及回滚见对应 Task。只操作原项目当前 Battle Scene 的 Play clone，正式内容和两名正式角色由原 bootstrap 建立，第三 OID211 经已有生产工厂注册；不改生产、DAT、图像、场景、Prefab、配置或非战斗代码。测试本身必须输出完整 tick 的两目标 HP/action/rest、攻击者动作和退出/四SHA；若 Unity 与源码不一致，独立生产 Task 才能修复。请求不自动删除，不覆盖既存结果。风险为测试初态或工厂与源码配置不等价，需要用结果显式注明，不能以静态差替代运行首差。

2026-10-01 首次 Unity Refresh 导入失败：新探针第241行两个 out 变量合并短路 Require 导致 CS0165，`Assembly-CSharp-Editor.dll` 尚未更新。已将两次 roster 检查拆开；此时仅为测试脚本编译修复，原Scene/生产/DAT未改，待重新导入确认。

2026-10-01 实际出口：首次Unity刷新报CS0165，拆分两个roster out检查并二次刷新后，原Editor程序集已包含新增探针。近距X530和远距X650分别进入原Battle Scene Play，3个完整Driver tick后均DONE/退出/Scene clean/四保护SHA前后稳定。近距首tick首目标HP420/rest44、次目标HP500/rest0，正式源码同tick两目标均HP420/rest44；远距次目标始终500。近距次目标在Unity第二tick才HP470，不能以终态掩盖首差。具体原件见 artifacts/diagnostics/NTSD28-336B44-Q07-C052-UNITY-SCENE-001/REPORT.md。测试请求保留并置false，未删除；仅测试脚本和meta新增。状态VERIFIED仅指本限定诊断。

2026-10-01 回归探针续改：原脚本同一请求入口仅扩唯一v2运行名及 `initialFirstAction=203` 零rest门槛初态；v1首轮误把第二目标HP不变当断言，第二目标实为470但对OID211的rest0，校正只断言第一目标HP/rest和第二目标对OID211的rest后v2通过。近/远v2各3tick、选定42/42正式源码同态，零rest v2 PASS，三次Play退出clean/四SHA稳。所有原v1/v2结果及请求快照保留，当前请求只置false，没有删除。测试脚本原Editor多次Refresh均编译0新错；本Record VERIFIED仅说明探针可重放及首差证据有效，生产修复另由消费者Record管理。
