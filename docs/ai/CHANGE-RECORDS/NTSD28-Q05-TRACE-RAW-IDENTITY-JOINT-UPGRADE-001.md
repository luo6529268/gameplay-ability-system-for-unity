<!-- CHANGE-RECORD
id: NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001
status: FOCUSED_TEST_PASS
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
evidence: validation-counts.json; native/Unity actual content match; RAW_PARITY_DIFFERENT; SelfCheck/legacy Play PASS; REPORT.md
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

## 最终限定出口（追加）

# Q05 Trace / Raw / 内容身份升级限定出口

FOCUSED_TEST_PASS / SAME_CONTENT_CAPTURE_PASS / RAW_PARITY_DIFFERENT / Q05_REPLAY_PENDING。准确19脚本，最终SHA见final-code-scope.json。canonical trace/descriptor/comparison/validation/selftest已v3；Unity raw header/tick/comparison/selftest、authority source wrapper/validation已v2；native build manifest2.0。实体50字段、44已绑定、原6MISSING保持。13/21/24/2/2仍在同一未发布Q05窗口；不代表整场对齐或Q07资源已部署。

## 实际改动

- 新增combat.objectAiExcludedGroupSourceSlot，native与Unity均读各自真实独立2F8，未借用owner/spawner。生产2F8写入/AI消费仍属Q06。
- 内容头绑定policy/scope/profile/rawDefinitionSha256/decodeContract/semanticSha256/catalogFingerprint64/schemas；采用既有V2 byte contract。只声明catalog对象DAT定义的身份，不扩大到所有图像/音频/背景。原版与Unity自身源码/assembly SHA作为分别的来源证据。
- Unity Editor捕获可选实际Logan root，使用已有catalog/完整native configs；默认旧内容入口有独立legacy profile和实际文件hash，退休旧常量manifest。临时scope保存/恢复config、objects、registry、frame configs与publication身份，并在构造后半途失败时恢复。正式场景/资源/global入口未切换。
- 原版runner用系统BCrypt对实际catalog/DAT读到的bytes计算相同BinaryWriter/SHA身份，初始化前后/模拟后复核。hash对象buffer寿命覆盖BCryptDestroyHash。源树只读，候选工具仅建于Temp，不覆盖正式EXE。
- 内容不匹配、伪语义摘要/投影、旧schema在字段比较前拒绝；canonical CLI不再把content-strategy-pending当成功。Raw FirstDifference现在按最早tick、slot稳定选择，保留字段顺序作为tie；各字段差异统计仍完整。
- 实际同内容捕获揭示旧Editor bootstrap把scenario当前MP传给Initialize(maxHp,maxMp)的错误。仅Logan诊断路径按native battle_world.cpp current_mp=request.mp、base_max_mp=stats.max_mp/fallback映射，既有生产Initialize/资源规则和默认legacy诊断保持。没有用修改输出字段来掩盖差异。

## 新鲜验证

1. Tools初始25项中4个新RED失败；集成时Program旧状态consumer编译遗漏、sealed异常继承尝试均已纠正，源码/Record留痕。新50字段补齐synthetic fixture；共享contract异常转为invalid结果，未放宽输入。
2. 最终.NET工具共88/88：trace51、raw14、B0-domain12、B0 comparator6、B2 input/RNG5。包括旧tag、缺key、错raw/tag/SHA/LE/schema/type/profile、不同合法内容、最早tick反例、missing2F8拒绝。B0/B2独立schema保持。
3. Unity初始4 RED全失败；首轮46=45PASS/1旧C25版本断言已按当前13/21/24修正。MP bootstrap新增RED证明200误当500；最终49/49（含真实Logan root、恢复原配置引用）与独立构造失败恢复1/1，共50个不同测试通过。validation-counts.json准确记录49+1，不冒称单轮50。
4. 真实当前正式Logan输入：source runner3tick/6entity、Unity同一neutral-common-two-entity（seed682973786、stage23、OID2/7、无按键）。两端完整content对象相同：raw4EFE1D2A6A51C20742EA839CC5EAC2BA0D09EE9E4A5888E77C8AC35D4AA0C58C；semanticDB579550BCEC0039383BB421B0F62FB9C741FA2BFD30212059F329B8CADA4407；projection3900ECBC509557DB。native复跑字节级相同，实际header/源码/binary SHA已归档。
5. 独立native binding unit包含当前实际write_entity，输出2F8=-1/0/37且owner19，和Unity非默认测试共同证明字段接线。该unit不是游戏运行或正式EXE trace。
6. 实际旧Unity capture与正式native不同内容：raw CLI exit1/reason content-identity-mismatch/ticks0。旧authority v1负夹具exit1拒绝；由现有selftest实际builder导出的synthetic canonical不同内容也CLI exit1/ticks0，详见canonical-cli-exit-evidence，非运行时证据。
7. 完整SelfCheck：请求2026-09-13T13:34:49.1829552Z、结果2026-09-13T13:35:48.861458Z PASS。Unity最后error CS查询0；native/工具均真实编译。
8. 现有旧Unity内容的真实NTSD_Battle Play：tick5、对象4→4，两OPoint队列pending拒绝/不消费、空闲capture成功，正常退出；无dedicated worker，非正式330资源的完整Play或物理技能验收。退出后bridge状态文件有一次写入中的空JSON读取失败，重读同Editor成功，无重启/第二实例。
9. Scene isDirty=false/root14，旧SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f保持。3059保护项2930同/111既有或声明变化/18旧缺失，无新增缺失。任务外用户performance roadmap未触碰；用户在工作中自行提交至ac09a622，本任务未提交/推送。最终脚本scope由preimages/SHA核对，不能用当前git diff数量代替完整scope。

## 必须保留的实际差异

same-content-comparison.json为different，不是parity PASS：300字段出现38处差异、7类差异、43类一致。严格最早首差tick1/slot0 combat.runtimeStateCode是MISSING；另5个原MISSING同样保留。已绑定的最早数值首差为tick3/slots0与1 vitals.currentMp，native200/Unity201。最大MP初始错位已在诊断入口修正，不再归因于正式内容不同；剩余MP增量的具体规则/时点原因尚待Q06追踪实际consumer，不在本包修改生产战斗逻辑。初始8类差异与旧排序结果均另存，不覆盖旧事实。

## 下一出口

同窗口步骤4的trace/raw身份与字段工具现已可用；下一NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001，使用当前内容身份/版本完成有意义的capture→修改→restore→同seed/input重放、claimed/raw/slot/pool复用与关闭重入验证，再关闭Q05。不得以本次3tick raw diagnostic、6MISSING或单场Play声明总目标完成，也不得提前Q07。Q06继承上述MP差异和所有已有consumer待办。

命令使用dotnet run --project Tools/NTSD28Parity各self-test/validate/compare、Build-AuthoritySourceCapture.ps1（正常runner和独立unit）、已归档native-run-command.json、Python -X utf8 Temp/Goal13_bridge.py的Unity tests/Play/Console，以及现有SelfCheck request/result。全部未使用computer-use，未改非战斗、Unity/GAS架构、Scene/InputActions、正式资源、Gen/Plugins或外部Server。

最终审计：Change Ledger PASS，503 Records/当前0脚本diff（用户已自行提交脚本）；19脚本最终SHA及实际exporter源码SHA复核一致，diff --check通过。
