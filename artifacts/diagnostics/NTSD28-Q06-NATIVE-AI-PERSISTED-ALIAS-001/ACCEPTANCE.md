# NativeAI 持久别名消费验收

2026-09-21。限定范围：NATIVE_AI_PERSISTED_ALIAS_CONSUMPTION。

本包修正持久别名到派生 AI 行和当前 native 决策的消费链。不是全部 AI 行为、所有角色内容或 Q06 整体对齐证书。

## 问题与最终行为

旧派生行没有 NativeAiProfileObjectId，kernel 把 actual ObjectId 当作别名分类/匹配；非零别名的 unsupported 分支未结束 ordinary 决策，实测负别名主入口出现多余移动及额外4次随机数调用。

现在三条实际行采集入口写入持久别名，数组增长保留，旧行校验与完整/单行比较识别 alias-only 变化。kernel 按源码区分 recognized classifier、非负 alias-or-actual matcher 和 actual-ID1 专属追击。特殊决策先消耗0x3c；通过门后成功33组合技或未成功的非零alias均停止本次决策，后续输入采样与tick继续。未重做出生/融合字段生产，未增加新持久schema或runtime owner。

## 权威与源证据

- 正式EXE：B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033。
- playable closure：07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F；native-build-final/build-manifest.json。
- 18例实际公开source API：profile7、special6、ordinary1、main2、two-tick2。双跑156167 bytes完全一致，SHA E3CCEFDA7A02AD530A5868B2C5375518D6434B8D118F1653090373992F36C07D。
- 独立408检查PASS，root复跑PASS；source/independent-validation.json。覆盖别名分支约束、同步RNG数值/cursor算法及聚焦采样。不是完整World/AI独立模型，也不是正式EXE录像。

## Unity 新鲜证据

| 证据 | 实际结果 |
|---|---|
| carrier 初始失败 | job7c9fdd9db6404cf699f32789ea5b4264，4例全部失败于缺失列；三producer前置成立。 |
| decision 初始失败 | joba68ff381f55845c985d9d22fef2eb6a4，15例7PASS/8FAIL；source0/2/4/6/7/9/11/14存在差异。 |
| 相关联合回归 | job01997c88d21741b18bc970fe3e6488df，27/27PASS，0.9922788秒；新21+既有special6（含零分配）。 |
| 正0x3c继续调用者 | jobb9363a6284da443596e79a70e6ed0d91中该方法PASS；difficulty3、负alias、实际正gate后继续0x1c。不是source13全部数值对照。 |
| 实际两tick与恢复 | job1c3e2a68cd9b450586d3e66c46eb6dbd，3/3PASS，0.8175234秒。source16/17 before/first/second全部零差异；alias-only snapshot restore、再采集和两tick重放一致。 |
| 实际stale guard | job7c9487019b5f4f4eb228ae7d614e6e72，1/1PASS，0.60857秒；真实注册/采集，生产post-input guard baseline true→alias-only stale false→真实重采集 true。 |
| 一次稳定fullSelfCheck | PASS，UTC2026-09-21T07:50:14.7862959Z；full-selfcheck.result。 |
| 实际Play | PASS2，UTC07:51:03.6155279Z；source16/17各两tick，使用pooled Renderer；主Scene checksum不变，Renderer borrowers2→2。 |
| Q05关闭 | PASS，UTC07:51:04.2112603Z；restore对象4→4，结束World/slots/logicBorrowers/renderBorrowers均0，保持Stopped两帧。 |

编译/域重载完成，目标测试实际执行；最终旧自检和Play后只增加stale guard测试，无生产变化，复用已通过批次证据。自检期间read_console一次socket超时，随后fresh结果PASS、连接恢复；没有重启Editor或重跑自检。

## 失败与夹具纠正记录

所有初始结果保留。carrier full-row比较夹具最初未初始化无关candidate products，明确隔离该独立前置后仅重跑失败项。tick夹具最初spawn绑定canonical store后只Restore runtime，导致旧pending未进入实际读取端；现只在首次初始化调用正式同步接口，没有每tick补偿。RNG observer补上accepted AI cursor出口。难度从实际Match.Difficulty设为与source一致的0，而不是只改Flow派生值。原source DAT/pending Up不变；两tick差异最终归零，没有修改生产代码掩盖这些夹具错误。

## 边界与保留项

- 原World/GAS框架、非战斗功能、Scene及正式资产未改；stage.dat USER_HOLD及所有表现例外保持。主Scene dirtyfalse/root14；文件SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6。
- 源及实际tick内容为明确合成DAT，snapshot使用synthetic身份。实际Play在原Unity内容Scene里运行隔离代表，不声明正式330内容、物理按键或像素表现验收。
- 新schema仍entity16/aggregate24/checksum27/core12；raw47/3不变。previousXYZ等source-only字段不提升绑定。
- seed42不覆盖0x39/0x3a/0x3b全部随机成功结果；本包证据是别名准入、受影响callsite和对应结果。旧非同步分支及实际ID sensing未改变。
- FirstDecision出口同时表示组合技成功和unsupported停止，不能把该出口名称当成施技成功统计。
- stale测试直接调用生产guard，证明alias-only拒绝及重采集恢复；不是另建完整rolling publication故障注入场景。
- 本包运行时验证使用DataOrientedCanonical。当前GameConfig.asset:16明确配置该模式，resolver无覆盖时也默认该模式；没有修改模式选择。显式LegacyCanonical及IndexedCanonical失败后转入旧core的异常fallback未在本包制造，不能据此宣称所有兼容/异常路径已对齐；后续consumer回访需先确认可达前置与已有hard gate，不能凭存在fallback函数直接认定动态Bug。
- 已完成融合职责复用。Q06仍未完成，Q07 DAT与角色图片迁移未开始，总目标ACTIVE。

独立最终复核：PASS / GO，允许按本限定scope关闭。完整新测试文件（未跟踪）已被审阅，不以空git diff代替新增文件审查。Change Ledger validator PASS620 records/101 governed changed scripts；该计数是整棵dirty tree，不是本包文件数。git diff --check exit0；最终Console error CS匹配0条。
