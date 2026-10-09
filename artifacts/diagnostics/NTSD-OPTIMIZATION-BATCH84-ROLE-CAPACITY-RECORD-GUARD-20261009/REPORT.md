# 第84批 Role 满候选记录守卫：真实工作负载不适用

结论：NOT_APPLICABLE_FOR_FROZEN_WORKLOAD / NOT_ADMITTED / DEFAULT_OFF。局部规则夹具中有收益信号，但两种冻结32tick真实1000 Sakura AI工作负载中，候选的满容量记录skip均为0。停止的是这个候选的推广，不是优化Goal；不追加本候选Play/性能窗口，不把局部collector收益外推为实际千人收益。H07/H11 OPEN，六项阶段4/6、63已执行子批、34父项关闭0，Goal保持active。

## 精确实施与权威

Task / Change：NTSD-OPTIMIZATION-BATCH84-ROLE-CAPACITY-RECORD-GUARD-20261009 / NTSD-OPT-H07-ROLE-CAPACITY-RECORD-GUARD-084。仅三个自编C#：BruteForceSceneQuery、RoleAwareCollisionShadowSelfCheckTests、BattleBruteProductionAdmissionEditorTests。生产collector/profile默认、Suite、Diagnostics、RawReplay validator及其它脚本没有本批改动。

Query新开关默认false，Role cached方向显式传入roleDirection；几何/原rest与ITR前置、CandidateAccepts和旧test-only legacy capture control之后，仅kind0且vrest非0且实际producer计数>=20时跳过纯记录。真实producer来自既有store/legacy模式，不把mesh chunk或其他容量当候选20。kind4源计数、nearest替换/tie RNG、其它kind、Brute/legacy/fallback原路保持，每collection重置skip，无新cache、native buffer、worker或lifecycle owner，无热路径扩容。代码存在不等于获准推广。

正式EXE本轮SHA仍336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3；对应playable closure B97中的battle_world.cpp容量、multiple/nearest语义用于限定排除条件。native capacity_exhaustions既有诊断映射不在本包，不声称native全域已对齐。33ms/3ms/max2、checksum/RNG/pass、publication只读、排序/segment/fail-closed及十一阶段关闭保持；不读Q06活跃排序方法体，不解冻EXT1/ATLAS/Mono/asmdef/Burst。

## 真实测试结果，不合并成全通过

| 作业 | 实际total / pass / fail | 结论 |
|---|---|---|
| f09246670d354da8b0c7cd9f1b622685 | 20 / 0 / 20 | 新API不存在的有效RED，不是编译错误或0case |
| 81058cf2e79241a4a13151aa4e7f2f0f | 29 / 28 / 1 | 新mixed fixture未触发非零RNG，原件保留 |
| 49d436ffdfcf485ca38c632b763b028c | 5 / 4 / 1 | 旧MultiBody四case PASS；新增baseline本身RNG0 |
| b973b136c1a6459faca30530a2e017ef | 1 / 1 / 0 | 仅修新增fixture：FrameCache建立前构造双nearest，原实际ITR与两边非零RNG断言保持 |
| 06ac528a0744491e99f719b61818a36b | 5 / 0 / 5 | 新冷Driver owner缺入口的有效RED |
| 6b46c55684bf4cada26b6ca10d04479b | 6 / 3 / 3 | 新default及两root PASS；千人两case skip0，旧冷接口last参数回归1 |
| 35c5f28eeaec44c68754d38d18efd09e | 2 / 2 / 0 | 移动本批新增可选参数，保留原last composition/defaultfalse/旧断言，受影响Role-default与composition通过 |

影响域复用后40去重具名结果为38 PASS、2未通过的千人准入假设；不是40/40、39/39或单job38全重跑。局部33项（20新/13旧）已通过，FormalCollector新增块移除后的旧全文normalized与当前dirty before相同。same-team fullqueue case仅证明被前置过滤后结果保持，不能独自闭合group resolver跳过覆盖。所有RED/失败/修后XML见同目录red-01、green-01/02/03、driver-red-01、driver-green-01及driver-default-regression-01，最终XML SHA177B2108E03804F33D931DFE5C0A4CBBB7F6134F125B0D7EEBDA43AFD511CD9B。

完整Driver作业UTC2026-10-08 19:55:20—20:02:32（本地次日），原6/3/3 XML SHA BB53C1327307E9E298A88EEF0F589E47A52957C2370B8DAC6F5190EED4CAC4E0。状态查询超时仅是观察失败，未重复启动/重启/kill；后来同job取得明确terminal。

