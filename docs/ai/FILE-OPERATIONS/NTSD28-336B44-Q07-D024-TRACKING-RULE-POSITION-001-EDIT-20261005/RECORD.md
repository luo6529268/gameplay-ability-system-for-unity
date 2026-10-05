# NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-001-EDIT-20261005

状态：VERIFIED（仅三脚本文本增量与备份核验；生产行为Record仍RUNTIME_PENDING）；类型：三个脚本限定文本增量及现有字节备份；无删除/移动/Git丢弃。

授权：用户已启动总目标，要求通用战斗逻辑与比例统一入口、不得改DAT/非战斗；准确路径/符号见[Task](../../TASKS/NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-001.md)。执行者/root，工作目录I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity；开始UTC 2026-10-05T12:10:29.058152+00:00。

逐文件绝对路径、SHA、Git状态、字节、已逐SHA验证backup及四保护SHA在[NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-20261005/before-manifest.json](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-20261005/before-manifest.json)。三个文件逐项：
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` → `artifacts/diagnostics/NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-20261005/before/LF2Entity.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponFrameLogicResolver.cs` → `artifacts/diagnostics/NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-20261005/before/LF2WeaponFrameLogicResolver.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs` → `artifacts/diagnostics/NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-20261005/before/NTSD28Q07NonCharacterHitFa7EditorTests.cs`

拟执行：apply_patch增加已声明NUnit方法；RED确认后仅共用X/Z读取及相应caller增量。构建/Unity导入会更新其既有生成程序集，不修改输入DAT或Scene。所有新测试输出唯一文件CreateNew；不创建临时trace请求，不执行git restore/reset/clean。执行结果/新SHA/范围外检查待追加；失败原件保留，不盲目重跑。

执行增量：2026-10-05原Editor精确job804fecf984b24ae1b42c89be989fad61完成12、六个预期首差/六通过；生成build退出0。实际先仅apply_patch测试档两个已声明方法，原MCP refresh/具名测试，没有DAT/Scene/资源写入。状态RUNNING；三个backup保持可恢复，生产共用读取增量准备中。

最终操作核验登记：2026-10-05 12:30:40 UTC。执行者/root；实际以apply_patch仅修改上列三脚本，新增共用ResolveFrameLogicPositionPair及其追踪/回收caller、既有测试新增12个参数例与518单tick方法，保留操作前用户/其它任务增量。三次生成命令均为dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly，退出0/0error；三次现有Editor MCP refresh_unity均获请求成功并核对新程序集后才执行精确run_tests。RED job804fecf984b24ae1b42c89be989fad61有六个预期失败；GREEN jobbb07a0d205e84ffaa0a3d4a656ce985f为18/18；完整Driver job242dd61ff0514d7d9eee2746486fb120为1/1。两次includeDetails观察超时保留原件，同handle摘要终态成功，没有重复开跑。shell PID未记录，不作推断。

逐文件操作后SHA/Git状态与相对before的实际diff均存[最终身份及Editor原件](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-20261005/final-authority-diff-and-editor-state.json)，采集UTC2026-10-05T12:25:34.650651+00:00；四保护文件、四权威文件和七DAT两端SHA保持。原Editor非Play/idle/无活动测试，原Battle clean/root11、Console0error。没有文件删除/移动、DAT/图片/Scene写入或Git丢弃；不触及nonbattle。备份恢复须另有精确授权和新Operation，未执行恢复。原始构建、测试和限制回链[报告](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-20261005/REPORT.md)。本Operation关闭只表示准确文件增量可追溯，不能据此称父Q/总目标已对齐。
