# Q12 退出后重进：首次尝试与安全停点（2026-10-04）

状态：`INCOMPARABLE_INPUT_ABSENT / RETRY_BLOCKED_DIRTY_SCENE`。本轮只使用原 Unity Editor、原项目 `NTSD_Battle` 和既有鸣人自然首253合成物理键探针；没有编辑生产脚本、DAT、图片、音频或 Scene。正式规则权威仍是根目录 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的 EXE 与对应 playable live source。

进入第一轮 Play 前，原 Editor 非 Play、idle、无编译/测试，唯一 Battle Scene `isDirty=false`。Battle/Menu/GameConfig/ProjectBattleModeConfig 的磁盘 SHA-256 分别为 `26432662102CC83F516D16260E040547C322BAA66A13F590011B8BE9EC6C995F`、`9EAAA0B4782974D74A017C367C9D5C77326C31D4281A2D820D1CBBA76986C1BA`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。原 BattleTestBootstrap 完成后执行现有首253菜单，原件为 [失败 JSON](reentry-attempt-01-input-absent.json)，SHA-256 `91F34AF9E175C71F7EFE51C4B2704C8F362EA76701DBBB136A82136536B10C3E`。

本次探针从 tick1533 开始记录，排队事件包含 `L`，但所有30行 `FrameInputSet` held/pressed/released 均为0、正式 proxy 当前字节均为0；combo1始终0，八次有限脉冲后以 `Physical combo step exhausted eight input pulses` 结束。此前同探针当前输入修正后的通过原件从首行即有 held/pressed=16，tick880 防御消费、tick911 首253、tick912 转301。菜单调用前两次只读 Editor 状态均显示 `is_playing=true` **且** `is_changing=true / playmode_transition`；虽已看到 BattleTestBootstrap 完成日志，本轮没有等到 Editor 的切换状态归零。这是本轮探针入口前置与前次通过时不同的已观察条件，可能解释合成设备未被正常采样，但不能从现有记录证明因果。两次的首差在物理事件进入战斗输入包之前；本轮不能作为正式规则或重进失败证书，物理事件丢失的具体原因未知。没有继续盲目加脉冲或修改角色逻辑。

停止 Play 后，原 Editor 返回非 Play/idle，第一次只读 Scene 查询为 `isDirty=false`，四保护磁盘 SHA 均与前值相同。准备唯一一次有界重试时，安全前置断言拒绝启动，因为 Battle Scene 已变 `isDirty=true`；后续只读磁盘 SHA 变为 `876C025831BF0B45611AECDB426E5EF6AF09E84F28603846ED0272E19108A421`，其它三文件保持前值。变化的写入者、时点和意图未证；本任务没有保存、回退或覆盖当前 Scene。第二轮 Play **没有启动**，故 Q12 同冻结版退出后重进仍 `PENDING`。

恢复条件：等待当前 Battle Scene 编辑完成并保存、原 Editor 再次非 Play/idle/无编译测试，重新冻结 Scene/程序集/配置身份；进入Play后必须等 `is_changing=false` 且bootstrap完成，再用现有探针确认合成物理 L 实际进入 `FrameInputSet`，然后做一次退出后重进。若仍全0，只定位共用设备→Action→Provider入口，不扩大到角色、音频或全部案例。

## 后续更正：Scene保存后仍全0，失焦策略假设被证伪

用户随后确认Battle Scene已保存且空闲。原Editor核对非Play/idle/无编译测试、唯一Battle Scene clean；新Battle磁盘SHA为`876C025831BF0B45611AECDB426E5EF6AF09E84F28603846ED0272E19108A421`，其余三保护SHA仍同前。再次原Scene Play，在bootstrap完成后调用相同菜单，[第二失败原件](reentry-attempt-01b-saved-scene-still-zero.json) SHA`82486343DA24740C983842C5AE835C7FA9777FCA5B816FEC522815322A725C22`：30tick输入包全0、八次L脉冲失败。调用前MCP `is_changing`始终报告true，即使bootstrap完成且已等待；不能以这个标志单独判断采样可用。该轮停止后非Play，四文件SHA仍稳。

再按独立[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q12-UNFOCUSED-PHYSICAL-PROBE-001.md)短暂试验Editor诊断脚本：选择P1 DefendAction绑定键盘L、临时设`IgnoreFocus`与Editor Game View设备输入策略并在结果前恢复。生成Editor工程0错、原Editor已导入新版程序集；Editor只读状态为未聚焦。第三轮[失败原件](reentry-attempt-02-focus-policy-still-zero.json) SHA`3DC0755B402A64925456BD88321A78C37E9C206B63DD961450E359DCC69F883F`，记录`keyboardDeviceId=1`、`focusPolicyAdjusted=true`、`focusPolicyRestored=true`，但30tick仍全0。这证伪了“仅靠失焦策略/绑定键盘即可恢复此探针”的假设。试验脚本已精确撤回、相对HEAD无diff，Change标`ROLLED_BACK`，失败原件保留；原Editor曾导入过试验程序集，磁盘脚本回退后需刷新方可继续运行。没有修改生产输入、DAT或战斗Scene。

当前结论：三份新样本均在设备事件进入`FrameInputSet`前不可比，不证明战斗规则退化，也不满足同冻结版重进验收；相较于已通过原件，故障具有Editor/输入设备或探针链的条件性。下一步只定位事件被InputSystem处理后，P1 `DefendAction`是否触发及`CharacterInputModule`缓冲是否入队，再决定是否有生产首差。不要继续盲跑整场或把固定相机/UI差异并入此案。
