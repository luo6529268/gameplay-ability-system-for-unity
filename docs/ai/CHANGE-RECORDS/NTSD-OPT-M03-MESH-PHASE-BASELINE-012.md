<!-- CHANGE-RECORD
id: NTSD-OPT-M03-MESH-PHASE-BASELINE-012
status: VERIFIED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleMeshUploadBaselineEditorTests.cs
authority: user next optimization batch; controlled Mesh phase timing only; formal336B44 unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006/REPORT.md
-->

# M-03 既有阶段计时受控基线

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006.md)声明准确范围、假设、验收和恢复；
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006/RECORD.md)保存八dirty备份/358保护。
事前PLANNED，任何脚本编辑前登记；仅扩展子批11fixture，生产不改。
原状：已测高物理segment增大CPU，但未将Resolve/Write/Upload与子网格元数据拆分。
改后目标：复用现有两个recorder并完成8组instrumented+12组无诊断同轮对照；不新增生产计数/自动采样。
计时作用、嵌套包含、观测税、synthetic/null资源与非生产0GC范围必须保留，不制造production RED。
无Runtime owner/worker/lease变更；本地backend Dispose，11阶段合约保持。
当前未执行compile/tests，未声称收益或设备通过；结果追加。

CODE_WRITTEN：原12项入口保留并委托同一RunBaseline（diagnostics=null）；新8项复用两recorder。
六phase预分配采样数组，所有Begin/End及scalar读纳入分配窗、Build时间单独括号；父子重叠与残余显式报告。
既有schema维持，阶段schema另为v1，NUnit输出前缀分开；无生产/资源/M0写入。

原Editor刷新scope-all后domain reload重试一次，最终idle/nonPlay/error-CS0；Editor DLL22:48:57晚于fixture22:48:41。
production DLL仍22:15:24，无本批runtime源码改动。原件compile-reloading/current；尚未称执行通过。

实际执行：fee8c39d9afb4f10b8a02c0d1cec8fc2 = 20/20 Passed（8阶段+12控制），67.1896762秒；
17476eed227049fd8cf4eca1e6a1595d = 34/34 Passed，2.0601896秒；54去重fullName全部Passed。
36000有效Build/1280warmup排除，六phase×8×1800=86400完成样本；各case当前线程0B/growth0、payload/segment与Mesh身份保持。
阶段开启有观测税，1000命令mean比顺序控制高0.33～0.46ms；非配对，不称精确timer成本/收益。
阶段父子不可相加；SetSubMeshes包括descriptor遍历/生成及native metadata，与GPU draw/流量无关。
原Menu前后clean/8roots/nonPlay，CS0；Runner临时untitled场景不是生产窗口。父M03与全链/像素/设备保持开放。
未执行Play/SelfCheck/完整M0/GPU/1000AI/Android；没有battle规则变化所以未重跑权威trace/全套。

最终VERIFIED仅本Editor fixture/阶段基线：compile、54具名测试、分配/样本/payload/身份及治理窄验全部通过。
22:55 Validator0错误/newscope0warning，358保护/8before备份和六compiled身份保持、164链接0缺失。
production与父M03仍OPEN/RUNTIME_PENDING；本Record不作为生产收益/完整链/Android认证。
