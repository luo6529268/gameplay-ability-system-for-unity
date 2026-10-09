# 第89批：已校验帧引用复用评估

结论：FOCUSED_TEST_PASS / NO_CONSISTENT_GAIN / NOT_ADMITTED / DEFAULT_OFF。局部验证通过，但集中夹具成本回升，不进入Driver/真实1000AI或生产默认。H07仍FAIL、Goal保持active；68已执行、阶段5/6、34父项关闭0。
Task：NTSD-OPTIMIZATION-BATCH89-ROLE-SNAPSHOT-FRAME-REUSE-20261009；Change：NTSD-OPT-H07-ROLE-SNAPSHOT-FRAME-REUSE-089。

## 实际改动与语义边界

准确三个C#：BruteForceSceneQuery默认OFF开关/collection计数，RecordOverlappingBodyCandidatesCached仅Role且无after-pair测试hook复用已校验current/tick四帧；Factory新增CaptureWithResolvedFrames和一个共用CapturePayload；FormalCollector原类新增20具名case/helper。没有新容器、buffer、owner/关闭阶段、Grid/Burst或collector生产默认改变。
原Factory guard和四查询保持，previous native frame、oid/type/team/action/facing/linkedHolder仍在原capture时点读取。原payload从previous lookup起normalized尾文同；旧测试移除本包插入块后normalized全文同，Query仅三个精确hunk。Brute/input fallback/legacyCapture不走新入口，eligibility/reject/kind4/source-count/nearest/capacity/select/RNG/store原链保持。当前33项测试不声称正式EXE千人全域对齐。

## 实际测试

- RED job f1ff2f67ef124b388018274ba829e845：20/0PASS/20FAIL/0skip，均缺新API的预期失败；UTC2026-10-09 00:16:52—00:16:54。原件[red-01/tests.xml](red-01/tests.xml) SHA CC2B393256E66260973DD1094B5EAC9B731CCB998C3675EE54ED31F9830E0F1E；test-first源码before-02/test-first.cs保留。
- GREEN job95abe2f98fcd431eb610d718fb1be407：33/33/0skip、16.038938s，UTC00:19:38—00:19:54；原件[green-01/tests.xml](green-01/tests.xml) SHA F5E865E35693B028D2FFEBE83ED8F283A4EDC59A5957C17D4E039D9A3DCED16B。
- 20新增覆盖默认OFF、六kind/nearest、四producer、后继live team/facing/previous、无重叠与重置、Brute/input fallback、legacy控制、after-pair hook、distinct native current/tick和两局部1000布局；另13既有必要回归为pair reuse7、失效/异常回退2、payload/store carrier4。catalog10377不是执行数。
- warm factory直接delegate4096次捕获0B，只该函数局部，不是完整logic/render/worker/device 0GC；无本包Play、完整Driver、正式native trace或实际1000AI/FPS。

## 局部交错成本

每layout只有一次8warm+16sample交错OFF/ON；1000个managed实体，不运行AI或完整StepOneTick。原计时capture snapshots—collect—end，sequence/payload/count/RNG先比同。

| 布局 | OFF平均ms | ON平均ms | 变化 | ON实际复用次数 |
|---|---:|---:|---:|---:|
| 分散组 | 14.935725 | 14.51569375 | -2.8123% | 15360 |
| 集中组 | 194.4171625 | 205.60508125 | +5.7546% | 614400 |

OFF次数均0。只能判断此次局部结果混合、没有一致收益，不能把全部差异归因于候选或外推稳定收益。集中回升原因尚未知，不猜测继续堆缓存，不重复同cost找PASS；保留默认OFF、不进入Driver/真实千人窗口，不代表H07完成。

## 继续方向及合同

最近具名实际千人仍为88：ON P9547.181850/50.517480ms、正常丢弃债务257/259，未达33ms/drop0门；SetPass5/中央CPUdraw2是此前窗口，不是89新测。其它五项已过scope不重开。
下一仅检查既有SimulationWorld.ProcessCharacterInputPass中的ProcessNativeSampledState调用与88约2.25ms输入热点；有据候选再准确Task/Change。不自动切Role生产默认或新Grid/Burst，EXT1/Mono/ATLAS不解冻。正式336B44、33/3/max2、RNG/input/pass/checksum、publication只读、透明排序/segment/fail-closed、seal禁扩容和11阶段关闭保持，Q06活跃排序方法体未读。

## 安全、异常与审计

unity-cli技能只用于原共享Editor安全流程：PID74724/Unity2022.3.62f3/MCP6402，完成后Menu唯一clean8/idle、非Play/Test/Compile；没有第二Editor、Scene保存/资源/Settings/Server/Gen/Plugins改动、删除/移动/Gitdiscard/commit/push。
import后一次state拒绝连接；compile期间一次run_tests header前断连未得job，之后确认原PID新domain idle/无job、XML保持RED才提交已确认GREEN。不把未知尝试称为执行成功，不据观察异常重启。窄global log有旧其它测试CS报错，无本包新Console0声明；当前新源成功编译并执行33项为实际证据。
九个当前dirty before-01副本事前核同，公共XML覆盖前副本/新phase无Force拷贝核SHA；910保护事前0漂移。PS planned-path串接落盘前修正、rg wildcard只读报错、Record局部行anchor失败、一次治理patch重复同文件header被atomic拒绝均保留说明；拒绝的patch未修改目标，改成单文件单operation后成功。
最终保护域、before副本、validator/diff结果稍后追加，不预写验收成功。

最终审计（2026-10-09 UTC00:32）：Validate-ChangeLedger.ps1 实际由子pwsh捕获，exit0 / PASSED，Records1384、governed code diff16、4177条WARNING字符串、0条ERROR；WARNING不视为本包新增或已解决，git diff --check exit0，只有CRLF提示。首次在同进程2>&1未捕获Write-Host导致输出截断及JSON解析失败，不能使用该次“warningStrings=0”作为结果；上述为后继完整捕获的真实计数。
保护manifest修正：原before-manifest-01继承后先排除写域、再additionalGuards重新加入6个本包合法写域，事前910条0漂移，事后仅该6个合法写域SHA变化。未修改原manifest；新增before-manifest-02在最后排除准确9写域，实际904范围外保护项0漂移，9个before-01副本SHA均同；原6项变化保留在audit-final-01，不以排除新范围隐匿越界。HEAD a10c358eeb956f2153e7d5bfbb60c3c1b353fd50不变、staged空、SharedStress request不存在且88result保持原SHA。
三C# SHA：Query CD68A227AB23F9BEFE91ADCA3B7F749EE35DD790813DD9EED8625DFCFC10C216；Factory1888ADFC288F3FBDC61367CEFA095BAB9C39703BD2384595849BF70F0BAA380D；Test B769A7B3D31B84235089662CECAC3A483043050AB4B37B7728DA04EC1C8B858A。Menu/Battle Scene SHA保持before一致；原Editor最后observed_at_unix_ms=1791505662956，Menu唯一clean8、idle且非Play/Test/Compile。本包没有Driver/Play/实际1000AI、Profiler或全链0GC证书。
仅此准确批次的代码/聚焦测试/一次局部成本/留痕已收口：FOCUSED_TEST_PASS / NO_CONSISTENT_GAIN / NOT_ADMITTED / DEFAULT_OFF；不是H07优化完成。后验逐文件SHA见after-manifest-01.json，保护与工具异常见audit-final-01.json。H07仍FAIL/Goal active，下一现有NativeSampledState输入热点；无新增生产默认、专项或无证据架构授权。
