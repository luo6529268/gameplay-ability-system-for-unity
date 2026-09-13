<!-- CHANGE-RECORD
id: NTSD28-B11-CONTENT-ENTRY-INVENTORY-001
status: VERIFIED
change-kind: DIAGNOSTIC_TOOL_AND_CONTENT_AUDIT
code-path: Tools/NTSD28ContentAudit/AuthorityContentCapture.cpp
code-path: Tools/NTSD28ContentAudit/Build-AuthorityContentCapture.ps1
code-path: Tools/NTSD28ContentAudit/UnityContentCapture.cs
code-path: Tools/NTSD28ContentAudit/UnityDiagnosticStubs.cs
code-path: Tools/NTSD28ContentAudit/Audit-Content.py
code-path: Tools/NTSD28ContentAudit/Test-ContentAudit.py
authority: User start instruction 2026-09-13; D-023 DAT/character-image target; NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and unchanged playable parser/content sources; Unity current parser/converter input only.
evidence: OFFLINE_AUDIT_ONLY / Q01_DELIVERED / SOURCE_INPUT_HEADER_IDENTITY_PASS / NATIVE_405_UNITY_405_138 / CONVERTER_6_FILES_9_FRAMES_REJECTED_RECORDED / DOUBLE_RUN_STABLE / PYTHON_12_PASS / PROTECTED_3059_UNCHANGED / LEDGER_PASS / NO_UNITY_OR_PLAY_OR_RESOURCE_MIGRATION
-->

# NTSD28-B11-CONTENT-ENTRY-INVENTORY-001

## 事前合同

总目标与批次：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-01 / Q01`。
完整需求、准确文件范围、验收和回滚见同ID Task。当前是内容审计，不修改战斗runtime或非战斗逻辑。

原状：Unity正式内容未迁移；CPoint/OPoint/资源root/PNG等差异已有静态线索，完整正式catalog和实际normalized projection未形成可执行接入清单。
改后职责：workspace-owned工具读取正式/Unity两套内容，用原解析源导出诊断数据、建立映射和引用证据；不修改、导入或删除任一输入资产。
副作用仅为本workspace诊断工具编译、审计报告和治理状态更新。关闭事务不受影响；本工具是离线进程，文件流在进程退出前关闭，不接入battle runtime。

## 实际验证记录

### 最终出口（2026-09-13，覆盖下方试跑及待验状态）

本Change只认证离线诊断工具与Q01清单交付，状态VERIFIED不表示游戏runtime或B11已对齐。正式报告Q01-REPORT.md、report目录、native/unity capture、scope-and-consumers.md和workspace-protection-end.json构成完整证据。Q01/BATCH-01为DELIVERED；总目标仍ACTIVE。R15只完成Q01身份与字段可用性部分；Q05/Q07尚未触发。Q02-A精确只读Task已准备。

实际执行：PowerShell7 `Build-AuthorityContentCapture.ps1 -Mode All` 编译/fixture/405输入双跑PASS；`dotnet build ...UnityContentCapture.csproj --no-restore -v:minimal -clp:ErrorsOnly` 0warning/0error、`dotnet run ... --no-build -- --self-test` PASS；两源真实捕获405/138并各双跑。最终Unity新版hash B7F7E90A3D1DBA059CDDD87480C0B9BD78FB1EADD1A5C488B24C38DF70C068A8，旧版hash 52135192067DD87AFE76F33ED98E68336148CC6513C9DC467577BB100D6D2BBD；native仍5F5C6C34CE907BB6D7855B8A1E69D410B3CD577807F202332B8A0DA30ED0146F。

主工具实际执行完整README命令的 Audit-Content.py（report/report-rerun两个输出）；身份gate校验405输入、9source+31header及Unity26链接源/逐文件input root/hash，type纳入registry/catalog对账，覆盖405/405/138。Python12项PASS；ChangeLedger462records/6governedfiles PASS；git diff --check PASS（仅Git既有CRLF提示）。最终双跑receipt见inventory-stability.json。菜单98份原生文档405层图片声明单列NON_BATTLE；Unity真实AST未保留layer则明确UNAVAILABLE，不伪造解析成功。3059保护文件hash无差异。

已改路径严格为metadata六个脚本、同工具csproj/README/.gitignore、报告工件、Q01/Q02-A Task及五个治理文档。没有Assets代码/资源/Scene、ProjectSettings、第三方或生成代码改动；没有提交/push。未执行Unity编译、SelfCheck、Play、正式EXE战斗trace或实际资源迁移，这些仍由后继批次验收。

### 集成审阅追加（旧过程状态不覆盖本节后续出口）

- 实跑两端原parser/converter后复核发现：Unity typed dictionary必须保留DTO实际默认字段，不能再按raw出现字段过滤；已修正并用仅fronthurtact输入验证alias后injury与cover默认值。旧试跑hash不再是最终输入。
- 全量registry首次映射中24条Unity路径附带`#comment`而误报不存在，现与生产GameDataManager相同剥离注释并加回归用例；125同ID对象全部有实际DAT。
- native BDY输出只是parser typed field bag而非最终CollisionGeometry box；分类已降为PARSED_BODY_FIELD_REVIEW。ITR scalar/array差异独立分类并追到实际consumer，19,438×2值比较仅两项首差、23块因结构不同不配对；证据itr-consumer-projection-review.json。原生CPoint float32与Unity int的差异保留给Q03，不把浮点舍入当打印错误。
- 最后只读工具review要求修正身份、type对账、路径穿越归一化和menu-face遗漏：Audit现必须绑定native input前后hash、capture稳定receipt、编译source+31保守header；Unity每文件必须绑定actualRoot/input hash/mode/auditId及26链接源与stub hash；catalog把type加入对账且输出完整元数据；逻辑路径拒绝`..`/absolute；menu-face单列非战斗审查。不得手写raw parser冒充Unity AST，真实AST未保留就报告不可用。
- Python目前12项测试已实际PASS，包括上述输入/source/mode篡改拒绝；首次完整清单17工件双跑hash一致（inventory-stability.json），因review追加身份/menu报告需再全量重跑后才作为最终出口。
- native诊断最终重新`-Mode All`通过：405输入前后hash相同，输出SHA仍5F5C6C34...0146F、双跑稳定，9source+31header当前hash均匹配。Conservative header manifest不是playable82完整闭包证书。
- 工作区保护实际检查3059文件hash无变化；ChangeLedger最新PASS462records/6governedfiles。无Unity/Play/资产迁移。最终报告位于artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/Q01-REPORT.md；后继Q02-A只读Task已准备，尚未执行。

