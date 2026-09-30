# Unity 原项目缺 hid6 资源根准备（非 Play 验收）

状态：`FIXTURE_PREPARED / BATTLE_PLAY_PENDING`。此文件只记录隔离资源根的构造，不能作为原 Battle Scene 发布、选帧或像素验收。

原项目现有内容根 `Assets/NTSD/Content/LoganRuntime` 保持原状。另在项目 `Temp/NTSD28Q09P20MissingHid6UnityRoot20260929` 新建诊断根：`catalog.csv` 是副本，`decoded_dat` 及除 `c/hid` 外的 VFS 目录使用指向原内容的目录连接，`c/hid` 仅复制除 `hid6.png` 外的九张 PNG。九张副本逐文件 SHA-256 与原内容一致，目标 `vfs/c/hid/hid6.png` 不存在；`vfs/c` 有 57 个角色目录连接。没有删除、移动或覆盖原项目的任何 DAT、PNG、Scene、Config 或角色资源。

后续如做原 Editor Battle Play，须使用专门的未保存 GameConfig 副本选择此根，进入 Play 前校验副本及目标缺图状态，退出后精确归还保存的 Asset 和完整内容发布，并再次核对 Scene/配置 SHA。不得将此诊断根设为项目正式内容根，也不得把目前的 Manager EditMode 聚焦通过写成真实 Battle Play。
