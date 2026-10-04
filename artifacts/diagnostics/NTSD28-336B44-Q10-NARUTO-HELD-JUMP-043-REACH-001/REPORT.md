# Q10 鸣人持续方向＋跳跃的正式声音事件（2026-10-04）

状态：`CURRENT_SOURCE_EXACT_INPUT_NEGATIVE_043 / CURRENT_ROOT_SELECTED_STATE_PASS / A7_FORMAL_FILE_GAP / UNITY_VOICE_PENDING`。这只裁决下述 80 tick 输入，不说明 `data/043.wav` 在其他招式或角色中不可达；Q10/Q12/总目标开放。没有改 DAT 数值、WAV、图片、Unity 生产/测试脚本、Scene、模式或非战斗功能。

当前正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；正式与 Unity staged `nar.dat` 各 SHA-256 `6BE721524C8CCA0E293BEB8D6BF1DFEE306CCB948181BDAA94545EDB29418ED9`。当前正式 DAT `c/nar/nar.dat` 的 frame641 `running2` 在第3411～3414行声明 `data/043.wav`，其显式条件链为 frame219 的 `hit_j:640`、frame640 的 kind8 `dvx:638`、638→639→641。当前 playable `battle_world.cpp` kind8 消费者在约第5795行把非999的 `dvx` 写入**攻击者**动作；静态帧声明不足以证明玩家落地输入会走该链。正式 decoded DAT 全树的 `data/043.wav` 还有其他 owner，共107文件/174处声明，不能归为鸣人专用音效。

新增[独立诊断源](../../../Tools/NTSD28Q10Diagnostics/naruto_held_jump_043_probe.cpp)，使用当前正式 `resources/runtime`、mode0、OID2鸣人/action0/X800/Z650 与远距OID7/action0/X1200/Z650、seed682973786。tick1～4仅持续向右，tick5～80向右＋跳跃均持续按住；未人工设置动作640/641或插入音效。按当前 playable 编译闭包的28个Core与4个playable实现单元编译，最终 [build-03](build-03/) exit0、stderr0。初版参数名从既有模板沿用 `decoded_dat/vfs`，前两次运行分别因传入错误的VFS根目录、缺catalog.csv退出4，原件 [run-01](run-01/) 与 [run-02](run-02/) 保留；随后澄清两个参数都需正式runtime根，并将导出状态改为当前DAT帧状态，旧编译/运行原件保留。

最终 [run-05](run-05/) 与 [run-06](run-06/) 各80 tick、exit0，CSV与LFR各自逐字节相同，LFR SHA-256 `D1C63A81CCD69E3A2F6D099D1B6815E0C8B1ED0C76AFF92D66708761E958D7E4`。源码未进入219/640/638/639/641，`data/043.wav`音频事件为0。tick9实际记 `data/017.wav` 与 `c/nar/w/a7.wav`，tick32记 `data/012.wav`；tick32～40鸣人动作215→215→0→7→7→7→8→8→8，与此前用户落地持续按键诊断的该局部轨迹一致，但历史报告本身不裁决当前权威。

同一LFR已由当前正式**根EXE**独立[回放](root-03/)：进程exit0，report `passed=true/failureCode=0/declaredTicks=80/completedTicks=81/nativeParityClaim=false`。根tick1～80的鸣人动作、DAT状态、整数X/Y/Z、MP及输入相位，逐项对最终源码CSV **560/560一致、0首差**，见[机械比较](comparison.json)。根公开trace没有audio事件字段；故当前结论是“当前源码此输入未发043，且根可见状态走同一动作链”，不能宣称已直接捕获正式EXE扬声器或Unity voice。

[四cue现存性](cue-status.json)：正式文件均存在；Unity正式暂存四条均不存在。旧Sound有`data/017.wav`、`data/012.wav`、`data/043.wav`，但没有`c/nar/w/a7.wav`。既有PCM清单已证017和012虽整文件字节不同、PCM却同版；本样本未触发043，故不按这个落地输入部署043。`a7.wav`是本样本**源码自然事件且两Unity音频根都缺**的单文件Q10候选，正式SHA `62DABF2DD6EEA06A042D806D3991625CA30822C096C3BE72B723C935ABEA4CBB`，mono/8-bit/22050Hz/3052帧。须在原Editor编译恢复后按单cue战斗路径接入并做自然voice/混音验收；单独内容接入不能关闭Q10。现有已证可达的`data/078.wav`仍按其独立Task优先处理。

四保护文件 Battle、Menu、GameConfig、ProjectBattleModeConfig 在本包前后SHA分别保持 `D88AD211...76CDF6`、`9EAAA0B4...C1BA`、`0527D737...8EA7`、`B57CFEF3...5B82`。没有进入Unity Play；原Editor编译仍需恢复。脚本审计与定向差异检查见Change Record。
