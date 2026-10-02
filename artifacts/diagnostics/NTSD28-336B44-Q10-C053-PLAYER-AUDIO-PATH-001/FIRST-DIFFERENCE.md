# Q10/C053 Windows Player 音频文件与路径首差

2026-10-02 只读审计。正式战斗权威仍为根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable 内容；用户排除原版背景和两类 mode DAT，旧 `Sound` 文件及非战斗播放保留。

当前 `Assets/NTSD/Content/LoganRuntime` 有 1373 个非 meta 文件、46,927,691 字节。Windows postbuild 的 v2 清单只有 1371 行、46,883,057 字节；其中两行用户排除的 `decoded_dat/data/mode.dat`、`decoded_dat/data/mode/ntsd.dat` 已不在实际源目录，另有 `bgm.dat`、`sound.dat` 与本轮新部署的 `vfs/data/020.wav`、`067.wav` 四个清单外文件。其余 23 个旧清单行字节数或 SHA 与现状不同。因此 `NTSD28Q07WindowsContentBuildProcessor.VerifySourceSet` 会因缺项/多项失败；不能只把常量从 1371 改成 1373。

对实际 1373 文件逐一与当前正式 `resources/runtime` 同相对路径比较：全部正式路径存在，1348 个原始 SHA 相同，25 个 DAT 仅 CRLF/LF 换行不同；把 CRLF 严格变为 LF 后 25/25 原始字节序列一致。没有发现数值、token 或其它内容差异。本包不改任何 DAT 数据或旧 v2 清单。

即使补齐打包清单，现行 `NTSDSoundPlayer.ResolveFormalBattleSoundSourcePath` 以 `Application.dataPath/NTSD/Content/LoganRuntime/vfs` 为根；Windows Player 的 `Application.dataPath` 是 `<EXE>_Data`，而现行 postbuild 把正式内容放在 EXE 同级的 `Assets/NTSD/Content/LoganRuntime`。所以 Player 端不会取到正式 020/067，战斗 cue 会回退到旧路径。现有 `GameConfig.BattleContentRuntimeRoot=Assets/NTSD/Content/LoganRuntime` 与 `CharacterAnimtorManager` 的内容根解析使用 `Application.dataPath/..`，可作为统一根。

下一包应保留 v2 作为历史，新建当前实际 1373 文件的版本化清单，按正式源与现状逐 SHA/换行等价校验；更新 Windows postbuild 的清单、计数和字节总数；让战斗音频从已配置的正式内容根读取；再用原 Editor 编译、聚焦测试、唯一输出 Windows Player 构建及独立运行结果验证。现有其它平台的原始文件部署本来未闭，不把 Windows 结果扩大为 Android/WebGL。
