# NTSD28-336B44-Q07-C053-SHADOW-HIT-SCENE-001

状态：`PLANNED`。父项：新版 336B44 G1 / BATCH-04 / Q07 / C053 / `NTSD28-UNITY-BATTLE-REALIGNMENT-001`。

目标：在已证 12 tick、132/132 同态的三角色自然双 producer 原 Battle Scene 案例中，读取 Unity 既有 `BattleHitExecutionPlanMode.ShadowCompare` 的实际命中候选顺序、观测 disposition、consume/writer 及 mismatch/failure 计数，对照当前正式 playable 源码 tick7 的 `51:0:2:156;52:0:2:156` 双 applied/effect2/Uj156。初始角色动作受控，仍不称物理按键自然选招。

脚本范围：仅扩展 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs` 的测试采集并新建独立唯一 RunId/结果；必要的生成工程目标只在 `Temp/`。不改生产脚本、DAT、图片、Scene、Prefab、ProjectSettings或非战斗功能，不删除/覆盖首轮原件。`ShadowCompare` 只在目标 World 的 tick0、正式输入及碰撞前设置；若 world 重建，则只在新 World tick0 重新设置，绝不在战斗中切换。若无法在此边界启用，保留失败结果，停止诊断。

验收：新探针生成工程/原 Editor 编译运行；同初态原场景 12 tick 仍与正式源码所选 132 字段零差；tick7 从现有 hit plan 读取按槽序的两条 attacker51/52→target50 候选及观察状态，`ShadowCompare` mismatch/failure为0，明确区分直接观察到的字段与由末态推断的字段；退出 Play、Scene clean、四保护 SHA 稳；Change Ledger 校验通过。只关闭本内部诊断子项，不关闭物理键、全 World、C053/Q07。回滚仅审阅此测试探针新增代码/诊断和记录，删除须另行授权留痕。
