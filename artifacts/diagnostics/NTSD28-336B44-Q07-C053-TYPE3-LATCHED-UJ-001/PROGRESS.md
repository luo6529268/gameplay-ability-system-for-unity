# C053 共用 type3 响应帧读取进度

状态：`CODE_WRITTEN / UNITY_RUNTIME_PENDING`。当前正式权威为336B44根EXE及对应 playable live source。没有把外部工程的35项说明或旧版B1E13测试结果晋升为当前运行时结论。

- 正式 `battle_world.cpp:7030-7044`：普通 Fj 读取目标当前动作，Uj 读取动作锁存帧；源 `data/data.txt` 索引OID808 `a/kat/kat.dat`，帧153有`hit_Uj:156`，帧156该字段缺省。current156/latch153可区分旧Uj回退20与正式Uj156。
- Unity 原 `BattleDamageWriter.ApplyNativeType3TargetGenericContinuation` 和 `BattleEcsHitExecutionPlan` 预检/投影各从 `target.Frame.D` 读取Uj。此次仅增共用`ResolveNativeType3TargetResponseAction`：Fj仍取当前`Frame.D`，Uj取`Runtime.WaitCounter`所指帧；三个消费者同用。不改type3匹配pair reset、kind-table transform。
- 既有Editor测试新增Uj current156/latch153与Fj current77/latch88两例。测试先写，但原Editor未导入，**没有实际RED或GREEN**；生成`Assembly-CSharp-Editor.csproj`包含该测试，`dotnet build --no-restore -v:q -clp:ErrorsOnly`成功、263警告/0错误。
- 原Unity Editor PID11944仍响应，但其`Library/ScriptAssemblies/Assembly-CSharp-Editor.dll`写入时间早于本轮新增脚本。不能用旧程序集的Play/测试声称本修复通过；不启动第二个写同一Library的Editor，不使用computer-use。正式根/原Battle Scene同初态仍待，父C053/Q07/总目标保持开放。

下一步在原Editor确实导入后先运行新增两例及邻近type3目标测试；再挑正式OID808产生 current/latch 不同且有后续命中的可达完整tick链，对正式根与原Battle Scene按同 seed/输入/初态比对动作、目标归属和后继候选。若找不到可达链，保留聚焦机制状态，不伪装整场验收。

2026-10-01 原 Editor 后续结果：已导入本轮脚本，完整类名的两项聚焦 EditMode job `4c72782ca2874157937b226ec28d9522` 实际2/2 PASS；第一次误用缺`.Editor`的测试名得到0项，不能作为通过。当前只把共用响应解析推进为 `FOCUSED_TEST_PASS / RUNTIME_PENDING`；正式OID808可达完整tick/根/Unity Play仍待，C053/Q07开放。

2026-10-01 只读可达性线索：当前正式 `c/jira/sag.dat` frame554 与 `c/hir/hir.dat` frame554 均以 OPoint 生成 OID808/action150；`a/kat/kat.dat` 的150→151→152→153，153/154 为 state3000、`hit_g:155` 且 `hit_Uj:156`，155 为 state15、next156、`hit_Uj:156`，156 无 Uj 字段。正式 `physics_integrator.cpp` type3 接地分支在 state3000 且 hit_g非零时选动作155；因此可研究 action155 锁存后推进到当前156的受击窗口。**这里只证 DAT/source 存在一条候选链**；尚未证正常选招会生成 OID808、接地时点、同tick锁存155/当前156、几何命中或进入 `hit_Uj` 分支。既有 current156/latch153 聚焦例是条件反例，不应宣称其必为自然相邻帧。下一步先用当前正式 GameSession 完整tick搜证同帧锁存/受击，再决定是否送根LFR与原Battle Scene；未命中则保留 C053 `RUNTIME_PENDING`。
