# 第一批优化文件操作记录

Operation ID：`NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006`
状态：`VERIFIED`（限定本包文件修订/留痕；不等于父H-11或Android通过）
用户授权：“那么开始按照优化文档来进行优化吧，并且用一个单独的进度总表文档来更新所有优化点的进度”。
执行者：Codex /root；工作目录：`I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity`。
开始前：2026-10-06T18:30:38.8815454+08:00；固定恢复commit：`5a5cde34b9685739326b638f9c5550b69eda7ef6`。
Task：NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006；Change：NTSD-OPT-H11-CACHE-PREWARM-001。

仅H-11第一子批：接入现有DisplayMotion预热、预热现有chunk描述数组、聚焦测试；
新增34项独立进度总表；治理文档只追加本包并同步当前状态。
不修改模拟/33ms/排序/UV/shader/segment语义/Scene/InputActions/资源/ProjectSettings/Server；
EXT-1和MONO专项不自动解冻。不删除/移动/清理/提交/push。

## 精确原位修订清单

下列文件在该快照时已跟踪且工作树干净，可从固定commit/path读取原始内容；
后续回滚需另获批准，精确逆向补丁，不用reset/checkout/restore。

| 相对路径（绝对路径=工作目录/相对路径） | 字节 | before SHA-256 |
|---|---:|---|
| `Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs` | 97191 | `9EB25754712EBC31BCA08437D965320DE845D51CA39B698BFDD45F8F0E6B293F` |
| `Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs` | 40771 | `D51F85E9D3CD2144C5CBE811A7DB547451CBF1FBBF246A8399A990C2CB142D8A` |
| `Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md` | 16686 | `AAE81CCB1C04E1C6B13BB8A446518F961F48FACEBB7FA828230B4943D82E82D3` |
| `Assets/NTSD/Docs/battle-optimization-rebaseline-and-start-gates-20261006.md` | 11407 | `3F68E0E25197D7F4490621E998B830692C5591518A53A806E12DC05428F2B1E9` |
| `Assets/NTSD/Docs/android-mobile-readiness/h-11-presentation-capacity-zero-gc.md` | 4189 | `AFDC3CBB79ECB347611F9D52BDCEA53E2703AAAAAA26566EBAB8A0D3548FCB8F` |
| `docs/ai/CHANGE-LEDGER.md` | 630138 | `CE4132CD23C815F8613F394B296639B55A3338C117D492E39F6C0788712AC9A3` |
| `docs/ai/STATE.md` | 1530141 | `E0664F23F95AAEB3D636A2E1A571C1AA65E0FB65C57ADCBC7BF80328567D1797` |
| `Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md` | 1460324 | `2D87EF13AA792AFA51404A152068B262090BAAE052346D1297E4556D10F40440` |
| `docs/ai/FILE-OPERATIONS/INDEX.md` | 22671 | `B83D773841004C561F46CC339B9293D887FD9E0CE7E10A4D29F50678090D17F2` |

## 创建清单

以下文件创建前确认不存在：
- `Assets/NTSD/Docs/battle-optimization-progress-tracker.md`
- `Assets/NTSD/Scripts/Test/Editor/BattlePresentationCapacityPrewarmEditorTests.cs`
- `docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006.md`
- `docs/ai/CHANGE-RECORDS/NTSD-OPT-H11-CACHE-PREWARM-001.md`
- `artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006/REPORT.md`
本Operation的RECORD.md、before.json、after.json、验证/测试输出在本目录或具名诊断目录create-only。
Progress.md.meta与新test.cs.meta可能由现有Editor自动创建，保留并登记，不手工写GUID。
聚焦测试局部内存Mesh/Frame，finally按既有Dispose，不删除项目文件。
Editor可连接时通过既有MCP工具刷新/编译/精确EditMode测试和只读pipeline/Scene状态；
测试框架可能生成Library/Temp输出，不清理或覆盖已有结果，使用唯一job/output。
Unity CLI status无Pipeline实例≠Editor未运行：pipeline list确认PID19040、Unity2022、无Pipeline；
MCP HTTP8080连接失败，现有TCP6402仅连接既有工具，不装包、不启动第二Editor。

