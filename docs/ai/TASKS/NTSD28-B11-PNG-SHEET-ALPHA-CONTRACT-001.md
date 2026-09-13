# Q02后继：正式PNG战斗sheet保留source alpha

状态VERIFIED_SOURCE_PNG_SHEET_AND_GPU_SAMPLES_ONLY；Q02-B raw PNG已VERIFIED，按Q02队列在range包后接续（共享CharacterAnimtorManager写范围，不并行覆盖）。父目标NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-02 / Q02，原差异新增P-21；回访R17与Q09。

## 已确认首差

正式nar.png为palette8，tRNS前4值0/241/238/241，head/small也有半透明。d3d11_renderer.cpp:552-633 WICFormat32bppRGBA→R8G8B8A8_UNORM，角色draw_quad默认source_alpha；shader在标准source-alpha路径不预乘，混合SrcAlpha/InvSrcAlpha。

Unity CharacterAnimtorManager.ProcessAndCreateSpritesAsync:1436-1464先decode再调用RuntimeSpriteProcessor.ProcessSheetPixelsFast，后者约465-487把黑RGB的alpha改0，其余改255。Raw PNG decoder即使逐像素相同，正式sheet最终发布仍会丢半透明。这不是Q02-B的PNG语法/解压错误。

## 后续准确范围与约束

先审视BMPLoader.BmpData是否需要显式raw像素alpha来源标记，以实际decode格式/来源区分，不能用“非BMP都一样”的猜测。仅正式战斗sheet路径选择保留PNG alpha；原BMP color-key、head/small菜单consumer与Editor preview不能被全局改写。RuntimeSpriteProcessor既有方法可保持，优先在生产caller根据已确认metadata分支。任何source标记是加载数据，不得加入World/snapshot或改变D-022版本。

还需追ClearDetectedGridSeparatorAlpha：只对被权威边界确认的网格像素清alpha，不能误删PNG本来可见边缘。Unity central shader是Blend One OneMinusSrcAlpha且fragment乘alpha；数学可等价不等于GPU实证，必须保持straight输入并分别验证采样/量化/color-space。

菜单selection face/roster small在原生使用black-color-key，与battle HUD source-alpha不同；用户排除的原生HUD不因此重开。本Task只针对角色/技能战斗sheet，不能统一改所有PNG用途的混合策略。

## 验证出口

写脚本前另建准确Change Record，测试需证明旧BMP输出保持、正式PNG的0/半透明/255与RGB不被覆盖、所有已声明grid行为可解释；再覆盖实际prewarm→SpriteCatalog与必要GPU读回，不用单个helper断言代替生产consumer。正式资源部署仍在Q07；可使用隔离来源/夹具验证加载适配。若最终GPU验证依赖Q09条件，明确保留该出口并保证Q07不会把错误alpha内容声明为完全可用。

本Task只是已确认缺口的持久化回链，尚未修改上述任何生产路径。

## 写前冻结（2026-09-13）

准确三脚本、API、格式/source双条件、旧入口保护、测试及回滚已登记同ID Change Record。GPU证据限实际URP/Gamma及生产sheet→catalog，完整全局prewarm/publication由后续source/cache事务完成。当前开始实施，不再是仅记录缺口。

## 最终出口

Unity21/21 PASS（6+13+2），隔离/正式nar在实际生产sheet→catalog→GPU共10样点通过；CS0/Scene dirtyfalse。准确代码、变更和未验边界见同ID Record与Q02-D-REPORT.md。新source像素处理子包关闭；Q02继续，下一NTSD28-B11-CATALOG-PUBLICATION-CONTRACT-001 / READY_CONTRACT。前文“尚未修改/写前冻结”为历史准备阶段，不覆盖本最终状态。
