# 第36批普通Brute精确缓存

## 当前结论
末次文件审计：334保护及11原dirty副本无差异、HEAD与三运行源SHA/双Scene同；validator1332/12 PASS（4247历史warning），git diff --check exit0。当前原Menu鲜读证明与原始窗口分开存档，没有新窗口或源改动。


SCOPED_GAIN_NOT_ADMITTED / RUNTIME_PENDING。两实际1000AI短窗已完成且有实质候选收益，H07性能仍FAIL；没有生产默认改动，没有120FPS/Android/native/完整0GC证书。下方WINDOWS_READY/PLANNED只保留事前记录。

| 本批均值，ms | Dispersed1000：34→36 | Combat1000：34→36 |
|---|---|---|
| 逻辑tick | 187.1289→84.2452（降低54.98%） | 183.1814→86.2220（52.93%） |
| CandidateCollect | 154.7478→52.6049（66.01%） | 152.6020→54.0975（64.55%） |
| PairExactLoop（36） | 52.0672 | 53.5334 |
| 显示帧间隔 | 527.0065→259.0010 | 482.9374→248.9521 |
| 逻辑tick P95（36） | 141.1311 | 128.7443 |
| 已完成帧main/render/GPU（36） | 189.9826 / 0.5832 / 0.7579 | 194.3083 / 0.6069 / 0.6935 |
| SetPass均值（count） | 1994.3146→1994.3146 | 1995→1995 |

比较不是同期A/B；每个冻结request65字段仅output变化。各120 warm/180 sample、17次实际观察AI/baseRoster下限1000，缓存300应用/0回退，三flag恢复，capacityCritical0，workload/harness有效/StoppedCleanly。显示帧仍约249–259ms而非流畅；collector仍占逻辑62.44%/62.74%，后续优先已有exact pair热点，其次CharacterInput约12ms与LateEntityUpdate约6ms。FrameTimingManager只接受本窗口已完成帧（main/render89、GPU81/79），不是截图因果诊断、FrameDebugger或GPU batch证明；高SetPass保留独立渲染风险，本批不解冻EXT1/ATLAS或改segment。

末tick300各20扩展/lockstep hash和完整JSON与34相同：Dispersed SHA F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA；Combat SHA E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。只是末帧相同、非逐tick/native准入。rawGC true仍UNCALIBRATED/UNKNOWN，H11严格完整FAIL保持。

容量观测缺口：Suite StartCurrentRun先启runner，随后在任何warm/sample前读取prepared容量；原件0/0/0与roster0/4为准备前快照，不是实际运行容量/bytes。现有代码每次使用前预检查硬上限及List.Capacity、300全应用能证明本次缓存运行准入，但无法补出准确容量/驻留字节。保持原JSON/UNKNOWN，不为该非性能字段再跑同版本窗口或宣称无此缺口。

关闭/编辑器：suite DONE / MEASUREMENTS_COMPLETED，objects/slots/borrowers0，Battle/Menu SHA保持。旧6401查询只读超时后，核对原PID19040实际监听6400，鲜读Menu8roots clean/idle/非Play/无测试；其他既有Unity进程未干预、没有新Editor/重启/重复启动。末次保护/validator/diff结果见post-run-audit-01.json，运行时与文件审计必须分别看。

证据：[两窗原始终态](windows-01/suite-result.json)、[同请求/耗时/末帧比较](window-comparison-01.json)、[鲜读原Editor](post-run-editor-01.json)、[事前审计](pre-run-audit-01.json)、[最终153测试](green-01.json)。源版本被冻结，没有测量后C#改动。

当前 RUNTIME_PENDING / WINDOWS_READY。新11有效RED全部缺接口失败；首次153回归仅新位置夹具漏SyncIntegerPosition失败，完整cache/legacy比较已通过，补自身同步且不改断言。focused11/11（3.124002s）、最终153/153（15.2827821s）；固定1000逻辑实体4warm/8交替A/B roster53.9446→cache22.4193ms，约58.4%局部降低，候选完整序列/RNG/Kind4保持。非实际1000AI/FPS、非native/0GC证书。

独立36菜单/root与应用/恢复接线已编译；累计applied/fallback基线差值覆盖全部collection，不以稀疏observer代替。334保护/11副本/HEAD/双Scene保持、validator1332/12 PASS，第一次diff只本批Index新EOF空行失败已修，原件保留。下一fresh Editor及diff审计后一次原两个120warm+180sample，不重复本版窗口、不切默认、不解冻专项门。35工具25/25留痕完整，长相机未启动/H11严格FAIL保持。

证据：[RED](red-01.json)、[首次GREEN失败](green-attempt-01.json)、[新11通过](focused-01.json)、[最终153通过](green-01.json)、[首次事前审计](pre-run-audit-attempt-01.json)。下方PLANNED为事前快照。

PLANNED，尚无代码/新测试/实测收益；只复用已有缓存表示，全部边界见Task。35工具25/25不是FPS改善，H07性能/H11完整0GC仍未达；Goal active、累计15新批、父关闭0。
