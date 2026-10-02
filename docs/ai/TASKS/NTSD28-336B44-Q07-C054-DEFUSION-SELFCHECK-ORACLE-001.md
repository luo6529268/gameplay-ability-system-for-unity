# NTSD28-336B44-Q07-C054-DEFUSION-SELFCHECK-ORACLE-001

状态：`VERIFIED`（仅SelfCheck合成口径；C054父出口开放）；父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`；新版 BATCH-04/Q07/C054。

原Editor完整SelfCheck第七轮通过C056合体锁存正反测试后，首差到 `CheckOid5152MirrorIdentityAndPresentation` 的旧“解融合伙伴复制主角精确小数 XYZ”断言，原件 `artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/selfcheck-result-07.txt`。当前336B44正式 `BattleWorld28::advance_native_fusions` 解融合时先复制主角整数XYZ到伙伴，再由整数重建伙伴精确XYZ；主角自己的精确XYZ保持。C054当前源码小数控制例两次同SHA/40断言PASS，Unity `BattleOid5152RuntimeModule.TrySplit` 已写相同。旧SelfCheck另在 `CheckOid5152SplitSuccessAndOddTruncate` 对同规则保留第二条过期精确坐标断言。

只改 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 上述两方法的两个位置断言：主角精确值继续按原预期；伙伴精确XYZ改成原整数XYZ转换后的值，整数快照和collision Y reference不变；必要时在失败消息中给出实际值。保持解融合HP/PP、动作锁存、Renderer、伙伴历史和生命周期断言不变，不改生产、DAT、Scene、相机、比例或非战斗。脚本前建同名Change Record、Ledger、STATE、handoff；脚本后编译、原Editor SelfCheck和账本/差异/四保护SHA验证，结果逐轮另存。

该修正只处理合成测试口径；C054正式根自然小数入口、原Battle Scene后继tick及Q07仍开放。回滚需审阅本两条断言的精确差量，不用整体Git恢复。
