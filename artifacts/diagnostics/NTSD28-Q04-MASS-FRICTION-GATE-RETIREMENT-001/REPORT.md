# Q04-A Mass摩擦条件退休

状态：VERIFIED_MASS_GATE_ONLY / Q05_CARRIER_PENDING。日期2026-09-13。生产改动只在CharacterMechanics.StepBattleLogic移除`&& ctx.mass > 0f`，增加合同注释。地面判断、积分/摩擦顺序、数值/epsilon、重力、落地和边界逻辑保持；mass/context/snapshot载体及schema仍留Q05。

## 结果与证据

- native PhysicsIntegrator28::step真实源码witness已编译执行8个场景，输出在native.tsv；再次运行byte一致，SHA B3D1673FEE3E41F191709CB4FA450B3B478011BC7C7248500813D57806DBEAAB。源码/正式EXE/fixture身份见prechange-source-identities.json，编译日志native-build.log。权威EXE未修改。
- RED job fa10ab39cd7d4d8c83605b663f722d2f：9项中6失败、3通过。mass0/-2在core、真实LF2Character.ApplyDynamics及ECS exact-character三入口均错误保留速度5/-5；native为4/-4；mass1通过。
- GREEN job05ce4f3ab0b34684b90a4a8c0a9cb339：本包9项+B4物理核心5项，14/14通过。core覆盖正负/小速度、阻挡、空中、刚落地、负collision reference和整数Y判断；刚落地的VX/VY后处理由Unity调用方执行，测试明确分阶段，不把core冒充整个native step。
- 完整SelfCheck：01:22:33 UTC写新request，结果timestamp更新后PASS，selfcheck-result.txt和selfcheck-request-time.txt保存新鲜性证据。
- 真实Play：现有NTSD_Battle世界、3个注入的非Mono逻辑角色，mass0/-2/1；实际driver ground tick2076→2077均X移动5/Z移动-5，Vx4/Vz-4。接着实际driver landing tick中三者结果一致，Y0/Vy0/Vz-5，VX见下方独立精度记录。finally注销临时角色，实体数与claimed slot恢复，cleanupPassed=true。play-final.json记录6行；这是synthetic注入/真实driver验证，不是物理键技能或正式新内容验收。
- 最终Unity编译错误查询0；退出Play后NTSD_Battle isDirty=false/root14。改动台账PASS：473 records/49 governed code files。3059保护基线当前3046不变、13项声明生产变更、0缺失；其中12项为前Q02，新增只有CharacterMechanics。

## 保留的失败与边界

扩展相关job4dc48dc005eb403a89e7b75b2ddd6623有19/20通过；旧NTSD28C06NestedPhysicsProductionEditorTests.FullTick_RecordsNestedPhysicsBeforeRevivalStageBoundsHeldAndSerialRemainder在空World硬编码phase[28]期望FrameAdvance，实际Stage。空World不调用mass分支，pass表未改；失败保留于green-related.json，登记Q12 fixture复核，不修改生产顺序迎合旧测试。14/14不是宣称20/20。

Play probe首次编译使用不存在的TryGetInstance导致CS0117，改为查现有组件，生产driver未改。首次Play fixture固定Z200落在真实stage min237外，钳制导致位置断言失败，摩擦4/-4已经正确、cleanup通过；报告play-first-stage-boundary.json保留。只修probe为warmup后取真实stage中点，重跑通过。Play切换发生域重载/旧连接超时；检查新域、结果时间后才在当前连接运行，未把旧报告当成功。

另发现独立的既有落地精度首差：native完整step `motion.x /= 3.0`在输入5时为1.6666666666666667；Unity Play为1.6666666666666666，当前LF2CharacterDamageStateResolver相关分支使用`*=0.3333333333333333`。这不是mass gate引起，三种mass都相同。本包只证明mass不影响落地，**不证明完整落地bit parity**。该首差必须进入后续Q06/B4精确数值包，再复核相关real/generic/weapon路径，不直接全仓替换所有三分之一运算。

## 范围与后继

实际新代码：Editor focused测试、Play probe、native witness；生产只有CharacterMechanics单gate。没有改非战斗功能、Unity/GAS架构、Scene/Prefab、DAT/图片、输入资产、Gen/Plugins、版本或关闭顺序。没有提交/push/删除文件。

Q04-A关闭，R13仅mass行为子条件PARTIAL_RETURN。Q04-B下一处理旧Oscillate剩余reader；Q05统一删除mass/reserved/Oscillate载体并升级已声明schema。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，BATCH-02未完成。
