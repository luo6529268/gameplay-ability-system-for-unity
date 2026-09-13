<!-- CHANGE-RECORD
id: NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001
status: VERIFIED
change-kind: NATIVE_CATALOG_AND_ALL_OR_NOTHING_CONFIG_CANDIDATE
code-path: Assets/NTSD/Scripts/Animation/LoganObjectCatalog.cs
code-path: Assets/NTSD/Scripts/Animation/BattleContentSource.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11LoganCatalogEditorTests.cs
code-path: Tools/NTSD28Catalog/NativeCatalogProbe.cpp
code-path: Tools/NTSD28Catalog/Build-NativeCatalogProbe.ps1
authority: User D-023/active goal; formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable object_catalog.cpp load_extracted_root and game_session.cpp initialize 1180 failure gate.
evidence: VERIFIED_CATALOG_AND_CONFIG_CANDIDATE_GATES_ONLY / RED_14_FAIL / UNITY_39_OF_39 / NATIVE_330_OF_330 / SIX_CONVERTER_FAILURES_REJECT_PARTIAL / CS_0 / SCENE_CLEAN
-->

# Logan目录及完整配置候选

准确计划见父Task NTSD28-B11-CATALOG-PUBLICATION-CONTRACT-001的E1。原状：Unity只读旧data.txt且解析失败continue，不能表示正式registry_index或全量newsource成功。

实现范围：新LoganObjectCatalog.Read(source)按native CSV语义/required columns/int解析/重复ID与index/发布目录优先且恰好1个top-level DAT/portable fallback读取元数据与DAT字节；排序registry_index；持有不可变source、entries、DAT文本/hash和DefinitionFingerprint。此fingerprint只包含catalog与对象DAT，不宣称完整图片/音频identity；SourceCacheKey另绑定root。Read只认证目录/可读输入，不伪造native DAT parse validity，正式source-linked probe独立证明原生全量接受。

BattleContentSource新增ResolvePublishedFolderPath，复用既有包含性验证；不拓宽现有source路径。manager新增静态BuildCharacterFrameConfigsFromCatalog，消费catalog内同源原始DAT经现有BuildCharacterDataFromSource，设置既有type_sub/OID并包装；任何失败聚合异常且不返回部分字典，不写pending/global。legacy ParseCharacterFrameConfigs不改。

Tools/NTSD28Catalog新probe仅链接实际object_catalog及所需源闭包，输出success/objectRows/entriesLoaded/排序entry/diagnostics；不改authority或旧Q01 capture。Build脚本记录compiler/源/header/EXE指纹，输出只在Temp/NTSD28Catalog与本包artifact。

验收：RED新API缺失；CSV quoted/CRLF/BOM/missing/duplicate/严格整数/registry顺序；published folder单DAT优先、多DAT拒绝；portable missing/path containment；identity对root与DAT修改响应，条目和元数据不可变；实际330正式条目与native probe路径/id/type/index对应；新源全配置构建对正式6文件9帧拒绝不返回partial，小型valid fixture成组成功。旧Q02路径/range/alpha相关窄回归、编译/Scene/保护/Ledger。Native invalid fixtures对照以success/failure为门槛，具体parser permissiveness不由新reader伪造。

副作用只有本地文件读取和加载时managed配置分配，无manager、worker、pool、World、publication owner和新增shutdown阶段。测试仅Temp夹具/子进程（CreateNoWindow），不构造manager。字段不进snapshot，12/20/23不变。新native config入口尚不等于全局发布；E2/E3和Q07为明确后继。回滚为本包精确diff，保留Q01/Q02用户工作；不得破坏性Git/删除旧资源。

## 验证记录

Task/Record先于所有脚本；RED/GREEN/native probe待执行。

写后登记：新增LoganObjectCatalog及不可变Entry/Read/DefinitionFingerprint/SourceCacheKey；BattleContentSource.ResolvePublishedFolderPath；manager.BuildCharacterFrameConfigsFromCatalog；新增NUnit与native probe/build脚本。Native实际330/330，331个catalog/DAT输入hash已冻结；RED job37693c0dfc9346c79006b6e4ea89c487 14项执行，全部因缺少新reader/API失败。GREEN/正式6失败gate/保护仍待验证。既有global读取与publish路径未改。

## 最终验证

本包限定出口VERIFIED_CATALOG_AND_CONFIG_CANDIDATE_GATES_ONLY。Native330/330、Unity最终39/39（15+24，jobd83c695fa2e94d55a87ae8a6a4f2db6d），正式6文件错误明确聚合拒绝partial；小型valid候选完整通过。首次实现后的测试反射包装/路径分隔符断言已改直接调用和规范化路径，不把测试失败当生产行为已修复。CS0/Scene dirtyfalse/331input+41sourceheader+EXEhash保持、3056既有文件原样/3声明变更/零缺失、Ledger467/23PASS。准确证据/命令/未验项见artifacts/diagnostics/NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001/Q02-E1-REPORT.md。E2 NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001已READY_CONTRACT；E3/Q03/Q07继续，未发布global/迁移资源/运行全场Play或SelfCheck。
