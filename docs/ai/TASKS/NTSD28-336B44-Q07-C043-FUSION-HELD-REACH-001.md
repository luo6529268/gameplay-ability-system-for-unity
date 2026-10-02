# NTSD28-336B44-Q07-C043-FUSION-HELD-REACH-001

状态：`VERIFIED`（限正式源码/根程序自然入口；Unity原Scene与C043父项开放）。父目标：新版 336B44 G1/BATCH-04/Q07/C043。

目标：有界验证正式 playable 是否能由正式角色 DAT 自然生成 kind-2 持有子体，随后让携带该子体的合体伙伴进入融合，观察同 tick 后轮 held-refill 是否遇到失效父槽并只清子体关系状态。普通 `despawn` 已经先清理关系，不能代替此候选。静态入口及限制见 [先行审计](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-REACH-AUDIT-20261002/REPORT.md)。

唯一脚本范围：新增 `Tools/NTSD28Q07Diagnostics/c043_fusion_held_reach_probe.cpp`，调用当前正式 `GameSession28::step`，只读取正式 runtime DAT；保存每 tick OID、动作、关系、child/parent 槽位、fusion 与两轮 held-refill 事件。对 7+8→51 正式记录做不超过 60 tick 的正式内容候选及无持有对照，不改正式 C++ 源码。输出只写新诊断目录，现有原件不覆盖或删除。

判定：必须分别记录正式 DAT kind-2 出生、融合前负关系、真实融合、融合后失效关系尾四个门。任一前置不成立即 `SCOPED_NEGATIVE`，只排除实际运行条件，不称 C043 已对齐；有阳性才继续根 336B44 EXE 的可观测入口和原 Unity Battle Scene 同条件。该包不改 DAT 数值、Unity 生产/测试脚本、Scene、资源、ProjectSettings 或非战斗功能。

验证：当前正式 EXE/DAT 身份、诊断 C++17 编译零错、相同 seed/输入双跑输出逐字节同态、逐 tick 证据与首个失败门、`git diff --check`、Change Ledger validator。回滚须先审阅本 ID 新增文件，保留用户工作与证据；未经批准不删除或 Git restore/reset。

阳性后的增量出口（脚本再改前）：初态 Chi frame256→257 已在 tick6 自然生成并持续持有 OID420；晚间双按方向 profile 在 tick28 合体，并由第二轮 held-refill 报互指缺失、子体关系-1→0。两次60tick CSV逐字节一致。为核当前根 EXE 载体，只在同一诊断脚本增加 GameSessionLfr28 录包，并将两角色当前/base HP统一100（LFR保存baseHP），保留此前100/500原件；正式根以 slot1 初始动作256、朝向左、人类输入覆盖及同一录包对照。若载体不支持，明确记录，不将源码探针误称正式根。
