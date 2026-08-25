# R8-CHARASSET-001 — type 0 character DAT / BMP deployment

> 状态：`IN_PROGRESS / USER APPROVED / RESOURCE-ONLY`

## Goal

恢复Unity `data.txt` 中缺失的type0角色资源，使对应DAT和BMP在`Assets/NTSD/Config/Character`与
`Assets/NTSD/Sprite/Character/<dat-basename>`下可由既有loader加载；不改变任何战斗数据或battle gameplay。

## Source and destination

- DAT source：`J:\QQFile\NTSD 2.4.1\chars`；
- BMP source：`J:\QQFile\NTSD 2.4.1\sprite`；
- DAT destination：`Assets/NTSD/Config/Character`；
- BMP destination：`Assets/NTSD/Sprite/Character/<dat-basename>`；
- mapping：`Assets/NTSD/Config/data.txt`的type0 `file:`。

## Rules

- 先预检，禁止覆盖已有目标DAT/BMP；
- 只补缺失type0，保留已有Unity Character DAT；
- DAT只改`bmp_begin`资源路径；保留加密header/key与所有frame/战斗字段；
- DAT内每一个`head`、`small`、`file(...)`都必须有对应目标BMP；
- source缺失或basename冲突时停止该条目，不猜测替代资源；
- 资源导入后先验证wrapper/resource，再解除`B-R8-R08-01`并继续R08。

## Out of scope

C++ source、battle gameplay、T8、AI、IL2CPP、Android、服务器、DAT数值/技能/碰撞逻辑。

## Preflight result

42个type0条目中5个目标DAT已存在、37个待恢复；源DAT缺失0、解密/解析失败0、BMP引用行204、
BMP源缺失0、同DAT basename冲突0、目标覆盖0。批准按manifest实施。
