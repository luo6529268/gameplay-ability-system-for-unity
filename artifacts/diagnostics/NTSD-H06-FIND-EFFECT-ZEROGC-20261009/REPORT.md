# FindEffect 单点分配修复结果

状态：VERIFIED，仅查找分配修复与所列聚焦门；不是 Role 限定准入完成，不是 Windows H07 阶段通过或 Android 认证。
Task/Operation：NTSD-H06-FIND-EFFECT-ZEROGC-20261009；Change：NTSD-H06-FIND-EFFECT-ZEROGC-001。

## 实际变更与语义

唯一生产变更是 Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs::FindEffect(int)：将 ReadOnlyCollection 的 foreach 改为 Count/index 索引遍历。返回首个匹配记录的同一对象；重复 effect、未命中 null、空集合、负数及 int 边界、SourceAvailable=false、null 元素的原异常/提前返回行为不变。48组查找组合在改前与改后各通过一次。没有缓存、新增 runtime 状态、owner、容量或失效变化，也没有新增十一阶段关闭责任。

测试新增 H06FindEffectAllocationEditorTests.cs；复用准备脚本 CreateFixture/Collect，未修改准备脚本、原千人测试、GC recorder、Profile、Suite 或默认选择入口。测试资源仅测试owner持有，finally Dispose recorder、解除seal、注销实体，检查objects/slots为0。未读取活跃Q06方法体。

## 观测与结果

| 验证 | 修复前 | 修复后 | 结论 |
|---|---|---|---|
| 100对象 collector，4warm+3sample | 22/22/22 GC.Alloc events | 0/0/0 | 本次同fixture基线与修复后均实测，前后正分配/空scope校准有效 |
| 100结果 | 三份ordered候选完整载荷、handle、固定空输入、extended checksum、双RNG完整标量/计数 | 三份逐项完全相等 | 非Driver tick trace；每次97 pair、1个最终candidate，实际Role=3、fallback0、无abort |
| 原千人混合 collector，4warm+8sample | 历史同fixture各234事件 | 新鲜八次均0 | 本次用例未观察到剩余分配；没有重跑前版或刷长窗 |
| 原千人容量 | 原门不变 | capacities [1000,2000,3000]、数组身份与容量不变；reject0→0 | 每次975 pair、fallback0、实际Role且无abort；finally objects/slots0 |
| 100清理/Profiler | 原设置 | 两轮settingsEqual=true、objects0、slots0 | 不全局录制、不清历史、不覆盖旧raw |

100 fixture保持76 ordinary/24 special、team50/50、40列16X20Z。1000保持760/240、team500/500及原布局；千人scope含snapshot/collect/end，100分配scope只含collect。所有断言、反射观察、序列化/checksum采集都在100测量scope外；原千人测试无hook。以上不是整tick、自然OPoint或实景Play的全域证明。

准备报告66个40B事件的首managed frame为FindEffect；本次最小循环替换后相应100及1000事件消失，支持所定位分配被消除。GC.Alloc事件不是回收停顿；recorder Unit=TimeNanoseconds，不把rawValueTotal当bytes，不声称CPU/P95收益。未据此解释95 Manual/headless失败。

## 原项目命令与证据

使用现有原Editor PID74724、Unity2022.3.62f3、MCP TCP127.0.0.1:6402；没有第二个同Library实例。

