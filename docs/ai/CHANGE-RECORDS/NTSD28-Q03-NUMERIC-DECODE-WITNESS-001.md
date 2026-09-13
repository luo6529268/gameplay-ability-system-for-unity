<!-- CHANGE-RECORD
id: NTSD28-Q03-NUMERIC-DECODE-WITNESS-001
status: VERIFIED
change-kind: SOURCE_LINKED_NUMERIC_DIAGNOSTIC
code-path: Tools/NTSD28Q03Numeric/AuthorityNumericWitness.cpp
code-path: Tools/NTSD28Q03Numeric/Build-And-Capture.ps1
authority: Active user realignment goal; Q03 field contract; NTSD2.8-Logan formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable DatParser/FieldBag/CombatRecordDecoder.
evidence: VERIFIED_SOURCE_LINKED_NUMERIC_CAPTURE_ONLY / NATIVE_37_DOUBLE_RUN_STABLE / UNITY_SOURCE_37 / VALUE_DIFF_113_OF_333 / FLOAT_TYPE_111 / PRODUCTION_UNCHANGED / NO_UNITY_PLAY
-->

# NTSD28-Q03-NUMERIC-DECODE-WITNESS-001

## 事前合同

准确Task同ID。原状：Q03已测量float截断和整数准入差异，尚未取得完整native float32 bit及语法边界输出。改后：增加调用真实native decoder的离线见证，为Q05解析实现提供固定输入/输出；不重写decoder公式。

输入只读，native源码和正式EXE禁止写入；临时exe与报告只在workspace。工具关闭文件后进程退出，不接入runtime，不改变十一阶段。无版本或资源迁移。后续只用有身份的输出修订合同，测试通过不代表整个战斗对齐。验证/回滚与未验边界见Task。

## 实际实施与验证

事前状态：脚本尚未写；编译及capture待执行。

### 最终限定出口

实际新增两脚本与README、37份合成DAT/manifest/capture/report；native编译双跑成功且source/header/input稳定。原版输出SHA 77C8FF1D8BD71FF05EA462C99D8EB9B4712495477996C8EE148944779DD2C7F3；复用UnityContentCapture重编译0warning/error并实际37/37通过语法/转换，333值113不同，float类型111槽另列，负零3槽另列。不是Unity新decoder通过，生产未改。

实际命令、原始路径、数值表与风险见同ID artifacts/diagnostics/REPORT.md；Ledger472/45PASS。无需新runtime生命周期接入；没有Editor/Play/SelfCheck或schema迁移。Q03出口复核发现独立Oscillate reader/base-shell遗漏，回到父合同处理，不在本诊断包改生产。
