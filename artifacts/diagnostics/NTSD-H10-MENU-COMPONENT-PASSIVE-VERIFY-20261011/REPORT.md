# H10 当前 Menu 管理组件被动核验结果

Task：NTSD-H10-MENU-COMPONENT-PASSIVE-VERIFY-20261011。

**结果：UNKNOWN_TOOL_QUERY_UNAVAILABLE；有限任务已停止。** 原 Editor 当前安全，Menu loaded/clean/root8；根列表内存在实际类型为 `MoreMountains.Tools.MMSoundManager` 的组件。但用于读取稳定身份和精确序列化字段的唯一一次查询在工具启动编译器时失败，不能给出 `CURRENT_MENU_COMPONENT_MATCHES_SAVED_ASSET`。

本轮未发现足以判定组件损坏的证据；UNKNOWN 是缺少必需读取结果，不是配置不一致。没有修改插件、脚本或现场来绕过工具限制，没有换编译器重试。

## 已观察事实

现有唯一原项目 Editor 为 PID39108，Unity2022.3.62f3。通过既有 TCP127.0.0.1:6402 读取；未启动 Editor、项目编译、测试、Play 或音频准备。

查询前后 `get_editor_state` 均为 idle，nonPlay/nonPaused/nonChanging、nonCompile、无 domain reload pending、nonUpdating、无测试。`manage_scene get_loaded_scenes` 均仅返回 `Assets/NTSD/Scene/NTSD_Menu.unity`，loaded/active、dirty=false、8 roots。两份根列表包括 ID、active、类型摘要、层级和 transform 摘要，前后完整相等。

当前根列表中的同名候选对象为 SoundManager，native GO ID224644，Transform ID224648，MMSoundManager ID224646，实际组件类型 `MoreMountains.Tools.MMSoundManager`；两次只读组件列表完全一致。该对象 activeSelf/activeInHierarchy=true，为根对象、childCount0，摘要 position(0,0,0)、Euler rotation(0,0,0)、scale(1,1,1)。**这些 native ID 仅标识当前观察到的候选，尚未与 Scene stable fileID 建立映射；旧 ID216430 未用于选择或推断。** 组件列表过滤 null 项，因此不能据其两个正常组件推断 missing-script 数为0。

## 逐项判定

完整机器对照见 [field-comparison.json](field-comparison.json)。

| 检查 | 磁盘基线 | 当前内存证据 |
|---|---|---|
| 当前 Scene GUID | 132c934aff207b048833ab33c1961278 | 一致，仅 Scene 身份 PASS |
| GO / component / Transform localID | 762446124 / 762446125 / 762446126 | UNKNOWN；查询未执行 |
| 实际组件类型 | MoreMountains.Tools.MMSoundManager | 同名根候选类型一致；未建立稳定身份 |
| script GUID | 614edb33c38bcf949846d5675f29f9fb | UNKNOWN |
| GO active | 1 | 同名根候选为 true |
| enabled | 1 | UNKNOWN |
| AutomaticallyUnparentOnAwake | 1 | UNKNOWN |
| AudioSourcePoolSize / PoolCanExpand | 10 / 1 | UNKNOWN |
| settingsSo GUID / localID | 07ea3ff88e1ecb84d91f58aa804badc6 / 11400000 | UNKNOWN |
| 父子 / Transform | 根、无子、local position0 / quaternion(0,0,0,1) / scale1 | 候选根/childCount/Transform 摘要相符；没有原始序列化字段及 quaternion |
| 匹配稳定身份的 manager 数 / missing scripts | 1 / 0 | UNKNOWN |
| 完整 SceneSetup、所有场景 GO/manager 集合、Current 前后 | 应无变化 | UNKNOWN；拟通过同一查询取两份快照，查询未执行 |

`MMPersistentSingleton<T>.Current => _instance` 已从当前源码确认没有创建副作用；本轮实际未取得其指针值。没有调用 `.Instance` 或初始化属性。未使用通用组件属性序列化，因为现有 serializer 会调用任意属性 getter；实际组件列表明确传 `include_properties=false`。

