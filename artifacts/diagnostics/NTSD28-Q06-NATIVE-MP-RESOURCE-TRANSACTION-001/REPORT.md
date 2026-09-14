# Q06 MP资源事务实施与验证

VERIFIED / SCOPED_MP_TRANSACTION_AND_DEFAULT_PRODUCTION_PASS。完整MP事务与当前默认生产路径已经通过，尚不代表Q06全部资源/模式/生命周期闭合。HUDBg独立Scene保存差异已于2026-09-14获用户确认为自己或其他任务的修改，已保留；不宣称Scene hash unchanged。

## 行为与范围

原版BattleWorld28::advance_native_resources_pre_display_range的MP段已进入共享BattleRecoveryStatusWriter.ApplyMpRecovery：World.ResourcePhase3、真实当前frame/definition、cmp/有符号右移、regen_mp各族、stats.bound/weak/阈值、51/52 basis例外、bonus、mode原值==1、F6负向消耗及helper内0..500限幅。ECS与LF2Entity回退两个实际caller共同接入；HP和负环境事务主体保留。

入口只接受不可变规则参数，没有新增World字段、schema或全局可变开关。当前尚无正式模式记录投影的caller明确使用GameSession28默认模式原值1，事务可接受任意mode原值；Q08仍必须注入真实选中记录，不能把本包写成所有模式已接通。五版本13/21/24/2/2及trace3/raw-source2/50字段不变。

父Record准确五脚本，生产仅writer、ECS caller、LF2Entity三个文件；新增Editor验证和workspace-owned C++ witness。独立DEFAULT-MP-RECOVERY-FIXTURE-CORRECTION-001修正三个旧测试文件，共8个声明路径。没有修改GAS/Mono架构、菜单/普通HUD脚本、资源、Scene文本、Gen/Plugins或外部Server；没有新增manager/queue/pool/停止阶段。

## 验证证据

- 原版正式EXE B1E13AE1…身份保持；native witness链接同07CD47A0…的75源/header闭包，通过原DatParser和BattleWorld28实际成员函数输出 **2028条向量**。文件native-mp.tsv、native-build-manifest.json。首次缺include的构建错误单独保留。
- RED job65be3777bfdb40399f84b9adbbbf69cf：11项7FAIL/4PASS，复现cmp/负向消耗两caller和缺完整入口。首次GREEN job0d1f70a0518e49098fbed0e01cfcaa02：12/12 PASS，含2028条逐值比对与实际Logan capture。
- 相关回归初次105=71PASS/34FAIL；33个旧默认MP增长预期、1个phase/no-op夹具，独立Record纠正，HP/weak例外与timer断言保留。SelfCheck旧PP0→4预期失败也留证。新增regen=-7的两路径测试验证最后一tick bonus先被消费再归零。
- 最终 jobb8b1f775837c46cb8ccef536cc2a0c99 / final-119-results.xml：**119/119 PASS**，含原版矩阵、两caller/错相/缺frame/warm0分配、恢复、session、Logan24tick重放与旧相关测试。2028是矩阵向量数，不另算2028个Unity测试。
- 完整SelfCheck按request UTC实际复跑PASS：SelfCheck-final.result及时间证据。当前Console0error。
- 重新执行原冻结native scenario并重新采集当前Unity，完整content身份相同；**两槽tick1/2/3 MP均200，原200/201数值首差消除**。用当前源码重新构建NTSD28Parity后比较300字段出现，44类已绑定字段一致，36个差异出现全部属于原6类MISSING。final-raw-comparison.json状态仍different，CLI退出1是严格保留未绑定差异的结果，不是全对齐证书。最初误用旧Release比较器被header版本拒绝，old-comparator-rejection.json保留，没有改header绕过。
- 真实Scene Play：OID2、tick5→6、资源phase3=0，注入中性状态后通过实际Driver tick观察默认模式MP保持200；显式mode0增加到201，weak>0抑制继续增加。之后全World checksum恢复、对象4→4（play-mp-pass.json）。输入为程序注入，Scene仍旧内容，不称物理技能/正式新版图片验收。
- 随后已有Q05探针复验恢复和App/Driver有序关闭：对象/槽/逻辑及渲染borrowers全0，两帧仍Stopped（play-cleanup-pass.json）。Scene已返回EditMode，dirty=false/root14。

## 范围保持与待确认

workspace-protection.json：3059文件，2926同基线、115已有或已声明差异、18既有缺失，无新增缺失。新增差异路径只有两个生产恢复文件和两个旧测试，均在Record覆盖；其他当前修改路径此前已属累计差异。

Scene SHA从a96e1106…变为bcd1047b…，唯一Git差异为HUDBg RectTransform anchoredPosition.x从50变30。文件保存时间2026-09-13T15:38:24Z，早于本包SelfCheck和Play；已向用户异步询问来源并保留该变化。dirty=false仅表明保存状态，不证明文件未变。该UI差异不纳入MP实现或作为战斗规则修复，本项来源仍待确认。

## 后继

Q06继续HP/pre-display整体资格、chp/regen_hp/regen_dhp/弱状态早返、display/post-display、CPoint/OPoint/+2F8/复活/terminal/pieces及landing精度。下一独立任务NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001。R07的MP事务子条件已验，正式mode/Q07内容联合证据保持等待；R02/R15同内容数值复验子条件已更新，不晋升六MISSING。默认stage.dat和既有表现例外保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use。


## 用户范围确认（2026-09-14）

用户明确答复HUDBg x50→30是自己或其他任务的修改。来源待确认标记现由USER_CONFIRMED_EXTERNAL_CHANGE取代，原检查事实和文件保留。当前Scene SHA bcd1047b…作为后续保护现状，不能回退到a96e1106…。MP本包已VERIFIED（完整事务/当前默认生产路径限定出口），Q08真实模式记录注入和其余Q06仍待，下一HP Task不变。
