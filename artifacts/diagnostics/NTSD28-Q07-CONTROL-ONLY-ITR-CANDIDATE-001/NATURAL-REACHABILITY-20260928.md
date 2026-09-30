# Q07 控制型 ITR 的自然入口收窄（2026-09-28）

状态：`STATIC_CONDITIONAL_ROUTE_ONLY / NATURAL_PLAY_PENDING`。本次只读正式发行内容与 playable 对应源码，未运行正式 EXE 或 Unity Play，未修改 DAT、脚本或场景。已有原 Editor 聚焦双收集器 3/3 PASS 仍只证明受控作者帧的候选准入，不因本审计升级为自然战斗验收。

正式 `resources/runtime/decoded_dat/data/data.txt:116-128` 将千代列为 OID8、傀儡五列为 OID854/type0、特效 OID419/type3。`c/chi/chi.dat:39-87` 的站立/行走帧声明 `hit_Uj: 301`；frame301 (`:1439`) 的 `next:560` 接 frame560→561→562→563→564 (`:2281-2309`)；frame564 的 OPoint 生成 OID419/action300。`c/chi/a/atk.dat:502-543` 中 action300→301→…→305，frame305 的 OPoint 生成 OID854/action310。`c/chi/pup5.dat:1062-1071` 中 310→311→312→108；frame108→109→0 (`:467-481`) 给出了傀儡进入普通帧的作者字段链。

同一傀儡 DAT 的 frame0 等普通帧具有完整 `kind:8 ... dvx:400` ITR（如 `:29-35`）。其转帧条件若在实际战斗中成立，frame400→401→402→403→407（`:1092-1149`）；frame407 的作者顺序为完整 kind8 ITR ordinal0、缺宽高的 `kind:100100` ITR ordinal1。正式 playable 对应源码 `collision_geometry.cpp:22-23` 将后者判为 `control_only`，`hit_candidates.cpp:154-169` 保留作者 ITR 序号但跳过候选。现有 Unity `BruteForceSceneQuery.IsReleaseItrGeometry` 修复及受控原 Editor 双 profile 结果见本 ID Change Record。

**没有证明的环节：** `hit_Uj` 被物理按键触发、千代该技能在正式根 EXE 和原 Battle Scene 成功生成傀儡、傀儡进入 kind8 接触并到 frame407、当帧敌方 BDY 与控制 ITR 投影相交、以及同状态两端候选/结果一致。只凭 `next`、OPoint、`dvx` 字段不能声称自然可达或可见伤害。下一个有价值的单例是从正式千代的物理组合键起步，先记录 OID419→OID854 的实际出生，再在满足 kind8 接触时检查 OID854/frame407 的 ITR 序号及候选；若实际入口未触发，则记录第一个断点并停止，不扩大为全角色矩阵。不要重复已有 3/3 受控作者帧或另行修改 DAT。

KonanAngel OID54/frame11 也有仅控制型 ITR；当前只读证据为 `data/data.txt:303-305` 收录、`c/kon/ang.dat:27-33` 声明 running_frame=9、frame9→10→11 (`:87-104`)，以及 `s/5/stage5.dat:41` 的 stage 出生字段。由于用户暂缓默认 stage DAT 部署，且未见普通战斗同条件入口，本次不把 stage 行当作 Q07 普通自然 Play 任务，也不声称 OID54 自然可达。
