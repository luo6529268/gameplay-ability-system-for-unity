# Q07/D-024 武器拾取相位与源坐标场景见证

状态：`VERIFIED_SCOPED_DIAGNOSTIC`。当前权威是根目录 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的正式 EXE、对应 playable live source 和正式 runtime 内容。此 Task 仅验证原 Unity Battle Scene 的诊断初态，不能改变正式规则。

前置事实：现有鸣人物理 J 键拾取探针在相对 tick1 报动作60、相位0，50 tick 未拾取；当前源码同例 tick1 相位1/动作0、tick2 相位0/动作115。原场景启动后探针记录的初始相位为1，Q09 已使用出生前把受控世界相位配成0的诊断方式。现有 Q07 探针未启用像素分支时，临时 OID120 武器也未初始化源规则坐标，无法证明 WPOINT 比例分支。相位错配是待验证假设，不是已证生产 bug。

范围：只修改 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NarutoPhysicalPickupBattlePlayProbeEditor.cs`，给既有请求增加仅诊断使用的相位配对与武器源规则出生选项，并在报告中记录配对前后相位、源出生状态。原有默认请求行为保持；不改输入映射、生产 runtime、DAT、PNG、Scene、Prefab、相机、菜单/结果页。

出口：原项目 Editor 闲置且保存的 Battle Scene clean 时，以新唯一 runId 运行物理 J→K 场景；比较正式当前源码同条件的拾取相位、动作、关系、源 X/Z 与原场景前 24 tick，持有时核对物理相对 X/Z 为源整数相对距离乘共用视口比例（整数锚点误差小于1输出像素）。记录首差或限定通过；若仍不拾取，停在真实首差，不为通过而改生产。运行生成工程/原 Editor 编译、目标 Play、退出 clean、原场景及配置与旧证据 SHA、`Tools/Validate-ChangeLedger.ps1`。无需重复全量场景。

回滚：审阅并逆向移除本 Task 诊断脚本的精确增量；保留已产生的请求和报告原件。禁止 blanket restore/reset/clean 或删除任何已有文件。

实际结果见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-PICKUP-PHASE-SOURCE-WITNESS-001/REPORT.md)：原Editor物理键注入后第2相对tick拾取；前7tick正式源码选定77格中75格原值同、2格空关系哨兵不同。18持有tick比例误差X/Z均小于1输出像素，退出clean/六SHA稳。未证根EXE同条件、canonical非武器或像素画面，Q07仍开。
