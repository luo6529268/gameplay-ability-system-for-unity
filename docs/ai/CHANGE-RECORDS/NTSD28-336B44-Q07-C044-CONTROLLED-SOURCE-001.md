<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C044-CONTROLLED-SOURCE-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/gaa_negative_decrease_control_probe.cpp
authority: selected 336B44 playable BattleWorld catch relation and horizontal impulse finalizer with formal OID16/OID2 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C044-CONTROLLED-SOURCE-001.md
-->

# C044 正式 DAT 受控逐 pass 边界

在脚本前创建。原状：正式OID16四条自然路径到负decrease帧，但timeout最低35，尚未跨零；当前权威源码跨零写pending impulse且保留帧计数，Unity共用writer当前立即写速度、重置计数。只声明新增一份C++诊断文件和证据目录，正式DAT只读。预期副作用仅手动编译/运行诊断；不改生产或非战斗。

验收与限制见Task：2/3/4同一正式帧低timeout正反，单独记录catch pass和impulse finalizer，不宣称正式根/自然Scene同态。若源码差异/编译失败，保留证据并诊断；后续Unity修改需独立Task/Change。回滚只精确审阅本脚本，保护原脏工作。状态`PLANNED`。

2026-10-01：诊断代码仅新增声明脚本；当前 Core/Playable 的 g++ 编译 exit 0、无诊断。正式 DAT 三边界两次运行各 exit 0，CSV 同 SHA 1ADD4554DAD2BD1098FFF056CE0C56071288A05F02E74067AAA1C7C29B2FE0ED；timeout 2 抓取 pass 动作 0/181、计数 7/8、pending 双方 1、受害者冲量 -4/-3 但 motion 0，finalizer 后 motion -4/-3 且 pending 0；timeout 3/4 为投掷反例。只证当前源码受控边界，不证根自然或 Unity。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-CONTROLLED-SOURCE-001/REPORT.md)。状态 VERIFIED / CONTROLLED_SOURCE_ONLY。
