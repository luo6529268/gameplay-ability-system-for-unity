# C045 Recover/Cover 分读限定验收（2026-10-01）

**结论：Unity 共用抓取伤害停顿读取 `recover` 的受控机制已验证；C045 自然 Unity 整链、Q07 和总目标仍开放。** 当前正式 336B44 源码/根 OID65 自然抓取到 action376 的依据见 [自然可达性报告](../NTSD28-336B44-Q07-C045-NATURAL-REACH-001/REPORT.md)。该证据中 action348/X550 在 tick18、action357/X700 在 tick23 造成 100 伤害，抓取者 hold=2、受害者 hold=-3；根三组 120 tick × 22 声明字段为 7920/7920 零差。

Unity 原实现把 `cover` 当作停顿控制。正式 `ank.dat` action376 是 `cover=1`、`recover` 未写（默认 0）；按旧实现，受伤后抓取者 hold=7，而正式源为 2。原 Editor 测试先以 40 例运行：job `f8624bcb8a1c4323b21204ce55297546`，其中 5 例预期 RED、35 例通过；正式 `cover=1/recover=0` 的首差即抓取者 hold 期望 2、实际 7。

修复只改 `BattleCpointWriter.ApplyHeldInjury` 正伤害后的三处条件为 `recover`；`SyncHeldPosition` 仍由 `cover` 决定持有位置。原 Editor 脚本编译 Tundra 成功、0 error；修后 job `9770a3ccb0a546b4990537e53aad4dd8` 同组与邻近抓取测试 40/40 PASS，涵盖九例 Recover/Cover 交叉矩阵。

原项目 `NTSD_Battle.unity` 在原 Editor Play 中运行既有 R8 抓取关系探针，显式设 `cover=1/recover=0`：结果 `PASS`，抓取者 `FrameDelay=2`、受害者 `FrameDelay=-3` 的断言通过；持有位置 `X/Y/Z=116/19/201` 与期望一致，临时对象/slot/两池回到基线，`cleanupCompleted=true`。原始 [Play 结果](original-editor-grab-cover1-recover0-v1.json) SHA-256 `A0D0674F5FBBB13AA21E18A43C066B97A184BED460920F32A1ACFE078A1501A9`。退出后 Editor idle、非 Play、非编译；Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 四个保护 SHA 与运行前一致。

此 Play 是受控临时抓取体和伤害配置，不能替代正式 OID65 在 Unity 原场景中自然入招、抓取、action376 伤害的逐 tick 同态；后者作为 C045 父门继续比较近距与远距控制。未修改 DAT、Scene、配置资产、GAS 或非战斗流程。
