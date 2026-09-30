# Q09/P-20 飞段 frame430 正式自然输入限定验收

状态：`VERIFIED_SCOPED_FORMAL_NATURAL_INPUT / UNITY_PLAY_AND_PIXEL_PENDING`。Change `NTSD28-Q09-P20-HIDAN-NATURAL-FRAME430-001` 只验证正式版输入可达性；Q09/P-20、BATCH-05及总目标仍开放，Q07/D-024碰撞域独立待决定。

正式根 `NTSD2.8-Logan.exe` 新鲜 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。唯一新增Tools诊断按正式配对 `GameSession28::step` 从飞段OID24/action0、actor X500/target X1200、Stage23/mode0、seed `0x28A55A5A` 运行；输入是攻击tick1–2、跳跃tick9–10、防御+前+攻击tick13–14，其余中立。首个组合时间候选tick13已自然命中，无须扩跑后续候选。正式配对源码tick14仍为action211、`comboFa=4`；tick15到212；tick16消费已武装组合到action430/pic119，PP300→150；tick17到431。注意不是必须在frame212才按下全部三个键。

同一Session产生[30行逐tick证据](formal-paired-natural-frame430-02.jsonl)与[LFR](formal-paired-natural-frame430-02.lfr)。正式根EXE无初始action/facing/MP override回放[报告](formal-root-frame430-report.json)为`passed:true`、failureCode0、declaredTicks30、trace写入成功；[根轨迹](formal-root-frame430-trace.jsonl)tick16同为action430/pic119/PP150。源码与根逐tick选定actor action/MP、输入相位、combo0计30×4=120项，差异0，独立[比较结果](formal-paired-vs-root-selected-compare.json)。报告中的`nativeParityClaim:false`保留；LFR PASS单独不作全态同相位证书。

构建只读正式source的28个core和4个playable翻译单元，加新Tools主程序，以g++ C++17编译到本包目录，exit0。首次运行把`complete_vfs_root`误给`resources/runtime/vfs`，初始化报`decoded_dat`目录不存在、[首份输出](formal-paired-natural-frame430.jsonl)为0行；未覆盖，改用`resources/runtime`的新输出运行exit0。正式根CLI调用最初异步返回时报告尚未落盘；后续新鲜读取确认报告/轨迹均已写成，不把早期“文件不存在”当作失败结论。

此处只证明从正式普通输入能自然到430及所选状态字段一致，尚无正式根同帧GPU本体像素、原Unity逻辑/Play/本体画面或同视口直接像素对照。下一门只用此精确序列验证原Battle，不重复全角色矩阵。未改正式source、DAT、图片、Scene、Unity生产或非战斗代码；旧诊断不覆盖。
