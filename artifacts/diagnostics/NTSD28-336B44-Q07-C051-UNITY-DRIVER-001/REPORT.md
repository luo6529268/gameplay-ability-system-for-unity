# C051 原 Unity 完整 Driver 的左右 effect23 首差

状态：`VERIFIED_SCOPED_DIAGNOSTIC / PRODUCTION_GAP_OPEN`。本包验证严格 raw 诊断入口并定位首差，不关闭 C051、Q07 或总目标。

当前规则权威是正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。沿用同目录 `NTSD28-336B44-Q07-C051-ORO-EFFECT23-REACH-001` 中正式源码完整 GameSession 与根 LFR 左右各 80 tick、各 640/640 选定字段同态的初态：seed 682973786、mode 0、OID20/action288/X500，经 OPoint 出生 OID888/action35→40，于 tick9 effect23/dvx-10 命中 OID2。目标右 X550、两人面右；或目标左 X350、两人面左。

原项目 Unity Editor 已导入新增 `ntsd28-336b44-q07-c051-effect23/1.0` 严格 schema；两份固定 JSON 在同目录，两次请求式生产 Driver raw 捕获结果文件均为 `PASS`，各完成 12 tick。生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit 0，0 错误、235 警告。未启动第二个 Unity 项目，未进入 Scene Play，未改生产、DAT 或 Scene。

以相同 tick、slot、OID 比较 slot0 父体、slot1 目标、slot50 子体的 `action`、`hp`、`vx`：左右各 99 个已声明字段中，首差均在 tick9 的 slot1 `action`：Unity 186，正式根 180；tick10～12 动作同样有差，tick12 `vx` 右侧 Unity +10 / 根 -10，左侧 Unity -10 / 根 +10。其余选定字段相同，包括子体出生与动作、目标扣血。Unity 项目自有 stage 的 Z 与正式版初态 Z 不同，属已批准地图例外；水平冲量反号不能用视觉比例解释。

源码定位：正式 `hit_response.cpp::accumulate_unarmored_horizontal` 对普通无甲命中在非 state2000/type4/6 下使用攻击者面向符号乘完整 `dvx`，不读 effect22/23；`accumulate_unarmored_vertical` 根据已累计水平冲量选动作 180/186。Unity `LF2HitResolveRuntimeData.ResolveStandardDamageKnockbackX` 的无甲共用路径则对 effect22/23 使用相对位置，随后 `BattleDamageWriter.ApplyStandardCharacterDamage` 用所得冲量选倒地方向。正式 reduced 分支的 effect22/23 确实使用相对位置；该分支不能随无甲修正一起改变。以上解释与双向反号吻合，具体生产修复和正反例必须在独立 Task/Change 中实施验证。

保护边界：本包四项 Scene/Asset 基线 SHA 未被 raw 捕获改动；没有声称完整 World、Scene 自然按键、护甲分支或 Q07 整组同态。下一步是共享无甲规则的聚焦 RED→GREEN、左右原 Driver raw 复测，再决定必要 Scene 验收。
