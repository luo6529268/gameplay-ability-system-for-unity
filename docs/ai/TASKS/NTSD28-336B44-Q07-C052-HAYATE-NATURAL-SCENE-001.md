# Q07/C052 疾风→417→211 原 Battle Scene 完整 tick 验证

状态：`VERIFIED_SCOPED_NATURAL_OPPOINT_SCENE / PHYSICAL_SELECTION_PENDING`。父 C052/Q07/总目标开放。[正式源码／根限定报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-OPPOINT-REACH-001/REPORT.md)与[原 Battle Scene 570字段验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-NATURAL-SCENE-001/REPORT.md)已登记。以下保留脚本前合同：正式初态为 OID73/action160/X500/Z400/team1、两名 OID2/action0/X589 与 X619/Z400/team2、mode0/difficulty0/seed682973786，三人中性输入。源码 tick4 出417、tick22出211、tick24子体双目标HP420。

只新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C052HayateNaturalBattlePlayProbeEditor.cs` 及唯一meta；不改Unity生产、Scene/Prefab/配置、DAT/图、音频和非战斗。原项目 Editor 空闲且 Battle Scene clean 时才通过一次性请求进入Play，Play副本中设三人初态，停稳后用生产 `StepOneTick` 逐tick推进至至少tick30，导出动作、HP、源坐标和OID417/211槽位、动作、XYZ。按同初态逐字段与正式源码比较，保留所有首差；检查退出后Scene clean及Menu/Battle/GameConfig/Mode Asset四SHA。原Editor导入编译和定向Play为必需门。物理键选到action160、全World、画面和设备音效不由本包宣称。

任何新请求／结果拒绝覆盖已有文件，不自动删除任何文件；失败保留原件。回滚仅在另行审计、明确许可后移除本包新文件；既有脏工作和用户资源不触碰。
