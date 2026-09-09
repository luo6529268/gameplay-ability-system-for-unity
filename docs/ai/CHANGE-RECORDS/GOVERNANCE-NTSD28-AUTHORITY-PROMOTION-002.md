# GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002 — Bug修复版正式权威晋升

<!-- CHANGE-RECORD
id: GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002
status: VERIFIED
change-kind: GOVERNANCE_AND_TOOL_IDENTITY
code-path: Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Tools/NTSD28Parity/TraceContract.cs
code-path: Tools/NTSD28Parity/B2InputRngJointRawContract.cs
code-path: Tools/NTSD28Parity/B2InputRngJointRawSelfTest.cs
authority: User explicit 2026-09-04 confirmation that current designated-root build is the intended NTSD 2.8-Logan bug-fixed authority and work should continue.
evidence: USER-CONFIRMED-BUGFIXED-AUTHORITY / EXE-B1E13AE1 / PLAYABLE-CPP-HEADER-CLOSURE-82-39DDDA15 / SOURCE-CAPTURE-MANIFEST-75-07CD47A0 / OLD-TOOL-73-SUBSET-5F2E5B41 / KIND-AND-MINIBAR-SOURCES-ADDED / CAPTURE-BUILD-0-8A5A96C1 / REAL-3-SCENARIO-9-STREAM-DATA-LINES-BYTE-EQUAL / THREE-VALIDATORS-PASS / DOTNET-BUILD-0-0 / TOOL-SELFTEST-49-OF-49 / FORMAL-HUMAN-AI-900-TICK-REPORT-SHAS-UNCHANGED / SYSTEM-DAT-33-3-REVERIFIED / B3-57-CONTRACT-REBASELINED / C02-C03-PLACEMENT-VERIFIED / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / USER-CONFIRMED / AUTHORITY-PROMOTED / B0-B3-IMPACT-CLASSIFIED`

## 决定

用户说明其发现并修复了 NTSD 2.8-Logan Bug，因此指定根中当前 EXE/source 的身份变化是有意发布，要求继续
执行总表。当前 `B1E13AE1...9033` EXE 与82-file `39DDDA15...6109` playable C++/header closure manifest 自本决定起正式取代
`1277B70B...DAF75` / `C59BD8D3...2D75`。最初记录的`5F2E5B41...5FA9`来自旧捕获脚本73-file
子集；按新版正式`build.ps1`补入`kind_catalog.cpp`和`minibar_catalog.cpp`后，完整75-file manifest为
`07CD47A0...778F`。后者是source-capture子闭包；包含renderer/audio/main等全部正式playable C++与headers的
完整82-file闭包为`39DDDA15...6109`。

## 已观察影响

当前 `simulation_tick_driver.cpp` 不只是二进制重建：至少改变了 horizontal impulse 相对第二次 clamp/
held refill/stage settlement 的位置，并把 display、reaction、armor、attacker rest、opoint、healing 等职责纳入
逐 live-slot 嵌套尾部。旧 B3 C18～C29 顺序表不能继续作为当前权威；需要重新生成 pass contract 和实际首差。

## 计划与结果

- 先更新所有当前恢复入口和 workspace 工具身份常量。
- 再完成 current source 的 B0～B3 影响矩阵。
- B3 pass contract/production sequence 若需变化，使用独立 test-first Change 实施并重新运行 compile、focused、
  SelfCheck 与必要 Play 验证。

authority 目录只读；本包不修改 Unity production battle behavior。

## 实际验证

- workspace捕获器第一次链接失败，原因是旧source list漏掉新版正式`build.ps1`新增的
  `kind_catalog.cpp`和`minibar_catalog.cpp`；补齐后C++构建通过，capture binary SHA
  `8A5A96C178A4F09A602B68A0D7CA5795A9D968B0E364FA8A99EA802220FE3BBB`。
- 新捕获header正确写入formal EXE `B1E13AE1...9033`与capture manifest`07CD47A0...778F`；
  common、standing-attack、AI三场景均各输出entity/domain/B2 input+RNG三条stream。除header身份外，9组
  3-tick数据行与旧artifact输出逐字节相等。
- common场景三类validator均PASS：entity 3 ticks/6 entities、domain 3 ticks、B2 3 ticks/6 entities；
  都保持`SOURCE_MODEL_DIAGNOSTIC_ONLY / certificate:false`。
- .NET build 0 warning/0 error；self-test `21+5+12+6+5=49/49` PASS。
- 当前正式EXE重新运行OID2 vs OID7 human/AI两个headless smoke，均exit0、passed、900 ticks；报告SHA仍分别
  `65D4FF97...359E0`与`2F845CBD...DABB0`，与旧artifact报告完全相同。
- `system.dat` SHA`D848CC6D...31AE2`仍明确`fps_value:33`、`fps_value_f5:3`。
- authority目录没有构建或输出；所有新产物只在workspace `Temp`。

## B0～B3影响矩阵

| 阶段 | 裁决 | 依据与后续 |
|---|---|---|
| B0 schema/binding/capture | `STRUCTURE_REVALIDATED / COVERED_SCENARIOS_BYTE_EQUAL` | 47-field及domain输出格式、3场景数据行未变；旧header身份已废止，后续证据必须用新header。reaction/rest等变化场景仍由B4/B5重证。 |
| B1 cadence/Host | `NO_OBSERVED_CHANGE_IN_COVERED_CONTRACT` | system.dat仍33/3ms，formal human/AI 900-tick报告SHA不变；既有OS physical B12与worker B9触发条件不变。 |
| B2 input/dual RNG | `SOURCE_AND_FORMAL_SMOKE_REVALIDATED` | common/standing/AI B2 raw数据行逐字节相等，工具49/49，formal human/AI报告不变；B11 content首差边界不变。 |
| B3 C00～C03/C01 | `REVALIDATED` | 新source仍input phase→spark→producer→route；C01 focused/SelfCheck/真实Play重证，C02/C03 production placement已验证。 |
| B3 C04以后 | `CONTRACT_CHANGED / REBASELINED` | physics normalize、type-separated hit、stage→impulse与16-step nested tail已改为57项contract；旧52项证书superseded，后续从FrameMotion/Cooldown首差继续。 |

## 关闭结论

identity drift等待已完全解除。当前恢复入口、工具常量与B3合同均指向Bug修复版；旧SHA只以明确历史标签出现。
后续若authority再次变化，仍须新建decision并重新计算两个manifest，不能按同名文件自动继承。
