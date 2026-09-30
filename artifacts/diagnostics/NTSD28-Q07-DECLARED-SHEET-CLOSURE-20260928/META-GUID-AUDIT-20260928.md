# Q07 已声明图集的 Unity 导入身份补核

状态：`VERIFIED_STATIC_META_GUID_ONLY / Q07_OPEN`。只读核对上一份 `declared-sheet-sha.json` 中的 703 个不同 Sprite Sheet 路径；该清单 SHA-256 为 `58555F8D2A24B8E08C5259272A61B267BF156DFCD23128D2386A5D9D0AF3DA4E`。

对每个 `Assets/NTSD/Content/LoganRuntime/vfs/<path>` 检查同路径 `.png.meta`，按整行 `guid: <32位十六进制>` 读取 GUID；然后扫描 `Assets/**/*.meta`，检查这 703 个 GUID 是否还被其他资产使用。结果：**703/703 `.meta` 存在，703/703 GUID 格式有效，703 个 GUID 互不重复，Assets 全目录中这 703 个 GUID 的跨文件碰撞为 0**。检查器只读文件并输出计数，退出码 0；没有修改导入设置、资源或 Scene。

这补上已声明图片的磁盘 `.meta` 身份门，不等于 Unity 实际导入成功、运行时 `BattleSpriteCatalog` 发布了全部帧、场景中的自然技能能使用全部图片，更不等于 GPU 画面与正式 EXE 一致。OID434/action396 的自然物理 J 续段实体 `(434,36)` 取图已由 `NTSD28-Q07-OID434-NATURAL-ATTACK-TAIL-001/ACCEPTANCE-20260925.md` 独立限定通过；早先未追加攻击的负分支不再算作此项待办。下一 Q07 动作仍只针对新证明可达的运行时引用首差或 D-024 共用碰撞域，已过实体取图、文件 SHA 和 GUID 门不重复运行。Q08 独立，Q09 画面另行验收；旧 521 项保留且删除授权不变。
