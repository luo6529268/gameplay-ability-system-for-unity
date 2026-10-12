# resume-02：第二轮generic播放观察首差

状态：SCOPED_HOT_0EVENTS_PASS / SECOND_CYCLE_GENERIC_PLAY_OBSERVATION_FAILED。一次获准Play已使用，源码保持本次实测版，无追加组或修正。此文只交独立审阅，不批准新实验/生产修改。

本版raw-key/nonnull/精确wrapper修正已实际通过两个冷prepare，generic clip ID -1261106跨轮相同，public wrapper数组同引用、cache key/value身份相同。两轮三范围42份真实热scope前后校准有效且0事件；第一轮普通/1000请求、formal与generic播放观察、非零输出、公开关闭和重入完成。第二轮热范围完成后，RunThreeSessions第4125行`genericVoice != null && genericVoice.isPlaying`为false，测试停止。

第二轮此前formal clip/enabled/active/isPlaying断言通过（控制流证据），随后8帧output读取执行过，但outputMax及完整play-evidence行在失败断言之后，未落盘。因此不补造第二轮波形、generic voice状态或播放计数。

已只读核旧诊断helper H10ResidentClosureDiagnostics.FindVoiceWithClip:1248–1260：枚举host子AudioSource，返回第一个clip匹配者，没有按isPlaying筛选，也没有记录其余同clip voice。generic clip按合同跨轮复用，而旧voice可能仍挂着同一clip；该选择器存在返回停播旧voice、遗漏另一个正在播放voice的可能。**这是从源码得到的诊断风险，不是本次运行已经证明的原因。** 原件没有失败时全部匹配voice的ID/clip/enabled/active/isPlaying，也没有失败调用前后的generic播放计数，无法区分null、首个匹配voice停播、其它voice在播或实际未播放。不能认定生产播放回归，亦不能把未证明的“旧voice误选”当修复结论。

建议后继先审同PcBudget内最小观察增量：

1. 不改被旧测试复用的FindVoiceWithClip，不改生产播放/gain/retrigger/64voice规则。只在PcBudget冷观察段，对PlaySfx前后所有AudioSource记录稳定于该Play会话的instance ID、clip ID、enabled、activeInHierarchy、isPlaying与相关已有计数。
2. 明确记录旧helper选中哪一个、全部同clip匹配者和其中正在播放者；证据必须在断言之前落盘，不以只筛active来隐藏停播voice或证据缺口。
3. 将null、clip身份、enabled、active、isPlaying断言分开，保留正式cue与generic观察的时点。是否应以全部matching voices证明“该clip实际在播放”，须由独立审阅按既有规则决定；不得先改判定后刷PASS。
4. 若原件证明实际上无voice播放，再按获批的生产只读范围追原因；若需要生产代码或其他写域，先提交精确证据，不在诊断中补行为。任何新有界运行需另派发与新目录，不追加到本轮。
5. 三个recorder和此次42个scope的证据不受上述观察增量影响可按指纹复用，但第三轮、完整三轮借用不误停、真实pending取消/迟到及自然Destroy仍未完成，不以局部成功替代。

H10父项OPEN；Player/真实1000实体/整进程预算/目标PC/Android未测。原byte[97]失败、resume-01 raw-key错误、旧13/14与Menu事故等原件保持。
