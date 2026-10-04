# Q07 鸣人螺旋丸后续 Attack 窗口：336B44 源码与正式根回访

状态：`CURRENT_ROOT_CONTROLLED_INPUT_PASS / CURRENT_UNITY_RUNTIME_PENDING / VISIBLE_CUE_PENDING`。这是用户报告的后续 **Attack→螺旋手里剑** 分支，不是 Q10 独立的后续 Jump→`data/078.wav` 音频案例。只关闭本案当前正式源码/根发行版的受控输入采样子门；Q07/R18/Q12 和总目标保持开放。没有改 Unity 生产脚本、DAT、图片、音频、Scene 或正式 EXE。

以当前正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 和当前正式 `resources/runtime` 为权威。未改[诊断源](../../../Tools/NTSD28Q07Diagnostics/rasengan_natural_lfr_probe.cpp)（SHA-256 `41733C290E686F7257F0D74A9E03348B5C592F0BBA97629DD60EE0C049A36262`），按正式 `build.ps1` 的 28 个 Core 编译单元、四个相关 playable 编译单元重编，exit0/stderr0；新工具 SHA-256 `2201245D2B477C7BB7EDD58B2418836F46686FC8B6F61AE52A066EA9F02F3359`。[逐文件复核](old-current-input-identity.json)证其 55 tick 三例源码 CSV 与 LFR 共 6/6 均和旧归档逐字节相同；这仅说明所声明样本没因版本升级改变，不能扩展到其它 35 项修复分支。

三例共同初态为 Naruto OID2/action0/HP500/MP500、远处 OID7，X500/1200、Z650、背景23、seed682973786、mode0/BGM2；防御 tick1～2、右 tick3～4、跳 tick5～6。攻击分别从 tick34、35、36 开始保持两 tick。当前正式根对新生成的每份 LFR 均独立回放，三次进程 exit0、报告 `passed=true/failureCode=0/declaredTicks=55/completedTicks=56`，每份 trace 57 行。按 [comparison.json](comparison.json) 比较三例 55 tick 的 tick、输入相位、鸣人动作/MP、当前/上一攻击采样位，共 **990/990 相同、0 首差**。每例的 `nativeParityClaim=false`；LFR 输入来自源码，不是独立正式键盘录制。

| 输入窗口 | 当前源码与正式根共同结果 | 边界含义 |
| --- | --- | --- |
| 首个 action253 后开始攻击（tick34） | tick33 `253/MP350/phase1`；tick34 `301/MP250/phase0` | 下一采样 tick 尚在可转帧内，成功 |
| 第二个 action253 后开始攻击（tick35） | tick34 `253/MP350/phase0`；tick35 `254/MP350/phase1`；tick36 `254/MP350/phase0` | 攻击到达 phase1，phase0 消费时已经越过 253，失败 |
| 进入 254 后开始攻击（tick36） | 与上例根 LFR 和所选字段同一采样流，维持 `254/MP350` | 根 LFR 无法区分这两次物理按键时点 |

旧 B1E13 的原 Unity **自然组合键**报告已观察到同一相位规律：首 253 后按 J 转 301；第二 253 后按 J 在 254/phase1 才进入 FrameInputSet，下一 phase0 未转换。另一个旧 Unity“从 action241 受控起始”的第二 253 物理探针成功转换，是**不同的起始相位**，不能与本自然 action0 链直接构成生产首差。两份旧 Unity Play 均不能自动晋升为当前 336B44 的运行时验收。有关旧样本与前提见 [自然组合键旧报告](../NTSD28-Q07-RASENGAN-NATURAL-COMBO-PLAY-001/ACCEPTANCE-20260925.md) 和 [受控物理键旧报告](../NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/FULL-WINDOW-COMPARISON.md)。

本轮没有当前原 Editor Battle Scene Play：该 Editor 进程仍在，但最近可读内存状态为 Scene dirty/compiling；修正探针源码晚于其程序集，尚无新的 clean 前置。现有 `NTSD28UserRasenganPhysicalPlayProbeEditor.NaturalProbe` 已提供三条自然物理键菜单及逐 tick 的 FrameInputSet、2tu phase、动作与 PP 记录，**无需为本次回访先写新探针**。下一步在安全前置下只运行这三条针对性菜单，复现当前自然防→前→跳→持续帧攻击，再核 Game View 呈现；不跑全量案例。正式根当前 headless trace 不能证明屏幕上哪个画面提示已出现、用户从看到提示到按键的实际时间，也不能据此修改 DAT wait 或加入鸣人特例。若同相位自然 Play 真有首差，先定位共用输入/呈现出口并按 Task/Change 合同修复，再做其它角色/技能相同规则回归。
