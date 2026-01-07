using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace NTSD.DatParser.Editor
{
    /// <summary>
    /// DAT 文件状态分析工具
    /// 用于提取所有 DAT 文件中的 state 值并生成对比报告
    /// </summary>
    public class DatStateAnalyzer
    {
        private const string ENCRYPTION_KEY = "odBearBecauseHeIsVeryGoodSiuHungIsAGo";

        // 文件路径
        private static readonly string[] AnimationDatFiles = new[]
        {
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\AnimationConfig\Mingren\naruto.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\AnimationConfig\XiaoYing\sakura.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\AnimationConfig\Kakashi\kakashi.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\AnimationConfig\ZuoZhu\sasuke.dat"
        };

        private static readonly string[] FrameDatFiles = new[]
        {
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\rasenshuriken 1.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\rasenshuriken.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\charge.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\chidori.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\death.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\doggy.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\poison.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\rasengan_ball.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\weapon5.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\weapon8.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\wind.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\flash.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\naruto_clone.dat",
            @"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Config\FrameConfig\sword.dat"
        };

        // LF2States 中已定义的状态
        private static readonly Dictionary<int, string> DefinedStates = new Dictionary<int, string>
        {
            { 0, "Standing" },
            { 1, "Walking" },
            { 2, "Running" },
            { 3, "Attack" },
            { 4, "Jump" },
            { 5, "Dash" },
            { 6, "Rowing" },
            { 7, "Defending" },
            { 8, "BrokenDefend" },
            { 9, "Catching" },
            { 10, "BeingCaught" },
            { 11, "Injured" },
            { 12, "Falling" },
            { 13, "Frozen" },
            { 14, "Lying" },
            { 15, "StopRunning" },
            { 16, "Injured2" },
            { 18, "Burning" },
            { 19, "FirenSpecific" },
            { 301, "DeepSpecific" },
            { 400, "TeleportToEnemy" },
            { 401, "TeleportToTeammate" },
            { 501, "RudolfTransform" },
            { 1700, "Heal" }
        };

        [MenuItem("NTSD/分析/生成 DAT 状态对比报告")]
        public static void GenerateStateAnalysisReport()
        {
            Debug.Log("开始分析 DAT 文件状态...");

            // 收集所有状态信息
            var stateInfo = new Dictionary<int, StateUsageInfo>();

            // 解析所有 DAT 文件
            var allFiles = AnimationDatFiles.Concat(FrameDatFiles).ToList();
            foreach (var filePath in allFiles)
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"文件不存在: {filePath}");
                    continue;
                }

                AnalyzeDatFile(filePath, stateInfo);
            }

            // 生成报告
            string report = GenerateReport(stateInfo);

            // 保存报告
            string outputPath = @"I:\C++Test\NTSD\NTSD_DAT状态分析报告.md";
            File.WriteAllText(outputPath, report, Encoding.UTF8);

            Debug.Log($"状态分析报告已生成: {outputPath}");
            Debug.Log($"总计发现 {stateInfo.Count} 个唯一状态值");
        }

        private static void AnalyzeDatFile(string filePath, Dictionary<int, StateUsageInfo> stateInfo)
        {
            try
            {
                // 解密文件
                string decryptedText = Lf2DatDecryptor.DecryptFile(filePath, ENCRYPTION_KEY);

                if (string.IsNullOrEmpty(decryptedText))
                {
                    Debug.LogWarning($"无法解密文件: {filePath}");
                    return;
                }

                // 解析文件
                var parser = new Lf2DatParserV2();
                var datFile = parser.Parse(decryptedText, filePath);

                string fileName = Path.GetFileName(filePath);

                // 提取所有帧中的 state 值
                foreach (var frame in datFile.Frames)
                {
                    foreach (var prop in frame.Properties)
                    {
                        if (string.Equals(prop.Key, "state", StringComparison.OrdinalIgnoreCase))
                        {
                            if (int.TryParse(prop.Value, out int stateValue))
                            {
                                if (!stateInfo.ContainsKey(stateValue))
                                {
                                    stateInfo[stateValue] = new StateUsageInfo
                                    {
                                        StateValue = stateValue,
                                        Files = new List<string>()
                                    };
                                }

                                if (!stateInfo[stateValue].Files.Contains(fileName))
                                {
                                    stateInfo[stateValue].Files.Add(fileName);
                                }

                                stateInfo[stateValue].Count++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"分析文件时出错 {filePath}: {ex.Message}");
            }
        }

        private static string GenerateReport(Dictionary<int, StateUsageInfo> stateInfo)
        {
            var sb = new StringBuilder();

            // 标题
            sb.AppendLine("# NTSD DAT 状态分析报告");
            sb.AppendLine();
            sb.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            // 统计总览
            sb.AppendLine("## 一、状态统计总览");
            sb.AppendLine();

            int totalUniqueStates = stateInfo.Count;
            int definedStatesCount = DefinedStates.Count;
            var missingStates = stateInfo.Keys.Where(s => !DefinedStates.ContainsKey(s)).ToList();
            var unusedStates = DefinedStates.Keys.Where(s => !stateInfo.ContainsKey(s)).ToList();

            sb.AppendLine($"- **总计发现的唯一状态数**: {totalUniqueStates}");
            sb.AppendLine($"- **LF2States 已定义状态数**: {definedStatesCount}");
            sb.AppendLine($"- **缺失状态数**: {missingStates.Count}");
            sb.AppendLine($"- **未使用状态数**: {unusedStates.Count}");
            sb.AppendLine();

            // 所有发现的状态值表格
            sb.AppendLine("## 二、所有发现的状态值（按数值排序）");
            sb.AppendLine();
            sb.AppendLine("| 状态值 | 出现次数 | 文件列表 | LF2States状态 |");
            sb.AppendLine("|--------|---------|---------|--------------|");

            foreach (var kvp in stateInfo.OrderBy(x => x.Key))
            {
                int stateValue = kvp.Key;
                var info = kvp.Value;
                string filesStr = string.Join(", ", info.Files);
                string definedState = DefinedStates.ContainsKey(stateValue)
                    ? $"✅ {DefinedStates[stateValue]}"
                    : "❌ 缺失";

                sb.AppendLine($"| {stateValue} | {info.Count} | {filesStr} | {definedState} |");
            }
            sb.AppendLine();

            // 缺失的状态
            sb.AppendLine("## 三、缺失的状态（需要添加到 LF2States）");
            sb.AppendLine();

            if (missingStates.Count == 0)
            {
                sb.AppendLine("✅ 无缺失状态，所有 DAT 中使用的状态都已在 LF2States 中定义！");
            }
            else
            {
                // 按范围分组
                var baseStates = missingStates.Where(s => s >= 0 && s <= 99).OrderBy(s => s).ToList();
                var skillStates = missingStates.Where(s => s >= 100).OrderBy(s => s).ToList();

                if (baseStates.Count > 0)
                {
                    sb.AppendLine("### 3.1 基础状态范围 (0-99)");
                    sb.AppendLine();
                    sb.AppendLine("```csharp");
                    foreach (int state in baseStates)
                    {
                        var info = stateInfo[state];
                        sb.AppendLine($"/// <summary>");
                        sb.AppendLine($"/// 状态 {state}: [需要补充描述]");
                        sb.AppendLine($"/// 出现在: {string.Join(", ", info.Files)}");
                        sb.AppendLine($"/// 出现次数: {info.Count}");
                        sb.AppendLine($"/// </summary>");
                        sb.AppendLine($"public const int State{state} = {state};");
                        sb.AppendLine();
                    }
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                if (skillStates.Count > 0)
                {
                    sb.AppendLine("### 3.2 技能/特殊状态范围 (100+)");
                    sb.AppendLine();
                    sb.AppendLine("```csharp");
                    foreach (int state in skillStates)
                    {
                        var info = stateInfo[state];
                        sb.AppendLine($"/// <summary>");
                        sb.AppendLine($"/// 状态 {state}: [需要补充描述]");
                        sb.AppendLine($"/// 出现在: {string.Join(", ", info.Files)}");
                        sb.AppendLine($"/// 出现次数: {info.Count}");
                        sb.AppendLine($"/// </summary>");
                        sb.AppendLine($"public const int State{state} = {state};");
                        sb.AppendLine();
                    }
                    sb.AppendLine("```");
                    sb.AppendLine();
                }
            }

            // 未使用的状态
            sb.AppendLine("## 四、未使用的状态（LF2States 中定义但未在 DAT 中使用）");
            sb.AppendLine();

            if (unusedStates.Count == 0)
            {
                sb.AppendLine("✅ 所有定义的状态都有被使用！");
            }
            else
            {
                foreach (int state in unusedStates.OrderBy(s => s))
                {
                    sb.AppendLine($"- **{DefinedStates[state]}({state})**: 未在任何 DAT 文件中找到");
                }
            }
            sb.AppendLine();

            // 修改建议
            sb.AppendLine("## 五、修改建议");
            sb.AppendLine();

            if (missingStates.Count > 0)
            {
                sb.AppendLine("### 5.1 添加缺失的状态常量");
                sb.AppendLine();
                sb.AppendLine($"需要在 `LF2States.cs` 中添加 {missingStates.Count} 个状态常量定义。");
                sb.AppendLine("参考上面「三、缺失的状态」中的代码。");
                sb.AppendLine();
            }

            if (unusedStates.Count > 0)
            {
                sb.AppendLine("### 5.2 未使用的状态处理");
                sb.AppendLine();
                sb.AppendLine("以下状态在 LF2States 中定义但未在 DAT 文件中使用：");
                sb.AppendLine();
                foreach (int state in unusedStates.OrderBy(s => s))
                {
                    sb.AppendLine($"- {DefinedStates[state]}({state})");
                }
                sb.AppendLine();
                sb.AppendLine("建议：");
                sb.AppendLine("1. 如果这些状态是预留的或用于其他场景，可以保留并添加注释说明");
                sb.AppendLine("2. 如果确认不再需要，可以移除或标注为 `[Obsolete]`");
                sb.AppendLine();
            }

            sb.AppendLine("### 5.3 命名建议");
            sb.AppendLine();
            sb.AppendLine("对于缺失的状态，建议根据以下规则命名：");
            sb.AppendLine();
            sb.AppendLine("- **基础状态 (0-99)**: 使用动词或状态名词，如 `Running`, `Jumping`, `Attacking`");
            sb.AppendLine("- **技能状态 (100+)**: 使用技能名称或描述性名称，如 `Rasengan`, `Chidori`, `ShadowClone`");
            sb.AppendLine("- 如果无法确定准确含义，可以先使用 `State{数值}` 作为占位符");
            sb.AppendLine();

            // 附录：分析的文件列表
            sb.AppendLine("## 附录：已分析的文件");
            sb.AppendLine();
            sb.AppendLine("### 角色 DAT 文件 (4)");
            foreach (var file in AnimationDatFiles)
            {
                sb.AppendLine($"- {Path.GetFileName(file)}");
            }
            sb.AppendLine();
            sb.AppendLine("### 对象 DAT 文件 (14)");
            foreach (var file in FrameDatFiles)
            {
                sb.AppendLine($"- {Path.GetFileName(file)}");
            }
            sb.AppendLine();

            return sb.ToString();
        }

        private class StateUsageInfo
        {
            public int StateValue { get; set; }
            public int Count { get; set; }
            public List<string> Files { get; set; }
        }
    }
}
