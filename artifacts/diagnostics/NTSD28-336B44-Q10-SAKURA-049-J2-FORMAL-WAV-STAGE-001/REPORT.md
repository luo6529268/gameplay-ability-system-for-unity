# Q10 小樱高血量 049/j2 正式战斗 WAV 暂存

状态：`FORMAL_CONTENT_STAGED / UNITY_IMPORT_AND_NATURAL_VOICE_PENDING`。本包只新增当前正式 `data/049.wav`、`c/saku/w/j2.wav` 及其 Unity `.meta`；DAT、旧 Sound、生产脚本、Scene、相机和非战斗路径均未改。根正式 EXE 本轮复核 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

触发依据是[同输入正式根回访](../NTSD28-336B44-Q10-SAKURA-HIGHHP-049-J2-ROOT-001/REPORT.md)：小樱 HP500、action0 的既有 55 tick 防→纵深上→攻 LFR 在根正式 EXE 退出0/PASS，根动作/MP/相机X与当前 playable 源码 165/165 同值；源码 tick21 发 j2、tick49 发049。根公开 trace 无 audio 字段，不声称根设备发声。正式049和旧Sound PCM不同，j2在两Unity查找根均缺，故这两条有具体内容缺口。

[前置清单](preflight.json)确认两目标 WAV/meta 不存在，正式源及旧Sound状态、四保护文件 SHA 已记录。以独占新建方式复制正式WAV，沿用各目录现有 AudioImporter 模板生成唯一 GUID meta，没有覆盖或移动文件。[后置复核](postflight.json)显示 `data/049.wav` 正式/Unity 原始 SHA 同为 `10ADC639123FA69CBB64DD430BE49D04314C50260D383FC22E98513D6E68D0C6`，`c/saku/w/j2.wav` 同为 `73C3904DB1151FA86727860A4D6BD8B7FA52C70A5209D6B0211C59803780A908`；声道/采样位宽/采样率/帧数分别同正式源，两个GUID各在Assets中仅出现一次。旧049与不存在的旧j2状态未变，Battle/Menu/两配置SHA全部同前。正式暂存战斗WAV现10份，10/10与正式VFS原始文件SHA相同；这不是完整970词法cue覆盖证明。

原 Editor 仍非Play、无测试、Battle Scene内存 `isDirty=false`，但持续 `is_compiling=true`、无编译完成时间。本包没有发Refresh、Test Runner或Play，无法宣称Unity已导入、battle-only正式SourcePath已选、clip/voice播放、Mixer PCM、设备听感或与正式EXE扬声器同态。Editor恢复后沿既有高血量55 tick链查 tick21/49 的实际battle voice与有序退出；Q10、Q12与总目标开放。范围和下一验证见[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q10-SAKURA-049-J2-FORMAL-WAV-STAGE-001.md)。

交付核对：本包后 `git diff --check` 退出0（Git仅提示工作树换行规范化警告），`Tools/Validate-ChangeLedger.ps1` 退出0/PASSED（1227 Records、28个受治理代码差异文件）。本轮没有新增或修改脚本；上述检查不能代替原Editor音频导入与Play。
