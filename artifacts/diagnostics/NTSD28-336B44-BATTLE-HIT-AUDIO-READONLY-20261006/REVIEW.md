# 独立限定复核与总表回链

本报告属于2026-10-06用户新提出的当前Unity与Logan正式版战斗逻辑/受击音效检查。[当前336B44总表](../../../Assets/NTSD/Docs/ntsd28-logan-336b44-vs-unity-battle-alignment.md)的旧目标限定收尾保持；本诊断不自动恢复Q07/Q10/Q12或创建新目标。本次新发现及下一步建议唯一报告为[REPORT.md](REPORT.md)。

独立复核执行者：/root/hit_sound_resource_identity（luna_worker）。只读重分组JSON/CSV，核基本33条、所选帧125条、并集150条；结果分别6/12/13/2、106/6/10/3、111/17/18/4，均按缺失/PCM或格式差异/PCM等价/逐字节相同顺序闭合。020/021优先正式staging，正确排除其旧文件差异。词法声明范围、自然可达未知、PCM与听感层级、非全部技能/BGM/菜单音频边界正确。无需更正。

主代理再次核12个脚本/Scene/配置hash未变、CSV150路径无重复、三组计数闭合、当前无已跟踪C#脚本diff，见final-audit.json。独立复核与本轮主代理均未运行Unity、构建、测试或音频回放。未修改已有项目文件。
