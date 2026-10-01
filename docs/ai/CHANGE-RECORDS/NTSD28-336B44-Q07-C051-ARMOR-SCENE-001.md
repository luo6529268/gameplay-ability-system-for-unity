<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C051-ARMOR-SCENE-001
status: VERIFIED
change-kind: EDITOR_PROBE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C051OroScenePlayProbeEditor.cs
authority: current 336B44 OID78 to OID447 effect23 source/root and original Unity Driver parity
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C051-ARMOR-SCENE-001.md
-->

# C051 护甲原 Battle Scene Play 探针

脚本前：现有 C051 Scene 探针仅支持 OID20→888 左右无甲案例，且只有菜单调用。正式源/根和原 Unity Driver 已证 OID78→447 对 OID97 护甲及 OID2 无甲控制各自限定同态；原 Scene 的 Play/关闭出口未证。本包只改现有 Editor 测试脚本的精确参数化和请求入口，不改生产、DAT、Scene 或非战斗逻辑。预期副作用为原 Editor 短时进入/退出 Play、测试 World 内对象出生、写唯一新结果和自动消费本轮新建临时请求。请求文件生命周期另立事前操作记录。失败留原件，不能以 Driver 证据掩盖 Scene 首差。四保护 SHA、Scene clean、正式内容根与同 tick 根对照是验收；回滚范围仅本包测试脚本增量，须遵守文件操作合同。当前 `PLANNED`，尚未修改脚本。
实际脚本：仅扩现有C051 Editor探针，对OID78→447护甲/无甲精确roster、相对朝向、target OID和唯一结果路径参数化；原OID20→888左右菜单入口和断言保留。新增目标MP/护甲HP/X、子体数量采样；专用Temp请求校验并消费。无新meta，生产/DAT/Scene/非战斗未改。生成Editor工程0 error/238 warning；本地Unity-MCP桥刷新后原Editor程序集晚于源码。实际验收：原Battle Scene两次真实Play各12生产Driver tick，护甲/无甲分别对当前336B44根11字段132/132零差；两次SCOPED_PASS、退出Play/Scene clean。唯一临时请求的自动删除按NTSD28-C051-ARMOR-SCENE-REQUEST-20261001-001逐项留痕；四保护SHA不变、LoganRuntime Git无差异、原Editor idle/非Play。本包VERIFIED / SCOPED_SCENE_PASS；逐hit内部字段/自然按键/完整World和表现待，父C051/Q07开放。[验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-SCENE-001/ACCEPTANCE.md)。

交付复核：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` PASS（1103 Records、当前diff 47受治理代码文件）；相关文档 `git diff --check` 返回0。第一次 Write-Host 捕获只留下2字节空行日志，第二次全流重定向的[完整日志](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-SCENE-001/change-ledger-validation-v2.log)是有效原件；不以首次空日志作证。没有为测试入口改动跑全量SelfCheck，自然物理键与完整World仍待。
