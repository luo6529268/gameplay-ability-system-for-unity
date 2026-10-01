# Q07/C052 疾风站立物理键原 Battle Scene 对照

状态：`VERIFIED_SCOPED_PHYSICAL_INPUT_SCENE`。父C052/Q07/总目标开放。[正式源／根限定证据](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-INPUT-001/REPORT.md)已证明OID73从action0、相对tick1～4 Jump、tick8～10 Defend+Right+Attack进入action160、生成OID417/211并双目标命中；原Unity Battle Scene同链36tick×19选定字段684/684零差。[Scene报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-SCENE-001/REPORT.md)。

只新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C052HayatePhysicalBattlePlayProbeEditor.cs`及唯一meta。原项目Editor空闲、单一干净Battle Scene才接受一次性请求；Play副本中三人OID73/2/2、源X500/589/619、Z400、team1/2/2、初始action0、seed682973786、mode0/difficulty0，用生产Driver提交上述固定离散键并运行36tick。导出动作、源规则位置、MP、目标HP和417/211出生状态，逐tick对正式源码前36tick；确认退出Play、Scene clean与Menu/Battle/GameConfig/Mode Asset四SHA前后稳定。保留所有首差和失败原件，结果／请求拒绝覆盖，不自动删除。

本包不改Unity生产、Scene/Prefab/配置、DAT/图、音频、非战斗。编译、原Scene完整tick与源逐字段比较是出口；结果仅证明已导出字段，逐hit内部字段／全World／画面仍各自保留。回滚涉及新增文件删除时先作文件审计并取得适用授权。

验收：原Editor导入v3脚本，原Battle Scene Play36tick后退出且Scene clean，Menu/Battle/GameConfig/Mode Asset四SHA稳；正式源`current_mp`对Unity`Runtime.PP`，tick8为300、tick31为420。v1输入域错误308差与v2资源字段错误29差均保留并更正，不算生产首差。当前限定子包已验，逐hit内部字段、真实玩家键盘和全World仍待。
