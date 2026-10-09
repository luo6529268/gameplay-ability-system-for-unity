# 第82批：现有Role候选逐tick完整Driver准入
状态 DRIVER_QUALIFICATION_PASS / FOCUSED_TEST_PASS / NO_DEFAULT_PROMOTION；日期2026-10-09，UTC开始2026-10-08T17:23:03.2334936Z；以下先行矩阵与执行前状态保留历史，最终证据见REPORT。
上一轮PROGRESS：81四同版千人短窗有约73%收集收益但P95>33/drop非0；本包补缺少的逐tick证据，不重复81测量/H06评估/原30长窗。

## 需求、所有权和准确写域
已批准Goal现行第0—8节允许必要资格/诊断；Role生产默认推广仍未批准，已向人类提出独立冷选择范围请求，未回复不实施。
唯一C# Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs，before SHA D33345A3DBA761C41E27B0CFF2A48C451E56C514973D93C50BFAA456B5B09242。Run目前仅ForceBrute；新测试明确选择既有ForceBrute/ForceRole，不写runtime/算法/default、Suite、Scene/Prefab/资源/settings/Input/Gen/Plugins/Server，Q06排序方法体不读不写。
其它写域仅本Task/Change/Operation/REPORT、原五治理入口、公共Temp/Goal18_LastTestResults.xml；新ABSENT artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH82-ROLE-DRIVER-ADMISSION-20261009。旧1—81raw/测试/失败保持。

## 冻结矩阵与test-first
新5case：RoleDriver_DefaultBruteAndExplicitQualificationOnly；RoleDriver_PreservesApplicableFormalRootAndEveryTick（right-x550/left-x350各12tick）；RoleDriver_ThousandSakuraCanonicalAiPreservesEveryTick（Dispersed1000/Combat1000各32tick）。
先新增case反射要求缺少RunRoleDriver，有效5RED，后实现helper再GREEN；旧OrdinaryBruteDefaults1、ProductionRequestDoesNotChangeFrozenWorkloadOrEnableCandidate(index0/1)2、EnvelopeBindingEligibilityQualificationDoesNotPromoteDefaults1必要回归，预计9以实际XML为准，不改旧期望。81已核同/通过的Role formal Direct/tree/RNG8复用，不重跑整类。
复用Run/CompareEveryTick/CompareRoot/WithLogan.../SaveNew。Run只增加nullable formalCollector（原last composeEnvelopeBindingEligibility之前），null的旧配置/断言不变；新helper仅ForceBruteForce/ForceRoleAware，4admitted Brute flags true、其它候选false，不强制Direct/Sweep/tree。
每tick比较完整声明extended/parity及lockstep hash、NativeRandom完整scalar/legacy State与calls、entity/actualAI数；记录requested/actual collector、whole-collection aborted（必须false）、特殊fallback participant数（不错误要求0）。冷prepared participant/exact body/itr数组身份与容量保持，不隐式扩容；不是仅末tick相同，也不把冷测试断言的分配称热路径0GC。

1000测试使用OID1 Sakura：新诊断scenario seed1314149188/32tick/初始两OID1/action0/HP500/team1,2，既有BuildSpawnPosition布局、LogicEntityFactory补至真实1000AI。PrepareThousandAi仅最后增加objectId=2可选参数，旧call不变，新Role明确1且每实体OID1。新scenario是诊断夹具，不是正式EXE trace、不继承旧OID2 dataSha；真实DAT由正式runtime catalog/AssertInputsCurrent核验。Manual完整Driver资格不代替81真实Battle Scene性能或全部request相同证明。
正式root EXE336B44…7BD3；right trace AA327E22…15FBE/left CF270170…BB9B2核12tick原CompareRoot oid/action/hp/vx（原打印容差）。此有限native字段不是native全域bit一致，不重建authority campaign。

## 本轮新鲜热点证据和非目标
81原件Combat EntityInputPass10.51355ms、RemainingAiDecision4.86694、IndexedKernel3.07091、NearestSearch1.51621、Combo0.47186、rowRefresh0.22292；canonical300000commits/0fallback、unified legacy builds0。不能把整个CharacterInput10.91860都称暴力寻敌。Freeze/producer full refresh/native route未单独计时，差值包含仪器/循环且未归因，不凭此写某方法热点。本包提取既有detail而不新增计时器、AI算法、cache或Burst。

## 生命周期、验证和回滚
复用临时ownedDriver/World/logic pool十一阶段关闭，objects/slot/borrower零才返回；无新增runtime服务、容量或关闭模块，不解除seal、不改顺序。33ms/3ms/max2、input/RNG/checksum/pass、publication只读/透排/segment/failclosed/预算不变。
原Menu sole clean8/6402 idle非Play/无测试已核；Unity2022.3用现有MCP，不装Unity6 Pipeline/启第二Editor。7准确现有path事前备份；父81递归guard扣本写域＋81新原件60项保护，HEAD/staged保持。先Task/Record/Operation/索引及Copy-Item无Force核同，再源5case/import/RED→公共XML副本→helper/import/9GREEN→新UUID原件→validator/diff/Scene保护与Menu恢复，失败/0用例不抹。
回滚另请求精确批准，仅before01对应hunk，不reset/restore/clean/stash/delete/commit/push。阶段4/6、59执行＋82准入、H07H11 OPEN、Goal active；本包结束不停止Goal，缺生产切换授权仅拦推广。

## 夹具入口纠正（首次GREEN原件后，先于修复）

首次9case实际7PASS/2FAIL：两个Sakura case在Raw.ValidateScenario的schema whitelist失败，尚未tick。原入口只接受固定native案例，通用legacy schema也绑定旧EXE/3tick/dataSha，不能伪用旧Q07 schema/dataSha或放宽正式校验来接新诊断夹具。
唯一C#写域不扩张。仅在Admission添加严格本路径/原固定SHA3ACBF13FB9442F47656BB6D952A0F063337F444F144C210BE74A630FB7CF79DF、schema、EXE336、32tick、MobileExtended/useProjectMode的诊断接线；复用Raw已有私有UnityCurrentDatScope、TemporaryObjectPoolScope、TemporarySimulationDriverScope和ConfigureWorldAndRoster，通过冷reflection读取现有属性/调用，保持前后AssertInputsCurrent、Manual/数据目录预热/seal、logic-only与十一阶段关闭/三零后置。未更改Raw、其正式ValidateScenario或生产入口。其余非本fixture调用完整原WithLogan入口，正式root两case不变。
这是本owned诊断fixture的独立验收入口，不把其当native trace，不删除原失败文件。首次green-01-fixture-fail/tests.xml与before-03-fixture-fail.xml均SHA BAC374212C2D200763B853387DD8CFC9BF5E1D98BDF345A38AC613D77CD47552；fixture副本SHA核同且正文不改。先保存当前C#再实现。只复验受影响两Sakura case；原7不变门复用首轮结果，去重总9，不重跑native12tick和旧4无关门。

## 最终阶段边界

有效5RED→首轮9/7/2→受影响2/2，去重9具名PASS；88paired/176完整step的声明hash、完整双RNG/实体数同、实际collector及受检3数组稳定、关闭三0，原Menu clean8/Scene保持。首次夹具拒绝、TCP busy/管理器超时原事实保留；这不是新性能收益或全域权威认证。60执行/阶段4/6/H07H11 OPEN/Goal active，Role生产默认未变/未获新增批准，旧81性能门仍FAIL。准确生产冷选择授权已发人类，不因缺此授权重开H06或停其它范围内优化。
