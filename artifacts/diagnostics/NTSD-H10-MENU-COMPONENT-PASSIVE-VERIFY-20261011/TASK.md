# H10 当前 Menu 管理组件被动核验

Task: NTSD-H10-MENU-COMPONENT-PASSIVE-VERIFY-20261011
Status at creation: IN_PROGRESS
Authority: 当前 H10 持续授权及讨论会话 01a11fe2-bfaa-72f3-a9ed-c8260f63b193 的具名有限指令；本会话最新交接只允许本目录新增证据。

## 问题与决定

验证当前已加载 Menu 的 MMSoundManager 是否与未变的磁盘 Scene 配置相同。PASS 仅为 CURRENT_MENU_COMPONENT_MATCHES_SAVED_ASSET，不追溯证明历史恢复瞬间，也不是 H10、声音播放、H07、Player 或 Android 完成。不一致或工具无法可靠读取必需身份/字段时记录 FAIL/UNKNOWN 并结束，不修复现场或改验收标准。

## 最小步骤与范围

1. 冻结 saved Scene/meta、manager/base 脚本、settings 及此前八项源码/程序集指纹；只读核对 GUID、fileID、预期字段。
2. 现有唯一原项目 Editor 查询 idle、nonPlay、nonCompile、nonUpdating、无测试及 Menu loaded/clean。任何不安全/不可靠状态都停止，不切场景或等待后自动改变。
3. 只用现有被动工具；必要时至多一次准备好的 execute_code，只做内存查询。SerializedObject 只读，不调用 .Instance、初始化、callback、ApplyModifiedProperties、SetDirty、创建/销毁/音频加载/清理/Play/保存/重载。动态查询工具内部内存编译不等于项目编译；本任务禁止项目编译和持久 C#。
4. 稳定选择依据 Scene GUID + local fileID/GlobalObjectId。包括 inactive，只查询已加载 Scene 实例。比较实际类型/script GUID、数量、GO/component 身份、active/enabled、四项配置和 settings 资产身份、Transform/父子/缺失脚本。
5. 查询前后比较 SceneSetup、dirty/root、场景 GO/manager 实例集合及经源码确认不创建对象的 Current 指针。不读任何初始化属性；读不到时明确 UNKNOWN。
6. 保存原始响应、逐字段对照、前后指纹、实际命令与有限报告；不重复实验。

## 磁盘待复核基线

- Assets/NTSD/Scene/NTSD_Menu.unity，scene GUID 132c934aff207b048833ab33c1961278。
- SoundManager GO 762446124；Transform 762446126；MMSoundManager 762446125。
- script GUID 614edb33c38bcf949846d5675f29f9fb。
- active/enabled/AutomaticallyUnparentOnAwake/PoolCanExpand 均 1；AudioSourcePoolSize 10。
- settingsSo GUID 07ea3ff88e1ecb84d91f58aa804badc6 / localID 11400000。
- 根对象、无子、position 0、rotation (0,0,0,1)、scale 1。

## 写域与操作记录

唯一写域是本新目录；操作为 CREATE_NEW_ONLY，使用排他创建保留所有既有文件。只新增本 Task、JSON、查询文本、命令记录及 REPORT。已有源码、场景、配置、旧证据和治理文件零写域。没有替换/删除/移动，没有需要恢复的旧文件；不创建代码 Change Record，不运行 ChangeLedger 为没有代码改动的查询冒充验收。

风险是读取工具内部可能调用属性或运行状态变化。先审查现有工具和 passive Current getter；不使用通用任意属性序列化，不安装/修改插件。任何 query 失败保留原响应，停止进一步尝试修改现场。

无新增 runtime 状态/缓存/owner/容量/关闭阶段。查询仅局部 managed 容器，无 Unity 对象创建；不恢复旧自动 Goal、队列、H06 或其他优化点，不委派子代理，不读 Q06 方法体。最后以独立 RESULT/REPORT 记录终态，本 Task 创建记录保持原样。
