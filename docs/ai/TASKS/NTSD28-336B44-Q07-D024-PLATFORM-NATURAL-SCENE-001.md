# NTSD28-336B44-Q07-D024-PLATFORM-NATURAL-SCENE-001

状态：`VERIFIED_SCOPED_NATURAL_SCENE_NO_CARRY`；父项为336B44总表Q07/D-024。权威限定输入及96tick结果见[正式源/根报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-PLATFORM-NATURAL-ENTRY-001/REPORT.md)：多由也36、飞段56、鸣人2均从action0，X100/200/185、Y0/Z400、team1/2/1、mode0/seed `0x28A55A5A`，多由也防御tick1–2→上tick3–4→攻击tick5–6，鸣人跳tick9–10。正式源/根各960/960字段同；tick23飞段182，tick29–31鸣人平台链接，鸣人X始终185。正式根 `nativeParityClaim=false`。原Battle Scene同链限定Play 1440/1440、零残留/四SHA稳，见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-PLATFORM-NATURAL-SCENE-001/REPORT.md)；非零自然搬运和Q07父项继续开放。

只扩现有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C040NaturalScenePlayProbeEditor.cs` 的**独立 opt-in mode=platform 与独立 Temp 请求/诊断输出路径**；保留原C040模式、样本、按键及路径。复用该探针已有原项目身份、Menu/Battle clean及四SHA保护、Play clone roster配置、正式LoganRuntime、生产`SimulationTickDriver.StepOneTick(FrameInputSet)`、有序关闭和回原Menu的流程。平台模式仅改测试初始 roster 的ID/规则X/seed、离散输入序列、样本长度96，增加只读平台碰撞参考/来源槽/阴影字段。正式三键到Unity旧字段的现有映射应明确使用：源防御→`SimulationInputButtons.Attack`，源攻击→`Jump`，源跳跃→`Defend`，方向Up不变。不得把用户物理键或Unity输入名称当源数组索引。

先原Editor导入编译，再新run执行一例；只和正式源/根共有tick及选定字段比较，立即记录首差。若首差来自探针输入或初态，先修探针并保留失败原件；若来自生产，按独立最小Change修通用战斗逻辑。若无差，限关闭此自然三人Scene子门，不将无搬运样本写成非零搬运通过，不重复已过受控十tick。验收包括脚本编译0错、一个真实原Battle Play、至少源/Unity三槽动作/X/Y/碰撞参考/链接与根选定字段配对、四保护SHA不变、有序关闭零残留、回原干净Menu。回滚按本ID前向修订，保留用户既有C040脚本改动与旧结果；不改DAT、正式资源、Unity生产、地图、相机、场景或非战斗模块。
