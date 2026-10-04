<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-NARUTO-HELD-JUMP-043-REACH-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Tools/NTSD28Q10Diagnostics/naruto_held_jump_043_probe.cpp
authority: 336B44 formal playable GameSession28 and current nar.dat frame219/640/638/639/641
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-NARUTO-HELD-JUMP-043-REACH-001.md
-->

# NTSD28-336B44-Q10-NARUTO-HELD-JUMP-043-REACH-001

脚本改动前记录。Unity/正式当前原状：`data/043.wav` 是正式鸣人frame641的静态声明，正式/Unity旧PCM不同、Unity正式WAV未接入；当前还没有336B44正式根与该用户输入配对的自然043事件，不能据此部署。唯一拟新增路径与符号是Task中声明的独立Tools C++诊断入口 `wmain`、`record_case`；不会改生产、DAT、音频、图片或Scene。预期副作用只有新建独立诊断CSV/LFR/报告。风险是具体输入前置与输入相位可能令80tick阴性，阴性不表示所有条件不可达。验收、四保护文件与前向回滚见Task；没有文件删除授权。

实际代码：新增唯一声明脚本 `Tools/NTSD28Q10Diagnostics/naruto_held_jump_043_probe.cpp` 的 `record_case`/`wmain`；用当前正式 GameSession 80tick自然输入导出CSV/LFR和声音事件，不写生产、DAT、WAV或Scene。最初沿用误导性的`decoded_dat/vfs`参数名，run-01/run-02分别因VFS根、catalog根传错退出4；原件保留。改成明确的`runtime_root/complete_vfs_root`，又把初版错误的 `frame_state_code` 诊断镜像改为读取实际帧的 DAT `state`；前版build-01/02与run-03/04保留，最终build-03及run-05/06为有效结果。

验证：当前Core28+playable4重编 build-03 exit0/stderr0；最终两次80tick CSV/LFR分别逐字节相同，源码043事件0、其它声音在tick9/32共3条。正式根EXE同LFR进程exit0、`passed=true/failureCode0`；源码/根鸣人动作、DAT状态、X/Y/Z、MP、输入相位560/560一致。根trace无音频字段，原Unity Editor停编，真实voice/设备未验；不能将本样本阴性推广至107个DAT/174处043声明。`c/nar/w/a7.wav`源码自然出现但Unity正式及旧Sound都缺，形成下一Q10资源候选。四保护文件SHA前后稳。详细原件、SHA及下游界限见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-NARUTO-HELD-JUMP-043-REACH-001/REPORT.md)。
