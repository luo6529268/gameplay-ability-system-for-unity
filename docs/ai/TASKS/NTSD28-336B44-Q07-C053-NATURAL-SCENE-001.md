# NTSD28-336B44-Q07-C053-NATURAL-SCENE-001

状态：`VERIFIED_SCOPED_NATURAL_SCENE`。父项：新版 336B44 G1 / BATCH-04 / Q07 / C053 / `NTSD28-UNITY-BATTLE-REALIGNMENT-001`。

目标：把已由正式 336B44 playable 源码和根 EXE 证明的受控初始双角色、自然 OPoint 生产者案例，放进原项目保存的 Battle Scene 的生产 `SimulationTickDriver`。OID65 安科 slot0/team1 初始 action511/source X610/Y0/Z400、OID702 自来也 slot1/team2 初始 action553/source X500/Y0/Z400，seed682973786、mode0、difficulty0、两角色 MP500；后续均为中性输入。比较前八 tick 的主角动作、OID808 与 OID875 的出生/动作/源规则坐标、OID808 锁存与 HP、命中 rest，发现首差即留原件。初始动作是受控设置，不称为玩家物理键自然选招。

修改范围：只新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalSceneProbeEditor.cs` 与唯一 `.meta`；必要的离线生成工程补项只写入 `Temp/`。新增本 Task/Change/诊断和当前总表记录。不得修改生产战斗脚本、DAT 数值或文件、原资源、Scene、Prefab、ProjectSettings、菜单和结算。复用现有请求式 Play 探针的原项目/单场景/clean 检查、Play 前 roster 配置、稳定暂停、完整 Driver 单步、SessionState 续写与退出清理，不创建临时 OID875。

前置与风险：只对已确认的原 Editor 非 Play、非编译、唯一 clean `NTSD_Battle.unity` 发送一次唯一请求；拒绝覆盖已有结果。正式资源预热、默认角色输入或对象槽位可能形成首差，失败时保留 JSON 和原场景，不为匹配而改 DAT/生产代码。启动前后核 Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 四文件 SHA；退出后还需 Editor 非 Play、Scene clean。若原 Editor 被用户占用或场景变化，停止场景动作，保持 C053/Q07 开放。

验收：新探针离线生成工程编译 0 error、原 Editor 导入编译 0 error；原 Battle Scene 完整 Driver 至少八 tick，逐字段对照同参数正式源码 CSV 和根 trace，输出差异明细；退出 Play、场景与四保护 SHA 稳定；运行 `Tools/Validate-ChangeLedger.ps1`。只在这些证据成立后把本子包写为 `VERIFIED`，不因此关闭 C053、Q07 或总目标。回滚只审阅本包新增文件；删除须单独记录与批准。

结果：首轮错误朝向导致tick1逆向位移，原件保留；第二唯一RunId按正确朝向在原Scene8tick完成、自然生成OID808/OID875、tick7单Uj末态，所选实体+源码seed RNG共184/184字段零差，Play退出clean/四保护SHA稳。根LFR不携带源码seed，CRT不能直接与源码seed案例比；见[限定报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-SCENE-001/REPORT.md)。物理选招/双Uj/逐hit仍待，父项不关闭。
