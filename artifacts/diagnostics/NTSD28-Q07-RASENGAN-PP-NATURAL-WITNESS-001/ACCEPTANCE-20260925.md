# Q07 鸣人自然螺旋丸资源字段定向验收（2026-09-25）

原项目 Unity 2022.3.62f3 Editor、`NTSD_Battle` 测试场景、正式 Logan 内容根与项目 Mode Asset。只在既有 `NTSD28UserRasenganPhysicalPlayProbeEditor.NaturalProbe` 中把技能资源的预检/逐tick `mp` 改为 `actor.Health.PP`，另记录 `Runtime.MP`、`InputMpConsumedTotal350` 和 `InputLocalResourceEnabled49D034`。该探针使用合成 InputSystem 物理设备事件及现有本地输入提供者单tick推进；没有设置 frame/combo、注入 FrameInputSet、编辑生产逻辑或改 DAT。

唯一新结果 `Temp/diagnostics/NTSD28-Q07-RASENGAN-NATURAL-COMBO-PLAY-001/natural-first253-20260925T070909750-495e3609db8e4896aff7261be305fa44.json`，SHA-256 `E34329F8321B22D2BE5790ABB175547241C9770F5EBD03E4FB1B23FC3B243F56`，`status=PASS`。初始逻辑tick775、PP500、消耗累计0、资源门启用；自然防→前→跳在tick780进入action240。tick781 action239/PP350/累计150；tick807首253；tick808采J/phase0→action301/PP250/累计250。此间旧 `Runtime.MP` 一直500，证实旧探针列并非 native `current_mp` 的可比字段。

根正式EXE受控 LFR `NTSD28-Q07-RASENGAN-NATURAL-FORMAL-PLAYBACK-001/first253-release-trace.jsonl` 同相对事件为tick6 action240/MP500/累计0、tick7 action239/MP350/累计150、tick33首253/MP350、tick34 phase0 J→action301/MP250/累计250。以 formal tick6 对 Unity tick780 的事件偏移归一，上述四个锚点的 action、资源、消耗累计、采样相位均一致。两端的完整世界、对手、RNG和坐标未归一，不声称全字段同世界或正式EXE独立人手/像素验收。

Unity 原 Editor 脚本重新导入，`Assembly-CSharp-Editor.dll` 时间晚于脚本，Console error 0；定向Play结束后已退出Play。Battle/Menu Scene 及 GameConfig 磁盘 SHA-256 依次保持 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、`785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`。本包只关闭这一个自然技能资源见证与错误诊断字段；Q07/R07/R18、正式EXE独立物理键与完整表现仍开放。
