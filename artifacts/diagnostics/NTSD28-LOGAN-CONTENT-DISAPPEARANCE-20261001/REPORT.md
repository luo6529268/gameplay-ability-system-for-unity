# LoganRuntime 资源消失调查（2026-10-01）

状态：`CAUSE_UNATTRIBUTED / NO_RESTORE_PERFORMED`。只读调查，不把相关性当作删除原因。

15:42:46（本地时间）左右，`Assets/NTSD/Content/LoganRuntime/decoded_dat` 和多个 `vfs` 子目录的最后写入时间变化。调查时 `git status --porcelain=v1 -uall -- Assets/NTSD/Content/LoganRuntime` 报 2974 项**未暂存的工作区删除**：338 `.dat`、1018 `.png`、1617 `.meta`、1 `.txt`；Git 暂存区该路径无变化。Git 跟踪此树 3070 项，当前残余 96 项；其中 `vfs/c/hid` 的 13 张 PNG 仍在。项目自有 `Assets/NTSD/Config` 的 138 DAT 仍在。此前按用户要求隔离的 `artifacts/diagnostics/NTSD28-Q07-EXCLUDED-NATIVE-BG-MODE-001/for-user-deletion` 独立包仍在，包含 52 DAT；本次缺失不是该包的既定排除范围。

正式资源根 `J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime` 仍有对应文件。1018 张缺失 PNG 与 Git blob 逐字节一致；338 个缺失 DAT 与 Git blob 在 CRLF→LF 规范化后全部一致，样本 `a/ama/ama.dat` 已直接比较原始字节，差异仅换行，当前 Git `core.autocrlf=true`。这说明数据内容有可核恢复源，不说明删除者或触发命令。`.meta` 可从 Git 恢复既有 GUID；但删除前工作区已有 `vfs/a/ama/ama2.png.meta`、`vfs/c/ank/ank1.png.meta` 两处修改，直接从 HEAD 恢复不能保证保留这两处未提交值，必须单独核对。

本轮工具命令没有对 `Assets/NTSD/Content/LoganRuntime` 执行 `Remove-Item`、`git clean`、`git restore`、移动或覆盖；本轮新选中的 `NTSD28B5EffectActionOverrideEditorTests.UnarmoredEffectHorizontalResponse_UsesFacingNotRelativePosition` 仅建立内存中的两个实体并调用共用伤害 writer，测试体没有文件删除调用。现存同项目 Unity Editor 是旧进程退出后 Unity Hub 新启的进程。新进程 `Editor.log` 的首次 AssetDatabase 刷新报告 `Asset File Changes: ... deleted=1617`，与缺失 meta 数量一致，证明它**观察到**文件已缺失；该日志不提供发起删除的进程。PowerShell Operational 在15:35～15:46仅10条事件、无路径/删除命令命中；Security 文件访问审计不可用，未发现可给出进程归属的日志。旧 Editor 的大型日志随重启已被替换，无法据现存日志证明旧进程是否参与。

所以目前能确定的是“资源树发生了批量工作区删除，未提交且未触及 Git 历史”；**不能确定具体是谁、哪个进程或哪条命令执行**。特别不能把 Editor 刷新的 `deleted=1617` 误读成 Unity 主动删除。未执行资源恢复。Q07 的正式内容 raw/Scene 验证在此缺失期间不可继续。

2026-10-01 用户已明确批准先还原 `LoganRuntime` 下所有被删除的文件。恢复前已生成 `deleted-tracked-paths.txt`（精确 2974 条）及 `survivor-sha256-before.json`（现存 96 项的哈希），计划只恢复前者。执行 `git restore --worktree --pathspec-from-file=artifacts/diagnostics/NTSD28-LOGAN-CONTENT-DISAPPEARANCE-20261001/deleted-tracked-paths.txt` 时，命令启动前被自动审批拒绝：操作需要审批，但当前环境的 AskForApproval 为 Never。没有改用其他 shell、解释器、工具或复制方式绕过。拒绝后再次只读核对：2974 项仍为未暂存删除，清单 2974 个路径均不存在，目录内无其他 Git 状态项；恢复尚未发生。需审批策略允许该操作，或由用户自行在 Git 客户端恢复后，再做文件/GUID/原有 96 项哈希复核。

本次续查再次逐项计算现存96文件的SHA-256，均与 `survivor-sha256-before.json` 一致（0项改变/缺失）。C051生成C#工程编译0错、Change Ledger通过，均不解除正式资源缺失对原场景运行时验收的阻塞。

2026-10-01 16:13（+08:00）后续观测更正：`deleted-tracked-paths.txt` 的2974项当前全部存在，`git status --porcelain=v1 -- Assets/NTSD/Content/LoganRuntime` 无输出；原96项现存文件逐SHA-256均与保护快照相同。该恢复不是本任务执行，具体执行者、命令和时间未知；不能把此前被拒绝的命令补记为成功。当前文件身份已回到Git版本，但删除前 `ama2.png.meta`、`ank1.png.meta` 的未提交值是否得到保留仍未知。资源缺失的当前阻塞解除，不代表C051原Editor或Play验收已通过。

用户后续要求所有删除处理留痕，已建立 `FILE-REMOVAL-AUDIT-001` 合同、AGENTS第13.3节及 `docs/ai/FILE-OPERATIONS/INDEX.md`。本事件被纳入索引，原失败、拒绝与未知归因均保留。
