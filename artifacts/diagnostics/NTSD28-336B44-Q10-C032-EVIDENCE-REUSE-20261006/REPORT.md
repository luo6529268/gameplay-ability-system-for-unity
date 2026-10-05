# C032 自然 voice 与隔离软件 PCM 证据复用

状态：`HISTORICAL_RAW_EVIDENCE_RECHECK_PASS / DOCUMENT_CORRECTION_ONLY`。权威仍正式336B44；父C032为RUNTIME_PENDING，Q10和总体开放。本轮未运行Unity编译、测试或Play，无新生产任务。

10月6日重新核对10月2日voice01、10月3日mono-pcm04和F03 scene03原始JSON：均PASS/DONE、128个完整Driver tick、Scene前后hash相同、clean/exitedPlay为true；voice/PCM完整128项samples分别与F03严格相等。两次自然data/016.wav在相对tick60/X373、tick66/X360，池播放计数3→4、4→5，assigned/playing、mono22100Hz/8158样本。

PCM04捕获48000Hz/9216软件立体声帧。重算左右RMS比15.666673766652398，对来源报告采用的固定全视野origin X0、94:6矩阵15.666666666666666，绝对差7.0999857318e-6，小于本次声明的1e-5容差。两个早先voice仅在捕获窗静音、无其它未静音源，原静音标志恢复且AudioRenderer停止。此结果只证明隔离的首声软件输出，实际路由为<master>，不能当作自然完整混音或Sfx Mixer组证明。

当前Unity与正式016.wav重新读取后，PCM SHA均为50A75E61ACBF4CD2A985CAD642E2B2901EF1835661441BBB2ED3818046121A6C，mono/22100Hz/8bit/8158frames。整WAV文件SHA不同但PCM相同，不引出文件替换。

更正总表Q10/C031/C032及父C032 Task/Record，取消旧的一概voice/声像待验排期；旧历史保留。正式根逐条音频、自然并发完整混音、硬件设备及其它条件仍未知，按实际非例外首差或用户新要求触发，不自动列入必跑。历史成功未被写成今日重新Play或全场验收。

原件路径/SHA/18项检查和限制见[evidence-evaluation.json](evidence-evaluation.json)；本次精确四文档操作及备份见docs/ai/FILE-OPERATIONS/NTSD28-336B44-Q10-C032-EVIDENCE-REUSE-20261006/RECORD.md。没有C#/DAT/WAV/Scene/配置或非战斗逻辑改动。
