<!-- CHANGE-RECORD
id: NTSD28-B11-PNG-WORKER-DECODE-001
status: VERIFIED
change-kind: BATTLE_RESOURCE_WORKER_PNG_DECODE
code-path: Assets/NTSD/Scripts/Animation/Runtime/PngPixelDecoder.cs
code-path: Assets/NTSD/Scripts/Animation/Runtime/BMPLoader.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11PngWorkerDecodeEditorTests.cs
code-path: Tools/NTSD28PngDecode/Generate-Fixtures.py
code-path: Tools/NTSD28PngDecode/Generate-CorpusManifest.py
code-path: Tools/NTSD28PngDecode/PngCorpusVerifier.cs
authority: User total goal and D-023/D-07; NTSD2.8-Logan formal PNG corpus; source-linked raw image decode before renderer; W3C PNG format specification; formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033.
evidence: VERIFIED_RAW_WORKER_DECODE_ONLY / RED_7_PNG_NULL_FAILURES / UNITY_13_OF_13 / MAX_SHEET_WORKER_1_OF_1 / CORPUS_1255_OF_1255_RGBA_HASH / INPUT_HASH_UNCHANGED / COMPILE_0_ERROR / BMP_AND_MAINTHREAD_PRESERVED / SCENE_DIRTY_FALSE / LEDGER_464_15_PASS / P21_ALPHA_PENDING / NO_CONTENT_MIGRATION
-->

# NTSD28-B11-PNG-WORKER-DECODE-001

准确范围与验证见同ID Task。原状：BMPLoader.LoadBmpData在线程池只能走BMP manual，正式PNG全部无法从生产后台入口读取。改后：signature为PNG时使用纯System decoder并转换既有BmpData，非PNG原分支保持；不修改CharacterAnimtorManager、RuntimeSpriteProcessor、UI cache、Scene、Importer或正式资源。

数据/生命周期副作用：仅一次资源加载过程的临时byte数组/Color数组与局部流，using关闭；无新manager/queue/worker服务/World关系/renderer/pool，沿现有prewarm任务与取消/关闭owner，不改变十一阶段shutdown。

风险：PNG原始像素成功不等于后处理/最终Sprite与原生一致；PNG色彩管理、alpha与上下行方向必须通过参考像素证明。当前实际corpus只含非交错palette1/2/4/8和RGBA8；不能声称通用PNG全部格式或B11完成。不增加新NuGet/Plugins/渲染框架。准确回滚为本记录新文件与BMPLoader新增分支的差异，保留用户工作，适用删除授权规则保持。

## 验证进度

### 最终出口（2026-09-13；覆盖以下过程中的待验文字）

本包VERIFIED只关闭D-07的后台raw PNG读取；Q02/B11及总目标仍未完成。已有调用者只新增BMPLoader的非主线程PNG signature分支，非PNG与主线程路径逐行保持。PngPixelDecoder纯System/DeflateStream输出bottom-up straight RGBA，不修改原始alpha，不引入框架/服务/外部包。

实际命令与结果：

