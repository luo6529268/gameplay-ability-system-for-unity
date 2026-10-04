# 我爱罗 tick27 内建声音的可比性核查

状态：`VERIFIED_DIAGNOSTIC / INCONCLUSIVE_MAP_BOUNDARY_FOR_TICK27`。这是音频诊断范围裁决，不是战斗规则或播放器修复。

原C++探针的`audio_paths`只写`WorldAudioEvent28.resource_path`，因此漏掉`builtin_channel`的空路径。按当前正式playable闭包重新编译两次，`build-02`/`build-03`各退出0、stderr空；`run-v2-01`保留正式初始Z650，新增`audio_tokens`列在tick27记录`path:data/033.wav|builtin:2|path:data/069.wav`。正式`resources/runtime/decoded_dat/data/sound.dat`按声明顺序将channel2映射`data/006.wav`，它与Unity的`SFX_006`代表同类内建声道；旧CSV因缺字段误把这一条看成Unity额外声音。原LFR SHA仍为`E22F52123DA1E7287959AB9964AEEAF58D8C42185A556990D904053BE9D1D9C1`，没有覆盖原CSV或回放证据。

Unity在tick27待播列表为`SFX_001;data/033.wav;SFX_006;data/069.wav`，且对手action186；正式根同tick对手action180。Unity项目地图将Z650钳到481；为排除初始Z不同，`build-03`在不改默认参数的前提下只加可选初始Z，`run-z481-01`再执行一次40tick。正式背景在**tick1**把481钳到542，Unity仍为481，所以两轮都无法建立碰撞时的同Z/同目标状态。正式Z481样本tick27仍记录`data/033|builtin:2|data/069`。`SFX_001`是否是多余的声音事件，在这两种不同地图边界和目标动作下**证据不足**，不创建生产修复或额外Unity Play。

我爱罗j4/043的tick13/15事件及实际voice已由独立同技能窗口证明；tick27差异归用户保留的项目地图边界后的条件门。要复查须先有同Z、同受击状态的可复现非例外自然样本，不能为了这次阴性扩大扫描。只更改`Tools/NTSD28Q10Diagnostics/gaara_sand_blast_043_probe.cpp`诊断输出及可选初始Z；正式源码/EXE、Unity生产/DAT/资源/Scene均未改。
