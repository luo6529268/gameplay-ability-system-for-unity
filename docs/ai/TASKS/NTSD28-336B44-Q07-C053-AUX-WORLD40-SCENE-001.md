# NTSD28-336B44-Q07-C053-AUX-WORLD40-SCENE-001

状态：`VERIFIED_SCOPED_SCENE`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/Q07/C053；本包已补齐受控辅助 type3 双命中案例在原 Unity Battle Scene 的 40 tick 五槽 World 对照，不关闭 C053/Q07。

2026-10-04 出口：原Editor生产Driver40tick，五槽有效字段正式源/Unity1136/1136、RNG标量200/200零差；根正式EXE/源该五槽另有1136/1136证书。退出非Play/Scene clean，Battle/Menu/GameConfig/Mode四SHA稳；独立EditMode残留结果为原序列化Driver1/World绑定0/Scene Pool0。只关闭受控子门，正式物理键、自然OID251/action0、其它C053和Q07/Q12仍待。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-WORLD40-SCENE-001/REPORT.md)。

2026-10-04 残留补证的脚本二次修改前登记：原Editor40tick结果已出，五槽有效字段1136/1136与正式源相同、RNG标量200/200同，退出非Play、Battle Scene clean/四SHA稳；现有结束报告没有独立记录原Scene Driver是否仍绑定World。只在同一已声明脚本增加独立 EditMode 菜单，核对本次结果的Scene SHA并只读枚举原Scene Driver/Pool，向本包新路径 CreateNew 写一次残留JSON。无需再次Play，不动旧结果、生产、DAT、Scene或非战斗。

权威：用户选定的根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`、对应 playable `GameSession28`/`SimulationTickDriver28` 与正式 runtime 内容。正式源与根已在 `artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001/` 对同初态 40 tick 的槽 0/1/2/50/51 有效字段 1136/1136 零差。第三对象 OID251/action0 为受控辅助体，不是自然角色选招；背景1只用作正式诊断共同域，不部署 Unity。

Unity 现状：既有 `NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor` 的 AuxGreen 12 tick 在原 Battle Scene 与正式源所选 261/261 同，但没有后续 28 tick 和五槽完整快照。原 Editor 目前非 Play、非编译、唯一 Battle Scene clean；磁盘 Battle SHA 需运行前重新核对，所有并行 UI/用户修改保留。

声明代码路径：仅 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs`。增加独立 Editor 菜单和唯一 run ID，复用原 AuxGreen 的正式内容、双角色＋受控辅助初态、seed/mode、生产 Driver、退出清理；新模式推进 40 tick，逐 tick 只读五槽 active/OID/action/HP/源 X/Y/Z 和现有 RNG 标量。新结果置于 `artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-WORLD40-SCENE-001/`，只在完成时以 CreateNew 写一次，不覆写旧结果/请求文件。其余只改本 Task/Change/Ledger/STATE/handoff/当前总表及证据报告。

验收：先生成 Editor 工程编译 0 error，再确认原 Editor 导入 0 error；运行前唯一 clean Battle Scene/四保护文件 SHA；原 Scene 40 个完整生产 tick；把五槽有效字段逐 tick 与上述正式源 CSV 比较，明确首差或无首差和可比值个数，不把受控初态称自然玩家链；退出后非 Play、Scene clean/保护 SHA 不变，检查 World/池残留。只跑这一个聚焦案例。若并行场景变化或探针失败，保留结果并如实报告，不保存或回退 Scene。

风险与回滚：测试脚本会在 Play clone 暂时创建受控辅助体，且此文件含既有 Q10/其它 C053 入口，改动须严格限于新 run 分支。失败时停用新入口，保留原件；删除任何文件另走文件操作审计和用户批准。DAT 数据、图片、Scene、Prefab、生产战斗、非战斗逻辑均不改。
