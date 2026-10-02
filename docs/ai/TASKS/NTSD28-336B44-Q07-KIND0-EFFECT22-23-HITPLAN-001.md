# NTSD28-336B44-Q07-KIND0-EFFECT22-23-HITPLAN-001

状态：`FOCUSED_TEST_PASS / SYNTHETIC_FIXTURE_ONLY`。父项：`NTSD28-UNITY-BATTLE-REALIGNMENT-001` / 新版336B44 G1/BATCH-04/Q07；关联 C053 扩展 ShadowCompare 相邻首差。

权威与 RED：当前正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应 playable `source/ntsd28_core/src/simulation/battle_world.cpp` 无甲普通命中调用 `HitResponseResolver28::accumulate_unarmored_horizontal`；`hit_response.cpp` 在非死亡稳定、非state2000、非type4/6目标时按攻击者朝向乘完整 `itr.dvx`，不对effect22/23另按相对X分支。相对X专支存在于另一 `accumulate_ordinary_horizontal`，不能跨路线复用。Unity实际 `BattleDamageWriter.ApplyStandardCharacterDamage` 对此路径也通过默认朝向冲量写入。既有原Editor扩展聚焦34例中，仅角色effect22、23两例 ShadowCompare writer mask 为`0x200020000000`（Facing+KnockbackVx）及`0x200000000000`（KnockbackVx），原始JSON：`artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001/focused-tests-status.json`。另15例type3旧测试已独立修成15/15，不在本包重做。

预期最小改动：仅在 `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs` 的 `ProjectStandardCharacterDamageWriterEffect` 删除 effect22/23 相对X投影分支，让后续既有“按攻击者朝向乘dvx”分支与正式无甲路径及真实写入同态。不修改 `ProjectAlternateCharacterDamageWriterEffect`、武器/其他对象路线、真实 `BattleDamageWriter`、DAT、图片、Scene、Prefab、ProjectSettings、非战斗逻辑。不得改变 source/physical 坐标统一入口或已批准相机例外。

验收：先保留上述两例 RED；修改后生成Editor工程编译0错，原项目唯一Editor刷新并仅运行 `ShadowCompare_StandardCharacterDamageWriterEffectMatchesAuthorityState` 方法组；要求 effect22/23 writer mask0，且该组相邻effect无回归。检查四保护资产磁盘SHA、Change Ledger、`git diff --check`。这只证明投影与真实写入的合成夹具同态，不证明正式DAT可达、根EXE自然Play或Q07整组关闭。若出现新首差，先核正式源码，不能盲目放宽测试。

**2026-10-02 测试夹具范围补充（第二次脚本编辑前）：** 投影专支移除后，原Editor18例的effect22/23均越过writer mask断言，后在测试自带`expectedKnockbackVx=-0.9`处失败，真实均为`+1.1`，其余16例通过。夹具`CreateScenario`给攻击者默认右向（`NTSDEntityRuntime.Dir="right"`）、X0，目标X10、`itr.dvx=1`、目标原待处理水平量0.1。正式`hit_response.cpp::accumulate_unarmored_horizontal`在角色目标、非state2000时不看effect22/23相对X，右向产生`+1`，故终值为`+1.1`。只把同一测试方法两个`TestCase`的旧`-0.9`改为`1.1`；不改其它参数、断言、真实写者或正式DAT。原Editor第一轮18例结果保留：`artifacts/diagnostics/NTSD28-336B44-Q07-KIND0-EFFECT22-23-HITPLAN-001/focused-status.json`。

回滚：精确审阅本方法投影分支及RED/GREEN原始结果后按仓库删除/恢复授权处理；保留历史记录和用户既有脏文件。

实际出口：最终生成Editor工程0错/249 warning；原Editor程序集晚于两处脚本，`ShadowCompare_StandardCharacterDamageWriterEffectMatchesAuthorityState` 18/18 PASS，原三方法组合34/34 PASS，原始结果分别为`artifacts/diagnostics/NTSD28-336B44-Q07-KIND0-EFFECT22-23-HITPLAN-001/focused-status-v2.json`与`combined-34-status.json`。原Editor停在Battle Scene、非Play/idle；Battle/Menu/GameConfig/ProjectBattleModeConfig四资产磁盘SHA与前包基线逐项相同。仅此无甲合成路径的投影/真实写入同态证书；正式DAT可达性、同条件根EXE、自然Battle Play及Q07整组仍开放。
