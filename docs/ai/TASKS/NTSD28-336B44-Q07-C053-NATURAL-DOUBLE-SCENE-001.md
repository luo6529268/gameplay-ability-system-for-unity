# NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-SCENE-001

状态：`VERIFIED_SCOPED_NATURAL_DOUBLE_SCENE`。父项：新版 336B44 G1 / BATCH-04 / Q07 / C053 / `NTSD28-UNITY-BATTLE-REALIGNMENT-001`。

目标：在原项目保存的 NTSD_Battle 场景中，使用三名正式角色的生产 `SimulationTickDriver` 复核当前 336B44 playable 自然双 Uj 来源。slot0/slot2 为 OID65 安科，team1、action511、源 X580；slot1 为 OID702 自来也，team2、action553、源 X500；均 Y0/Z400、HP/MP500，seed682973786、mode0、difficulty0，后续中性输入。正式源码同初态的 12 tick 原件见 `artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-PRODUCER-001/source-run-01.csv`。初始动作受控设置，不宣称物理按键自然选招。

范围：仅新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs` 与唯一 `.meta`、本 Task/Change/诊断及总表记录；必要的生成工程编译目标仅写入 `Temp/`。不得修改生产脚本、DAT、图片、Scene、Prefab、ProjectSettings、菜单或结果页；不得手动创建 OID875/808。沿用已验证的原项目/干净场景/Play 前 roster 配置、稳定暂停、完整 Driver 单步、唯一请求、SessionState 续写和退出流程。

风险与保护：工作树现有未提交内容全部保留。只有原 Editor 非 Play、非编译、原 Battle Scene 单场景 clean 和四保护文件 SHA 核对后才发送唯一请求。若条件不满足，保持只读。发现首差保留 JSON，不以修改 DAT/生产代码迎合。退出后核 Editor idle、Scene clean 与四 SHA 不变。由于正式根 LFR 不支持第三槽初始动作覆盖，不把不等价根 trace 当本案例依据。

验收：新探针生成工程编译 0 error、原 Editor 导入编译 0 error；原 Battle Scene 至少 12 个完整 tick，逐 tick 比较三角色动作、自然 OID875 数量/owner/槽位/动作/位置、OID808 动作/HP/锁存及两个独立 rest，准确记录首差；退出 Play、场景 clean、四 SHA 稳；运行 `Tools/Validate-ChangeLedger.ps1`。本子包通过仍不关闭 C053/Q07 或总目标。回滚仅审阅新增探针、meta、文档及诊断，删除须另行记录和批准。

结果：新增测试探针/meta，生成工程 0 error/293 warning、原 Editor 导入运行；原 Battle Scene 12 tick×11 个选定字段 **132/132** 同正式源码，tick7 两自然攻击者各自 rest10、目标 HP450。Play 已退出、Scene clean、四保护 SHA 稳。逐 hit 内部过程、物理键、完整 World 仍待，C053/Q07/总目标开放。[原件](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-SCENE-001/REPORT.md)。
