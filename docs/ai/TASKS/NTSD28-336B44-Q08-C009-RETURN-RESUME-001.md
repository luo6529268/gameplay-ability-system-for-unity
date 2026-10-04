# NTSD28-336B44-Q08-C009-RETURN-RESUME-001

状态：`RUNTIME_PENDING / FORMAL_SOURCE_ROOT_SCOPED_PASS`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q08/C009。

现有正式源/根与 Unity 原 Battle Scene 证据只覆盖第二阵营返回后暂停结果计时；第二阵营再次消失、仅剩一组后继续计时仍无正式可达证书。正式 `data/data.txt` 与对象 DAT 的静态只读筛选给出两组有限候选：OID220/type3/action0 经 OPoint 生 OID9/type0/action386（声明 `wait:3 next:1000`）；OID230/type3/action77 经 OPoint 生 OID14/type0/action355（声明 `wait:5 next:1000`）。DAT 声明不等于运行时可达，须以当前 336B44 playable `GameSession28::step()`、`BattleFlow28::step()`及正式根 EXE 裁决。

唯一新代码路径 `Tools/NTSD28Q08Diagnostics/return_resume_lfr_probe.cpp`。以既有 OID56/team1 存活者、候选 type3/team2 发射者、正式 runtime 和 mode0/seed682973786，各自最多 20 个中性输入完整 tick；记录每 tick 结果 timer、存活组、对象出生/消失与槽位，验证单组→两组暂停→再次单组恢复的具体时点。只对达到该顺序的候选生成 LFR，并用根正式 EXE 重放，比对选定字段。若两组均阴性，如实记录首差，不扩大为正式版不支持恢复。

不修改正式 C++ 源码/EXE、DAT、图片、Unity 生产代码、Scene、Prefab、项目背景/模式和非战斗模块。新诊断输出目录须拒绝覆盖。此工具的受控初始 action 不等于玩家物理输入；即使源/根通过，原 Battle Scene 同初态和物理输入链仍独立待验，不能关闭 C009/Q08。

风险：OID220 的 `next:1000` 可能在 OPoint 落地前回收发射者，OID230 的 cpoint/帧状态可能影响新生对象，故静态候选均不作结果预设。回滚仅审查新增工具、Task/Change/报告；删除或覆盖须另依文件操作审计和用户授权。

2026-10-04 实测：OID220 案 tick1 子体出生、tick2～4 timer1 暂停、tick4 World 子体消失、tick5 timer2 恢复；正式源与根选定7字段×20tick=140/140 零差，根回放 PASS。OID230 限定20tick无子体。原 Unity Editor 仍编译中，Battle Scene 未跑；不把正式源/根子门升级为 C009 完成。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURN-RESUME-001/REPORT.md)。
