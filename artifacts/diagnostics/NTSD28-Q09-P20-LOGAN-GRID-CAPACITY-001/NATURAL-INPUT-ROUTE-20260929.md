# Q09/P-20 飞段 frame430 自然输入路径审计

状态：`SOURCE_ROUTE_IDENTIFIED / ROOT_EXE_AND_UNITY_PLAY_PENDING`。这是只读源码与既有轨迹对账，不是新增运行时验收；不改变 `NTSD28-Q09-P20-LOGAN-GRID-CAPACITY-001 / FOCUSED_TEST_PASS`，Q09、BATCH-05及总目标保持开放。

正式 `resources/runtime/decoded_dat/c/hid/hid.dat` 的 frame212（jump）声明 `hit_Fa: 430`；frame213、214、216、217（dash）也声明相同字段。frame430使用 pic119；首张声明sheet `pic62-126, row5, col11` 的容量为55，pic119的局部索引为57。既有容量修复的原Editor聚焦测试4/4 PASS，只覆盖索引发布门。

正式 playable `InputRouter28::step_sampled` 先 `process_sampled_inputs`、`advance_combos`，再依次 `route_combo_fields` 和 `route_type0_builtins`。`advance_combos` 在防御键边沿武装前向攻击组合，后续水平前向与攻击边沿可使 `combo_state[0] >= 4`；`route_combo_fields` 随后从当前 frame 读取 `hit_Fa`。源码存在进入 frame430 的自然路径，且组合处理先于普通空中攻击分支。实测已将此前“必须在frame212完成组合”的假设更正：组合可在frame211完成并保留至frame212消费。dash 的 state5 另有持攻击直达 action90 的分支，不能未经实测就把 dash 序列也写为430可达。

既有正式配对 Session 轨迹 `NTSD28-Q07-HIDAN-NATURAL-INPUT-REACHABILITY-001/source-action0-attack-jump-x1200-lfr.jsonl` 中，jumpStart9 的 actor 在 tick15 首次到 action212，tick16、17仍为212；那是未加组合时的跳跃入口。后继定向包 `NTSD28-Q09-P20-HIDAN-NATURAL-FRAME430-001` 已在配对 Session 和正式根 EXE 自然到达430，准确输入相位与120项选定字段对照见该包[验收](../NTSD28-Q09-P20-HIDAN-NATURAL-FRAME430-001/ACCEPTANCE.md)。原 Battle Scene 同条件帧与本体像素仍待；不扩跑角色矩阵。

本审计本身未修改脚本、DAT、图片、Scene、配置或非战斗文件，未运行新编译、Unity测试或Play；后继定向包的Tools脚本与正式根回放另行留痕。Q07/D-024碰撞域与本项独立，Q07仍为BATCH-04最早未闭组。
