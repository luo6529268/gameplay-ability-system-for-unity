<!-- CHANGE-RECORD
id: NTSD-OPT-M03-DYNAMIC-DISPLAY-016
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs
authority: user next optimization batch; presentation-only dynamic validation; formal336B44 unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH16-DYNAMIC-DISPLAY-20261007/REPORT.md
-->

# M-03 动态显示回归验证（第16批）

最终审计：Validate-ChangeLedger exit0/1312 Records/9 governed scripts覆盖/0error、本Change0warning（全库4260历史warning）；8/8 before备份与582保护SHA零漂移。34项=高12/中14/低8；2907本地链接全扫20旧错误均事前已存在，本批新增错误0，不扩范围改历史链接。git diff --check exit0、HEAD/staged不变，原EditorPID19040/6401，formal336身份保持。 文件after冻结仅收尾文件审计，metadata仍RUNTIME_PENDING，生产/父项不晋升。

SCOPED_DYNAMIC_PUBLICATION_PASS：原saved Battle自然240tick/308camera，21次实际actor坐标变动、1183snapshot身份/帧号/可见性比对，18新增身份/16离开publication、最多8实体/12命令。全部body command只来自当前publication，生产没有新改动；307Build/308request（一个既有sameUnityFrame gate），0failed/rejected/growth，camera/observer记录各0B。11个有body的身份首个可见publication与body同一观测tick；9个oid518无body，不据EntityVisible旗标推导其应有body，专项资格/first-visible/GPU未知不晋升。关闭objects/slots/borrowers0、Scene clean/SHA同；原Menu恢复。数据只观测camera采样publication，不覆盖漏过的中间tick、完整chain/1000AI/Android或正式EXE/透明像素。metadata及父M03/H11继续RUNTIME_PENDING；Task子批限定通过。

FOCUSED_TEST_PASS：8b5e5c239f6e414faea04a1129ef9f42，50/50具名Passed/2.2119742s（Deferral14/Latest13/Capacity16/pure motion3/Shutdown4）。非两个历史UnityTest或全部9256测试。事前Ledger exit0/1312Records/9scripts覆盖/0error，本Change0warning；全库4260历史warning。准备唯一saved Battle动态窗；新增observer断言实际运行待证。

原Editor compile/error CS0；生产DLL仍A49C69CE...，新EditorDLL5B764BC3.../6188032。首次reload两读与test请求均只返回retry（未启动job），等待完成后才正式请求focused。new harness运行/动态覆盖待证，不晋升父项。

首次编译两诊断：缺NTSD.Game namespace，RuntimeEntityHandle.Generation实际uint。只修Editor探针using及诊断record字段类型，未改生产API；原件initial-compile-state.json保留。移除未使用skillTick，运动计数改按实际actor source X/Z变化而非排序后首实体位置，避免排序变化制造运动假阳性。重新编译待证。

CODE_WRITTEN：唯一Editor探针追加batch16独立菜单/240tick动态键序列、128身份/2048样本硬界、publication snapshot与body command只读核验、覆盖字段和关闭前释放键。旧14/15模式默认为96tick；生产未编辑。尚未编译/测试/Play，不称生成/退休或0GC通过。

事前PLANNED，尚未编辑脚本、编译或运行。Task NTSD-OPTIMIZATION-BATCH16-DYNAMIC-DISPLAY-20261007。
[Task](../TASKS/NTSD-OPTIMIZATION-BATCH16-DYNAMIC-DISPLAY-20261007.md) / [Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH16-DYNAMIC-DISPLAY-20261007/RECORD.md)。

仅扩展现有Editor探针：独立batch16菜单/唯一输出，沿用原saved Battle启动、自然tick、
camera begin/end只读观察与11阶段关闭。增加正常InputSystem按键移动/技能序列，
观察生成/退休及实体snapshot/command身份，逐相机核验latest publication，记录覆盖不足。
不写production runtime、World真值或资源，不改逻辑/插值/排序/segment/slot。
新诊断存储启动时固定容量，禁止观察中隐式扩容/截断；输入事件产生的Editor分配单独标明。
无新runtime owner：输入在正常Shutdown之前释放，观察随已有退出回收。
具体范围/副作用/验收/恢复见Task；0GC/透明像素/first-visible/正式EXE/设备未知不晋升。

