# Q01 正式 type-0 DAT 与角色图片部署限定核验（2026-10-02）

状态：`TYPE0_158_DEPLOYMENT_EDITMODE_PASS / RAW_OID77_DAT_SHA_DIFFERENCE_PRESERVED / Q01_OPEN`。当前正式根 EXE SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本包仅修改 `CharacterAssetDeploymentEditorTests.cs` 一份 Editor 测试，没有修改正式或 Unity 暂存 DAT、PNG、生产脚本、Scene、模式 Asset 或非战斗功能。

前次测试在 OID77 `c/kar/kar.dat` 原始 SHA 相等断言处截断。独立 [type-0 逐文件字节审计](type0-byte-audit.json)（SHA-256 `33C5EA5F299D6E3DB5A26E9EE030983A9F2994CB0BEA79346A8BBED4C994AC07`）从当前正式 `catalog.csv` 的 158 个 type-0 索引与既有暂存清单出发，重新读取两边实际 DAT 字节：157/158 原始相同，158/158 在**只把 CRLF 转为 LF**后相同。唯一原始不同的是 OID77；正式 SHA `987696ABE0D2E562C56DC0265E999B7D37443B55324310A7A5D47D15AEAC66FF`，暂存 SHA `A4500E6B40B33667460C11ABE3C47F7F5B8F07C1DCEBF39A74A9AF0E2CE21F62`。该差异是真实原始身份差，不把它写成原始字节一致。

测试的 DAT 断言现在先比较已有原始 SHA；仅当 SHA 不同时，重新读两边文件字节，删除每个 CRLF 配对中的 CR 后逐 byte 比较。其他空白、数值、token、路径与图片差异不会被此规则吞掉。原测试仍对正式/暂存两端各330个索引对象执行 `BuildCharacterFrameConfigsFromCatalog`，再逐一检查158个 type-0角色的 catalog 位置、路径、发布目录、解析帧数、声明图片路径及所有声明图片的**原始字节 SHA**；没有改变生产身份或缓存指纹。

- 最终生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q`：0 error、251 existing warning，原始日志 [generated-editor-build.log](generated-editor-build.log)。原 Unity 2022.3.62f3 Editor 重新编入修改后的脚本。
- 只运行 `NTSD.Test.CharacterAssetDeploymentEditorTests.FormalTypeZeroCharacterDatAndImagesAreDeployed`，job `3e70cde305c8445d84b026633cfdff14`，`succeeded`、总1/通过1/失败0/跳过0、17.06秒。[紧凑结果](focused-test-summary.json) SHA `70CB098843D82630AE35B2105708A9FE41AC48E77E375F79228926A05FFC5877`；[原始作业结果](focused-test-job.json) SHA `A7FB5DBC32A5E30A904598A8CF6BE84CB23FFE41E6CC9D07744B48A87CA4ABC3`。前次 OID77 FAIL 仍保留在此前 Q01 报告及原始结果中，不被本次覆盖。
- 测试后原 Editor `idle`、非 Play、非编译、无测试作业；活动 `NTSD_Battle.unity` 为 `isDirty=false`、rootCount13。Battle/Menu/GameConfig/ProjectBattleModeConfig 四个磁盘 SHA 分别保持 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`、`DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`；`LoganRuntime` 限定 Git 状态为空。

这个结果关闭**当前 158 名 type-0 角色的 DAT 换行等价、帧数与声明图片磁盘身份测试出口**。它不证明原始 DAT 指纹相同、Game View 图片实际像素、非对象 `resource.dat`/`system.dat` 图片消费者、全部 Q01 或整个战斗对齐。Q01/Q07/Q09/Q12 仍按新版总表各自出口继续。
