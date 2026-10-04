# Q09 正式根窗口 tick 14 画面基准（2026-10-05）

## 结论

已从用户指定的正式 `NTSD2.8-Logan.exe` 真实可见 D3D11 窗口抓取 1280×720 客户区 PNG。双方静置、F5 关闭、渲染上限 R30；暂停后两次窗口标题均为 `tick 14`、P1/P2 `HP 500 MP 200 A 3`，且均含 `PAUSED`。截图在第二次稳定读数之后取得。此证据提供了正式版的一个确定 tick 画面基准，**没有 Unity 同初态截图配对，也没有玩家输入或伤害过程，不能据此判定 Q09 或战斗总目标已对齐**。原版背景、固定相机与普通 HUD 仍按用户已确认的例外处理，不作为逐像素相等条件。

## 身份、步骤与原件

- 正式根目录 EXE SHA-256：`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`（本轮重算）。正式 `resources/runtime` 为资源根；P1 OID 2 鸣人，P2 OID 9 鼬；P1 X500/Z400，P2 X620/Z400；P2 设为 human；`--render-fps 30`。R30 避开 `main.cpp` 只在 `render_fps > 30` 采用的表现插值分支。
- 通过 Win32 向该正式窗口发 F1 暂停消息，等待标题刷新；先后两次独立读取均为 tick 14、`PAUSED`。`main.cpp::update_title()` 从 World sequence、实体 HP/MP/action 和 host paused 状态组装标题；它的 FPS、TPS、渲染耗时字段会随 Present 刷新。原始 `capture-observation.json` 中 `stablePause:false` 只说明**整串标题不相等**，不能解释为 tick 不稳定。本报告保留原件不覆盖，按 `tick` 和 `PAUSED` 字段重新判读。
- [正式根窗口 PNG](formal-paused-client.png) SHA-256：`96DA6CF6E9EC4675984D981EF9108A0B4E3A5073D29C0CB02F2AC69794DC3450`。`PrintWindow` 返回 true，PNG 存在且已人工查看，画面有两名角色及正式版背景/栏位。抓取方法不修改 Unity 工程或正式资源。
- [窗口及进程观察](capture-observation.json)：进程已退出，正式窗口可见，暂停消息已送达。两次暂停标题的 FPS/TPS 数值不同，但 tick、P1/P2 战斗摘要和 `PAUSED` 一致。
- [正式 GUI 报告](formal-gui-report.json)：窗口可见、228 次成功 Present、14 个 simulation tick、`runtimeContractPassed:true`。GUI 总验收 `passed:false`、退出码 38、`failureStage:manual_input`，因为本次只发宿主 F1，未给 P1/P2 输入；不能将其表述为 GUI 全验收通过。

前两次尝试目录保留：首轮启动参数中的空格未正确引号导致参数解析失败；v2 使用隐藏窗口，正式 GUI 可见性门失败。这两份失败不构成战斗规则差异，也不覆盖 v3 的原件。

## 后续裁决门

若要比较战斗逻辑，先固定双方内容、位置、模式、随机表/随机调用点、输入边沿和逻辑 tick，再比较正式源/根与 Unity 的字段及首次不同 tick。画面比较需在同态逻辑状态下读取 Unity 原 Battle Scene 的 Game View，并仅裁决非用户例外的角色、阴影、挂点等表现。当前这张正式版截图只是其中一端的基准；不因缺少同态 Unity 截图自动安排全角色或全场景重跑。
