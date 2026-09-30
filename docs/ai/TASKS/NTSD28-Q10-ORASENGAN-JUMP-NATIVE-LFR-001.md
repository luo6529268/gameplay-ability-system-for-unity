# NTSD28-Q10-ORASENGAN-JUMP-NATIVE-LFR-001

> **2026-09-30 当前出口：`FOCUSED_TEST_PASS`（旧 B1E13 限定）。** 旧发布源码树逐 SHA 复原并运行本探针，tick34 action326/MP250/`data/078.wav`一次；同 LFR 经旧版同 SHA 归档 EXE 回放，tick30～38动作/MP 9/9同。旧EXE自身不导出音频，Unity物理输入/clip/听感、版本G0决定及Q10整组仍待。[验收](../../../artifacts/diagnostics/NTSD28-Q10-ORASENGAN-JUMP-NATIVE-LFR-001/ACCEPTANCE-20260930.md)。下方BLOCKED是先前无旧版隔离入口的快照。

> **2026-09-30 后续：** 使用逐哈希同 B1E13 旧权威的归档 EXE 隔离回放现有LFR，报告PASS，tick30～38 action/MP 与当前源码 9/9 同；旧EXE trace无 audio 字段，故旧版 `078.wav` 消费仍待。G0根版本选择、Unity物理键和clip/听感不因本次局部证据关闭。[验收](../../../artifacts/diagnostics/NTSD28-Q10-ORASENGAN-JUMP-NATIVE-LFR-001/ACCEPTANCE-20260930.md)。

状态：`BLOCKED_FORMAL_ROOT_IDENTITY / LOCAL_SOURCE_FOCUSED_PASS`。父项：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q10 / O-05 / R17`。原计划依据 B1E13 正式根身份；当前根已变为 336B44，故当前源码诊断结果不能晋升为原权威证书。Naruto DAT 的后续 Jump 静态关系与现有证据见[资源身份审计](../../../artifacts/diagnostics/NTSD28-Q10-NARUTO-ORASENGAN-CUE-IDENTITY-20260929/REPORT.md)；本包结果见[限定验收](../../../artifacts/diagnostics/NTSD28-Q10-ORASENGAN-JUMP-NATIVE-LFR-001/ACCEPTANCE-20260930.md)。

仅新增 `Tools/NTSD28Q10Diagnostics/orasengan_jump_lfr_probe.cpp`。复用已证 `NTSD28-Q07-RASENGAN-NATURAL-FORMAL-PLAYBACK-001` 的双角色、背景23、seed682973786、固定 BGM2、tick1～6 防/右/跳及 tick7 起中性输入；在第一253后的 tick34～35 再输入 Jump，记录每 tick 的 action/MP/输入相位、正式音频事件源/路径与 LFR。新目录独立输出，拒绝覆盖已有文件。编译只用正式 playable 清单内的 core/Session/Selection/LFR 源与这个新 Tools 入口，绝不回写正式目录或覆盖根 EXE。

出口：源码完整 Session 实际进入 325/326 且报告 `data\078.wav` 事件，根正式 EXE 对同 LFR headless 回放 `passed=true`，比较其可导出的动作/MP/输入相位；若根 trace 不含声音事件，明确只报告配对源码事件与根动作证书。若输入时点不能触发，保存 FAIL 和完整首差，再决定更窄输入窗口，不能硬设动作。该包只证明正式侧自然离散输入，不证明 Unity 物理键、实际 clip、设备听感或 Q10 整体。

保护：DAT、WAV、Unity 脚本/Scene、ProjectSettings、非战斗功能和用户脏文件均不改；受保护文件哈希、编译、运行、ChangeLedger 与 diff 检查如实记录。删除新诊断和输出仍须遵守仓库删除授权；回滚不能使用 restore/reset/clean。
