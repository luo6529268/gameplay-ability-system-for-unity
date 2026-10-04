# Q10 已归档当前战斗事件样本：音频内容覆盖清单

状态：`VERIFIED_15_PATH_DISK_CONTENT_ONLY / UNITY_VOICE_AND_BROADER_REACH_PENDING`。只读核对当前正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`、正式 VFS、Unity 正式暂存 VFS和旧 Sound，未修改 DAT、WAV、脚本、Scene或非战斗资源。

从已归档的当前336B44源码/原Scene具名样本整理15条战斗事件路径，逐条来源、原文件SHA、PCM SHA、格式和分类见[CSV](coverage.csv)及[机器清单](coverage.json)。其中 **10条已有正式 WAV暂存且原文件 SHA 完全一致**；另外 **5条尚未暂存正式 WAV，但旧 Sound 的 PCM SHA、声道、位宽、采样率和帧数均与正式文件相同**：`data/007.wav`、`012.wav`、`016.wav`、`017.wav`、`101.wav`。在这15条样本内，没有发现“正式/旧 PCM不同且正式文件仍缺”的内容项。`016`来自C032声道6经正式sound.dat映射；`078`来源CSV计数列，a7/017来源鸣人事件列，其余来源对应逐tick音频路径。不同样本含自然按键、受控初态和负控制，不能合称15条玩家自然可达。

此结论**只覆盖这15条已有事件证据**，不证明当前正式DAT其他声明、970条历史词法路径或所有战斗条件已经可达/可播放。它也不证明旧 Sound 的Unity导入后Clip、battle-only正式文件优先选择、音量/声像、Mixer或设备输出：原Editor仍编译未完成，且根EXE公开trace缺逐条音频。已暂存内容的独立Task仍需各自原Scene自然voice和运行时验收；此清单不替代Q10或Q12最终出口。

下一步应停止对这15条重复做文件级复制，优先在原Editor恢复后验证已有正式WAV的SourcePath→Clip→battle voice和PCM/设备；若有**新的**当前336B44实际可达事件，才按首差补清单，不从970路径批量搬WAV。其它Q07/Q09战斗规则/表现子项可独立推进。当前Q10仍`IN_PROGRESS`、Q11/Q12和总目标仍开放。[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q10-OBSERVED-CUE-CONTENT-COVERAGE-001.md)。

落盘后复核：15/15来源文件存在，分类仍为正式暂存10、旧PCM及WAV格式同版5；10份暂存WAV再次逐SHA同正式，Battle/Menu/两配置保护SHA 4/4稳定。相关文档 `git diff --check` 退出0（仅有Git工作树换行提示），新Task/报告无行尾空格。没有运行Unity编译、SelfCheck或Play，本只读清单不需要以其它角色/场景的整套测试背书。
