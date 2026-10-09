<!-- CHANGE-RECORD
id: NTSD-OPT-H07-PRODUCER-SNAPSHOT-REUSE-087
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs
authority: effective bounded goal sections0-8; H07 existing input producer redundant runtime-copy candidate only; no rule or default promotion
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH87-PRODUCER-SNAPSHOT-REUSE-20261009/REPORT.md
-->
# 第87批生产者快照候选

创建于脚本前；PLANNED / DEFAULT_OFF，未执行新测试/测量。[Task](../TASKS/NTSD-OPTIMIZATION-BATCH87-PRODUCER-SNAPSHOT-REUSE-20261009.md) / [Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH87-PRODUCER-SNAPSHOT-REUSE-20261009/RECORD.md)。原状为每个AI producer后完整自复制，已测边界约0.95ms；拟复用既有非别名current guard、exact class边界和Runtime别名契约，只省相同副本，保持后继AI行刷新/原生规则。准确2C#、15预期具名用例与最窄回归见Task。

副作用/风险：候选新bool默认false、两int per-pass诊断计数，未创建cache/lease/queue/worker或热容器；无需新增shutdown阶段。未知派生、五陈旧字段或producer hook必须回退原完整refresh。不改变Native权威、input/RNG/33ms/max2/排序/segment/seal/资源预算；不自动切Role默认。不把source或pure fixture证明当做千人运行时收益。

不可回退边界及恢复来源：现存dirty文件属于用户；仅追加候选hunks，准确before SHA保存在Operation，禁止整体HEAD恢复、删除失败证据或改Scene/资源/Q06/第三方。未获实际恢复授权不执行。

## 执行记录（追加）

当前仅事前声明；实际脚本、命令、test job、原XML、失败、未验证项和最终状态按证据追加。H07仍FAIL/阶段5of6/Goal active。

2026-10-09 test-first source：仅现有CharacterInputLiveSlotLoopEditorTests新增ProducerSnapshotReuse测试，生产World尚未修改。实际16个参数展开用例（此前预计15更正为16），五非别名陈旧项含Frame属性，LF2FrameInfo.N本身独立保存，因此Frame陈旧反例保留，用属性而非字段反射。新API用反射避免RED因C#编译失败；旧用例不改。已在原Editor导入测试并观察0CS/idle/Menu clean，旧86 MCP job随正常domain reload标orphan，86原RunFinished XML不变。当前尚未运行87 RED，不宣称测试通过或性能收益。一次只读NTSDEntityRuntime路径定位错误已纠正为rg --files，不产生文件操作。

RED实际执行纠正：job a2e1660e1b60474ba12c0fb42312bd15于UTC22:32:29—22:32:38完成，15/15因新候选API缺失失败；实际原XML15唯一case，SHA156148AA9077DF2320EF449DD0B82FF8DAD16923274CEB38C50E38D8EF79E736。前一人工16计数错误以此纠正，最初15预计正确。RED XML及原86公共XML已分别复制到不存在的新目标保留，未经覆盖。随后World只新增默认false开关、两计数及exact/current/hook guard，后继AI行刷新块保持原文；首次补丁的hook成员名已按实际runtimeHooks.CharacterInputProducerPassMutationOverride纠正，尚未宣称编译或GREEN通过。一次import请求误传Hashtable返回Invalid JSON，已用JSON重试成功；未改Editor场景。

原Editor导入World后自然reload，UTC22:34:05/07 runtime+Editor新DLL已生成；reload期间6402两次连接被拒，未重启/开启第二实例，之后同PID74724/6402恢复。UTC22:36:07确认idle/非Play/编译结束/0CS、Menu唯一clean/root8。当前dirty before对比仅World23增1删、test252增0删；移除本包新增测试块后旧测试原文全部相同，AI后继刷新852字符块内容相同。RED已复制保护，再提交整fixture GREEN；结果尚待，不把编译或source证明升格为运行时收益。

GREEN01 job676dd50d14e44b89b9ff67e5d8de0278：64case实际63PASS/1FAIL。唯一失败是本包alias fixture用默认世界却调用ExtendedChecksum，需DesktopExtended；不是候选逐位差异。其他14个新case及49旧case通过。原XML SHA1312A6484638F393B1106518B6B29C71E9F22E9232AA88B78C6FCA06828DD989已保存；未删除失败。只把本包该fixture的两个world设为DesktopExtended512，完整checksum及其它断言全部保留。run期间get_test_job一次25s传输Read超时，原job继续自然结束，同job随后取到终态。尚无完整GREEN，不宣称收益。

GREEN02真实终态：job29b9be582dac4550be1f22b6ae48b3e2，UTC22:40:39—22:42:15，64/64 PASS、0skip（新15/旧49），88.7520286s；公共RunFinished XML及green-02副本SHA D4694E6304E8D61ED4716424EA7E6D60BCF8D6F09376EC440B158883903320D8一致。DEFAULT_OFF/五非别名回退/未知派生虚拟刷新/producer hook/human回退、Runtime alias及native/legacy两profile16tick完整Extended/Lockstep/native scalar/legacy RNG/proxy均通过，开启候选确有skip；局部warm64+256委托0B通过，不是千人full Driver证书。实际源码2C# SHA见after；未扩大Suite范围，无新测量或收益宣称。状态仅FOCUSED_TEST_PASS / DEFAULT_OFF / PERFORMANCE_PENDING；H07/Goal继续，66执行/阶段5of6/34父0。

范围复核：800保护0漂移、8当前before副本0漂移、10预期不存在仍保持；HEAD未变/staged空/Menu和Battle SHA保持。Tools/Validate-ChangeLedger.ps1于本批已执行，1382Records/15governed diff、4180既有warning/0error、exit0。最终治理与diff check追加到报告/after，不把治理PASS当runtime验收。回滚仍须独立批准，仅本包hunks，不恢复HEAD或丢弃失败。

最终UTC22:46:34核同800保护/10absent/8backup、HEAD/staged；原Editor idle/Menu clean8/0CS。最终Ledger1382/15/4180warning/0error/exit0，git diff --check exit0（CRLF提示保留）。准确输出与源SHA见Operation after-01.json，真实validation.json/报告已留。一次治理patch因真实STATE link不同校验失败未应用，重新读取五文件准确首行后仅追加状态及更新原tracker三H07行；历史保持。没有新增Suite/测量或默认推广，FOCUSED_TEST_PASS并不完成H07，Goal active。
