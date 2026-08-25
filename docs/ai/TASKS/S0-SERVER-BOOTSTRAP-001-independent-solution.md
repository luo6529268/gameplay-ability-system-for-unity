# Task Contract — S0-SERVER-BOOTSTRAP-001

> 状态：`FOCUSED_TEST_PASS / SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`  
> 所属阶段：服务器优先基础包；S0 服务器侧准备  
> 建立日期：2026-08-24

> 留痕说明：Server 仓库已在修改首个 Server 脚本前建立 `NTSD_Server/docs/ai/CHANGE-RECORDS/S0-SERVER-BOOTSTRAP-001.md`，并拥有自己的 Ledger/State/Handoff。本 Client 仓库保留此上位合同与进度引用，不伪造外部代码路径的 Change Record。

## 1. 目标

在 `I:\GitHub\Unity_GAS\NTSD_Server` 建立独立 Git 仓库与可重复构建的 .NET 10 LTS solution，为后续服务器协议、权威帧、房间 tick、journal/checksum、配置日志、错误隔离和健康检查提供清晰模块边界。该包只搭建服务器工程与治理骨架，不修改、编译或验证 Unity Client，也不实现真实 Socket、数据库或正式 NTSD BattleKernel。

## 2. 环境与版本合同

- SDK：`.NET 10 LTS`；`global.json` 基线 `10.0.100`，`rollForward=latestFeature`，只允许在 10.0 feature band/patch 范围滚动。
- 选择依据：微软 2026-07-14 支持表中 .NET 10 为 Active LTS，结束支持日期 2028-11-14；本机现有 .NET 8/9 均在 2026-11-10 结束支持，不作为新生产服务端基线。
- 当前本机事实：已安装 3.1、8.0、9.0 SDK；未安装 10.0 SDK。
- 当前目录事实（初始）：`I:\GitHub\Unity_GAS\NTSD_Server` 尚不存在，且不在当前 Codex 可写 workspace roots。
- 更新事实（2026-08-24）：用户已创建目标目录并在 user-level config 添加 writable root；活动任务未重载，实际 `New-Item`/`git init` 仍遭 access denied，且未产生 Server 文件。
- 不允许自动安装全局 SDK、不修改系统 PATH、不写用户 profile；用户完成 SDK/工作区准备后再运行构建。

## 3. 允许创建的服务器结构

```text
I:\GitHub\Unity_GAS\NTSD_Server\
  .gitignore
  AGENTS.md
  README.md
  global.json
  Directory.Build.props
  Directory.Packages.props
  NTSD.Server.sln
  src/
    NTSD.Battle.Protocol/
    NTSD.Battle.Kernel.Abstractions/
    NTSD.Server.BattleHost/
    NTSD.Server.Configuration/
    NTSD.Server.Observability/
    NTSD.Server.Hosting/
  tests/
    NTSD.Battle.Protocol.Tests/
    NTSD.Server.BattleHost.Tests/
    NTSD.Server.ArchitectureTests/
    NTSD.Server.IntegrationTests/
  scripts/
    bootstrap.ps1
    build.ps1
    test.ps1
    run-local.ps1
  config/
  deploy/
  docs/ai/
    CHANGE-LEDGER.md
    CHANGE-RECORDS/
    STATE.md
```

## 4. 依赖与模块边界

- `Protocol` 与 `Kernel.Abstractions` 必须保持无 Unity、无 Host、无 DB、无 transport 依赖；未来允许提供 Unity 可消费的 `netstandard2.1` 产物，但本包不接客户端。
- `BattleHost` 只依赖 Protocol/Kernel abstractions；它拥有 room 顺序执行边界，不拥有渲染或持久化真相。
- `Configuration`、`Observability` 和 `Hosting` 不得反向渗入 Protocol/Kernel。
- 不创建无约束的 `Common` 项目；共享类型必须有明确 owner。
- 不复制 Unity 现有 `SimulationWorld` 或战斗规则到 Server 仓库。
- 可创建 `TestKernel` 仅供服务器测试；命名、命名空间和文档必须明确其不是正式 NTSD BattleKernel。

## 5. 工程质量合同

- `nullable`、隐式 using、确定性构建、warnings-as-errors 和推荐分析级别在 `Directory.Build.props` 统一开启。
- 包版本集中在 `Directory.Packages.props`，项目文件不散落版本号。
- Release/Debug 均能命令化构建；测试项目不得依赖本机绝对路径。
- 配置模板无 secret；日志和异常不输出 token、密码、连接串或隐私数据。
- 脚本必须从仓库根运行，并对缺少 .NET 10、错误工作目录和失败退出码给出明确提示。

## 6. 明确不做

- 不修改 `gameplay-ability-system-for-unity/Assets`、ProjectSettings、Packages 或 Unity 测试。
- 不创建公网 listener、不扫描或连接任何公网 IP。
- 不选择真实 transport，不实现 ACK/Jitter/reconnect。
- 不接 PostgreSQL/Redis/消息队列。
- 不宣称 S0 `VERIFIED`；本包最高状态为 `SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`。

## 7. 验收

1. `dotnet --version` 在 Server 根解析为受支持的 10.0 SDK。
2. `scripts/bootstrap.ps1` 能检查前置条件且重复运行无副作用。
3. `scripts/build.ps1 -Configuration Release` 成功。
4. `scripts/test.ps1 -Configuration Release` 成功。
5. architecture tests 证明禁止的项目引用不存在。
6. `git status` 不包含 `bin/obj/TestResults/logs/secrets` 等产物。
7. Server 仓库 Change Ledger validator 或等价检查通过。
8. Unity Client 工作树在本包中没有新增脚本 diff。

## 8. 回滚

本包尚未进入实现前，无代码回滚。实现后若失败，只回滚 `NTSD_Server` 仓库中本 Change ID 创建的 solution/项目/脚本；保留 Task Contract、Change Record、失败日志与原因。不得回滚或清理 Unity Client 的既存用户改动。

## 9. 当前阻塞与恢复

- 已解除的环境事实：新的任务 sandbox 已允许 `NTSD_Server` 写入，用户已创建独立 Git 仓库，`.NET SDK 10.0.400` 已安装，`global.json` 可通过 `latestFeature` 解析该 SDK。
- 实际结果：bootstrap 两次、Debug/Release build、四项 no-package test executables、Ledger validation 和 no-network local health run 已通过；完整证据在 Server Change Record。
- 后续边界：本 package 已完成。后续 Server 扩展必须建立新的 Work Package/Change Record；任何 Unity Client 接入、编译、测试或验证都先写 `CLIENT_INTEGRATION_REQUIRED` 并等待用户明确批准。
