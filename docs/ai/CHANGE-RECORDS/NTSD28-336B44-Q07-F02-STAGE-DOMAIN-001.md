<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F02-STAGE-DOMAIN-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/f02_pickup_throw_entry_probe.cpp
authority: 336B44 playable and root F02 kind10 positive; user-owned Unity stage domain
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F02-STAGE-DOMAIN-001.md
-->

# NTSD28-336B44-Q07-F02-STAGE-DOMAIN-001

脚本修改前创建。原工具把三实体的 Z 固定为 542，原 Unity Battle Scene 的项目地图在首轮完整 tick 把 Z 限制到 481/482，故旧正式源/根证书不能用于这张地图的同初态 Scene 比较。本次仅增加可选 Z 参数并保留默认行为；模式、角色、武器、条件攻击输入、动作和生命周期不变。不修改正式源码、DAT、Unity 生产、Scene、地图、菜单或其它用户文件。范围、验证、失败保留和回滚见同 ID Task；任何新 Z 正例都必须重新跑正式源码/根，不得继承旧 Z542 的阳性结论。

正式源/根已实测背景23/Z400在 tick1 被正式背景边界限回 Z542，所以根报告虽 PASS，仍非同条件 Unity 比较。下一脚本修订前追加边界：只给同一诊断增加可选正式背景 ID1（该正式背景 DAT 的纵深 375～575 包含 Z400），默认 ID23 保持；Unity 仍使用自己地图，不迁入原版背景。之后按新背景重新跑正式源/根并测同初态 F02，不直接转用背景23/Z400报告。

实际代码仅改既有 `make_config` 与 CLI 参数解析，允许 Z180～542和正式背景1/23；旧默认不变。`g++` 以原 playable 源构建清单编译两次exit0、输出0字节；第二版背景1/Z400正式源初态+128tick、根336B44同LFR的24字段3096/3096无首差，拾取18、投掷24、释放30、kind10命中7次、F02 tick39；旧默认背景23/Z542 CSV SHA与旧报告相同。完整证据、资源根失败原件及限定范围见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F02-STAGE-DOMAIN-001/REPORT.md)。Unity同条件Scene仍待，因此状态`RUNTIME_PENDING`。回滚仍为前向更正，保留旧结果。