两root每侧12tick四字段oid/action/hp/vx适用formal trace和OFF/ON声明hash、完整native scalar/legacy RNG比较通过，不是全域native证明。两千人32tick OFF/ON的逐tick声明extended/parity、lockstep、完整双RNG、actualRole/noabort/actualAi1000比较均执行至最后非零skip断言，然后因0失败；原Run还检查3 prepared数组identity/容量与关闭三0。不把“失败前断言通过”包装为整个准入PASS或性能证书。

两千人探针现标Explicit并说明NOT_APPLICABLE；原actualSkips>0断言不删除、不改为>=0，不再次跑同负载刷PASS。Explicit是已拒绝候选的手工资格探针隔离，不会把已存在失败原件变成通过，也不是生产回归测试的新豁免。开关保持OFF，只保留未推广诊断候选和完整审计来源。

## 局部收益只作为被否决的外推线索

1000 managed collector夹具（非实际AI/非FPS/GC UNKNOWN），8warmup+16sample交替OFF/ON：
分散mean11.60806875→10.2743ms，capture15360→2560，skip79360；
集中mean150.8763375→87.3339375ms，capture614400→2560，skip3673600。
这说明人工满容量场景能触发守卫；真实Sakura资格skip0说明这个局部信号不能支持当前两千人工作负载的推广。不猜测具体不触发原因全是nearest或所有实际ITR的vrest配置，未新增分布计数。

本批没有实际Play/FPS/可靠完整camera0GC/Profiler/M0新测量。最新有效性能仍复用83：Role诊断OFF logic mean42.458058/43.187729ms，P9547.148750/51.420515ms、drop263/262；SetPass5、中央CPU draw2，不等真实GPU batch。H07精确33ms和0drop门未过，H11旧完整camera失败不撤销。

## 接口与输出审计更正

新增nullable roleCapacityRecordGuard现位于原composeEnvelopeBindingEligibility之前，默认null；原Last参数名/默认false及旧case/helpers未改。旧composition调用均为命名参数，冷接口回归已经2/2修复，没有改生产Runtime或profile默认。

既有SaveNew会在逻辑名字后追加Guid，然后FileMode.CreateNew。before-manifest-02四个无Guid的文件名只是家族意图，事前将其误称准确完整路径的表述在Task/Operation已更正。实际仅生成下述两个root文件；千人断言失败先于SaveNew，不能声称四份JSON均生成，未更改SaveNew或覆盖旧输出：

- driver-qualification-01/right-x550-capacity-record-driver-parity-8f2d8d9199004cf4b6e919124458804a.json，SHA0B1BB8DFE71BED1270D4DC8D418B780E62A93B3FC2E9784278C21FE040ABD679。
- driver-qualification-01/left-x350-capacity-record-driver-parity-a447eef77c7b466ebda06992acceb3bc.json，SHA2C129056B5E0B671889D66DD3EDDF3F7F7441AEF8F034CE22E4517AD29440E4E。

有效manifest02的754非写域保护项重新核对0漂移；9个操作前当前dirty原件副本核同，增量版本和7份XML全部保留。一次guard只读工具误把返回值函数当out-map使摘要0并打印列表，未写文件；已按返回值重新核验754，不能以错误0guard结果证明保护。

原Editor2022.3.62f3/MCP6402精确import/EditMode，Console error CS0；原Menu sole clean8/非Play/非Compile/无运行测试，HEAD a10c358eeb956f2153e7d5bfbb60c3c1b353fd50、staged空。终验Validate-ChangeLedger.ps1和git diff --check结果记audit-final-01.json；静态验收不使两个准入失败变PASS。没有启动第二Editor、Scene保存/切换、资源/设置/Gen/Plugins/Server改动、删除/移动/破坏性Git/push。

Unity CLI技能的共享Editor安全流程用于既有2022编辑器，不安装Unity6 CLI/Pipeline、不升级编辑器或包。技能没有授权新增场景或资源操作。

## 下一必要动作

不继续推广这个未触发候选，不重复局部cost/33资格/root或同32tick千人。复用83 CharacterInput约10.52—10.59ms、LateEntityUpdate约5.37—5.41ms线索，先在既有输入/AI path及已有细分diagnostics中定位实际热点，再准确Task/Change后做有信息量动作；不是所有CharacterInput耗时都叫寻敌。现有AI模块已含地面/空中/team空间索引，不能未测就再造网格。

现有Role adaptive Direct/Sweep/tree的81实际收益和82限定资格复用；生产profile冷默认切换仍待人类准确授权，仅阻止该推广，不停止独立已批准优化。完成边界按有效Goal合同0—8节，4/6不标Goal complete，不按次数停目标。
