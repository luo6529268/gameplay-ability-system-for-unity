<!-- CHANGE-RECORD
id: NTSD-OPT-H07-RENDER-TEXTURE-BINDING-REUSE-067
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralSameSamplePixelEditorTests.cs
authority: approved effective six-stage Task0-8; current RenderPass identical texture-property preparation reuse only; formal336 invariants unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH67-RENDER-TEXTURE-BINDING-REUSE-20261008/REPORT.md
-->
# 第67批：中央segment纹理属性准备复用

PLANNED。66真实NO_GAIN已闭合，Goal仍active，本批转现有中央提交CPU操作的有界资格，不据批数停工、不改逻辑33/3ms/max2或降低H07门。Task NTSD-OPTIMIZATION-BATCH67-RENDER-TEXTURE-BINDING-REUSE-20261008.md完整读取；原Editor19040/Menu1savedclean8roots/idle，当前源/dirty及266guards重新扫描、无66保护漂移。

## 准确变更与符号

TESTS_WRITTEN增量：25纯资格＋2Cost写入原pixel fixture；旧6用例/输出/断言未改。冷反射绑定使缺helper能正常编译并产生有效RED。写后重扫实际enum为Source0/Array1/Page2，新fixture的mode标签沿旧pixel测试Source0/Page1/Array2，已显式映射命名枚举而非强转；此修正在首次import/测试前，不是生产语义变化或失败豁免。尚无候选C#、RED或收益结果。

原Editor19040准确import后经历一次47.213s domain reload，6402两次connection-refused不是关闭/失败，PID保持且原端口恢复。fresh idle核实后discovery实际27case＝25纯＋2Cost，未以编译失败作RED；纯25唯一作业df6f0b9d2a5444498e4265ace5fc4844已启动，terminal待观察。Feature仍初始SHA，未执行成本/实景/GPU采集。

- BattleRenderFeature：新增默认false静态EnableSegmentTextureBindingReuseForDiagnostics及LastSegmentTextureBindingPrepareCountForDiagnostics/LastSegmentTextureBindingReuseCountForDiagnostics两个primitive int；既有_MainTex/_MainTexArray Shader IDs移到同feature外层供原Foot/Health与新body helper共用。新增internal static AppendSegmentDrawCommands(CommandBuffer,BattleDynamicMeshBackend,MaterialPropertyBlock,bool,out int,out int)，仅抽取原有效segment body循环，返回原DrawMesh计数。局部lastTexture/lastMode/prepared仅一次调用存活；OFF原每有效segment Clear/SetTexture，ON仅连续同实际Texture引用与BindingMode复用同一块。Material仍逐draw原值、slice继续原顶点，不减少/合并任何segment/draw、非法segment仍原skip。BattleRenderPass.Execute原foot与health及lease/CommandBuffer finally不动，仅调用body helper＋两个计数，invalid lease入口本项计数清零。
- BattleCentralSameSamplePixelEditorTests：只新增SegmentTextureBindingReuse_前缀的25纯资格case（default1、首次/重复属性3、所有异mode转换6、同尺寸不同真实texture identity3、新camera/slot/CommandBuffer等价的局部调用重置3、非法segment1、empty backend1、Foot/body/Health槽清理1、三binding modes/两draw modes独立像素oracle6）；原测试不弱化。新增Cost_两个固定2000有效segment pattern（同纹理/交替纹理），OFF/ON相同几何/材质/顺序/draw数量，4warm+8sample每侧balanced唯一一次，完整Native DrawMesh录制纳入timer，不执行GPU或Profiler采集。用一次冷反射CreateDelegate绑定新helper，RED缺API但代码可编译，GREEN/cost热环不做反射分配。

实施前以实际discovery/XML核25+2数量，计划数不是通过数；local材质/纹理/Mesh/RenderTexture等仅测试Owned HideAndDontSave fixtures，在finally Destroy/Release，不删除项目文件。新结果FileMode.CreateNew在唯一67 artifacts，不能覆盖17旧输出。

## 不变量、所有权和预算

只现成MPB/CommandBuffer与stack/primitive，不新增Sprite/生产Texture/材质/GPU实例buffer/数组/owner/服务，不热扩容。lastTexture借用当前submission既有资源且不跨helper/Execute/camera/slot/lease驻留；原CPU/GPU生命周期与十一阶段/Join不变，Execute返回/lease0不证明GPU完成。默认OFF、配置/Scene不序列化变更；无逻辑World写入，publication/插值/排序/first-visible/每segment failclosed原序保持。Q06仅hash/状态，不读取方法体；不解冻EXT1/M0/ATLAS/Mono/Role-aware。新增primitive字段不冒充61字节预算闭合；局部NoAlloc不代替H11完整可靠0GC。

## 保护、操作与回滚

Operation NTSD-OPTIMIZATION-BATCH67-RENDER-TEXTURE-BINDING-REUSE-20261008/before-manifest-01.json逐9写域/266guards/HEAD527350afa08633357ba20e7453a9d260eaa2b97c，初始Feature SHA43C2A3FA6D06476D60E062CCB9D7B5EF24E3B74C76F44324413A3EC10D9C6419、Test SHA37C061B74FDC0F6EB5266233D5CF882F30EAE30D832CD0FC0ABE1DFC2EDEDD3E。先本Record/新Operation/manifest，再所有当前dirty副本逐SHA核同，之后才改既有INDEX/治理/Task/C#；不能用66或HEAD替代dirty副本。原66失败/无收益/坏01audit与完整02audit全部保护。

