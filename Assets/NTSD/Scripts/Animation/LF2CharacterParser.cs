using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// LF2完整角色配置解析器
    /// </summary>
    public static class LF2CharacterParser
    {
        /// <summary>
        /// 解析完整的角色配置数据
        /// </summary>
        public static LF2CharacterData ParseFullCharacterData(string datContent)
        {
            LF2CharacterData characterData = new LF2CharacterData();

            // 1. 解析基本信息
            ParseBasicInfo(datContent, characterData);

            // 2. 解析精灵图文件信息
            ParseSpriteFiles(datContent, characterData);

            // 3. 解析移动参数
            ParseMovementParameters(datContent, characterData);

            // 4. 解析帧数据
            characterData.frames = LF2DatParser.ParseDatContent(datContent);

            return characterData;
        }

        /// <summary>
        /// 解析基本信息
        /// </summary>
        private static void ParseBasicInfo(string content, LF2CharacterData data)
        {
            // 解析角色名称
            Match nameMatch = Regex.Match(content, @"name:\s*(.+?)(?:\r?\n|$)");
            if (nameMatch.Success)
            {
                data.name = nameMatch.Groups[1].Value.Trim();
            }

            // 解析头像
            Match headMatch = Regex.Match(content, @"head:\s*(.+?)(?:\r?\n|$)");
            if (headMatch.Success)
            {
                data.head = headMatch.Groups[1].Value.Trim();
            }

            // 解析小图
            Match smallMatch = Regex.Match(content, @"small:\s*(.+?)(?:\r?\n|$)");
            if (smallMatch.Success)
            {
                data.small = smallMatch.Groups[1].Value.Trim();
            }
        }

        /// <summary>
        /// 解析精灵图文件信息
        /// </summary>
        private static void ParseSpriteFiles(string content, LF2CharacterData data)
        {
            // 匹配格式: file(0-69): sprite\sys\naruto_0.bmp  w: 79  h: 79  row: 10  col: 7
            var fileMatches = Regex.Matches(content,
                @"file\((\d+)-(\d+)\):\s*(.+?)\s+w:\s*(\d+)\s+h:\s*(\d+)\s+row:\s*(\d+)\s+col:\s*(\d+)");

            foreach (Match match in fileMatches)
            {
                if (match.Success && match.Groups.Count >= 8)
                {
                    int startFrame = int.Parse(match.Groups[1].Value);
                    int endFrame = int.Parse(match.Groups[2].Value);
                    string filePath = match.Groups[3].Value.Trim();
                    int width = int.Parse(match.Groups[4].Value);
                    int height = int.Parse(match.Groups[5].Value);
                    int row = int.Parse(match.Groups[6].Value);
                    int col = int.Parse(match.Groups[7].Value);

                    data.files.Add(new SpriteFileInfo(filePath, startFrame, endFrame, width, height, row, col));
                }
            }
        }

        /// <summary>
        /// 解析移动参数
        /// </summary>
        private static void ParseMovementParameters(string content, LF2CharacterData data)
        {
            // 行走参数
            data.walking_frame_rate = GetIntValue(content, "walking_frame_rate");
            data.walking_speed = GetFloatValue(content, "walking_speed");
            data.walking_speedz = GetFloatValue(content, "walking_speedz");

            // 奔跑参数
            data.running_frame_rate = GetIntValue(content, "running_frame_rate");
            data.running_speed = GetFloatValue(content, "running_speed");
            data.running_speedz = GetFloatValue(content, "running_speedz");

            // 负重行走参数
            data.heavy_walking_speed = GetFloatValue(content, "heavy_walking_speed");
            data.heavy_walking_speedz = GetFloatValue(content, "heavy_walking_speedz");

            // 负重奔跑参数
            data.heavy_running_speed = GetFloatValue(content, "heavy_running_speed");
            data.heavy_running_speedz = GetFloatValue(content, "heavy_running_speedz");

            // 跳跃参数
            data.jump_height = GetFloatValue(content, "jump_height");
            data.jump_distance = GetFloatValue(content, "jump_distance");
            data.jump_distancez = GetFloatValue(content, "jump_distancez");

            // 冲刺参数
            data.dash_height = GetFloatValue(content, "dash_height");
            data.dash_distance = GetFloatValue(content, "dash_distance");
            data.dash_distancez = GetFloatValue(content, "dash_distancez");

            // 翻滚参数
            data.rowing_height = GetFloatValue(content, "rowing_height");
            data.rowing_distance = GetFloatValue(content, "rowing_distance");
        }

        /// <summary>
        /// 获取整数值
        /// </summary>
        private static int GetIntValue(string content, string key, int defaultValue = 0)
        {
            Match match = Regex.Match(content, key + @"\s+(-?\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取浮点数值
        /// </summary>
        private static float GetFloatValue(string content, string key, float defaultValue = 0f)
        {
            Match match = Regex.Match(content, key + @"\s+(-?\d+\.?\d*)");
            if (match.Success && float.TryParse(match.Groups[1].Value, out float result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}
