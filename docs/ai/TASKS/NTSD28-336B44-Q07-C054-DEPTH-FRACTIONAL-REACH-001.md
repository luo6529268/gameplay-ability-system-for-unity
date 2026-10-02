# Q07/C054 自然纵深小数解融合入口

状态：`VERIFIED / SOURCE_SCOPED_NEGATIVE`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，BATCH-04/Q07/C054；仅本输入窗关闭，C054/Q07开放。

正式规则依据：336B44 playable 的 `BattleWorld28::advance_native_fusions` 在解融合时用主角整数XYZ重建伙伴精确XYZ；正式OID10/11→OID52的融合记录 `decrease=200`，已有中性及水平右移样本tick201自然拆分但主角精确XYZ全为整数。正式OID52 `c/nar/kyu.dat` 的 `running_speedz=3.7`；`input_routing.cpp` 的跑步纵深速度与 `physics_integrator.cpp` 的精确Z积分提供可判别的自然小数候选。只是源码路径假设，必须实测。

只在现有 `Tools/NTSD28Q07Diagnostics/fusion_natural_fractional_reachability_probe.cpp` 增加一个 `diagonal_run` 输入 profile：第二融合记录tick1自然合体后，以离散水平双按进入奔跑、短时同时按纵深方向，随后停止并等到tick201或会话终止。保持原三个 profile 行为不变、原件不覆盖。输出完整tick精确/整数XYZ与自然拆分标志；若真正得到自然小数拆分，再追加同次LFR供336B44根正式EXE回放，并考虑原Unity Battle Scene。不得注入精确小数、手动减计时、改DAT/角色图、正式源码、Unity生产、Scene或非战斗。

验收：脚本修改前建独立Change/Ledger/STATE/handoff；按当前 playable 构建闭包编译0错；新profile双跑CSV/summary同SHA；检查tick1融合、输入动作与纵深速度、拆分前精确Z是否非整数以及拆分时整数/精确重建。阳性才做根/Unity对照；阴性按唯一输入窗记录，不推断其它移动、命中或整场不可达。最窄账本检查与diff check；回滚只审阅本诊断脚本差量，不删除或还原现有文件。

结果：首次编译误用枚举名失败并留原件，更正后当前 playable 闭包 g++ 退出0。两次独立源码运行 CSV 与摘要逐SHA相同；tick1合体、tick201拆分，输入窗内动作310不变、纵深400不变、自然小数拆分未发生。正式fusion记录配置310，但OID52当前DAT无frame310，正式输入路由在无帧时返回。仅本候选阴性；根/Unity对照未运行。见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C054-DEPTH-FRACTIONAL-REACH-001/REPORT.md)。
