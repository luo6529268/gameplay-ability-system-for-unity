# Q07/F02 自有地图共同纵深诊断

状态：`FORMAL_SOURCE_ROOT_PASS / UNITY_SCENE_PENDING`。唯一战斗权威仍是 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的正式根 `NTSD2.8-Logan.exe` 和对应 playable 源码。Unity 保留自己的地图与边界；本包不部署正式背景 DAT，也不改项目背景、角色 DAT 或生产代码。

原正式背景23/Z542 阳性在 Unity Battle Scene 首轮 tick1 被本项目地图限制到源 Z481/482。直接把正式同背景初始 Z 改为400也不可比：正式背景23的 native 边界在 tick1 将三实体限回约 Z542（见 `z400-root-02/root-trace.jsonl`）；该根回放虽 PASS，却不证明 Unity Z400 同条件。Unity 第二轮 Z400 在 Play 启动阶段反复退出、未取得任何 tick，`../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-kind10-scene-20261003-02/00010.json` 超时 `FAIL/DONE`，干净返回 Menu、无 live World、受保护四SHA稳定。

为建立共同战斗纵深，正式诊断会话改选正式背景 ID1，其 `b/San/b.dat` 声明 `zboundary: 375 575`，包含 Z400；这仅是正式会话的背景选择。`f02_pickup_throw_entry_probe.cpp` 增加可选初始 Z 与背景 ID，旧默认 Z542/背景23保持。旧默认正例重新运行，`source-ticks.csv` 与旧证据 SHA 同为 `56C00058E3758F53ACF796538A5913FAB7655089A6612B784D8970D6314A39C8`。新背景1/Z400 源会话 `bg1-z400-source-01/summary.txt` 仍观测落地17、拾取18、轻投24、释放30、kind10 applied 7次、state1000高速31、F02返回39；根 LFR `bg1-z400-root-01/root-report.json` `passed=true/failureCode=0/completedTicks=129`。初态和128个完整 tick 的三槽24字段共 **3096/3096** 零差，见 `bg1-z400-root-01/paired-comparison.json`；正式根 tick0/1 三实体 Z均为400，武器耐久字段均250。

这些结果只建立正式版在共同纵深候选中的对照，不等于 Unity 实际可行走区域、F02 Scene 或画面通过。下一步原 Editor 单一干净 Scene 下重跑 Unity Z400，直接观察第一差异；若地图不允许，则只记录真实地图限制，不能改 DAT/地图来迎合对照。原始编译参数与零错误输出见 `compile-argv-v2.txt`、`compile-output-v2.txt`。首次把正式根路径指向没有背景的 Unity 暂存资源导致初始化失败的原件 `z400-root-01/root-report.json`、源诊断两次资源根失败输出保留；其后正确指向发行 `resources/runtime`。
