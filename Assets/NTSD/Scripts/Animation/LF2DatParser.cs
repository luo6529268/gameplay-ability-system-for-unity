using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NTSD.Animation
{
    /// <summary>
    /// LF2 DAT文件解析器
    /// </summary>
    public static class LF2DatParser
    {
        private const string FRAME_START_TAG = "<frame>";
        private const string FRAME_END_TAG = "<frame_end>";

        /// <summary>
        /// 从DAT文件内容解析帧数据
        /// </summary>
        public static List<LF2FrameData> ParseDatContent(string datContent)
        {
            List<LF2FrameData> frames = new List<LF2FrameData>();

            // 分割帧内容
            string[] frameContents = datContent.Split(new string[] { FRAME_START_TAG }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string frameContent in frameContents)
            {
                if (frameContent.Contains(FRAME_END_TAG))
                {
                    LF2FrameData frame = ParseSingleFrame(frameContent);
                    if (frame != null)
                    {
                        frames.Add(frame);
                    }
                }
            }

            return frames;
        }

        /// <summary>
        /// 解析单个帧
        /// </summary>
        private static LF2FrameData ParseSingleFrame(string frameContent)
        {
            try
            {
                LF2FrameData frame = new LF2FrameData();

                // 提取帧ID和名称
                string firstLine = frameContent.Split('\n')[0].Trim();

                // 修改 1: 建议同时支持空格和制表符(\t)分割，增强兼容性
                string[] firstLineParts = firstLine.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

                if (firstLineParts.Length > 0 && int.TryParse(firstLineParts[0], out int frameId))
                {
                    frame.frameId = frameId;
                }

                // 修改 2: 修复带空格的名字被截断的问题
                if (firstLineParts.Length > 1)
                {
                    // 原代码: frame.frameName = firstLineParts[1];
                    // 新代码: 从索引1开始，将剩余的所有部分用空格连接起来
                    frame.frameName = string.Join(" ", firstLineParts, 1, firstLineParts.Length - 1);
                }

                // 解析基本参数
                ParseBasicParameters(frameContent, frame);

                // 解析武器点
                ParseWeaponPoints(frameContent, frame);

                // 解析碰撞盒
                ParseBodyBoxes(frameContent, frame);

                // 解析交互区域
                ParseInteractionAreas(frameContent, frame);

                // 解析对象点
                ParseObjectPoint(frameContent, frame);

                // 解析血点
                ParseBloodPoint(frameContent, frame);

                // 解析抓取点
                ParseCatchPoint(frameContent, frame);

                // 解析声音
                ParseSound(frameContent, frame);

                return frame;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"解析帧数据失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析基本参数
        /// </summary>
        private static void ParseBasicParameters(string content, LF2FrameData frame)
        {
            // 使用正则表达式匹配参数
            frame.pic = GetIntValue(content, "pic:");
            frame.state = GetIntValue(content, "state:");
            frame.wait = GetIntValue(content, "wait:", defaultValue: 1);
            frame.next = GetIntValue(content, "next:");
            frame.dvx = GetIntValue(content, "dvx:");
            frame.dvy = GetIntValue(content, "dvy:");
            frame.dvz = GetIntValue(content, "dvz:");
            frame.centerx = GetIntValue(content, "centerx:");
            frame.centery = GetIntValue(content, "centery:");
            frame.mp = GetIntValue(content, "mp:");

            frame.hit_a = GetIntValue(content, "hit_a:");
            frame.hit_d = GetIntValue(content, "hit_d:");
            frame.hit_j = GetIntValue(content, "hit_j:");
            frame.hit_Fj = GetIntValue(content, "hit_Fj:");
            frame.hit_Fa = GetIntValue(content, "hit_Fa:");
            frame.hit_Da = GetIntValue(content, "hit_Da:");
            frame.hit_Ua = GetIntValue(content, "hit_Ua:");
            frame.hit_ja = GetIntValue(content, "hit_ja:");
            frame.hit_Dj = GetIntValue(content, "hit_Dj:");
            frame.hit_Uj = GetIntValue(content, "hit_Uj:");
        }

        /// <summary>
        /// 解析武器点
        /// </summary>
        private static void ParseWeaponPoints(string content, LF2FrameData frame)
        {
            // 查找所有wpoint块
            var wpointMatches = Regex.Matches(content, @"wpoint:(.*?)wpoint_end", RegexOptions.Singleline);

            foreach (Match match in wpointMatches)
            {
                string wpointContent = match.Groups[1].Value;
                WeaponPoint wpoint = new WeaponPoint();

                wpoint.kind = GetIntValue(wpointContent, "kind:");
                wpoint.x = GetIntValue(wpointContent, "x:");
                wpoint.y = GetIntValue(wpointContent, "y:");
                wpoint.weaponact = GetIntValue(wpointContent, "weaponact:");
                wpoint.attacking = GetIntValue(wpointContent, "attacking:");
                wpoint.cover = GetIntValue(wpointContent, "cover:");
                wpoint.dvx = GetIntValue(wpointContent, "dvx:");
                wpoint.dvy = GetIntValue(wpointContent, "dvy:");
                wpoint.dvz = GetIntValue(wpointContent, "dvz:");

                frame.wpoints.Add(wpoint);
            }
        }

        /// <summary>
        /// 解析碰撞盒
        /// </summary>
        private static void ParseBodyBoxes(string content, LF2FrameData frame)
        {
            // 查找所有bdy块
            var bdyMatches = Regex.Matches(content, @"bdy:(.*?)bdy_end", RegexOptions.Singleline);

            foreach (Match match in bdyMatches)
            {
                string bdyContent = match.Groups[1].Value;
                BodyBox body = new BodyBox();

                body.kind = GetIntValue(bdyContent, "kind:");
                body.x = GetIntValue(bdyContent, "x:");
                body.y = GetIntValue(bdyContent, "y:");
                body.w = GetIntValue(bdyContent, "w:");
                body.h = GetIntValue(bdyContent, "h:");

                frame.bodies.Add(body);
            }
        }

        /// <summary>
        /// 解析交互区域
        /// </summary>
        private static void ParseInteractionAreas(string content, LF2FrameData frame)
        {
            // 查找所有itr块
            var itrMatches = Regex.Matches(content, @"itr:(.*?)itr_end", RegexOptions.Singleline);

            foreach (Match match in itrMatches)
            {
                string itrContent = match.Groups[1].Value;
                InteractionArea itr = new InteractionArea();

                itr.kind = GetIntValue(itrContent, "kind:");
                itr.x = GetIntValue(itrContent, "x:");
                itr.y = GetIntValue(itrContent, "y:");
                itr.w = GetIntValue(itrContent, "w:");
                itr.h = GetIntValue(itrContent, "h:");
                itr.zwidth = GetIntValue(itrContent, "zwidth:");
                itr.injury = GetIntValue(itrContent, "injury:");
                itr.fall = GetIntValue(itrContent, "fall:");
                itr.arest = GetIntValue(itrContent, "arest:");
                itr.vrest = GetIntValue(itrContent, "vrest:");
                itr.effect = GetIntValue(itrContent, "effect:");
                itr.kill   = GetIntValue(itrContent, "kill:");
                itr.catchingact = GetIntArrayValue(itrContent, "catchingact:");
                itr.caughtact = GetIntArrayValue(itrContent, "caughtact:");
                itr.attacking = GetIntValue(itrContent, "attacking:");
                itr.throwvz = GetIntValue(itrContent, "throwvz:");

                frame.itrs.Add(itr);
            }
        }

        /// <summary>
        /// 解析对象点
        /// </summary>
        private static void ParseObjectPoint(string content, LF2FrameData frame)
        {
            if (content.Contains("opoint:"))
            {
                string opointContent = GetContentBetween(content, "opoint:", "opoint_end");
                if (!string.IsNullOrEmpty(opointContent))
                {
                    frame.opoint = new ObjectPoint();
                    frame.opoint.kind = GetIntValue(opointContent, "kind:");
                    frame.opoint.action = GetIntValue(opointContent, "action:");
                    frame.opoint.objectId = GetIntValue(opointContent, "id:");
                    frame.opoint.x = GetIntValue(opointContent, "x:");
                    frame.opoint.y = GetIntValue(opointContent, "y:");
                    frame.opoint.dvx = GetIntValue(opointContent, "dvx:");
                    frame.opoint.dvy = GetIntValue(opointContent, "dvy:");
                    frame.opoint.oid = GetIntValue(opointContent, "oid:");
                    frame.opoint.facing = GetIntValue(opointContent, "facing:");
                }
            }
        }

        /// <summary>
        /// 解析血点
        /// </summary>
        private static void ParseBloodPoint(string content, LF2FrameData frame)
        {
            if (content.Contains("bpoint:"))
            {
                string bpointContent = GetContentBetween(content, "bpoint:", "bpoint_end");
                if (!string.IsNullOrEmpty(bpointContent))
                {
                    frame.bpoint = new BloodPoint();
                    frame.bpoint.x = GetIntValue(bpointContent, "x:");
                    frame.bpoint.y = GetIntValue(bpointContent, "y:");
                }
            }
        }

        /// <summary>
        /// 解析抓取点
        /// </summary>
        private static void ParseCatchPoint(string content, LF2FrameData frame)
        {
            if (content.Contains("cpoint:"))
            {
                string cpointContent = GetContentBetween(content, "cpoint:", "cpoint_end");
                if (!string.IsNullOrEmpty(cpointContent))
                {
                    frame.cpoint = new CatchPoint();
                    frame.cpoint.kind = GetIntValue(cpointContent, "kind:");
                    frame.cpoint.x = GetIntValue(cpointContent, "x:");
                    frame.cpoint.y = GetIntValue(cpointContent, "y:");
                    frame.cpoint.fronthurtact = GetIntValue(cpointContent, "fronthurtact:");
                    frame.cpoint.backhurtact = GetIntValue(cpointContent, "backhurtact:");
                    frame.cpoint.vaction = GetIntValue(cpointContent, "vaction:");
                    frame.cpoint.throwvz = GetIntValue(cpointContent, "throwvz:");
                    frame.cpoint.hurtable = GetIntValue(cpointContent, "hurtable:");
                    frame.cpoint.throwinjury = GetIntValue(cpointContent, "throwinjury:");
                    frame.cpoint.decrease = GetIntValue(cpointContent, "decrease:");
                }
            }
        }

        /// <summary>
        /// 解析声音
        /// </summary>
        private static void ParseSound(string content, LF2FrameData frame)
        {
            if (content.Contains("sound:"))
            {
                // 提取声音文件路径
                Match soundMatch = Regex.Match(content, @"sound:\s*(\S+)");
                if (soundMatch.Success)
                {
                    frame.sound = soundMatch.Groups[1].Value.Trim();
                }
            }
        }

        /// <summary>
        /// 从字符串中提取整数值
        /// </summary>
        private static int GetIntValue(string content, string key, int defaultValue = 0)
        {
            Match match = Regex.Match(content, key + @"\s*(-?\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        // 解析 "key: [a, b]" 格式的整数数组（对应 FLF catchingact/caughtact）
        private static int[] GetIntArrayValue(string content, string key)
        {
            Match match = Regex.Match(content, Regex.Escape(key) + @"\s*\[\s*(-?\d+)\s*,\s*(-?\d+)\s*\]");
            if (match.Success &&
                int.TryParse(match.Groups[1].Value, out int v0) &&
                int.TryParse(match.Groups[2].Value, out int v1))
            {
                return new int[] { v0, v1 };
            }
            return null;
        }

        /// <summary>
        /// 获取两个标签之间的内容
        /// </summary>
        private static string GetContentBetween(string content, string startTag, string endTag)
        {
            int startIndex = content.IndexOf(startTag);
            if (startIndex == -1) return "";

            startIndex += startTag.Length;
            int endIndex = content.IndexOf(endTag, startIndex);
            if (endIndex == -1) return "";

            return content.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }

    /// <summary>
    /// LF2 DAT文件生成器
    /// </summary>
    public static class LF2DatGenerator
    {
        /// <summary>
        /// 生成DAT文件内容
        /// </summary>
        public static string GenerateDatContent(List<LF2FrameData> frames)
        {
            StringBuilder sb = new StringBuilder();

            foreach (LF2FrameData frame in frames)
            {
                sb.AppendLine($"<frame> {frame.frameId} {frame.frameName}");

                // 基本参数 - 构建参数字符串
                StringBuilder paramLine = new StringBuilder();
                paramLine.Append($"  pic: {frame.pic}  state: {frame.state}  wait: {frame.wait}  next: {frame.next}  dvx: {frame.dvx}  dvy: {frame.dvy}");

                // 只在dvz不为0时输出
                if (frame.dvz != 0)
                {
                    paramLine.Append($"  dvz: {frame.dvz}");
                }

                paramLine.Append($"  centerx: {frame.centerx}  centery: {frame.centery}");

                // 只在mp不为0时输出
                if (frame.mp != 0)
                {
                    paramLine.Append($"  mp: {frame.mp}");
                }

                sb.AppendLine(paramLine.ToString());

                // 按键响应
                sb.AppendLine($"  hit_a: {frame.hit_a}  hit_d: {frame.hit_d}  hit_j: {frame.hit_j}  hit_Fj: {frame.hit_Fj}  hit_Fa: {frame.hit_Fa}  hit_Da: {frame.hit_Da}  hit_Ua: {frame.hit_Ua}  hit_ja: {frame.hit_ja}  hit_Dj: {frame.hit_Dj}  hit_Uj: {frame.hit_Uj}");

                // 武器点
                foreach (var wpoint in frame.wpoints)
                {
                    sb.AppendLine($"  wpoint:");
                    sb.AppendLine($"    kind: {wpoint.kind}  x: {wpoint.x}  y: {wpoint.y}  weaponact: {wpoint.weaponact}  attacking: {wpoint.attacking}  cover: {wpoint.cover}  dvx: {wpoint.dvx}  dvy: {wpoint.dvy}  dvz: {wpoint.dvz}");
                    sb.AppendLine($"  wpoint_end:");
                }

                // 碰撞盒
                foreach (var body in frame.bodies)
                {
                    sb.AppendLine($"  bdy:");
                    sb.AppendLine($"    kind: {body.kind}  x: {body.x}  y: {body.y}  w: {body.w}  h: {body.h}");
                    sb.AppendLine($"  bdy_end:");
                }

                // 交互区域
                foreach (var itr in frame.itrs)
                {
                    sb.AppendLine($"  itr:");
                    sb.AppendLine($"    kind: {itr.kind}  x: {itr.x}  y: {itr.y}  w: {itr.w}  h: {itr.h}  zwidth: {itr.zwidth}  injury: {itr.injury}  fall: {itr.fall}  arest: {itr.arest}  vrest: {itr.vrest}  effect: {itr.effect}");
                    sb.AppendLine($"  itr_end:");
                }

                // 对象点
                if (frame.opoint != null)
                {
                    sb.AppendLine($"  opoint:");
                    sb.AppendLine($"    kind: {frame.opoint.kind}  action: {frame.opoint.action}  id: {frame.opoint.objectId}  x: {frame.opoint.x}  y: {frame.opoint.y}  dvx: {frame.opoint.dvx}  dvy: {frame.opoint.dvy}  oid: {frame.opoint.oid}  facing: {frame.opoint.facing}");
                    sb.AppendLine($"  opoint_end:");
                }

                // 血点
                if (frame.bpoint != null)
                {
                    sb.AppendLine($"  bpoint:");
                    sb.AppendLine($"    x: {frame.bpoint.x}  y: {frame.bpoint.y}");
                    sb.AppendLine($"  bpoint_end:");
                }

                // 抓取点
                if (frame.cpoint != null)
                {
                    sb.AppendLine($"  cpoint:");
                    StringBuilder cpointLine = new StringBuilder();
                    cpointLine.Append($"    kind: {frame.cpoint.kind}  x: {frame.cpoint.x}  y: {frame.cpoint.y}");

                    // 只输出非零值
                    if (frame.cpoint.fronthurtact != 0)
                        cpointLine.Append($"  fronthurtact: {frame.cpoint.fronthurtact}");
                    if (frame.cpoint.backhurtact != 0)
                        cpointLine.Append($"  backhurtact: {frame.cpoint.backhurtact}");
                    if (frame.cpoint.vaction != 0)
                        cpointLine.Append($"  vaction: {frame.cpoint.vaction}");
                    if (frame.cpoint.throwvz != 0)
                        cpointLine.Append($"  throwvz: {frame.cpoint.throwvz}");
                    if (frame.cpoint.hurtable != 0)
                        cpointLine.Append($"  hurtable: {frame.cpoint.hurtable}");
                    if (frame.cpoint.throwinjury != 0)
                        cpointLine.Append($"  throwinjury: {frame.cpoint.throwinjury}");
                    if (frame.cpoint.decrease != 0)
                        cpointLine.Append($"  decrease: {frame.cpoint.decrease}");

                    sb.AppendLine(cpointLine.ToString());
                    sb.AppendLine($"  cpoint_end:");
                }

                // 声音
                if (!string.IsNullOrEmpty(frame.sound))
                {
                    sb.AppendLine($"  sound: {frame.sound}");
                }

                sb.AppendLine("<frame_end>");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}