# Q07/C040 雏田→奇拉比中间站位自然分源筛选

状态：`SCOPED_NEGATIVE / C040_REBASELINE_ON_REACH`。只复用现有 2026-10-01 `hinata_bee_held_position_lfr_probe.cpp` 的已编译诊断程序，读取当前正式 `resources/runtime`，没有修改 C++、Unity 脚本、DAT、图片或 Scene。正式根 `NTSD2.8-Logan.exe` 的 SHA-256 本轮复核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；诊断程序本身不晋升为正式行为权威。

固定初态为雏田 OID41/action286/X500 与奇拉比 OID75/action0、同 Z、seed682973786、mode0、中性输入。目标 X505～695 每隔 5 像素共 39 例，每例完整 `GameSession28` 120 tick，记录源 tick/RNG/LFR。32 例形成自然抓取和后续持有伤害；7 例无抓取。形成抓取的站位按首抓取/首次持有伤害 tick 分布：X515～580 为 1/9（14例），585～595 为 2/10（3例），600～610 为 3/11（3例），615～625 为 4/12（3例），630～640 为 5/13（3例），645～655 为 6/14（3例），660～670 为 7/15（3例）。本批 39 例 `first_c040_candidate` 全为 -1。

进一步逐行检查抓取关系有效、当前受害者帧与 `vaction` 不同、两帧挂点相对位置不同且 tick 入口受害者停顿非零的近门事件：共 480 行，`target_hold_before` 全为 -1。正式物理积分在持有结算前将其归零，结算将被抓者动作改为 `vaction`；因此这批样本没有验证到 C040 的“保留不同动作帧并读取 vaction CPOINT”定位分支。X515 与 X670 各重复一次，逐 tick CSV、RNG CSV、LFR 三文件 SHA 均与首次完全相同。

这只排除 action286、中性输入、上述 39 个离散站位和 120 tick 窗口。没有扫每个整数 X、其它角色/动作/按键；无阳性前置，故未运行正式根 LFR 或 Unity Scene，不能把阴性源诊断写成 C040 已对齐或不可达。后续寻找能使被抓者在结算时仍有非零停顿且当前动作不同于 `vaction` 的正式自然入口；避免重复仅改变上述站位而不改变停顿/动作时序的测试。

原件：`sweep-a286-summary.csv`、各 `a286-x*/source-ticks.csv`、`source-rng.csv`、`source-packets.lfr` 以及 X515/X670 的 `-repeat` 对照。旧 X550/X600 报告仍保留在 `NTSD28-336B44-Q07-C040-HINATA-BEE-NATURAL-REACH-001/REPORT.md`。
