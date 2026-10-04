# NTSD28-336B44-Q10-NARUTO-HELD-JUMP-043-REACH-001

状态：`FOCUSED_TEST_PASS / EXACT_INPUT_043_NEGATIVE / UNITY_VOICE_PENDING`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q10`；关联用户“鸣人持续按方向与跳跃，落地后快速奔跑”输入。此包只判定这一具体输入是否触发正式 `data/043.wav`，不重复已通过的位移比例 Play，不用旧 B1E13 源码定义新规则。

权威/原状：当前根 EXE SHA `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，当前正式 `nar.dat` 与 Unity staged `nar.dat` SHA 同为 `6BE721...ED9`。正式 DAT 的 frame641 `running2` 声明 `data/043.wav`，其显式帧路由有 frame219 `hit_j:640`、frame640 kind8 `dvx:638`、638→639→641；当前源码 `BattleWorld28` kind8 的 `dvx` 写攻击者 action。此路径需要条件性输入/碰撞，静态声明不证明自然声音。`data/043.wav` 在当前正式 decoded DAT 的107文件/174帧中声明，故不能把文件整体归为鸣人专属。

修改前精确代码清单：只新增 `Tools/NTSD28Q10Diagnostics/naruto_held_jump_043_probe.cpp`，提供 mode0/正式DAT 的 `GameSession28` 自然离散输入探针：鸣人OID2/action0，远距中性对手OID7，先方向4tick，之后方向＋跳跃持续若干tick。每 tick 导出动作、帧状态、位移和 `last_tick().audio_events`，写 LFR 供根EXE回放。不得人工设置动作640/641、插入声音、编辑DAT/WAV、Unity生产脚本、Scene、相机、地图、模式或非战斗文件。

验收：用当前 `build.ps1 -Target playable` 的 Core/playable闭包编译退出0；同初态两次诊断 CSV/LFR逐字节一致。若源码自然发出043，同LFR由当前根正式EXE回放PASS并比对公开动作/位置等字段；根trace不含audio，不能声称根设备发声。若80tick范围未发，明确报告条件性阴性，不上升为全局不可达，也不修改资源。保护四文件SHA前后不变；运行 `Tools/Validate-ChangeLedger.ps1`、定向 diff check。回滚为前向修正/标记废弃本新增工具与记录，保留失败原件；不得无记录删除。

2026-10-04 实际结果：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-NARUTO-HELD-JUMP-043-REACH-001/REPORT.md)。最终C++编译0错，双跑CSV/LFR逐字节相同，源码80tick未发043；正式根同LFR进程0/PASS，7项选定字段560/560同态。源码实际发017、`c/nar/w/a7.wav`、012；其中a7两Unity音频根皆缺。只关闭这一输入的043来源判定，a7自然Unity播放、其它043入口与Q10整体仍待。
