# C032 自然命中触地音频：正式源码与原 Battle Scene

状态：`VERIFIED_SCOPED_SOURCE_UNITY_AUDIO`。仅验证当前正式内容的多由也36/action243→鸣人2自然命中链在严格触地时的声道6事件及原 Unity 战斗场景生产音频发布。C032/Q10整组、正式根逐条音频输出、设备听感、声像和音量继续开放。

当前规则权威为根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 与对应 playable `GameSession28::step` / `BattleWorld28::step_physics`。正式源码诊断使用 mode0/BG1/seed682973786，OID36 X500/action243 与 OID2 X550/action0、两方HP/MP500、128 tick中立输入。`source-audio.csv` 顺序记录6条音频：帧声在tick1、13、23、96；builtin声道6仅在自然state12严格触地的tick60、66出现，world X分别373和360。相邻tick没有声道6。`data/sound.dat` 索引6映射 `data\016.wav` 的独立内容证据在C032父包报告。

原 `NTSD_Battle.unity` 的Play clone用正式暂存DAT/角色图、生产 `SimulationTickDriver.StepOneTick` 同初态运行128 tick。唯一 `tay36-a243-x550-natural-scene-03.json` 的每tick `PendingSounds` 总计6条；与源码按tick、顺序、映射后的cue、source-rule X和全局tick偏移5核对，**30/30字段一致，事件数6/6，无额外或缺失事件**。声道6在tick60/66各一次，cue为 `data\016.wav`，X373/X360。[逐项比较](source-unity-audio-comparison-v1.json)同时确认此次128个实体/RNG采样与F03已验Scene v2逐项完全相同。新C++探针生成的实体、RNG、关系、帧CSV和LFR字节SHA均与F03已通过的源码输出相同；LFR SHA为`3DC1C1B587B506B90BFF49507529611D62826542ADA708CBAF58D7ABBC7FE77D`，此前正式根通过的是同一字节流，但其JSON trace不含逐条音频，不能由此推断正式根声音通道已经取证。

Native诊断g++编译成功、运行退出0；原Editor刷新后程序集时间晚于脚本、运行时未见编译错误，Play报告`PASS/DONE`、128样本、exitedPlay/sceneCleanAfter均true。原Editor最后idle/非Play；四个保护资产SHA与本包前基线全部一致。只改两个已声明的诊断脚本及产物，没有修改生产代码、DAT、WAV、Scene、Config或UI。正式根LFR trace当前不公开audio_events；设备是否实际可听及声像/音量仍须分层验收。父C032保持`RUNTIME_PENDING`，Q10及总目标保持开放。

交付检查：`Validate-ChangeLedger.ps1` exit0/PASSED（1070 Records、8 governed code files，两条诊断脚本均覆盖此Change ID）；`git diff --check` exit0。没有重复运行与本次诊断无关的完整SelfCheck或设备播放。
