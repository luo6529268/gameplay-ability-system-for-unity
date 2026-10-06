# NTSD-SCENE-MISSING-TMP-FONT-EDITOR-001-PREPARE

状态：PLANNED。用户2026-10-06当前会话明确要求将MenuFont3500MigrationEditor改为当前Scene的缺失FontAsset补齐编辑器，可指定目标字体。执行者Codex，根 I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity。

操作：原脚本原地替代，治理文档追加、新聚焦测试（及Unity生成.meta）；无删除移动或Git丢弃。五个已存在文件逐路径/SHA/dirty与逐字备份见 artifacts/diagnostics/NTSD-SCENE-MISSING-TMP-FONT-EDITOR-001/prechange-manifest.json；Menu/Battle、Plus字体和原脚本.meta只读保护。旧特定字体迁移入口被本用户需求替代，历史Record不删除。

拟执行：原脚本apply_patch；dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly；原Editor刷新、定向EditMode测试（仅临时场景）及菜单打开；Tools/Validate-ChangeLedger.ps1、git diff --check。实际字体批量改动不在本轮工具开发中执行；不保存Scene/字体。

恢复：逐字before镜像，核对后继用户修改后恢复准确代码；治理追加纠正。输出唯一新文件不覆盖旧证据，不安装/改插件，不启动同项目第二Editor。


执行后状态：VERIFIED。原脚本原地替代、新测试/.meta与治理追加，五个前镜像SHA相同。最后compile-05 exit0/0error；首次TestRunner前置与首次直接诊断Scene API不支持失败原件保留，最终直接Editor两项通过JSON。只新增并关闭自身未保存临时场景，原Scene字体引用、活动Scene及dirty=true保持；原Menu/Battle、Plus资产及原脚本.meta磁盘SHA相同。菜单已打开，未选择/执行真实Scene批量赋值或保存。完整后镜像postchange-manifest.json、validator-final/diff-check-final原件同ID，无删除/移动/Git操作。
