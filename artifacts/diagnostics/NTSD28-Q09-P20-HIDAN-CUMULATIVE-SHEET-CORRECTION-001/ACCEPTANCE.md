# Q09/P-20 飞段图片累计范围纠错限定验收

正式权威 `source/ntsd28_core/src/data/dat_parser.cpp::project_sprite_sheet` 保留 Hidan DAT 文本声明 `file(62-126)` / `file(117-128)` 供诊断，但为运行时按前序 sheet 的 `row×col` 容量累计范围：hid2 是62–116，hid6是117–128。`render_snapshot.cpp::SpriteFrameResolver28::resolve` 使用后者，pic119落在hid6第2个局部索引。此前[P-20容量记录](../NTSD28-Q09-P20-LOGAN-GRID-CAPACITY-001/CORRECTION-20260929.md)把文本端点直接当运行时归属，关于“pic119无本体”的结论已撤销。

本轮只改聚焦测试：从项目内正式 Logan `decoded_dat/c/hid/hid.dat` 解析原始声明，调用既有 `CharacterAnimtorManager.BuildSpriteFilesForSource` 得到累计范围，再断言 pic119属于hid6且对应局部rect可用。另保留一例合成的真正超容量索引门，不把它误称为飞段实际内容。原 Editor唯一实例 `gameplay-ability-system-for-unity@b1b02287` 新编程序集晚于测试源码，精确EditMode job `067693fcdd0045d7ade4f7f08f7002ef` 为2/2通过、0失败、0跳过；[完整MCP job](focused-unity-job.json)保留。生成 Editor C# 工程0错/211既有警告，`Tools/Validate-ChangeLedger.ps1` PASS（1005 Records、34 governed code files）、`git -c core.safecrlf=false diff --check`通过。

原Battle两轮自然frame430中央命令的独立[证据](../NTSD28-Q09-P20-HIDAN-BATTLE-COMMAND-001/ACCEPTANCE.md)保留。四保护资源SHA-256：Battle Scene `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`；Menu Scene `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`；GameConfig Asset `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`；ProjectBattleModeConfig Asset `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，均保持既有值。此次未改生产C#、DAT、PNG、Scene、配置或非战斗代码。

限定：目录映射和中央命令并非真实像素证明；原 Unity Battle 当前命令实际使用的资源绑定与正式根同状态、同视口 GPU 像素仍待。Q09/P-20/BATCH-05和总目标保持开放；Q07/D-024碰撞域选择独立待定。
