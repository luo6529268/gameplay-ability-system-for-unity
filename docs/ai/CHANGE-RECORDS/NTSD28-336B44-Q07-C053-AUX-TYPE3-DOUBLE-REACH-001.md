<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/c053_aux_type3_double_probe.cpp
authority: selected 336B44 playable GameSession28 and formal OID251 action0 effect2 plus OID65/OID702 OPoint live path
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001.md
-->

# C053 辅助type3同tick双命中可达性探针

脚本修改前登记。既有 C053 源/根/Unity自然OPoint链只有单Uj；既有受控双OID875命中不具备正式根LFR额外槽动作覆盖。正式OID251/action0具有effect2攻击框，或可作为第三初始对象，由target运动在自然OID875命中同tick进入碰撞；目前仅DAT静态候选，绝不作为已证行为。

本Record只拥有新增 `Tools/NTSD28Q07Diagnostics/c053_aux_type3_double_probe.cpp`，不编辑已有诊断、正式源码、DAT、Unity生产/测试、Scene或非战斗。预期副作用只有新诊断可执行文件、矩阵CSV、必要的阳性LFR/根报告与文档。先做固定有限91组×12tick的原生完整会话；若发现双方命中，严格记录攻击槽/候选/effect/Uj/HP/动作和同seed重复性，再考虑根LFR与原Scene。若没有阳性，按边界保留阴性，不通过调DAT或额外专用规则追求PASS。

不变量：现有C053单Uj、受控双Uj、Q07共享命中写者及用户保留的地图/模式/固定相机例外不因本探针改变。回滚只审新文件行级差异；产物删除遵循文件操作审计及用户授权。编译/源探针、根回放、Unity原Scene分别记录，不能跨层级冒充完整验收。后续实际写入、测试结果与未验项追加在本Record；当前状态`PLANNED`。

2026-10-04 实际新增唯一代码路径 `Tools/NTSD28Q07Diagnostics/c053_aux_type3_double_probe.cpp`；`make_config`建立受控slot0 OID65/action511、slot1 OID702/action553及slot2 OID251/action0，`run_case`在完整GameSession内按目标OID808收集攻击槽/实际OID/status/effect/post-Uj并识别不同攻击者同tick双Uj，主程序以固定91组×12tick写新CSV且拒绝已存在目录。尚未编译/运行，不能写成规则证书。未改其它代码、DAT、图片、Scene、项目模式或非战斗；预期风险是第三体自身碰撞改变旧单Uj轨迹，必须保留阴性/首差。状态`CODE_WRITTEN / COMPILE_PENDING`，回滚只审本新文件，删除需单独授权与审计。

第一轮编译调用因旧参数文件未包含编译器可执行路径而未启动进程，空日志/异常原件保留；第二轮显式加同一MinGW g++路径后33个源文件编译exit0/stderr空。新探针在正式资源下完成91组×12tick、无terminal，56组第7tick双Uj。代表X700/Y-60的逐hit为slot2/OID251与slot51/OID875各一次status0/applied、effect2/Uj156，目标本tick后HP440。此为正式源码受控第三对象阳性，尚无正式根或Unity同态。

**后续同文件修改前的准确增补：** 在已声明的同一Tools文件增加opt-in单案40tick LFR捕获和逐tick实体/RNG CSV，不改矩阵默认语义、正式源码、DAT或Unity。仅对上述代表站位执行两次新目录确定性校验，随后用正式根EXE验证第三初始槽载体、逐hit和可比状态；如根拒绝，保留failure原件，不伪造成规则首差。输出目录仍拒绝已存在路径。

2026-10-04 opt-in 捕获已写入同一Tools文件，首次重编因 `run_case` 调用缺少新增 tick 参数失败，原日志保留；修正后 v3 编译 exit0、stderr 空。代表 X700/Y-60 两次独立40tick正式源码捕获，LFR/逐tick/命中/摘要逐SHA同；两份LFR在336B44根正式EXE回放均`passed=true/failureCode=0`，根trace逐SHA同。根tick7有slot2→50 HP35和slot51→50 HP25两次applied，目标action156/HP440；源码两次effect2/Uj156。40tick×5槽的active/OID/action/HP/X/Y/Z实际1136项可比字段零差。根事件无effect/Uj字段，不能把源码数据冒称根可见；第三受控体另受目标命中90HP，属于该受控案例真实交互。原Editor测试已停、Battle Scene clean/非Play，但仍`is_compiling=true`，Unity原Scene未跑。状态`FOCUSED_TEST_PASS`仅限正式源码/根回放；C053/Q07及全World、物理键、Unity Play开放。详[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001/REPORT.md)。
