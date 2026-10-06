# 当前场景缺失 TMP 字体补齐工具

已完成：原MenuFont3500MigrationEditor改为EditorWindow，类名/原脚本.meta保持，移除固定Menu场景、3500/Plus字体路径、图集重建/fallback、Input/SubMesh/RawImage迁移和自动保存逻辑。

菜单：NTSD → UI → 补齐当前场景 TMP 字体。目标FontAsset用ObjectField拖入项目TMP_FontAsset文件或点击选择器。扫描当前活动普通场景，显示缺失数量和可点击定位的完整Hierarchy路径。包含inactive/disabled；序列化m_fontAsset为空或失效引用才处理。已经指派默认字体也算已有字体，跳过；其他已加载Scene和TextMeshPro 3D不处理。Play/Prefab Stage禁用。执行重新扫描当前Scene，不用过期缓存；Undo单组、Prefab实例覆盖、标记场景待保存，由用户保存。TMP字体setter负责兼容材质，不强制覆盖兼容的自定义材质。

验证：最后dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly：0error/297warning/9.55秒。原Editor直接Diagnostics两项断言全部通过，editor-check-passed-20261006-100954336.json：一份临时未保存Scene中的缺失/inactive/disabled、已指定跳过、其他Scene拒绝/原Scene字体不变、重复执行0、材质、Undo恢复；窗口ObjectField与未选目标禁用。原Scene活动/dirty状态均保持；本轮未在真实Scene赋字体。

旧失败保留：首次TestRunner的空Untitled阻止NewScene(Additive)，窗口测试已过；后一次SceneManager.CreateScene仅支持Play失败。最终用原保存过的Scene旁一份Editor临时Scene，由原Scene只读字体快照做隔离对照，finally关闭临时Scene并恢复。没有保存临时资产、丢弃或清除当前Scene未保存工作。实际Menu从初始clean后来观测dirty=true，来源未确认，最终保持true而非声称clean。直接检查完整通过独立于旧TestRunner失败。

Scene/Menu/Battle/Plus字体和脚本.meta磁盘SHA与开发前相同；五个before备份逐项SHA相同。最终Tools/Validate-ChangeLedger.ps1与git diff --check原件见validator-final.txt/diff-check-final.txt。当前Editor非Play/idle，窗口菜单已调用打开，目标由用户自行指定；没有执行当前Scene字体批量替换或文字美术验收。没有新增依赖、删除移动文件、Git提交/push。

脚本：Assets/NTSD/Scripts/Test/Editor/MenuFont3500MigrationEditor.cs。
Change/Task/Operation：NTSD-SCENE-MISSING-TMP-FONT-EDITOR-001，详细历史见docs/ai对应Record。精确备份与后SHA见prechange-manifest.json/postchange-manifest.json。
