# Q09/C040 第 25 tick 原 Battle Game View 见证

状态：`VERIFIED_SCOPED_GAME_VIEW_CAPTURE`，父项 Q07/C040、Q09、Q12 和总目标仍开放。权威为正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应 playable live path；本包使用已配对的角都 OID25、奇拉比 OID75、凯 OID97 全普通初态离散输入链，只取 Unity 真实画面。

首轮 `c040-view-tick25-20261003-01` 在 Play 启动时因新测试模式未进入旧 `OnSceneLoaded/OnPlayMode` 条件分支而失败，未取得战斗样本。原 [失败 JSON](c040-view-tick25-20261003-01.json) 保留；四保护文件前后 SHA 稳定。探针未自动返回 Menu，是因为同一测试模式漏记 EnteredPlayMode；确认原 Editor 空闲、Battle Scene 干净、非 Play 后，经现有 MCP 连接加载回原保存的 Menu，未保存 Scene。该失败不构成战斗逻辑首差。仅前向修正测试模式分支与唯一请求发现，不修改生产战斗。

第二轮 [原始 JSON](c040-view-tick25-20261003-02.json) 为 `CAPTURED / DONE`、错误空。原 Battle Scene 40 个完整 Driver tick 的整个 `samples` 数组与此前正式源/根配对的 [run-06](../NTSD28-336B44-Q07-C040-NATURAL-SCENE-001/kakuzu-bee-guy-natural-scene-20261003-06.json) 逐值完全相同，40/40 样本无首差；因此继承该样本已声明的 640/640 规则字段配对，不能扩展为全 World 或像素同态。截图等待时 Driver 保持暂停，当前 tick 和样本数量未前进。

[第 25 tick PNG](c040-view-tick25-20261003-02/game-view-tick25.png) 是原 Editor 在 Play 中抓得的 1920×1080 Game View，文件 2,238,040 字节，SHA-256 `23BEA33FFA4D07928CD3E7CD72A91EBC55835E91CE0A79FFFBDCD2C7FA4A8855` 与探针报告一致。画面中央可见近距交叠的战斗角色、血条与地面标记；该帧未见明显缺图或不透明黑矩形。叠放使单帧无法分辨每名角色的精确边缘、抓取帧内先后或确认视觉逐像素相同；PNG也没有独立 GPU 帧号。项目保留自己的背景/HUD/固定视口，正式根 EXE 同条件实际 Present 像素尚未捕获，因此不能宣称画面与正式版完全一致。

本轮有序关闭 `Completed / RuntimeMapCleared`，World 对象、运行槽、池借用、活动池对象和 Sprite 全为 0，pool quiesced、World detached。退出返回原项目单一干净 Menu；Battle Scene `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`、Menu `5D79DBB7F3C6E9FF790413A8D6C0D9942A093F5D9E1B69FC351468C05EC0052D`、GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82` 均与运行前相同。生成 `Assembly-CSharp-Editor.csproj` 0 错误/273 警告，原 Editor 导入后真实 Play 成功。

仅测试探针和诊断/状态文档改动。DAT、图片、Scene、相机、地图、生产战斗及非战斗脚本均未因本包修改。后续 Q09 仍须对有条件可比的正式 EXE 画面和其他正式可达表现出口取证。
