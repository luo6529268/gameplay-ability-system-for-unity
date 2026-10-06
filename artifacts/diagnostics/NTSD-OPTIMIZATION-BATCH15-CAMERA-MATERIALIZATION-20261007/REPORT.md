# 第十五批相机物化消重报告

事前快照：IN_PROGRESS / 尚未修改生产脚本、编译或测试。以下保留初始事实，最终结论在后。
[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007.md)；[Record](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-CAMERA-MATERIALIZATION-015.md)。
范围和验收见Task；正式336B44/33ms/checksum/只读publication/专项门保持。

## 2026-10-07最终限定结论

SCOPED_CAMERA_MATERIALIZATION_PASS；生产Change NTSD-OPT-M03-CAMERA-MATERIALIZATION-015 保持RUNTIME_PENDING，父M-03/H-11 OPEN。
warm CentralOnly的host不再重复生成几何，而是Queue并保留原ResolveDisplayAlpha取样；
既有真实camera入口生成当前显示几何。冷启动、stale/world变化、<=30FPS、Edit/batch、
legacy/shadowbuild及相机/feature/material路由失效仍原Flush；显式Flush API不变。

### 同观察逻辑、原saved Battle前后窗口

三窗各tick8→104（96正常逻辑tick），自然默认小roster；配置renderFps120不是实测120FPS。
同一probe源SHA E8D8CA3BEC37B976EC3767E70ADB6B746AE5430EFC21D5B8CA7548A234C6A3FC。
[完整对比](comparison-final.json)，[改前原件](before/production-window-01.json)，
[改后1](after/production-window-01.json)，[改后2](after/production-window-02.json)；
对应各目录materialization-window原件保留。

| 口径 | 改前 | 改后1 | 改后2 |
|---|---:|---:|---:|
| 真实world-camera样本 | 132 | 143 | 118 |
| Build count | 264 | 143 | 118 |
| Build / camera | 2 | 1 | 1 |
| 实体vertex stream上传字节 | 185856 | 100672 | 83072 |
| 实体vertex bytes / camera | 1408 | 704 | 704 |
| publication-change Build | 96 | 96 | 95 |
| alpha-change Build | 168 | 47 | 23 |
| physical segment / recorded CPU draw | 4 / 5 | 4 / 5 | 4 / 5 |

本窗口按camera归一，实体vertex payload/Build工作次数各少50%；不是CPU总耗时/驻留内存/全GPU流量/
GPU batch/实际FPS/整场性能50%。上传计数不含Foot/Health/index/submesh；5是RenderPass recorded CPU draw，不是真实GPU batch。
after2一次相机间隔logicTick95→97（delta2）、94次delta1、23次delta0，
因此publication类别95不是“96tick都必须逐个显示”；真实采样displayTick均等于当前publication，现有latest-publication/max2语义未改。
不能从该一次delta2推断精确wall-clock/native Host或全World一致性。

三窗全部2实体/4command/4segment/1chunk，零growth/failed/rejected，publication与captured隔离，
合法segment区间与finite bounds、stride44保持；两slot、每窗native Mesh identity稳定，
after两窗end-camera实际各观察两个Mesh；begin/end generation每camera+1。
每相机后CPU read lease0不是GPU完成证明，本批不新增buffer/fence，不更改existing生命周期。
first/last记录第一实体position同值(2.1300344,-9.1900005,0)，不是动态位移/技能/透明重叠/GPU像素验收。

每窗现有tick/DriverUpdate/LatePresentation/PlayerLoop分配计数及独立camera envelope/observer分别0B，
新warm host预热16后64次0B；Editor collection control/PlayerLoop hardgate支持false。
诊断报告/JSON窗外分配，不叠加多层计数，不声称完整链/其它线程/Player/Android 0GC。
三窗均现有11阶段关闭objects/slots/borrowers0，Scene clean/SHA同：
253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010。
结束恢复单一原Menu clean/8roots/idle/nonPlay，[恢复记录](menu-restoration.json)。

### Test-first、编译与实际命令

