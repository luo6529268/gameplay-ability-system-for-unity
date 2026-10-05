# NTSD28-336B44-Q09-IDLE-TICK14-GAMEVIEW-001

状态：`COMPILE_PASS / ORIGINAL_EDITOR_PLAY_PENDING / TEST_ONLY`。归属当前 336B44 总表 Q09 的一例非例外战斗画面出口。

正式根 EXE 已在真实 D3D11 窗口留下鸣人 OID2／鼬 OID9、X500/620、Z400、双方 HP500/MP200、无玩家战斗输入、暂停 tick14 的 1280×720 画面。旧 Unity 截图和 P-08 探针初态不同，不能配对。用户保留项目背景、固定相机、普通 HUD，且已删除 WORDS 字形图；对照只裁决同态逻辑下的角色本体、阴影、挂点及非例外战斗效果。

只修改现有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09P08SameStateBattlePlayProbeEditor.cs` 的 opt-in 诊断分支；不改变既有 P-08 请求的行为和结果路径。新分支仅由带唯一 runId、短有效期的 `Temp` 请求触发：从单一已保存的 Battle Scene 开始，在 Play clone 中选择 OID2/OID9、固定上述源坐标与 HP/MP、模式0/难度0/seed0，提交14个无按键的完整生产 Driver tick，记录每 tick 角色状态、随机初态与发布 tick，暂停后获取原 Game View 合成 PNG。报告应同时记录截图前后逻辑 tick、相机/视口和退出后的 Scene 文件身份；不以不同背景/视口作逐像素门。

安全前置：不在当前 Editor Play、编译、更新或 Scene dirty 时切场景/运行；如 Menu 已保存，可仅在本次有限请求期间打开原 Battle Scene，结束后回到原干净 Menu；若场景或文件身份变化，拒绝继续并留失败原件。请求过期后不得自发启动。全过程不保存、覆盖或还原 Scene，保留并行 UI/字体/资源修改。

验证：先运行生成 Editor 工程编译与 Change Ledger 校验，再等待原 Editor 导入。只运行这一例，不启动第二个同项目 Editor或全套测试。运行成功也只关 Q09 的该同态画面子门；若逻辑初态或 tick14 行为不匹配，先定位首差，不能凭截图修表现。回滚以新的精确编辑撤销本 opt-in 分支，保留历史测试结果和其它写者改动；不执行 Git restore/reset/delete。
