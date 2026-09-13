<!-- CHANGE-RECORD
id: NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001
status: IN_PROGRESS
change-kind: TRACE_CONTENT_IDENTITY_AND_FIELD_UPGRADE
code-path: Tools/NTSD28Parity/TraceContract.cs
code-path: Tools/NTSD28Parity/TraceComparator.cs
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Tools/NTSD28Parity/RawEntityCaptureComparator.cs
code-path: Tools/NTSD28Parity/AuthorityCaptureValidator.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
code-path: Tools/NTSD28Parity/TraceContentIdentity.cs
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28TraceContentIdentity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TraceIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Tools/NTSD28Parity/Program.cs
code-path: Tools/NTSD28AuthorityTrace/trace_binding_witness.cpp
authority: D-023; Q03 VERSION-IDENTITY-AND-CAPTURE-CONTRACT; current playable ObjectDefinitionCatalog28 and entity2F8; Q05 semantic identity.
evidence: prechange.json / TEST_FIRST
-->

# Q05 Trace / Raw / 内容身份同窗口升级

IN_PROGRESS / TEST_FIRST / INTERMEDIATE_UNPUBLISHED_Q05_WINDOW。准确17脚本见metadata。当前生产snapshot/checksum13/21/24/2/2已限定验证，本包不改变其载荷/规则。更新canonical trace及descriptor/comparison/validation/selftest到v3，Unity raw header/tick/comparison/selftest与authority source wrapper/validation到v2，build manifest2.0。B0-domain/B2-input-RNG独立schema保持。

新增combat.objectAiExcludedGroupSourceSlot：native直接读object_ai_excluded_group_source_slot_2f8，Unity直接读ObjectAiExcludedGroupSourceSlot2F8，50字段/44 bound/6 missing。+2F8 carrier/copy/reset/restore已有证据；Q06生产者/AI未接，binding不是behavior对齐。原6MISSING不动。

内容对象精确键：policy、scope、profile、rawDefinitionSha256、decodeContract、semanticSha256、catalogFingerprint64、schemas。policy=logan-dat-character-images；profile=logan-runtime时scope=catalog-object-definitions，decodeContract=NTSD28_LOGAN_DAT_SEMANTICS_V2，语义SHA按既有tag+NUL+raw32，投影16位大写LE ulong hex（0→1），schemas=entityRuntime13/aggregate21/checksum24/characterShell2/entityBaseShell2。profile=unity-legacy时scope=unity-legacy-dat-files，明确专用NTSD28_UNITY_LEGACY_DAT_SEMANTICS_V1，不冒充正式Logan内容。raw legacy定义为BinaryWriter UTF8字符串NTSD28_UNITY_LEGACY_DAT_FILES_V1、实际data.txt SHA、Config目录所有DAT按Ordinal相对正斜杠path排序的path/SHA；读取前后复核。

以上身份域只承诺catalog对象DAT定义与解码合同，不声明独立背景/场景/音频/图像文件全部已对齐；scenarioDataSha及独立视觉输入证据保持。native source、Unity runtime/editor assembly分别是来源证据，不互相要求相等。content对象必须验证exact keys、tag、SHA自洽、LE及schema；不同profile/raw/semantic/版本在实体比较前明确拒绝，退休content-strategy-pending。canonical authority header只准Logan，Unity legacy仍可作为可识别诊断输入；certificateEligible=false保持。

Unity exporter新增可选Logan runtimeRoot及测试入口，用既有LoganObjectCatalog/BuildCharacterFrameConfigsFromCatalog实际读取全部候选，并在现有temporary scope接入真实objects/registry/config引用，结束完整恢复。正式global资源不切换、无texture/atlas加载。保留默认legacy诊断入口，常量旧manifest改为实际读取指纹。补原scope构造失败时没有Dispose的恢复缺口：所有原引用准备好后才进入mutation try/catch；registry/publication identity/key原引用同样保存恢复。新内容helper为Editor-only，不向Core增加文件I/O。Unity header增加实际runtime assembly SHA。

native runner用系统BCrypt SHA256（build仅加系统-lbcrypt，不引入第三方库），现有ObjectDefinitionCatalog28读取相同resourceRoot、按registry_index排序与实际readable_dat_path计算既有BinaryWriter定义指纹；startup/session初始化后/输出结束复核内容不变，拒绝明显源漂移。正式authority源码保持只读，hash帮助函数留同runner cpp让其source SHA覆盖。捕获仍SOURCE_MODEL_DIAGNOSTIC_ONLY，不是正式EXE证书，也不声称文件系统原子快照可抵御任意并发篡改。

