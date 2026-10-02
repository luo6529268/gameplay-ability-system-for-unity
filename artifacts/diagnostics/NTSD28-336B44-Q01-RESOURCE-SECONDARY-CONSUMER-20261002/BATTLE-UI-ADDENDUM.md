# Q01 `resource.dat` 战斗 UI 候选范围复核（2026-10-02）

状态：`READ_ONLY_SCOPED_CONSUMER_RECHECK / Q01_NONOBJECT_VISUAL_PENDING`。当前根正式 `NTSD2.8-Logan.exe` SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本次只读正式 playable 源码与现有 55 项矩阵；未改 DAT、图片、脚本、Scene 或配置。

## 已证的窄结论

1. `resource.dat` 主表索引 27 是 `sprite/UI/SCORE_BOARD4.png`。`source/ntsd28_core/include/ntsd28/render_snapshot.h:713-724` 明确把模式 4 的 `SCORE_BOARD4` 队伍汇总与当前普通模式结果板分开；`source/ntsd28_playable/src/game_session.cpp:3354-3373` 的当前普通结果板只在模式 0 结果显示阶段绑定索引 24～26、28～31，没有绑定 27。用户已排除原生结果图文。因此索引 27 对本轮资源部署是**范围排除**，但它在正式 EXE 中是否有其它实际消费者仍未证，不能写成“正式程序不用”。[v3 矩阵](resource-55-path-matrix-v3.csv)仅将它的范围标签改成 `RESULT_GRAPHIC_USER_EXCLUDED_CONSUMER_UNPROVEN`。
2. 主表索引 22 是 `sprite/UI/PAUSE.png`。当前 `source/ntsd28_playable/src/d3d11_renderer.cpp:2292-2296` 的战斗暂停分支在 `paused` 时调用 `draw_solid`，不读取该 PNG。该结论仅覆盖已检查的 playable 战斗暂停绘制分支；本轮未发现其它直接解析点，故 v3 保留 `NO_DIRECT_PLAYABLE_RESOLVER_FOUND`，不把负搜索升级为全局不可达证明。
3. 对 v2 仍无直接解析点的其余文件名，在当前 `ntsd28_playable/src`、`ntsd28_core/src` 与 `ntsd28_core/include` 的 `.cpp/.h` 做精确文件名检索，没有找到新的生产路径常量；唯一命中的 `MENU_CLIP3.png` 是已有的剧情结果索引 3。此检索不覆盖由字段、索引或动态路径构造的消费者，因此不改变其余条目的未知状态。

## 对计划的影响

正式主表仍有 20 项未获明确直接消费者证明，其中索引 27 的图文部署范围已按用户例外明确，余下 19 项仍需结合真实消费者或画面条件判断；独立 `<frame>` 七项继续未证。索引 22 不因文件名像暂停画面就搬入 Unity。活跃战斗直接读取的 WORDS0～5/SPARK 七图 7/7 同版结论不变。Q01 非例外 Game View、Q09 视觉配对及 Q12 终验保持开放；下一优先走真实战斗画面与可达消费链，不按资源表剩余数量批量复制图片。

v1、v2 矩阵原件保留。本更正没有为任何文件获得删除授权。
