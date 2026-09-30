# Q07 飞段 X580 显式 BGM 同初态 RNG 诊断

状态：`VERIFIED_SCOPED_DIAGNOSTIC_RNG_PARITY`。本包只纠正原 Editor raw 诊断的一个预抽前提，不改变生产 RNG/攻击规则，不关闭 Q07、BATCH-04 或总目标。

正式根 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 的既有 X580 40-tick LFR 明确选择 BGM2，战斗前同步 RNG counter/index/calls `0/0/0`。旧 Unity raw 诊断的 `ResetForDirectBattle(seed)` 先为随机 BGM 抽一次，头部 `1/1/1`，使旧 440/440 已选战斗字段无法证明同步 RNG 同初态。本轮只将 `NTSD28UnityRawCaptureEditor.ConfigureWorldAndRoster` 的 `Q07HidanNaturalCatchScenarioSchema` 加入既有显式 BGM `ResetFromSeed` 分支；其余 schema 和生产调用方不变。

生成的 `Assembly-CSharp-Editor.csproj --no-restore` 编译 0 错、201 个原有警告，日志见 `generated-editor-build.log`。原项目唯一 Editor PID11944 通过项目已有 Unity MCP 本地桥接执行 `refresh_unity`；Unity Tundra 编译成功，`Library/ScriptAssemblies/Assembly-CSharp-Editor.dll` 时间晚于编辑后的脚本。原 Editor 在 EditMode 消费唯一 X580 请求，`hidan-x580.result.txt` 为 `PASS`，输出 40 行完整 Driver raw、domain-v2、input-RNG；请求文件已消费。没有启动第二个 Unity 项目或 Editor。

独立重读新输出与既有根正式 EXE 轨迹，见 `hidan-x580-comparison.json`：头部同步 RNG counter/index/calls、last site 和 table hash 全同；40 tick × 11 个已选战斗字段 `440/440`、40 tick × 5 个同步 RNG 字段 `200/200`、输入相位 `40/40`，均无首差。tick2 两端在 `0x82` 位点作第一笔同步调用，Unity result1、正式 requested action65、Unity action65。新 raw SHA-256 `CEA5600C908D30D7BA5E3670FCC7D557243CD4EE30DFEB22D22EA1ADF1E55F64`；input-RNG SHA-256 `0F6639B9370F765E3083854CDD2B7DFECEC47ECA51A45FCB6FD8015D344D7974`。既有 X1200、所有角色和全场景未重跑。

这不是完整 RNG 状态证书：根 LFR playback 的 CRT 初始化用默认 seed0，Unity 诊断用场景 seed681925210，所以 CRT 标量不比较；根轨迹未提供可与 Unity 每次调用逐条比对的完整随机流。另一个原 Battle Scene 物理 Play 首攻动作60使用不同 Stage/Z/输入相位且未有同步随机表同初态证据，仍按原报告保留，不能据本 EditMode 结果推断它的原因或修改生产攻击公式。

验证后 Editor MCP `get_editor_state` 为原 Battle Scene、EditMode、idle、非编译/非导入/非测试。Battle Scene SHA-256 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`，Menu `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`，GameConfig Asset `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`，项目 mode Asset `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，均与包前既有值一致。`git -c core.safecrlf=false diff --check` 退出码 0；Change Ledger 校验结果见 Record。

Change Ledger 校验退出码 0（完整输出 `ledger-validator.log`，含既有历史 Record 的非当前 diff 警告）。
