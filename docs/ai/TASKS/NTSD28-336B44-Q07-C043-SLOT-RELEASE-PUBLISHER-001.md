# NTSD28-336B44-Q07-C043-SLOT-RELEASE-PUBLISHER-001

状态：`VERIFIED`（限共用槽释放失效顺序和所验自然Scene链）。父目标：新版 336B44 BATCH-04/Q07/C043仍开放。

触发：原 Battle Scene 的正式 Lee7/Chi8 自然持有融合链，选定前41 tick除无关系时空槽哨兵外与正式源码相同；第42 tick 正式 OID420 自然退场，Unity 在 `ReleaseRuntimeSlot → ClearReferencesToReleasedSlot → TargetSlotIndex` 抛 `Unified AI row publisher observed a stale slot generation after commit`。真实Play失败和异常原件在 [Scene结果](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/late-double-tap-scene-02.json)；退出后四保护资产SHA未变，池借用非零。

规则与设计边界：正式 playable tick42正常清子体，Unity统一AI发布行只是派生视图；槽位占用变更后必须先让旧发布行失效，再由清理其他实体关系字段的共用 writer 写入。不得跳过关系清理、吞掉代数错误或按OID420特判。Unity有序关闭阶段顺序保持。

声明脚本路径：`Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs` 的 `ReleaseRuntimeSlot`；`Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs` 的共用占用变更关系清理回归。先加聚焦失败测试，再最小移动现有发布器失效调用至 `ClearReferencesToReleasedSlot` 之前。原 C043 Scene probe只作为端到端复核，不顺手修改DAT/Scene/资源/其它模块。

验收：原Editor新聚焦测试 RED→GREEN、相邻占用变更和关系生命周期测试通过；生成工程与原Editor编译0错；原Scene同输入完成60生产tick并与正式根逐字段配对，退出clean/池借用0/四保护SHA不变；Change Ledger validator及差异检查。若后续出现新首差则记录并重新限定范围，不把旧局部同态当完成。

风险：发布器失效时点影响同 tick 后续消费者；只移动已有调用、不新建缓存或改变战斗状态，聚焦测试及自然生产链核验。回滚仅审查本 ID 两个脚本hunk；不使用 Git restore/删除或覆盖已有用户文件。

验收结果：目标聚焦RED1/1→GREEN1/1，邻近两例2/2；原Scene同输入60tick无异常，正式源17字段raw5个无链接哨兵表示差、限定归一后1020/1020；退出借用0/Scene clean/四SHA稳。未跑整个SelfCheck或物理键，Q07仍开放。[证据](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/REPORT.md)。
