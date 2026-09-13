# Q02 E1：正式目录与完整配置候选

状态：VERIFIED_CATALOG_AND_CONFIG_CANDIDATE_GATES_ONLY。此包不发布全局内容、不切换菜单资源，不代表正式新版内容已可用。

## 原生依据与入口差异

正式 EXE SHA-256 为 B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033，build 脚本已实际校验。

正式 playable 的 ObjectDefinitionCatalog28::load_extracted_root 读取 catalog.csv，校验六个必需列、严格整数、重复 ID/index；发布目录存在时要求恰好一个顶层 DAT，否则使用 decoded_dat/source_path。GameSession28 初始化在目录 load.success=false 时终止，不使用部分目录继续启动。

旧 Unity ParseCharacterFrameConfigs 从 data.txt 读取且跳过失败 DAT，返回部分字典。缓存命中会直接触发任务完成，跳过 Execute，因此完整迁移还需 E2/E3 的来源/发布与任务缓存约束。

## 本包实现

- LoganObjectCatalog.Read 捕获目录、对象 DAT 文本与 SHA-256，按 registry_index 排序；条目为不可变属性，列表只读。它认证目录与可读输入，**不把当前 Unity parser 的宽松性冒充原生 DAT 语法验证**。
- 目录 CSV 按原生单行解析规则处理 BOM、CRLF、引号、重复列覆盖和严格整数；非 object 行跳过。缺列、重复项、缺文件和多 DAT 发布目录不返回目录对象。路径通过 BattleContentSource 的包含性检查，作为 Unity 资源根约束。
- DefinitionFingerprint 只覆盖 catalog 和对象 DAT。SourceCacheKey 另绑定来源根；它不宣称包含图片、声音和完整资源依赖。捕获后保留原文本，不在稍后的配置构建中重新读取漂移文件。
- manager.BuildCharacterFrameConfigsFromCatalog 使用目录自带的 source 和新 Logan parser/builder 构建全部配置。任何条目失败都会聚合异常，不返回部分可发布字典；既有 legacy 入口、global lookup、pending/config/sprite publication 均未修改。

## 原生对照与测试记录

Tools/NTSD28Catalog/NativeCatalogProbe.cpp 直接链接正式 object_catalog 及其所需源闭包；Build-NativeCatalogProbe.ps1 记录 compiler、10 个源文件（含诊断入口）、31 个保守头文件和 probe EXE 指纹。工具不修改 authority 或旧 Q01 捕获。输出明确标为 SOURCE_MODEL_DIAGNOSTIC_ONLY，不等价于完整发行 EXE 运行证书。

实际原生加载 **330/330 成功**；所有条目实际采用 decoded_dat 路径。native-formal-catalog.json 保留排序后的 registry_index/id/type/source_path/published_folder/dat_path；formal-input-before.json 冻结 catalog 加 330 DAT。

RED job 37693c0dfc9346c79006b6e4ea89c487：14 项因新接口缺失失败。首次实现后 job b1e46395eff242aa9dd8de510f28f116 完成39项；正式330目录对应、内容/root身份和旧路径测试已通过，11个失败属于测试反射包装与混合分隔符断言。已改直接调用/规范化路径，原结果保留 green-result.json，不因文件名称为green而宣称通过。最终job d83c695fa2e94d55a87ae8a6a4f2db6d：39/39 PASS（本包15、既有路径24），原始结果final-result.json与verification-summary.json；正式6失败gate按预期抛异常，未返回partial。

实际配置构建已揭示并拒绝已知六文件：OID2 c/nar/nar.dat 的 WPoint dircontrol；OID65 c/ank/ank.dat 的 effect；OID870 c/ank/ssnk.dat 的 7；OID27 c/min/min.dat 与 OID449 c/min/sag.dat 的 CPoint drain；OID63 c/hir/hir.dat 的 WPoint dircontrol。这是Q03必须处理的正式接入缺口；负向验收通过只证明拒绝部分发布，不能证明新内容整体成功。

## 后继与保护

E2 在同一个候选身份下准备 config/object lookup/sprite/UI staging，并在无await提交段统一发布；不要让 IsPrewarmCompleted 在头像准备之前为true。E3 再处理固定 cache key、重复/取消请求、menu/test/battle caller 的最小接线。全正式迁移留Q07，先满足Q03/Q05/Q06；不忽略或填零六个Converter失败。

本包仅加载期managed数据，无World/worker/renderer/pool新owner，不改变shutdown序列、GAS或snapshot12/20/23。UI选择/随机/排序与旧资源未改；测试只创建Temp夹具和无窗口native子进程。源指纹/保护/Scene/CS0/Ledger最终job d83c695fa2e94d55a87ae8a6a4f2db6d：39/39 PASS（本包15、既有路径24），原始结果final-result.json与verification-summary.json；正式6失败gate按预期抛异常，未返回partial。

实际命令：`Tools/NTSD28Catalog/Build-NativeCatalogProbe.ps1`；既有Temp/Goal13_bridge.py调用refresh_unity、run_tests(EditMode，NTSD.Test.NTSD28B11LoganCatalogEditorTests与NTSD.Test.Editor.NTSD28B11ContentSourceEditorTests)、get_test_job、read_console(error CS)、manage_scene(get_active)；Python hashlib只读核对；Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path。下一NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001 / READY_CONTRACT，图像内容identity和原子发布不能被E1定义fingerprint代替。
