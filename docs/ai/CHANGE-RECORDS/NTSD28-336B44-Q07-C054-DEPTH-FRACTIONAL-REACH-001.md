<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C054-DEPTH-FRACTIONAL-REACH-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/fusion_natural_fractional_reachability_probe.cpp
authority: selected 336B44 playable fusion split and native depth-running physics, formal OID52 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C054-DEPTH-FRACTIONAL-REACH-001.md
-->

# C054 自然纵深小数诊断留痕

脚本前原状：正式融合记录2的中性/持续向右输入于tick201自然拆分，精确XYZ始终为整数；晚跳在tick171无完成tick。原探针只有这三种 profile，无斜向跑步。正式DAT `running_speedz=3.7` 与正式物理精确Z积分给出新候选，但尚未证明输入实际进入跑步、保留小数或等到拆分。

拟只在 `input_for` 增加有界 `diagonal_run` profile、在主 profile 清单增加该项；旧 profile不改，产出新目录。若阳性需同一正式初态LFR/根/Unity后续证据，另追加实际范围及验证。不得改任何生产战斗、DAT/图、Scene/Prefab/配置/非战斗代码。回滚、保护与验收见同ID Task。修改后必须登记真实差量、编译、双跑、结果边界和未验项。

实际改动：只修改 `Tools/NTSD28Q07Diagnostics/fusion_natural_fractional_reachability_probe.cpp` 的 `input_for` 和 profile 清单，新增斜向输入窗；旧三个 profile 原样执行。首编 `InputKey28::up` 不存在，`compile-v1.txt` 退出1；改为 `depth_up` 后 `compile-v2.txt` 退出0。两次独立运行均退出0、CSV SHA同为 `92669428F6DBE476A1D9DE0671F2FE2034FAD5DBF3B94647217652FA95EF385A`、摘要SHA同为 `79FE3BB5058597452DD10747DC152A9BB55E85CC13807A0C13EFCD2C4C75F712`。输入窗tick1合体、tick201拆分，小数拆分未出现；正式fusion action310在OID52 DAT缺帧，正式输入路由无帧即返回，tick1–200动作不变。

验证层级仅为正式源码有界阴性，未运行根正式EXE、原Unity Editor、Battle Scene或完整World。C054/Q07/总目标仍开放。回滚只审阅并反向应用此诊断脚本的有限新增分支；不执行任何Git restore/reset/删除。原始输出与界限见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C054-DEPTH-FRACTIONAL-REACH-001/REPORT.md)。

账本与差异：`Tools/Validate-ChangeLedger.ps1` 退出0、1136 Record、当前代码差量12文件均有覆盖（共享工作树的历史声明产生非阻断warning）；`git -c core.safecrlf=false diff --check` 退出0。验证日志保存在同报告目录 `change-ledger-validator.txt`。
