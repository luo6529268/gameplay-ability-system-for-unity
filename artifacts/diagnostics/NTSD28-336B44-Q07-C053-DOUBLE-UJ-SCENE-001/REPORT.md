# C053 原 Battle Scene 双 Uj 定向对照（2026-10-01）

当前战斗规则权威为根目录正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 与其对应 playable 源码。本报告的源侧是按该源码闭包重新链接的**受控四战斗对象完整 GameSession**，Unity 侧是**原项目保存的 NTSD_Battle Scene、正式暂存内容和生产 SimulationTickDriver**；现有根 EXE headless LFR 只能重建两名战斗对象，故不声称四人根 EXE 回放同态。

初态：OID702/action553/source X500、OID2/action0/source X700、两名正式 OID875/action55/team2 分别为 runtime slot2/3/source X620、Z401；mode0、difficulty0、seed682973786、中性输入。Unity 原 Editor 编译已导入新探针 GUID `4f78e939fa554f0291e66c1df17a8d03`，`Assembly-CSharp-Editor.dll` 含其类型名，Tundra 构建成功；离线显式纳入新文件的 `dotnet build` 0错误（277警告）。唯一请求 `o702-t700-a620-b620-scene-01` 由原 Editor 消费，Play 完成后结果为 `SCOPED_PASS / DONE`、8相对tick、global tick5→13、已退出Play且Scene clean。

独立比较原件 [source-unity-first8-comparison.json](source-unity-first8-comparison.json)：逐tick比较 child slot/action/latch/source X/Y/Z、两攻击者动作、CRT state/calls、自定义同步随机计数/index/calls，共 **13 字段×8 tick＝104/104 相同，首差无**。子体slot50在相对tick1出生；tick6 action/latch153、X618；tick7 action/latch156、X638，CRT state `1758127634→1334545526`、calls `3000→3004`，两攻击者动作55→11。源tick7 hit事件顺序 `2:0:2:156;3:0:2:156`；Unity两名攻击者对应的受害者rest在tick6均0、tick7均10、tick8均9。这些与动作、位置、RNG同态共同支持**该受控双命中出口**，但Unity探针没有逐hit事件或命中瞬间的锁存字段，不能把源事件序列当作Unity直接观测值。

Play结果原件：[o702-t700-a620-b620-scene-01.json](o702-t700-a620-b620-scene-01.json)。Unity报告的Battle Scene入/出SHA均为 `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`，`exitedPlay=true`、`sceneCleanAfter=true`。独立复核 Menu、GameConfig、ProjectBattleModeConfig的SHA分别为 `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，与前值一致；四文件均无Git状态差异。原Editor日志末1200行中本探针错误、C#错误、NullReference/InvalidOperation标记为0，但这不等于整个Console无历史错误。请求文件已消费为`requested:false`。

**边界与后续：** 这是受控正式帧与正式内容、8tick、声明字段的源→Unity原Scene证书；不是玩家物理键自然选招、根EXE四人回放、全战斗、全部C053触发或Q07完成。探针未导出对象池借用数；Task承诺的该项关闭证据仍缺。C053父项记 `SCOPED_SCENE_PASS / RUNTIME_PENDING`，Q07/BATCH-04和总目标保持开放。下一按当前336B44总表G1选择自然可达首差或补充借用/正式根载体，不重复全量旧案例。
