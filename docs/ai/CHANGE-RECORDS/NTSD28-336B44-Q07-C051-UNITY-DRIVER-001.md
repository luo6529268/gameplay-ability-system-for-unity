<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C051-UNITY-DRIVER-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: selected 336B44 root LFR and playable GameSession OID20 to OID888 effect23 natural child hit
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001.md
-->

# C051 原Unity完整Driver诊断schema

脚本前：旧通用raw exporter对当前OID20→888和336B44身份没有严格接受的schema；C051源码/根右左各640/640选定字段同态，Unity尚未验证。拟仅增加新严格诊断入口及两固定夹具，不改生产、DAT、Scene或其它旧诊断合同。副作用仅新请求/输出；预期比较为左右各12完整Driver tick。风险是现有Unity自有stage的Z钳位及比例表现差异，须与战斗规则首差区分。验收和回滚见Task；写脚本后立即登记实际符号和验证。

实际已在唯一脚本`NTSD28UnityRawCaptureEditor.cs`增加C051 schema常量、正式内容门、336B44头身份/seed/12tick/固定两人初态校验，并在本ID目录新增面右X550、面左X350两份JSON。C050及其它旧schema逻辑不改，Unity生产/DAT/Scene不改。生成工程编译、原Editor导入及raw请求仍待；状态只为`CODE_WRITTEN`。

`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0，0错误/235警告；原项目Editor已核对身份、当前非Play/空闲并触发refresh，导入和两组raw结果仍待。`git diff --check`本脚本exit0。

后继实际结果：原 Editor 已完成导入，左右两份生产 Driver raw 请求均 `PASS`、各完成12 tick。各对照99个已声明字段，首差同在tick9目标动作Unity186/根180，tick12目标Vx在右例Unity+10/根-10、左例Unity-10/根+10；其余选定字段一致。此 `VERIFIED` 只指诊断入口与首差，不指C051机制或Q07已对齐。详情见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/REPORT.md)。
