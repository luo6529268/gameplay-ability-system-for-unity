# Q07 state15 通用速度修复的第二实体回归（2026-09-25）

原 `NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/FULL-WINDOW-COMPARISON.md` 记录：鸣人螺旋丸续按攻击的 tick26 开始、保持2tick 夹具错过转螺旋手里剑；完成 tick30 时生成 OID434/slot52/action397 的源模型 X 速度为0、当时 Unity 为550。该旧结果只来自配对 playable 源模型／当时原 Editor，不是正式根 EXE 独立同条件录制。

旧测试 `NTSD28UserRasenganWindowEditorTests.FullRasenganWindowRecordsPhysicalAttackBoundary(26,2)` 仍存在，但原 Temp 三 tick seed 已不存在。本次从仓库 `Tools/NTSD28AuthorityTrace/Scenarios/neutral-two-entity.json` 的旧诊断格式重建一个**新**临时 seed：seed682973786、Stage23、鸣人 OID2/action241/MP200/X500/Z650 对 OID7/X1200，3tick seed 内无输入；测试本身在32tick的第26、27个场景输入 tick 持续注入 Jump。该 seed 用 legacy 诊断 schema/参考ID，仅为 `WithLoganScenarioForReplayTests` 的输入格式；实际测试方法明确给 `RuntimeRoot` 为正式 Logan `resources/runtime`，不能将 seed 的旧参考ID当成当前规则权威。保存重建 seed 的 SHA-256 为 `03FDC515C4BBFF409EB0589032FDA962C5EFBE75E56027E228938025FC925903`。

同一原项目 Editor 实例的唯一 EditMode job `381af4720ec34f61a0ed66c40ad222da` 执行所选 `(26,2)` 一项，结果 **1/1 PASS**，无失败/跳过，32个连续完成 tick 行输出，原始文件 SHA-256 `87B37AEB675912E815D50033DF5B23AC579E562D24865F97C2E31E7AC29A310D`。聚焦读取生成 OID434：tick29 slot52/action396/state15/Vx0；**tick30 slot52/action397/state15/Vx0/X494**；tick31 同 slot/action/state/Vx0。另一个 OID434 slot51 在 tick30 是 action35/state1001/Vx0，不能拿它代替目标。shutdown 文件为 `AwaitingRuntimeMapCleanup / ObjectPoolQuiesced`，对象0、slot0、借用0；这份已有测试没有完成后续 map-cleanup 事务，不把它写成完整有序关闭验收。Editor 事后 idle/Edit Mode。

这是针对旧 OID434 首差的**跨实体 Unity 回归**：现行通用 state15 后物理修复后，第二实体在旧首差所在的 tick/action 上也不再写回原始550。因为临时 seed 是从保留报告重建、旧 C++/正式 EXE 轨迹没有在本次重新生成，此证据不能宣称全部32tick与正式 EXE 逐字段一致，也不关闭用户报告的画面可见按键时点或 R18/Q07。未修改生产代码、DAT、Scene、GameConfig或非战斗功能。
