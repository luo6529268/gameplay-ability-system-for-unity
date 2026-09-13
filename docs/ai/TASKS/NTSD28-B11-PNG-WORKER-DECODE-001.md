# Q02-B：正式 PNG 后台像素解码

状态VERIFIED_RAW_WORKER_DECODE_ONLY；父目标NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-02 / Q02。最终Unity13/13、最大图补测1/1、正式1255/1255像素hash相同，见同ID Record和Q02-B-REPORT.md。生产内容仍未切换，Q02整体未完成。

## 写代码前冻结的准确文件与实现

- Assets/NTSD/Scripts/Animation/Runtime/PngPixelDecoder.cs：纯System decoder，输出bottom-up RGBA byte[]，无Unity API，支持正式corpus的palette1/2/4/8与RGBA8、非交错；明确拒绝未支持格式和坏数据。复用System.IO.Compression.DeflateStream，不安装库；按W3C PNG规范验证signature/chunk长度/CRC/zlib/Adler/filter/palette/transparency与exact输出尺寸。
- Assets/NTSD/Scripts/Animation/Runtime/BMPLoader.cs：主代理只接LoadBmpData的PNG signature分支，转换RGBA为既有Color[]，BMP和非PNG原路径保持。PNG bitstream失败返回日志/null；既有File.ReadAllBytes异常传播语义保持，不在本包全局修改IO/caller或渲染后处理。
- Assets/NTSD/Scripts/Test/Editor/NTSD28B11PngWorkerDecodeEditorTests.cs：实际线程池调用与Unity主线程参考像素对比；读取正式资源只读，不导入Unity；fixture在Temp/NTSD28PngDecode，输出artifacts/diagnostics/NTSD28-B11-PNG-WORKER-DECODE-001。
- Tools/NTSD28PngDecode/Generate-Fixtures.py：只生成本工具的PNG/BMP坏数据/参考RGBA夹具与manifest，使用Python标准zlib/struct+binascii、参考Pillow；不写Authority/Assets。工具README/.gitignore、工具所需PngDecodeTests.csproj/Program.cs及本工具NUnit源链接配置若添加须先更新Record准确code-path。
- 新脚本.meta由现有Unity导入生成；文档范围为本Task/Record、Ledger/STATE/handoff/CURRENT-AUTHORITY/对齐总表和本ID工件。
- 全corpus验证工具（写前追加）：Tools/NTSD28PngDecode/Generate-CorpusManifest.py、PngCorpusVerifier.cs、PngCorpusVerifier.csproj、.gitignore。前者Pillow只读1255PNG生成input/像素hash，后者链接同一实际decoder，用Unity bundled Mono执行逐文件验证，不创建Unity贴图或GPU资源。只写本ID artifacts与本工具bin/obj；不把它当作最终渲染或所有生产caller证据。

Decoder实现可委派单文件worker，主代理拥有BMPLoader/测试/生成器/集成。worker不独立决定更大格式/框架范围，也不修改其它文件；所有人共享工作树，保留他人编辑。

公开格式规范仅定义PNG解码： https://www.w3.org/TR/png/#9Filters 、#11IHDR、#11tRNS、#10Compression 。战斗权威与最终像素行为仍以正式NTSD2.8-Logan和实际图片为准，不用规范替代游戏行为。

## 已确认入口与范围

现有Animation/Manager/CharacterAnimtorManager.cs的ProcessAndCreateSpritesAsync和LoadBMPAsSpriteAsync在线程池调用Animation/Runtime/BMPLoader.cs的LoadBmpData；其后台分支只走BMP手动解析。必须在既有API中提供PNG像素结果，保持BmpData.Width/Height/Pixels的行序和既有BMP行为，不引入第二套loader/manager，不将Texture2D/Unity API放在线程池。

先检查仓库已有可复用PNG实现/依赖，再冻结最小实现选择和准确Change Record；不得为了本包升级Unity/URP、安装或重构第三方。可能实现路径限定BMPLoader.cs及与其同目录的纯PngPixelDecoder.cs，测试与工具准确路径须在写脚本前登记。

## 已测量内容前置

artifacts/diagnostics/NTSD28-B11-CONTENT-SOURCE-PATH-CONTRACT-001/png-header-inventory.json记录正式vfs全部1255 PNG的IHDR：indexed colorType3 bitDepth8=1082、1=25、2=15、4=123；RGBA colorType6 bitDepth8=10；全部interlace0。此全VFS格式覆盖只是解码前置审计，不扩大角色资源迁移范围。既有PNG实际alpha/PLTE/tRNS/chunk压缩、行过滤与像素方向必须按权威图片和格式规则验证；不能只支持普通8-bit RGBA后宣称内容可用。

## 出口要求

- Test-first：实际后台入口先对正式来源代表文件/隔离fixture失败，再实现后通过；保留BMP输入回归。
- 比较每像素RGBA与可靠解码参考（优先Unity主线程LoadImage或经验证原生decode）。覆盖1/2/4/8-bit palette、透明与半透明、RGBA8、多IDAT/行过滤及非整字节宽度；读取失败/截断/不支持输入明确失败，禁止错解出“成功”图片。
- 不应默认修改RuntimeSpriteProcessor的去背景/网格处理。该后处理与原生最终像素是否一致另记Q02-C/Q09/R17；raw decoder通过不等于final sprite显示通过。
- 通过现有Unity实例编译、focused EditMode并实际从线程池调用。记录Scene dirty/hash保持、native资源hash保持与ChangeLedger。
- 真实正式内容staging/publication仍依赖catalog/source/cache合同、字段Q03/Q05/Q06和Q07；本包不改变菜单选择/排序、全局cache、音频、Scene、Importer或正式DAT/图片，不隐式开始资源迁移。

Unity连接恢复：CLI已确认目标Unity2022.3.62f3进程存在但无Pipeline；仓库已有MCP桥端口由C:/Users/Logan/.unity-mcp/unity-mcp-status-b1b02287.json实时读取，Temp/Goal13_bridge.py可调用get_editor_state、refresh_unity、run_tests、get_test_job、read_console。先复核身份/进程再用；不得重开第二实例。execute_code当前Roslyn缺失、CodeDom命令行过长，不能依赖其成功；已有正式focused TestRunner工作。不要为诊断工具安装Pipeline或重写第三方。

本Task raw decode已实现并验证；下一Q02-C NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001已准备。另有已确认P-21/NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001，必须在Q07对应内容出口前处理，不能用此包raw像素PASS遮盖。全局source/catalog/cache/publication与最终渲染继续后继验收。
