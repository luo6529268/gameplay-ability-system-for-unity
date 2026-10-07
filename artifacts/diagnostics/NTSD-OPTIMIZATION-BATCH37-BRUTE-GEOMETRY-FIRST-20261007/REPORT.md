# 第37批普通Brute几何粗判前置
结论：SCOPED_GAIN_NOT_ADMITTED / PERFORMANCE_FAIL / RUNTIME_PENDING。本批 ordinary Brute 粗判前置候选在两固定1000AI短窗有有限收益，仍未达到33ms/流畅显示目标，不推广生产默认。H07/H11/Goal仍开放，34父项关闭0，新批累计16（22–37）；专项门不变。

## 实施与测试

核心default-false EnableBruteGeometryFirstForDiagnostics仅已有36 exact-cache普通Brute路径显式使用；先已有纯几何粗判，miss仍按原base/slot条件读IsBound保存HasVrest失效绑定清理副作用。pass进入原规则且不重复粗判；原i<j/a→b/b→a、kind5 coarse、payload/body/RNG/kind4保持。Role/platform不接入，无新容器/owner、热路径扩容或默认切换。
test-first RED job c2336d8c47044b9caf647be3decde931 completed11：10缺API，1为本批fixture错误预设旧路径attack-exempt保留stale binding。重扫证明快照准备写target Arest=0已清理，无候选原因；纠正该预设并保留A/B完整状态断言。首GREEN11/11；最终原Formal＋Suite164/164 PASS（job d8fa5b77de6d4c94a924bf81df1a5650，16.3053776s）。
局部1000逻辑实体40active/960inert、4warm/8交替：cache22.558275→geometry20.105775ms，非1000AI、FPS或0GC证书。完整结果保留red/green/focused原始JSON。

## 两个实际1000AI短窗

各120warm+180sample，seed1314149188；与36所有65请求字段仅outputPath不同，baseline非同期，不能称稳定统计估计。suite DONE/MEASUREMENTS_COMPLETED，两次workloadValid/StoppedCleanly；16次观察各最低AI/baseRoster1000。cache/geometry各300应用、cache0回退，四flag恢复。

| 指标（ms） | Dispersed：36→37 | Combat：36→37 |
|---|---|---|
| logic mean | 84.245228→79.670794（-5.43%） | 86.222018→80.611748（-6.51%） |
| logic P95 | 141.131145→134.272115 | 128.744330→123.676880 |
| CandidateCollect mean | 52.604885→48.307897（-8.17%） | 54.097501→50.116486（-7.36%） |
| PairExactLoop mean | 52.067221→47.797916 | 53.533434→49.566960 |
| visible Unity frame mean | 259.000964→249.768591 | 248.952076→235.677777 |

collector仍占logic60.63%/62.17%；CharacterInput11.84/11.46ms、LateEntityUpdate5.96/5.69ms。显示远未流畅；SetPass1994.3146/1995完全未降。现有FrameTiming accepted main89/89为181.14/182.41ms，render89/89为0.564/0.600，GPU78/83为0.680/0.717ms；这些不是GPU capture/真实batch认证，不用来归因用户截图1490ms，也不是生产默认FPS改善。没有启动新Profiler/FrameDebugger/GPUcapture/EXT1专项M0。
末tick300两类各20finalParity/Lockstep hash全同36；完整snapshot SHA分别F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA、E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12，与36同。不是逐tick/native准入。
本批post-preparation观测：roster/participant容量1050，body5250，itr14700；roster声明payload下界4204B非全部managed/native内存。36准备前0/0/0原件不改，不能反向补为当时已测。观测max geometry reject145920/137350，不冒充全300tick累计拒绝。capacityCriticalDelta0；rawGcGatePassed=true仍UNCALIBRATED/UNKNOWN，不撤销H11完整0GC严格FAIL。

## 安全/恢复与剩余门

11阶段objects/slots/borrowers均0；Menu/Battle SHA同，原Editor PID19040当前6400恢复Menu8roots clean/idle/非Play/无测试。源码从最终回归到终态SHA同：core FFBC1A8A4725FA04CB0150E8743EA14C56D6749D168D44382A8CE4320CC68A92；tests 003B58517BFCD373A8C2156B75CB285DCF4EDAE24AE500D29291BCA98AD2F620；suite 893E7F3624D244AB1CA195D087DC7BB825AFC7EF640E09A289BD8E4879F7167D。
379非写域0差异/11备份0差异，HEAD8107196b1f17ee0f7ce9e7fcbb7ffb9fc260956c不变；最终validator/diff审计见本批final-validation原件。未删除/回退用户改动，未读Q06活跃方法体。只本批三源/声明docs/现有suite终态范围。
RUNTIME_PENDING指生产准入/native/逐tick/完整0GC/性能门待，不是两短窗未跑。不重复37短窗刷PASS，不把约6%逻辑收益当流畅；继续依据仍占61–62%的collector与第二热点AI input推进最小有证改动，默认false/专项门保持。不是Android/120FPS认证。

