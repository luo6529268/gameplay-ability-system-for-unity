# NTSD28-336B44-Q10-ORASENGAN-078-NATURAL-VOICE-001

状态：`RUNTIME_PENDING / SCOPED_NATURAL_VOICE_PASS`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，BATCH-05/Q10。此独立包只核对正式 `data/078.wav` 暂存之后的原 Unity Battle Scene 自然技能事件→实际战斗 voice；Q10/Q12 不自动关闭。

当前权威：根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 与对应 playable。`NTSD28-336B44-Q10-ORASENGAN-JUMP-ROOT-LFR-20261004` 的 OID2 鸣人 X500/Z650、OID7 远距对手 X1200/Z650、seed682973786、mode0、正式背景23，在防2tick→右2tick→跳2tick→空到33→跳34–35 的55tick链中，根动作/MP与源码110/110一致；源码tick34在动作325→326时发 `data/078.wav` 一次。根公开trace无audio字段，不能称根扬声器证据。此为后续 Jump，不是用户报告的后续 Attack→螺旋手里剑。

前置：`NTSD28-336B44-Q10-ORASENGAN-078-FORMAL-WAV-DEPLOY-001` 已将正式54,104帧/8-bit/22050Hz原文件新增于 `Assets/NTSD/Content/LoganRuntime/vfs/data/078.wav`，逐SHA `D8CAB6CE5CCF129FD689F9FD0170D06B21E04B930BE77EF9AAC9AD4B563F3560`；旧非战斗 `Assets/NTSD/Sound/data/078.wav` 保留。原Editor当前非Play/非编译/唯一Battle Scene clean，运行前重新确认。

声明代码路径：只新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q10Orasengan078NaturalVoiceProbeEditor.cs` 及Unity自动 `.meta`。独立菜单在原项目唯一clean Battle Scene的Play clone配置OID2/7，沿现有正式内容与共用 `SimulationTickDriver.StepOneTick` 输入桥接跑55tick，逐tick记录动作/PP/输入相位、待播078事件与播放器播放计数、匹配的AudioSource；读取通用战斗/非战斗同名cue的 SourcePath、正式clip帧数/声道/频率。结束写唯一CreateNew原件，并补场景/四保护SHA及残留只读检查。无生产分支、DAT/图/Scene/Prefab/模式/旧Sound修改。

验收：生成Editor工程0 error、原Editor导入新程序集；正式cue SourcePath在LoganRuntime且clip为单声道22050Hz/54104帧，非战斗同名cue仍指旧Sound；Unity按正式LFR提交同序列离散 `FrameInputSet` 的55tick，与当前336B44源码/root动作和PP逐tick配对，tick34至少一个正式078待播事件且生产 pooled voice 用同一正式clip；退出非Play、唯一Scene clean、Battle/Menu/GameConfig/ModeAsset四SHA稳定、原Scene Driver不绑World/Pool0。此离散输入链不证明真实键盘设备→Action→Provider，物理键另保留后继出口。若出现首差或编译/Play失败，保留原件、修正测试前置或按首差另立生产Change，不以硬编码078分支修播放器。仅跑该定向链；声像/设备PCM、其他cue和Q10总门保持待证。

风险与回滚：Play clone预热正式资源并创建临时voice，绝不保存Scene。新诊断结果与新脚本只增不覆写；若需撤销，先按用户的文件操作审计合同记录并取得相应授权，不触碰已有工作区内容。

执行结果（2026-10-04）：[原场景报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-ORASENGAN-078-NATURAL-VOICE-001/REPORT.md)与原始JSON已保存。原Editor导入探针，最终生成Editor工程0 error；55tick的五字段275/275与当前正式源码一致，tick34正式078待播、生产pooled voice及匹配的正在播放AudioSource同 tick 阳性；战斗正式cue与非战斗旧cue分路、正式clip样本数/声道/采样率符合前置。Play结果内六SHA同、退出非Play。独立残留检查World解绑/Pool0，但并行Scene文件哈希已变化，跨窗口Scene稳定性为`INCONCLUSIVE`。真实物理键、设备PCM、后续Attack以及Q10/Q12总门继续开放。
