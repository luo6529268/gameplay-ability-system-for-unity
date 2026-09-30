# Q09/P-21 香燐自然子体中央 GPU 限定验收

状态：`VERIFIED_SCOPED_NATURAL_CENTRAL_GPU_CONTRIBUTION`。本项只证原 Battle Scene 中自然生成的 OID314 本体被 Unity 中央渲染出口消费，且该目标在白底 scratch GPU 图中的贡献保持低透明度量级；不关闭 P-21、Q09、BATCH-05 或总目标，也不把本项计入 Q07。

正式根 EXE SHA-256：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。正式 `vfs/c/kar/a/cha4.png` SHA-256：`5bbd5c1af4061f891fe6cae1432498e2a7742908db54cd5bbf5d650b58799654`，首个 79×79 单元的非零 alpha 共 1,433 像素、最大 33/255。已有配对正式 Session/WARP 同快照 A/B 逐点匹配该源掩码，见 `NTSD28-Q09-P21-PAIRED-KARIN-WARP-001/ACCEPTANCE.md`；其世界、背景、视口不同于本例。

本次只扩既有可选 Editor 探针 `NTSD28Q09KarinState9997BattlePlayProbeEditor.cs`。生成 Editor C# 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q /p:UseSharedCompilation=false` 退出 0，0 error、202 warning；现有 Unity Editor PID11944 通过本机 Unity-MCP `refresh_unity` 重新导入，原 Editor.log 记录 `Tundra build success (4.82 seconds)`，无 C# error。MCP `manage_scene/get_active` 在运行前确认唯一加载、保存且 clean 的 `NTSD_Battle.unity`。

唯一请求 `karin-central-alpha-x500-20260928-01` 从 X500/slot8、mode0 etc1、CentralOnly 在原 Battle Scene 完整 Driver 推进。tick8 自然生成 OID314/action50/state9997/pic60/owner8；已发布 7 条中央命令，移除精确一条子体本体后为 6 条；生产 resolver/material 所解析的命令也是 7→6。两次 scratch GPU 渲染基于同一冻结帧，分别存入 `karin-central-alpha-x500-20260928-01-body-on.png` 和 `-body-off.png`，原始 JSON 在 `../NTSD28-Q09-P12-KARIN-UNITY-COMMAND-001/karin-central-alpha-x500-20260928-01.json`。报告状态 `CENTRAL_GPU_BODY_PIXELS_OBSERVED`；World tick 8→8，parity checksum 前后均为 `9ba20c773b62913f827dda77fc4296773c2950f64fb272de26eee7d0661ab8e7`。

独立 Pillow/NumPy 读取两张 1280×720 PNG 后，任意 RGBA 通道变化 >0 的像素共 1,245，变化 >2 的共 609（与探针计数相同）；后者左上原点 bbox `[459,188,506,254]`，RGB 通道最大差为 `[20,22,25]`，alpha 通道差 0，所有变化 RGB 差均未超过正式源图首单元的最大 alpha 33。机器数据和三份输入 SHA 在 `karin-central-alpha-x500-20260928-01-pixel-analysis.json`；两 PNG SHA 分别为 `dc80caf2984834546b2d88a3c1a7265ae6d14f6c0a64bf979a46d98ed8ef362f`、`4c20493d487a7a0b1dfbc7b84a05f557d584de2c8deaa1abbbc3dc7209941014`。这支持低 alpha 的目标贡献，不能从不同像素网格与白底差图反推出逐点混合公式完全相等。scratch 图的纵向朝向与正常 Game View 不同，不能作为最终画面验收图。

探针完成时对象数 4→4、已占槽 2→2、借用 2→2，fixture/child 均释放，暂停状态恢复。MCP `get_editor_state` 随后报告 `is_playing=false`、`is_paused=false`、`is_changing=false`、activity `idle`、非编译/导入；四个保护文件 Battle Scene、Menu Scene、GameConfig Asset、ProjectBattleModeConfig Asset 前后 SHA 相同，明细在 `preflight-20260928-01/protected-sha-before.txt` 与 `protected-sha-after.txt`（before 中不存在的旧 mode 猜测路径不作为保护项，实际 Asset 在 `Assets/NTSD/Resources/`）。`Tools/Validate-ChangeLedger.ps1` 退出 0、`Change ledger validation PASSED`（980 Records，当前 diff 的 16 个代码文件均有覆盖；历史 Record 的非当前 diff 路径产生提示）；`git -c core.safecrlf=false diff --check` 退出 0。新增文档 NUL 检查均为 0。本包未改生产战斗规则、DAT/PNG/Scene/相机资产或非战斗代码。

未完成：正式根 EXE 与 Unity 同世界/同视口的最终像素对应、中央 Game View 完整画面、其它透明子体及 P-21/Q09 聚合验收。Q07/D-024 共用碰撞域另归 Q07，仍待取舍；Q08 独立。
