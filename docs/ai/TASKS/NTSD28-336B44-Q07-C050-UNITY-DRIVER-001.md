# NTSD28-336B44-Q07-C050-UNITY-DRIVER-001

状态：VERIFIED（限定诊断出口）。父目标G1/BATCH-04/Q07/C050仍开放。

正式依据：336B44源码完整 GameSession 中 OID24/action37→38 对 OID56/action259 第2 tick 的 ITR kind0/dvy-5 命中，目标锁存首 BDY kind50，正式跳过垂直累计、目标Vy0/HP465；近X520/X550双跑阳性，X1200无命中，根EXE同LFR近远各40tick的5字段×40=200/200相同。源报告见 `artifacts/diagnostics/NTSD28-336B44-Q07-C050-HIDAN-BDY50-REACH-001/REPORT.md`。

范围：仅扩展 `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs` 的严格新版 `ntsd28-336b44-q07-c050-bdy50/1.0` 诊断 schema、336B44身份、正式catalog与固定初态/3tick校验；新建近/远两份场景 JSON 和输出在本Task诊断目录。原Unity Editor空闲时用既有请求式 raw capture 的正式 `LoganRuntime` 输入运行，先读取逐tick目标HP、Vy、动作等是否对正式源/根首差。此Task不得修改生产代码、DAT数值、图片、Scene或非战斗逻辑。若出现首差另建最小生产修复 Task/Record，不提前改规则。

验证与风险：生成C#工程/原Editor编译0错；请求运行近X520远X1200各3tick，实际输出与根对应前三tick按选定字段比较；若数据绑定或Editor不可用，明确记录阻断。只允许原Editor，不开第二Editor。资源和四保护场景/配置SHA前后复核。回滚本新schema/JSON须遵守删除批准规则，已有请求及脏工作不覆盖。

结果：原Editor修前两请求均PASS并定位近距tick2/3目标动作首差；本schema头更正及独立生产修复后，原Editor同两请求再次PASS，正式头身份336B44、近远各18/18选定字段同根。地图Z夹持属于项目场景例外，没有纳入这些战斗字段。见同ID报告；整场Play/所有字段另验。