## 查询失败与实际命令

唯一 `execute_code` 参数：`action=execute`、`safety_checks=true`、默认 auto compiler，完整读取代码保存为 [passive-query.cs.txt](passive-query.cs.txt)。代码计划只读 SceneSetup、含 inactive 的已加载 Scene 对象、GlobalObjectId、指定 SerializedObject 字段、Current，并在同次同步查询内比较前后快照；没有写属性、创建 Unity 对象、回调、保存或清理调用。此文本不在 Assets 中，也未加入项目编译。

原始响应 [passive-query-response.json](passive-query-response.json)：外层 transport `status=success`，但内层 `result.success=false`，错误为启动现有 Mono 编译器时“文件名或扩展名太长”。不能把 transport success 当查询 PASS。根据工具的 compile-then-invoke 路径，本次未进入读取代码，未产生预期字段/集合快照。与历史类似失败分别保留，本轮没有再次请求 Roslyn 或安装工具。

实际9次 Editor 工具调用按序完整保存在 [commands.jsonl](commands.jsonl)：

1. `get_editor_state {}`。
2. `manage_scene {action:get_loaded_scenes}`。
3. `manage_scene {action:get_hierarchy,page_size:100,include_transform:true}`。
4. `get_gameobject_components {instanceID:224644,include_properties:false}`。
5. 上述唯一 `execute_code`。
6. 第4项同参数，取得后快照。
7. 第3项同参数，取得后快照。
8. 第2项同参数，取得后快照。
9. `get_editor_state {}`。

原进程路径只读核对使用 `Get-CimInstance Win32_Process`；文件指纹用 SHA256，Git只读命令为 `git status --short --branch`、`git rev-parse HEAD`、`git diff --cached --binary`、`git diff --binary`。本轮没有项目代码改动，因此未运行 ChangeLedger、编译或测试，也不将旧 validator PASS 当本轮验收。

## 磁盘与现场保护

[disk-before.json](disk-before.json) 冻结21项精确文件；此前完整音频任务的八项源码/程序集 SHA 全部相同。Menu YAML9190–9237、Scene meta、manager script meta、settings asset/meta 的预期 ID/字段均已重新核对。查找源码只有本任务必要的现有工具、manager/base 与 Scene 片段，没有 Q06 方法体读取或子代理。

[RESULT.json](RESULT.json) 保存全部终态比较：21项具名文件 SHA 无漂移，HEAD、暂存 diff、tracked diff 与本 Task 外 Git 状态不变；旧报告未改写。实际只新增本目录 Task、读取代码文本和证据。本轮未删除、替换、恢复、清理旧数据或更改任何项目源码、Scene、资源、Settings。

现场无变化的直接证据限于 loaded Scene/dirty/root 摘要与该候选组件列表，均前后相同；由于查询失败，**不能宣称整个场景所有对象、组件字段或完整实例集合已逐项证明无变化**。磁盘字节相同与工具实际无写操作不能补造这份缺失的内存快照。

## 证据复用及交回

此前音频所有权、唯一完整目录两轮218.31MiB账本及释放证据按原范围复用，没有重新加载或测量。当前核验即使将来 PASS，也只能证明当时当前组件与 saved Scene 一致，不能追溯证明旧失败后恢复瞬间的完整内存状态；历史恢复边界及禁读越界记录继续保留。

本次停在工具能力缺口。后继需讨论会话决定取得指定稳定ID和序列化字段的合法读取方式；本 Task 不修改插件或新增测试绕过，也不以同名对象/根数/clean 降低门槛。H10 仍待合法声音来源覆盖与预算边界审阅，完整预加载保持；不新增 cue、subset 或预算阈值，不推进其他优化点，不恢复旧 Goal/队列，H07/Player/Android 状态不扩大。