1. manage_asset {action: import, path: Assets/NTSD/Scripts/Test/Editor/H06FindEffectAllocationEditorTests.cs}。首轮CS0246（新增测试缺NTSD.Animation using）已修复并保留修正前副本；最终Editor DLL 13:25:12Z晚于源码，原Editor完成reload。
2. run_tests {mode: EditMode, testNames: [NTSD.Test.Editor.H06FindEffectAllocationEditorTests.FindEffectPreservesLookupSemantics, NTSD.Test.Editor.H06FindEffectAllocationEditorTests.Capture100BeforeRepair]}。job2813c3da7a3c471eb540ba0d4b99182b，2/2 PASS，3.3364119s，before-tests.xml。
3. manage_asset {action: import, path: Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs}。Assembly-CSharp.dll 13:26:06Z晚于源码13:25:59Z，reload后idle且后续测试执行成功。
4. run_tests {mode: EditMode, testNames: [NTSD.Test.Editor.H06FindEffectAllocationEditorTests.FindEffectPreservesLookupSemantics, NTSD.Test.Editor.H06FindEffectAllocationEditorTests.Verify100AfterRepair]}。job0f3b699ebee744e2bb660f6d796eae49，2/2 PASS，3.5982006s，after-tests.xml。
5. run_tests {mode: EditMode, testNames: ['NTSD.Test.Editor.BattleBruteProductionAdmissionEditorTests.H06RoleAdmission_FourShapeCollectorZeroGc("OPointBurst-shaped")']}。job4fbfa314bf0a4d588b28e9476f032972，1/1 PASS，1.0421012s，thousand-tests.xml。
6. get_test_job逐job读取终态；get_editor_state、manage_scene get_loaded_scenes、read_console get errors；git diff --check -- Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs 退出0。
7. ChangeLedger validator结果见本目录validator.log与下方追加。

原千人新鲜数据：[OPointBurst-shaped-collector-gc-7f5a826ab9dc4874822a67910dbc7bf6.json](../NTSD-H06-ROLE-SCOPED-ADMISSION-20261009/OPointBurst-shaped-collector-gc-7f5a826ab9dc4874822a67910dbc7bf6.json)。改前对应文件9e8228bbc42e47b796c0f80825d4ea06保留。
100证据before-100.json / after-100.json包含完整结果；cleanup单列。fingerprints.json绑定生产/测试/夹具/recorder/千人结果SHA。

并发读取桥接状态时两次ConnectionReset以及MCP disposed-object报错：已改串行查询并成功。未重复测试、未启动Editor、未修插件。最终Console返回只有MCP连接日志，未见本次C#编译/战斗异常；原Menu仍唯一8roots、clean、idle、非Play/非编译/非测试（editor-final-0/1/2.json）。不把桥接告警隐去或当成战斗失败。

## 保护、复用与边界

2439个事前保护文件对比仅登记的LoganKindCatalog.cs发生变化，其余2438字节一致；11份事前真实dirty备份SHA核验通过；HEAD未变。没有清旧数据、改Scene/资源/设置/Server/Gen/Plugins；没有push或Git破坏性操作。新增测试/meta为本Task输出；治理文件是已登记增量，详Operation。

复用此前四场景32tick强一致以及其它三个collector形状0GC的已测结果，保留其历史指纹，不称本次复测。此次仅改变无副作用查找遍历方式，由改前后48组独立查找oracle和相同100结果覆盖改动域，不重跑未影响的pass/输入/AI/生成/关闭全域。25 collector与82有限Driver仍各守原scope，不互换为全域证据。91/93实际scope0GC及95 Manual/headless GC FAIL分别保留。

尚未执行：真实Battle Play、正式EXE新对照、同配置Brute/Role收益窗、正常默认接线、120warm+180冒烟/120warm+1800正式门、Windows H07和Android设备认证。本Task不扩大到这些后续阶段。

当前H06已移除这一个已知collector GC阻塞，但限定准入仍未完成。按D1=A，继续核必要门及同配置收益，满足后才默认推广；整tick33ms未达不是该子阶段阻塞，父项/H07最终33ms、无持续backlog、正常drop0与Android条件不降低。
下一项H07-INPUT建议不变：复用91输入归因，先准确冻结热点、字段读取、失效及逐tick输入/双RNG/checksum合同，再由用户决定具名实施；Role默认不是硬前置，无固定收益承诺，旧自动Goal不恢复。

最终Ledger：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity，exit0、1393 records、12 dirty governed code paths、0 errors、4264 warnings（历史Record覆盖/路径漂移等，完整保留validator.log，不为此清理旧记录）。staged diff与事前空字节完全一致。
