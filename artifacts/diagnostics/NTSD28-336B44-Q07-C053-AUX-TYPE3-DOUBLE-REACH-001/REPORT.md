# Q07/C053 受控辅助 type3 同 tick 双命中

状态：`FOCUSED_TEST_PASS`，仅正式源码与 336B44 根正式 EXE 的受控三初始对象可达性；原 Unity Battle Scene、物理键、全 World 与自然第三对象生成仍待，C053/Q07 不关闭。

权威为根目录正式 `NTSD2.8-Logan.exe`（SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`）、对应 playable `GameSession28` 完整 tick 与正式 `resources/runtime`。无 DAT token、角色图片、背景、模式、生产脚本或 Scene 改动。

既有两个初始角色为 slot0/OID65/action511、slot1/OID702/action553，分别自然生成 OID875 与 OID808；第三初始对象 slot2/OID251/action0/team1 是**受控辅助攻击体**，不是三角色物理选招或自然完整战局。seed `682973786`、mode0、背景1、前二者 X610/X500、Z400；只扫辅助 X670～730 每5及 Y-90～-30 每10，共91组，每组12个完整 tick。[矩阵](run-a/summary.csv) 91/91 无 terminal，56组在 tick7 对 OID808 出现两名不同攻击者的同 tick Uj。源探针 C++ 编译 exit0、stderr 空；第一次编译调用遗漏编译器路径及后续一次签名错误的原始失败日志也保留，最终 v4 调用编译通过并输出 `c053-aux-type3-capture-v3.exe`。

代表 X700/Y-60 做两次独立40tick捕获：[capture-a](capture-a/summary.csv) 与 [capture-b](capture-b/summary.csv) 的 LFR、逐tick CSV、命中 CSV 和摘要各自 SHA 相同。正式源码 tick7 先记录 slot2/OID251→slot50/OID808，后记录自然 slot51/OID875→slot50/OID808，两者 `status0/applied`、`effect2`、目标 `post_uj156`；目标 tick 后 HP440。[源命中](capture-a/hits.csv)。

两份 LFR 分别由根正式 EXE 独立回放，[根报告 A](root-a/report.json) / [B](root-b/report.json) 均 `passed=true/failureCode=0`、声明40tick（终端另含一个 tick）。两份根 trace SHA 均为 `52C366A97EF36E37EC6F2044CA93E3B058169F7D608BA4F2ADFFA149836C0BDF`。根 [trace A](root-a/trace.jsonl) 的 tick7 确有 slot2→50 `applied/candidate0/hpDamage35` 和 slot51→50 `applied/candidate0/hpDamage25`；期间另有 slot50→slot2 的90点命中，不能忽略第三体所改变的交互。目标 tick 后 OID808/action156/HP440。根事件不导出 `effect/Uj`，这两个字段来自正式源码，不能说成根直接可见。[逐tick对照](source-root-comparison.json) 对40tick、槽0/1/2/50/51的 active/OID/action/HP/X/Y/Z 作1136项有效比较，零差；没有把不等价的内部 latch 或 LFR 未编码的 RNG 初种算作同态。根报告的 `nativeParityClaim=false` 仍保留其原意。

原 Editor 在用户取消误启动全套 EditMode 测试后，Unity-MCP 只读显示 `tests.is_running=false`、Battle Scene `isDirty=false`、非 Play；但 `compilation.is_compiling=true/last_compile_finished=null` 持续，故未在旧程序集上执行 Unity Play。下一步先恢复原 Editor 编译，再用独立、预登记的原 Battle Scene 诊断将同一三初始对象及 tick7 双命中逐 writer 核对，连同有序关闭与 Scene SHA 验收；此源/根阳性不自动使 C053 或 Q07 完成。
