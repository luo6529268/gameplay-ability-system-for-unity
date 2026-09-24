# Q09 P-04 同 Z 绘制顺序阶段验收（2026-09-24）

状态：`FOCUSED_TEST_PASS / RUNTIME_PENDING`。正式根 EXE SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。playable `render_snapshot.cpp` 的最终 `entity_commands` 以 Z 升序、同 Z 物理 slot 降序绘制，`d3d11_renderer.cpp` 按该命令顺序消费。独立 sprite/spark 源数组的 slot 升序不是最终 painter 流。

Unity 改动只影响战斗表现排序：Stage Legacy 比较器、Coordinator 比较回退及中央 radix 的同 Z 索引段，均让较大 slot 先绘制；冻结实体行仍保持物理 slot 顺序。测试将快照存储顺序与 painter 顺序分开断言，SelfCheck 中旧升序期望同步更正。未改 DAT、图片、Scene、相机、逻辑 tick、位移或非战斗模块。

验证：原项目 Unity Editor 刷新后 Console 编译错误 0；定向 `BattlePresentationBeginFrameReuseEditorTests` 的 EditMode job `f65c869d9f4044269117e7e35458c17a` 为 14/14 PASS。前一轮同类 job `a8c8e94645f04003a82ba1e190127179` 为 11/14，3 失败系测试把冻结快照存储顺序误作 painter 顺序，随后修正测试语义并通过。最新 `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 19:57:29 为 PASS；此前两次分别暴露紧凑排序和 Legacy SpriteRenderer 的旧升序 oracle，均已在测试文件中按正式规则更正。

原 Battle Scene R07B Play 探针曾进入 tick 5，至手动退出的 tick 272 未形成夹具基线或同 Z 观察；退出结果为 `FAIL: Play Mode or the production world ended before completion`，`sameZPainterSlotOrderStable=false` 不能作为 P-04 行为失败证据。探针仍依赖旧 DAT 中可用于 pending/free 的 `hit_Fa` 非角色帧；本轮正式内容下该前置未建立。还需建立适用于当前正式内容的独立 Play 同 Z 遮挡/像素见证，再做正式 EXE 同场景对照。Q09、R16 与总目标仍开放。

原 Battle Scene 磁盘 SHA-256 在 Play 前后均为 `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39`。Menu Scene 原有并行变化保留，本包未改。

误触发的未筛选 EditMode job `0428a4ab0f644890b3d40d329861bfe8` 到 6774/8500 提前失败，含大量其他领域旧测试失败，不作为本包通过或失败结论；它已结束，后续只使用正确 `groupNames` 定向筛选。