test-first：field50/+2F8独立和旧tag/缺语义头反例，再完整helper规范向量/伪SHA/tag/LE/schema/missing/profile拒绝、真实native/Unity同root capture与first difference。原字段值/输入/RNG/slot/关闭流程不得顺手改动；有首差保留由Q06回访。跑.NET self-tests、C++诊断构建（仅Temp）、相关Unity EditMode/SelfCheck和已有场景检查/保护/ledger。未证实部分保持IN_PROGRESS，不提前Q07或发布baseline。无新Runtime manager/queue/worker，复用既有Editor scope按driver先关闭/data后恢复，十一阶段不变。

保持Unity/GAS/非战斗/Scene/InputActions/资源/Gen/Plugins/外部Server与所有例外；禁止computer-use。回滚须批准按preimages准确差量，不覆盖已有工作/旧capture。后继父Q05完整restore/replay/Play仍待。

## CLI退出码consumer补充（事前）

首次工具集成编译指出Program.cs仍把ContentStrategyPendingStatus视作成功退出。准确scope增至18脚本；仅移除该已退休成功分支，内容不一致必须非零，不恢复旧状态以兼容编译。原编译失败记录保留，接下来复验self-test与CLI不同内容退出码。

## 非默认native字段见证补充（事前）

准确scope19：新增独立trace_binding_witness.cpp，仅包含当前runner并改名其wmain后调用同一write_entity，初始化-1/0/37的2F8与不同owner19输出三行JSON。用现有Build-AuthoritySourceCapture -RunnerSource独立Temp目录构建；该见证不是实际战斗/正式EXE capture，单独保存runner与include源码SHA，不将自测manifest冒充正式runner来源。无需向正式Scenario或native runtime植入debug字段、修改producer或authority源码。

## 当前集成证据与旧测试consumer

Tools RED4已通过新版51/51，raw13/13；Unity RED4留证，首轮46=45PASS/1旧C25 schema断言12/20/23失败，真实Logan capture及配置引用恢复PASS。C25文件已在本Record准确scope（raw44）；现仅同步其跨行schema断言为现有13/21/24，生产版本不再改。首次工具编译遗漏Program退出码、尝试派生sealed InvalidDataException的编译错误已纠正为共享IsContractFailure过滤器；旧/缺entity字段应返回invalid而非异常逃逸。native正常capture已计算相同4EFE raw/DB57 semantic/3900 projection；完整首差和SelfCheck仍待。

## 实际同内容比较发现的诊断边界修正（事前）

真实raw300字段出现8类差异，其中6为保留MISSING，currentMp201/200与baseMaxMp200/500。已追踪：Unity exporter CreateCharacter调用Initialize(source.hp,source.mp)，但该生产API参数是maxHp/maxMp；正式battle_world.cpp:1275/1276分别current_mp=request.mp、base_max_mp=definition.stats.integer(max_mp).value_or(request.mp)。因此先修本已声明Editor exporter的Logan-only场景映射：用已解码NativeMetadata.Stats max_mp/fallback初始化max，再设current MP/PP为scenario.mp；默认legacy路径保持，不改生产Initialize/资源规则。不把此bootstrap错位登记为已复现生产战斗bug。先给真实capture加base500/current200 RED断言再改。另RawComparator FirstDifference目前按field契约序列取第一项而非最早tick，需在当前声明文件改为按firstTick、firstSlot稳定排序，保留字段顺序作同tick tie；加两tick反例。原比较/原Unity输入捕获单独保留。

## 当前已写/测量（完整出口前）

19脚本已接：Tools8个C#含CLI/新identity、native runner/build/独立binding witness、Unity raw projection、Editor exporter/helper/新tests及4旧count/schema消费者。Trace51/51、raw14/14（含首差时序RED后修复）、B0-domain12/12、B0 comparator6/6、B2 input/RNG5/5，共88项；Unity正式复验49/49，另构造失败后的恢复用例正在最窄验证。native真实3tick/6实体、实际DAT raw4EFE/semanticDB57/projection3900与Unity完全一致；nondefault2F8实际输出函数-1/0/37见证通过，owner19独立。实际legacy输入在比较前content-identity-mismatch、CLI exit1/ticks0，旧source wrapper v1负夹具也exit1拒绝。校正Logan-only MP bootstrap后，same-content comparison为7类差异/43类相同：严格首差是tick1 runtimeStateCode缺绑定；已绑定的最早数值差为tick3 currentMp native200/Unity201，原6MISSING保留，不据此更改规则。完整SelfCheck/最终Play/保护验证仍待，仍非完整对齐。
