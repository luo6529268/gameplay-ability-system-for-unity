# Q01 正式/暂存候选跨根内容核验测试

状态：`VERIFIED_SCOPED_CANDIDATE_CONTENT_TEST`，不关闭 Q01、Q07、Q09 或最终目标。唯一规则权威仍为正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 和相应 `resources/runtime`；本包只修正 Unity Editor 测试口径，未改 DAT、PNG、生产加载器、Scene 或非战斗功能。

原 `NTSD28Q07StagedCandidateIdentityEditorTests` 将正式/暂存候选的原始对象定义、fusion 输入及其派生指纹要求相等。既有独立磁盘审计证明 330 对象中 20 份 DAT、另 8 份全局 DAT 中 5 份仅换行字节不同；生产身份指纹本来就应区分原始字节，因此跨根等号不能作为内容相同的测试。本次保留生产身份和各候选同根 freshness gate，只在这一份测试内逐对象按注册索引/ID/type/源路径及严格 CRLF→LF 后的 DAT 原始字节比较，逐候选图片按相对路径与原始 SHA-256 比较。fusion 选择为文件时也用同一严格字节归一比较，并保留解析语义指纹等号。没有把生产 `SemanticFingerprint` 重新命名或改算法；它派生自原始输入，不被冒充跨根语义等价证书。

生成 Editor 工程两次 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q -clp:ErrorsOnly` 均 exit0、0 error、273 warning。原 Editor 的 Menu Scene 在测试前后均 `isDirty=false`，脚本范围刷新成功。首轮具名 EditMode job `d085808ab559488383d59aa4d858f40d` 实际只执行1项，在旧 fusion 原始指纹断言失败；[原件](named-test-result.json)保留，不能称其通过。随后同脚本修正 fusion 测试口径，重编/刷新后第二轮 job `96e7743087e146fb953fb1c2615ed49e` [结果](named-test-result-v2.json) **1/1 Passed、0 Failed、0 Skipped**；测试完整执行 330 个对象和 906 张候选图片的跨根循环以及两端 `AssertInputsCurrent`。job 内 `progress.total=8812` 是 Editor 发现数，实际执行数以 `result.summary.total=1` 为准。无需为同一对象重复运行158角色专项、全量自检或全部场景。

保护核查：Battle/Menu/GameConfig/ProjectBattleModeConfig 前后 SHA-256 依次保持 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`、`5D79DBB7F3C6E9FF790413A8D6C0D9942A093F5D9E1B69FC351468C05EC0052D`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`；`LoganRuntime` 限定 Git 状态无新增差异。最终 Editor state 为 idle/nonPlay/no test，Menu 单场景干净。`git diff --check` 通过；最终 [Change Ledger validator](change-ledger-validation-final.txt) 报 `PASSED`、1198 条 Record（其它非当前代码路径的历史 warning 未当失败）。

这证明当前两根候选的对象 DAT 字节除 CRLF 外一致、候选图片逐 SHA 一致，且生产候选可读取并自检输入未过期。原始指纹差异真实保留；战斗 Game View、非对象图片消费者、全部角色自然可达与 Q01 整组仍需独立证据。