## 拟执行入口与验收

- 只读rg/Get-Content/Get-FileHash/git status/show/diff；本地编辑全部apply_patch。
- 原Editor existing UnityConnection(host=127.0.0.1,port=6402)：只读预检；
  精确测试/刷新参数和实际返回另录原件；busy/Play不干扰，不保存/切换Scene。
- Tools/Validate-ChangeLedger.ps1；scoped git diff --check；文档ID/链接检查。
- test-first：中央入口高slot数组预热、首次/后续segment高水位描述缓存身份、
  4096跨chunk保守预热及尾部几何回归。
- 不将数组预热小批PASS晋升H-11完整0GC、M0或Android证书。
  硬上限/seal/overflow/full-path仍后续子批；不更改现有拒绝策略。

## 保护快照

- `Assets/NTSD/Scene/NTSD_Menu.unity` SHA-256 `6B5BAD6DB12E2C5FA3324F275FE87E788C27B0F0B562E2637B92BDDD6DD204AE`
- `Assets/NTSD/Scene/NTSD_Battle.unity` SHA-256 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`
- `ProjectSettings/ProjectSettings.asset` SHA-256 `25475E98DDF5AE903C1CE4685353715F843D004485D088E0BED33BEA98792338`
- `Packages/manifest.json` SHA-256 `335747D833A5CAC9B58B5F6D910E11750457B58A040154CF736D07875C831AFF`
- `Assets/NTSD/Shaders/BattleCentralTransparent.shader` SHA-256 `8E76494C31681B25DC5D1F55C83F8DD2DCFCAADC7918FC0CD029EB6BBFD945D7`
- `Assets/NTSD/Shaders/BattleCentralTransparentArray.shader` SHA-256 `010D34A0F3C1EB5A55366621EF3336604E13B5A7A2F967390CFE3CA777D24639`
- `Assets/NTSD/Docs/battle-optimization-proposals-index.md` SHA-256 `DE327708E00ED58291F66626B712488A84BB7BAE26452D996F4E4EC1208BB234`
- `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs` SHA-256 `200816F8ACBD15CF2C7920071C8D2AAF2E1106F8225A121AF15E4AA0A764D5B5`

## 执行后

2026-10-06 已执行apply_patch仅上述清单与本包新增文件；无删除/移动/清理/提交/push。
原Editor TCP6402，刷新/请求编译及精确EditMode：RED job117da49c9b9340b6a02cde0683ddecaf
执行7项/6个预期失败；GREEN两旧class jobe9e9a108d17c4512850c3d472c81ee95 8/8，
新class job2bc5c122aa65405ba5066eba0a8fcd57 7/7；namespace误填导致首个GREEN仅旧8项，
纠正为NTSD.Test后另跑新7项，失败/请求/结果原件全部保留。
Assembly-CSharp/Editor原Editor编译18:38:09/10；最新get_editor_state非Play/idle，
manage_scene Menu isDirty=false，read_console error CS0，无Scene切换或保存。
Tools/Validate-ChangeLedger.ps1 exit0/3个治理code-path覆盖；历史4251 warnings不清理。
差异/34项唯一ID/高12中14低8/本批文档链接和保护SHA另见after.json及诊断报告。
初次diff --check发现本包追加治理段落多了EOF空行，仅修正本包新增空行后重查；
不把该失败或测试请求缺跑抹去。自动生成progress.md.meta/test.cs.meta原样保留并记账。
操作后逐文件SHA和保护8项、现有JSONL保留情况登记after.json；after清单自身不递归哈希。
未运行完整M0、真实Battle Play/关闭重进、GPU/Player/Android或设备测量，父H-11保持开放。
after快照首次返回因工具输出预算截断而无法解析，未写after文件；提高读取预算后重新生成，
没有覆盖已有结果或改变项目内容。

