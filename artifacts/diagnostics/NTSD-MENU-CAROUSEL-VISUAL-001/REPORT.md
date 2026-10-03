# NTSD-MENU-CAROUSEL-VISUAL-001 验证报告

状态：RUNTIME_PENDING。代码与原菜单事件接口 Play/画面验证通过；实体鼠标/触摸、移动端 Player 和用户视觉验收未执行，不声明逐像素复刻。

## 行为与接入

用户明确要求当前 NTSD_Menu 模式选择实现参考图的居中高亮、外围字体大小/颜色/柔化变化及环形列表。复用此前 MenuLoopCarousel/MenuCarouselMotion/MenuOptionList，仅给既有七个选项附加 MenuCarouselTextEffect。

中心放大 1.9 倍、白色面、红色描边和独立黑色阴影；离中心越远越小、灰度/透明度/SDF 柔化增加，视口边缘全字渐隐。循环搬移点位于不可见区域。行左侧统一对齐，拖动/滚轮/既有键盘导航共享选择索引，结束拖动自动吸附；非中心点击先选择，已经吸附的中心项点击确认。

SelectGameModeController 的既有运行时 Configure 路径已接入，无需另建预制体或手绑七份选项。场景编辑状态保留原布局，进入 Play 后显示动态效果。MenuLoopCarousel 管理循环/布局；MenuCarouselTextEffect 管理字体表现；MenuCarouselStyle.asset 只提供当前已有中文备用字体引用。

本项目定制 TMP 将描边与阴影颜色混在同一 mesh tangent。项目自有 Resources/UI/MenuCarouselText.shader 提供独立的红色描边和黑色阴影，同时保留 UI mask/stencil 支持。未修改第三方 TMP/Plugin 源码。

现有书法字体缺部分菜单字。每行使用临时字体/材质副本，已有字形保持；缺字使用当前 SourceHan 字体。Style 为整个菜单一次准备完整字集，七行共享一个临时 fallback atlas；末个借用释放后销毁其 font/material/texture。当前 TMP 分批 TryAddCharacters 会抛重复 glyph key，因此不能逐行分批追加。原字体/材质配置与布局在关闭时恢复。MenuOptionBase 的运行时 externalHighlight flag 避免旧 ShowHide 将 TMP fallback child0 当高亮对象而隐藏文字。

最终只读检查原始JifengBladeArtSC-0480.ttf的cmap：481字符，菜单缺决/闯/关/淘/汰/赛/争/展/1/V/2（font-source-glyph-check.json）。这些字在原TTF本身也不存在，不能通过扩大原SDF atlas恢复同款字形；当前备用字形确保完整显示，但书法一致性仍依赖后续完整字体资源。

## 实际验证

- 原 Editor PID105896 / Unity2022.3.62f3 / native bridge6401；显式导入声明脚本、shader。未启动第二 Editor，未停止其他任务的 Play。
- 最终生成工程编译：dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly，exit0、0 error；最后增量编译24 warning，之前完整生成296 warning。日志 final-compile-04.txt 与 final-compile-03.txt。这些警告未在本任务内清理。
- 原 Editor focused job b811e12125834e3b853252f01200ccc4：12/12 PASS，focus-result-04.json / focus-results-04.xml。涵盖前后多圈、快速输入、拖动抑制确认、单路导航、相邻 wrap、布局恢复，以及 center/edge、私有材质、shader支持、缺字填充与最后借用释放。
- 原 Menu scene UnityTest job36eff145effd4ad2ac733ea9254bd163：1/1 PASS，最终 XML play-results-08.xml，2026-10-03 07:50:49+08。实际 EnterPlayMode，七行 glyph/material assertions；Navigate七次回索引0/y0，Click2选中并截图；关闭/重开保持原对象身份/七效果组件；ExitPlayMode后两个Scene SHA不变。
- 实际渲染截图：center-vs-235046841.png、center-tournament-235049058.png。已查看前一次同内容截图，后一次最终通过场景截图内容相同。提供图供用户检查，不声称用户已确认美术效果。
- 实体鼠标、触摸、手柄、Android/iOS/WebGL Player 未测试。BattleRuntimeSelfCheck/权威 EXE trace 不适用本次菜单表现，未运行，不更改 battle 规则。
- git diff --check 对本次声明源与文档通过；Change Ledger validator 结果见最终 ledger 日志。其治理根仅 Scripts/Tools，因此 Resources shader 使用 asset-path，仍纳入本记录、手工源审查与ShaderUtil编译测试。
- 并行其他任务改动及中途外部 Git commit 保留；本任务没有 commit/push。

## 已保留失败与修正

focus01：渐隐 SmoothStep 区间错使中心alpha0.16；修正后 focused02通过。
Play01：EditMode UnityTest 的 WaitForEndOfFrame 不受支持，改异步截图等待。
Play03：吸附近似白色被精确Color比较误判，改合理容差。
Play04：闯缺字，项目 TMP 不会自动填动态fallback，改临时atlas。
Play05：分批填字触发已有TMP重复glyph异常，改完整菜单一次填充。
Play06：已完成截图/wrap/reopen，退出域重载使静态SHA基准变null，改SessionState，当前SHA实际未改变。
Play02/07：Editor已在/进入Play，原任务未启动；等Editor空闲后继续，没有强制停止。
最终Play08 PASS。失败XML/源码preimage与旧截图均保留，center-vs.png是早期缺字版本，不作为最终图。

最终交付检查：Tools/Validate-ChangeLedger.ps1 全工作区 exit0 / PASSED（1184 records、9 governed changed code files，包括并行任务），change-ledger-final.txt；声明源及共享治理文档 git diff --check exit0，仅现有LF/CRLF提示。最终两截图已逐张打开查看：白面/红边/黑影，外围字体渐隐，中文字符完整；没有将缺字备用字体称为与书法字形一致。此前非治理Resources code-path错误已通过metadata归类更正，不修改验证器。

## 保护与回退

当前任务开始/最终一致：
Menu 5D79DBB7F3C6E9FF790413A8D6C0D9942A093F5D9E1B69FC351468C05EC0052D
Battle 93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60
Packages manifest 335747D833A5CAC9B58B5F6D910E11750457B58A040154CF736D07875C831AFF

前一任务未知Scene writer历史未归因；本任务采用用户此次请求下读取的当前字节，并未恢复E8旧场景。最终文件hash见final-hashes.json。六份 manifest 指定已有文件备份SHA复查一致。仅本任务hunk可前向恢复，需先审查后续用户/并行改动；禁止整文件旧备份覆盖当前共享文档。
