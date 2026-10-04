# Q10 我爱罗自然 j4/043 战斗声音

状态：`VERIFIED_SCOPED_NATURAL_VOICE`。原项目唯一 Editor 在已保存、clean 的 `NTSD_Battle.unity` 运行一次40tick Play；原 Editor 脚本导入后，生成工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` 退出0、0错误（299警告）。未运行完整测试集。

按当前336B44正式OID16/action0/mode0/seed682973786，Play clone 中我爱罗X800/Z650、远距OID7 X1200/Z650，离散防2→右2→攻2；Unity项目可行走区将双方Z钳到481。生产`SimulationTickDriver.StepOneTick`从全局tick5推进40tick到45。[逐tick原件](gaara-j4-043-scene-01.json)与[第一轮配对](comparison.json)中，正式Z650源码对Unity的phase/action/实际PP/X/Y五字段**200/200同，首差空**；Z因项目地图边界40tick均650对481，按用户保留的地图例外不作同坐标结论。

正式源码自然tick13为action243并发`c/gaa/w/j4.wav`，tick15为action244并发`data/043.wav`；Unity这两tick各有一条对应`PendingSoundEvent`，`PooledOneShotPlayCount`各增长1，两只`AudioSource`分别持正式LoganRuntime路径的AudioClip且同tick`isPlaying=true`。正式j4/043 clip均为单声道22050Hz，样本数11620/13641；正式WAV及旧`Sound/data/043.wav`前后SHA稳定。此证据证明生产播放器实际发声入口，不是Windows设备波形或正式EXE扬声器录音。

Play后自动回到非Play，原场景clean；[独立残留回执](gaara-j4-043-postplay-01.json)为`SCOPED_PASS`，Scene Driver 1、World绑定0、Scene Pool0，Battle/Menu Scene和两配置/两正式WAV/旧043的磁盘SHA均与前值一致。没有改生产、DAT、旧Sound、Scene或非战斗。

`comparison.json`同时记录tick27 Unity额外`SFX_001`/`SFX_006`标记；它不能从此样本判成可裁决首差。原C++第一版CSV只写资源路径，漏掉builtin channel；后续补证在同tick看到正式内建channel2，对应sound.dat的`data/006.wav`。更重要的是正式根tick27对手action180、Unity186，双方Z边界已不同；正式初始Z481控制在tick1就被原版背景钳到542，仍无法建立同Z。见[单独诊断报告](../NTSD28-336B44-Q10-GAARA-BUILTIN-CUE-NORMALIZATION-001/REPORT.md)。不据此改共享碰撞/声音生产。
