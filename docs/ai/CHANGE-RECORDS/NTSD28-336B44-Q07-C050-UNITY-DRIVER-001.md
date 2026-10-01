<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C050-UNITY-DRIVER-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: selected 336B44 root replay and playable GameSession C050 natural confirmed hit
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C050-UNITY-DRIVER-001.md
-->

# C050 原Unity完整Driver首差诊断入口

脚本前原状：既有 raw capture 严格场景只承认历史 B1E13 SHA 的正式 schema；当前336B44 C050 两正式对象/动作无法以真实身份提交。Unity prelude 的特殊 rest 门已存在，但普通垂直消费可能缺跳过；未有同初态完整tick Unity证据。

预定唯一代码路径为 `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs` 新增一个严格诊断 schema 与近远初态校验，不改旧schema或生产。预期副作用仅本诊断请求/输出；Scene、DAT、非战斗不动。验收与回滚见 Task，运行后补实际结果与风险。

实际已写严格schema/336B44身份/正式catalog/起始状态3tick校验与两个JSON；dotnet生成Editor工程0错，原Editor导入后近X520和远X1200 raw请求均PASS。源/根对照近距第2、3tick target action259 vs Unity186，HP465/Vy0同；远距18/18选定字段同。第一次raw头的通用formalAuthority字段仍旧B1E13，已仅对本schema更正，新输出重跑待；生产另包。证据及地图Z例外见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-UNITY-DRIVER-001/REPORT.md)。

2026-10-01 诊断包完成：原Editor已导入更正后的schema，`unity-x520-green` 与 `unity-x1200-green` 各3tick结果`PASS`，两头字段均为正式336B44身份；各18个选定战斗字段与根完全一致。这组GREEN发生在独立生产修复后，诊断包本身仅负责固定身份、初态与读数；其修前首差输出仍完整保留。未验证完整World/原Battle Scene Play，C050父项不随诊断包关闭。
