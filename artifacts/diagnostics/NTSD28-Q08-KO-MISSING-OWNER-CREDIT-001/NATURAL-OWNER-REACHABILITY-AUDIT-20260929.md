# Q08 KO 失效 owner 自然可达性限定审计（2026-09-29）

状态：`READ_ONLY_SELECTED_CORPUS_CLASSIFICATION / NATURAL_ORPHAN_NOT_PROVEN`。此项只决定是否需要立刻补一个普通对战自然 Play 案例；不升级既有 `FOCUSED_TEST_PASS`，不关闭 Q08 或总目标。

当前正式根 `NTSD2.8-Logan.exe` SHA-256 复核为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。正式 `source/README_SOURCE.md` 指向 playable 构建；`battle_world.cpp:1439–1475` 的标准命中归属在 owner 查找失败时保留最后有效 credit，`BattleWorld28::despawn` (`:1511–1553`) 清除部分 link/catch，却不普遍清除存活实体的 `owner_slot`。此前 Unity 聚焦测试已覆盖该规则，详同目录 `ACCEPTANCE.md`。

本轮只读扫描正式 `resources/runtime/catalog.csv` 的 330 个 `object` 行及其全部 330 个现存 `decoded_dat` 文件，按行首 `itr: kind: 2`、`opoint: kind: 2` 分类：

| 声明 | 命中的对象定义 | 所在定义的类型 |
| --- | ---: | --- |
| ITR kind 2 | 148 | type 0：148 |
| OPoint kind 2 | 104 | type 0：102；type 3：2 |

这只是声明级扫描，不等于 148/104 个动作都在当前默认对战可达。正式 `game_session.cpp:2179` 将普通参战者 `owner_slot` 初始化为自身槽；`battle_world.cpp:7615–7621` 与 `object_spawning.cpp:147` 使 OPoint 子体继承上游 owner，而不是无条件改成中间发射体槽。ITR kind 2 可在 `battle_world.cpp:5373` 将目标 owner 写为攻击者的物理槽，但上述当前目录中携带该 ITR 的定义全为 type 0。由此不能仅凭“despawn 不清 owner”推定普通 mode 0 自然战斗中存在一个可致死的失效 owner 链，也不应为该未证前提反复跑全角色 Play。

边界：这不证明失效 owner 永远不可达。宿主剧情阶段会在 `game_session.cpp:2049` 清除部分槽，同时保留选定实体；动态状态转移、OPoint kind 2 的两个 type 3 定义、非默认模式、额外 DAT 或真实输入可能形成其它路径，本轮没有构造或运行这些条件，也没有同世界正式根/Unity 对照。若出现自然 orphan 命中、owner 生命周期规则变动或 Q12 整场验收触发，按具体输入和槽序再做一个成对 Play；在此之前保留已有聚焦规则证据并推进其它 Q08 正式可达出口。本轮未改脚本、DAT、Scene、资源或非战斗功能，未启动 Editor/EXE 回放。
