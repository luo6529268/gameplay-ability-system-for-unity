# Q10 小樱高血量自然 049/j2：336B44 根回放与 PCM 核对

状态：`ROOT_SELECTED_FIELDS_165_OF_165 / SOURCE_NATURAL_AUDIO_EVENTS / FORMAL_PCM_GAP / UNITY_VOICE_PENDING`。本包未修改正式发行 EXE/源码、Unity 生产、DAT、WAV 或 Scene，只运行既有 LFR 和只读文件对比。正式根 EXE 运行前 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

复用已存 [小樱高血量控制](../NTSD28-336B44-Q10-SAKURA-STEREO-NATURAL-REACH-001/source-run-04/source-events.csv)：HP500、OID1 小樱从 action0，防2 tick→纵深上2 tick→攻2 tick，55 tick 内没有进入低血量 `tra.wav` 分支。完整 playable `GameSession28` 仍自然在 tick6/12/19 发 `data/007.wav`，tick21 发 `c/saku/w/j2.wav`，tick49 发 `data/049.wav`，后两条来源 X500。该源码样本是低血量招式筛选的**负控制**，不是声音事件的负控制。

用同目录未经修改的 `source-packets.lfr` 运行正式根 `--headless-playback-lfr`，参数与输入 SHA 留于 [运行清单](run-manifest.json)。进程退出0，根 [报告](root-report.json) 为 `passed=true`、`failureCode=0`、声明55 tick、完成56 tick；末尾额外完成 tick 不计入55 tick同态。将根 [trace](root-trace.jsonl) 的slot0动作、MP、相机X与源码逐tick对照，**55×3=165/165** 同值，首差无；计算见 [比较](comparison.json)。这只证明同输入下根公开的可比状态与当前源码记录一致。根 trace 未导出逐条audio event，也未测正式 EXE扬声器。

正式 `data/049.wav` 与旧 Unity Sound 文件在采样长度相同的情况下 PCM SHA 不同：正式mono/8-bit/22050Hz/32908帧，旧mono/16-bit/22050Hz/32908帧。正式 `c/saku/w/j2.wav` 为mono/8-bit/22050Hz/7868帧，当前 Unity 正式 VFS与旧Sound均无此路径。完整原文件/PCM SHA及格式见比较JSON。当前战斗单文件解析因缺正式文件，会对049退到旧Sound；j2缺两个读取根。文件差异不单独证明设备听感或生产实播差异。

下一独立内容包可以只新增两条已证源码自然cue的正式WAV及meta，保留旧Sound和非战斗路径；原Editor仍在编译，Unity导入、正式SourcePath→clip→voice、Mixer PCM及设备须待恢复后验。其它高血量输入、其它cue和Q10/Q12总出口仍开放。`git diff --check`及文件删除审计不涉及本包既有文件，因为未改或删除任何原件；新诊断输出均使用此前不存在的路径。[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q10-SAKURA-HIGHHP-049-J2-ROOT-001.md)。
