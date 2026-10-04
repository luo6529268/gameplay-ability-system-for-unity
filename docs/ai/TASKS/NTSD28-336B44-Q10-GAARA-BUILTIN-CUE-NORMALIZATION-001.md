# NTSD28-336B44-Q10-GAARA-BUILTIN-CUE-NORMALIZATION-001

状态：`VERIFIED_DIAGNOSTIC / TICK27_INCONCLUSIVE_MAP_BOUNDARY`。Q10我爱罗原Scene40tick已证j4/043正式clip播放；全音频路径对照在tick27出现Unity额外`SFX_001`/`SFX_006`，但原独立C++诊断只记录`event.resource_path`，对正式`builtin_channel`为空，尚不能裁决多声。

只修改 `Tools/NTSD28Q10Diagnostics/gaara_sand_blast_043_probe.cpp` 的诊断CSV输出，使每个 `WorldAudioEvent28` 记录 source/channel/resource_path 的规范值，保留原`audio_paths`列与旧样本不覆盖；用原正式playable闭包编译并只重跑同一40tick到全新输出目录。第一次正式Z650复核发现根tick27目标动作180、Unity186，Unity项目可行走区将Z650钳至481，不能把此条件下的音频列表直接裁决为同初态首差。故在同一诊断脚本增加可选的整数初始Z参数，保持原Z650默认，仅额外运行一次正式Z481控制（双方同Z），再与既有Unity原Scene逐tick待播列表及双方目标状态比较。正常内建channel映射为Unity `SFX_NNN` 时须核对正式渲染/声音消费者，不能仅凭相似字符串假定。若有非例外首差，再另建精确生产包。

只读取现有原Scene结果，不再启动Unity Play/第二Editor或跑全套；不改正式源码/EXE、生产、DAT、WAV、Scene、非战斗。既有原CSV/报告保留，修订写v2报告。变动前后保护文件哈希及Change Ledger/diff check。无需删除；若需撤销，仅前向更正并按文件操作合同处理任何删除/覆盖。

结果：[诊断报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-GAARA-BUILTIN-CUE-NORMALIZATION-001/REPORT.md)。正式tick27有builtin channel2，按正式sound.dat对应`data/006.wav`，解释Unity SFX_006；SFX_001是否额外不可在两种不同地图边界下裁决。正式Z481控制tick1即被原背景钳到542，Unity项目地图为481；正式根目标action180、Unity186，碰撞音效不是同受击状态。按停排规则不做生产修复或再跑Unity。
