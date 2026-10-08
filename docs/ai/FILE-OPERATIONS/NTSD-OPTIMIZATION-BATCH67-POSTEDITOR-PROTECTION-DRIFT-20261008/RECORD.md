# 第67批终核范围外变化事件

Operation NTSD-OPTIMIZATION-BATCH67-POSTEDITOR-PROTECTION-DRIFT-20261008 / UNKNOWN_CAUSE。2026-10-08首次实质终核UTC04:41:59.7760795Z发现，04:42:50.6038242Z LiteralPath再次确认。仅保存现场，不执行删除/恢复/覆盖或字体修复；用户现有内容保持。本事件不授予任何文件操作权限。

逐文件准确绝对路径、初始/当前SHA、bytes、备份在observation-01.json，初始证据回链67 before-manifest-01.json。三个现象：

- `I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/MoreMountains/MMTools/Demos/MMTween/Fonts/Lato/SDF/JifengBladeArtSC-3500-Plus SDF.asset`：8ABE0842…248A/2127125B→B1F4AD54…09FE/2127152B，当前Git modified、lastWrite04:32:20.553459Z。不读取/修改其内容，不把它回退到HEAD或旧值；当前属于保留的范围外工作。初始guard只冻结hash，未在本批建立该字体字节副本。
- `I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Temp/NTSD_ProductionEntityStress.result`：当前缺失，初始533B/SHA D3980E4A…7EAB。66 backups-01/10-NTSD_ProductionEntityStress.result已重新核同完整SHA/bytes，原结果有精确恢复来源，但未进行恢复。
- `I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Temp/Goal18_LastTestResults.xml`：本批已授权test callback覆盖，此后当前缺失；initial九号副本保持，RED/GREEN/Cost分别在fresh67artifacts复制并核SHA，最后Cost FF445A47…BCFA保留。未在关闭后重建公共XML。

原Editor19040已不在、CIM无Unity.exe、6402无监听；log最后写04:33:10.5562285Z。时间相关性不能证明Editor/User/任务谁发起改变或移除；global log末8KiB未见exit/crash关键词也不能证明原因。执行者、实际命令、开始/结束时间未知，没有根据目录时间戳归因。未清理或解析14GB全量日志，未修改日志/系统监控/Editor设置。

本任务实际命令为Get-FileHash/Get-Item/Test-Path/git status/Get-CimInstance只读确认；脚本写入仅67两个C#与已有治理/新artifact/Task68。测试fixture Destroy/Release内存对象不删除项目文件。当前脚本SHA与GREEN/Cost一致、9backup无漂移；266guards中264保持、字体sha与Temp result缺失2项不符必须保留，不声称全保护通过。Scene/settings/Q06/authority/Server等其余保护保持。本事件不是新优化批，不增加46计数，也不自动停止Goal或关闭H07/H11。
