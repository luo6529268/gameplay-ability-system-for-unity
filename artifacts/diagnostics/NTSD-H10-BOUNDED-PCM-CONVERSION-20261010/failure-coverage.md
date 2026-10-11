# H10故障覆盖与源码保证
生产NTSDSoundPlayer.cs仅PrepareBattlePlaybackClips中长mono分支和CreateBoundedBattlePlaybackClip新增，原catch/Release/OnDestroy/short路径与原备份逐字归一化一致。

实测：stream=true AudioClip.GetData返回false，Unity确实记录Can't get data from streamed samples错误；原生产返回错误Cannot prepare battle PCM: <source.name>。
同cue首条stream失败无目标clip残留；第二条stream失败时第一条有效longclip已完成转换并加入owned，catch撤销本cue新增而先前独立cue的1个副本仍活，cue.BattlePlaybackClips仍null。指定fixture的native AudioClip实例ID集合与失败前完全一致。新输出不是通过伪造失败注入产生。

未动态触发：同一个可读clip的第一块成功之后后续GetData失败；本方法新创建、非stream、合法非空缓冲与合法offset的SetData返回false；巨大clip checked溢出。未改生产加入注入/替代AudioClip。

源码保证：
1. helper仅在调用者try块内调用，无局部catch吞错；firstOwned在cue前记录。
2. 第一次GetData在checked总samples*2、AudioClip.Create和owned.Add之前。首次读取失败不会创建该clip目标。
3. checked(totalFrames*2)显式保留，成功读首块后、创建目标前执行。之后所有乘法最多4096*2或remaining*2，不绕过原总长度拒绝。
4. AudioClip.Create后下一条语句即ownedBattlePlaybackClips.Add，早于第一次SetData。创建后的每次GetData/SetData false抛原InvalidOperationException文本，均落回原catch，从Count-1回收到firstOwned。
5. publication仅整个sourceClips循环结束后的cue.BattlePlaybackClips赋值。helper只在全部块完成时return；异常没有部分publication。
6. 每次写入offset为已完成帧数，mono.Length为4096或最后0<remaining<4096。offset+mono.Length<=totalFrames；stereo.Length=2*mono.Length；尾部到达totalFrames立即返回，故无尾部满缓冲环回。Unity的GetData/SetData offset以采样帧推进，实际bits边界/倍数已覆盖。
7. 样本有效但native API中途失败的动态故障位置仍属于未执行覆盖，不记PASS；按派发允许用具体源码证明并报告缺口，未以此制造生产故障注入，也没有取消可执行的真实失败门。

官方API核对：
https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioClip.GetData.html
https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioClip.SetData.html
