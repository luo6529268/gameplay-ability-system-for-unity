# TMP-VERTEX-EFFECTS-SHARED-20261008 文件操作记录

状态：PREPARED。类型：已授权的定向编辑及新资源创建，无删除或移动。
需求/授权：用户在本聊天明确要求不同描边共用材质。
执行者：当前 Codex 聊天。目标根：I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity；副本根：I:/PoXiao_Main/PoXiao_Main/TextMeshProPortable。
现有文件逐项范围、SHA-256、准确备份：before.json；操作前 Git 状态：git-status-before.txt。备份均已核验哈希相同。
计划 API：apply_patch 最小补丁；Add-Content 仅追加本任务记录；新增 Resource Material/meta；隔离临时项目复制和新 ZIP 不覆盖旧文件。
回滚来源：before/ 中的准确文件内容，以差异补丁恢复，先取得用户要求的回滚授权。
Change Record：../../CHANGE-RECORDS/TMP-VERTEX-EFFECTS-SHARED-20261008.md。

最终状态：VERIFIED，限定本次文件操作和共享材质行为。
开始：2026-10-08 09:52:53 +08:00（operation目录创建时间）；结束：2026-10-08 10:09:08 +08:00。
实际操作：apply_patch 修改声明的 TextMeshProUV、配套Shader及README；Copy-Item 同步到事前备份的portable placeholder并新增Material/meta；Compress-Archive 新建20261008包；Add-Content只追加本Change的状态及审计链接。无文件删除、移动、Git丢弃或push。
逐项结果：after.json；源与便携副本的4个产品文件SHA-256逐项相同，旧脚本/Shader GUID保留。
验证命令及原始证据：../../../../artifacts/diagnostics/TMP-VERTEX-EFFECTS-SHARED-20261008/ 下run01（根）、run02/03失败保留，run04实际渲染及所有行为项成功（Unity CLI exit0）；编译Editor/Player exit0；change-ledger-final.log成功/exit0，diffcheck exit0。
校验限制：validator只覆盖NTSD/Tools，第三方TMP代码依照Change Record package-code-path单独登记；不声称已测当前项目实际场景、URP画面、最终Draw Call或Player Build。
