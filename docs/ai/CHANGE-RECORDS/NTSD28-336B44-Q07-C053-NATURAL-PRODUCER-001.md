<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/c053_natural_producer_probe.cpp
authority: selected 336B44 playable live GameSession28 and formal OID65 to OID875 action50 to action55 plus OID702 to OID808 content
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001.md
-->

# C053 正式自然 producer 入口诊断

脚本前登记。当前受控 C053 源/原Battle Scene 8 tick末态已104/104，但正式根 LFR 对额外初始槽位的action55覆盖不足，不能形成同初态。正式DAT另有可由 OID65 动作生成 OID875/action50、继而转action55的静态候选；是否进入同一命中窗口尚未知。

仅新增声明的 C++ 离线诊断，不修改正式源码、Unity生产或测试、资源、DAT、Scene、Prefab及非战斗。使用当前正式 playable 构建闭包编译并在正式 `resources/runtime` 运行；预期副作用仅为新诊断可执行文件、CSV/LFR和报告。必须记录身份、初态、每tick生成/命中、固定种子双跑、首差或有界阴性，不把受控场景或静态OPoint当自然证书。若真正自然源码阳性，再单独建立根/Unity配对出口。

回滚只针对本包新增诊断及文档，先确认路径与既有工作无重叠；删除需按用户删除记录规则单独审计。验证以编译、双跑确定性和具体状态/命中证据为准，旧场景104/104不重跑。本Record将在代码写入后登记实际结果和未验项。

2026-10-02 实际写入与验证：新增所列C++诊断，不改Unity生产或正式DAT。v1/v2用于定位Y=-50下无命中，保留原产物；v3把安科初始Y设为可参数化，并在Y0、X550～730有限搜索。当前正式playable闭包 `g++` 编译exit0；同组两次运行的14个CSV/LFR逐SHA相同。X610在tick1生OID808、tick4生OID875/action50、tick5进55、tick7源码正式hit `51:0:2:156`，根EXE同LFR同步进程exit0/报告passed/failure0，40tick×12声明字段480/480同态。根trace不公开逐hit，初始action511/553受控，双Uj与Unity Scene/物理键未验；只记`VERIFIED`本诊断子包，不关闭C053/Q07。编译argv、源CSV/LFR、root报告/trace和独立比较见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001/REPORT.md)。未运行全量BattleRuntimeSelfCheck，因为本包只新增离线诊断，未改Unity代码。

交付检查：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` exit0/PASSED，1115 Record/61个当前差异代码文件，完整原件见[validator 输出](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001/ledger-validation-v1.txt)。本包相关已跟踪文档的`git diff --check` exit0；新诊断行尾空格0。Battle/Menu Scene、GameConfig和项目模式Asset四SHA与既有保护基线相同；未运行Unity Editor/Play。
