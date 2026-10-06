# NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006 编辑审计

Operation NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006 / PLANNED。
执行者本会话Codex；授权为用户“开始执行下一批的任务”，承接子批12受控metadata热点。
类型：九个准确现有文件的追加/最小编辑；无删除/移动/整文件替换或Git丢弃。
路径/原字节数/SHA/Git状态见 [before](before.json)；保护393文件见 [protected](protected-before.json)。
七个dirty治理文档与两个clean自编代码均先保存当前字节到本目录backups，校验后才编辑。
实际范围：
- Assets/NTSD/Docs/android-mobile-readiness/m-03-dynamic-mesh-upload.md
- Assets/NTSD/Docs/battle-optimization-progress-tracker.md
- Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md
- Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md
- docs/ai/CHANGE-LEDGER.md
- docs/ai/STATE.md
- docs/ai/FILE-OPERATIONS/INDEX.md
- Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
- Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleDynamicMeshBackendSubMeshEditorTests.cs
新增Task、Change、报告和raw JSON为本Operation的审计证据，非运行时资源。
只能apply_patch准确追加；现有未提交工作属于用户，不能用HEAD恢复dirty。
恢复需另授权；准确恢复源为九个before备份，不执行reset/checkout/clean/stash。
测试只原Editor非Play具名EditMode；禁止额外Editor/切Scene/Play/完整M0/GPU/Android测量。
生产修改仅SetSubMeshes API调用策略，不改descriptor/range/bounds/vertex/segment/lease/11阶段关闭。
事前九个备份全部SHA一致。第一次改前job新bounds exact断言失败，未修改生产；
result=null原件保留，改测试浮点容差后重跑，不称生产RED。首次git diff--check四文档EOF空行，已窄修；
一次read-only错误定位独立segment类路径不存在，未写入，改rg按类型定位。
最终限定编辑审计VERIFIED：两个脚本确切差异为production4新增/1删除、fixture131新增/2删除；
九before备份均匹配，393范围外SHA保持、旧场景/资源/InputActions/Settings/EXT1/MONO/ATLAS与Q06 hash-only不变。
原Editorcompile/改前26/改后20/回归71均Passed，91去重、117成功执行；
首次测试exact浮点断言失败保留并修正fixture，非生产RED；没有恢复/删除/覆盖用户dirty。
script按Task范围apply_patch；生产NativeAPI批量不是segment合并/GPUBatching；runtime验收仍待。
operation只封编辑审计，不把生产Change RUNTIME_PENDING或Android状态升格；after清单待最终写入并冻结。
23:14最终窄验：九个before备份/393保护保持，160links0missing、compiled七文件0drift，
Validator1309records/6current-diff scripts/0error/本scope0warnings（全局历史4268保留）。
after.json将记录九owned、Task/Change、本Operation与本artifact所有新增证据的bytes/SHA/Git状态，
不包括after自身；写入后冻结仅只读复查，不因Operation审计关闭晋升RuntimePending。
首次after只读汇集对每文件执行git status耗时超过exec wait，返回session/空即时输出被误解析，
JSON解析失败、未写after也未改项目。改用一次全局status并在解析前保存session/检查完成；
只采用后续完整snapshot。原只读读者无文件写入，未重启Unity/test job、未删除任何内容。