[RED](red-result.json)：job543f78077ae348c4bea53f2f6bd61d26，14completed，三个预期首差；
失败job result空，仅实际failures列表作RED证据，不猜补成功数。
初fixture四编译诊断（CameraType必填/lease为struct）已修，不称性能失败；RED旧取到lease的断言先失败，
最终fixture先Dispose再assert，防止失败漏lease；before Play有Domain Reload，原件保留。
[GREEN](green-result.json)：257cf5f7012a49a5990ee691d1476d2d，14/14 Passed；
[回归](regression-result.json)：b17ba0b5f4694ebc860d10cdbe1c396b，135/135 Passed；
[去重分组](confirmed-cases-summary.json)共149真实具名Passed，含旧latest13/submesh6/report58/capacity16/
memory5/pure motion3/aux15/resolver15/shutdown4。
warm fixture同tick空World checksum64保持，仅局部不反写真值证据，不是正式全场回放。
不重跑两个子批14缺最终结果的旧UnityTest，不覆盖其trace，不把它们算PASS。

使用原Editor MCP6401：
refresh_unity(force/all/compile request)、get_editor_state/read_console(error CS)、run_tests(EditMode,testNames)/get_test_job；
manage_scene仅load原saved Battle/Menu，不save；execute_menu_item三个Batch15 Production Before/After First/After Second。
原真实TestRunnerApi.IsRunActive guard每次菜单内确认；无第二Editor。
[编译指纹](compiled-manifest.json)：生产DLL A49C69CE527D7E0268661F96F6A5A28B0C79E6204872BDA6EAF1A1930270AE8D，
Editor D27CD497D89A27E4ADD81440368942CABCF4CF13E38CAC834CE2402F01BE505D；
CS0、两轮after没有后续脚本修改。
PowerShell：精确Get-FileHash、Git只读status/diff --check、Tools/Validate-ChangeLedger.ps1（最终原件另附）。
并未运行Full BattleRuntimeSelfCheck、Profiler/FrameDebugger/GPU capture、1000AI/Player/device或新的native replay。

### 最小代码与合同/技能影响

本次重新扫描：BattleCentralRenderSystem.cs:486新增host helper；:499保留原clock调用；
:572相机入口、:1356 clock公式、:2076原renderer proof均未改。
SimulationStageRenderModule.cs:442仅一处调用替换；旧mode suppression在LF2ObjectRenderer、stage late bypass、
overlay与spark继续mode消费，不依赖早期geometry。[精确对照](production-scope-check.json)
证明Central除了20行helper外（规范化换行）全字相同，Stage只调用替换，source/runtime排序Q06方法体未读/改。
只扩既有probe的唯一输出/显式周期菜单，无新Runtime owner/cache/worker/slot，11阶段合同保持。
unity-cli指导复用现有Editor桥与版本，不安装/升级Unity6CLI；2d-pixel-perfect指导确认既有URP并保持UV/PPU/相机/采样/stride，
不套用技能示例60Hz固定步长，也不改Scene/importer/AA/Settings。
精确33ms/3ms/max2、RNG/input/checksum/pass/presentation只读保持，未宣称重新对齐正式EXE。
EXT1 PROPOSED/MODIFY_REQUIRED无专项M0/instancing；MONO USER_HOLD；PERF/ATLAS正文、bank/预算/格式/segment/failclosed不变。

### 最终审计、风险和下一门

10个dirty/现存准确before备份、514事前保护SHA，三个旧after manifests另补末期保护，不将末期digest伪称事前；
其last-write均早于Task，子批14after SHA另与上轮冻值一致。
所有旧failed/中间/本批新证据保留，无文件删除/移动/覆盖旧结果，Git HEAD/staged保持，没有add/commit/push或丢弃工作树。
34项优先级高12/中14/低8，父关闭0，最新结果回写独立进度总表/风险/M03/STATE/handoff/Ledger/Operation。
后续先用动态实体/运动/生成退休场景闭合显示像素、首可见与延迟/透明重叠，
再扩同布局高负载/完整0GC/设备收益；A1 dirty-chunk仍未来设计，不能以同publication跳过插值位置。
本批受控窗口成果可交付，但production RUNTIME_PENDING，不制造GPU/Android证书。

### 最终治理结果

Tools/Validate-ChangeLedger.ps1 exit0、1311 Record、当前9脚本COVERED、0error，
全库4260历史warning，本Change/新fixture0；宽过滤的4条为旧Record声明路径，不顺手修旧记录。
[Validator原件](change-ledger-validation.json)；[静态原件](static-validation.json)：514事前保护/10准确备份0drift，
175本地链接0missing、git diff --check0、34=12/14/8、编译identity0drift；三末期補保护另读无漂移，HEAD/staged不变。
最后after.json冻结当前文件身份（不包含自身），冻结后只读核验，不再改脚本/文档/证据。
