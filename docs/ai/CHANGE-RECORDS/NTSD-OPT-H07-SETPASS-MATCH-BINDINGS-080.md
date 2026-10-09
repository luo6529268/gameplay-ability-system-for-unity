<!-- CHANGE-RECORD
id: NTSD-OPT-H07-SETPASS-MATCH-BINDINGS-080
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCommonAtlasBindingEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
authority: human explicit approval prioritizing match visual dependencies and existing-budget shared bindings; presentation cold path only
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH80-SETPASS-MATCH-BINDINGS-20261008/REPORT.md
-->
# SetPass 本局共享绑定修复
[Task](../TASKS/NTSD-OPTIMIZATION-BATCH80-SETPASS-MATCH-BINDINGS-20261008.md)；[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH80-SETPASS-MATCH-BINDINGS-20261008/RECORD.md)。
原状：75真实OID1/1000AI，1chunk/2024segments/2023纹理身份断点；全catalog145page/2320MiB超既有512MiB，合法Source2D回退；77全帧SetPass1995未降。
预期：五准确路径cold API/App/实际stress冷接线与聚焦tests/两窗入口；全logic/UI/catalog/source保留，仅选中递归全帧候选＋common共享。副作用仅冷CPU像素、新atlas资源/catalog；透明顺序/segment规则/预算/格式不改。
生命周期：Preparing/空World/未seal/无读lease门，新资源仍已存在manager所有；冷原子替换、旧atlas按原catalog retirement，无新增模块/关闭顺序变化；异常不部分提交，source/Sprite保留。
验收/不可回退边界与回滚见Task。状态未验证；任何代码后立即登记实际符号/命令/结果；旧失败/其它源与资产保护，H07/H11 OPEN。

首次脚本写入：仅BattleCommonAtlasBindingEditorTests同文件新增12个case及MatchMethod反射助手；其余四源仍原SHA。先要求缺少的cold API存在，旧实现应断言FAIL而不是编译失败；RED尚待执行。既有测试原文保持，manage_asset import该唯一源已返回成功，非编译或测试PASS。

RED实际：job550687af8b5b44698414c1626db1bd2a，12/12有效断言FAIL，缺少三个cold API；Editor非编译态/CS0。原正式body/SPARK/WORDS已Apply(false,true)，不可直接GetPixels；Task已补准确冷像素provider：验证原SHA重解码并复用原像素处理，不新建Sprite/源Texture或改importer，unknown仍Source2D。

CODE_WRITTEN（未编译）：Manager新增CollectBattleVisualDependencyIds/BattleVisualBindingBoundaryAllows/PrepareBattleCentralBindings/EnumerateMatchCommonBindings/TryBuildMatchCentralAtlasPublication；App在服务出生前use玩家冷调用；Stress仅selectedCharacterOid冷调用。测试builder参数补可选像素provider，无热分配/排序/segment/逻辑配置变更。新资源同manager owning，旧atlas预算扣余量；原Source/legacySprite不删除。Suite两窗入口仍待写。

Editor导入三runtime源完成、当前idle/CS0；追加Suite固定两窗请求3聚焦case，尚未写Suite入口，准备有效RED；原tests与公共XML已备份。暂不声称新API/runtime收益已通过。

Suite RED job31470c3e8f9b49fbb33a453be52440d4：3/3断言缺请求helper，red-02/tests.xml保存。其后Suite新增BeginMatchCentralBindings/BuildMatchCentralBindingsRequest、state专用bool/新固定root、两run、StartRun反射开关try/finally；OFF显式恢复全部Source2D防止预先App共享污染，ON真实selected OID递归common共享。原readback/统计/17参数/默认/cleanup不变。五C#均已写，GREEN待跑。

首GREEN job231f3f8e266b4d638f72df5e20548328：41执行/40PASS/1FAIL，仅新请求测试错误预期combat，旧冻结请求为combat1000；已更正测试而不改既有请求，失败原件归档green-01。追加不可读源冷provider直接case，证明不改原Texture可读性/共享身份；该case不单独算先RED。

GREEN实际 job8e67792ee2f7498eb0b90fe1fbc414ed：42/42 PASS（新16＋原common及窄readback），0skip，Editor0CS。公共XML归档green-02/tests.xml。之后Suite追加每次StartRun覆盖SharedResult前的精确File.Copy（新outputPath.shared-terminal-before.txt/拒绝存在），确保两实际窗口分别保存旧终态字节；API/结果与原Scene门后核，未运行actual收益。

windows-01实际PARTIAL（原件保留）：OFF完整120+180/SetPass1995/正常清理；ON冷Prepare遇多个配置独立Texture复用同源pixel.png，新AddSource错误要求同一源Texture对象，尚0tick拒绝，非battle规则/排序错误。Suite原owner关闭三残留0/Menu sole clean8，原场景恢复；新增同路径distinctTexture两个case，等像素必须共享/不同像素拒绝，先窄RED。修复仅冷source去重：验证dim/处理后pixels，不改变GPU实际绑定key/排序/segment。修后新windows-02两窗，01不覆盖。

去重RED job1568e8169573440584fa3c719f543e5e：2case/1PASS（不同pixels正确拒绝）/1FAIL（相同pixels误拒绝）；非编译FAIL。其后冷AddSource验证same path distinct texture dimensions/pixels完全一致才共享，无法取得duplicate pixels则整条source path明确Source2D fallback；metadata/Source/Sprite identity保持。Suite本包root改新windows-02，terminal准入仅本包evidenceRoot接受01/02，无旧请求或结果覆盖。下一44case GREEN与两窗待执行。

最终聚焦GREEN joba39a1c0032c847a886feec3f7cdf706c：44/44PASS、0skip，green-03/tests.xml。18新case＋旧common/窄readback共26回归；原失败与原02GREEN保存。611保护0变化（正确排除实际shared terminal）；五准确源都由本Record覆盖。下一02真实窗口，当前仅FOCUSED_TEST_PASS，无修复收益或H07/H11完成结论。

windows-02实测MEASUREMENTS_COMPLETED/2runs：两actual1000AI/120+180/同请求除output，tick300完整checksum文件字节同。全帧Profiler SetPass1995→5（max2038→5）；中央segment avg1990.494382→1，tick128 physical2024→1/仍1chunk、源/解析2026、unresolved0；中央CPU submission Draw均值1991.494382→2不等GPUbatch。ON4pages64MiB/budget512MiB/Texture2DArray，fla超尺寸仍Source2D。logic avg90.416156→87.920167ms/P95113.763385→112.167190，drop923→725，H07未达33ms/0drop，不闭总Goal。兩正常teardown全0/orderedShutdown/原Menu clean8/scene byte不改，CS0。正常role-select/App/mixed/device/视觉像素A/B尚未做，Record保持RUNTIME_PENDING而非整体VERIFIED；本实际同角色SetPass子证据PASS/默认冷接线已启用。H11完整0GC/旧43失败不改变。原CPU prepared两个容量与lease/stale总数此schema无新计数，不编造0。

最终留痕审计：[audit-final-01](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH80-SETPASS-MATCH-BINDINGS-20261008/audit-final-01.json)，611保护路径0变化/原备份核同/HEAD保持/staged空，两Scene原SHA核同；Tools/Validate-ChangeLedger.ps1 exit0（1375Records/5实际代码diff covered/0errors/4350历史warnings），git diff --check exit0。文件操作单独VERIFIED，不替代本Record的RUNTIME_PENDING；下一真实瓶颈据本窗CandidateCollect57.166ms、CharacterInput11.383ms另立精确任务，不制造新架构或重开已过4项。

