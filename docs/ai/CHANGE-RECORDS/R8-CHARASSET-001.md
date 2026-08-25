# R8-CHARASSET-001 — type 0 character DAT / BMP deployment and Unity path adaptation

<!-- CHANGE-RECORD
id: R8-CHARASSET-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterAssetDeploymentEditorTests.cs
authority: USER-APPROVED-R8-CHARASSET-001 / UNITY-RUNTIME-DAT-LOAD-CONTRACT
evidence: RESOURCE-STATIC-42-227-PASS / UNITY-COMPILE0 / EDITMODE-1-OF-1-PASS / NORMAL-PLAY-CONSOLE0
-->

> 创建日期：2026-08-24  
> 状态：`VERIFIED / USER APPROVED / RESOURCE-ONLY`  
> 关联：`R8-WP01G-R08`、`B-R8-R08-01`

## 1. 用户批准的目标

从`J:\QQFile\NTSD 2.4.1\chars`恢复当前Unity `data.txt`中缺失的`type: 0`角色DAT至
`Assets/NTSD/Config/Character/`；对每个已复制DAT，按其实际`<bmp_begin>`引用从
`J:\QQFile\NTSD 2.4.1\sprite`复制BMP到`Assets/NTSD/Sprite/Character/<dat-basename>/`，并把
DAT中的`head`、`small`、`file(...)`路径改为对应`Assets/...`路径。同步把该`data.txt`条目的`file:`改为
`Assets/NTSD/Config/Character/<dat-basename>.dat`。

## 2. 范围与边界

- 仅处理`data.txt`中`type: 0`、当前目标DAT缺失且源DAT存在的条目；已在`Config/Character`存在的DAT不重复制；
- DAT仅修改资源定位字段：`<bmp_begin>`的`head`、`small`、`file(...)`；frame/state/itr/bdy/opoint等战斗数据不改；
- 逐DAT创建同名sprite目录；共享BMP按用户目录规则可在各角色目录重复保存；
- 保留原DAT 123-byte encryption header；解密后改路径，再以同一key重新加密payload；
- 不修改C++ authority、不运行/构建C++、不改Unity battle gameplay、CentralOnly、slot、pass order或T8；
- 若预检发现源DAT、任一引用BMP、目标路径或同DAT basename映射冲突，停止该条目并记录，不用替代资源猜补。

## 3. 依据

- 用户于2026-08-24明确要求执行此恢复流程；
- `Assets/NTSD/Config/data.txt`是Unity对象→DAT映射；
- `GameDataManager.ResolveObjectFilePath`接受以`Assets/`开头的项目路径；
- `CharacterAnimtorManager.ParseCharacterFrameConfigs`使用`Lf2DatDecryptor`读取DAT；
- 资源来源为用户指定的`J:\QQFile\NTSD 2.4.1`，不是`ntsd_proto`或C++ release source tree。

## 4. 预检与验收

预检必须输出：type0总数、已存在数、待恢复数、源DAT缺失数、BMP引用数、BMP源缺失数、basename碰撞数、目标覆盖数。

完成后必须验证：

1. `data.txt`所有已恢复type0条目指向`Assets/NTSD/Config/Character/`；
2. 每个恢复DAT可由当前decryptor读取，且所有修改后的`head/small/file(...)`路径存在；
3. Unity导入/编译0 error；
4. 角色加载时恢复的wrapper可用；
5. R08的OID7/8/51 wrapper、running/DJA可达性重新检查；
6. 资源记录和最终清单同步到Task/Handoff/STATE；Change Ledger validator通过。

## 5. 当前状态

预检、部署和Unity验证已完成。预检结果：type0总数42、已有目标DAT5、待恢复DAT37、源DAT缺失0、
解密/解析失败0、BMP引用行204、BMP源缺失0、同DAT basename冲突0、目标BMP覆盖0。

实际部署：复制37份DAT、182个去重BMP（DAT内总计227条引用）；`data.txt`的42个type0条目均统一指向
`Assets/NTSD/Config/Character/`，每个恢复DAT的`head/small/file(...)`均改为其角色目录内的
`Assets/NTSD/Sprite/Character/...`资源路径。首次失败的复制命令在写入前因PowerShell不支持`-NoClobber`
停止；其stage/backup目录经检查为空，第二次以显式存在性检查完成，不存在半写入资源。

最终验证证据：

1. 机械静态复核：42 type0、0映射偏离、0缺失DAT、0解密/BMP块失败、227 BMP引用、0缺失BMP；
2. Unity全资源导入后Console compiler error=0；
3. `NTSD.Test.CharacterAssetDeploymentEditorTests.TypeZeroCharacterCatalogDecryptsParsesAndResolvesDeclaredBitmaps`
   实测`1/1 PASS`（job `34a8a483ff314b82b65e9df5f4aaaf0e`，1.84s）；
4. 资源导入后的`NTSD_Battle`正常Play 20秒，Console新增error/warning=0。

本Record只证明资源契约已恢复并可被Unity读取；OID7/8/51的merge/dormant/split战斗行为尚未执行，归R08独立验收。
