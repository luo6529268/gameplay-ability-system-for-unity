# Q07/F04 OID600→OID219→E4 自然后继：60 tick 有界停止点

状态：`VERIFIED_BOUNDED_NEGATIVE / UNITY_RUNTIME_PENDING`。本包不关闭 F04、Q07 或总目标。正式根 EXE SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；使用其对应 playable 源码及正式 `resources/runtime`，未改 DAT、EXE 或生产 Unity 脚本。

复用已验的受控普通战斗初态：mode0、seed2833、Naruto OID2/槽0/team1、Lee OID7/槽1/team2、正式 OID600/type4/action0/槽2/team1/X500/Y-20/Z650/HP250。没有手设 E4、当前 HP 或速度。新增[源诊断](../../../Tools/NTSD28Q07Diagnostics/f04_oid600_heal_chain_probe.cpp)在完整 `GameSession28::step()` 中连续跑60个中性输入 tick，记录槽0/1/2与50～53的身份、动作、XYZ、HP、owner/team、目标槽和 E4。当前 playable 源码闭包的 [编译参数](compile-argv-v1.txt) 使独立诊断编译 exit0、stderr 0 行；自编 EXE SHA-256 `BFB5B3B83B3A7CB0322121DB64900DA7C3D0D33D1F28322E077BD27485ED186B` 只是诊断，不是正式行为权威。

源诊断记录的 [LFR](source-run-v1/heal-chain-source-packets.lfr) SHA-256 `A1B005D73EDF6377945C9AC86A366FA6426397792F0DAF4BD6E1223FB3770627`；源与独立本地 LFR 播放在初始+60tick的七槽 [427/427 个声明采样无差异](source-run-v1/source-vs-local-playback.csv)，包含 E4 和预赋目标槽。正式根 EXE 原样回放该 LFR，进程 exit0、[报告](root-run-v1/root-report.json) `passed=true/failureCode=0/declaredTicks=60/completedTicks=61/nativeParityClaim=false`。独立比较根 trace 的 tick0～60、七槽、仅实际导出的 active/OID/type/action/XYZ/HP/owner/team，[2983/2983 项可比字段无差异](root-run-v1/root-vs-source-comparison-v2.json)。首版比较器把**空槽**源端默认坐标0和根端无实体哨值-1作比较，误报572差；[旧输出](root-run-v1/root-vs-source-comparison.json)保留。v2只对空槽比较 active，活跃槽再逐字段比较，得到零差；这不是战斗规则修复。

同一诊断 EXE 对同初态第二次独立运行 [source-run-v2](source-run-v2/source-vs-local-playback.csv)，进程exit0；两轮 CSV 与 LFR 均逐SHA相同，第二轮仍为427采样零差、`first_e4_tick=-1`。根只回放了v1同一LFR，双跑确定性证据不扩成双根验收。

完整链实际停在治疗前：[源摘要](source-run-v1/chain-summary.json)显示 OID600 于tick12生成OID219/action50控制体，tick13进入action51；tick14在槽51形成 `aiTarget=0` 的 OID219子体，但其 HP=0。60tick窗口共47个预赋目标子体槽采样，HP值只有0；子体从未到action60，鸣人 E4 全程0、HP全程500。正式根 trace 也在tick14观察到槽51/OID219/HP0，槽51所有可见采样HP均0，全部OID219均无action60；[根子体摘要](root-run-v1/root-child-summary.json)不包含E4，**不能**被写成正式根直接读取到E4=0。

这与当前正式 playable 源码的具体顺序一致：`NativeAi28::step_non_character_hit_fa` 的 behavior5 子体 `SpawnRequest28.hp=0`（`native_ai.cpp:190-192`），后继 behavior4 在 `subject->current_hp<=0` 处提前返回（`:366-371`），故当前受控链到不了 `target->heal_timer_e4=100`（`:488`）。Unity 现有 `LF2Entity.RunHitFa5FrameLogic` 同样设置 `task.initialHp=0`（`LF2Entity.cs:2260`），`RunHitFa2Or4Or12Or14FrameLogic` 也有 `Health.HP<=0` 返回（`:1902`）；这是静态同向证据，**没有**执行 Unity 本链 Play，也没有证明其它生成入口或其它初态全局不可达。

因此 F04 精确回满生产 kernel 的既有聚焦通过仍有效，但当前 OID600→219/预赋目标链不能为 E4 或精确回满提供自然运行时证书。剧情 OID600 入口本身还受用户 stage 资产部署暂缓约束。后续如发现正式可达的正 HP 预赋目标子体或其它 E4 写者，应以新鲜正式源/根/Unity同条件运行证据回访；在此之前转向 Q07 其它已证可达的首差，不修改 DAT 数值或添加专用治疗规则。

验证边界：本包运行了正式源码诊断编译、源/本地 LFR、正式根 EXE 回放和独立字段对照；未运行 Unity Editor 编译、SelfCheck、Battle Scene Play、Game View 或设备验收。保护已有用户改动，未复制或删除资源。

交付检查：[Change Ledger validator](change-ledger-validation.txt) exit0/PASSED（1113 Records、56 个当前代码差异文件覆盖）；相关已跟踪文档 `git diff --check` exit0；四个本包新文本文件 UTF-8、尾空格与末行检查通过。这些静态检查不提升 F04 运行时状态。
