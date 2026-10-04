<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-GAARA-J4-043-NATURAL-VOICE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10GaaraJ4043NaturalVoiceProbeEditor.cs
authority: 336B44 formal playable natural Gaara j4 and 043 event chain and user-approved Q10 battle-only scope
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-GAARA-J4-043-NATURAL-VOICE-001.md
-->

# NTSD28-336B44-Q10-GAARA-J4-043-NATURAL-VOICE-001

脚本改前登记。正式OID16自然防前攻已证tick13/15的j4/043事件，Unity对应正式WAV已暂存和导入，但战斗生产voice未知。现有078探针具备原Scene安全Play、声音clip/voice见证及退出保护模式；本包仅为我爱罗条件新增独立Editor-only opt-in探针与meta，不改其原探针和任何生产/DAT/Scene/旧Sound/非战斗。

受影响符号限新增类 `NTSD28Q10GaaraJ4043NaturalVoiceProbeEditor` 的菜单入口、Play clone配置、逐tick记录、退出保存；副作用只是在原Editor一次40tick Play、写一个新JSON和诊断日志。不得覆盖原始输出，必须复核Battle/Menu/配置/两正式WAV哈希、Scene clean和池/World残留。可能风险是测试输入映射与自然动作不同、并行Scene保存、Editor脚本编译/导入或播放提前中断；一律记录首差/阻断，不把诊断失败写成战斗规则FAIL。验收与前向更正/文件操作边界见Task。实际代码、编译、Play和结果待写，不预先升格。

实际只新增声明的Editor诊断脚本及独立meta GUID `15b3f75a127347f586a96d9a8487f932`（全Assets唯一）；复用078探针安全生命周期，改为Gaara OID16、40tick、防前攻、两正式cue及位置/voice字段。原Editor刷新/程序集重载、生成工程`dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` exit0/0错；原Battle Scene opt-in Play一次，正式可比phase/action/PP/X/Y 200/200同，j4/043分别tick13/15待播、正式AudioClip、池化play+1、对应AudioSource播放中。退出非Play/Scene clean、Driver World空/Pool0、Battle/Menu/配置/正式两WAV/旧043 SHA稳定。[原始JSON、残留和报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-GAARA-J4-043-NATURAL-VOICE-001/REPORT.md)。Z与tick27撞击因用户项目地图例外不可同态，设备PCM/正式EXE扬声器未验；此Record仅关闭自然j4/043 voice。
