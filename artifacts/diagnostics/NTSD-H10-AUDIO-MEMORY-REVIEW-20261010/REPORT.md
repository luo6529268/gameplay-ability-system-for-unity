# H10 音频内存独立评审
状态：SCOPED_REVIEW_COMPLETE / READY_FOR_BOUNDED_IMPLEMENTATION。生产未改。

用户授权本讨论会话检验H10，形成准确优化方案后交“处理优化方案”会话实施。先前H08默认接入保持。此为H10 PCM预处理/加载内存子项，不是按局音频闭包或资源格式迁移。

## 当前事实
NTSDSoundPlayer.PrepareBattleCuesAsync收集全部已加载配置声音及内建cue。formal cue有路径别名复用；单声道转双声道是既有增益适配，不能直接删除。每条mono原PrepareBattlePlaybackClips分配samples长度mono和2*samples长度stereo float数组，再创建双声道AudioClip。ReleaseBattlePlaybackClips仅管理播放副本，loader/cache另有所有权，不能随手释放借用原clip。
磁盘978 WAV/965mono，最终双声道样本payload220188696B约209.99MiB，等于历史估计，但不是当前battle实测。原始clip的Profiler对象内存不能按float32估计替代（实测8bit源比float估计小）。不能把原clip、临时数组、播放副本或RSS混在一起。

## 收敛方案
只对mono且samples>=8192使用4096采样帧分块；短音频原全量转换。完整块复用同一mono[4096]/stereo[8192]，尾块精确remaining长度，避免SetData环回覆盖开头。最终AudioClip仍完整双声道、相同name/samples/frequency、逐样本相同。仅每次调用的局部数组，不跨cue/cache/manager，既有owner与失败清理保持。
当前Editor镜像最多4个float数组，数据payload严格小于98304B；长源原为12*samples。数组对象头不在payload数字中，小于8192原两数组无额外对象头。没有更改音频资产、格式、音量、声道、事件/voice/retrigger/原clip寿命。

## 实际验证
原Editor PID39108/Unity2022.3.62f3非Play，新增一个Editor-only诊断。
最终job3414d8585d874140858fc9b51c6c709c实际2/2通过，10边界rows（1/2/4095/4096/4097/8191/8192/8193/65537与stereo借用）及4真实WAV。直接调用生产private转换，对比候选镜像，输出左右声道每个float位相同、输入未改、samples/frequency一致。
真实最大e3.wav329244frames：原temporary float payload3950928B，新67920B；原/新最终clip Profiler对象内存同为2636720B。其余三条见analysis-summary。显式原owner cleanup属于诊断清理，不冒称自然OnDestroy验收；本次owned clips均释放/借用source先保持再由唯一测试owner释放。

唯一冷转换成本组jobe977e59321f444fba32d7e21532f6420实际1/1执行成功，2长度各OFF/ON/ON/OFF、4warm16sample。时钟含转换/AudioClip创建，不含反射delegate准备、输入准备、readback/assert/cleanup。原方法使用冷编译delegate调用，ON为Editor镜像；不是同production接线比较。冷转换允许分配，不要求0GC。65537frames原mean0.703225/0.567181ms，新0.299963/0.364444；329244frames原3.421575/3.155081，新1.576106/1.619956。16样本P95/P99都落max，不能当稳定长尾估计。原始ticks已独立复算；支持进一步生产验证，不承诺整局加载提速。
全磁盘965mono若每个转换一次，临时float累计payload数学估计330283044→68499492B；不是同一时刻peak、RSS或实测整库收益，更不是节省210MiB播放副本。未实际加载整库或改变加载范围。

## 失败与边界
最初未import新type的job total0不是PASS。class-filter job初始化超时但后续原件落盘；随后exact-method执行完成断言却因CreateNew同名输出拒绝覆写，失败原件保留。最终只改诊断输出UUID、短源保留原路后重新精确运行2tests PASS。execute_code只读探针因命令行长度失败，未执行；改用现有Editor tests/scene工具，未重启或第二Editor。最终成本仅一组，不重跑找PASS。
缓存侧审发现single并发/late cache等静态风险，但当前声音预热顺序await，实际重复频率未知，不作为本候选收益依据。formal/legacy重叠75路径仅2文件相同，禁止按相对路径跨root合并。

## 实施边界
准确指令见execution-handoff.txt。只NTSDSoundPlayer原private转换及H10诊断，保留原Prepare入口、loader、generation、所有权、声道增益和声音规则。不做新音频管理器/lease/跨路径去重/按局筛选/AB/资源改动。后继须先最新实际生产转换等价/失败清理，再一次有界比较确认不引入明显加载退化，必要原增益/retrigger测试复用。H10父项仍OPEN，整局驻留/峰值/设备未测。

## 保护
87生产/Scene/Settings保护及978WAV全部SHA不变；5源码程序集成本前后相同。7真实dirty文档备份+诊断中间备份保持。最终validator、diff和Scene证据另附；不得把仅脚本编译当生产应用。
