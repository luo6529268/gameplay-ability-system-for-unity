# Q07 鸣人螺旋丸后续 Attack：当前336B44原场景窗口复验

**2026-10-04 最新状态：`RUNTIME_PENDING / FIRST_WINDOW_SCOPED_PASS / OTHER_TWO_WINDOWS_DEFERRED`。** 下方`UNITY_IMPORT_PENDING`是导入完成前的历史快照；末尾补证覆盖它。

状态：`CODE_WRITTEN / FIRST_WINDOW_SCOPED_PASS / UNITY_IMPORT_PENDING`。当前正式根 EXE 为 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。前置[正式源码/根报告](../NTSD28-336B44-Q07-RASENGAN-ATTACK-WINDOW-RECHECK-20261004/REPORT.md)已证三条自然防→前→跳→后续 Attack 55 tick，动作、MP、2tu相位与攻击采样等990/990同。当前包只推进原 Unity Battle Scene 的同版验证；Q07/R18/Q12及总目标开放。

原 Editor 单一 `NTSD_Battle` Scene在非Play、clean时启动。初次Play后诊断启动过晚：鸣人分别已到动作6/PP400、动作487/PP105，探针拒绝，未将其当成生产FAIL。退出后场景磁盘SHA从最初观察值`91F8C274...`变为`CAFF95AD...`，同期存在其它Battle UI工作；写入者未证，保留内容、不回退。第二次Play监测`BattleTestBootstrap`完成日志并立即运行既有自然物理设备探针，首窗口有效：初始tick2/PP500，Unity首253=tick35，合成J在tick36/输入相位0进入`FrameInputSet`及native proxy，并同tick转动作301/PP250。[原始PASS](first253-original-editor-pass.json) SHA-256 `3DFDCD0E8E04166FE2FE6EFB5D6472288961624F47A2F21A945DAF2F79779945`；以初始tick归零，当前正式源码CSV与原Unity动作、MP、相位、combo1共34tick×4字段=**136/136**无首差，[机械配对](first253-current-source-unity-paired.json)。这是合成键盘设备事件通过原InputAction/生产Driver的逻辑证据，不是真人人手键或屏幕提示时差证据。

同轮之后鸣人仍在后续动作/PP99，第二窗口探针按前置拒绝。第三次干净Play中，Editor已失焦；第二253探针排入的L键在30tick均未进`FrameInputSet`，八次脉冲耗尽，[原始FAIL](second253-editor-unfocused-input-fail.json) SHA-256 `5EE9097C1ADB659A3204E5C459B2A3FBFE66A6121EB42FC34A04A3845D02FECC`。这不构成正式规则与Unity生产首差。随后退出Play，单一Battle Scene报告clean，Battle磁盘SHA为`CAFF95AD...`，未执行Scene保存或回退。

为排除后台Editor不消费合成设备事件，已在[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q07-RASENGAN-ATTACK-CURRENT-SCENE-001.md)和[Change Record](../../../docs/ai/CHANGE-RECORDS/NTSD28-336B44-Q07-RASENGAN-ATTACK-CURRENT-SCENE-001.md)登记后，仅在既有Editor诊断的`NaturalProbe.Queue`排键后调用`InputSystem.Update()`。生产脚本、DAT数值、图、Scene、Prefab及非战斗未改。`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` exit0/0 error/332 warning；`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` exit0，`git diff --check`尚待本包末次复核。原Editor `refresh_unity`已返回refresh/compile requested，但其`Library/ScriptAssemblies/Assembly-CSharp-Editor.dll`仍停在2026-10-04 08:09:56Z，早于诊断改动08:39:04Z；MCP状态读取连续超时，尚未取得原Editor编译或新代码Play。因此**新诊断脚本不能报告已运行通过**。

下一步仅在原Editor导入新程序集且Battle Scene clean/非Play后，用同一出生起点复验首253、次253、254后三菜单；逐tick核对正式根当前输入相位、动作/PP与采样边界，再处理屏幕可见提示和真实设备时点。若新显式InputSystem更新改变键到达相位，保留结果并先判断诊断契约，不据此修改DAT或加鸣人特例。

## 2026-10-04 原Editor导入后补证与停止线

原Editor程序集于08:46:15Z更新，晚于诊断脚本08:39:04Z；MCP恢复idle，Battle Scene clean。新探针首253在独立Play中再次PASS：初始tick4，首253=tick37、攻击FrameInputSet/native采样=38/phase0、action301=38。[新原件](first253-current-probe-pass.json) SHA-256 `2789DAA2AE2CDA8F1946D773591F514C7A7E0B70FB08B2F792900968349D133C`。严格同初态比较发现第一相对tick源码action0、Unity action1；这是进入探针前Unity已走到站立循环的起始帧差，[原始严格配对](first253-current-probe-paired.json)保留135/136和首差，不伪称全链零差。从防御消费的相对tick2起，动作/PP/相位/combo1至转换tick34共132/132同，[分界配对](first253-current-probe-paired-v2.json)。退出Play后单一Battle Scene clean、磁盘SHA仍CAFF95AD…。

再启第二253独立Play时，监测器对大批日志截断过早，错过实际已写入的bootstrap完成标记；160秒后未触发菜单，故没有新Unity第二窗口结果。已退出Play，Battle Scene clean且SHA仍CAFF95AD…。这次是编排器失败，不是生产FAIL，也不重复启动更多昂贵Play来制造样本量。

按2026-10-04总表工作量停止线，本包暂收于首窗口限定证据。当前336B44正式源/根三个时点仍有990/990同；旧B1E13 Unity另外两个时点仅历史线索，新版Unity次253和254后仍待。没有当前生产首差，不改DAT或加鸣人特例。后续只有出现实际可见提示/真实按键首差，或进入Q12冻结版本最终矩阵时才复验这两个边界；届时须使用稳健bootstrap标记监测并保证同初态。新测试侧InputSystem更新已写且单窗口原Editor运行通过，不能解释为玩家设备实测。
