# Q07/C053 锁存候选消费：原 Editor 定向证据

状态：`RUNTIME_PENDING / SCOPED_GREEN`。本报告只评估共享候选消费修复和相邻的纯角色锁存回归，不关闭 Q07、其它 C053 情形或总目标。

## 规则与修复范围

当前根目录正式 `NTSD2.8-Logan.exe` SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。对应 playable `BattleWorld28` 在 `special_hit_latch_0eb` 且目标为角色时拒绝当前普通命中候选；`SimulationTickDriver28` 的候选循环只在显式终止标记出现时结束。原 Unity `BattleHitCandidateSequenceRunner.TryConsumeCandidate` 把这项拒绝返回为整次攻击者消费结束，造成后续合法非角色候选漏命中。本 Change 仅把该分支改为继续扫描后继候选，没有按角色或 OID 加特判，也没有改 DAT、Scene 或非战斗逻辑。

原 Battle Scene 的混合候选案保留三份修复前 RED；修复后第四次独立 12 tick Play 为 `SCOPED_PASS`。正式源码与 Unity 的 261 个可比数值逐 tick 一致，第 7 tick 后继 OID251 命中恢复 90 HP。该案证明“先拒角色、后消费非角色”分支在受控完整 Driver 中生效；详见 [场景报告](../NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001/REPORT.md)。

## 相邻回归

为检查“连续两个角色候选仍各自被拒”，先调用原 Editor 既有 R8 全矩阵。它在角色伤害的 kill/combo/global-stat 归属断言处提前 `FAIL`，尚未执行锁存断言；[R8 原件](r8-matrix-original-editor-20261004.json) 的清理已恢复对象数 10→10，不能把这次失败归因于锁存修复，也不能把它当作该断言通过。只读回查发现该旧断言要求 `characterHolder.KillStat == 1` 和 `ComboCountAtk == 10`；当前336B44正式普通致死路径写给credit的是 `knockout_count_358` 与 `input_score_total_348`，Unity对应写者为 `KnockoutCount358` 与 `InputScoreTotal348`，另有独立世界kill/damage数组。现有 `NTSD28B5StandardReducedKnockoutProducerEditorTests` 还明确断言生产写者不含旧 `holder.KillStat++`。因此旧R8矩阵至少存在过期统计字段期望；本次失败JSON没有记录各字段实际值，仍不把具体失败子表达式或其它矩阵状态写成已测。

随后只在已登记的 Editor 测试脚本增加独立的 C053 latch-only 入口，生产条件与数据保持。首个原 Editor Play 结果 [旧格式原件](latch-only-original-editor-20261004.json) 为 `PASS`：收集两个有序角色候选，两个目标 HP 均为 100，vrest、命中确认未写，清理后对象数 4→4，Scene 退出 clean 且磁盘 SHA 保持 `2BF4047C…D67C1`。但其旧 `hitConfirmAbort` 序列化了未直接测量的布尔占位值，不可用于证明整次攻击者的终止状态。已将新分支改为只输出实际测量的 `latchCharacterCandidates` 字段，并改用唯一 v2 结果路径；生成 Editor 工程重新编译 exit 0、0 error、299 warning，原 Editor 测试 DLL 时间晚于脚本。

用户确认保存后，Battle Scene 一度又显示 dirty；本包只读等待至原 Editor 报告 clean、非 Play、非编译且测试空闲，才启动唯一的 v2 Play。[v2 原件](latch-only-original-editor-20261004-v2.json) 为 `PASS`：tick402 收集两个角色候选，slot3/4 的 HP 均为100、vrest均为0；攻击者 `SpecialHitLatch0EB=true`、`HitConfirm2=0`。结果的 `cleanupCompleted=true`，对象数4→4、已占槽2→2、两个池活跃数2→2，RNG、统计、待播音效、rest和命中计划模式均已恢复。该原件的 SHA-256 为 `788EB23B9DD5E6B3BC680A5B6C142FDA6855EC13A01524330A4ABA4743FA6924`。`matrix.latchCharacterCandidates` 是本次实际测量的证据；同一通用序列化结构里的其它矩阵字段为未执行分支的默认值，不能用其布尔 `false` 推断行为。

退出 Play 后原 Editor 为非 Play、唯一 Battle Scene clean/rootCount13，然而磁盘 Scene SHA 从运行前的 `2BF4047C…D67C1` 变为 `8CC56145…269047E`，写入时点、责任者与意图未证。当前 Git 差异含另一个 `RippleRing` UI 对象和既有按钮预览改动；本包没有保存、覆盖或回退 Scene，也不把“磁盘文件前后不变”列为这次验收证据。测试仅使用注册的合成战斗对象并在同一生产碰撞 pass 检查结果；这次磁盘变动使 Scene 文件保护门保留待复核，但不改变原始结果中已测量的 HP/vrest/锁存值。R8 的统计归属失败需另按正式权威和同条件证据调查，不能由锁存案推定根因。

## 剩余出口

1. 如需继续使用 R8 全矩阵，应另建测试修订包，将旧 `KillStat/ComboCountAtk` 期望与当前336B44的 `KnockoutCount358/InputScoreTotal348`、世界统计分开核对，并保存改前实际值；这不是 C053 锁存修复的前置条件。
2. Q07 的 Tobi 自然物理按键链、其它 C053 分支、Scene 文件保护门和 Q12 集成验收仍开放。