- Generate-Fixtures.py最终28valid（26PNG+24bitBMP+生产SPARK.BMP）/16invalid；已知像素+Pillow独立核对。旧入口RED job e798c9d002d54385ba5f6341723c6a46 completed9、7个PNG明确worker-null失败，失败job result=null，不伪造2项PASS。
- dotnet build Tools/NTSD28PngDecode/PngCorpusVerifier.csproj最终0warning/0error。Unity bundled Mono链接实际decoder源验证corpus-reference.tsv，最终1255/1255尺寸/RGBA SHA一致（corpus-results-final.txt），读取时校验每个input SHA，后继protection.json复核1255原输入hash全部不变。该证据是实际Unity目标源decoder对Pillow，不称为正式EXE trace。
- 主代理审查发现TryDecodeCore后期失败会保留width/height；公开wrapper已统一在false重置0/null，16坏数据/直接out状态测试通过。
- 真实Unity先worker12/12（job 9f0f2f58690c48ebbfa0e5f1a684c07c），经review增加最大sheet后完整13/13、0fail/0skip（job 3e6a8ac444704c3da7f85015f5d67fd0，final-result.json）。覆盖7张正式sheet/head/small与Unity主线程LoadImage/GetPixels32逐像素相等；最大2001x8768/17,544,768像素的worker Color[]转换及RGBA hash也通过。
- 最大图的初次Unity Mono Process.WorkingSet64返回0，明确标Unavailable，不当0内存。补充managed采样与外部Windows进程采样后窄测1/1（job 7116edc970cf47439ad6871c4a112f9f，memory-result.json）：sampledManagedHeapPeak=1643495424、managedBefore=1403457536、managedAfter=1643556864，期间gen0 collection2；外部162次进程采样最大working set6206914560。它们是整个Editor的采样，不是精确decoder独占分配、无GC或并行预热内存证书。该复测只改测试测量，不改已验证decoder/loader。
- 实际DetectPipeline为URP；Unity编译后新测试均已运行，read_console error CS=0。源代码/安全review无当前范围阻断问题；非PNG旧IO异常传播保持，不扩展修复全局File.ReadAllBytes行为。
- protection.json：3059个既有基线文件仅已声明BMPLoader.cs变化，另外3058不变；正式EXE SHA匹配、1255PNG不变，NTSD_Battle dirtyfalse/rootCount14。最终Ledger464records/15governedfiles PASS，diff检查PASS。

新增confirmed P-21已写总表和NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001 Task：sheet后处理会把非黑像素alpha强制255；RAW解码PASS不能替代该producer/consumer修复。Q02-C NativeSpriteRange Task已准备为下一入口，alpha Task明确在Q07内容可用前处理；catalog/source/cache/publication整体接线仍待做。R17本次仅RAW_PNG_INPUT_HASH_AND_DECODE子条件PARTIAL_RETURN，不关闭Q02/迁移/视听出口。

未运行整场Play、BattleRuntimeSelfCheck、完整SpriteCatalog/GPU或正式资源迁移；本包只新增原始加载能力。decoder仅支持当前实际corpus格式，不声称通用PNG16bit/交错/所有极端输入；静态alpha/GPU/gamma待项见Q02-B-REPORT.md。没有提交/push或删除旧资源。

写工具前补充准确验证范围：Tools/NTSD28PngDecode/Generate-CorpusManifest.py只读正式PNG，以已有Pillow生成input SHA/尺寸/bottom-up RGBA SHA的TSV；PngCorpusVerifier.cs与PngCorpusVerifier.csproj链接实际PngPixelDecoder源，在现有Unity bundled Mono中验证全部1255PNG，无Unity stub，不接入游戏。输出在本ID artifacts，.gitignore仅排除本工具bin/obj/__pycache__。此工具不承载新规则，不写Authority/Assets。

Task/Record在脚本前建立。尚未运行RED/GREEN或PNG生产路径，不声明完成。

追加RED事实：2026-09-13 Unity job e798c9d002d54385ba5f6341723c6a46为terminal failed，completed9，failures_so_far完整列出7个PNG输入worker返回null（palette/RGBA fixture与5个正式文件）。MCP failed-job的result=null，没有可用汇总，因此只报告7个已证失败，不把另外2项无失败记录擅自写为PASS。red-result.json保存原始证据。此时PngPixelDecoder尚未写、BMPLoader生产未改；RED后才授权独立worker实现该文件。

夹具初版26valid/7invalid，后增坏输入至16invalid；valid25 PNG由手工期望RGBA并用Pillow独立验证，另1 BMP控制。Generate-CorpusManifest实际读取1255正式PNG生成输入SHA/尺寸/上下翻转后RGBA SHA，corpus-reference.tsv；最大17,544,768像素，decoder32MiPixels上限覆盖该实际输入。所有正式资源保持只读。

构建输出审阅：worker为避开当时被corpus进程占用的bin生成了Tools/NTSD28PngDecode/Temp/NTSD28PngDecode/build及obj，已确认全部为编译器生成产物；仅对此两个精确目录加ignore，不删除/移动文件、不隐藏手写源码。仓库全局*.csproj规则还会隐藏三个本次campaign手写诊断工程，已在三工具各自.gitignore中用!/*.csproj显式保留，保证可重建配置随交付可见；未改变全局Git配置或Unity生成项目规则。
