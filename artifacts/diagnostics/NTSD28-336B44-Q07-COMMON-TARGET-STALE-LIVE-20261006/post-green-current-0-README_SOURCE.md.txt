# NTSD 2.8-Logan 源代码快照

本目录包含Logan独立版当前持续开发的C++源代码。根目录发行EXE不随每批覆盖，
新功能以独立候选构建和启动器交付，不能把当前源快照与旧发行EXE视为逐字节对应。

2026-10-06第十一批锦标赛候选入口为`../Start_NTSD2.8-Logan-TournamentCandidate.cmd`，
输出目录为`ntsd28_playable/build/tournament_20261006_batch11_final_v2`。
已接普通菜单、单真人报名、Loading、赛程晋级及冠军返回；这是功能候选，
原版参数grid特殊门、完整视觉/输入一致性及特殊战斗字段仍有明确待闭合边界。

源目录结构：

```text
source/
├─ ntsd28_core/
│  ├─ include/
│  ├─ src/
│  ├─ tests/
│  ├─ scripts/
│  └─ tools/
└─ ntsd28_playable/
   ├─ include/
   ├─ src/
   ├─ tests/
   ├─ scripts/
   └─ resources/    # 仅RC和编译必需ICO
```

## 不重复游戏资源

DAT、精灵图、UI、背景和音频只位于：

```text
../resources/runtime
```

源码目录没有复制第二份 `decoded_dat` 或 `vfs`。运行时通过启动参数把
`--resource-root` 与 `--complete-vfs-root` 同时指向唯一的 runtime 根。

## 构建要求

- Windows 10/11 x64
- MinGW-w64 GCC 15.x，包含 `g++.exe` 与 `windres.exe`
- 工具链中的 zlib 开发库（链接参数 `-lz`）
- Windows SDK/系统导入库：D3D11、DXGI、D3DCompiler、WIC、XAudio2、
  Media Foundation、WinMM

构建脚本按以下顺序查找编译器：

1. `-Compiler <g++.exe完整路径>`
2. 环境变量 `NTSD28_CXX`
3. 系统 `PATH` 中的 `g++.exe`
4. 原开发机路径（仅兼容回退）

## 构建命令

在本目录运行：

```powershell
powershell -ExecutionPolicy Bypass -File .\ntsd28_playable\scripts\build.ps1 -Target playable
```

或双击 `Build_NTSD2.8-Logan.cmd`。

输出位于：

```text
ntsd28_playable\build\Ntsd28Playable.exe
```

构建产物不会自动覆盖发行目录根部的 `NTSD2.8-Logan.exe`。

## 范围

此源码快照不包含逆向工具、原版 `NTSD.exe`、历史二进制、3792矩阵、
运行报告或原版录像语料。主项目中的长期逆向证据和历史报告不属于编译源码。
