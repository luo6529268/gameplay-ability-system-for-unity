# C048 首 BDY 提前返回前防御累计：原 Battle Scene 定向结果

状态：`CONTROLLED_PLAY_PASS / FORMAL_NATURAL_PENDING`。当前唯一规则权威仍为正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 与对应 playable 源。正式 `data/data.txt` 将 OID301 指向 `s/1/1.dat`；正式与 Unity 暂存文件 SHA-256 同为 `FB2517651A071550E23B53D667321F4273175DC56CC9CC8E909ECA3540C8FF6D`。frame29 的首 BDY 是 kind1033，默认 respond0。

原 Editor PID11944 在原 `Assets/NTSD/Scene/NTSD_Battle.unity` 真实 Play 完成正式内容预热后，运行新增的 C048 专用单对探针。它只注册一个攻击者和一个 OID301/frame29 目标，生产 World 收集到 **1 个**候选，再执行 `PostInteractionTickAll`。结果为 `PASS`：目标动作33、队伍1、攻击者/目标停顿3/-3、目标帧等待77、HP100、vrest0、**Bdefend45**。探针 tick794 执行，独立原始结果在 [c048-dedicated-play-pass.json](c048-dedicated-play-pass.json)。

清理 `cleanupCompleted=true`，对象数4→4、占用槽2→2、Renderer池2→2、逻辑池2→2；全局统计、RNG、待播音效、rest 和命中计划模式均恢复。随后退出 Play，原 Editor 状态为 idle/非 Play，活动 Battle Scene `isDirty=false`。Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 最终 SHA-256 分别为 `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`、`DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，与前值相同。

验证途中两个未构成战斗结论的失败均保留：旧 R8 夹具 OID300/frame30 不再携带 kind1033，见 [原夹具失败](fixture-oid300-failure.json)；修为 OID301 后，旧全矩阵在到达 C048 断言前被另一项击倒统计断言中止，见 [无关矩阵失败](full-matrix-unrelated-stat-failure.json)。为消除该遮挡，仅在既有测试脚本内增 C048 专用入口和结果文件；原 R8 全矩阵入口保留，生产战斗写入没有再次修改。生成 Editor C# 工程 0 错误/232 警告，原 Editor 完成导入与真实 Play。

本证据只证明受控单对在原战斗场景中使用生产消费路径时首 BDY 提前返回保留 Bdefend45 及列出的结果。正式 OID301 自然玩家战斗可达链、正式根同初态逐 tick 与其它 C048 入口仍待；C048、Q07 和总目标不据此关闭。

2026-10-01 正式内容只读回访：全 `resources/runtime/decoded_dat` 仅 `s/1/1.dat` 含 `bdy kind:1033`。该 DAT 的 frame29 为首个已测帧；同文件未见 `next:29` 或 `hit_*:29`，全 decoded DAT 未见同时声明 `oid:301` 与 `action:29` 的 OPoint。已观察到的 OID301 OPoint 使用 action193/195/208 等其它入口。因此现有证据**不能证明** frame29 可经普通 OPoint/帧转移自然到达；也不能排除 stage、战斗启动或其它动态入口。后续先找到正式 release live path 的真实入口，再做自然根/Unity 对照；若该帧在当前内容中不可达，保留受控机制验收并将自然出口标条件待触发，不为追求案例而改 DAT 或选非战斗场景。

2026-10-01 **前驱更正（覆盖上段“未见普通帧转移入口”的不完整判断；只读源码/内容证据）**：正式 `s/1/1.dat:33-34` 的 frame30 有 `wait:0 next:-29`；当前 playable 闭包 `frame_machine.cpp` 将负 `next` 取绝对值、翻转朝向，因此该帧推进到 frame29。正式 `data/stage.dat:9-12` 的 mission 1 指向 `s/1/stage1.dat`；其 `Stage_1-3`（child id3）第三个 phase `bound:1328` 在 `stage1.dat:160` 有 `id:301 act:30 x:1100 #pig`。当前 playable `game_session.cpp` 的 `load_native_story_missions28` 读取正式 stage 父/子表，`begin_native_story_phase` 遍历 phase 行，`spawn_native_story_instance` 把 `row.action_108` 写入 `SpawnRequest28.initial_action` 并调用 `world.spawn_at`。因此 **在正式剧情走到这条 phase 且生成成功的条件下**，存在 OID301/action30→29 的源码与 DAT 前驱链。三份正式 decoded DAT SHA-256 分别为 `5783EC9878E4ED2445ED4EB5B84E38C9131FBF3512969B6F59C4FE7478EE0B6E`（父表）、`B06B20A5C38480C264D305E88DB67D259872061F9A3F8120C65505E97D4C93A7`（子表）、`FB2517651A071550E23B53D667321F4273175DC56CC9CC8E909ECA3540C8FF6D`（OID301定义）。

这不是正式 EXE 剧情实际走到该 phase、frame29 被命中、或 Unity 剧情 Play 的证书。本轮未运行正式根/Unity；Unity 生产 `Assets/NTSD/Content/LoganRuntime/decoded_dat` 中 `s/1/1.dat` 在位且同 SHA，但 `data/stage.dat` 和 `s/1/stage1.dat` 未部署，符合用户“默认 stage.dat 部署暂缓”的边界。C048 改记 `STORY_CONTENT_DEFINED_PREDECESSOR / STORY_RUNTIME_PENDING / USER_STAGE_ASSET_HOLD`；普通战斗仍无已证自然前驱。不得为补 C048 自行部署/修改 stage DAT，也不把剧情条件证据当作 C048 整体关闭。下一转不依赖暂缓 stage 内容的正式可达首差。
