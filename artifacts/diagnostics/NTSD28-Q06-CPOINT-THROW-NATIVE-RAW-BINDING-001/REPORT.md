# Q06 CPoint throw native raw binding

IN_PROGRESS。正式 EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；75项权威源码闭包manifest 07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F。诊断runner编译未修改的playable/core，不是正式EXE观察。

## 源端

`source/first.jsonl` 与 `repeat.jsonl` 各3594544 bytes，SHA c809a36431d9c5b8a81c1cfd3590612fd2c708ddfa9cda41a1eae3481f03a367；392行。运行 `D:/anaconda3/python.exe -X utf8 Tools/NTSD28AuthorityTrace/validate_cpoint_throw_raw_binding_witness.py artifacts/diagnostics/NTSD28-Q06-CPOINT-THROW-NATIVE-RAW-BINDING-001/source`：112506 checks / 0 differences。

矩阵为双方各7种destination（声明0、隐式99、声明900、声明999、缺失999、1000、-900）×两朝向×输入选择开关×injury0/-1。每例确实投掷一次；选择输入时先选101再按原100帧center/next/cpoint投掷。独立after派生覆盖全部已序列化字段，包括第三owner0实体保持，双方counter0、latch保持、无RNG消费。没有正伤害资源转移或depth输入组合；这些不被本矩阵替代。

当前source没有throwinjury==-1变身；已撤销本Task早期沿用Unity的“原definition转换”前提。后继完整tick也已捕获，使用BattleConfig默认mode门，但独立validator只证明立即投掷输出。

## Unity 已运行

- 初始RED job7939fe89bc7440ae985bad4a13c9a5e0：两profile各392 before0/after5676，见production-red。
- 首轮修复 job4cf4c995960b4b7a8a3cce3f36b46d83：联合30项26PASS/4FAIL。投掷raw/extra差异清除，仅两profile各84个选帧缓存wait差异；另两条旧type3 oracle失败已拆独立Record。见after-first-fix。
- ApplyAction选帧改native绑定后 joba4f37d29d54146a09d6ee67f9c7c7736：立即矩阵2/2 PASS，两profile各392 before0/after0，见immediate-2-pass.xml及immediate-*.json。
- kind3原800×4组与既有投掷专项在第一次联合运行已通过；完整后继tick、SelfCheck、Play、回放、有序关闭尚未完成。

只修改CPoint战斗读写和关联测试。保留Unity/GAS框架、非战斗、Scene及全部资源；Q07未迁移。原三个raw缺绑定仍显式列出。当前完成证据不能关闭Q06或总目标。

## 后继tick与当前依赖
完整driver fixture的canonical输入、预注册native profile、关闭误启用AI已修正；源没有AI输入。最终jobe756a088两immediate PASS、两following FAIL，各392 before/immediate0、196后继差异仅28个next99隐式+attack案例，其余364行当前raw/extra/lifetime符合源。source input_routing.cpp907/987 state缺失默认-1，与Unity native ground/airdash typed state0不同；独立Task NATIVE-INPUT-MISSING-STATE-ROUTING-001已经建立，未修改该项生产。完整tick未通过，不关闭父包。
SelfCheck17:40:42Z出现DATA-01C旧Cpoint raw拒绝450/857 oracle（失败归档）；已仅分离该方法legacy ImmediateFrame与native raw断言，重新编译CS0，新的SelfCheck请求等待。Play、回放、关闭待后继输入Task完成后执行。

最新SelfCheck17:43:37Z PASS（self-check-174337-pass.result），旧失败保留；父状态仍IN_PROGRESS / FULL_TICK_DEPENDENCY。不得将此selfcheck通过写成完整投掷生命周期、Q06或资源迁移已完成。

## 最新限定出口
VERIFIED / DECLARED_CPOINT_RAW_BINDING_AND_FOLLOWING_TICK。Fresh SelfCheck18:13:58Z PASS；真实Play18:15:16Z投掷/后继tick1568+输入864全部PASS，Renderer2→2、Scene checksum保持；关闭18:16:07Z恢复4→4、World/slots/logic/render全0、两帧Stopped。before-throw本地回放112场景224重放tick已PASS。最终Scene dirtyfalse/root14/文件SHA不变，Editor idle/notPlaying。先前失败保留，Q06与总目标未关闭。下一准确Task为NATIVE-INPUT-ACTION-COST-FRAME-READERS-001。
