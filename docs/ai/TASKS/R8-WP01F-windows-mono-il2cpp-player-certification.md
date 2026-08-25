# R8-WP01F — Windows Mono / IL2CPP Player certification

> 日期：2026-08-23  
> 状态：`STOPPED / USER EXCLUDED IL2CPP FROM CURRENT SCOPE`  
> 首个 Change：`R8-PLAYERBUILD-001`

## Goal

在国际版Unity 2022.3.62f3当前工作树上，分别构建并运行Windows x64 Mono与IL2CPP Development Player，
使用同一production battle scene、同一stress request、同seed和同1000实体配置，验证两后端production
scene、CentralOnly、Player hard zero-GC gate、capacity、teardown和cross-runtime checksum一致。

## Preflight facts

- 当前Editor路径：`D:\Unity\HubEditor\2022.3.62f3\Editor\Unity.exe`；
- WindowsStandaloneSupport与`Editor/Data/il2cpp`均存在；
- ProjectSettings当前Standalone backend为IL2CPP；
- 现有`ProductionEntityStressPlayerBuild`只支持旧U9 Mono、硬编码旧输出目录/文件名，没有IL2CPP入口；
- `ProductionEntityStressPlayerBootstrap`已经支持`--ntsd-production-stress-request <absolute path>`、
  auto-stop、report gate和Player退出码；
- C++ authority不参与本包，不运行/构建/修改/写入。

## WP01F-01 — build tool preparation

由`R8-PLAYERBUILD-001`治理，只允许：

- 将现有Mono build入口提取为共享Windows Player build helper；
- 新增明确的Mono与IL2CPP菜单入口和各自独立输出目录/可执行文件名；
- 两后端均仅包含`Assets/NTSD/Scene/NTSD_Battle.unity`，启用Development、frame timing、run-in-background；
- 继续复制已有Config DAT/TXT、BMP与SPARK raw battle dependencies，不部署或生成缺失的默认stage.dat；
- 在finally恢复原ScriptingBackend、frame timing、runInBackground和Burst设置字节；
- 构建失败时保留BuildReport错误并确保ProjectSettings恢复；
- 输出只写`Temp/R8-Windows-Player/<Backend>/`，不得清理或覆盖未知用户目录。

禁止修改Player runtime、stress harness、scene、gameplay、render、pool、DAT、ProjectSettings持久默认值或C++。

## WP01F-02 — build and run matrix

对Mono与IL2CPP分别：

1. build result Succeeded、0 build error；
2. 确认可执行文件与Data目录存在，Build后ProjectSettings/Burst恢复原值；
3. 使用相同绝对request文件启动Player，不启动第二个Unity Editor；
4. Player使用正常图形后端，不用`-nographics`，以保留URP/Central提交；
5. 1000 production GameObject、30 warmup + 180 sampled Combat1000、DataOrientedCanonical、
   MobileExtended、每帧最多1 tick、zero-GC gate；
6. process exit code=0，report `harnessValidity=true`、180 sampled、Player hard envelope/tick/presentation 0 B、
   Gen0/1/2=0、capacity critical0、central draw/pixels>0、teardown restored；
7. Mono/IL2CPP的input、RNG、metadata、world、slots、aRest、vRest、stats、events、overall、workload和roster
   hash逐项一致；
8. 保存Player log、request、report、build result和可执行identity。

## Acceptance

只有两个backend均build/run PASS且12项hash一致，WP01F才可`VERIFIED`。单一backend build成功、Player只启动、
Editor报告或旧U9结果都不能替代。

## Stop conditions

- 需要安装/切换Unity版本或额外平台模块；
- IL2CPP toolchain/build失败且需要改变工程依赖、link.xml、stripping、unsafe/AOT架构；
- Player runtime出现first difference，需要修改bootstrap/harness/gameplay/render/pool；
- build helper不能在finally恢复ProjectSettings/Burst原状态；
- 需要默认stage.dat、Android、服务器或C++ executable；
- 需要删除旧build、清理Library或启动第二个Editor。

## Deliverables

- 两后端build/run artifacts与JSON；
- `docs/ai/RESEARCH/R8-WP01F-windows-player-evidence-20260823.md`；
- STATE、总计划、差异登记与handoff；
- 任何first failure独立Task/Change Record，不在认证包内扩大修复。

## Out of scope

Android/iOS、Release商店包、真实服务器、网络联机、T8默认stage.dat、C++ full trace、Player性能60秒门，以及
任何为通过AOT而改变C++ observable gameplay的架构重写。

## 2026-08-23 stop decision

用户最新明确指示：`IL2CPP Player 不会有任何问题，不要做相关处理`。该指示覆盖本合同此前的
IL2CPP执行安排：

- 不再构建、运行、诊断、修复或重新认证IL2CPP Player；
- 不把本轮Codex沙箱中的IL2CPP进程结果升级为Unity gameplay差异或项目blocker；
- 不根据该结果修改runtime、bootstrap、render、pool、PlayerSettings或第三方组件；
- 已生成的Temp artifact与已写build helper保持原样，不擅自删除或回退；
- 当前工作回到C++ Release→Unity C#战斗逻辑对齐主线。

因此本WP不标`VERIFIED`，也不标为项目IL2CPP故障；它按用户范围决定停止。
