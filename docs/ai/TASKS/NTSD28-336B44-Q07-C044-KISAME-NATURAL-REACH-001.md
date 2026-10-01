# NTSD28-336B44-Q07-C044-KISAME-NATURAL-REACH-001

状态：`VERIFIED_SCOPED_SOURCE_ROOT / UNITY_NATURAL_SCENE_PENDING`。父目标 NTSD28-UNITY-BATTLE-REALIGNMENT-001，BATCH-04/Q07/C044。正式336B44 decoded `c/kis/kis.dat` OID17 action314/316 的kind3抓取会选action120；后续123↔124帧均为state9、`decrease:-7`循环，静态上可能在持有超时跨零。此前OID16负decrease自然链在timeout38→35阶段先投掷，未触发跨零；受控低timeout与Unity修复已有独立证书，不能冒充正式根自然出口。

只新增 `Tools/NTSD28Q07Diagnostics/kisame_negative_decrease_lfr_probe.cpp`，以当前正式源码/playable闭包和正式`resources/runtime`建立完整`GameSession28`；OID17 X500/action314或316、鸣人OID2 action0，双方Z400、HP/MP500、mode0、seed682973786、中性输入。先扫近距和远距，逐tick记录动作/计数、抓取关系、timeout、投掷/释放、待结算冲量、两实体位置速度HP和RNG，并输出LFR。阳性标准是自然kind3建立关系且正式负decrease在timeout从非负跨到负时进入release分支；之后才拿同LFR跑根EXE，再另立Unity Scene任务。阴性记录首个阻断和终点，不改DAT或伪造初始关系/timeout。

本包不碰Unity生产、Scene、Prefab、配置、GAS或非战斗；保留用户现有脏工作。编译当前Core/Playable、核正式DAT与源身份、运行近远控制、必要时根同态、登记证据、账本与diff check；回滚只审阅本新增诊断文件，不清理/覆盖其它文件。

执行证据：[自然正式源/根报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-KISAME-NATURAL-REACH-001/REPORT.md)。近距双动作tick47自然1→-6释放、远距阴性，根LFR三例9600/9600声明字段零差；原Battle Scene自然出口转独立任务。
