# NTSD28-336B44-Q07-D024-REPEATED-PLATFORM-CARRY-001

状态：`RUNTIME_PENDING / FOCUSED_TEST_PASS`（覆盖创建时静态候选）。原Editor连续RED后共用修正，帧运动整类39/39及本轮新SelfCheck PASS；正式自然非零平台链接、根/原Scene同条件及画面仍待。[本轮报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-REPEATED-PLATFORM-CARRY-001/REPORT.md)。父项为当前 336B44 总表的 Q07/D-024；只处理已有链接平台搬运写者在固定完整背景视口下的重复 X/Z 位移。不得修改 DAT、地图、相机、Scene、非战斗逻辑、平台候选选择、源规则坐标或已批准例外。

## 权威与现状

正式根 EXE SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。对应 playable 闭包内 `source/ntsd28_core/src/simulation/battle_world.cpp::apply_frame_motion` 第 1724–1777 行：链接平台非零 `dvx/dvz` 均由乘客当前规则**整数**坐标重建精确坐标，再原生取整。Unity `LF2Entity.ApplyLinkedPlatformMotion` 已按 D-024 对单次乘客搬运量乘 World X/Z 比例，但物理域每次仍从 `Runtime.XInt/ZInt` 重基；源域同时由 `SourceRuleXInt/ZInt` 重基。现有 `LinkedPlatformCarryUsesViewRatioAndFacing` 只覆盖一次搬运，不能证明连续比例稳定。静态复算示例（源X200/Z250、每步+4/+2、2048×1152视口）预测第 8 次绝对误差首次超过1输出像素，24次约为X 3.347/Z 3.592像素；**尚非正式根/Unity实测首差**。

## 实施边界

第一步仅在 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FrameMotionTailEditorTests.cs` 增加共用平台链的重复搬运测试，覆盖默认/配置视口、左右朝向、X/Z精确源与物理比例；记录原 Editor RED。若 RED 证实该共用写者缺口，只修改 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs::ApplyLinkedPlatformMotion`：源位置已初始化时先按正式整数源基底完成规则更新，再按本步源精确变化量经既有 `BattleSpatialProjection`/World比例累加物理位置；未初始化源位置保留原有路径。旧单次“物理/源锚点故意不同”夹具需按声明的源差与保留锚点更新物理预期，不得改规则预期。

这不是重做 Q06 平台候选、碰撞或 pass 顺序；不与上轮直接帧尾补丁合并。正式 OID/输入自然链接和根 EXE 同态须先取得具体可达条件，不能把合成测试或旧 B1E13 夹具自动晋升为 336B44 证书。

## 验收与回滚

先查旧测试和本轮脚本 diff，再做原 Editor 定向 RED→GREEN、相邻帧尾/平台测试、生成 C# 编译、完整 SelfCheck。确定正式可达条件后，在原 Battle Scene 完整 Driver 做同条件至少一例连续链接搬运、源/物理逐 tick、保护 SHA、有序关闭和根/Unity选定字段对照；无自然可达证据时只报聚焦测试状态，不关闭 Q07/D-024 或整个本 Task。仅对本 Change 声明的增改行制作经审查的反向补丁回滚，保护工作区其它修改和用户文件；不使用破坏性 Git 操作。
