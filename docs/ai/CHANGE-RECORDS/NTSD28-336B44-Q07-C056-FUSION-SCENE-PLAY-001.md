<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C056FusionHoldBattlePlayProbeEditor.cs
authority: selected 336B44 playable full-tick fusion hold counter and OPoint birth timing using formal DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001.md
-->

# C056 原 Battle Scene 完整 tick 探针

脚本前记录。当前 Unity 生产修复聚焦通过，缺原 Battle Scene 完整 Driver、结构性 OPoint 出生、池借用和有序退出证据。仅新增请求式探针及meta，精确路径如元数据；复用既有 Scene/Bootstrap/Driver，不改变生产、DAT、场景或非战斗。两组正式OID7/8/51/213内容SHA与根正式资源一致；源码同条件三tick出生 0/0/0 与 1/1/0。先编译后在原Editor按单Scene clean与非Play门执行，两次请求分别落唯一结果，原件不得覆盖。失败保留并定位首差。回滚须保留用户既有脏工作，需删除时依批准规则；其余验收/风险见Task。

2026-10-01 新增请求式原 Battle Scene 探针及 .meta：在 Play clone Bootstrap Start 前使用正式 OID7/8，同组 action9/HP100/源坐标、停顿3；三次中性完整 Driver tick 记录合体字段、结构 Writer 出生、OID213数和池借用，退出核 Scene SHA、池归零和关闭阶段。仅代码写入，待原 Editor 导入编译与 Play。

原 Editor 首轮导入暴露新探针缺 `NTSD.Simulation.Ecs` using，已补；第二轮原 Editor Tundra build success，0 error，程序集时间 03:50:22 UTC 晚于脚本；完整域重载后本项目原 Editor idle/非Play/原Battle Scene。生成 Editor 项目另跑 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0、235 warning、0 error。域重载可能使旧 World 引用不可读取，退出报告用 -1 明示未观测，不冒称池零残留；两组实际 Play 待。

首轮 c7-scene-01 原 Battle Scene 三 tick 已实跑，结果 `FIRST_DIFFERENCE`：主角 tick1～3 为 OID7/action650、计数7/hold2；伙伴未休眠，结构出生 0/0/0。形式上的出生数吻合但没有合体，不能算通过。Play 退出，Scene SHA 前后相同且 clean，引用池 2→0，关闭阶段因域重载未观测。初态排查发现探针漏把伙伴 AI 关闭，且伙伴朝向与正式源相反；此为**可能的探针前置首差，尚未证实原因**。保留原结果不覆盖，探针改为双角色非 AI、伙伴朝左，并增加伙伴动作/HP/team/源坐标、AI/融合计时诊断；下一运行 ID 为 c7-scene-02。当前脚本改后需重新编译，先前 COMPILE_PASS 仅适用于旧版。

修正后原 Editor 最新 DLL 编译0错，c7-scene-02 与 c0-scene-02 各3完整 Driver tick 均 SCOPED_PASS：合体OID51/action290、动作计数和停顿分别与当前源码 7/7/1、0/0/1 和 2/1/0 同态；结构出生分别0/0/0、1/1/0，c0两次均OID213。各次独立Play退出、Scene clean/SHA不变、引用池分别2→0和4→0。正式根EXE同受控入口、源码侧活体OID213和本次Scene十一阶段关闭顺序未观测，仍 RUNTIME_PENDING。原 Editor Tundra最新编译0错；生成Editor工程最新dotnet build exit0/235 warning/0 error，validator PASS 1090 records。完整边界与首轮无效前置见 [Scene报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001/REPORT.md)。
