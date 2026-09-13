# Q02-A1：显式内容来源与路径合同

状态：VERIFIED_PURE_PATH_CONTRACT；总目标NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-02 / Q02。前置Q01已交付，Q02-A实际调用链审计已确认现有root/cache共享风险。真实Unity EditMode24/24、源链接24/24、代码编译与保护检查通过；consumer未接入，Q02整组仍IN_PROGRESS。

## 目标与实际边界

新增不可变BattleContentSource，明确正式runtime的catalog.csv、decoded_dat/data/data.txt、decoded_dat与vfs根，以及现有Unity项目的Config/Assets/DAT相对路径。提供可由现有parser/loader接入的显式路径入口；本子包不切换任何正式caller、不reload singleton、不发布新catalog、不新增管理器/缓存/依赖。

权威：正式EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；GameSession28构造1153-1161分开decoded_dat/vfs，initialize1180后load_extracted_root；object_catalog.cpp load_extracted_root读取catalog.csv并在portable布局读取decoded_dat/source_path；render_snapshot.cpp native_sprite_path按vfs根解析。上述源均在playable build.ps1明确列表内。data.txt不是正式对象catalog.csv的替代；本类不解析对象目录、不裁决registry_index/order。

## 精确写范围（写脚本前声明）

- Assets/NTSD/Scripts/Animation/BattleContentSource.cs：主代理负责不可变路径合同；普通托管value object，不持有Unity对象/stream/task，也不接入shutdown阶段。
- Assets/NTSD/Scripts/Test/Editor/NTSD28B11ContentSourceEditorTests.cs：同一NUnit测试在Editor或源链接诊断进程执行。
- Tools/NTSD28ContentSourceTests/Program.cs、ContentSourceTests.csproj、.gitignore：复用Unity已安装NUnit断言执行上述相同tests，只写本工具bin/obj和Temp/NTSD28ContentSourceTests；不是替代Unity验收。
- 新脚本.meta可由现有Unity自动生成；不得手改Scene/Prefab/asset。文档限本Task/Record、Q02-A审计、Ledger/STATE/handoff/authority/总表与artifacts/diagnostics/NTSD28-B11-CONTENT-SOURCE-PATH-CONTRACT-001/。

## 合同与测试

正式来源由调用者传runtime根，不硬编码J盘。实例捕获绝对根，不依赖后续工作目录变化；index与catalog路径独立、object key相对decoded_dat，sheet/head/small相对同一vfs。Unity旧来源保留Assets路径和DAT相对图片解析，空头像key返回空。两来源实例不共享可变全局state。安全路径校验只约束来源目录边界，不修改游戏字段/常量或正式资源。

不在这里猜测.bmp到.png自动替换：正式角色render_snapshot路径按token原样消费，特定resource catalog fallback属于该消费方，不能推广为所有角色sheet规则。图片解码、pic范围、cache/正式publication与Q07实际切换另建包。

验收：先运行同一NUnit测试在无source class的RED，覆盖runtime根/索引/数据/图片、多个source不串、旧Unity Assets与相对路径、缺省图片、错误root/key边界；补实现后GREEN并链接实际生产源。Unity已有进程，不能启动第二个；本包没有Editor request bridge时源链接PASS仅标FOCUSED_TEST_PASS/UNITY_PENDING。源码编译、diff/ledger与资源保护检查必须实际记录。

风险：后续只接resolver但未接registry/cache/UIImage publication仍会混源，因此本包不可宣称Q02整体交付。失败抛明确参数异常，不用旧源自动fallback；禁止在Running/Stopping切内容。回滚仅本包新增精确文件，适用删除批准仍遵守，不回退用户工作。
