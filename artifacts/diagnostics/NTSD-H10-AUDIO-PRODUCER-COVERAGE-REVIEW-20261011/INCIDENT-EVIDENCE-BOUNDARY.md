# 历史 Menu manager 恢复证据边界

本文件是对 NTSD-H10-FULL-CATALOG-AUDIO-ENVELOPE-20261011 旧报告的追加限定说明；旧报告、事件记录和失败原件未改写。本任务只读原文件/日志/测试框架源码，没有调用 Editor、重载、测试、编译或对象 API。

**直接可证：**首次资格失败缺少原 root ID216430；磁盘 Menu/Battle scene 与 meta 未变。现存 Editor.log 实际记录 Menu 重新加载，随后场景查询为 loaded/clean/root8。**未证：**该次恢复后原 MMSoundManager 组件的稳定身份与逐个序列化字段完全相等。sceneRestoredByFramework=true 是恢复测试写入的常量，不能自证这项结论。

| 链条 | 精确原件或源码 | 层级 / 限制 |
|---|---|---|
| 失败现场 root 缺失 | 旧 qualification-cleanup-failure.json：expected roots含216430，actual缺少；cleanup空、新generic key保留 | 直接断言失败；原root→稳定组件身份未单独保存 |
| 场景文件可用且未变 | qualification-scene-asset-identity.json，四项当前SHA重新核对；本目录 historical-recovery-evidence.json | 磁盘事实，不等于内存组件恢复 |
| 框架保存/恢复路径 | Library/PackageCache/com.unity.test-framework@1.1.33/UnityEditor.TestRunner/TestLaunchers/EditModeLauncher.cs:94-97 保存SceneSetup/打开临时场景；TestRunner/Callbacks/EditModeRunnerCallback.cs:103-113 RestoreSceneManagerSetup/catch warning | 设计路径，未逐组件断言 |
| 首次用例版本标识 | Editor.log LF行5068733/5068737，fixture入口源码3137，保存job7021305ff10748fa8268f1ffff4dd1ac；XML test ID18914/seed1875422841 | 联合时间/版本/顺序关联 |
| 实际Menu重新加载 | Editor.log LF行5068821，Deserialize24.933ms/Integration451.981ms；5068838 RunFinished / 5068845 CompleteTestRun | 直接日志。Loaded行本身不带jobID或timestamp；不能声称该行直接标注job |
| 排除v2混淆 | v2 job025d61ab50aa439a922c33ce1cd0c630，XML ID43767/seed788336489，入口源码3149，MenuLoaded行5347069 | 和首次失败分开 |
| 恢复检查 | planned-incident-recovery-test.txt 检查scene loaded/clean/root8后写sceneRestoredByFramework=true | 不含manager字段/稳定ID/组件数验证 |
| 后续资格 | qualification-v2与full按各自新捕获基线通过 | 证明各自范围；不能追溯证明第一次恢复瞬间的原组件完整状态 |

最小日志摘录为本目录 historical-menu-reload-log-extract.txt（原文件二进制LF行号，与rg -n一致；不使用Python splitlines对独立CR扩行）。源日志大小/当前SHA、包源码和场景指纹见 historical-recovery-evidence.json。日志中的Unity资源卸载行是原执行记录，并非本只读任务执行清理；不以此解释特定native ID消失原因。

磁盘场景 Assets/NTSD/Scene/NTSD_Menu.unity:9190-9237 明确保存：
- SoundManager GameObject fileID762446124，active1。
- MMSoundManager component fileID762446125，enabled1，script GUID614edb33c38bcf949846d5675f29f9fb。
- AutomaticallyUnparentOnAwake1，settingsSo GUID07ea3ff88e1ecb84d91f58aa804badc6/localfileID11400000。
- AudioSourcePoolSize10，PoolCanExpand1。
- Transform fileID762446126：无父、位置0、旋转单位、缩放1。

缺失的历史现场数据：原216430→sceneGUID/localfileID或GlobalObjectId映射；恢复后同一稳定序列化身份的manager个数、script/类型、active/enabled、上述设置、settingsSo身份、层级与缺失脚本。首次两个execute_code请求技术失败，没有产生这些快照。旧qualification-stages中的mmSoundManagerRestored=true与root缺失同时存在，证明该值只涵盖当时指针检查，不能升级为组件重建证明。

建议后继仅做一次当前现场非破坏只读验收：先查询idle/nonPlay/非编译；枚举已加载Menu中的真实MMSoundManager（含inactive），不用.Instance/初始化入口；读GlobalObjectId/sceneGUID/fileID、script GUID、上述SerializedObject字段和settingsSo身份、层级/active/enabled/缺失脚本。检查前后scene dirty/root与实例集合，不调用ApplyModifiedProperties/SetDirty/保存/重载/播放/销毁；未知或不一致则保留现场并报明，不自动恢复。此检查只能证明“当前与未变资产配置一致”，历史瞬间证据缺口仍保留。本轮没有执行。

源码所有权修复、修正资格及完整音频测量按原声明范围继续有效；不因这项证明边界重开旧实验。

