# Unity 命中火花事务验收

VERIFIED / DECLARED_HIT_SPARK_TRANSACTION_SCOPE。总目标仍 ACTIVE / FULL_ALIGNMENT_INCOMPLETE。

## 已实现

- 公共 BattleNativeHitSparkWriter.Append 复用原实体十槽记录和 World.NativeRandom.CrtNext；按原 guard、owner、容量、编码、snapshot 存在性、整数几何、Y/X CRT 顺序执行。
- 原 candidate index 由 SequenceRunner 的同步值类型作用域贯穿 Shadow 预测和实际分发；异常/嵌套退出恢复，旧 CurrentItrIndex 和 persistent schema 不改。直接兼容调用仅匹配 snapshot itr 原引用，否则使用已声明默认0。
- Character DamageWriter 保存命中前 selected armor 和分支，普通/reduced 尾部各发一次；两个外层 Character resolver 移除紧邻重复调用。Weapon/special 既有 RecordKind0Hit 使用共同无护甲入口；缺失的非角色 armor feedback 分支仍交下一 Task。
- Shadow 独立实现相同记录与 CRT 投影，新增 transient CRT state/count 捕获比较，保留旧随机数观测，不修改全局随机算法、C17 或 hit-record 生命周期。

## 新鲜验证

- Unity refresh/compile 后最终 EditMode job `83f41bb5395b4d878c19cba88cca9e59`：34/34 PASS，见 final-tests-34-pass.xml。不是发现列表中的7630项全部运行。
- 438 原源码端点 × Authority400/MobileExtended：876例完整双host数组、CRT result/state/count/order、native调用0差异。使用正式 Logan parser/converter 的合成DAT，不是旧资源部署证据。
- 282 原角色实际路由 × 两profile：564例0差异，经原冻结candidate、真实统一consumer和runtime itr拷贝分发；覆盖原index2、无护甲/reduced/selected armor、容量和几何。非角色feedback不在这282例中。
- 异常/嵌套scope恢复、旧CurrentItrIndex哨兵保持、非kind0不写记录且不耗随机通过。
- 原完整driver两场景 × 两profile × optimized开关：8向量0差异，四项PASS，原Bdefend已清后的剩余Spark RNG差异现已消除；Shadow亦0差异。C17仅保留legacy1/tick。
- 本地snapshot：四route/index向量 × 两profile，恢复后tick2/3共16个replayed ticks，完整checksum、火花数组、CRT状态/计数一致。未扩大为任意跨World恢复证明。
- 旧C01/生命周期22项全部PASS。旧SimTU测试hook改为实际C25测试hook，生产生命周期未改。
- 完整BattleRuntimeSelfCheck：08:06:43Z PASS（08:05:42Z请求）。火花纯端点补显式World/CRT；七处旧武器回归spark随机预期改为独立CRT delta2，其他兼容行为断言保留。
- 真实NTSD_Battle Play：08:11:26Z，两个factory每个438端点+282实际角色分发，共1440例PASS，differences=[]，主Scene checksum不变，Renderer借用2→2。见play-final-1440-pass.json。此前单端点876 Play PASS也保留。
- 最终有序关闭：08:11:53Z PASS，原位snapshot恢复4→4，World objects/slots/logic borrowers/render borrowers均0，连续两帧保持Stopped。见shutdown-final-pass.json。
- Scene dirty=false、rootCount14；文件SHA bcd1047bf912c6a4a8bc9f3a76eaf3fa954211ad064e0402b1c01bf3ba0e9fb6保持，用户HUDBg x30保留。
- 正式EXE SHA b1e13ae17c86b77240b61a971afd4c3374b645705f42b0bbce304fd1d2819033、源438 trace SHA b5df61136227ffcc17f15947ca22360d1234586ca71aa94907632d5e8eece6f8复核保持；生产七文件与验收前hash一致。HEAD61b3b6cf，未提交。

## 原失败与测试纠正

red/保存旧记录和随机流首差、EditMode未初始化renderer pool的前置失败。after-emitter/保存cover96/64差异：新fixture误选legacy ConvertToFrameData，现已改用现成ConvertLoganFrameData，没有修改parser来适配测试。人工count288纠正为源枚举实际282。

spark-lifecycle-22-initial.xml、selfcheck-initial-fail.txt、selfcheck-weapon-rng-fail.txt均保留。测试更正独立记录在SPARK-C01-TEST-HOOK-001和HIT-SPARK-SELF-CHECK-ORACLE-001，不隐藏旧失败。

## 范围与下一步

本包只证明上述火花事务、角色actual路径、现有weapon/special公共入口及对应受测Shadow/生命周期。没有验证物理按键或正式Logan图片逐像素表现，没有修复全部weapon reaction或非角色armor feedback。实际Scene仍为Unity旧内容，Q07正式DAT/角色图片尚未部署。

下一唯一Task：NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001。先核所选armor/特殊关系rest的原完整顺序与Unity入口，准确新Record，然后接公共Append(world,...,armor,false,false)。不能在所有命中入口无条件早返。之后UNARMORED-WEAPON-REACTION / TYPE5-HIT-PLAN-COVERAGE，再回BDEFEND256与父collision/qualification。原96/64/32为上一轮测量，尚未按本轮生产重跑，不作为新鲜数量。

schema15/23/26/2/2、raw47/3、跨World allocation epoch恢复缺口、stage.dat USER_HOLD、其余用户例外保持。禁止computer-use；本包未改资源、Scene、非战斗逻辑、GAS框架或Server。

最终交付检查：Editor idle/非Play/无测试或编译运行，Console error CS查询0条；git diff --check退出0；Tools/Validate-ChangeLedger.ps1通过，565 Records/10 governed code files，历史record路径告警不影响本次覆盖。见editor-state-final.json、compile-console-final.json、ledger-final.txt。
