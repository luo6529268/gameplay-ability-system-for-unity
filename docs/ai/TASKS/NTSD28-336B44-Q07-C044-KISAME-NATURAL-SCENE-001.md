# NTSD28-336B44-Q07-C044-KISAME-NATURAL-SCENE-001

状态：`VERIFIED_SCOPED_NATURAL`。父目标 NTSD28-UNITY-BATTLE-REALIGNMENT-001；BATCH-04/Q07/C044。正式源/根 OID17 action314/X550 在tick47自然timeout1→-6释放、受害者pending冲量X4/Y-3；action314/X1200无抓取，正式根同源LFR已验。[前置报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-KISAME-NATURAL-REACH-001/REPORT.md)。

只新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C044KisameNaturalBattlePlayProbeEditor.cs` 及Unity生成的同名`.meta`，在**原项目原 Battle Scene** 且Scene干净、Editor非Play时，由文件请求启动两次独立Play：OID17/action314 X500 对OID2/action0 X550、X1200，Z400、HP/MP500、种子682973786、mode0、难度0、两队、中性输入，各60个生产Driver tick。捕获双方动作/计数/源规则位置/速度/HP/抓取关系/timeout及RNG；重点比较tick46～48，检查自然释放后下一步冲量结算。不得改DAT、Scene、Prefab、生产代码、非战斗或新建Unity项目；不通过computer-use操作Editor。

脚本必须限定原项目、请求ID、干净Scene，结束退出Play并确认Scene哈希和四保护资产SHA稳定。先原Editor编译0错，再跑近远两例；与正式源同初态、同tick逐字段首差。若发现真实首差，另立最小生产修复Task/Change并先定向RED；本包不预判结果。记录失败、未测和回滚边界；回滚仅审阅本新增测试脚本与`.meta`，不得清理用户文件。

进展：独立探针及meta已写；生成C#工程包含新脚本的构建0错误，原Editor程序集未更新。已请求原Editor执行Assets→Refresh，Play与比较尚未执行。

2026-10-01 原 Editor 已完成导入/编译；近距 X550 与远距 X1200 各采 60 生产 tick，分别对当前正式源码 20 字段 1200/1200 首差0，近距 tick47/48 释放与延后速度一致，远距不抓。双 Play 退出 clean、四保护 SHA 稳。限定本 Task 为 `VERIFIED_SCOPED_NATURAL`；C044 其它入口与 Q07 保持开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-KISAME-NATURAL-SCENE-001/REPORT.md)。
