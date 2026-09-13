# Q02-D 正式 PNG 战斗 sheet 透明度

状态：VERIFIED_SOURCE_PNG_SHEET_AND_GPU_SAMPLES_ONLY。Q02 整组仍 IN_PROGRESS，正式全局目录/缓存切换与资源迁移尚未执行。

## 依据和修复

正式 EXE SHA-256：B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033。`source/README_SOURCE.md` 和 playable build 闭包指向的 `d3d11_renderer.cpp::load_texture` 经 WIC 32bppRGBA 上传 R8G8B8A8_UNORM，`render` 的 sprite 分支使用默认 source_alpha；blend 为 SrcAlpha/InvSrcAlpha。`render_snapshot.cpp::SpriteFrameResolver28` 按 w+1/h+1 取源矩形，跳过一像素分隔，不按绿色占比擦行列。七个权威/输入文件的前后 hash 一致，见 authority-before.json 与 protection-result.json。

Unity 原 sheet consumer 将黑色 alpha 置 0、其他置 255，再按绿色占比清整行/列。现在：

- BMPLoader.BmpData.IsPng 由实际签名和成功解码确定，主线程/后台两路都传递；扩展名不定义格式。
- 旧 LoadCharacterSpritesAsync 的公共签名保持，委托 null source；新 LoadCharacterSpritesFromSourceAsync 显式传来源到同一个生产 sheet staging 方法。
- PrepareBattleSheetPixels 只在 Logan source + 实际 PNG 时保留全部 straight RGBA，跳过启发式清边。旧源 PNG、BMP 和未知格式继续原算法。
- 处理结果原样进入主纹理上传和 atlas source pixels，再由现有 SpriteCatalog 取片。没有改 shader、SpriteRenderer/Camera、原像素处理方法、头像 consumer、UI、spark、全局缓存所有权或 snapshot 12/20/23。
- ProcessAndCreateSpritesAsync 原本只使用显式 staging 参数/静态 helper，现为 internal static 供隔离验证与同一生产调用复用。没有新增 manager 或生命周期 owner，不属于 Mono/Core 重构。

## 实测

RED job `94e86ba6fc2c48a1b391230a6aaa00c0`：6 项均因缺少格式/source/像素策略入口失败。实际结果在 red-result.json。

GREEN job `5226692062864130b652cd8be8089e29`：**21/21 PASS**，包括本包 6 项、既有 PNG worker 13 项和旧 Overlap 2 项。见 green-result.json / verification-summary.json。

1. 隔离 PNG 故意使用 .dat 后缀，主线程和 worker 都确认 IsPng；默认非 PNG metadata 为 false。
2. CPU 覆盖原始 0、128、238、241、255 alpha、半透明黑色和绿色边缘；旧源和非 PNG 数据仍按原去黑/清边处理。
3. 隔离 sheet 与正式 `vfs/c/nar/nar.png` 实际走 ProcessAndCreateSpritesAsync，所有 atlas 输入像素与解码像素一致；通过实际 SpriteCatalog 选取可见矩形。
4. 两份 sheet 使用现有 NTSD/BattleCentralTransparent 材质，各 5 个实际纹理样点经 GPU 绘制到独立 RenderTexture 后读回。在当前 **Direct3D11 / URP / Gamma** 下，RGB 与原生 source-alpha 公式相差不超过 2/255，输出 alpha 相差不超过 1/255。隔离样点覆盖透明、半透明绿色和黑色；正式样点使用可见矩形内的半透明像素。
5. Unity CS error 0；NTSD_Battle isDirty=false、rootCount14。原3059文件中3056保持原样，只有累计声明内的BMPLoader、manager、ParserV2变化，零缺失。RuntimeSpriteProcessor和shader的本轮hash不变。

实际工具命令：既有 `Temp/Goal13_bridge.py` 调用 refresh_unity；run_tests(mode=EditMode,testNames=NTSD.Test.NTSD28B11PngSheetAlphaEditorTests、NTSD.Test.Editor.NTSD28B11PngWorkerDecodeEditorTests、NTSD.Test.BattleSpriteOverlappingRangeEditorTests)；get_test_job；read_console(error CS)；manage_scene(get_active)。Python hashlib 核对保护清单和权威输入；Change Ledger 使用 `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path`。

## 证据边界与后继

此证书关闭显式 source 的 PNG sheet 处理和当前设备上的样点 GPU 路径。它不是全部角色/技能整场画面、Texture2DArray/atlas-page 绘制、其他色彩空间或正式 EXE 同场景截图的完整一致性证书。实际全局 prewarm/publication 未被本测试调用；测试只复用其中同一个生产 sheet staging 方法，所有自建 Unity/GPU 对象已 finally 释放，没有构造 manager 或触发其 OnDestroy。

P-21 原 alpha/grid 擦除差异已在新 source 路径修复，但正式 global caller 仍待目录/cache事务切入；R17 保持 PARTIAL_RETURN。下一 NTSD28-B11-CATALOG-PUBLICATION-CONTRACT-001 从 Q02-A 已确认调用链冻结目录/来源/缓存/发布事务；Q03 的6文件9帧转换拒绝等字段问题独立准备，Q07 再迁移正式资源，Q09 补完整表现。
