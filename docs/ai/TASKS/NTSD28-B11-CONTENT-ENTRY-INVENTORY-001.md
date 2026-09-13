# Q01 / BATCH-01：正式内容接入清单与兼容性检查

- Parent：`NTSD28-UNITY-BATTLE-REALIGNMENT-001`。
- Queue / batch：`Q01 / BATCH-01`；回访：R15，后继Q02/Q03。
- 状态：`DELIVERED / VERIFIED_OFFLINE_AUDIT_ONLY`；用户于2026-09-13启动，本批实际完成只读清单和两端诊断验证。资源迁移与Unity验收未执行；最终证据见同ID Record及Q01-REPORT.md。
- 目标：用当前正式indexed catalog和两端实际parser/converter，产出资源与数据接入缺口及后继最小包。不能用旧DAT数量/单行grep或旧2.4工具代替。

## 权威与范围

正式根：`J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan`；EXE SHA-256必须为`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。
读取`source/README_SOURCE.md`、playable build closure、正式runtime catalog/data.txt及DAT/图片。DAT/角色图片目标为D-023；旧Unity138-DAT仅为迁移前基线。
允许写入仅限下面诊断工具、文档和审计工件；禁止修改Assets下任何脚本、DAT、图片、Scene、Prefab、importer、ProjectSettings、共享kernel或authority文件。已有Foot Marker相关修改属于用户工作，保护不动。

## 工具与文件所有权（写脚本前声明）

- 主代理：`Tools/NTSD28ContentAudit/Audit-Content.py`、`Test-ContentAudit.py`、`README.md`、`.gitignore`（仅本工具bin/obj/__pycache__生成文件），本Task/Record、Ledger/STATE/handoff/总表/恢复入口。
- Authority诊断导出：`Tools/NTSD28ContentAudit/AuthorityContentCapture.cpp`、`Build-AuthorityContentCapture.ps1`；只编译读取当前authority源，不写authority目录。
- Unity诊断导出：`Tools/NTSD28ContentAudit/UnityContentCapture.csproj`、`UnityContentCapture.cs`、`UnityDiagnosticStubs.cs`；链接实际生产parser/converter源，必要stub仅用于Unity外围类型，不重写解析/转换规则。
- 输出：`artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/`下JSON/CSV/TSV/Markdown与hash index；临时编译/fixture在`Temp/NTSD28ContentAudit/`。SDK/Python也可在本工具目录产生bin/obj/__pycache__，仅由本工具.gitignore排除，不作为手写源码或交付证据。工具可重跑，禁止删除输入/已有未知输出。
- 任何额外源码写入先修订本Task与Record的准确路径；不同worker只写自己声明的工具文件和分属输出，不修改共享治理文档。

## 必须产出

1. EXE、输入资源/catalog、实际编译源闭包和Unity source身份；开始/结束保护检查。
2. 全目录与indexed对象/背景/非对象DAT分开统计；catalog重复ID/缺失/冲突显式报告。
3. 正式DAT结构和Unity当前parser/converter的成功/拒绝/丢字段、默认/alias差异；单行与多行subblock同口径。诊断source model不等于EXE运行证书。
4. 按object ID与正式source path建立旧→新DAT映射，识别新增、同ID变化、冲突，禁止basename猜配。
5. 角色/技能相关图片、头像/small、共用效果与其他资源引用闭包；路径、hash、尺寸/格式、当前GUID及可观察引用者。
6. 旧资源删除候选与必须保留/待审集合；发现引用不完整时标REVIEW_REQUIRED，不自动提升为可删除。
7. Q02/Q03的具体脚本差异、最小包、前置及触发R项；所有未知项保留，不以0/null补证。

## 验证与退出

- 新工具编译/自检及有意义的fixture验证（多行、重复ID、缺失、PNG路径、解析拒绝/未知字段等）。
- 实际运行两端诊断导出和inventory，记录输入/输出hash及失败，能重跑验证确定性。
- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>`与`git diff --check`；非本批现有失败单独报告不顺手修。
- 禁止启动/修改Unity场景或执行战斗Play；本批不声明运行时对齐。完成文档/清单/兼容性结果后才标本Q已交付，总目标仍未完成。
- 回滚方式：本批输出与工具均独立新增；如需撤回只按本批精确文件/差异处理并取得适用删除授权，不回退用户工作或authority。

## 当前执行

Task已在工具脚本写入前建立。初步确认正式catalog包含330条object、24条background；该计数仍需按正式registry及重复/启用规则复核，不能当最终可达数量。Unity旧headless-runner指向2.4，禁止复用其规则作为本次authority。
