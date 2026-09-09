# NTSD28-B2-FORMAL-EXE-HEADLESS-INPUT-OBSERVATION-001 — 正式EXE无窗口输入观察

<!-- CHANGE-RECORD
id: NTSD28-B2-FORMAL-EXE-HEADLESS-INPUT-OBSERVATION-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: Root formal NTSD2.8-Logan.exe SHA 1277B70BA030A1F33B625EEA20B43834325B280CEC555650BF43CD90A64DAF75 built-in headless smoke path.
evidence: TASK-CONTRACT-CREATED / FORMAL-EXE-SHA-1277B70B-VERIFIED / P2-HUMAN-HEADLESS-EXIT0-PASSED-900-TICKS / P1-P2-SEVEN-KEY-MASK-127 / P1-P2-ACTIONS-AND-NO-INPUT-DIVERGENCE / P2-AI-HEADLESS-EXIT0-PASSED-900-TICKS / RESET-QUEUED-DISCARD-FULL-CLEAN-IDLE-CLEAN-REATTACK / HUMAN-REPORT-SHA-65D4FF97 / AI-REPORT-SHA-2F845CBD / AUTHORITY-INVENTORY-UNCHANGED-71275342-2809-FILES-110386049-BYTES / PER-CALL-RNG-AND-EXACT-FIELDS-NOT-EXPOSED / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / FORMAL-EXE-HUMAN-AND-AI-HEADLESS-PASS / INPUT-RESET-OBSERVED / PER-CALL-RNG-NOT-EXPOSED`

## 计划

- 冻结formal EXE SHA与authority树只读摘要。
- 将human/AI两个headless smoke报告写到workspace Temp，读取退出码与报告字段。
- 复核authority树摘要未变化；计算报告SHA。
- 只把正式EXE能直接观察的输入/reset/native-AI smoke记为证据；逐次RNG/exact字段继续诚实保留边界。

## 当前事实

- source-model joint trace已证明common/standing全等、AI ticks1—2 exact+RNG全等；AI tick3首差归B11内容下游。
- 根formal EXE source入口确实解析`--headless-smoke`、`--p2-human`、`--p2-ai`和workspace report路径，
  报告schema为`ntsd28-playable-headless-case/1.1`。
- 立项时尚未运行；实际执行与结果如下，状态只按报告和exit code推进。

## 实际结果

- formal EXE SHA precheck通过。使用`Start-Process -WindowStyle Hidden -Wait -PassThru`分别运行human/AI，
  两次process exit code均为0，报告`passed:true`、`failureCode:0`、battle ticks900。
- human：P1/P2七键mask127、动作应用367/307、位置改变及与no-input control分歧均成立；reset queued
  input discarded、P1/P2 full input clean、idle clean、post-reset attack sampled均为true。
- AI：正式报告`p2NativeAi:true`，P1输入合同通过；双方reset clean与post-reset attack sampled通过。
  报告没有把AI内部逐次按键/RNG导出，因此只记native-AI smoke通过，不伪造内部字段证书。
- human/AI报告SHA分别为`65D4FF973644DA3618C13B4C8AB249D372132B29AE89C47EEED5A232805359E0`
  与`2F845CBD36BB8E9C18A92F0CC827FFE093A1F70C0DD0E8F5033653708EEDABB0`。
- authority树运行前后清单签名同为`71275342A4E21EF0C9472008CC6B7DDA6CF2DCB38131353C1561EDF0081E3D87`，
  文件数2809、总长度110386049、最大mtime不变；authority零写入。

## Git / 交接

- production代码：零修改。
- authority：只读，禁止生成任何文件。
- validator：`PASSED / Records 150 / governed code files 104`；本包`code-path: NONE`。
