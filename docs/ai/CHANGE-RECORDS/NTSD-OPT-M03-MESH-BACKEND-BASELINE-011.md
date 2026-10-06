<!-- CHANGE-RECORD
id: NTSD-OPT-M03-MESH-BACKEND-BASELINE-011
status: VERIFIED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleMeshUploadBaselineEditorTests.cs
authority: user next optimization batch; controlled current backend baseline only; formal336B44 and production invariants unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH11-MESH-BACKEND-BASELINE-20261006/REPORT.md
-->

# M-03 当前 backend 受控上传成本基线

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH11-MESH-BACKEND-BASELINE-20261006.md)完整声明范围、测试数据、限制、验证和恢复；
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH11-MESH-BACKEND-BASELINE-20261006/RECORD.md)保存事前备份/保护身份。
仅新增具名Editor fixture，现有生产代码不改；不制造修复RED或声称生产Scene基线。
事前状态PLANNED；编译/执行结果按实际追加。

实际首次scripts-only请求未导入新fixture，Editor DLL仍旧，不能作为新代码编译通过；
随后scope-all导入编译暴露本fixture的NonParallelizable属性在当前NUnit不可用（两条CS0246）。
仅移除本批新属性，沿用原Unity EditMode串行入口；没有升级依赖或修改第三方，原件保留。

第二次fixture编译CS1503：BattleRenderCommand既有构造器第16参数是value descriptor而不是Color32；
仅本新fixture改回既有测试的default descriptor，resolver仍明确white tint；不改生产数据契约。
按既有SubMesh fixture调用确认第二int为VisualDataId，新fixture使用index以实际交错variant；未读Q06活跃body。

本批限定VERIFIED_TEST_ONLY：原Editor实际编译CS0，新12/12、相关旧26/26，去重38Passed。
12组各64预热/1800采样，stride44/实际API payload复算，managed0B/capacityGrowth0/Mesh身份保持；
不是自然publication/素材/插值/RenderPass/GPU/AI/Player/Android或完整链。报告保存局部percentile及原件。
TestRunner临时untitled场景样本path为空；原Menu在前后保持clean/8roots，未调用Scene切换/保存API。
生产源码全部保持，子批10counter未伪写；重复Build上传不是生产same-sample门失效证据。
父M-03 OPEN/RUNTIME_PENDING，下一据phase拆分分段成本后最小A/B；全部专项门保持。
