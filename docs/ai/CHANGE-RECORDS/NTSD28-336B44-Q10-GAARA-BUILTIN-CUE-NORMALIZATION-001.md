<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-GAARA-BUILTIN-CUE-NORMALIZATION-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q10Diagnostics/gaara_sand_blast_043_probe.cpp
authority: 336B44 playable WorldAudioEvent28 builtin-channel contract and original Battle Scene Gaara audio observation
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-GAARA-BUILTIN-CUE-NORMALIZATION-001.md
-->

# NTSD28-336B44-Q10-GAARA-BUILTIN-CUE-NORMALIZATION-001

脚本改前登记。原我爱罗C++诊断的`audio_paths`只写`resource_path`，正式内建channel字段为空；Unity tick27含SFX_001/SFX_006，不能直接按路径清单判它们是重复声音。本包仅扩独立C++诊断在新CSV列中显式记录source/channel/path，重新编译并一次40tick至全新目录，不改原样本/生产/资源/Scene。

预期副作用是新CSV/LFR与配对报告；风险是channel转Unity SFX符号并非直接一一映射，须追正式消费者确认。验收为新输出可追溯、不覆盖旧输出，逐tick内建/定义音频事件在相同条件下有可比规范形式，并记录真实首差或证伪；保护文件哈希保持。回滚为前向更正，所有已有样本保留。实际代码/编译/运行结果待补写。

脚本第二次修改前增补范围：第一轮Z650仅得正式tick27一个builtin channel2，而Unity同tick SFX_001/006；但正式根目标动作180、Unity186且Unity项目地图把Z650钳到481，不是同初态。因此允许同一独立诊断新增可选初始Z参数，默认650不变，仅一次Z481源对照。若Z481后目标仍异，音频差保持不可裁决，不改生产。

实际只修改声明的C++诊断工具：保留旧`audio_paths`并新增`audio_tokens`，对builtin输出channel；可选初始Z参数默认为650。`build-02`/`build-03`均按已存编译参数 exit0、stderr空；`run-v2-01`与`run-z481-01`各40tick exit0，所有输出目录CreateNew且旧CSV/LFR不覆盖。Z650 tick27记录`path:data/033.wav|builtin:2|path:data/069.wav`，原LFR SHA不变；正式sound.dat index2为`data/006.wav`，解释Unity SFX_006。Z481第一tick按正式背景边界变542，Unity项目边界为481；正式根同条件原tick27目标action180而Unity186，剩余SFX_001不能同受击条件裁决。没有新可修的生产首差；[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-GAARA-BUILTIN-CUE-NORMALIZATION-001/REPORT.md)。正式源码、生产、DAT、音频、Scene均不改；回滚为前向更正并保留全部原件。