无Query/Driver/Suite/Observer/Harness/native/Scene/Prefab/资源/Settings/Input/Gen/Plugins/Server/并行TMP写域。准确回滚仅本67副本的本次hunk且另获批准，不reset/checkout/clean/stash/delete/move/覆盖用户工作/push。

## 验收与真实状态

RED_CONFIRMED：df6f0b9d2a5444498e4265ace5fc4844 terminal failed/completed25，fresh callback XML实际25/0pass/25fail，缺helper24＋缺opt-in1；不是编译失败/0case。red-results-01.xml源/副本SHA均9A5C4C7EE713AD176D7CA4970AD065966E2ACBB269C1592980B895B6CDD07086；下一callback前已fresh复制。原桥progress total9943是整catalog，不能把它当本批用例数，实际XML/discovery冻结25+2。

CODE_WRITTEN：原body逐segment循环抽取同Feature helper，默认false、同一次调用局部实际Texture引用/BindingMode/first准备；每有效segment仍原DrawMesh，材质/mesh/submesh/order原值。Foot/Health分支与lease/CommandBuffer finally/RecordSubmission保留；仅两个primitive Last在Execute入口清零并记录body计数，Shader IDs同类上提复用。没有常驻Texture/实例数组/owner或热扩容，GREEN/成本/实景收益尚待。

GREEN事前保护核对：初次诊断脚本误把J绝对路径Join-Path到I工作区，输出一条missing不是文件消失；修正为IsPathRooted分支后UTC04:29:20.0451295Z全部266guards核同/HEAD同，无缺失/恢复操作。Feature F5F1173E63B18A2F90F9073B87AA933FBDC83731822487DEE38DE336587FB6C2、Test2EA04B0CBDB01F3E7ABF5431B03C6FCB4AF2FEA639341CF274FB5330CD3FAA85冻结；原Editor新reload后确实idle/nonPlay再提交同25case GREEN，成本仍未启动。

GREEN_CONFIRMED：e3579f27cc454ef1b34cec22b3b69c58 terminal succeeded/25pass/0fail/0skip，callback25case SHA E0A36E30B2BF98E0A5A6A50A23B92C52D7FBB6EBB4407605F72C658D5437C069在成本回调前复制核同。三mode×两drawMode各同/异纹理12固定alpha0.5样本，OFF/ON均maxChannelError0，内容/反序负控有效；无需弱化或重复原旧6。准确C#保持冻结版，准备唯一两2000segment完整native命令录制cost，不称GPU/实景/logic P95或完整0GC。

COST_COMPLETED：29618ee5e63b445e932eb1fc70e23cff succeeded/2pass/0fail，callback SHA FF445A47B66895526B5D65010AE7564205ECF11F759964532C0F8C6B99C1BCFA复制核同。constant OFF均值5.2033875→ON3.60085ms（-30.798%）、中位4.11735→3.25445（-20.958%），6/8配对更低；alternating 3.9516875→3.9147875（-0.934%），中位3.65605→3.70275（+1.277%）不可辨。两侧2000有效segment/2000DrawMesh/168000bytes不减，ONconstant准备1/reuse1999、alternating准备2000/reuse0。4warm+8sample各侧唯一一次，全部raw保留，未重采/剔除；LOCAL_GAIN_SIGNAL仅重复纹理fixture，NOT_ADMITTED_FOR_PRODUCTION/defaultOFF，非实际AI/FPS/GPU/logic或完整0GC。

27去重实际case通过，原6旧测试不改/不重复。测试terminal后get_loaded_scenes/get_editor_state/read_console连接拒绝；Get-Process19040缺失、CIM无任何Unity.exe、6402无监听，关闭原因未知。未启动/重启Unity，最终live Scene/Console无法验证，只能用保护SHA证明Scene文件没改。read-only global log尾8KiB未见exit/crash关键词，不能据此归因；初次Math.Max int64类型诊断错误已修，不触碰日志文件。Task68仅新READY文档，实景Editor前置缺失，H07/H11与Goal不关闭。

最终审计UTC04:44:20.5582710Z：9backup/两准确source/HEAD同；264/266guards同，SDF字体sha改变、旧Temp/stress result缺失为真实范围外变化，公共Temp/XML亦缺失。旧stress result完整同SHA66副本及67三XML原件保持，无恢复/删除/覆盖；UNKNOWN_CAUSE独立Operation登记，不归因Editor/用户或本任务，也不伪称全保护PASS。字体当前内容保留，Scene/settings/Q06/Server/authority等其余保护同，final live Scene/Console UNKNOWN。Tools validator实际exit0/1364Records/6governed/4314WARNING/0ERROR；首摘要空格正则漏WARNING导致0仅历史错误，02实际重跑更正，完整stdout未持久化。精确两个C#＋总表/Task67 diff check exit0/3CRLF提示，不宣称全树/旧治理EOF PASS。首次audit stdout混入numstat警告被JSON.parse拒绝未落盘，修正后terminal-audit-01有效，未重跑测试/采样。报告与68Task回链已更新，46执行/阶段4of6/H07/H11 OPEN/Goal active，有限局部资格收口不是整个性能目标完成。

先25非Cost有效RED，再最窄GREEN/独立像素，只有正确性通过才唯一两个Cost pattern。等待真实reload idle再提交作业；连接失效只查同job/PID，不重复启动或第二Editor。XML逐次fresh copy/SHA，保留全部失败/修复；最窄diff/新Markdown/266guards/9backup/HEAD/Menu clean/Tools Validate-ChangeLedger实际结果追加。此时未写本批C#、未运行测试/成本，无收益/实际1000AI/FPS/可靠GC/GPU/设备证据；不关闭H07/H11或Stage。
