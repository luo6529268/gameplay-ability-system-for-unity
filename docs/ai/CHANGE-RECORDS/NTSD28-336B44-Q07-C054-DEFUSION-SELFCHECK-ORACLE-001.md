<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C054-DEFUSION-SELFCHECK-ORACLE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal 336B44 playable BattleWorld28::advance_native_fusions split branch reconstructs partner precise XYZ from integer XYZ
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C054-DEFUSION-SELFCHECK-ORACLE-001.md
-->

# Q07/C054 SelfCheck 解融合位置预期版本修正

脚本前建立。原Editor完整SelfCheck第七轮在镜像OID8→51→8/7解融合的旧伙伴精确坐标断言失败，原件另存`selfcheck-result-07.txt`。正式336B44 `battle_world.cpp:2938-2940` 从伙伴整数XYZ重建伙伴精确XYZ，Unity C054共用 `TrySplit` 已同态，正式源码受控小数正反两轮及40断言已有证据。另一个OID7→51→7/8解融合方法含同一过期断言，先静态审计并纳入同一修改，避免每条重跑完整SelfCheck。

声明代码路径仅 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 的 `CheckOid5152MirrorIdentityAndPresentation` 与 `CheckOid5152SplitSuccessAndOddTruncate` 两条位置断言。主角精确位置保留；伙伴精确位置分别应为 `(77,-5,9)`、`(90,-3,6)`，与各自整数XYZ相同；碰撞Y、动作锁存、伙伴历史、Renderer等其它检查不改。预期副作用只在测试口径，不使C054父门或Q07整体完成。四保护文件和原诊断结果不覆盖不删除。

验收：生成工程0 error、原Editor完整SelfCheck越过两个方法或如实记录下一首差、Ledger validator、diff check和四保护SHA。回滚只反向调整本两条测试期望。

2026-10-02 代码已写：只更正两条解融合伙伴精确XYZ期望，镜像例为`(77,-5,9)`、常规例为`(90,-3,6)`；原主角精确小数、伙伴整数XYZ、collision Y及其它断言保留。失败消息补伙伴精确/整数实值；生产、DAT、Scene、非战斗未改。状态`CODE_WRITTEN`，编译与原Editor验收待执行。

2026-10-02 完整自检限定验收：生成Editor工程退出0、0 error/281 warning，原Editor刷新后程序集新于脚本。第八轮 `BattleRuntimeSelfCheck.RunAllChecksStatic()` 实际写出`PASS`，结果原件`artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/selfcheck-result-08.txt` SHA-256 `2F9ACB02FAA121BB2A3621951F57B4C690655337EDEE2E5AC350BE2B3BE88EA8`。第七轮旧断言FAIL原件`selfcheck-result-07.txt`保留。原Editor回idle/nonPlay/noncompiling、四保护SHA逐项不变；仅本测试口径`VERIFIED`，C054正式根自然小数入口、原Scene后继及Q07开放。
