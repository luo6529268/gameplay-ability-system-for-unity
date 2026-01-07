using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NTSD.Animation
{
    public class TextPasteDatParser : MonoBehaviour
    {
        [Header("粘贴的DAT内容")]
        [TextArea(10, 50)]
        public string pastedDatContent = "";

        [Header("解析结果")]
        public List<LF2FrameData> parsedFrames = new List<LF2FrameData>();
        public string analysisResult = "";

        [Header("导出选项")]
        public string exportPath = "Assets/Exported/";

        /// <summary>
        /// 解析粘贴的DAT内容
        /// </summary>
        [ContextMenu("解析粘贴的内容")]
        public void ParsePastedContent()
        {
            if (string.IsNullOrEmpty(pastedDatContent))
            {
                Debug.LogError("请先粘贴DAT文件内容");
                return;
            }

            Debug.Log($"开始解析粘贴的内容，长度: {pastedDatContent.Length} 字符");

            // 预处理文本内容
            string processedContent = PreprocessText(pastedDatContent);

            // 解析帧数据
            parsedFrames = LF2DatParser.ParseDatContent(processedContent);

            // 生成分析报告
            analysisResult = GenerateAnalysisReport(parsedFrames, pastedDatContent);

            Debug.Log($"解析完成: {parsedFrames.Count} 个帧");

            // 保存解析结果
            SaveParsedData();
        }

        /// <summary>
        /// 预处理文本内容
        /// </summary>
        private string PreprocessText(string content)
        {
            StringBuilder processed = new StringBuilder();

            // 处理常见的复制粘贴问题
            string[] lines = content.Split('\n');

            foreach (string line in lines)
            {
                string processedLine = line.Trim();

                // 修复常见的编码问题
                processedLine = FixCommonEncodingIssues(processedLine);

                // 移除空行
                if (!string.IsNullOrEmpty(processedLine))
                {
                    processed.AppendLine(processedLine);
                }
            }

            return processed.ToString();
        }

        /// <summary>
        /// 修复常见的编码问题
        /// </summary>
        private string FixCommonEncodingIssues(string line)
        {
            // 修复常见的乱码字符
            string fixedLine = line
                .Replace("锟斤拷", "")  // 常见的中文乱码
                .Replace("?", "")      // Unicode替换字符
                .Replace("\\n", "\n")  // 转义的换行符
                .Replace("\\r", "\r")  // 转义的回车符
                .Replace("\\t", "\t"); // 转义的制表符

            // 修复数字和符号
            fixedLine = fixedLine
                .Replace("：", ":")    // 中文冒号转英文冒号
                .Replace("，", ",")    // 中文逗号转英文逗号
                .Replace("；", ";")    // 中文分号转英文分号
                .Replace("（", "(")    // 中文括号转英文括号
                .Replace("）", ")")
                .Replace("【", "[")
                .Replace("】", "]");

            return fixedLine;
        }

        /// <summary>
        /// 生成分析报告
        /// </summary>
        private string GenerateAnalysisReport(List<LF2FrameData> frames, string originalContent)
        {
            StringBuilder report = new StringBuilder();

            report.AppendLine("LF2 DAT文件解析报告");
            report.AppendLine("===================");
            report.AppendLine($"解析时间: {System.DateTime.Now}");
            report.AppendLine($"原始内容长度: {originalContent.Length} 字符");
            report.AppendLine($"解析帧数: {frames.Count}");
            report.AppendLine();

            // 帧统计
            report.AppendLine("帧统计:");
            foreach (var frame in frames)
            {
                report.AppendLine($"  帧 {frame.frameId}: {frame.frameName}");
                report.AppendLine($"    图片索引: {frame.pic}, 状态: {frame.state}, 等待: {frame.wait}");
                report.AppendLine($"    下一帧: {frame.next}, 速度: ({frame.dvx}, {frame.dvy})");
                report.AppendLine($"    中心点: ({frame.centerx}, {frame.centery})");
                report.AppendLine($"    碰撞盒: {frame.bodies.Count}, 交互区域: {frame.itrs.Count}");
                report.AppendLine();
            }

            // 结构分析
            report.AppendLine("结构分析:");
            int totalBodies = frames.Sum(f => f.bodies.Count);
            int totalItrs = frames.Sum(f => f.itrs.Count);
            int totalWpoints = frames.Sum(f => f.wpoints.Count);

            report.AppendLine($"  总碰撞盒: {totalBodies}");
            report.AppendLine($"  总交互区域: {totalItrs}");
            report.AppendLine($"  总武器点: {totalWpoints}");
            report.AppendLine($"  有对象点的帧: {frames.Count(f => f.opoint != null)}");
            report.AppendLine($"  有血点的帧: {frames.Count(f => f.bpoint != null)}");
            report.AppendLine($"  有声音的帧: {frames.Count(f => !string.IsNullOrEmpty(f.sound))}");

            return report.ToString();
        }

        /// <summary>
        /// 保存解析数据
        /// </summary>
        private void SaveParsedData()
        {
            // 确保目录存在
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            // 保存分析报告
            string reportPath = Path.Combine(exportPath, "dat_analysis_report.txt");
            File.WriteAllText(reportPath, analysisResult, Encoding.UTF8);

            // 保存解析后的DAT文件（清理后的版本）
            string cleanedDatPath = Path.Combine(exportPath, "cleaned_data.dat");
            string datContent = LF2DatGenerator.GenerateDatContent(parsedFrames);
            File.WriteAllText(cleanedDatPath, datContent, Encoding.UTF8);

            // 保存为JSON
            string jsonPath = Path.Combine(exportPath, "frame_data.json");
            string json = JsonUtility.ToJson(new FrameDataWrapper { frames = parsedFrames }, true);
            File.WriteAllText(jsonPath, json, Encoding.UTF8);

            Debug.Log($"解析结果已保存到: {exportPath}");
            Debug.Log($"  - 分析报告: {reportPath}");
            Debug.Log($"  - 清理后的DAT: {cleanedDatPath}");
            Debug.Log($"  - JSON数据: {jsonPath}");
        }

        /// <summary>
        /// 导出为Unity可用的资源
        /// </summary>
        [ContextMenu("导出为Unity资源")]
        public void ExportAsUnityAssets()
        {
            if (parsedFrames.Count == 0)
            {
                Debug.LogError("没有解析数据，请先解析DAT内容");
                return;
            }

            // 创建ScriptableObject
            var characterData = ScriptableObject.CreateInstance<CharacterFrameData>();
            characterData.frames = parsedFrames;
            characterData.sourceContentLength = pastedDatContent.Length.ToString();
            characterData.parseTime = System.DateTime.Now.ToString();

            // 保存Asset
            string assetPath = Path.Combine(exportPath, "CharacterFrameData.asset");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(characterData, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif

            Debug.Log($"Unity资源已创建: {assetPath}");
        }

        /// <summary>
        /// 验证解析结果
        /// </summary>
        [ContextMenu("验证解析结果")]
        public void ValidateParsing()
        {
            if (parsedFrames.Count == 0)
            {
                Debug.LogError("没有解析数据可供验证");
                return;
            }

            StringBuilder validation = new StringBuilder();
            validation.AppendLine("解析验证报告");
            validation.AppendLine("=============");

            int errors = 0;
            int warnings = 0;

            foreach (var frame in parsedFrames)
            {
                // 验证基本数据
                if (frame.frameId < 0)
                {
                    validation.AppendLine($"? 帧 {frame.frameId}: 帧ID为负数");
                    errors++;
                }

                if (string.IsNullOrEmpty(frame.frameName))
                {
                    validation.AppendLine($"?? 帧 {frame.frameId}: 帧名称为空");
                    warnings++;
                }

                if (frame.pic < 0)
                {
                    validation.AppendLine($"?? 帧 {frame.frameId}: 图片索引为负数");
                    warnings++;
                }

                // 验证碰撞盒数据
                foreach (var body in frame.bodies)
                {
                    if (body.w <= 0 || body.h <= 0)
                    {
                        validation.AppendLine($"?? 帧 {frame.frameId} 碰撞盒: 宽度或高度无效 ({body.w}x{body.h})");
                        warnings++;
                    }
                }

                // 验证交互区域数据
                foreach (var itr in frame.itrs)
                {
                    if (itr.w <= 0 || itr.h <= 0)
                    {
                        validation.AppendLine($"?? 帧 {frame.frameId} 交互区域: 宽度或高度无效 ({itr.w}x{itr.h})");
                        warnings++;
                    }
                }
            }

            validation.AppendLine();
            validation.AppendLine($"验证完成: {errors} 个错误, {warnings} 个警告");

            if (errors == 0 && warnings == 0)
            {
                validation.AppendLine("? 所有数据验证通过!");
            }

            Debug.Log(validation.ToString());
        }
    }

    /// <summary>
    /// 用于存储解析后的帧数据的ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterFrameData", menuName = "LF2/Character Frame Data")]
    public class CharacterFrameData : ScriptableObject
    {
        public string sourceContentLength;
        public string parseTime;
        public int ID;
        public List<LF2FrameData> frames = new List<LF2FrameData>();
    }
}