# NTSD28-RESPAWN-STALE-INTEGER-FIXTURE-READ-CORRECTION-001 — Task Contract

> Goal7 Part A / 2026-09-09 / VERIFIED / TEST_ONLY；本合同修改前建立，最终证据见同ID Record。

用户明确授权：CheckRespawnReadsPhysicsTailIntegerCoordinates只修改dead原始整数断言与失败消息。
GLM已独立复核并要求直接采用：Authority advance_native_revivals battle_world.cpp:8488-8516与Unity BattleRespawnModule.cs:105-119均只写precise X/Z；整数延迟到下一物理尾。
LF2Entity.cs:4983-4995的GetRuntimeXInt/GetRenderZInt在raw为0时回退(int)precise，不能检验raw零值。

## 精确范围
唯一代码路径：Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs。
唯一符号：CheckRespawnReadsPhysicsTailIntegerCoordinates。
dead.GetRuntimeXInt()==0 / GetRenderZInt()==0改为dead.Runtime.XInt==0 / ZInt==0。
消息分别打印rawInteger、getter、precise并保留expectedPrecise与stale信息。
其余任何断言（包括后续respawn scan不得全局同步）、fixture、生产和时序不动。
附属只允许本Task/Record、Ledger、STATE、对齐总表与Temp结果。Part B零文件改动，结论仅在最终报告，不建立修复包。

## 验收与停止
指定Unity instance gameplay-ability-system-for-unity@b1b02287 / 2022.3.62f3 / NTSD_Battle；不Play，不第二实例。
full SelfCheck该方法全部断言必须通过；下个停点只记录不修。仍在本方法失败立即停。
两套Assembly build0 error，validator通过，Scene SHA保持D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。
代码字节逆替换与初始SHA证明只有允许增量；最终状态仅关闭夹具修正，不代表full SelfCheck或完整battle parity。
风险：后续独立SelfCheck可能失败，只记录。没有runtime状态、World/RNG/资源或shutdown副作用，没有不可逆边界。
回滚须用户明确批准，仅撤销本包断言/消息与治理增量，保留其他工作。
完成后停止等待Goal8；生产与未授权范围继续USER_HOLD。

实际验收：2026-09-09T13:24:41Z full SelfCheck PASS，目标全部断言及后续检查通过，无下一停点；两套build0 error，指定Scene SHA不变。只关闭夹具修正。
