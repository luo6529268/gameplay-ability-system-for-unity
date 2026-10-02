# Q10/C053 正式战斗声音声像首差与相机坐标诊断

状态：`VERIFIED / SOURCE_CAMERA_SCOPED_PASS / UNITY_STEREO_FIRST_DIFFERENCE_STATIC`。已取得正式相机X与10事件左右矩阵；Unity生产未改、设备输出未验。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-STEREO-CAMERA-AUDIT-001/REPORT.md)。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，新版 336B44 总表 BATCH-05/Q10。正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable live path 定义战斗规则；D-024 固定完整背景、项目背景/模式 Asset 和 DAT 原值边界均不变。

先验首差：正式 `main.cpp` 在每次战斗 tick 后以 `session.camera_x()` 设声像布局，`audio_backend.cpp::native_stereo_mix28` 用事件源规则 `world_x` 与布局 `{cameraX,333,666,-333}` 输出左右百分比，XAudio2 按该矩阵提交。当前 Unity `NTSDSoundPlayer.PresentSound` 虽消费同版 C053 已证10事件的源规则 X，正式单文件 cue 通过 fallback AudioItem 的 `range=0` 走2D `spatialBlend=0`、`panStereo=0`，没有读相机 X 或写左右矩阵；因此不能把已证事件/文件/播放入口称作声像对齐。自然受控初态和事件见 [C053 事件报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001/REPORT.md)。

精确写范围只限 `Tools/NTSD28Q10Diagnostics/c053_natural_double_audio_probe.cpp`：新增编译期开关 `NTSD_Q10_STEREO_CAMERA_AUDIT`，仅开启时在原18行/12tick CSV 后追加每tick正式 `GameSession28::camera_x()` 一列。默认编译输出 schema 和既有事件路径/X/战斗字段不变。新的编译产物、CSV、命令、摘要与报告只写入独立 `artifacts/diagnostics/NTSD28-336B44-Q10-C053-STEREO-CAMERA-AUDIT-001/`；原C053诊断EXE/CSV及正式源、Unity生产脚本、DAT/图、Scene/配置不改。

验收：当前正式EXE SHA再核；使用既有正式playable 28 Core+4 host 源文件闭包的编译参数、单独输出路径，编译 0 error；运行两次新输出不覆盖旧文件，逐 SHA 相同；新列严格对应12次 `session.step()` 后 cameraX，剥掉新列的旧字段/18行与原报告逐字节同；对10条事件按正式函数整数分支计算左右百分比并标明它只是当前源码计算值，不冒充实际设备波形。阅读Unity默认 AudioItem/voice 代码并标出实测与推断；最终写首差报告、新版总表状态及 Ledger/STATE/handoff，运行 `Tools/Validate-ChangeLedger.ps1` 和 `git diff --check`。

风险：若诊断代码改变旧默认 CSV 或同场景 tick，就停在失败证据，不据此改生产声像。之后如需修生产，单独建 Task/Change，先确定固定完整背景例外下的音频可见比例与 Unity 左右声道实现/验收；本 Task 不绕过 D-024 或改非战斗声音。

