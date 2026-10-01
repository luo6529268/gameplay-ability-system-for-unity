<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001
status: VERIFIED
change-kind: EDITOR_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: 336B44 OID78 to OID447 effect23 versus OID97 armor and OID2 control source-root positive
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001.md
-->

# C051 effect23 护甲原 Unity Driver 诊断入口

脚本前：原Unity Raw Capture 的当前 336B44 C051 schema 只接受 OID20→888 左右两组，尚不能承载已证正式OID78→447对OID97护甲及OID2无甲控制。只在该 schema 的精确 combatant 断言新增两个初态允许式，不放宽版本/正式DAT/seed/mode/tick/输入和其它场景。新输出路径唯一，原结果不覆盖。预期只影响Editor诊断验证，生产不变；错误初态会被拒绝。编译/Driver首差、四SHA及退出检查见Task；回滚仅本包的新增允许条件（需依文件操作合同授权），保留证据。状态 PLANNED，尚未改脚本。

实际脚本：仅在 `NTSD28UnityRawCaptureEditor.ValidateScenario` 的现有 C051 当前正式 schema 断言中新增 OID78/action466/X500 面右对 OID97/action0/X550 面左及 OID2 同态控制；原 OID20→888 左右、版本/seed/tick/mode/正式内容门不变。编译和原Editor运行待验，状态 CODE_WRITTEN。

实际验收：原 Editor 导入编译程序集更新；生成 Editor 工程0 error/235 warning。MCP `execute_code` 动态编译报文件名过长，未产raw；已留失败原件。按 `NTSD28-C051-ARMOR-RAW-REQUEST-LIFECYCLE-20261001-001` 事前记录后使用原Editor请求入口，护甲/无甲两案各12生产Driver tick raw/result PASS、与当前根选定11字段各132/132零差。护甲tick3目标HP495/action0，无甲HP450/action180。两次自动清理的唯一新请求均已消失，Editor idle/非Play，四保护SHA相同、LoganRuntime Git clean。仅测试入口脚本变动，生产/DAT/Scene未改。本包 `VERIFIED / SCOPED_DRIVER_PASS`；原Scene及逐hit内部字段待，不提升父 C051/Q07。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001/REPORT.md)。
交付复核：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` PASS（1102 Records，当前 diff 47 个受治理代码文件全部覆盖）；`git diff --check` 对本包脚本及总表/STATE/handoff 返回0。首次以 PowerShell 5 独立 `-File` 运行时，脚本默认参数 `$PSScriptRoot` 为空而在参数绑定失败；显式 RepositoryRoot 重跑已通过。未扩大全量 SelfCheck，因本包只扩 Editor 诊断输入 schema，真实生产战斗仍在父 C051 门下。
