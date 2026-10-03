# Q10 当前正式内容的音效预热边界（2026-10-03）

状态：`READ_ONLY_CURRENT_CORPUS_FRAME_CUE_COVERAGE / LEGACY_FROZEN_CUE_UNCONFIRMED`。正式规则身份为根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及其对应 playable live source。本轮只读正式 DAT、C++ 与 Unity 生产调用链；没有修改脚本、DAT、WAV、Scene、相机或非战斗功能，也没有运行 Unity Play。

## 已测内容与调用链

- 逐文件扫描当前正式 `resources/runtime/decoded_dat` 的 **405** 份 DAT，按 `<frame>`/`<frame_end>` 划分 **55,351** 个帧块，并把 `itr`、`bdy`、`opoint`、`wpoint`、`cpoint`、`bpoint` 子块的字段排除后统计顶层 `sound:`：**46,487** 帧为零条，**8,864** 帧为一条，**0** 帧为两条或以上；一条声音的值全部以 `.wav` 结尾，共 **976** 种文本值。用未排除子块的宽松扫描曾将 `c/kar/a/cha.dat` 帧80两个 `itr.sound: 1` 误计为帧声音；核对原始行148～155及子块后，此假阳性已排除。该扫描是当前资源的文本结构核验，不代替 Unity 实际加载/播放证书。
- 当前 playable 的 `battle_world.cpp::append_native_frame_sounds` 读取帧顶层 `values.all("sound")`、最多排队20条。Unity `Lf2DatConverter` 将每条顶层 `sound` 收入 `FrameSounds`，但 `LF2FrameData.sound` 留最后一条；`LF2Entity.QueueNativeC25FrameSounds` 播放前者，`CharacterAnimtorManager.CollectBattleSoundIds` 预热后者。因此两处代码对未来多声音帧并不等价，但**当前正式 405 DAT 没有可触发这一遗漏的多声音帧**。不能据此改正式 DAT，也不能把未来内容的多声音支持写成已验证。
- 静态核对生产 C#（排除 `Test`、`Editor`、`UI`）中 `SFX_###` 字面值：13 个值与 `NTSDSoundPlayer.BuiltInBattleSoundIds` 的13个值完全相同，遗漏0；额外的 `data\\016.wav` 也明确在内建预热表。`AppManager.InitializeBattleAsync` 在第193行等待 `PrepareBattleCuesAsync` 完成后，才于第213行进入 `BattleRunning`。这些事实使当前正式帧声音和已枚举固定 SFX 在正常入场、成功预热的前提下不依赖播放时首次异步加载；它们不证明每份 WAV 都已成功解码，也不覆盖所有动态构造的 cue。

## 独立保留的条件门

`LF2CharacterDamageStateResolver.StateFrozen("state_exit")` 另排队旧字符串 `Battle/Ice/Shatter`；它不在上述13项预热表，正式 decoded DAT 中也未检到这个字符串。项目存在 `Assets/NTSD/Sound/Battle/Ice/Shatter/shatter_1_066.wav`。当前 `NTSDSoundPlayer.GetOrPrepareCue` 在战斗预热后封表，对清单外 cue 增加 `RejectedUnpreparedCueCount` 并返回空；另一个晚期状态转移写者在前帧 state13 或 action200 离开时排队已预热的 `SFX_066`。**本轮没有正式根程序同条件音频事件/PCM和原 Battle Scene 冰冻退出 Play**，因此不能判断旧字符串是应退休的重复事件，还是遗漏的正式音效，也不能为了让它播放而擅自加入预热表。

Q10 的“未预热异步路径”应收窄为上述旧冰冻分支、预热失败/控制器更换或尚未枚举的动态 cue 等条件门；当前正式帧数据的“前一条 sound 未预热”不是可达缺口。其它正式可达 cue、自然多声混音、动态音频相机、设备及正式 EXE 扬声器输出仍待，Q10/Q12/总目标均开放。若后续冰冻退出事件成为目标，应先从正式 playable 同条件音频事件确认 cue 与时点，再做 Unity 最窄首差，不改 DAT 数值或非战斗播放器。

本轮核对的 Unity 文件 SHA-256：`NTSDSoundPlayer.cs` `0203D7DB0DAF11AB4F9E64AD65C59B33414119A2D09414E1048E10DBA5AB0415`、`CharacterAnimtorManager.cs` `3A0501DC9993047546DBDB7316EE708C60760BEFAC089308B283F5BAD46241CD`、`Lf2DatConverter.cs` `D4AA32211B939989DD25DD59A6F040F8CA24047F149A4C6F6DBA804EE93E0722`、`LF2Entity.cs` `5C1C0D9575A6EFB0B34697697EB6ED5516F84E9D73EC35CD3E38D295B51DDBEE`。扫描输出为 `files 405 frames 55351 top_level_sound_distribution {0: 46487, 1: 8864} multi 0 distinct_values 976`；静态 SFX 集合差输出为 `missing_literals []`。