- registry只读运行结果：正式330 object/24 background与catalog完全一致、无registry解析拒绝/重复ID；Unity registry137 object。native全目录初步导出405文件，其中generic DatParser 402成功、3文件有6 error（data/frame/INKHUD.dat、INKHUD2.dat、data/resource.dat），全部非indexed object格式；必须按专门HUD/resource parser归属解释，不将generic parser报错当正式release缺陷。资源/背景辅助引用单列owner-review，不自动迁移或删除。
- ChangeLedger修正后实际PASS：462 records / 6 governed diagnostic source files，receipt为`artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/change-ledger-validation.txt`。Python工具当前6项自检PASS；完整typed export及inventory仍待完成。

- 首次实际ChangeLedger检查失败：csproj不是validator定义的governed脚本扩展；SDK生成的obj下三个`.cs`被列为未登记脚本。已将csproj保留在Task/工具配置清单而不作为code-path，并新增仅限本工具的bin/obj/__pycache__忽略规则（不隐藏任何手写源码、不改.git配置、不清理文件）；后续复跑validator。该失败属于本诊断工程配置，不是Unity战斗脚本错误。

- 工具集成审阅发现并要求在正式使用导出结果前纠正三项诊断实现问题：Unity Debug不可静默丢警告；unity模式Decryptor必须使用与生产manager相同的标准格式key；`normalized`必须来自真实Converter返回DTO，不能将原始string字段字典冒充typed projection。修正与重跑由工具owner执行，此前试运行不是正式Q01语义证据。native正常summary写stderr被PowerShell包装NativeCommandError也需区分进程exit与日志格式。以上只涉及新诊断工具，production不动。

- 主代理已新增`Audit-Content.py`：registry/catalog对账、两源typed差异、图片metadata/引用、旧资源GUID反查及保守处置；输出目录和资源读取边界检查。新增`Test-ContentAudit.py`，实际`python Tools/NTSD28ContentAudit/Test-ContentAudit.py`五项PASS（重复registry、PNG/native fallback、缺失零字段、重复frame、输出与输入隔离）。两端实际source exporter仍在独立工具范围实施，完整inventory尚未运行，不代表Q01已交付。

- 2026-09-13：实际`Get-FileHash`确认EXE身份匹配；读取现有rules/README/manifest/Git。当前有用户Foot Marker脚本、Scene/config和资源变更，全部保持。
- 当前未运行新导出、inventory、自检、validator、Unity编译/Play；不可声明Q01完成。

## 未验证与后继

Q02加载前置、Q03字段/schema合同须由本批结果精确拆分；R15在输入身份与导出字段合同闭合后回访。所有数据成功/拒绝和unknown都会如实报告。
