# Q05 同版本恢复与回放验证

当前为 VERIFIED / Q05_RESTORE_REPLAY_EXIT；仅完成Q05合同、同版本恢复回放与局部真实Scene生命周期出口，不宣称整体战斗已对齐。禁止 computer-use，所有结果来自当前 Unity Editor 桥接、测试 XML 和请求文件探针。

## 已验证

- 实际 Logan runtime DAT / catalog 330 定义通过隔离测试入口进入 Authority400 与 MobileExtended；内容投影 `3900ECBC509557DB`，五版本仍为 13/21/24/2/2，trace v3 / raw-source v2 / 50 字段。正式项目资源部署尚未实施。
- 固定场景 seed 682973786，实际输入槽 0/1。原 3 tick 输入后明确延长 neutral tail 到 24 tick，tick 2 捕获 checkpoint，再继续、破坏当前状态、恢复并重放余下 22 tick。移动场景使用 BattleLockstepSession，攻击场景使用既有 BattleWorldSimulationTickExecutor；每 tick 64-bit checksum 与最终完整 checksum 一致。两profile均通过。
- claimed/raw 未占用槽的独立 +2F8、raw InputHistory、角色数值和 RNG 进入恢复验证。移动过程中观察到 8 种帧/位置状态，非空 World 纯数字复制测试。
- 攻击实际生成入口执行 1 次、注册 1 次；slot 50 在 tick 6 的 late-entity-update 内 allocate → free → unregister-deferred → unregister-flush，所以帧末最大对象数仍为 2。恢复后的有序事件序列与原执行一致，不能以 SpawnCount 单独代替此证据。
- OID 124/type4/action40 来自真实 DAT，通过实际 LogicEntityFactory/OPoint task 生成；同一 pool instance 复用，slot generation 1→3，+2F8 在复用时重置为 -1，旧 handle 拒绝。纯值 snapshot 恢复先前 generation 和 +2F8=37，未来 handle 拒绝。错误 catalog identity 在恢复前拒绝，完整 World checksum 不变。
- 上述用例暴露了旧 shell 未归还所属 pool 的生产缺陷。独立 `NTSD28-Q05-SNAPSHOT-RETIRED-SHELL-POOL-RETURN-001` 修复后，恢复时借用数由错误 4 变为 3，测试 World 有序关闭后 object/slot/borrower 均为 0。带 Renderer 的被丢弃 shell 在修改 World 前拒绝；保留 shell 不回收。不改十一阶段顺序。

## 命令与证据

通过 `D:/anaconda3/python.exe -X utf8 Temp/Goal13_bridge.py` 调用现有 Editor 的 `refresh_unity`、`read_console`、`run_tests`、`get_test_job`。未启动第二个 Editor。

- `related-77-results.xml`：job a17ae351c13d4dfeae504adc67f094e3，77/77 PASS；含恢复9、关闭4、slot3、session9、双OPoint/完整边界20、版本23、本包6、pool修复3。
- `final-replay-6-results.xml`：job 6067b971aa93487689375c81a5f7effa，最新断言6/6 PASS，已加入真实注册/注销事件重放及错误内容身份拒绝。此6项属于前述77项，不能相加为83个不同测试。
- 完整 BattleRuntimeSelfCheck：请求/结果 UTC 与 `SelfCheck.result`，实际 PASS。
- 新增事件诊断曾有两处必填参数遗漏；修复前45项执行的是旧程序集，归档为 `pre-instrumentation-assembly-45-results.xml`，不是新增事件验证证据。编译错误修复后才运行上述77/6项。
- `workspace-protection.json`：3059文件，2930不变、111已声明/既有差异、18既有缺失；相对上一出口无新增缺失或新增差异路径。Scene SHA 仍为 `a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f`。

## 当前边界

真实 Scene 原地恢复/Renderer保留与关闭后两帧零残留、退出/再次进入已经通过（play3-pass.json / play4-reentry-pass.json）。该 Play 使用当前 Unity 旧内容，Logan 实际数据回放是独立的 Editor 验证，不混称正式新版画面/物理输入/整技能验收。

真实Play最初两次失败：World对象4而战斗槽2，另外两个是LF2ObjectRenderer注册。独立 `NTSD28-Q05-SNAPSHOT-RENDERER-REGISTRY-RETENTION-001` 修复原恢复中的数量混用和整个bucket清空；仅重建实体项、保留双向owner仍被local snapshot持有的Renderer，未知模拟对象拒绝。两个dormant/待flush回归也确认活动数与占槽数独立。没有改Renderer生命周期框架、Scene或非战斗逻辑。

最终 job6400c6d4e1fd432cbe7fe837231dad20 / `final-82-results.xml` 为82/82 PASS（前77+新增Renderer5）；warm循环8次零新增分配。`SelfCheck-final.result` 在最终生产修改后实际PASS。两次真实Play均tick5/objects4→4，停止后objects/slots/logic borrowers/render borrowers全0，两帧仍Stopped；场景isDirty=false/root14、旧SHA保持，Console当前0error。三Record合计6个脚本，其中生产只改两个snapshot文件，其余4个Editor验证脚本。早期失败/旧程序集证据全部保留。

Q05步骤1来源、步骤2退休载体、步骤3身份/边界、步骤4五版本与trace、步骤5恢复回放的限定出口均已满足，BATCH-02可交付。13/21/24/2/2成为已验证的联合schema基线，后续不得重开无依据版本迁移；这不把尚未接通的Q06 consumer或Q07正式资产宣布为完成。R13/Q05载体与schema回访子条件、R15/Q05身份版本回放子条件已满足，其余触发条件保持。

呈现边界仍明确：本包支持保留本地已有双向绑定Renderer的原地恢复、无Renderer逻辑World的纯值恢复；不能跨World自动生成Renderer，丢弃仍有呈现绑定的shell继续在修改World前拒绝。R16在本次两个必要快照修复上的关闭/重入子条件已通过，后续新增producer/queue/renderer仍按原触发规则验收。最终账本506 Records / 6 governed code files PASS，`git diff --check`无空白错误。

此前同源 native/Unity raw 仍有六个 MISSING，已绑定数值首差 tick3 currentMp：native200、Unity201。此包只证明当前 Unity 版本自身恢复重放一致，不修改或掩盖该规则差异；Q06追踪实际资源 consumer，Q07后正式内容与Q09/Q10表现、Q12完整终验继续。总目标 ACTIVE / FULL_ALIGNMENT_INCOMPLETE。
