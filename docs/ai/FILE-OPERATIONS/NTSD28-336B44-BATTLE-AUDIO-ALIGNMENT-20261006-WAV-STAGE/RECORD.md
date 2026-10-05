# NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-WAV-STAGE
状态 RUNNING。开始 2026-10-05T19:01:17.385831+00:00。执行者 /root。
用户授权2026-10-06角色与战斗全部音效修复；Task NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006。
范围 manifest-planned.json 978个实际battle/项目KO引用（977DAT/sound.dat并集+项目Asset m_join），只新增缺失WAV/meta与文件夹meta；12个既有WAV逐SHA相同保留，禁止覆盖/删除旧资源。
中文完整981对应正式981全SHA同；未被battle/项目配置引用的 m_cancel/m_end/m_pass不部署；BGM不部署。
拟执行 Python exclusive-create 从中文文件读取 bytes，原相对路径映射入统一LoganRuntime/vfs。新AudioImporter PCM保留双声道/原采样率；既有GUID/meta不改。
无丢弃内容，不需要旧WAV备份；治理索引修改已有PREPARE备份。Unity Refresh负责Library派生导入缓存，原Editor19040/6401，不启动第二Editor。
恢复仅移除本次新增manifest项，须再获删除授权并留记录；本次无删除。

原执行结尾校验表达式TypeError，保留失败，不重复复制。按实际manifest复核所有WAV SHA/.meta均在位：{"operation": "NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-WAV-STAGE", "ended": "2026-10-05T19:02:18.855606+00:00", "mapped_source_total": 981, "battle_required_total": 978, "existing_kept": 12, "new_wav": 966, "new_wav_metas": 966, "new_folder_metas": 53, "all_destinations_sha_equal": true, "initial_validation_error": "TypeError at final verification expression root[e['destination']] after successful staging; no writes retried. Rechecked actual manifest destinations using root / path."}。状态 VERIFIED，仅部署身份。

记录措辞更正：新AudioImporter沿用既有078 meta模板（compressionFormat:1）；没有宣称Unity导入缓存为PCM编码。生产播放器通过UnityWebRequest按原WAV文件解码，原采样率/声道/样本数由实际加载测试验收。WAV原字节逐SHA相同，原12meta/GUID不变。
