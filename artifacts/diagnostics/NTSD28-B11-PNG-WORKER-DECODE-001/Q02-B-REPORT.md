# Q02-B 正式 PNG 后台解码

总目标NTSD28-UNITY-BATTLE-REALIGNMENT-001；Change NTSD28-B11-PNG-WORKER-DECODE-001。2026-09-13。

## 实际改动

BMPLoader.LoadBmpData的非主线程分支新增PNG signature识别，交给纯System的PngPixelDecoder，再转换为原有BmpData/Color[]。BMP分支、主线程TryLoadWithUnityData、LoadBMP、CharacterAnimtorManager、RuntimeSpriteProcessor、全局cache和UI逻辑均保持。

Decoder只实现当前正式corpus必需的非交错palette1/2/4/8及RGBA8；支持PLTE/tRNS和五种行过滤、连续多IDAT、signature/chunk长度/CRC/zlib/Adler和精确像素长度，输出bottom-up straight RGBA8，不做预乘、去黑或gamma变换。失败返回false、尺寸0、像素null及原因；BMPLoader沿日志/null合同处理。32MiPixels限额覆盖当前最大17,544,768像素，未知格式明确失败，未宣称通用PNG全部格式。

PNG数据结构和过滤规则参照[W3C PNG规范](https://www.w3.org/TR/png/)，战斗表现权威仍是正式NTSD2.8-Logan。原生d3d11_renderer.cpp:552-633用WIC 32bppRGBA加载，角色draw_quad默认source_alpha。独立Pillow参考与Unity主线程参考只用于像素交叉验证，不代替最终原生GPU对照。

## 实际证据

| 验证 | 实际结果 | 原始证据 |
|---|---|---|
| RED：既有后台入口 | 7个PNG失败明确为worker返回null；job completed9但失败job result=null，不伪造完整PASS计数 | red-result.json |
| 已知像素fixture | 26 PNG（20 palette过滤组合、1无tRNS、5 RGBA）+24bit BMP+现有SPARK.bmp共28有效文件，逐像素比较 | Generate-Fixtures.py与Temp/NTSD28PngDecode/generated/manifest.json |
| 坏数据fixture | 16文件被拒绝；生产入口null、直接decoder失败无部分尺寸/像素状态 | green-result.json |
| 正式PNG全量独立比对 | 1255/1255尺寸和bottom-up RGBA SHA与Pillow相同；读取时校验输入SHA | corpus-reference.tsv、corpus-results-final.txt |
| 真实Unity focused | 最终13/13、0fail、0skip；实际Task.Run调用，7个正式sheet/head/small文件逐像素对照Unity主线程LoadImage/GetPixels32；含最大sheet | final-result.json，job 3e6a8ac444704c3da7f85015f5d67fd0 |
| 最大sheet补充验证 | 2001x8768/17,544,768像素worker读取、Color[]转换及参考hash通过；测量修订后窄测1/1 | memory-result.json、largest-sheet-worker-memory.txt、windows-memory-samples.json |
| 编译 | net472源码链接0warning/0error；Unity刷新/domain reload后实际新测试通过，Console error CS为0 | corpus-build-final.txt、unity-compile-errors.json |
| 活动场景 | NTSD_Battle仍dirtyfalse、rootCount14 | scene-after.json |

测试中的DetectPipeline实际返回URP（Temp/NTSD28PngDecode/pipeline.txt），没有修改相机、PPU、材质、滤镜、URP包或ProjectSettings。Unity连接复用已存在的2022.3.62f3/MCP桥，未启动第二实例。一轮get_editor_state等待超时随后恢复，后续job已明确terminal succeeded，没有据观察超时重启Editor。

内存测量限定：Unity Mono的Process.WorkingSet64返回0，已标Unavailable。实际managed heap采样峰值1,643,495,424 bytes、前值1,403,457,536；外部Windows162次进程采样最大working set6,206,914,560 bytes。这些包含整个编辑器与GC影响，不是精确解码器独占内存、无GC承诺或多图并行预热证明。

## 已确认但本包不冒充关闭的差异

新增总表P-21：CharacterAnimtorManager在sheet raw decode后调用RuntimeSpriteProcessor.ProcessSheetPixelsFast。该方法将非黑像素alpha变255、黑色变0，损坏正式PNG的半透明（例如nar.png palette alpha241/238）。本decoder已保留原始alpha，但最终sheet仍须独立改正。任务NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001已建立，关联Q02后续/Q09/R17，不能遗忘到“完整渲染以后再说”。

head/small的LoadBMPAsSpriteAsync没有上述sheet后处理，但原生selection face/roster small与battle HUD的混合模式不同，不能据raw pixel成功统一修改菜单或重开用户排除HUD。

Q02-C的累计effective pic与reversed declaration、catalog.csv/registry_index、source/config/UI缓存事务仍未实现；正式DAT/PNG没有迁移进Unity，未运行整场Play或BattleRuntimeSelfCheck。此包只关闭后台原始像素读取缺口，不关闭B11、Q02整组或总目标。

## 文件与框架保持

只有已声明的BMPLoader已有文件新增分支，其余为独立新decoder/tests/工具和治理文档。没有新manager/服务/对象池/第三方依赖，没有修改非战斗流程、Scene、Prefab、importer、正式资源或shutdown顺序。decoder局部stream在using结束时释放；Color/byte数组由现有预热过程使用。

最终源码/输入保护、工具审阅和Ledger结果见同ID Change Record与本目录receipts；工作组状态必须按这些实际证据更新。

最终状态VERIFIED_RAW_WORKER_DECODE_ONLY。1255正式PNG hash不变；3059原基线文件中只有声明的BMPLoader.cs变化，另3058不变；Ledger464/15及diff检查通过。R17只完成raw输入/解码部分，range/alpha/source/cache/迁移/最终视听仍未关闭。
