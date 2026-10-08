# 第66批：普通包络成功证明复用资格

## 结论

SCOPED_CORRECTNESS_PASS / NO_GAIN_OBSERVED / NOT_ADMITTED。最小候选确实省去了960/7643次重复coarse证明，65去重聚焦case均有通过证据；但唯一两个1000participant局部平衡对照未观察到收益。不晋升默认、不继续本候选Driver或千人实景、不重复采样找PASS。H07/H11仍OPEN，六项阶段4/6，Goal active；本批不是优化成功或120FPS/Android认证。

只改两个准确C#：Query新增默认false开关/Last与Total primitive、原入口Last清零、原成功envelope无kind5/完整前置下传现成coarseFilterPassed；53dispatch开启时不复用。原builder/原coarse谓词、拒绝绑定清理、PairAllowed、每itr/body/depth和record顺序不改。原Q06引用reset不改，Q06排序器方法体不读。新测试仅原FormalCollector类，两个Cost fixture只比较本flag。

## 实际测试与失败留存

| 作业/原件 | 实际结果 | 解释 |
|---|---|---|
| RED ac1d2f3c1f7c45bcaf93bafb6714eb56 / red-tests-01.xml | callback27case/0pass/27fail | 都是新反射接口缺失，不是编译失败或0case。bridge在reload后终态initTimeout/completed0/resultnull，与callback不一致；原terminal JSON保留，不能称job正常完成。 |
| GREEN 289df5a4c7554d0e94b1e466bfa250a7 / green-tests-failure-01.xml | 63case/62pass/1fail | 新26＋旧58/65直接影响36通过。唯一Default用例误用会关闭四default的GetQuery夹具；原失败保留。 |
| REPAIR 10d7334774ca4d2ca7212587403f88ae / green-tests-repair-01.xml | succeeded/1pass/0fail | 只该新用例改为直接world.SceneQuery获取，四原defaults断言不减，Query不改。没有重跑已过62。 |
| COST 60b2537f2fba4298ba9f2448b1c4ad51 / cost-tests-01.xml | succeeded/2pass/0fail | 两布局各唯一balanced4warm＋8sample/侧，双方完整CandidateRun、pairSnapshot/handles/RNG同、OFF reuse0/ON>0；不是实际1000AI。 |

因此27新纯语义＋36旧纯语义＋2成本=65去重case都有通过证据，不称一次65/65新鲜整轮。实际匹配group和job时序在Change Record；旧58cost/65实景/全历史未复跑。RED原件SHA6AC76E16376EAF568A43105BC4BD8BA150FF4A839977C4646A92771E58C6371F，GREEN初失败415AF5900F6D6F0D9B3DDD4C80DDDF914394F7EA211E15ED4504E1F69F3C0D9E，REPAIR7438C6276EF9FC6BA0D01C8C175DCC30A8C6FA8510D51CDB82F091F912EDF351，COST6C0508A60DAAB806E7B21377668CEB2D5B94828CF098A61F540AB093B16B57C5；每次下一回调前fresh复制并核SHA，旧结果不覆盖。

## 唯一局部成本结果

固定1000participant、40攻击者、CollectionSeed，双方exact/geometry/envelope/rejectedBinding ON，其它eligibility/kind5/dispatch/timing OFF；只新proof flag OFF/ON交替先后，每侧4warm+8sample。测capture→collect→EndConsumption，不含实际AI/完整Driver/真实camera。GC=UNKNOWN。

| spacing | OFF mean ms | ON mean ms | ON变化 | 实际reuse/collection |
|---|---:|---:|---:|---:|
| 120 | 17.5847375 | 17.621775 | +0.2106% | 960 |
| 12 | 42.3000125 | 42.5899125 | +0.6853% | 7643 |

两差异较小、样本重叠，只有“本次未观察到收益”的结论，不能确证候选必然变慢或跨硬件收益。保持全部样本，不重采：

- spacing120 OFF：17.6025,17.5978,17.4072,17.5722,18.0878,17.4276,17.4983,17.4845；ON：17.6032,17.6558,17.5737,17.7689,18.1472,17.4286,17.2039,17.5929。
- spacing12 OFF：41.0159,43.8157,41.8475,43.0539,40.2955,41.2178,45.1501,42.0037；ON：44.2824,44.0617,43.5924,44.1141,41.8041,41.861,40.5132,40.4904。

## 保护与证据边界

10初始dirty/公共Temp副本全部在旧治理/Task/C#变更前复制核同，234保护清单先后核验见Operation；HEAD527350afa08633357ba20e7453a9d260eaa2b97c保持。Query finalSHA1534E0BF9429AE1D3987D823684AF32B65F17B6C3BBCE0BC3086614528A72105，Test finalSHA2759690A0A5E77C654B40506A43E242B32CA63163AE638BE8BD994660182E791。原Editor19040持续、最终Menu单Scene/8roots/saved clean/idle/noCompile/noTest；本批未进入Play、未切Scene/保存Scene、未启动第二Editor或新Profiler/GPU/M0。

Tools/Validate-ChangeLedger.ps1真实exit0/PASSED/1363Records/4 governed diff，4320历史WARNING、0ERROR，summary已存，完整stdout未另落盘。最初C#/tracker scoped diff exit0；最终扩大到四治理文档的git diff --check exit2，报告其现有EOF blank line（不触碰并行工作），不能称全树whitespace通过；后继逐副本对照和最终保护另存。无破坏性Git/删除/移动/覆盖用户改动。

04:04:01.7221262Z逐核234guards/10backup漂移0/HEAD及两源同。四治理文件last200字符等初始dirty副本、前后EOF换行bytes均3，所报EOF空行是既有内容，未清理；两个C#及唯一总表范围diff exit0（3条CRLF warning）、6个新Markdown无尾随空白。公共XML保持Cost同SHA，原EditorMenu clean。本项最终audit记录实际guard/source/backup/检查状态，不泛称整工作树通过。

输出更正：terminal-audit-01.json因工具1000token截断而JSON无效，原件保留、不作为完整证据；足额输出并先JSON.parse通过后保存terminal-audit-02.json，必要SHA/PID/HEAD只读重核仍全部相同。后置保护证据以02为准，未重跑测试、成本或实景。

没有本批完整Driver/native准入、可靠logic或完整camera0GC、实际1000AI/FPS/GPU batch/字节预算或设备证据；65原性能FAIL、43迟发12event、61budget UNKNOWN及所有专项门保持。局部候选默认OFF，无新资源/heap/buffer/owner；primitive字段不是实测字节预算证书。

## 后继

本候选判断已闭合，不重跑本方向。[本次新代码依据](NEXT-ACTION-FINDINGS.md)与[第67批READY任务](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH67-RENDER-TEXTURE-BINDING-REUSE-20261008.md)只定义下一现有RenderPass重复纹理准备资格；未实施/收益未知。它保持每segment/DrawMesh/顺序，不是GPU instancing或EXT1专项M0，不改变bank/预算/格式；实施前独立Task/Change/Operation准确副本，阶段仍未完成。
