<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C051-ORO-SCENE-PLAY-001
status: VERIFIED
change-kind: EDITOR_PROBE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C051OroScenePlayProbeEditor.cs
authority: selected 336B44 OID20 to OID888 effect23 source/root reach and C051 original Unity Driver
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C051-ORO-SCENE-PLAY-001.md
-->

# C051 原 Battle Scene Play 探针

脚本前：只新增声明的Editor探针及meta，验证两向共用无甲修正在真实Battle Scene/正式内容/生产Driver中的tick9动作HP和tick12速度，退出后Scene clean。现有Raw通过只覆盖受控完整Driver，不能推断Scene/关闭证据。副作用仅原Editor进入/退出Play、短时内存World及各向新报告；不改DAT/Scene/资源/生产。潜在启动超时、旧World接管或场景脏状态直接FAIL并留证。具体验收与回滚见Task，当前未修改脚本。

实际脚本：新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C051OroScenePlayProbeEditor.cs` 及 Unity 生成的同名 `.meta`。原Battle Scene两个菜单入口分别启动Play；仅Play clone预Start设置OID20/2，稳定暂停后按当前正式DAT执行12个生产Driver tick，SessionState跨domain reload保存进度；Exit Play后只以FileMode.CreateNew写一次各向结果，保护四SHA并检查Scene clean。没有临时请求删除路径、没有修改生产逻辑或DAT/Scene。

验收：第一次 `refresh_unity` 只恢复连接，脚本未进生成工程，随后第二次全范围刷新导入新脚本和meta；生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 0 error/235 warning，原 Editor 编译程序集时间晚于脚本导入。MCP只向原Editor发两个菜单调用，`right-x550-v1.json` 与 `left-x350-v1.json` 均为 `SCOPED_PASS`/12 tick/退出Play/Scene clean/四SHA相同。每向与当前正式根trace所导出的78个动作、HP、Vx、子体存在／动作／X字段零差。`git status --short -- Assets/NTSD/Content/LoganRuntime` 无差异。结果不证明自然物理键、effect22、护甲分支、对象池借用数或完整十一阶段关闭。限定报告 `artifacts/diagnostics/NTSD28-336B44-Q07-C051-ORO-SCENE-PLAY-001/ACCEPTANCE.md`；生产 C051 和 Q07 仍开放。回滚只撤此脚本/meta，须先依文件操作合同记录及获批；结果原件保留。

逐字段比较原件 `scene-root-selected-fields-comparison-v1.json`：左右各78/78、`differences=[]`，包含Scene结果/根trace SHA。交付 `Tools/Validate-ChangeLedger.ps1` PASS（1100 Records／46 diff脚本；历史路径警告未作为本包失败），相关 `git diff --check` PASS。未进行角色自然物理键或全量SelfCheck；不以此替代父任务验收。
