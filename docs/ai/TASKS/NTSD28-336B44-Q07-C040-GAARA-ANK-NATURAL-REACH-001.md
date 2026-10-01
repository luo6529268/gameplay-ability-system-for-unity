# NTSD28-336B44-Q07-C040-GAARA-ANK-NATURAL-REACH-001

状态：RUNTIME_PENDING（本候选有界阴性）。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07/C040，仅正式完整会话自然可达性诊断。

依据：336B44 playable `BattleWorld28::settle_catch_relations` 从当前受害者帧读 center、从抓取者 CPOINT 的 vaction 目标帧读挂点。正式 DAT OID16 `gaa.dat` action365 kind3 可进入 action370 抓取链，action376 为 injury100/vaction130；OID65 `ank.dat` 的 130 帧 kind2 受伤可进131/132，130的相对挂点(-10,40)、131/132为(0,40)。这是静态候选，尚无自然运行结果。此前 OID41→75 六例均阴性，不重复。

范围：新增且仅新增 `Tools/NTSD28Q07Diagnostics/gaa_ank_held_position_lfr_probe.cpp`，以既有 C040 源诊断结构复用正式 GameSession + LFR，OID16→65、action365、近距 X780 与远距 X1200，120tick，中性输入，记录抓取、伤害、结算后 `held!=0 && target_action!=vaction && pose_diff` 正例，以及逐tick字段/RNG/LFR。只在真正正例后送根正式 EXE 同LFR对照；本包不改正式C++源、DAT、Unity代码或场景。新诊断脚本属于代码改动，先建本 Task、Change、Ledger/STATE/Handoff 再写。

验收：权威内容SHA与根EXE身份核对；诊断编译0错、两个相同输入复跑CSV/LFR SHA一致；近距若无完整条件则记有界阴性，不虚构阳性；远距无抓取为反例。若近距阳性，按正式根支持的LFR回放命令比较相同声明字段，再另包做Unity原Scene逐tick。不得用静态DAT或局部函数直接调用代替自然完整tick。

风险：action365跳过前置技能输入，仅是被正式GameSession接受的起始动作条件；若它未自然进入action376或伤害/停顿条件不成立，则换候选而非改DAT。回滚仅本包新增诊断文件/记录，任何删除仍按仓库规则另行批准。

出口：action365/X780与X1200正式完整GameSession各120tick双跑，编译0诊断、同输入CSV/RNG/LFR逐字节相同；近距tick1抓取/tick28伤害但目标action130与vaction130始终同态、C040分源0，远距无抓。按合同不送根EXE或Unity，另找自然阳性；C040/Q07仍开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C040-GAARA-ANK-NATURAL-REACH-001/REPORT.md)。
