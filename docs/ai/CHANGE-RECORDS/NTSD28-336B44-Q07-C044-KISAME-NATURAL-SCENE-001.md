<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C044-KISAME-NATURAL-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C044KisameNaturalBattlePlayProbeEditor.cs
authority: selected 336B44 formal root and playable OID17 natural crosszero evidence
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C044-KISAME-NATURAL-SCENE-001.md
-->

# C044 鬼鲛自然跨零原 Battle Scene 对照

Unity现有C044共用writer已在受控低timeout测试及原Scene受控Play通过，但正式OID17自然跨零尚未在原Battle Scene逐tick对照。已增声明的Editor请求探针及同名meta；基于C042已验探针复用生产Driver采样，只接受OID17/action314的X550/X1200两例、原项目且干净Battle Scene，结束检查Scene哈希。生成 `Assembly-CSharp-Editor.csproj` 加入本新文件后 `dotnet build --no-restore -v:q` 为0错误/263警告；**原Editor程序集仍早于新脚本，未证明Editor编译，Play未运行**。`unity status` 返回当前项目无Pipeline实例，已请求用户在原Editor执行Assets→Refresh。若Play出现首差，另立最小生产修复Task/Change。未改生产、DAT、Scene或非战斗；回滚只审阅新增探针与meta。

2026-10-01 后续追加：原 Editor PID11944 经本地 MCPForUnity 刷新导入本探针，Unity 脚本编译成功。近距/远距两次原 Battle Scene Play 均 `CAPTURED/DONE`、60tick；各自对正式源码 20 字段×60tick=1200/1200、合计2400/2400首差0。近距tick47 timeout1→-6且目标速度0、tick48 X4/Y-3；远距无抓取。两次退出Scene clean，四保护SHA稳定，原Editor idle非Play。探针未直接记录pending内字段、Unity仅采前60tick；不外推C044其它路径或Q07。状态 `VERIFIED` 仅指本Task限定两例。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-KISAME-NATURAL-SCENE-001/REPORT.md)。
