# NTSD28-336B44-Q07-C051-UNITY-DRIVER-001

状态：VERIFIED（仅严格诊断及首差）；生产缺口开放。原 Editor 左右 raw 均 PASS，首差见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/REPORT.md)。父目标G1/BATCH-04/Q07/C051及`NTSD28-UNITY-BATTLE-REALIGNMENT-001`。

正式前置：`NTSD28-336B44-Q07-C051-ORO-EFFECT23-REACH-001`已经证明OID20/action288→OID888/action35→40，第9tick对OID2应用effect23/dvx-10；右X550冲量-10、左X350 +10，当前336B44根各80tick×8字段640/640同态。远X1200和钳位X2000均仍命中，不作为阴性。

唯一脚本范围为`Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`增加严格`ntsd28-336b44-q07-c051-effect23/1.0`诊断schema，固定336B44身份、正式catalog SHA、seed682973786、mode0/stage23/difficulty0、12tick、OID20/action288/X500对OID2/action0/X550面右或X350面左、两人Z400/HP/MP500且中性输入。两个新JSON只写本Task证据目录。必须用现有原项目Editor和生产Driver的请求式raw捕获，按实体OID/slot映射对正式根tick1～12动作/HP/X/速度并检查子体出生及第9tick效果，先看首差再决定是否需生产修复。Unity项目地图Z/场景视觉比例是已批准例外，报告时独立标注，不伪称全字段同态。

不改DAT、角色图片、Scene、Prefab、生产规则或非战斗；不改旧schema夹具，也不覆盖既有输出。生成C#工程与原Editor导入0错；只运行两组聚焦raw及必要的同条件比较，不跑全测试。四保护Scene/Asset SHA复核。若原Editor被占用，保持请求未发、不另开项目。回滚仅撤本新schema/JSON并遵守用户工作保护与删除批准；C051/Q07/总目标不因诊断自动关闭。
