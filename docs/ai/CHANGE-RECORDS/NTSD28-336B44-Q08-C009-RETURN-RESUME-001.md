<!-- CHANGE-RECORD
id: NTSD28-336B44-Q08-C009-RETURN-RESUME-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Tools/NTSD28Q08Diagnostics/return_resume_lfr_probe.cpp
authority: selected 336B44 playable GameSession28 BattleFlow28 and formal runtime catalog/OPoint
evidence: docs/ai/TASKS/NTSD28-336B44-Q08-C009-RETURN-RESUME-001.md
-->

# C009 再次单组结果计时正式源/根探针

脚本修改前登记。现状只有 OID304→OID56 返组暂停源/根84/84、原Scene72/72，尚无同一正式可达战斗中“返组后再次单组恢复”证据。仅新增 `Tools/NTSD28Q08Diagnostics/return_resume_lfr_probe.cpp`；两组 DAT 候选各最多20 tick，写逐 tick CSV、阳性 LFR、根正式 EXE 回放与选定字段对照。正式 authority 为 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的根 EXE 和对应 playable live path。

预期副作用只是在新诊断目录创建输出；不得覆盖已有文件、改 Unity/正式生产逻辑或 DAT。验收：当前源码闭包编译0错；若有阳性，源/根同 LFR 成功且逐 tick timer、存活组、出生/消失时点一致；阴性则记录，不伪报闭合。Unity Editor 编译卡住时不运行旧程序集，原Scene待。回滚是审阅此新工具和文档，任何删除需文件操作审计及用户授权。

2026-10-04 改后职责：仅新增 `Tools/NTSD28Q08Diagnostics/return_resume_lfr_probe.cpp`，两组正式对象受控初态各20tick，记录结果 pass 与 World 尾部、拒绝覆盖输出，只在达到恢复顺序时写 LFR。未改旧 C009 writer、Unity生产、DAT、Scene、资源。按既有 54 参数完整 playable 闭包调用 `g++`，exit0、`compile-output.txt`为空；探针 exit0。OID220 从出生、暂停到恢复阳性；OID230 20tick阴性。当前336B44根正式EXE SHA复核同 authority，根回放 exit0、`passed=true/failureCode=0`，20tick×7字段140/140同；根额外终端tick21排除。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURN-RESUME-001/REPORT.md)。未验证：Unity原 Battle Scene 同初态、物理输入自然链、全World一致、Editor编译；故 `RUNTIME_PENDING`，C009/Q08不关闭。风险：受控type3发射者不是玩家按键，不能外推到真实选择/输入。

治理与补丁检查：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` exit0，1232条Record、当前差异33个受治理代码文件，本工具被本ID覆盖；[输出](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURN-RESUME-001/change-ledger-validation.txt)。本包已跟踪文档 `git diff --check` exit0；新工具及Task/Record/报告仅新增文件，未删除或覆盖既有文件。
