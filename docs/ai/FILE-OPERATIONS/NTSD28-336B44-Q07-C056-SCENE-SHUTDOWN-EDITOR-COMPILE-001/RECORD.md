# NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-EDITOR-COMPILE-001

状态：`PLANNED`。类型：原 Editor 脚本编译可能覆盖生成的程序集。Task/Change：`NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-001`。用户已启动战斗对齐并要求完成定向编译/运行验收；本操作只编译原项目脚本，不触及 DAT、图片、Scene、非战斗源文件或正式 EXE。执行者：`/root`；工作目录：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`。

执行前时间：2026-10-04T02:57:24.798446+08:00。逐文件清单：[PRE-MANIFEST.json](PRE-MANIFEST.json)。可预见被替换的两个 `Library/ScriptAssemblies` 程序集已按当前哈希复制到范围外 [precompile-backup](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-EDITOR-COMPILE-001/precompile-backup/) 并复算同 SHA。它们是被 Git 忽略的可再生成编译产物；备份用于精确恢复，不能以 HEAD 替代。该操作无文件删除、移动或项目资产覆盖。

拟执行入口：现有 Unity Editor（本项目） `refresh_unity(mode=force,scope=scripts,compile=request,wait_for_ready=false)`；之后读取原 Editor 编译状态与 Console。Unity 自身可能更新 `Library` 内部缓存，其完整动态路径预先不可枚举；本记录不将未列缓存误称逐文件备份完成。若需对当前用户生成缓存做精确恢复，必须先停止此操作并扩大清单。实际操作、实例、结果与前后哈希待追加。

执行开始：2026-10-04T03:02:20.508046+08:00。状态：`RUNNING`。原 Editor PID 待桥接返回；启动前两程序集 SHA 仍与 PRE-MANIFEST 相同。

执行结果：`VERIFIED`。原 Editor PID `105896`（已运行的本项目实例），初轮编译报新增探针 CS0136，修正新分支局部变量后再次 `refresh_unity(mode=force,scope=scripts,compile=request)`；原 Editor 完成程序集重载、回到 idle/非Play，Battle Scene `isDirty=false`。Unity 日志本次成功重载后 `error CS` 0、`Tundra build failed` 0，`Assembly-CSharp-Editor.dll` SHA 从 `C31FCF0CA0D7409A1A4CF5222726CA437F10BAB3CA8E8750909F4296E423F067` 改为 `31607177AC1103ADA72499904A35D96C2FE78D750755EBA186769DB03B9308B1`；`Assembly-CSharp.dll` SHA 保持 `8B4F63523C331AB6DCE85BBEDE211396C0207B667CAB7D01818BC5F153F8182A`。两个执行前备份 SHA 均与 PRE-MANIFEST 一致，未执行回滚。Battle/Menu 两个 Scene 磁盘 SHA 分别保持 `93448372...7BF60`/`F01144C9...16329F`；其他 Unity 内部缓存不在本逐文件恢复范围内。
