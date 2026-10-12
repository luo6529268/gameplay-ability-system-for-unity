# resume-01 首差定位与同文件最小后继审阅

状态：MEASUREMENT_QUALIFIED_FIXTURE_CUE_ID_MISMATCH。本轮额度已实际使用一次Play，未追加第二组；失败版源码保持。本文件是独立审阅输入，不是执行批准。

实际原件：unique-play-tests.xml为0/1 Error，15.118494s；play-result.json首差是RunThreeSessions第4056行ReadPrivateCue对null wrapper调用GetType。发生在第一轮public prepare完成后的冷检查，正式热循环/播放/三轮及取消/自然Destroy尚未运行。生产分配缺陷未证明。

本轮新增常量PcBudgetGenericCue写为`"data/016.wav"`，这是执行者把磁盘相对路径写法误用为sealed catalog标识。资格只核磁盘路径存在，没有核public prepare实际注册的raw key；冷阶段又漏掉了读取字段之前的wrapper非null断言。二者均为本次夹具缺口。

只读源码链（NTSDSoundPlayer.cs，不是Q06）：

- BuiltInBattleSoundIds第27行为`@"data\016.wav"`。
- PrepareBattleCuesCoreAsync第338–356行按原soundId集合调用GetOrPrepareCue。
- GetOrPrepareCue第524–535行先按原soundId查询generic preparedCues；catalog sealed时miss计拒绝并返回null。
- 第548行的NormalizeRelativeFolder发生在sealed检查之后；第591行generic注册仍是preparedCues.Add(soundId, preparedCue)。磁盘路径规范化不等于给generic目录注册别名。
- 本次predeclared-generic-routes.json的真实builtin ID和唯一cache key都是反斜杠data\016；精确wrapper查询使用了另一字符串，与null首差吻合。没有证据支持改生产以接受新别名。

后继最小修正建议仍限PcBudget新增增量：

1. 使用真实注册ID`@"data\016.wav"`查询和PlaySfx，保留原根、五formal代表及builtins。不创建新alias、不改生产归一化或封存合同。
2. 无解码资格增加对只读BuiltInBattleSoundIds的Ordinal精确成员断言，并记录原字符串、路径和cache key三个不同概念；要求只有真实路径对应的raw ID成为本夹具负控。
3. 冷prepare后先Assert.That(generic, Is.Not.Null)，再读取Clips；以只读preparedCues中已有raw key核wrapper同引用，证据先落盘再断言clip/缓存身份，避免空引用掩盖首差。不以另一root或额外load补齐。
4. 本轮通过的三个独立recorder资格与原始前后校准可在未改动指纹下复用；不要把这三个空范围资格结果当作生产热0GC。Play前仍要新鲜安全资格；若后继版本获派新的有界组，重新登记源码/程序集和新CreateNew目录，旧失败保留。
5. 本轮关闭证据中的pendingAtStage5Return变量实际采样于整个公开ShutdownBattleRuntime返回（stage10），不能解释成独立stage5时点测量。后继仅修正新增诊断字段名/时点说明，不改变关闭流程。

后继需补齐尚未执行的三轮真实音频负载、热scope、实际clip/isPlaying/波形、取消迟到和自然Destroy。旧正控FAIL、当前cue-ID首差、旧13/14、Menu历史UNKNOWN均保留。无Player/Settings/其他优化任务。
