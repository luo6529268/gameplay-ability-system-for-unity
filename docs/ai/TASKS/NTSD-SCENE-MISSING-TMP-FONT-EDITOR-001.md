# NTSD-SCENE-MISSING-TMP-FONT-EDITOR-001

状态：PLANNED。需求：当前场景缺失TextMeshProUGUI FontAsset可配置编辑器。限定原脚本替代及聚焦验证，不自动保存/执行真实Scene替换，不扩展游戏UI逻辑。目标、前置、验证、副作用与恢复详对应Change Record。授权为2026-10-06当前用户明确要求修改此脚本并可指定字体。


最终状态：VERIFIED（Editor工具范围）。菜单NTSD/UI/补齐当前场景 TMP 字体；原Editor已打开入口，用户选择字体后执行。compile-05 0error、两项直接Editor断言通过，保留实际Scene未保存状态和资源SHA；没有执行真实Scene批量改动。证据/先前失败和验证适配详Change Record与同ID REPORT.md。
