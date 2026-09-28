# Q09/P-13 项目背景地震视觉消费者：限定验收

> 2026-09-27 续证更正：下方“worker独立发布待证”是当时待确认项。原Battle Scene唯一探针已确认worker因Unity表现绑定不适用，当前Q09以inline自然Battle Play像素和正式根同帧可见对照为剩余出口；自然技能需先过Q07/D-024碰撞域门。详 `artifacts/diagnostics/NTSD28-Q09-P13-WORKER-BACKGROUND-PLAY-001/ACCEPTANCE.md`。

状态：`RUNTIME_PENDING / FOCUSED_GPU_PASS`。本包只让项目 Map Sprite 在绘制时消费已冻结的地震偏移，未做正式根 EXE 与 Unity 同帧像素 A/B，也未跑真实 Battle 自然 Han 技能 Play；P-13/Q09/BATCH-05/总目标开放。

正式依据：根 EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；配对 `d3d11_renderer.cpp` 只把 `snapshot.earthquake.background_offset_x/y` 加到背景绘制坐标，不加到角色、影子、道具或相机。项目 Map `SpriteRenderer.bounds` 是固定全景取景的输入，因此不平移 Map Transform。正式 screen Y 向下，Unity world Y 向上。

修前原 Editor 精确 EditMode job `0af8f3b6b8464266bffbd7c872009787` 失败在缺少冻结地震背景视觉消费者。实现仅在战斗世界相机渲染期间使用项目背景专用 Sprite shader 平移绘制顶点；偏移 0、相机结束或组件释放时恢复原 `sharedMaterial`，Shader/Material 为运行时按需创建并在组件释放时销毁。没有改 `SpriteRenderer.bounds`、Map Transform、相机参数或实体逻辑/表现坐标。

原 Editor 首次 GPU job `23abdee4b122435d94c53a27564bd0cd` 1/1 PASS：64×64 定向截图中，冻结偏移 `(2,1)` 使红色 Map 像素右移 2、下移 1，无关绿色精灵不动；Map bounds、Map Transform 和相机均不变，归零恢复原画面。随后新增“相机结束时恢复原材质”断言，job `0a0c2214a8ee41ddbf4cd0226f0d3d00` 失败在测试误以为 `Camera.Render()` 后材质仍挂着；实际结束回调已经完成恢复。这是测试时序错误，未修改生产以满足它。修订测试先确认自动恢复，再重施偏移核对非目标相机不清理、目标相机结束才清理。最终原 Editor job `8b17b4acf0884c10955920c32233e47c` 1/1 PASS。相邻背景相机取景两个精确测试 job `b4bc13d49c8f4b7fab43ffb7afc17434` 2/2 PASS。

生成 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 最终相关构建均 0 error；Shader 的实际导入及 GPU 效果由原 Editor 像素用例覆盖。`NTSD_Menu.unity`、`NTSD_Battle.unity` 的磁盘 SHA-256 仍为 `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、`2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`。`Tools/Validate-ChangeLedger.ps1` 退出0，939条Record/当前12个差异受管代码文件覆盖；`git diff --check` 退出0。DAT 数值、排除的背景/模式 DAT、Scene、Prefab、ProjectSettings、非战斗代码均未修改。

剩余：本定向 Editor GPU 测试直接传入冻结帧，未验证生产 `SimulationTickDriver` → 发布帧 → `RenderPipelineManager.beginCameraRendering` 的真实 Battle Play 整链；worker 独立发布及根 EXE 同画面像素也待证。后继应复用自然 action0 Han/Lee 抓取→150/151 的已证输入轨迹，在原 Battle Scene 对比偏移前、偏移时和归零后的项目背景像素，同时锁定角色/影子像素及相机不动；不重复已过的状态生产和隔离 GPU 测试。
